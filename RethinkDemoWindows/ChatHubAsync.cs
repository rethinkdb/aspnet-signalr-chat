using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json.Linq;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;

namespace RethinkDemoWindows
{
    public class ChatHubAsync : Hub
    {
        public static RethinkDB r = RethinkDB.r;
        static Connection conn;

        internal static async Task Init()
        {
            conn = await r.connection().connectAsync();
        }

        public async Task Send(string name, string message)
        {
            await r.db("test").table("chat")
                    .insert(new ChatMessage
                    {
                        username = name,
                        message = message,
                        timestamp = DateTime.Now
                    }).runResultAsync(conn);
        }

        public async Task<JArray> History(int limit)
        {
            return await r.db("test").table("chat")
                    .orderBy(r.desc("timestamp"))
                    .limit(limit)
                    .orderBy("timestamp")
                    .runResultAsync<JArray>(conn);
        }
    }
}