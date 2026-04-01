using NansHoi4Tool.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Configure SignalR with detailed errors for easier debugging with your friend
builder.Services.AddSignalR(opts =>
{
    opts.EnableDetailedErrors = true;
    opts.MaximumReceiveMessageSize = 1024 * 1024; // 1MB limit for project changes
});

// Updated CORS: SetIsOriginAllowed(_ => true) allows your friend to connect 
// even if their browser/client origin doesn't match your local machine
builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(p =>
        p.SetIsOriginAllowed(_ => true) 
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()));

var app = builder.Build();

// Enhanced Logging Middleware: This will print to your console the moment 
// your friend's computer tries to "knock on the door" of your server
app.Use(async (context, next) => 
{
    var path = context.Request.Path.Value;
    if (path != null && path.StartsWith("/hub")) 
    {
        Console.WriteLine($"[Incoming Hub Connection] {DateTime.Now:T} - IP: {context.Connection.RemoteIpAddress}");
    }
    await next();
});

app.UseCors();

// Map the hub defined in ModProjectHub.cs
app.MapHub<ModProjectHub>("/hub");

// Health check endpoint to verify the server is reachable via browser
app.MapGet("/health", () => new { status = "ok", time = DateTime.UtcNow, server = "NansHoi4Tool" });

var port = args.Length > 0 ? int.Parse(args[0]) : 51420;

// CRITICAL: "http://*:{port}" binds to all network adapters, including your Hamachi IP.
// Using "localhost" here would block all external connections.
app.Urls.Add($"http://*:{port}");

Console.WriteLine("=============================================");
Console.WriteLine($"   Nan's Hoi4 Tool - Collaboration Server");
Console.WriteLine("=============================================");
Console.WriteLine($"Status:      Active");
Console.WriteLine($"Port:        {port}");
Console.WriteLine($"Local:       http://localhost:{port}");
Console.WriteLine($"Action:      Ensure your friend pings your Hamachi IP.");
Console.WriteLine($"Instruction: They should connect to http://[YOUR_HAMACHI_IP]:{port}");
Console.WriteLine("=============================================");

app.Run();