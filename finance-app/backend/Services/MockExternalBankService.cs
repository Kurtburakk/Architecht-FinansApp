namespace Backend.Services
{
    public class MockExternalBankService : IExternalBankService
    {
        public Task<bool> SendTransferAsync(string iban, decimal amount)
        {
            // Mock mantık: 10 TL üstündeki transferler başarılı, altında başarısız örn.
            return Task.FromResult(amount >= 10);
        }
    }
}