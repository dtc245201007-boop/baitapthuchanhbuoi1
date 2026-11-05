using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký Service vào DI Container (Phải đứng TRƯỚC builder.Build())
builder.Services.AddSingleton<GreetingService>();

var app = builder.Build();

// 1. GET / -> Trả về thông tin ứng dụng (200 OK)
app.MapGet("/", () => Results.Ok(new { application = "BasicWebApi", version = "1.0" }));

// 2. GET /hello/{name}?language=en -> Chào theo ngôn ngữ (200 OK)
// 'name' lấy từ Route, 'language' lấy từ Query String
app.MapGet("/hello/{name}", (string name, [FromQuery] string? language, GreetingService service) =>
{
    var message = service.GetGreeting(name, language);
    return Results.Ok(new { message });
});

// 3. POST /greetings -> Tạo mới lời chào
// Nếu hợp lệ: Trả 201 Created kèm Header Location
// Nếu thiếu dữ liệu: Trả 400 Bad Request
app.MapPost("/greetings", ([FromBody] GreetingRequest request, GreetingService service) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest(new { message = "Name must not be empty." });
    }

    // Đặt breakpoint (chấm đỏ) ở dòng dưới đây nếu muốn Debug (Bước 8.6)
    var createdGreeting = service.CreateGreeting(request.Name);
    
    return Results.Created($"/greetings/{createdGreeting.Id}", createdGreeting);
});

app.Run();

// ----------------------------------------------------
// Dữ liệu & Service xử lý logic
// ----------------------------------------------------
public record GreetingRequest(string Name);
public record GreetingItem(int Id, string Name, string Message, DateTime CreatedAt);

public class GreetingService
{
    private readonly List<GreetingItem> _greetings = new();
    private int _nextId = 1;

    public string GetGreeting(string name, string? language)
    {
        // Bước 8.5: Thêm ngôn ngữ thứ ba (ja -> Konnichiwa)
        return language?.ToLower() switch
        {
            "en" => $"Hello, {name}!",
            "vi" => $"Xin chao, {name}!",
            "ja" => $"Konnichiwa, {name}!",
            _ => $"Chao, {name}!"
        };
    }

    public GreetingItem CreateGreeting(string name)
    {
        var item = new GreetingItem(_nextId++, name, $"Hello, {name}!", DateTime.UtcNow);
        _greetings.Add(item);
        return item;
    }
}
