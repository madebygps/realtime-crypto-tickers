using System.Net.Http.Json;
using Blazorise.Animate;
using Completed.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Completed.Client.Pages
{
    public class GetTickersBase : ComponentBase
    {
        protected Coin[]? prices;
        [Inject]
        public HttpClient? Http { get; set; }
        protected HubConnection? hubConnection;
        public bool IsConnected => hubConnection?.State == HubConnectionState.Connected;
        //public Animate myAnim;

		/*private void RunAnimation()
		{
			myAnim.Run();
		}*/

        protected override async Task OnInitializedAsync()
        {

            // Prod
            if (Http is not null)
            {
                //prices = await Http.GetFromJsonAsync<Coin[]>("https://cryptotickersnetgps.azurewebsites.net/api/GetPricesJson");
                // Local
             //prices = await Http.GetFromJsonAsync<Coin[]>("http://localhost:7071/api/GetTickersJson");

            // Prod    
           // hubConnection = new HubConnectionBuilder().WithUrl("https://cryptotickersnetgps.azurewebsites.net/api/").Build();
            // Local
             hubConnection = new HubConnectionBuilder().WithUrl("http://localhost:7071/api").Build();

            hubConnection.On<Coin[]>("updated", (coin) =>
                {
                    prices = coin;
                    StateHasChanged();
                   // RunAnimation();
                }
            );

            await hubConnection.StartAsync();
            }

            

        }

        public async ValueTask DisposeAsync()
        {
            if (hubConnection is not null)
            {
                await hubConnection.DisposeAsync();
            }
        }
    }
}