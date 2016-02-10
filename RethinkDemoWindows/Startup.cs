using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.SignalR;
using RethinkDb.Driver;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using RethinkDb.Driver.Net;

[assembly: OwinStartup(typeof(RethinkDemoWindows.Startup))]

namespace RethinkDemoWindows
{
    class ChatMessage
    {
        public string username { get; set; }
        public string message { get; set; }
        public DateTime timestamp { get; set; }
    }

    public class ChatHub : Hub
    {
        public static RethinkDB r = RethinkDB.r;
        static Connection conn;

        internal static void Init()
        {
            conn = r.connection().connect();
        }

        public void Send(string name, string message)
        {
            r.db("test").table("chat")
             .insert(new ChatMessage {
                 username = name,
                 message = message,
                 timestamp = DateTime.Now
             }).run(conn);
        }

        public JArray History(int limit)
        {
            var output = r.db("test").table("chat")
                          .orderBy(r.desc("timestamp"))
                          .limit(limit)
                          .orderBy("timestamp")
                          .coerceTo("array")
                          .run<JObject>(conn);
            return output;
        }
    }

    static class ChangeHandler
    {
        public static RethinkDB r = RethinkDB.r;

        public static void HandleUpdates()
        {
            var hub = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            var conn = r.connection().connect();
            var feed = r.db("test").table("chat")
                              .changes().runChanges<ChatMessage>(conn);

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

            Task.Factory.StartNew(
                ChangeHandler.HandleUpdates,
                TaskCreationOptions.LongRunning);
            
            app.MapSignalR();
        }
    }
}
