using System;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json.Linq;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;

namespace RethinkDemoWindows
{
    public class ChatHub : Hub
    {
        public static RethinkDB R = RethinkDB.R;
        static Connection conn;

        internal static void Init()
        {
            conn = R.Connection().Connect();
        }

        public void Send(string name, string message)
        {
            R.Db("test").Table("chat")
                .Insert(new ChatMessage {
                    username = name,
                    message = message,
                    timestamp = DateTime.Now
                }).Run(conn);
        }

        public JArray History(int limit)
        {
            var output = R.Db("test").Table("chat")
                .OrderBy(R.Desc("timestamp"))
                .Limit(limit)
                .OrderBy("timestamp")
                .RunResult<JArray>(conn);
            return output;
        }
    }
}