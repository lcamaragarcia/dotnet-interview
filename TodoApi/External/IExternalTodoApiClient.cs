using TodoApi.External.Dtos;

namespace TodoApi.External;

public interface IExternalTodoApiClient
{
    Task<List<TodoListDto>> GetTodoListsAsync();
     Task<TodoListDto> CreateTodoListAsync(CreateTodoListBody todoList);
    Task<TodoListDto> UpdateTodoListAsync(string id, UpdateTodoListBody todoList);
    Task DeleteTodoListAsync(string id);
    Task<TodoListItemDto> CreateTodoItemAsync(string listId, CreateTodoItemBody item);
    Task<TodoListItemDto> UpdateTodoItemAsync(string listId, string itemId, UpdateTodoItemBody item);
    Task DeleteTodoItemAsync(string listId, string itemId);
}
