using System.Text.Json.Serialization;

namespace TodoApi.External.Dtos;
public class UpdateTodoItemBody
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("completed")]
    public bool? Completed { get; set; }
}
