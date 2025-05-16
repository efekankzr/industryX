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

        public async Task<ProductTransfer?> GetByBarcodeAsync(string barcode)
        {
            return await _transferRepository
                .GetQueryable()
                .Include(t => t.Product)
                .Include(t => t.SourceWarehouse)
                .Include(t => t.DestinationWarehouse)
                .Include(t => t.InitiatedByUser)
                .Include(t => t.DeliveredByUser)
                .Include(t => t.ReceivedByUser)
                .FirstOrDefaultAsync(t => t.TransferBarcode == barcode);
        }

        public async Task<List<ProductTransfer>> GetAllCreatedTransfersAsync()
        {
            return await _transferRepository
                .GetQueryable()
                .Include(t => t.Product)
                .Include(t => t.SourceWarehouse)
                .Include(t => t.DestinationWarehouse)
                .Where(t => t.Status == TransferStatus.Created)
                .ToListAsync();
        }

        public async Task<List<ProductTransfer>> GetAllInTransitTransfersAsync()
        {
            return await _transferRepository
                .GetQueryable()
                .Include(t => t.Product)
                .Include(t => t.SourceWarehouse)
                .Include(t => t.DestinationWarehouse)
                .Where(t => t.Status == TransferStatus.InTransit)
                .ToListAsync();
        }


        public async Task<(ProductTransfer? Transfer, List<ProductTransferDeficit> Deficits)> GetDetailAsync(int transferId)
        {
            var transfer = await _transferRepository.GetQueryable()
                .Include(t => t.Product)
                .Include(t => t.SourceWarehouse)
                .Include(t => t.DestinationWarehouse)
                .Include(t => t.InitiatedByUser)
                .Include(t => t.DeliveredByUser)
                .Include(t => t.ReceivedByUser)
                .FirstOrDefaultAsync(t => t.Id == transferId);

            if (transfer == null)
                return (null, new List<ProductTransferDeficit>());

            var deficits = await _deficitRepository.GetQueryable()
                .Where(d => d.ProductTransferId == transferId)
                .Include(d => d.User)
                .ToListAsync();

            return (transfer, deficits);
        }

        public async Task<IEnumerable<ProductTransfer>> GetFilteredAsync(
            int? sourceWarehouseId,
            int? destinationWarehouseId,
            TransferStatus? status,
            DateTime? startDate,
            DateTime? endDate)
        {
            var query = _transferRepository.GetQueryable();

            query = query
                .Include(x => x.Product)
                .Include(x => x.SourceWarehouse)
                .Include(x => x.DestinationWarehouse)
                .Include(x => x.InitiatedByUser)
                .Include(x => x.DeliveredByUser)
                .Include(x => x.ReceivedByUser);

            if (sourceWarehouseId.HasValue)
                query = query.Where(x => x.SourceWarehouseId == sourceWarehouseId.Value);

            if (destinationWarehouseId.HasValue)
                query = query.Where(x => x.DestinationWarehouseId == destinationWarehouseId.Value);

            if (status.HasValue)
                query = query.Where(x => x.Status == status.Value);

            if (startDate.HasValue)
                query = query.Where(x => x.CreatedAt >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(x => x.CreatedAt <= endDate.Value.Date.AddDays(1).AddSeconds(-1));

            return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
        }

        public async Task<(bool Success, string? Error)> DirectTransferAsync(
            int sourceWarehouseId,
            int destinationWarehouseId,
            int productId,
            int quantityBox,
            string performedByUserId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return (false, "Product not found.");

            int quantityPieces = quantityBox * product.PiecesInBox;

            var sourceStock = await _stockRepository.GetQueryable()
                .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == sourceWarehouseId);

            if (sourceStock == null || sourceStock.Stock < quantityPieces)
                return (false, "Not enough stock in source warehouse.");

            var destStock = await _stockRepository.GetQueryable()
                .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == destinationWarehouseId);

            int destOldStock = destStock?.Stock ?? 0;
            decimal destOldPrice = destStock?.Price ?? 0;
            decimal sourceUnitPrice = sourceStock.Price;

            decimal transferValue = quantityPieces * sourceUnitPrice;
            decimal destStockValue = destOldStock * destOldPrice;
            int totalNewStock = destOldStock + quantityPieces;

            decimal newAveragePrice = totalNewStock > 0
                ? (destStockValue + transferValue) / totalNewStock
                : sourceUnitPrice;

            if (destStock == null)
            {
                destStock = new ProductStock
                {
                    ProductId = productId,
                    WarehouseId = destinationWarehouseId,
                    Stock = quantityPieces,
                    QruicalStock = 0,
                    Price = newAveragePrice
                };
                await _stockRepository.AddAsync(destStock);
            }
            else
            {
                destStock.Stock += quantityPieces;
                destStock.Price = newAveragePrice;
                _stockRepository.Update(destStock);
            }

            sourceStock.Stock -= quantityPieces;
            _stockRepository.Update(sourceStock);

            await _stockRepository.SaveAsync();

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> CreateAsync(int sourceWarehouseId, int destinationWarehouseId, int productId, int quantityBox, string initiatedByUserId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return (false, "Product not found.");

            var sourceStock = await _stockRepository.GetQueryable()
                .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == sourceWarehouseId);

            int quantityPieces = quantityBox * product.PiecesInBox;

            if (sourceStock == null || sourceStock.Stock < quantityPieces)
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

            sourceStock.Stock -= quantityPieces;
            _stockRepository.Update(sourceStock);

            var deficit = new ProductTransferDeficit
            {
                ProductId = productId,
                ProductTransfer = transfer,
                UserId = initiatedByUserId,
                DeficitQuantity = quantityPieces
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
            var transfer = await _transferRepository.GetQueryable()
                .Include(t => t.Product)
                .FirstOrDefaultAsync(t => t.TransferBarcode == barcode);

            if (transfer == null)
                return (false, "Transfer not found.");

            if (transfer.Status != TransferStatus.Created)
                return (false, "Transfer already in progress or completed.");

            var product = transfer.Product;
            int deliveredPieces = deliveredBoxCount * product.PiecesInBox;
            int totalPieces = transfer.TransferQuantityBox * product.PiecesInBox;
            int remainingPieces = totalPieces - deliveredPieces;

            var creatorDeficit = await _deficitRepository.GetQueryable()
                .FirstOrDefaultAsync(d => d.ProductTransferId == transfer.Id && d.UserId == transfer.InitiatedByUserId);

            if (creatorDeficit == null)
                return (false, "Original deficit not found.");

            if (remainingPieces > 0)
            {
                creatorDeficit.DeficitQuantity = remainingPieces;
                _deficitRepository.Update(creatorDeficit);
            }
            else
            {
                _deficitRepository.Delete(creatorDeficit);
            }

            var driverDeficit = new ProductTransferDeficit
            {
                ProductTransferId = transfer.Id,
                ProductId = transfer.ProductId,
                UserId = deliveredByUserId,
                DeficitQuantity = deliveredPieces
            };

            await _deficitRepository.AddAsync(driverDeficit);

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
            var transfer = await _transferRepository.GetQueryable()
                .Include(t => t.Product)
                .FirstOrDefaultAsync(t => t.TransferBarcode == barcode);

            if (transfer == null)
                return (false, "Transfer not found.");

            if (transfer.Status != TransferStatus.InTransit)
                return (false, "Transfer not in transit.");

            var product = transfer.Product;
            int receivedPieces = receivedBoxCount * product.PiecesInBox;

            var driverDeficit = await _deficitRepository.GetQueryable()
                .FirstOrDefaultAsync(d => d.ProductTransferId == transfer.Id && d.UserId == transfer.DeliveredByUserId);

            if (driverDeficit == null)
                return (false, "Driver's deficit not found.");

            var stock = await _stockRepository.GetQueryable()
                .FirstOrDefaultAsync(s => s.ProductId == product.Id && s.WarehouseId == transfer.DestinationWarehouseId);

            if (stock == null)
            {
                stock = new ProductStock
                {
                    ProductId = product.Id,
                    WarehouseId = transfer.DestinationWarehouseId,
                    Stock = 0,
                    QruicalStock = 0,
                    Price = product.MaterialPrice
                };
                await _stockRepository.AddAsync(stock);
            }

            // Fiyat güncelleme mantığı
            decimal existingValue = stock.Stock * stock.Price;
            decimal transferValue = receivedPieces * product.MaterialPrice;
            int newTotalStock = stock.Stock + receivedPieces;

            stock.Price = newTotalStock > 0
                ? (existingValue + transferValue) / newTotalStock
                : stock.Price;

            stock.Stock += receivedPieces;
            _stockRepository.Update(stock);

            int driverRemaining = driverDeficit.DeficitQuantity - receivedPieces;

            if (driverRemaining > 0)
            {
                driverDeficit.DeficitQuantity = driverRemaining;
                _deficitRepository.Update(driverDeficit);
            }
            else
            {
                _deficitRepository.Delete(driverDeficit);
            }

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
