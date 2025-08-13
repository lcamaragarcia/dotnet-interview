namespace TodoApi.Models;
public class TodoList
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public string? ExternalId { get; set; }
    public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSyncedAt { get; set; } = DateTime.UnixEpoch;
    public bool IsDeleted { get; set; } = false;
    public List<TodoListItem> TodoListItem { get; set; } = new List<TodoListItem>();
}
