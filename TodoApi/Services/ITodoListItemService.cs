using TodoApi.Models;

namespace TodoApi.Services;

public interface ITodoListItemService
{
    Task<IList<TodoListItem>> GetByTodoListIdAsync(long todoListId);
    Task<TodoListItem?> GetByIdAsync(long id);
    Task<TodoListItem> CreateAsync(TodoListItem item);
    Task<TodoListItem?> UpdateAsync(TodoListItem todoItem);
    Task<bool> DeleteAsync(long id);
    Task<int> CompleteAllItemsAsync(long todoListId);
   
}
