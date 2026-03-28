using System;
using System.Text.Json;
using System.Threading.Tasks;
using SocketIOClient;

namespace MarketSocket
{
    public class XTSMarketDataSocketClient
    {
        private readonly string? token;
        private readonly string? userID;
        private readonly string root_url;
        private readonly string publishFormat = "JSON";
        private readonly string broadcastMode = "Full";
        private readonly string connection_url;

        private readonly MarketDataSocketClient clientHandler;
        private SocketIOClient.SocketIO socket;

        public XTSMarketDataSocketClient(string? token, string? userID, string root_url, MarketDataSocketClient handler)
        {
            this.token = token;
            this.userID = userID;
            this.root_url = root_url;
            this.clientHandler = handler;

            connection_url = $"{root_url}/?token={token}&userID={userID}&publishFormat={publishFormat}&broadcastMode={broadcastMode}";
        }

        public async Task Connect()
        {
            socket = new SocketIOClient.SocketIO(connection_url, new SocketIOOptions
            {
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
                Reconnection = true,
                ReconnectionAttempts = 3,
                ConnectionTimeout = TimeSpan.FromSeconds(10)
            });

            socket.OnConnected += async (sender, e) =>
            {
                await clientHandler.on_connect();
            };

            socket.OnDisconnected += async (sender, reason) =>
            {
                await clientHandler.on_disconnect();
            };

            socket.On("message", async ctx =>
            {
                await clientHandler.on_message(ctx.GetValue<string>(0));
            });

            socket.On("1501-json-full", async ctx =>
            {
                await clientHandler.on_event_touchline_full(ctx.GetValue<JsonElement>(0));
            });

            socket.On("1501-json-partial", async ctx =>
            {
                await clientHandler.on_event_touchline_partial(ctx.GetValue<JsonElement>(0));
            });

            socket.On("1502-json-full", async ctx =>
            {
                await clientHandler.on_event_market_data_full(ctx.GetValue<JsonElement>(0));
            });

            socket.On("1502-json-partial", async ctx =>
            {
                await clientHandler.on_event_market_data_partial(ctx.GetValue<JsonElement>(0));
            });

            socket.On("1505-json-full", async ctx =>
            {
                await clientHandler.on_event_candle_data_full(ctx.GetValue<JsonElement>(0));
            });

            socket.On("1505-json-partial", async ctx =>
            {
                await clientHandler.on_event_candle_data_partial(ctx.GetValue<JsonElement>(0));
            });

            socket.On("1507-json-full", async ctx =>
            {
                await clientHandler.on_event_market_status_full(ctx.GetValue<JsonElement>(0));
            });

            socket.On("1510-json-full", async ctx =>
            {
                await clientHandler.on_event_openinterest_full(ctx.GetValue<JsonElement>(0));
            });

            socket.On("1510-json-partial", async ctx =>
            {
                await clientHandler.on_event_openinterest_partial(ctx.GetValue<JsonElement>(0));
            });

            socket.On("1512-json-full", async ctx =>
            {
                await clientHandler.on_event_last_traded_price_full(ctx.GetValue<JsonElement>(0));
            });

            socket.On("1512-json-partial", async ctx =>
            {
                await clientHandler.on_event_last_traded_price_partial(ctx.GetValue<JsonElement>(0));
            });

            socket.On("1105-json-full", async ctx =>
            {
                await clientHandler.on_event_instrument_change_full(ctx.GetValue<JsonElement>(0));
            });

            socket.On("1105-json-partial", async ctx =>
            {
                await clientHandler.on_event_instrument_change_partial(ctx.GetValue<JsonElement>(0));
            });

            await socket.ConnectAsync();
        }

        public override string ToString()
        {
            return $"XTSMarketDataSocketClient(token={token}, userID={userID}, root_url={root_url})";
        }
    }

    public class MarketDataSocketClient
    {
        public Task on_connect()
        {
            Console.WriteLine("Connected");
            return Task.CompletedTask;
        }

        public Task on_disconnect()
        {
            Console.WriteLine("Disconnected");
            return Task.CompletedTask;
        }

        public Task on_message(string data)
        {
            Console.WriteLine($"Message: {data}");
            return Task.CompletedTask;
        }

        public Task on_event_touchline_full(JsonElement data) => Print("Touchline Full", data);
        public Task on_event_touchline_partial(JsonElement data) => Print("Touchline Partial", data);
        public Task on_event_market_data_full(JsonElement data) => Print("Market Full", data);
        public Task on_event_market_data_partial(JsonElement data) => Print("Market Partial", data);
        public Task on_event_candle_data_full(JsonElement data) => Print("Candle Full", data);
        public Task on_event_candle_data_partial(JsonElement data) => Print("Candle Partial", data);
        public Task on_event_market_status_full(JsonElement data) => Print("Market Status", data);
        public Task on_event_openinterest_full(JsonElement data) => Print("OI Full", data);
        public Task on_event_openinterest_partial(JsonElement data) => Print("OI Partial", data);
        public Task on_event_last_traded_price_full(JsonElement data) => Print("LTP Full", data);
        public Task on_event_last_traded_price_partial(JsonElement data) => Print("LTP Partial", data);
        public Task on_event_instrument_change_full(JsonElement data) => Print("Instrument Full", data);
        public Task on_event_instrument_change_partial(JsonElement data) => Print("Instrument Partial", data);

        private Task Print(string type, JsonElement data)
        {
            Console.WriteLine($"{type}: {data}");
            return Task.CompletedTask;
        }
    }
}