namespace Backend.Services
{
    public interface IExternalBankService
    {
        Task<bool> SendTransferAsync(string iban, decimal amount);
    }
}