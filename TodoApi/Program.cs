using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Hubs;
using TodoApi.Services;

var builder = WebApplication.CreateBuilder(args);
builder
    .Services.AddDbContext<TodoContext>(opt =>
        opt.UseSqlServer(builder.Configuration.GetConnectionString("TodoContext"))
    )
    .AddEndpointsApiExplorer()    
    .AddControllers();

builder.Services.AddSignalR();
builder.Services.AddScoped<ITodoListItemService, TodoListItemService>();
builder.Services.AddAutoMapper(cfg =>
{    
    cfg.AddProfile(typeof(Program));
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

app.UseAuthorization();
app.MapHub<TodoHub>("/todohub");
app.MapControllers();
app.Run();
