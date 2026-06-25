using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.API.Services;
using AjoCoreBackend.Persistence.Extensions;
using AjoCoreBackend.Infrastructure.Extensions;
using AjoCoreBackend.Application.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register Clean Architecture Layers
builder.Services.AddApplicationLayer();
builder.Services.AddPersistenceInfrastructure(builder.Configuration);
builder.Services.AddExternalServices();

// Register API specific services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<AjoCoreBackend.API.Middlewares.ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
