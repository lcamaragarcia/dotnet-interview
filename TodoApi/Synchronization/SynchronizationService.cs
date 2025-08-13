using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using System.Collections.Generic;
using TodoApi.External;
using TodoApi.External.Dtos;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Synchronization;

public class SynchronizationService : ISynchronizationService
{
    private readonly IExternalTodoApiClient _externalClient;
    private readonly TodoContext _context;
    private readonly ILogger<SynchronizationService> _logger;
    private readonly ITodoListService _todoListService;
    private readonly ITodoListItemService _todoListItemService;

    // Asumimos un policy de resiliencia configurado
    private readonly IAsyncPolicy _resiliencePolicy;

    public SynchronizationService(IExternalTodoApiClient externalClient,
                                    TodoContext context,
                                    ILogger<SynchronizationService> logger,
                                    ITodoListService todoListService,
                                    ITodoListItemService todoListItemService)
    {
        _externalClient = externalClient;
        _context = context;
        _logger = logger;
        _todoListService = todoListService;
        _todoListItemService = todoListItemService;
        _resiliencePolicy = Policy.Handle<HttpRequestException>().WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(Math.Pow(2, i)));
    }

    public async Task SyncAsync()
    {
        _logger.LogInformation("Iniciando sincronización bidireccional.");
        var now = DateTime.UtcNow;

        // Datos de la API externa
        _logger.LogInformation("Obteniendo listas de tareas de la API externa.");
        var externalLists = await _resiliencePolicy.ExecuteAsync(() => _externalClient.GetTodoListsAsync());
        _logger.LogInformation("Obtenidas {Count} listas de la API externa.", externalLists.Count);
        var externalListsDict = externalLists.ToDictionary(l => l.Id!);

        // Datos de la base de datos local
        _logger.LogInformation("Obteniendo listas de tareas de la base de datos local.");
        var localLists = await _todoListService.GetAllAsync();
        _logger.LogInformation("Obtenidas {Count} listas de la base de datos local.", localLists.Count);
        var localListsDictByExternalId = localLists.Where(l => l.ExternalId != null).ToDictionary(l => l.ExternalId!);

        // Sincronización: Pull (API externa -> Base de datos local)
        _logger.LogInformation("Iniciando fase de 'Pull' (API externa -> Base de datos local).");
        foreach (var extList in externalLists)
        {
            if (localListsDictByExternalId.TryGetValue(extList.Id!, out var localList))
            {
                // La lista ya existe localmente, revisar si necesita actualización
                if (extList.UpdatedAt > localList.LastSyncedAt)
                {
                    if (localList.Name != extList.Name && extList.Name != null)
                    {
                        localList.Name = extList.Name;
                        localList.LastModifiedAt = now;
                    }
                    localList.LastSyncedAt = now;
                    _logger.LogInformation("Actualizada lista local '{Name}' desde externa.", extList.Name);
                }
            }
            else
            {
                // La lista no existe localmente, crear una nueva
                var newList = new TodoList
                {
                    Name = extList.Name!,
                    ExternalId = extList.Id,
                    LastSyncedAt = now,
                    LastModifiedAt = now,
                    TodoListItem = extList.Items.Select(i => new TodoListItem
                    {
                        Description = i.Description!,
                        IsComplete = i.Completed,
                        ExternalId = i.Id,
                        LastSyncedAt = now,
                        LastModifiedAt = now,
                    }).ToList()
                };
                await _todoListService.CreateWithItemsAsync(newList);
                _logger.LogInformation("Creada nueva lista local '{Name}' desde externa.", extList.Name);
            }
        }

        // Sincronización: Push (Base de datos local -> API externa)
        _logger.LogInformation("Iniciando fase de 'Push' (Base de datos local -> API externa).");
        foreach (var localList in localLists)
        {
            if (localList.IsDeleted)
            {
                if (localList.ExternalId != null)
                {
                    _logger.LogInformation("Eliminando lista '{Name}' de la API externa.", localList.Name);
                    await _resiliencePolicy.ExecuteAsync(() => _externalClient.DeleteTodoListAsync(localList.ExternalId));
                }
                else
                {
                    _logger.LogInformation("Eliminando lista '{Name}' (sin ID externo) localmente.", localList.Name);
                }
                   
                _context.TodoList.Remove(localList);
            }
            else if (localList.ExternalId == null)
            {
                _logger.LogInformation("Creando lista '{Name}' en la API externa.", localList.Name);
                var createdExtList = await _resiliencePolicy.ExecuteAsync(() =>
                    _externalClient.CreateTodoListAsync(new CreateTodoListBody
                    {
                        Name = localList.Name,
                        SourceId = localList.Id.ToString(),
                        Items = localList.TodoListItem.Select(i => new CreateTodoItemBody { Description = i.Description, Completed = i.IsComplete, SourceId = i.Id.ToString() }).ToList()
                    }));

                localList.ExternalId = createdExtList.Id;
                localList.LastSyncedAt = now;
                _logger.LogInformation("Creada lista '{Name}' en externa con ID: {ExternalId}", localList.Name, localList.ExternalId);
            }
            else if (localList.LastModifiedAt > localList.LastSyncedAt)
            {
                _logger.LogInformation("Actualizando lista '{Name}' en la API externa.", localList.Name);
                await _resiliencePolicy.ExecuteAsync(() => _externalClient.UpdateTodoListAsync(localList.ExternalId, new UpdateTodoListBody { Name = localList.Name }));
                localList.LastSyncedAt = now;
                _logger.LogInformation("Actualizada lista '{Name}' en externa.", localList.Name);
            }
        }

        // Guardar todos los cambios en la base de datos local
        _logger.LogInformation("Guardando cambios en la base de datos local.");
        await _context.SaveChangesAsync();
        _logger.LogInformation("Sincronización completada con éxito.");
    }
}