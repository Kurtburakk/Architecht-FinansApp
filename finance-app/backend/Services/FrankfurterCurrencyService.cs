using System.Text.Json;

namespace Backend.Services
{
    public class FrankfurterCurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;

        public FrankfurterCurrencyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> ConvertAsync(decimal amount, string from, string to)
        {
            from = from.ToUpper();
            to = to.ToUpper();

            // Eğer from == to ise direkt geri döndür
            if (from == to)
                return amount;

            // Sadece EUR tabanlı çevirim destekleniyor
            // 1. EUR -> X veya X -> EUR veya EUR <-> USD/TRY gibi
            // 2. X -> Y için önce X'i EUR'ya, sonra EUR'dan Y'ye çevirmek gerek

            if (from == "EUR")
            {
                // EUR'dan başka para birimine çevir
                var url = $"https://api.frankfurter.app/latest?from=EUR&to={to}";
                var res = await _httpClient.GetAsync(url);
                res.EnsureSuccessStatusCode();
                var json = await res.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                var rates = doc.RootElement.GetProperty("rates");
                if (!rates.TryGetProperty(to, out var rateElem))
                    throw new Exception($"Kur bulunamadı: EUR -> {to}");

                var rate = rateElem.GetDecimal();
                return amount * rate;
            }
            else if (to == "EUR")
            {
                // X'ten EUR'ya çevirmek için oranı bul, böl
                var url = $"https://api.frankfurter.app/latest?from=EUR&to={from}";
                var res = await _httpClient.GetAsync(url);
                res.EnsureSuccessStatusCode();
                var json = await res.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                var rates = doc.RootElement.GetProperty("rates");
                if (!rates.TryGetProperty(from, out var rateElem))
                    throw new Exception($"Kur bulunamadı: {from} -> EUR");

                var rate = rateElem.GetDecimal();
                return amount / rate;
            }
            else
            {
                // X'ten Y'ye doğrudan çevirim yok, önce EUR'ya sonra hedefe çevir
                // 1. X -> EUR
                var url1 = $"https://api.frankfurter.app/latest?from=EUR&to={from}";
                var res1 = await _httpClient.GetAsync(url1);
                res1.EnsureSuccessStatusCode();
                var json1 = await res1.Content.ReadAsStringAsync();
                using var doc1 = JsonDocument.Parse(json1);

                var rates1 = doc1.RootElement.GetProperty("rates");
                if (!rates1.TryGetProperty(from, out var rateFromElem))
                    throw new Exception($"Kur bulunamadı: {from} -> EUR");

                var rateFrom = rateFromElem.GetDecimal();
                var amountInEur = amount / rateFrom;

                // 2. EUR -> Y
                var url2 = $"https://api.frankfurter.app/latest?from=EUR&to={to}";
                var res2 = await _httpClient.GetAsync(url2);
                res2.EnsureSuccessStatusCode();
                var json2 = await res2.Content.ReadAsStringAsync();
                using var doc2 = JsonDocument.Parse(json2);

                var rates2 = doc2.RootElement.GetProperty("rates");
                if (!rates2.TryGetProperty(to, out var rateToElem))
                    throw new Exception($"Kur bulunamadı: EUR -> {to}");

                var rateTo = rateToElem.GetDecimal();
                return amountInEur * rateTo;
            }
        }
    }
}