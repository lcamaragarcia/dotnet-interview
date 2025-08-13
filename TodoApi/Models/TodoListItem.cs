namespace TodoApi.Models;

public class TodoListItem
{        
    public long Id { get; set; }
    public required string Name { get; set; }

    // Clave foránea para la relación con TodoList
    public long TodoListId { get; set; }
    public bool IsComplete { get; set; }

    // Propiedad de navegación para la relación (opcional pero recomendada)
    public TodoList TodoList { get; set; } = null!;    
}
