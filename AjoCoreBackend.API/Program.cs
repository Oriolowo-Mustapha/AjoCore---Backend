using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.API.Services;
using AjoCoreBackend.Persistence.Extensions;
using AjoCoreBackend.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Register Clean Architecture Layers
builder.Services.AddPersistenceInfrastructure(builder.Configuration);
builder.Services.AddExternalServices();

// Register API specific services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
