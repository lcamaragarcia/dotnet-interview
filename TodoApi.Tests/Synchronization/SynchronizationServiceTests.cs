using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApi.External;
using TodoApi.External.Dtos;
using TodoApi.Hubs;
using TodoApi.Models;
using TodoApi.Services;
using TodoApi.Synchronization;
using Xunit;

namespace TodoApi.Tests.Synchronization;

public class SynchronizationServiceTests
{
    private readonly Mock<IExternalTodoApiClient> _externalClientMock;
    private readonly Mock<ILogger<SynchronizationService>> _loggerMock;
    private readonly DbContextOptions<TodoContext> _dbContextOptions;

    public SynchronizationServiceTests()
    {
        _externalClientMock = new Mock<IExternalTodoApiClient>();
        _loggerMock = new Mock<ILogger<SynchronizationService>>();
        _dbContextOptions = new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private TodoContext CreateContext() => new TodoContext(_dbContextOptions);

    [Fact]
    public async Task PullFromExternalAsync_CreatesNewTodoListAndItems_WhenLocalDatabaseIsEmpty()
    {
        // Arrange
        var externalListId = "ext-list-1";
        var externalItemId1 = "ext-item-1";
        var externalItemId2 = "ext-item-2";

        var externalData = new List<TodoListDto>
        {
            new TodoListDto
            {
                Id = externalListId,
                Name = "External List",
                UpdatedAt = DateTime.UtcNow.AddMinutes(-5),
                Items = new List<TodoListItemDto>
                {
                    new TodoListItemDto { Id = externalItemId1, Description = "External Item 1", Completed = false, UpdatedAt = DateTime.UtcNow.AddMinutes(-6) },
                    new TodoListItemDto { Id = externalItemId2, Description = "External Item 2", Completed = true, UpdatedAt = DateTime.UtcNow.AddMinutes(-6) }
                }
            }
        };

        _externalClientMock.Setup(c => c.GetTodoListsAsync()).ReturnsAsync(externalData);

        using (var context = CreateContext())
        {
            var service = new SynchronizationService(_externalClientMock.Object,
                                                    context,
                                                    _loggerMock.Object,
                                                    new TodoListService(context, Mock.Of<ILogger<TodoListService>>()),
                                                    new TodoListItemService(context, Mock.Of<IHubContext<TodoHub>>(), Mock.Of<ILogger<TodoListItemService>>())
);
            // Act
            await service.SyncAsync();

            // Assert
            var localLists = await context.TodoList.Include(l => l.TodoListItem).ToListAsync();
            Assert.Single(localLists);
            var createdList = localLists.First();
            Assert.Equal(externalListId, createdList.ExternalId);
            Assert.Equal("External List", createdList.Name);
            Assert.Equal(2, createdList.TodoListItem.Count);

            var createdItem = createdList.TodoListItem.First(i => i.ExternalId == externalItemId1);
            Assert.Equal("External Item 1", createdItem.Description);
            Assert.False(createdItem.IsComplete);
        }
    }

    [Fact]
    public async Task PushToExternalAsync_CreatesNewTodoList_WhenItHasNoExternalId()
    {
        // Arrange
        var localList = new TodoList { Name = "Local New List", LastModifiedAt = DateTime.UtcNow };
        var externalListId = "ext-new-list-1";

        var createdExternalDto = new TodoListDto { Id = externalListId, Name = "Local New List" };
        _externalClientMock.Setup(c => c.CreateTodoListAsync(It.IsAny<CreateTodoListBody>())).ReturnsAsync(createdExternalDto);
        _externalClientMock.Setup(c => c.GetTodoListsAsync()).ReturnsAsync(new List<TodoListDto>()); // Evitar pull para este test

        using (var context = CreateContext())
        {
            context.TodoList.Add(localList);
            await context.SaveChangesAsync();

            var service = new SynchronizationService(_externalClientMock.Object,
                                                    context,
                                                    _loggerMock.Object,
                                                    new TodoListService(context, Mock.Of<ILogger<TodoListService>>()),
                                                    new TodoListItemService(context, Mock.Of<IHubContext<TodoHub>>(), Mock.Of<ILogger<TodoListItemService>>())
);

            // Act
            await service.SyncAsync();

            // Assert
            var updatedLocalList = await context.TodoList.FirstAsync();
            Assert.NotNull(updatedLocalList.ExternalId);
            Assert.Equal(externalListId, updatedLocalList.ExternalId);
            _externalClientMock.Verify(c => c.CreateTodoListAsync(It.IsAny<CreateTodoListBody>()), Times.Once);
        }
    }
  
    [Fact]
    public async Task PullFromExternalAsync_UpdatesExistingTodoList_WhenExternalIsNewer()
    {
        // Arrange
        var externalListId = "ext-list-1";
        var localName = "Local Old Name";
        var externalName = "External New Name";

        var oldLastSyncedAt = DateTime.UtcNow.AddHours(-2); // Guardamos el timestamp original
        var localList = new TodoList { Name = localName, ExternalId = externalListId, LastSyncedAt = oldLastSyncedAt };

        var externalData = new List<TodoListDto>
    {
        new TodoListDto
        {
            Id = externalListId,
            Name = externalName,
            UpdatedAt = DateTime.UtcNow.AddHours(-1),
            Items = new List<TodoListItemDto>()
        }
    };

        _externalClientMock.Setup(c => c.GetTodoListsAsync()).ReturnsAsync(externalData);

        using (var context = CreateContext())
        {
            context.TodoList.Add(localList);
            await context.SaveChangesAsync();

            var service = new SynchronizationService(_externalClientMock.Object,
                                                    context,
                                                    _loggerMock.Object,
                                                    new TodoListService(context, Mock.Of<ILogger<TodoListService>>()),
                                                    new TodoListItemService(context, Mock.Of<IHubContext<TodoHub>>(), Mock.Of<ILogger<TodoListItemService>>())
);

            // Act
            await service.SyncAsync();

            // Assert
            var updatedLocalList = await context.TodoList.FirstAsync();
            Assert.Equal(externalName, updatedLocalList.Name);
            Assert.True(updatedLocalList.LastSyncedAt > oldLastSyncedAt); // Comparamos con el valor original
        }
    }

    [Fact]
    public async Task PushToExternalAsync_UpdatesExistingTodoList_WhenLocalIsNewer()
    {
        // Arrange
        var externalListId = "ext-list-1";
        var localName = "Local New Name";

        var localList = new TodoList { Name = localName, ExternalId = externalListId, LastModifiedAt = DateTime.UtcNow };

        _externalClientMock.Setup(c => c.GetTodoListsAsync()).ReturnsAsync(new List<TodoListDto>());
        _externalClientMock.Setup(c => c.UpdateTodoListAsync(It.IsAny<string>(), It.IsAny<UpdateTodoListBody>())).ReturnsAsync(new TodoListDto { Id = externalListId, Name = localName });

        using (var context = CreateContext())
        {
            context.TodoList.Add(localList);
            await context.SaveChangesAsync();

            var service = new SynchronizationService(_externalClientMock.Object,
                                                                context,
                                                                _loggerMock.Object,
                                                                new TodoListService(context, Mock.Of<ILogger<TodoListService>>()),
                                                                new TodoListItemService(context, Mock.Of<IHubContext<TodoHub>>(), Mock.Of<ILogger<TodoListItemService>>())
            );
            // Act
            await service.SyncAsync();

            // Assert
            _externalClientMock.Verify(c => c.UpdateTodoListAsync(externalListId, It.Is<UpdateTodoListBody>(b => b.Name == localName)), Times.Once);
        }
    }

    [Fact]
    public async Task PushToExternalAsync_DeletesTodoList_WhenMarkedAsDeleted()
    {
        // Arrange
        var externalListId = "ext-list-1";
        var localList = new TodoList { Name = "List to delete", ExternalId = externalListId, IsDeleted = true, LastModifiedAt = DateTime.UtcNow };

        _externalClientMock.Setup(c => c.GetTodoListsAsync()).ReturnsAsync(new List<TodoListDto>());
        _externalClientMock.Setup(c => c.DeleteTodoListAsync(externalListId)).Returns(Task.CompletedTask);

        using (var context = CreateContext())
        {
            context.TodoList.Add(localList);
            await context.SaveChangesAsync();

            var service = new SynchronizationService(_externalClientMock.Object,
                                                                context,
                                                                _loggerMock.Object,
                                                                new TodoListService(context, Mock.Of<ILogger<TodoListService>>()),
                                                                new TodoListItemService(context, Mock.Of<IHubContext<TodoHub>>(), Mock.Of<ILogger<TodoListItemService>>())
            );

            // Act
            await service.SyncAsync();

            // Assert
            Assert.Empty(await context.TodoList.ToListAsync());
            _externalClientMock.Verify(c => c.DeleteTodoListAsync(externalListId), Times.Once);
        }
    }
}