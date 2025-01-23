using System.Net;

namespace UserManagementApi.Middleware {
    
    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger) {
        public async Task InvokeAsync(HttpContext context) {
            try {
                await next(context);
            }
            catch (Exception ex) {
                logger.LogError(ex, "An unhandled exception has occurred.");
                await HandleExceptionAsync(context);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context) {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var response = new { error = "Internal server error." };
            return context.Response.WriteAsJsonAsync(response);
        }
    }
}