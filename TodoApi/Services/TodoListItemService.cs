using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Drawing.Text;
using TodoApi.Hubs;
using TodoApi.Models;

namespace TodoApi.Services;

public class TodoListItemService : ITodoListItemService
{
    private readonly TodoContext _context;
    private readonly IHubContext<TodoHub> _hubContext;
    private readonly ILogger<TodoListItemService> _logger;

    public TodoListItemService(TodoContext context,
                                IHubContext<TodoHub> hubContext,
                                ILogger<TodoListItemService> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _logger=logger;
        _logger.LogInformation("TodoListItemService iniciado.");
    }
    public async Task<TodoListItem> CreateAsync(TodoListItem todoListItem)
    {
        try
        {
            _logger.LogInformation("Creando un nuevo ítem en la lista {TodoListId} con nombre '{Name}'", todoListItem.TodoListId, todoListItem.Description);
            _context.TodoListItem.Add(todoListItem);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Ítem {Id} creado con éxito.", todoListItem.Id);
            return todoListItem;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear un ítem.");
            throw;
        }            
    }   

    public async Task<bool> DeleteAsync(long id)
    {
        try
        {
            var todoItem = await _context.TodoListItem.FindAsync(id);
            if (todoItem == null)
            {
                _logger.LogWarning("Intento de eliminar ítem fallido: No se encontró ítem con id {Id}.", id);
                return false;
            }

            _context.TodoListItem.Remove(todoItem);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Ítem {Id} eliminado con éxito.", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al intentar eliminar el ítem con id {Id}.", id);
            throw;
        }
       
    }

    public async Task<TodoListItem?> GetByIdAsync(long id)
    {            
        try
        {
            return await _context.TodoListItem.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar el ítem con id {Id}.", id);
            throw;
        }
    }   

    public async Task<IList<TodoListItem>> GetByTodoListIdAsync(long todoListId)
    {            
        try
        {
            return await _context.TodoListItem.Where(w => w.TodoListId == todoListId).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar ítems de la lista con id {Id}.", todoListId);
            throw;
        }
    }

    public async Task<TodoListItem?> UpdateAsync(TodoListItem todoListItem)
    {
        try
        {
            var itemToUpdate = await _context.TodoListItem.FindAsync(todoListItem.Id);
            if (itemToUpdate == null)
            {
                return null;
            }

            itemToUpdate.Description = todoListItem.Description;
            itemToUpdate.IsComplete = todoListItem.IsComplete;

            await _context.SaveChangesAsync();

            return itemToUpdate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el ítem con id {Id}.", todoListItem.Id);
            throw;
        }            
    }

    public async Task<int> CompleteAllItemsAsync(long todoListId)
    {
        try
        {
            _logger.LogInformation("Iniciando proceso para completar ítems de la lista con id {id}.", todoListId);

            var incompleteItems = await _context.TodoListItem
            .Where(i => i.TodoListId == todoListId && !i.IsComplete)
            .ToListAsync();

            var totalItems = incompleteItems.Count;

            if (totalItems == 0)
            {
                _logger.LogWarning("No se encontraron ítems incompletos para la lista con id {id}.", todoListId);
            }
            else
            {
                _logger.LogInformation("{total} de items no completos encontrados para la lista con id {id}.", totalItems, todoListId);
            }

            await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Server", $"Iniciando actualización de {totalItems} ítems sin completar...");

            int itemsCompleted = 0;
            foreach (var item in incompleteItems)
            {
                await Task.Delay(1000);

                item.IsComplete = true;
                _context.TodoListItem.Update(item);
                await _context.SaveChangesAsync();

                itemsCompleted++;

                var progressPercentage = (int)((double)itemsCompleted / totalItems * 100);
                await _hubContext.Clients.All.SendAsync(
                    "ReceiveMessage",
                    "Server",
                    $"Actualizando... {itemsCompleted} de {totalItems} ítems completados. ({progressPercentage}%)"
                );
            }

            await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Server", "Proceso de actualización finalizado.");

            _logger.LogInformation("Proceso de completar ítems para la lista con id {id} finalizado.", todoListId);

            return itemsCompleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar ítems de la lista con id {Id} como completos.", todoListId);
            throw;
        }
        
    }
   
}
