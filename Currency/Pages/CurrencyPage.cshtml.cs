using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System;
using Microsoft.AspNetCore.Localization;
using RestSharp;
using RestSharp.Serializers;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

#pragma warning disable 0618
namespace Currency.Pages
{
    public class CurrencyPageModel : PageModel
    {
        static List<string> CurrenciesList { get; set; } = new();
        public SelectList Currencies { get; set; }

        [BindProperty(SupportsGet = true)]
        [StringLength(3)]
        public string CurrencyFrom { get; set; }

        [BindProperty(SupportsGet = true)]
        [StringLength(3)]
        public string CurrencyTo { get; set; }
        [BindProperty(SupportsGet = true)]
        public decimal Quantity { get; set; }

        public decimal Result { get; set; }
        public void OnGet()
        {
            RestClientOptions options = new("https://api.apilayer.com")
            {
                ThrowOnAnyError = true,
                Timeout = -1
            };
            RestClient client = new(options);
            var request = new RestRequest("/currency_data/list", Method.Get);
            request.AddHeader("apikey", "ZHuLoYDee0XTzJM9H56mMJgAqAB59ZLw");
            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };

            RestResponse response = client.Execute(request);
            using (StreamWriter sw = new("wwwroot/listcurrencies.json"))
            {
                sw.WriteLine(response.Content);
            }

            ListModel dataFromJson = new();
            using (StreamReader sr = new("wwwroot/listcurrencies.json"))
            {
                string json = sr.ReadToEnd();
                dataFromJson = JsonSerializer.Deserialize<ListModel>(json);
            }
            CurrenciesList = dataFromJson.currencies.Keys.ToList();
            Currencies = new(CurrenciesList);
        }

        public IActionResult OnPost()
        {
            if (CurrencyFrom == CurrencyTo)
            {
                return RedirectToPage("Nothing");
            }
            if (!ModelState.IsValid)
            {
                return RedirectToPage("Nothing");
            }

            //RestClient Options
            RestClientOptions options = new("https://api.apilayer.com")
            {
                ThrowOnAnyError = true,
                Timeout = -1
            };

            RestClient client = new(options);
            var request = new RestRequest($"/currency_data/live?source={CurrencyFrom}&currencies={CurrencyTo}", Method.Get);
            request.AddHeader("apikey", "ZHuLoYDee0XTzJM9H56mMJgAqAB59ZLw");
            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };

            //Getting data and serializing
            RestResponse response = client.Execute(request);
            using (StreamWriter sw = new("wwwroot/exchangeusd.json"))
            {
                sw.WriteLine(response.Content);
            }

            //Deserializing data

            ExchangeModel dataFromJson = new();
            using (StreamReader sr = new("wwwroot/exchangeusd.json"))
            {
                string json = sr.ReadToEnd();
                dataFromJson = JsonSerializer.Deserialize<ExchangeModel>(json);
            }
            decimal rate;
            dataFromJson.quotes.TryGetValue(String.Concat(CurrencyFrom, CurrencyTo), out rate);
            Result = rate * Quantity;
            Result = Math.Round(Result, 2);

            return Redirect($"/result/{CurrencyFrom}/{Quantity}/{CurrencyTo}/{Result}");
        }
    }


    public class ListModel
    {
        [JsonPropertyName("success")]
        public bool success { get; init; }
        [JsonPropertyName("currencies")]
        public Dictionary<string, string> currencies { get; init; }
    }
    public class ExchangeModel
    {
        public bool success { get; init; }
        [JsonPropertyName("timestamp")]
        public long timestamp { get; init; }
        [JsonPropertyName("source")]
        public string source { get; init; }
        [JsonPropertyName("quotes")]
        public Dictionary<string, decimal> quotes { get; init; }
    }
}
