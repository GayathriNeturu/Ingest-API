var builder = WebApplication.CreateBuilder(args);

// Register Controllers
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

//Enables attribute based controllers
app.MapControllers();

app.Run();
