using System.IO;
using System.Threading.Tasks;
using Bulwark.Integration.GitLab;
using Bulwark.Integration.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Bulwark.Integration.WebHook.Controllers
{
    public class GitLabController : Controller
    {
        readonly ILogger<GitLabController> _logger;
        readonly IMessageSender _messageSender;

        public GitLabController(ILogger<GitLabController> logger,
            IMessageSender messageSender)
        {
            _logger = logger;
            _messageSender = messageSender;
        }
        
        public async Task<ActionResult> Index()
        {
            string eventType = null;
            if (Request.Headers.TryGetValue("X-Gitlab-Event", out StringValues temp))
            {
                eventType = temp;
            }

            switch (eventType)
            {
                case "Merge Request Hook":
                    var mergeRequest = await DeserializeBody<GitLab.Api.MergeRequest>();
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
                var content = await streamReader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<T>(content);
            }
        }
    }
}