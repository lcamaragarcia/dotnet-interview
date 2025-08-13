using TodoApi.Dtos;
using TodoApi.Models;
using AutoMapper;

namespace TodoApi.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Mapeo para TodoList
        CreateMap<CreateTodoList, TodoList>();
        CreateMap<UpdateTodoList, TodoList>();

        // Mapeo para TodoListItem
        CreateMap<CreateTodoListItem, TodoListItem>();
        CreateMap<UpdateTodoListItem, TodoListItem>();
    }
}
