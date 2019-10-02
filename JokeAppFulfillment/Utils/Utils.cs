using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;

namespace JokeAppFulfillment.Utils
{
    static class Utils
    {
        static void LogRequest(this HttpRequest req, ILogger logger)
        {
            var injectedRequestStream = new MemoryStream();

            try
            {
                var logString = new StringBuilder();
                logString.AppendLine($"REQUEST HttpMethod: {req.Method}, Path: {req.Path}");

                using (var bodyReader = new StreamReader(req.Body))
                {
                    var bodyAsText = bodyReader.ReadToEnd();
                    if (string.IsNullOrWhiteSpace(bodyAsText) == false)
                    {
                        logString.AppendLine($", Body : {bodyAsText}");
                    }

                    var bytesToWrite = Encoding.UTF8.GetBytes(bodyAsText);
                    injectedRequestStream.Write(bytesToWrite, 0, bytesToWrite.Length);
                    injectedRequestStream.Seek(0, SeekOrigin.Begin);
                    context.Request.Body = injectedRequestStream;
                }

                _logger.LogTrace(requestLog);

                await _next.Invoke(context);
            }
            finally
            {
                injectedRequestStream.Dispose();
            }
        }
    }
}
