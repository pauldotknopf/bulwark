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
                case "Push Hook":
                    var push = await DeserializeBody<PushHook>();
                    await _messageSender.Send(new PushEvent
                    {
                        Push = push
                    });
                    return Ok();
                case "Merge Request Hook":
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
                var content = await streamReader.ReadToEndAsync();
                Debug.WriteLine(content);
                return JsonConvert.DeserializeObject<T>(content);
            }
        }
    }
}