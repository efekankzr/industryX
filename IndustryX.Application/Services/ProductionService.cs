using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.Domain.Interfaces;
using IndustryX.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace IndustryX.Application.Services
{
    public class ProductionService : IProductionService
    {
        private readonly IRepository<Production> _productionRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductReceipt> _receiptRepository;
        private readonly IRepository<ProductStock> _productStockRepository;
        private readonly IRepository<RawMaterialStock> _materialStockRepository;
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IRepository<LaborCost> _laborCostRepository;
        private readonly IEmailService _emailService;

        public ProductionService(
            IRepository<Production> productionRepository,
            IRepository<Product> productRepository,
            IRepository<ProductReceipt> receiptRepository,
            IRepository<ProductStock> productStockRepository,
            IRepository<RawMaterialStock> materialStockRepository,
            IRepository<Warehouse> warehouseRepository,
            IRepository<LaborCost> laborCostRepository, 
            IEmailService emailService)
        {
            _productionRepository = productionRepository;
            _productRepository = productRepository;
            _receiptRepository = receiptRepository;
            _productStockRepository = productStockRepository;
            _materialStockRepository = materialStockRepository;
            _warehouseRepository = warehouseRepository;
            _laborCostRepository = laborCostRepository;
            _emailService = emailService;
        }


        public async Task<IEnumerable<Production>> GetAllAsync()
            => await _productionRepository
                .GetQueryable()
                .Include(p => p.Product)
                .Include(p => p.Pauses)
                .ToListAsync();

        public async Task<Production?> GetByIdAsync(int id)
            => await _productionRepository
                .GetQueryable()
                .Include(p => p.Product)
                .Include(p => p.Pauses)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<(bool Success, string? Error)> CreateAsync(Production production)
        {
            var product = await _productRepository
                .GetQueryable()
                .Include(p => p.ProductReceipts)
                .ThenInclude(r => r.RawMaterial)
                .FirstOrDefaultAsync(p => p.Id == production.ProductId);

            if (product == null)
                return (false, "Product not found.");

            if (production.BoxQuantity <= 0 || production.WorkerCount <= 0)
                return (false, "Box quantity and worker count must be greater than zero.");

            var mainProdWarehouse = (await _warehouseRepository.GetAllAsync())
                .FirstOrDefault(w => w.IsMainForProduct);

            if (mainProdWarehouse == null)
                return (false, "Main warehouse for products not found.");

            var mainRawWarehouse = (await _warehouseRepository.GetAllAsync())
                .FirstOrDefault(w => w.IsMainForRawMaterial);

            if (mainRawWarehouse == null)
                return (false, "Main warehouse for raw materials not found.");

            var totalPieces = production.BoxQuantity * product.PiecesInBox;

            foreach (var receipt in product.ProductReceipts)
            {
                var totalNeeded = receipt.Quantity * totalPieces;

                var stock = await _materialStockRepository.GetQueryable()
                    .FirstOrDefaultAsync(s =>
                        s.RawMaterialId == receipt.RawMaterialId &&
                        s.WarehouseId == mainRawWarehouse.Id);

                if (stock == null || stock.Stock < totalNeeded)
                {
                    var name = receipt.RawMaterial?.Name ?? "Unknown Material";
                    return (false, $"Not enough stock for raw material: {name}");
                }
            }

            var lowStockMaterials = new List<string>();

            foreach (var receipt in product.ProductReceipts)
            {
                var totalNeeded = receipt.Quantity * totalPieces;

                var stock = await _materialStockRepository.GetQueryable()
                    .FirstOrDefaultAsync(s =>
                        s.RawMaterialId == receipt.RawMaterialId &&
                        s.WarehouseId == mainRawWarehouse.Id);

                if (stock != null)
                {
                    stock.Stock -= totalNeeded;
                    _materialStockRepository.Update(stock);

                    if (stock.Stock < stock.QruicalStock)
                    {
                        var name = receipt.RawMaterial?.Name ?? $"RawMaterial #{receipt.RawMaterialId}";
                        lowStockMaterials.Add($"{name} (Remaining: {stock.Stock:0.###}, Critical: {stock.QruicalStock:0.###})");
                    }
                }
            }

            await _materialStockRepository.SaveAsync();

            if (lowStockMaterials.Any())
            {
                var body = $@"
            <p>The following raw materials fell below their critical stock levels after production:</p>
            <ul>{string.Join("", lowStockMaterials.Select(m => $"<li>{m}</li>"))}</ul>
            <p>Please take necessary actions.</p>";

                await _emailService.SendEmailAsync(
                    toEmail: "kiziroglu29@outlook.com.tr",
                    subject: "🔔 Critical Raw Material Stock Alert",
                    body: body
                );
            }

            await _productionRepository.AddAsync(production);
            await _productionRepository.SaveAsync();

            return (true, null);
        }

        public async Task<bool> StartProductionAsync(int id)
        {
            var production = await GetByIdAsync(id);
            if (production == null || production.Status != ProductionStatus.Planned)
                return false;

            production.StartTime = DateTime.Now;
            production.Status = ProductionStatus.InProgress;

            _productionRepository.Update(production);
            await _productionRepository.SaveAsync();
            return true;
        }

        public async Task<bool> PauseProductionAsync(int id)
        {
            var production = await GetByIdAsync(id);
            if (production == null || production.Status != ProductionStatus.InProgress)
                return false;

            var pause = new ProductionPause
            {
                ProductionId = id,
                PausedAt = DateTime.Now
            };

            production.Status = ProductionStatus.Paused;
            production.Pauses.Add(pause);

            _productionRepository.Update(production);
            await _productionRepository.SaveAsync();
            return true;
        }

        public async Task<bool> ResumeProductionAsync(int id)
        {
            var production = await GetByIdAsync(id);
            if (production == null || production.Status != ProductionStatus.Paused)
                return false;

            var lastPause = production.Pauses.LastOrDefault(p => p.ResumedAt == default);
            if (lastPause == null)
                return false;

            lastPause.ResumedAt = DateTime.Now;
            lastPause.Duration = (decimal)(lastPause.ResumedAt - lastPause.PausedAt).TotalMinutes;

            production.Status = ProductionStatus.InProgress;

            _productionRepository.Update(production);
            await _productionRepository.SaveAsync();
            return true;
        }

        public async Task<bool> FinishProductionAsync(int id)
        {
            var production = await GetByIdAsync(id);
            if (production == null || production.Status != ProductionStatus.InProgress || !production.StartTime.HasValue)
                return false;

            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                production.EndTime = DateTime.Now;
                production.Status = ProductionStatus.Completed;

                // TotalTime ve BreakOutTime milisaniye cinsinden
                long totalTimeMillis = (long)(production.EndTime.Value - production.StartTime.Value).TotalMilliseconds;
                long breakMillis = (long)production.Pauses.Sum(p => p.Duration * 60000); // her pause süresi dakika, milise çevir

                production.BreakOutTime = breakMillis;
                production.TotalTime = totalTimeMillis;

                var product = production.Product;
                var stock = await GetMainProductStock(production.ProductId);

                if (product != null && stock != null)
                {
                    int producedUnits = production.BoxQuantity * product.PiecesInBox;

                    decimal totalRawMaterialCost = producedUnits * product.MaterialPrice;

                    var laborCost = (await _laborCostRepository.GetQueryable()
                        .Where(l => l.EffectiveDate <= production.CreatedAt)
                        .OrderByDescending(l => l.EffectiveDate)
                        .FirstOrDefaultAsync())?.HourlyWage ?? 0;

                    decimal totalLaborCost = ((totalTimeMillis - breakMillis) * production.WorkerCount * laborCost) / 3600000m;

                    decimal totalCost = totalRawMaterialCost + totalLaborCost;

                    decimal existingValue = stock.Stock * stock.Price;
                    int totalQty = stock.Stock + producedUnits;

                    stock.Price = totalQty > 0 ? (existingValue + totalCost) / totalQty : stock.Price;
                    stock.Stock += producedUnits;

                    _productStockRepository.Update(stock);
                    await _productStockRepository.SaveAsync();
                }

                _productionRepository.Update(production);
                await _productionRepository.SaveAsync();

                transaction.Complete();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<ProductStock?> GetMainProductStock(int productId)
        {
            var warehouse = (await _warehouseRepository.GetAllAsync()).FirstOrDefault(w => w.IsMainForProduct);
            return warehouse == null
                ? null
                : await _productStockRepository
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == warehouse.Id);
        }
    }
}
