using System.Net.Http.Json;
using TodoApi.External.Dtos;

namespace TodoApi.External;

public class ExternalTodoApiClient : IExternalTodoApiClient
{
    private readonly HttpClient _httpClient;

    public ExternalTodoApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<TodoListDto>> GetTodoListsAsync()
    {
        var response = await _httpClient.GetAsync("/todolists");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<TodoListDto>>())!;
    }

    public async Task<TodoListDto> CreateTodoListAsync(CreateTodoListBody body)
    {
        var response = await _httpClient.PostAsJsonAsync("/todolists", body);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TodoListDto>())!;
    }

    public async Task<TodoListDto> UpdateTodoListAsync(string id, UpdateTodoListBody body)
    {
        var response = await _httpClient.PatchAsJsonAsync($"/todolists/{id}", body);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TodoListDto>())!;
    }

    public async Task DeleteTodoListAsync(string id)
    {
        var response = await _httpClient.DeleteAsync($"/todolists/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<TodoListItemDto> CreateTodoItemAsync(string listId, CreateTodoItemBody body)
    {
        var response = await _httpClient.PostAsJsonAsync($"/todolists/{listId}/todoitems", body);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TodoListItemDto>())!;
    }

    public async Task<TodoListItemDto> UpdateTodoItemAsync(string listId, string itemId, UpdateTodoItemBody body)
    {
        var response = await _httpClient.PatchAsJsonAsync($"/todolists/{listId}/todoitems/{itemId}", body);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TodoListItemDto>())!;
    }

    public async Task DeleteTodoItemAsync(string listId, string itemId)
    {
        var response = await _httpClient.DeleteAsync($"/todolists/{listId}/todoitems/{itemId}");
        response.EnsureSuccessStatusCode();
    }
}