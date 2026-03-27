using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DotNetEnv;
using XtsApiClient;

class Program
{
    static async Task Main(string[] args)
    {        
        Env.Load();     // Load the file
        string API_key = Environment.GetEnvironmentVariable("API_KEY");
        string API_secret = Environment.GetEnvironmentVariable("API_SECRET");
        string API_source = Environment.GetEnvironmentVariable("API_SOURCE");
        string API_root = Environment.GetEnvironmentVariable("API_URL");

        var xt = new XtsConnect(
            API_key,
            API_secret,
            API_source,
            API_root
        );

        try
        {
            // Login
            await xt.MarketdataLoginAsync();

            List<string> top_5_nifty = new List<string> {"HDFCBANK", "RELIANCE", "BHARTIARTL", "ICICIBANK", "INFY"};
            for (int i = 0; i < 5; i++)
            {
                var current_company = await xt.SearchByScriptnameAsync(
                    searchString: top_5_nifty[i]
                );
                
                var response_of_current_company = (System.Text.Json.JsonElement)current_company["result"]!;
                var eqIds = response_of_current_company.EnumerateArray()
                            .Where(item => item.GetProperty("Series").GetString() == "EQ"   // Equity
                                   && item.GetProperty("ExchangeSegment").GetInt32() == 1   // NSECM = 1
                                   && item.GetProperty("Name").GetString() == top_5_nifty[i])
                            .Select(item => item.GetProperty("ExchangeInstrumentID").GetInt64())
                            .ToList();

                var res = await xt.GetOhlcAsync(
                    exchangeSegment: 1,  // For NSECM
                    exchangeInstrumentID: eqIds[0],
                    startTime: "Mar 27 2025 090000",
                    endTime: "Mar 27 2025 130000",
                    compressionValue: 14400
                );

                var resultElement = res["result"];
                Console.WriteLine($"OHLC Data for : {top_5_nifty[i]} is {resultElement}");
            }


            // Logout
            await xt.MarketdataLogoutAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
        }            
    }
}
