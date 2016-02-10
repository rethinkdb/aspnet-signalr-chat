using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.SignalR;
using RethinkDb.Driver;
using Newtonsoft.Json.Linq;
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

    public class ChatHub : Hub
    {
        public static RethinkDB r = RethinkDB.r;

        public void Send(string name, string message)
        {
            var conn = r.connection().connect();
            r.db("test").table("chat")
             .insert(new ChatMessage {
                 username = name,
                 message = message,
                 timestamp = DateTime.Now
             }).run(conn);
            conn.close();
        }

        public JArray History(int limit)
        {
            var conn = r.connection().connect();
            var output = r.db("test").table("chat")
                          .orderBy(r.desc("timestamp"))
                          .limit(limit)
                          .orderBy("timestamp")
                          .coerceTo("array")
                          .run<JObject>(conn);
            conn.close();
            return output;
        }
    }

    class ChangeHandler
    {
        public static RethinkDB r = RethinkDB.r;

        async public void handleUpdates()
        {
            var hub = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            var conn = r.connection().connect();
            var feed = r.db("test").table("chat")
                        .changes().runChangesAsync<ChatMessage>(conn);

            foreach (var message in await feed)
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
            new ChangeHandler().handleUpdates();
            app.MapSignalR();
        }
    }
}
