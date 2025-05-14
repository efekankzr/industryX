using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IndustryX.Application.Services
{
    public class ProductTransferService : IProductTransferService
    {
        private readonly IRepository<ProductTransfer> _transferRepository;
        private readonly IRepository<ProductStock> _stockRepository;
        private readonly IRepository<ProductTransferDeficit> _deficitRepository;

        public ProductTransferService(
            IRepository<ProductTransfer> transferRepository,
            IRepository<ProductStock> stockRepository,
            IRepository<ProductTransferDeficit> deficitRepository)
        {
            _transferRepository = transferRepository;
            _stockRepository = stockRepository;
            _deficitRepository = deficitRepository;
        }

        public async Task<(bool Success, string? Error)> CreateTransferAsync(ProductTransfer transfer)
        {
            var stock = await _stockRepository.GetQueryable()
                .FirstOrDefaultAsync(s => s.ProductId == transfer.ProductId && s.WarehouseId == transfer.SourceWarehouseId);

            if (stock == null || stock.Stock < transfer.TransferQuantityBox)
            {
                return (false, "Not enough stock in source warehouse.");
            }

            stock.Stock -= transfer.TransferQuantityBox;
            _stockRepository.Update(stock);

            // Transfer kaydını oluştur
            await _transferRepository.AddAsync(transfer);

            // Kullanıcı zimmeti oluştur (source user için)
            var sourceZimmet = new ProductTransferDeficit
            {
                ProductId = transfer.ProductId,
                ProductTransferId = transfer.Id,
                UserId = transfer.InitiatedByUserId,
                DeficitQuantity = transfer.TransferQuantityBox
            };

            await _deficitRepository.AddAsync(sourceZimmet);

            await _stockRepository.SaveAsync();
            await _transferRepository.SaveAsync();
            await _deficitRepository.SaveAsync();

            return (true, null);
        }
    }

}
