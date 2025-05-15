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
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductTransferDeficit> _deficitRepository;

        public ProductTransferService(
            IRepository<ProductTransfer> transferRepository,
            IRepository<ProductStock> stockRepository,
            IRepository<ProductTransferDeficit> deficitRepository,
            IRepository<Product> productRepository)
        {
            _transferRepository = transferRepository;
            _stockRepository = stockRepository;
            _deficitRepository = deficitRepository;
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<ProductTransfer>> GetAllAsync()
        {
            return await _transferRepository
                .GetQueryable()
                .Include(t => t.Product)
                .Include(t => t.SourceWarehouse)
                .Include(t => t.DestinationWarehouse)
                .Include(t => t.InitiatedByUser)
                .Include(t => t.DeliveredByUser)
                .Include(t => t.ReceivedByUser)
                .ToListAsync();
        }

        public async Task<ProductTransfer?> GetByIdAsync(int id)
        {
            return await _transferRepository
                .GetQueryable()
                .Include(t => t.Product)
                .Include(t => t.SourceWarehouse)
                .Include(t => t.DestinationWarehouse)
                .Include(t => t.InitiatedByUser)
                .Include(t => t.DeliveredByUser)
                .Include(t => t.ReceivedByUser)
                .FirstOrDefaultAsync(t => t.Id == id);
        }


        public async Task<(bool Success, string? Error)> CreateAsync(
            int sourceWarehouseId,
            int destinationWarehouseId,
            int productId,
            int quantityBox,
            string initiatedByUserId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return (false, "Product not found.");

            var sourceStock = await _stockRepository.GetQueryable()
                .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == sourceWarehouseId);

            if (sourceStock == null || sourceStock.Stock < quantityBox * product.PiecesInBox)
                return (false, "Not enough stock in source warehouse.");

            var barcode = $"TR-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

            var transfer = new ProductTransfer
            {
                TransferBarcode = barcode,
                SourceWarehouseId = sourceWarehouseId,
                DestinationWarehouseId = destinationWarehouseId,
                ProductId = productId,
                TransferQuantityBox = quantityBox,
                InitiatedByUserId = initiatedByUserId,
                CreatedAt = DateTime.Now,
                Status = TransferStatus.Created
            };

            // Depodan stoğu düş
            sourceStock.Stock -= quantityBox * product.PiecesInBox;
            _stockRepository.Update(sourceStock);

            // Zimmet kaydı oluştur
            var deficit = new ProductTransferDeficit
            {
                ProductId = productId,
                ProductTransfer = transfer,
                UserId = initiatedByUserId,
                DeficitQuantity = quantityBox * product.PiecesInBox
            };

            await _transferRepository.AddAsync(transfer);
            await _deficitRepository.AddAsync(deficit);
            await _stockRepository.SaveAsync();
            await _deficitRepository.SaveAsync();
            await _transferRepository.SaveAsync();

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> AcceptTransferAsync(string barcode, int deliveredBoxCount, string deliveredByUserId)
        {
            var transfer = await _transferRepository
                .GetQueryable()
                .Include(t => t.Product)
                .Include(t => t.InitiatedByUser)
                .Include(t => t.DestinationWarehouse)
                .Include(t => t.SourceWarehouse)
                .FirstOrDefaultAsync(t => t.TransferBarcode == barcode);

            if (transfer == null)
                return (false, "Transfer not found.");

            if (transfer.Status != TransferStatus.Created)
                return (false, "Transfer is already in progress or completed.");

            if (deliveredBoxCount <= 0 || deliveredBoxCount > transfer.TransferQuantityBox)
                return (false, "Invalid box quantity.");

            var deliveredPieces = deliveredBoxCount * transfer.Product.PiecesInBox;
            var totalPieces = transfer.TransferQuantityBox * transfer.Product.PiecesInBox;
            var deficitPieces = totalPieces - deliveredPieces;

            var existingDeficit = await _deficitRepository.GetQueryable()
                .FirstOrDefaultAsync(d => d.ProductTransferId == transfer.Id && d.UserId == transfer.InitiatedByUserId);

            if (existingDeficit == null)
                return (false, "Original user deficit record not found.");

            // Zimmeti yeni kullanıcıya devret
            var newDeficit = new ProductTransferDeficit
            {
                ProductTransferId = transfer.Id,
                ProductId = transfer.ProductId,
                UserId = deliveredByUserId,
                DeficitQuantity = deliveredPieces
            };
            await _deficitRepository.AddAsync(newDeficit);

            // Kalan zimmeti eski kullanıcıda bırak
            if (deficitPieces > 0)
            {
                existingDeficit.DeficitQuantity = deficitPieces;
                _deficitRepository.Update(existingDeficit);
            }
            else
            {
                _deficitRepository.Delete(existingDeficit);
            }

            // Transfer güncelle
            transfer.DeliveredByUserId = deliveredByUserId;
            transfer.PickedUpAt = DateTime.Now;
            transfer.Status = TransferStatus.InTransit;

            _transferRepository.Update(transfer);
            await _deficitRepository.SaveAsync();
            await _transferRepository.SaveAsync();

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> CompleteTransferAsync(string barcode, int receivedBoxCount, string receivedByUserId)
        {
            var transfer = await _transferRepository
                .GetQueryable()
                .Include(t => t.Product)
                .Include(t => t.DestinationWarehouse)
                .FirstOrDefaultAsync(t => t.TransferBarcode == barcode);

            if (transfer == null)
                return (false, "Transfer not found.");

            if (transfer.Status != TransferStatus.InTransit)
                return (false, "Transfer is not in transit or has already been completed.");

            if (receivedBoxCount <= 0 || receivedBoxCount > transfer.TransferQuantityBox)
                return (false, "Invalid received box quantity.");

            var receivedPieces = receivedBoxCount * transfer.Product.PiecesInBox;
            var totalPieces = transfer.TransferQuantityBox * transfer.Product.PiecesInBox;
            var deficitPieces = totalPieces - receivedPieces;

            // Taşıyıcının zimmetini al
            var carrierDeficit = await _deficitRepository.GetQueryable()
                .FirstOrDefaultAsync(d => d.ProductTransferId == transfer.Id && d.UserId == transfer.DeliveredByUserId);

            if (carrierDeficit == null)
                return (false, "Carrier deficit record not found.");

            // Teslim alan kişi için yeni zimmet oluştur
            if (receivedPieces > 0)
            {
                // Depo stoğunu arttır
                var stock = await _stockRepository.GetQueryable()
                    .FirstOrDefaultAsync(s => s.ProductId == transfer.ProductId && s.WarehouseId == transfer.DestinationWarehouseId);

                if (stock == null)
                {
                    stock = new ProductStock
                    {
                        ProductId = transfer.ProductId,
                        WarehouseId = transfer.DestinationWarehouseId,
                        Stock = 0,
                        QruicalStock = 0,
                        Price = transfer.Product.MaterialPrice
                    };
                    await _stockRepository.AddAsync(stock);
                }

                stock.Stock += receivedPieces;
                _stockRepository.Update(stock);
            }

            // Kalan zimmet taşıyıcıda kalsın
            if (deficitPieces > 0)
            {
                carrierDeficit.DeficitQuantity = deficitPieces;
                _deficitRepository.Update(carrierDeficit);
            }
            else
            {
                _deficitRepository.Delete(carrierDeficit);
            }

            // Transferi tamamla
            transfer.ReceivedByUserId = receivedByUserId;
            transfer.DeliveredAt = DateTime.Now;
            transfer.Status = TransferStatus.Delivered;

            _transferRepository.Update(transfer);

            await _stockRepository.SaveAsync();
            await _deficitRepository.SaveAsync();
            await _transferRepository.SaveAsync();

            return (true, null);
        }
    }
}
