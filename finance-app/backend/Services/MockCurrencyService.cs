namespace Backend.Services
{
    public class MockCurrencyService : ICurrencyService
    {
        private static readonly Dictionary<string, decimal> MockRates = new()
        {
            { "USD", 32.0m },
            { "EUR", 35.0m },
            { "TRY", 1.0m }
        };

        public Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency)
        {
            if (!MockRates.ContainsKey(fromCurrency) || !MockRates.ContainsKey(toCurrency))
                throw new Exception("Desteklenmeyen para birimi");
            var tryAmount = amount / MockRates[fromCurrency];
            var result = tryAmount * MockRates[toCurrency];
            return Task.FromResult(decimal.Round(result, 2));
        }
    }
}