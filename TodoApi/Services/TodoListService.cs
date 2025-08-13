using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TodoApi.Models;

namespace TodoApi.Services;

public class TodoListService : ITodoListService
{
    private readonly TodoContext _context;
    private readonly ILogger<TodoListService> _logger;

    public TodoListService(TodoContext context, ILogger<TodoListService> logger)
    {
        _context = context;
        _logger = logger;
        _logger.LogInformation("TodoListService iniciado.");
    }
    
    public async Task<IList<TodoList>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Obteniendo todas las listas de tareas.");
            var todoLists = await _context.TodoList.Include(l => l.TodoListItem).ToListAsync();
            _logger.LogInformation("Se encontraron {Count} listas de tareas.", todoLists.Count);
            return todoLists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todas las listas de tareas.");
            throw;
        }
    }
    
    public async Task<TodoList?> GetByIdAsync(long id)
    {
        try
        {
            _logger.LogInformation("Buscando lista de tareas con id {Id}.", id);
            return await _context.TodoList.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar la lista de tareas con id {Id}.", id);
            throw;
        }
    }
    
    public async Task<TodoList> CreateAsync(TodoList list)
    {
        try
        {
            _logger.LogInformation("Creando una nueva lista de tareas con nombre '{Name}'.", list.Name);
            _context.TodoList.Add(list);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Lista de tareas {Id} creada con éxito.", list.Id);
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la lista de tareas con nombre '{Name}'.", list.Name);
            throw;
        }
    }

    public async Task<TodoList> CreateWithItemsAsync(TodoList list)
    {
        _logger.LogInformation("Iniciando la creación de la lista '{Name}' con sus items.", list.Name);
        try
        {
            _context.TodoList.Add(list);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Lista '{Name}' creada exitosamente con ID: {Id}", list.Name, list.Id);
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la lista '{Name}'.", list.Name);
            throw; 
        }
    }

    public async Task<TodoList?> UpdateAsync(TodoList list)
    {
        try
        {
            _logger.LogInformation("Actualizando lista de tareas con id {Id}.", list.Id);
            _context.Entry(list).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Lista de tareas {Id} actualizada con éxito.", list.Id);
            return list;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (!await _context.TodoList.AnyAsync(e => e.Id == list.Id))
            {
                _logger.LogWarning("Intento de actualización fallido: No se encontró lista de tareas con id {Id}.", list.Id);
                return null;
            }
            _logger.LogError(ex, "Error de concurrencia al actualizar la lista de tareas con id {Id}.", list.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar la lista de tareas con id {Id}.", list.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(long id)
    {
        try
        {
            _logger.LogInformation("Intentando eliminar lista de tareas con id {Id}.", id);
            var list = await _context.TodoList.FindAsync(id);
            if (list == null)
            {
                _logger.LogWarning("Intento de eliminación fallido: No se encontró lista con id {Id}.", id);
                return false;
            }
            
            _context.TodoList.Remove(list);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Lista de tareas {Id} eliminada con éxito.", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al intentar eliminar la lista de tareas con id {Id}.", id);
            throw;
        }
    }
}