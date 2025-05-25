using IndustryX.Domain.Entities;

namespace IndustryX.Application.Interfaces
{
    public interface ISalesProductService
    {
        Task<List<SalesProduct>> GetAllAsync();
        Task<SalesProduct?> GetByIdAsync(int id);
        Task<(bool Success, string? Error)> CreateAsync(SalesProduct salesProduct);
        Task<(bool Success, string? Error)> UpdateAsync(SalesProduct salesProduct);
        Task<bool> DeleteAsync(int id);
        Task<List<SalesProduct>> GetActiveListAsync();
        Task<SalesProduct?> GetByUrlAsync(string url);
        Task InitializeSalesProductStocksAsync(int salesProductId);
        Task<List<SalesProduct>> GetBestSellersAsync();
        Task<List<SalesProduct>> GetPopularAsync();
    }
}
