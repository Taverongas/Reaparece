using Reaparece.Api.Services;

const string PoliticaFrontend = "Frontend";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<UsuarioStore>();
builder.Services.AddSingleton<ComunidadStore>();

builder.Services.AddCors(options =>
    options.AddPolicy(PoliticaFrontend, policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(PoliticaFrontend);

app.UseAuthorization();

app.MapControllers();

app.Run();
