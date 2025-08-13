namespace TodoApi.Models;

public class TodoListItem
{        
    public long Id { get; set; }
    public required string Description { get; set; }

    // Clave foránea para la relación con TodoList
    public long TodoListId { get; set; }
    public bool IsComplete { get; set; }

    // Propiedad de navegación para la relación (opcional pero recomendada)
    public TodoList TodoList { get; set; } = null!;

    public string? ExternalId { get; set; }
    public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSyncedAt { get; set; } = DateTime.UnixEpoch;
    public bool IsDeleted { get; set; } = false;
}
