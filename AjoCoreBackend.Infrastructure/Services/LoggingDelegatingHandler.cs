using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AjoCoreBackend.Infrastructure.Services
{
    public class LoggingDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger<LoggingDelegatingHandler> _logger;

        public LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("================ HTTP REQUEST ================");
            _logger.LogInformation("Method: {Method}, Url: {Url}", request.Method, request.RequestUri);

            if (request.Content != null)
            {
                var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("Request Body: {RequestBody}", requestBody);
            }

            var stopwatch = Stopwatch.StartNew();
            var response = await base.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            _logger.LogInformation("================ HTTP RESPONSE ================");
            _logger.LogInformation("StatusCode: {StatusCode}, Elapsed: {ElapsedMilliseconds}ms, Url: {Url}", 
                (int)response.StatusCode, stopwatch.ElapsedMilliseconds, request.RequestUri);

            if (response.Content != null)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("Response Body: {ResponseBody}", responseBody);
            }
            _logger.LogInformation("===============================================");

            return response;
        }
    }
}
