using System.Text.Json.Serialization;

namespace TodoApi.External.Dtos;

public class UpdateTodoListBody
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
