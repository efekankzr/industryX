using IndustryX.Domain.Entities;

namespace IndustryX.Application.Interfaces
{
    public interface IProductTransferService
    {
        Task<(bool Success, string? Error)> CreateTransferAsync(ProductTransfer transfer);
    }
}
