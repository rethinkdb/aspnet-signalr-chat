using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.SignalR;
using RethinkDb.Driver;
using System.Collections.Generic;

[assembly: OwinStartup(typeof(RethinkDemoWindows.Startup))]

namespace RethinkDemoWindows
{
    class ChatMessage
    {
        public string username { get; set; }
        public string message { get; set; }
        public DateTime timestamp { get; set; }
    }

    static class ChangeHandler
    {
        public static RethinkDB R = RethinkDB.R;

        public static void HandleUpdates()
        {
            var hub = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            var conn = R.Connection().Connect();
            var feed = R.Db("test").Table("chat")
                              .Changes().RunChanges<ChatMessage>(conn);

            foreach (var message in feed)
                hub.Clients.All.onMessage(
                    message.NewValue.username,
                    message.NewValue.message,
                    message.NewValue.timestamp);
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ChatHub.Init();
            //Best we can do without async void 
            //ChatHubAsync.Init().Wait();

            Task.Factory.StartNew(
                ChangeHandler.HandleUpdates,
                TaskCreationOptions.LongRunning);
            
            app.MapSignalR();
        }
    }
}
