using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TodoApi.Dtos;
using TodoApi.Hubs;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers;

[Route("api/todolistitems")]
[ApiController]
public class TodoListItemsController : ControllerBase
{
    private readonly ITodoListItemService _todoListItemService;
    private readonly IMapper _mapper;
    
    public TodoListItemsController(ITodoListItemService todoListItemService,
                                    IMapper mapper)
    {
        _todoListItemService = todoListItemService;
        _mapper = mapper;
    }

    [HttpGet("/api/todolistitems/{todoListId}/items")]
    public async Task<ActionResult<IList<TodoListItem>>> GetTodoListItems(long todoListId)
    {
        try
        {
            var todoItems = await _todoListItemService.GetByTodoListIdAsync(todoListId);
            return Ok(todoItems);
        }
        catch (Exception)
        {
            return StatusCode(500, "Ocurrió un error inesperado al procesar la solicitud.");
        }
        
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TodoListItem>> GetTodoListItem(long id)
    {
        try
        {
            var todoListItem = await _todoListItemService.GetByIdAsync(id);

            if (todoListItem == null)
            {
                return NotFound();
            }

            return Ok(todoListItem);
        }
        catch (Exception)
        {
            return StatusCode(500, "Ocurrió un error inesperado al procesar la solicitud.");
        }
        
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> PutTodoListItem(long id, UpdateTodoListItem payload)
    {
        try
        {
            if (id != payload.Id)
            {
                return BadRequest();
            }

            var todoListItem = _mapper.Map<TodoListItem>(payload);
            var updatedItem = await _todoListItemService.UpdateAsync(todoListItem);

            if (updatedItem == null)
            {
                return NotFound();
            }

            return Ok(updatedItem);
        }
        catch (Exception)
        {
            return StatusCode(500, "Ocurrió un error inesperado al procesar la solicitud.");
        }
    }

    // POST: api/todolistitems
    // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<TodoListItem>> PostTodoListItem(CreateTodoListItem payload)
    {
        try
        {
            var todoListItem = _mapper.Map<TodoListItem>(payload);
            var createdItem = await _todoListItemService.CreateAsync(todoListItem);

            return CreatedAtAction("GetTodoListItem", new { id = createdItem.Id }, createdItem);
        }
        catch (Exception)
        {
            return StatusCode(500, "Ocurrió un error inesperado al procesar la solicitud.");
        }
    }

    // DELETE: api/todolistitem/5
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTodoListItem(long id)
    {
        try
        {
            var deleted = await _todoListItemService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500, "Ocurrió un error inesperado al procesar la solicitud.");
        }
        
    }

    [HttpPost("/api/todolistitems/{todoListId}/complete-all")]
    public async Task<ActionResult> CompleteAllTodoItems(long todoListId)
    {
        try
        {
            await _todoListItemService.CompleteAllItemsAsync(todoListId);

            return Ok(new { message = $"Proceso para completar items de la lista con id {todoListId} iniciado." });
        }
        catch (Exception)
        {
            return StatusCode(500, "Ocurrió un error inesperado al procesar la solicitud.");                
        }
    }

}
