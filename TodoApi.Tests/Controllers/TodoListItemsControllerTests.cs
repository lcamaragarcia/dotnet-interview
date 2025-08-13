
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TodoApi.Controllers;
using TodoApi.Dtos;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Tests.Controllers;

    public class TodoListItemsControllerTests
    {
        private readonly Mock<ITodoListItemService> _serviceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly TodoListItemsController _controller;

        public TodoListItemsControllerTests()
        {
            _serviceMock = new Mock<ITodoListItemService>();
            _mapperMock = new Mock<IMapper>();
            _controller = new TodoListItemsController(_serviceMock.Object, _mapperMock.Object);
        }

       

        // --- GET METHODS ---
        // Tests para los métodos Get (ya discutidos, se incluyen para completar la clase)

        [Fact]
        public async Task GetTodoListItem_WhenItemExists_ReturnsOkResultWithItem()
        {
            // Arrange
            var todoListItem = new TodoListItem { Id = 1, Name = "Test Item", IsComplete = false, TodoListId = 1 };
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(todoListItem);

            // Act
            var result = await _controller.GetTodoListItem(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedItem = Assert.IsType<TodoListItem>(okResult.Value);
            Assert.Equal(todoListItem.Id, returnedItem.Id);
        }

        [Fact]
        public async Task GetTodoListItem_WhenItemDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((TodoListItem?)null);

            // Act
            var result = await _controller.GetTodoListItem(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetTodoListItem_WhenServiceThrowsException_ReturnsStatusCode500()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ThrowsAsync(new InvalidOperationException("DB is down"));

            // Act
            var result = await _controller.GetTodoListItem(1);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        // --- PUT METHOD ---
        // Nuevos tests para el método Put

        [Fact]
        public async Task PutTodoListItem_WhenIdsDoNotMatch_ReturnsBadRequest()
        {
            // Arrange
            var payload = new UpdateTodoListItem { Id = 1, Name = "Updated Item" };

            // Act
            var result = await _controller.PutTodoListItem(999, payload);

            // Assert
            Assert.IsType<BadRequestResult>(result);
            _serviceMock.Verify(s => s.UpdateAsync(It.IsAny<TodoListItem>()), Times.Never);
        }

        [Fact]
        public async Task PutTodoListItem_WhenItemDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var payload = new UpdateTodoListItem { Id = 1, Name = "Updated Item" };
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<TodoListItem>())).ReturnsAsync((TodoListItem?)null);

            // Act
            var result = await _controller.PutTodoListItem(1, payload);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PutTodoListItem_WhenUpdateIsSuccessful_ReturnsOkResult()
        {
            // Arrange
            var payload = new UpdateTodoListItem { Id = 1, Name = "Updated Item", IsComplete = true };
            var updatedItem = new TodoListItem { Id = 1, Name = "Updated Item", TodoListId = 1, IsComplete = true };
            _mapperMock.Setup(m => m.Map<TodoListItem>(payload)).Returns(updatedItem);
            _serviceMock.Setup(s => s.UpdateAsync(updatedItem)).ReturnsAsync(updatedItem);

            // Act
            var result = await _controller.PutTodoListItem(1, payload);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedItem = Assert.IsType<TodoListItem>(okResult.Value);
            Assert.Equal(updatedItem.Name, returnedItem.Name);
        }

        [Fact]
        public async Task PutTodoListItem_WhenServiceThrowsException_ReturnsStatusCode500()
        {
            // Arrange
            var payload = new UpdateTodoListItem { Id = 1, Name = "Updated Item" };
            _mapperMock.Setup(m => m.Map<TodoListItem>(payload)).Returns(new TodoListItem { Id = 1, Name = "Test Item" });
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<TodoListItem>())).ThrowsAsync(new InvalidOperationException("DB is down"));

            // Act
            var result = await _controller.PutTodoListItem(1, payload);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        // --- POST METHOD ---
        // Tests para el método Post

        [Fact]
        public async Task PostTodoListItem_WhenCalled_ReturnsCreatedAtAction()
        {
            // Arrange
            var createDto = new CreateTodoListItem { Name = "New Item", TodoListId = 1 };
            var todoListItem = new TodoListItem { Id = 3, Name = "New Item", TodoListId = 1 };
            _mapperMock.Setup(m => m.Map<TodoListItem>(createDto)).Returns(todoListItem);
            _serviceMock.Setup(s => s.CreateAsync(todoListItem)).ReturnsAsync(todoListItem);

            // Act
            var result = await _controller.PostTodoListItem(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedItem = Assert.IsType<TodoListItem>(createdResult.Value);
            Assert.Equal(todoListItem.Id, returnedItem.Id);
            _serviceMock.Verify(s => s.CreateAsync(todoListItem), Times.Once);
        }

        [Fact]
        public async Task PostTodoListItem_WhenServiceThrowsException_ReturnsStatusCode500()
        {
            // Arrange
            var createDto = new CreateTodoListItem { Name = "New Item" };
            _mapperMock.Setup(m => m.Map<TodoListItem>(createDto)).Returns(new TodoListItem { Name = "New Item" });
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<TodoListItem>())).ThrowsAsync(new InvalidOperationException("DB is down"));

            // Act
            var result = await _controller.PostTodoListItem(createDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        // --- DELETE METHOD ---
        // Nuevos tests para el método Delete

        [Fact]
        public async Task DeleteTodoListItem_WhenItemExists_ReturnsNoContent()
        {
            // Arrange
            _serviceMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteTodoListItem(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteTodoListItem_WhenItemDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            _serviceMock.Setup(s => s.DeleteAsync(999)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteTodoListItem(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteTodoListItem_WhenServiceThrowsException_ReturnsStatusCode500()
        {
            // Arrange
            _serviceMock.Setup(s => s.DeleteAsync(1)).ThrowsAsync(new InvalidOperationException("DB is down"));

            // Act
            var result = await _controller.DeleteTodoListItem(1);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        // --- COMPLETE ALL METHOD ---
        // Nuevos tests para el método CompleteAll

        [Fact]
        public async Task CompleteAllTodoItems_WhenCalled_ReturnsOk()
        {
            // Arrange
            _serviceMock.Setup(s => s.CompleteAllItemsAsync(1)).ReturnsAsync(2);

            // Act
            var result = await _controller.CompleteAllTodoItems(1);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            _serviceMock.Verify(s => s.CompleteAllItemsAsync(1), Times.Once);
        }

        [Fact]
        public async Task CompleteAllTodoItems_WhenNoItemsToComplete_ReturnsOk()
        {
            // Arrange
            _serviceMock.Setup(s => s.CompleteAllItemsAsync(1)).ReturnsAsync(0);

            // Act
            var result = await _controller.CompleteAllTodoItems(1);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task CompleteAllTodoItems_WhenServiceThrowsException_ReturnsStatusCode500()
        {
            // Arrange
            _serviceMock.Setup(s => s.CompleteAllItemsAsync(1)).ThrowsAsync(new InvalidOperationException("DB is down"));

            // Act
            var result = await _controller.CompleteAllTodoItems(1);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }
