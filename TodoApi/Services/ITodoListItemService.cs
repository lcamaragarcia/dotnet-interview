using TodoApi.Models;

namespace TodoApi.Services;

public interface ITodoListItemService
{
    Task<int> CompleteAllItemsAsync(long todoListId);
    Task<TodoListItem> CreateAsync(TodoListItem item);
    Task<bool> DeleteAsync(long id);
    Task<IList<TodoListItem>> GetByTodoListIdAsync(long todoListId);
    Task<TodoListItem?> GetByIdAsync(long id);    
    Task<TodoListItem?> UpdateAsync(TodoListItem todoItem);    
}
