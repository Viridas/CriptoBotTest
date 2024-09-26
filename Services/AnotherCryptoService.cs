using System.IO.Compression;
using HtmlAgilityPack;

namespace CryptoBot.Services
{
    public class AnotherCryptoService
    {
        private readonly HttpClient _httpClient;

        public AnotherCryptoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetBitcoinPrice()
        {
            var searchUrl = $"https://www.binance.com/ru-UA/price/bitcoin";
            var htmlDoc = new HtmlDocument();

            var response = await _httpClient.GetAsync(searchUrl);
            response.EnsureSuccessStatusCode();

            var responseStream = await response.Content.ReadAsStreamAsync();
            using (var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress))
            using (var streamReader = new StreamReader(decompressedStream))
            {
                var responseBody = await streamReader.ReadToEndAsync();
                htmlDoc.LoadHtml(responseBody);
            }

            string price = htmlDoc.DocumentNode.SelectSingleNode("/html/body/div[3]/section/div/div[2]/div[4]/div[1]/div[1]").InnerText.Trim();

            return price;
        }

        public async Task<string> GetEthereumPrice()
        {
            var searchUrl = $"https://www.binance.com/ru-UA/price/ethereum";
            var htmlDoc = new HtmlDocument();

            var response = await _httpClient.GetAsync(searchUrl);
            response.EnsureSuccessStatusCode();

            var responseStream = await response.Content.ReadAsStreamAsync();
            using (var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress))
            using (var streamReader = new StreamReader(decompressedStream))
            {
                var responseBody = await streamReader.ReadToEndAsync();
                htmlDoc.LoadHtml(responseBody);
            }
            
            string price = htmlDoc.DocumentNode.SelectSingleNode("/html/body/div[3]/section/div/div[2]/div[4]/div[1]/div[1]").InnerText.Trim();

            return price;
        }
    }
}