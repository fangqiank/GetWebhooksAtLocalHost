var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

app.MapPost("/webhooks", (HttpContext ctx) =>
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("--> We've got a hit");

    var headers = ctx.Request.Headers;

    foreach (var item in headers)
    {
        Console.WriteLine($"{item.Key} / {item.Value}");
    }

    Console.ResetColor();

    return Results.Ok();
});

app.Run();
