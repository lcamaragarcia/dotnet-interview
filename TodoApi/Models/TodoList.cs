namespace TodoApi.Models;
public class TodoList
{
    public long Id { get; set; }
    public required string Name { get; set; }

    public List<TodoListItem> TodoListItem { get; set; } = new List<TodoListItem>();
}
