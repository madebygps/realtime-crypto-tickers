using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Completed.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Completed.Api
{
    public class TickerFunctions
    {


        [Function("GetTickers")]
        public async Task<MyOutputType> RunTimer([TimerTrigger("*/15 * * * * *", UseMonitor = true)] MyInfo myTimer, FunctionContext context)
        {

            Coin[]? prices;
            MySignalRMessage mySignalRMessage;

            var logger = context.GetLogger("GetTickers");
            //logger.LogInformation($"C# Timer trigger function executed at: {myTimer.ScheduleStatus?.LastUpdated}");
            logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");


            using (var httpClient = new HttpClient())
            {

                var coinData = await httpClient.GetAsync("https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc&per_page=100&page=1&sparkline=false");
                var body = await coinData.Content.ReadAsStringAsync();
                prices = JsonSerializer.Deserialize<Coin[]>(body);

                if (prices is not null)
                {
                    /*foreach (var coin in prices)
                    {
                        logger.LogInformation(coin.symbol);

                    }*/

                    mySignalRMessage = new MySignalRMessage()
                    {
                        Target = "updated",
                        Arguments = new object[] { prices }
                    };
                    return new MyOutputType()
                    {
                        mySignalRMessage = mySignalRMessage,
                        CosmosCoins = prices
                    };
                }
                else
                {
                    return new MyOutputType()
                    {
                        mySignalRMessage = new MySignalRMessage(),
                        CosmosCoins = new Coin[1] 
                    };
                }
            }
        }

        [Function("GetTickersJson")]
        public static object Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
            [CosmosDBInput("%DatabaseName%", "%CollectionName%", ConnectionStringSetting = "CosmosDBConnectionString")] Coin[] coins,
                FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("GetTickersJson");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return coins;
        }

    }


    public class MyInfo
    {
        public MyScheduleStatus? ScheduleStatus { get; set; }
    }

    public class MyScheduleStatus
    {
        public DateTime Last { get; set; }

        public DateTime Next { get; set; }

        public DateTime LastUpdated { get; set; }
    }
    public class MyOutputType
    {
        [SignalROutput(HubName = "stocks", ConnectionStringSetting = "AzureSignalRConnectionString")]
        public MySignalRMessage? mySignalRMessage { get; set; }

        [CosmosDBOutput("%DatabaseName%", "%CollectionName%", ConnectionStringSetting = "CosmosDBConnectionString")]
        public Coin[]? CosmosCoins { get; set; }

    }
    public class MySignalRMessage
    {
        public string? Target { get; set; }

        public object? Arguments { get; set; }
    }
}
