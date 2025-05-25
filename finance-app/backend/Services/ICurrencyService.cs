namespace Backend.Services
{
    public interface ICurrencyService
    {
        Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency);
    }
}