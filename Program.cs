using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.HttpLogging;
using Serilog;

using UserManagementApi.Middleware;

try {
    var builder = WebApplication.CreateBuilder(args);

    // builder.Services.ConfigureHttpJsonOptions(options => {
    //     options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    // });
    // builder.Services.AddHttpLogging(logging => {
    //     logging.LoggingFields = HttpLoggingFields.All;
    // });

    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .CreateLogger();

    builder.Host.UseSerilog(Log.Logger);

    // builder.Services.AddSerilog((services, config) => config
    //     .ReadFrom.Configuration(builder.Configuration)
    //     .ReadFrom.Services(services)
    //     .Enrich.FromLogContext()
    // );

    builder.Services.AddControllers();

    builder.Services.AddSwaggerGen();
    
    var app = builder.Build();

    // if (!app.Environment.IsDevelopment()) {
    //     app.UseExceptionHandler("/error");
    // } else {
    //     app.UseDeveloperExceptionPage();

    // }
    if (app.Environment.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // app.UseHttpLogging();
    app.UseSerilogRequestLogging();

    app.UseRouting();

    app.UseMiddleware<TokenValidationMiddleware>();
    app.UseMiddleware<RequestResponseLoggingMiddleware>();
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.MapControllers();

    // app.MapGet("/error", () => Results.Problem("An error occured.", statusCode: StatusCodes.Status500InternalServerError));
    
    app.Run();
} catch (Exception ex) {
    Log.Fatal(ex, "Application terminated unexpectedly");
} finally {
    Log.CloseAndFlush();
}