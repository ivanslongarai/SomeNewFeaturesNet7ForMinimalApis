using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

/* Using filters with Minimal Api's */

app.MapGet("filters", (string secretKey) =>
{
    return "Using filters with Minimal Api's";
}).AddEndpointFilter(async (ctx, next) =>
{
    // This method runs before the endpoint execution
    if(!ctx!.HttpContext!.Request!.QueryString!.Value!.Contains("ValidFormatAndValueKey"))
    {
        return Results.BadRequest();
    }

    return await next(ctx);    
});


/* Using file uploading */

app.MapPost("/upload-file", async (IFormFile file) =>
{
    string tempFile = CreateFileTempPath("upload");
    using var stream = File.OpenWrite(tempFile);
    await file.CopyToAsync(stream);

    return Results.Ok("File sent successfully");    
});

app.MapPost("/upload-files", async (IFormFileCollection files) =>
{
    foreach (var file in files)
    {
        string tempFile = CreateFileTempPath("uploads");
        using var stream = File.OpenWrite(tempFile);
        await file.CopyToAsync(stream);
    }

    return Results.Ok("Files sent successfully");
});

/* Array binding */

app.MapGet("search", (string[] something) => {
    var result = $"Names: {something[0]}, {something[1]}, { something[2]}";
    return Results.Ok(result);
});

app.MapGet("search-as-parameter", ([AsParameters] Book book) => {
    return $"Book: {book.Title}, {book.Author}, {book.Year}";
});

/* Short-circuiting

    When a delegate fails to pass a request to the next delegate, it is considered to be short-circuiting the request pipeline. 
    Short circuiting is generally desirable because it avoids unnecessary work in some cases.
*/

app.MapGet("short-circuiting", () => "It'll never be executed").AddEndpointFilter<ShortCircuit>();

app.UseHttpsRedirection();

app.Run();


/* Auxiliar methods for Using file uploading */

static string CreateFileTempPath(string fileFolder)
{
    var fileName = $@"{DateTime.Now.Ticks}.tmp";
    var directoryPath = Path.Combine("temp", fileFolder);
    if (!Directory.Exists(directoryPath))
    {
        Directory.CreateDirectory(directoryPath);
    }
    return Path.Combine(directoryPath, fileName);
}

/* Auxiliar class for  Array binding */
public class Book
{
    [Required]
    public string Author { get; set; } = string.Empty;
    [Required]
    public string Title { get; set; } = string.Empty;
    public int Year { get; set; }
}

/* Auxiliar class for the ShortCircuit endpoint */
public class ShortCircuit : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, 
        EndpointFilterDelegate next)
    {
        return new ValueTask<object?>(Results.Json(new { Short = "Circuit" }));
    }
}