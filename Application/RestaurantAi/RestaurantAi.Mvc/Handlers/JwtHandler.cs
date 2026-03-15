using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace RestaurantAi.Mvc.Handlers
{
    public class JwtHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<JwtHandler> _log;

        public JwtHandler(IHttpContextAccessor httpContextAccessor, ILogger<JwtHandler> log)
        {
            _httpContextAccessor = httpContextAccessor;
            _log = log;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    _log.LogInformation("JwtHandler attached Authorization header to outgoing request {Method} {RequestUri}", request.Method, request.RequestUri);
                }
                else
                {
                    _log.LogInformation("JwtHandler found no JWToken in session for request {Method} {RequestUri}", request.Method, request.RequestUri);
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "JwtHandler failed to read token from session");
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}