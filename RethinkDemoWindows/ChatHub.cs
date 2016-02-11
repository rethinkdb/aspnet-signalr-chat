using System;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json.Linq;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;

namespace RethinkDemoWindows
{
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
                .runResult<JArray>(conn);
            return output;
        }
    }
}