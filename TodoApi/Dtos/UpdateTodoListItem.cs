namespace TodoApi.Dtos;

public class UpdateTodoListItem
{
    public long Id { get; set; } 
    public required string Description { get; set; }
    public bool IsComplete { get; set; }    
}
