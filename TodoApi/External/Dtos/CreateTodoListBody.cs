using System.Text.Json.Serialization;

namespace TodoApi.External.Dtos;
public class CreateTodoListBody
{
    [JsonPropertyName("source_id")]
    public string SourceId { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("items")]
    public List<CreateTodoItemBody> Items { get; set; } = new();
}
