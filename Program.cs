using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), ServerVersion.Parse("8.0.36-mysql")));

builder.Services.AddScoped<ToDoDbContext>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("myAppCors", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod();
    });
});

//builder.WebHost.UseUrls("https://localhost:5152");

var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors("myAppCors");
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }
app.MapGet("/", () => "API is running");
app.MapGet("/items", async (ToDoDbContext dbContext) =>
{
    var items = await dbContext.Items.ToListAsync();
    return items;
});
app.MapPost("/items", async (Item newItem, ToDoDbContext dbContext) =>
{
    dbContext.Items.Add(newItem);
    await dbContext.SaveChangesAsync();
    return newItem;
});

app.MapPut("/items/{id}", async (int id, bool IsComplete, ToDoDbContext dbContext) =>
{
    var itemToUpdate = await dbContext.Items.FindAsync(id);
    if (itemToUpdate != null)
    {
        // itemToUpdate.Name = updatedItem.Name; // Assuming you only want to update the name
        itemToUpdate.IsComplete = IsComplete;
        await dbContext.SaveChangesAsync();
        return Results.Ok();
    }
    else
    {
        return Results.NotFound($"Item with ID {id} not found");
    }
});
 app.MapDelete("/items/{id}", async (int id,ToDoDbContext dbContext) =>
{
    var itemToRemove = await dbContext.Items.FirstOrDefaultAsync(item => item.Id == id);

    // var id = context.Request.RouteValues["id"] as string;
    if (itemToRemove != null)
    {
        dbContext.Items.Remove(itemToRemove);
        await dbContext.SaveChangesAsync();
        return Results.Ok();
    }
    else
    {
        return Results.NotFound($"Item with ID {id} not found");
    }
});


app.Run();
