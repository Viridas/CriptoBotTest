using System.Globalization;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;

namespace CryptoBot.Services
{
    public class BinanceAPIService
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://p2p.binance.com/bapi/c2c/v2/friendly/c2c/adv/search";

        public BinanceAPIService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private async Task<decimal> GetCryptoPriceAsync(string json)
        {
            try
            {
                var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), json);
                Console.WriteLine("Helo World");
                List<decimal> prices = new List<decimal>();
                using (var httpClient = new HttpClient())
                {
                    Console.WriteLine("Helo World2");
                    var jsonContent = await File.ReadAllTextAsync(jsonFilePath, Encoding.UTF8);
                    Console.WriteLine(jsonContent);

                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    Console.WriteLine(content);

                    var response = await httpClient.PostAsync(ApiUrl, content);
                    Console.WriteLine(response);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseStream = await response.Content.ReadAsStreamAsync();
                        using (var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress))
                        using (var streamReader = new StreamReader(decompressedStream))
                        {
                            var responseBody = await streamReader.ReadToEndAsync();
                            MyApiResponse responseData = JsonConvert.DeserializeObject<MyApiResponse>(responseBody);
                            for (int i = 0; i < 5; i++)
                            {
                                prices.Add(Convert.ToDecimal(responseData.Data[i].Adv.Price, CultureInfo.InvariantCulture));
                                Console.WriteLine($"{responseData.Data[i].Adv.Price}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Помилка: {response.StatusCode}");
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Відповідь API з помилкою: {errorContent}");
                    }
                }

                return await CountArithmeticMean(prices[0], prices[1], prices[2], prices[3], prices[4]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 0;
            }
        }
        public async Task<decimal> CountRightProcentPriceAsync(string buyPath, string sellPath, decimal proz)
        {
            var buyArithmeticMean = await GetCryptoPriceAsync(buyPath);
            var monoRate = await GetCryptoPriceAsync(sellPath);
            var price = buyArithmeticMean > monoRate ? buyArithmeticMean : monoRate;
            var sellArithmeticMean = ((price / 100) * proz) + monoRate;
            return Math.Truncate(sellArithmeticMean * 100) / 100;
        }

        public async Task<decimal> CountLeftProcentPriceAsync(string buyPath, string sellPath, decimal proz)
        {
            var buyArithmeticMean = await GetCryptoPriceAsync(buyPath);
            var rate = await GetCryptoPriceAsync(sellPath);
            var price = buyArithmeticMean > rate ? rate : buyArithmeticMean;
            var sellArithmeticMean = price - ((price / 100) * proz);

            return Math.Truncate(sellArithmeticMean * 100) / 100;
        }

        private async Task<decimal> CountArithmeticMean(decimal i1, decimal i2, decimal i3, decimal i4, decimal i5)
        {
            return (i1 + i2 + i3 + i4 + i5) / 5;
        }
    }
}