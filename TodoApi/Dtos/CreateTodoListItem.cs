namespace TodoApi.Dtos;

public class CreateTodoListItem
{
    public required string Name { get; set; }
    public long TodoListId { get; set; }
}
