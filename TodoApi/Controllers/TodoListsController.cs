using Microsoft.AspNetCore.Mvc;
using TodoApi.Dtos;
using TodoApi.Models;
using TodoApi.Services;
using AutoMapper;

namespace TodoApi.Controllers
{
    [Route("api/todolists")]
    [ApiController]
    public class TodoListsController : ControllerBase
    {
        private readonly ITodoListService _listService;
        private readonly IMapper _mapper;

        public TodoListsController(ITodoListService listService, IMapper mapper)
        {
            _listService = listService;
            _mapper = mapper;
        }

        // GET: api/todolists
        [HttpGet]
        public async Task<ActionResult<IList<TodoList>>> GetTodoLists()
        {
            try
            {
                var todoLists = await _listService.GetAllAsync();
                return Ok(todoLists);
            }
            catch (Exception)
            {
                return StatusCode(500, "Ocurrió un error inesperado al obtener las listas de tareas.");
            }
        }

        // GET: api/todolists/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoList>> GetTodoList(long id)
        {
            try
            {
                var todoList = await _listService.GetByIdAsync(id);

                if (todoList == null)
                {
                    return NotFound();
                }

                return Ok(todoList);
            }
            catch (Exception)
            {
                return StatusCode(500, "Ocurrió un error inesperado al obtener la lista de tareas.");
            }
        }

        // PUT: api/todolists/5
        [HttpPut("{id}")]
        public async Task<ActionResult> PutTodoList(long id, UpdateTodoList payload)
        {
            try
            {
                var todoList = await _listService.GetByIdAsync(id);
                if (todoList == null)
                {
                    return NotFound();
                }

                _mapper.Map(payload, todoList);
                todoList.LastModifiedAt = DateTime.UtcNow;

                var updatedList = await _listService.UpdateAsync(todoList);

                if (updatedList == null)
                {
                    return NotFound();
                }

                return Ok(updatedList);
            }
            catch (Exception)
            {
                return StatusCode(500, "Ocurrió un error inesperado al actualizar la lista de tareas.");
            }
        }

        // POST: api/todolists
        [HttpPost]
        public async Task<ActionResult<TodoList>> PostTodoList(CreateTodoList payload)
        {
            try
            {
                var todoList = _mapper.Map<TodoList>(payload);
                todoList.LastModifiedAt = DateTime.UtcNow;

                var createdList = await _listService.CreateAsync(todoList);

                return CreatedAtAction("GetTodoList", new { id = createdList.Id }, createdList);
            }
            catch (Exception)
            {
                return StatusCode(500, "Ocurrió un error inesperado al crear la lista de tareas.");
            }
        }

        // DELETE: api/todolists/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTodoList(long id)
        {
            try
            {
                var deleted = await _listService.DeleteAsync(id);

                if (!deleted)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "Ocurrió un error inesperado al eliminar la lista de tareas.");
            }
        }
    }
}