using System.Linq.Expressions;
using IndustryX.Domain.Entities;

namespace IndustryX.Application.Interfaces
{
    public interface IProductTransferService
    {
        Task<IEnumerable<ProductTransfer>> GetFilteredAsync(int? sourceWarehouseId, int? destinationWarehouseId, TransferStatus? status, DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<ProductTransfer>> GetAllAsync();
        Task<List<ProductTransfer>> GetAllAsync(Expression<Func<ProductTransfer, bool>> predicate);
        Task<List<ProductTransferDeficit>> GetDeliveredTransfersWithUserDeficitsAsync(string userId);
        Task<ProductTransfer?> GetByIdAsync(int id);
        Task<(ProductTransfer? Transfer, List<ProductTransferDeficit> Deficits)> GetDetailAsync(int transferId);
        Task<(bool Success, string? Error)> DirectTransferAsync(int sourceWarehouseId, int destinationWarehouseId, int productId, int quantityBox, string performedByUserId);
        Task<(bool Success, string? Error)> CreateAsync(int sourceWarehouseId, int destinationWarehouseId, int productId, int quantityBox, string initiatedByUserId);
        Task<ProductTransfer?> GetByBarcodeAsync(string barcode);
        Task<List<ProductTransfer>> GetAllCreatedTransfersAsync();
        Task<List<ProductTransfer>> GetAllInTransitTransfersAsync();
        Task<(bool Success, string? Error)> AcceptTransferAsync(string barcode, int deliveredBoxCount, string deliveredByUserId);
        Task<(bool Success, string? Error)> CompleteTransferAsync(string barcode, int receivedBoxCount, string receivedByUserId);
    }
}
