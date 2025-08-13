using TodoApi.Models;

namespace TodoApi.Services
{
    public interface ITodoListService
    {
        Task<IList<TodoList>> GetAllAsync();
        Task<TodoList?> GetByIdAsync(long id);
        Task<TodoList> CreateAsync(TodoList list);
        Task<TodoList> CreateWithItemsAsync(TodoList list);
        Task<TodoList?> UpdateAsync(TodoList list);
        Task<bool> DeleteAsync(long id);
    }
}
