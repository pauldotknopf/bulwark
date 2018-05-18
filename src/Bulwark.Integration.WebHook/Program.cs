using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace Bulwark.Integration.WebHook
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            //JsonConvert.DeserializeObject<Bulwark.Integration.GitLab.Api.MergeRequestApprovals>(
            //    "{\"id\":4,\"iid\":3,\"project_id\":1,\"title\":\"Pull branchdcsd\",\"description\":\"sdsdflkjjhbsdfsdfsdfkj\",\"state\":\"opened\",\"created_at\":\"2018-05-16T19:51:14.171Z\",\"updated_at\":\"2018-05-17T11:59:26.294Z\",\"merge_status":"can_be_merged","approvals_required":1000,"approvals_left":3,"approved_by":[],"suggested_approvers":[{"id":2,"name":"Test User1","username":"user1","state":"active","avatar_url":"https://www.gravatar.com/avatar/9911037e9fdaff5563cf6b56bda59b16?s=80\u0026d=identicon","web_url":"http://192.168.6.170/user1"},{"id":3,"name":"Test User2","username":"user2","state":"active","avatar_url":"https://www.gravatar.com/avatar/780a0f7d8632b75d5bce11b4b0e37440?s=80\u0026d=identicon","web_url":"http://192.168.6.170/user2"},{"id":4,"name":"Test User2","username":"user3","state":"active","avatar_url":"https://www.gravatar.com/avatar/6c4aceaa06ef85f3ae0715b6c70960d7?s=80\u0026d=identicon","web_url":"http://192.168.6.170/user3"}],"user_has_approved":false,"user_can_approve":false}");
            
            BuildWebHost(args).Run();
        }

        private static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://*:5000")
                .UseStartup<Startup>()
                .Build();
    }
}