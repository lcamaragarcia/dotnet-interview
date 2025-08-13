using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.External;
using TodoApi.Hubs;
using TodoApi.Mappings;
using TodoApi.Services;
using TodoApi.Synchronization;

var builder = WebApplication.CreateBuilder(args);
builder
    .Services.AddDbContext<TodoContext>(opt =>
        opt.UseSqlServer(builder.Configuration.GetConnectionString("TodoContext"))
    )
    .AddEndpointsApiExplorer()    
    .AddControllers();

builder.Services.AddSignalR();

builder.Services.AddHttpClient<IExternalTodoApiClient, ExternalTodoApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApi:BaseUrl"]!);
});

builder.Services.AddScoped<ISynchronizationService, SynchronizationService>();
builder.Services.AddHostedService<SynchronizationBackgroundService>();
builder.Services.AddScoped<ITodoListService, TodoListService>();
builder.Services.AddScoped<ITodoListItemService, TodoListItemService>();

builder.Services.AddAutoMapper(cfg =>
{    
    cfg.AddProfile(typeof(MappingProfile));
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

app.UseAuthorization();
app.MapHub<TodoHub>("/todohub");
app.MapControllers();
app.Run();
