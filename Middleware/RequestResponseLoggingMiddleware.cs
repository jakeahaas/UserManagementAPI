using System.Text;

namespace UserManagementApi.Middleware {
    public class RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger) {
        public async Task InvokeAsync(HttpContext context) {
            //Log the request
            context.Request.EnableBuffering();
            var requestBody = await ReadStreamAsync(context.Request.Body);
            logger.LogInformation("Incoming Request: {Method} {Path} {Body}", context.Request.Method, context.Request.Path, requestBody);
            context.Request.Body.Position = 0;

            //Capture the response
            var originalResponseBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            await next(context);

            //Log the response
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            logger.LogInformation("Outgoing Response: {StatusCode} {Body}", context.Response.StatusCode, responseBody);

            await responseBodyStream.CopyToAsync(originalResponseBodyStream);
        }

        private static async Task<string> ReadStreamAsync(Stream stream) {
            stream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true);
            var text = await reader.ReadToEndAsync();
            stream.Seek(0, SeekOrigin.Begin);
            return text;
        }
    }
}