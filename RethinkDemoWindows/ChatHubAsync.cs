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
        public static RethinkDB R = RethinkDB.R;
        static Connection conn;

        internal static async Task Init()
        {
            conn = await R.Connection().ConnectAsync();
        }

        public async Task Send(string name, string message)
        {
            await R.Db("test").Table("chat")
                    .Insert(new ChatMessage
                    {
                        username = name,
                        message = message,
                        timestamp = DateTime.Now
                    }).RunResultAsync(conn);
        }

        public async Task<JArray> History(int limit)
        {
            return await R.Db("test").Table("chat")
                    .OrderBy(R.Desc("timestamp"))
                    .Limit(limit)
                    .OrderBy("timestamp")
                    .RunResultAsync<JArray>(conn);
        }
    }
}