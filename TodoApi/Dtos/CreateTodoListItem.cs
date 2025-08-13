namespace TodoApi.Dtos;

public class CreateTodoListItem
{
    public required string Description { get; set; }
    public long TodoListId { get; set; }
    public string? SourceId { get; set; }
}
