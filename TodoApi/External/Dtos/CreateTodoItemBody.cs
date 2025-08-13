using System.Text.Json.Serialization;

namespace TodoApi.External.Dtos;
public class CreateTodoItemBody
{
    [JsonPropertyName("source_id")]
    public string SourceId { get; set; } = null!;

    [JsonPropertyName("description")]
    public string Description { get; set; } = null!;

    [JsonPropertyName("completed")]
    public bool Completed { get; set; }
}
