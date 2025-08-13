using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TodoApi.Controllers;
using TodoApi.Dtos;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Tests;

#nullable disable
public class TodoListsControllerTests
{
    // Mocks para los servicios y AutoMapper
    private readonly Mock<ITodoListService> _listServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly TodoListsController _controller;

    public TodoListsControllerTests()
    {
        _listServiceMock = new Mock<ITodoListService>();
        _mapperMock = new Mock<IMapper>();
         _controller = new TodoListsController(_listServiceMock.Object, _mapperMock.Object);
    }

    
    [Fact]
    public async Task GetTodoLists_WhenCalled_ReturnsOkResultWithLists()
    {
        // Arrange
        var todoLists = new List<TodoList>
        {
            new TodoList { Id = 1, Name = "Task 1" },
            new TodoList { Id = 2, Name = "Task 2" }
        };
       
        _listServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(todoLists);

        // Act
        var result = await _controller.GetTodoLists();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedLists = Assert.IsType<List<TodoList>>(okResult.Value);
        Assert.Equal(2, returnedLists.Count);
    }

    [Fact]
    public async Task GetTodoLists_WhenServiceThrowsException_ReturnsStatusCode500()
    {
        // Arrange
        _listServiceMock.Setup(s => s.GetAllAsync()).ThrowsAsync(new InvalidOperationException("DB is down"));

        // Act
        var result = await _controller.GetTodoLists();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GetTodoList_WhenListExists_ReturnsOkResultWithList()
    {
        // Arrange
        var todoList = new TodoList { Id = 1, Name = "Task 1" };
        _listServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(todoList);

        // Act
        var result = await _controller.GetTodoList(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedList = Assert.IsType<TodoList>(okResult.Value);
        Assert.Equal(1, returnedList.Id);
    }

    [Fact]
    public async Task GetTodoList_WhenListDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _listServiceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((TodoList)null);

        // Act
        var result = await _controller.GetTodoList(999);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetTodoList_WhenServiceThrowsException_ReturnsStatusCode500()
    {
        // Arrange
        _listServiceMock.Setup(s => s.GetByIdAsync(1)).ThrowsAsync(new InvalidOperationException("DB is down"));

        // Act
        var result = await _controller.GetTodoList(1);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

 [Fact]
    public async Task PutTodoList_WhenListDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var payload = new Dtos.UpdateTodoList { Name = "Task 3" };
        _listServiceMock.Setup(s => s.GetByIdAsync(3)).ReturnsAsync((TodoList)null);

        // Act
        var result = await _controller.PutTodoList(3, payload);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task PutTodoList_WhenUpdateIsSuccessful_ReturnsOkResult()
    {
        // Arrange
        var payload = new UpdateTodoList { Name = "Changed Task 2" };
        var existingList = new TodoList { Id = 2, Name = "Task 2" };
        var updatedList = new TodoList { Id = 2, Name = "Changed Task 2" };

        _listServiceMock.Setup(s => s.GetByIdAsync(2)).ReturnsAsync(existingList);
        _mapperMock.Setup(m => m.Map(payload, existingList)); // Simular el mapeo
        _listServiceMock.Setup(s => s.UpdateAsync(existingList)).ReturnsAsync(updatedList);

        // Act
        var result = await _controller.PutTodoList(2, payload);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedList = Assert.IsType<TodoList>(okResult.Value);
        Assert.Equal("Changed Task 2", returnedList.Name);
    }

    [Fact]
    public async Task PutTodoList_WhenServiceThrowsException_ReturnsStatusCode500()
    {
        // Arrange
        var payload = new UpdateTodoList { Name = "Changed Task 2" };
        var existingList = new TodoList { Id = 2, Name = "Task 2" };

        _listServiceMock.Setup(s => s.GetByIdAsync(2)).ReturnsAsync(existingList);
        _mapperMock.Setup(m => m.Map(payload, existingList));
        _listServiceMock.Setup(s => s.UpdateAsync(It.IsAny<TodoList>())).ThrowsAsync(new InvalidOperationException("DB is down"));

        // Act
        var result = await _controller.PutTodoList(2, payload);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

   [Fact]
    public async Task PostTodoList_WhenCalled_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateTodoList { Name = "Task 3" };
        var createdList = new TodoList { Id = 3, Name = "Task 3" };

        _mapperMock.Setup(m => m.Map<TodoList>(createDto)).Returns(createdList);
        _listServiceMock.Setup(s => s.CreateAsync(createdList)).ReturnsAsync(createdList);

        // Act
        var result = await _controller.PostTodoList(createDto);

        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedList = Assert.IsType<TodoList>(createdAtResult.Value);
        Assert.Equal(3, returnedList.Id);
    }

    [Fact]
    public async Task PostTodoList_WhenServiceThrowsException_ReturnsStatusCode500()
    {
        // Arrange
        var createDto = new CreateTodoList { Name = "Task 3" };
        _mapperMock.Setup(m => m.Map<TodoList>(createDto)).Returns(new TodoList { Name = "Task 3" });
        _listServiceMock.Setup(s => s.CreateAsync(It.IsAny<TodoList>())).ThrowsAsync(new InvalidOperationException("DB is down"));

        // Act
        var result = await _controller.PostTodoList(createDto);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task DeleteTodoList_WhenListExists_ReturnsNoContent()
    {
        // Arrange
        _listServiceMock.Setup(s => s.DeleteAsync(2)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteTodoList(2);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteTodoList_WhenListDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _listServiceMock.Setup(s => s.DeleteAsync(999)).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteTodoList(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteTodoList_WhenServiceThrowsException_ReturnsStatusCode500()
    {
        // Arrange
        _listServiceMock.Setup(s => s.DeleteAsync(2)).ThrowsAsync(new InvalidOperationException("DB is down"));

        // Act
        var result = await _controller.DeleteTodoList(2);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }
}