using IdentityCore.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace IdentityCore.Services
{
    public class SessionService : ISessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SessionService> _logger;

        public SessionService(IHttpContextAccessor httpContextAccessor, ILogger<SessionService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public string GetCurrentTenantId()
        {
            var tenantId = _httpContextAccessor.HttpContext.Request.Headers["tenantId"];
            return tenantId;
        }

        public string GetCorrelationId()
        {
            string requestId = string.Empty;
            if (string.IsNullOrEmpty(requestId))
                requestId = GetHeaderValueOrDefault("request-id");
            if (string.IsNullOrEmpty(requestId))
                requestId = GetHeaderValueOrDefault("Request-Id");
            if (string.IsNullOrEmpty(requestId))
                _logger.LogWarning($"No form of request id found. Correlation Id will be incorrectly logged.");

            return requestId;
        }

        private string GetHeaderValueOrDefault(string key)
        {
            return (string)_httpContextAccessor?.HttpContext?.Request?.Headers?.Where(x => x.Key == key).Select(x => x.Value).FirstOrDefault();
        }
    }
}
