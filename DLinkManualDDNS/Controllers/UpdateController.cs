using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;

namespace DLinkManualDDNS.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class UpdateController : ControllerBase
    {
        private readonly ILogger<UpdateController> _logger;

        public UpdateController(ILogger<UpdateController> logger) => _logger = logger;

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> UpdateHost([FromQuery, Required] string hostname)
        {
            try
            {
                var userAgent = Request.Headers["User-Agent"].FirstOrDefault();
                var clientIp = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

                _logger.LogInformation($"[Identifier: {Request.HttpContext.TraceIdentifier}] Updating IP: {clientIp} User-Agent: {userAgent}");

                // Get Username / Password.
                var authorization = Request.Headers["Authorization"].FirstOrDefault().Replace("Basic", "").Trim();
                var bytes = Convert.FromBase64String(authorization);
                var userPwd = Encoding.ASCII.GetString(bytes).Split(':');

                using var httpClient = new HttpClient(new HttpClientHandler()
                {
                    Credentials = new NetworkCredential(userPwd[0], userPwd[1]),
                });

                var response = await httpClient.GetStringAsync($"https://domains.google.com/nic/update?hostname={hostname}&myip={clientIp}");
                _logger.LogInformation($"[Identifier: {Request.HttpContext.TraceIdentifier}] Response: {response}");

                return response switch
                {
                    string a when a.StartsWith("good") => StatusCode(StatusCodes.Status200OK, "The update was successful!"),
                    string b when b.StartsWith("nochg") => StatusCode(StatusCodes.Status200OK, $"The supplied IP address {clientIp} is already set for this host."),
                    string c when c.StartsWith("nohost") => StatusCode(StatusCodes.Status404NotFound, $"The hostname does not exist, or does not have Dynamic DNS enabled.\nHostname: {hostname}"),
                    string d when d.StartsWith("badauth") => StatusCode(StatusCodes.Status401Unauthorized, "The username / password combination is not valid for the specified host!"),
                    string e when e.StartsWith("notfqdn") => StatusCode(StatusCodes.Status400BadRequest, $"The supplied hostname is not a valid fully-qualified domain name!\nHostname: {hostname}"),
                    string f when f.StartsWith("badagent") => StatusCode(StatusCodes.Status400BadRequest, "Your Dynamic DNS client is making bad requests. Ensure the user agent is set in the request, and that you’re only attempting to set an IPv4 address. IPv6 is not supported."),
                    string g when g.StartsWith("abuse") => StatusCode(StatusCodes.Status401Unauthorized, "Dynamic DNS access for the hostname has been blocked due to failure to interpret previous responses correctly."),
                    string h when h.StartsWith("911") => StatusCode(StatusCodes.Status500InternalServerError, "An error happened on our end. Wait 5 minutes and retry."),
                    _ => StatusCode(StatusCodes.Status501NotImplemented, response)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[Identifier: {Request.HttpContext.TraceIdentifier}]");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal error.");
            }
        }
    }
}