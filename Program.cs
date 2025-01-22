using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure to listen on HTTP only for simplicity
builder.WebHost.ConfigureKestrel(options => {
    options.ListenLocalhost(5126);
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

Log.Logger = new LoggerConfiguration()
    // .MinimumLevel.Warning()
    .WriteTo.Console()
    .WriteTo.File("logs/usermanagementapi.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options => {
        options.SuppressModelStateInvalidFilter = true;
    });
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Use(async (context, next) => {
    try {
        await next();
    }
    catch (Exception ex) {
        Console.WriteLine($"Global exception caught: {ex}");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("An unexpected error occured. Please try again later.");
    }
});

// Middleware to log security events if response status indicates an issue
app.Use(async (context, next) =>
{
    await next(); // Run the next middleware first

    if (context.Response.StatusCode >= 400)
    {
        Console.WriteLine($"Security Event: {context.Request.Path} - Status Code: {context.Response.StatusCode}");
    }
});

// Simulated HTTPS Enforcement Middleware
app.Use(async (context, next) =>
{
    // Check for a query parameter to simulate HTTPS enforcement (e.g., "?secure=true")
    if (context.Request.Query["secure"] != "true")
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Simulated HTTPS Required");
        return;
    }

    await next();
});

// Middleware for input validation
app.Use(async (context, next) =>
{
    var input = context.Request.Query["input"];
    if (!IsValidInput(input))
    {
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid Input");
        }
        return;
    }

    await next();
});

// Middleware for short-circuiting unauthorized access
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/unauthorized")
    {
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized Access");
        }
        return; // Exit middleware pipeline early if unauthorized
    }
    await next();
});

// Middleware for simulated authentication and secure cookies
app.Use(async (context, next) =>
{
    // Simulate authentication with a query parameter (e.g., "?authenticated=true")
    var isAuthenticated = context.Request.Query["authenticated"] == "true";
    if (!isAuthenticated)
    {
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Access Denied");
        }
        return;
    }

    context.Response.Cookies.Append("SecureCookie", "SecureData", new CookieOptions
    {
        HttpOnly = true,
        Secure = true
    });

    await next();
});

app.UseRouting();
app.MapControllers();

// Helper method for input validation
static bool IsValidInput(string input)
{
    // Checks for any unsafe characters or patterns, including "<script>"
    return string.IsNullOrEmpty(input) || (input.All(char.IsLetterOrDigit) && !input.Contains("<script>"));
}

app.Run();