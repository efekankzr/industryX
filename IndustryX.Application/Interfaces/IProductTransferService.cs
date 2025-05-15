using IndustryX.Domain.Entities;

namespace IndustryX.Application.Interfaces
{
    public interface IProductTransferService
    {
        Task<IEnumerable<ProductTransfer>> GetFilteredAsync(int? sourceWarehouseId, int? destinationWarehouseId, TransferStatus? status, DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<ProductTransfer>> GetAllAsync();
        Task<ProductTransfer?> GetByIdAsync(int id);
        Task<(bool Success, string? Error)> CreateAsync(int sourceWarehouseId,int destinationWarehouseId,int productId,int quantityBox,string initiatedByUserId);
        Task<ProductTransfer?> GetByBarcodeAsync(string barcode);
        Task<(bool Success, string? Error)> AcceptTransferAsync(string barcode, int deliveredBoxCount, string deliveredByUserId);
        Task<(bool Success, string? Error)> CompleteTransferAsync(string barcode, int receivedBoxCount, string receivedByUserId);
    }
}
