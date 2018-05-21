using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Bulwark.Integration.GitLab;
using Bulwark.Integration.GitLab.Api.Hooks;
using Bulwark.Integration.GitLab.Events;
using Bulwark.Integration.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Bulwark.Integration.WebHook.Controllers
{
    public class GitLabController : Controller
    {
        readonly ILogger<GitLabController> _logger;
        readonly IMessageSender _messageSender;
        readonly GitLabOptions _options;

        public GitLabController(ILogger<GitLabController> logger,
            IMessageSender messageSender,
            IOptions<GitLabOptions> options)
        {
            _logger = logger;
            _messageSender = messageSender;
            _options = options.Value;
        }
        
        public async Task<ActionResult> Index()
        {
            if (!_options.Enabled)
            {
                _logger.LogWarning("A GitLab hook request was recieved, but isn't enabled.");
                return BadRequest();
            }
            
            string eventType = null;
            if (Request.Headers.TryGetValue("X-Gitlab-Event", out StringValues eventTypeTemp))
            {
                eventType = eventTypeTemp;
            }
            
            string secretToken = null;
            if (Request.Headers.TryGetValue("X-Gitlab-Token", out StringValues secretTokenTemp))
            {
                secretToken = secretTokenTemp;
            }

            if (!string.IsNullOrEmpty(_options.SecretToken))
            {
                // We are validating the sender knows our secret token.
                if (_options.SecretToken != secretToken)
                {
                    _logger.LogWarning("Invalid request, secret token didn't match");
                    return Unauthorized();
                }
            }
            
            switch (eventType)
            {
                case "Push Hook":
                    _logger.LogInformation("Sending push hook event");
                    var push = await DeserializeBody<PushHook>();
                    await _messageSender.Send(new PushEvent
                    {
                        Push = push
                    });
                    return Ok();
                case "Merge Request Hook":
                    _logger.LogInformation("Sending merge request event");
                    var mergeRequest = await DeserializeBody<MergeRequestHook>();
                    await _messageSender.Send(new MergeRequestEvent
                    {
                        MergeRequest = mergeRequest
                    });
                    return Ok();
                default:
                    _logger.LogWarning($"Unknow event type: \"{eventType}\"");
                    return BadRequest();
            }  
        }

        private async Task<T> DeserializeBody<T>()
        {
            using (var streamReader = new StreamReader(Request.Body))
            {
                return JsonConvert.DeserializeObject<T>(await streamReader.ReadToEndAsync());
            }
        }
    }
}