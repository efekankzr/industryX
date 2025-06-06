﻿using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IndustryX.Application.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductReceipt> _productReceiptRepository;
        private readonly IRepository<RawMaterial> _rawMaterialRepository;
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IRepository<ProductStock> _productStockRepository;

        public ProductService(
            IRepository<Product> productRepository,
            IRepository<ProductReceipt> productReceiptRepository,
            IRepository<RawMaterial> rawMaterialRepository,
            IRepository<Warehouse> warehouseRepository,
            IRepository<ProductStock> productStockRepository)
        {
            _productRepository = productRepository;
            _productReceiptRepository = productReceiptRepository;
            _rawMaterialRepository = rawMaterialRepository;
            _warehouseRepository = warehouseRepository;
            _productStockRepository = productStockRepository;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _productRepository.GetQueryable()
                .Include(p => p.ProductReceipts)
                    .ThenInclude(pr => pr.RawMaterial)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _productRepository.GetQueryable()
                .Include(p => p.ProductReceipts)
                    .ThenInclude(pr => pr.RawMaterial)
                .Include(p => p.ProductStocks)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(Product product)
        {
            if (await _productRepository.AnyAsync(p => p.Name.ToLower() == product.Name.ToLower()))
                return (false, "A product with the same name already exists.");

            if (await _productRepository.AnyAsync(p => p.Barcode == product.Barcode))
                return (false, "A product with the same barcode already exists.");

            var validReceipts = product.ProductReceipts
                .Where(r => r.Quantity > 0)
                .ToList();

            product.ProductReceipts = validReceipts;
            product.MaterialPrice = await CalculateMaterialPriceAsync(validReceipts);

            await _productRepository.AddAsync(product);
            await _productRepository.SaveAsync();

            await InitializeStocksForProduct(product.Id);

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(Product product)
        {
            var nameExists = await _productRepository.AnyAsync(p => p.Id != product.Id && p.Name.ToLower() == product.Name.ToLower());
            if (nameExists)
                return (false, "Another product with the same name already exists.");

            var barcodeExists = await _productRepository.AnyAsync(p => p.Id != product.Id && p.Barcode == product.Barcode);
            if (barcodeExists)
                return (false, "Another product with the same barcode already exists.");

            var existing = await _productRepository.GetQueryable()
                .Include(p => p.ProductReceipts)
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            if (existing == null)
                return (false, "Product not found.");

            existing.Name = product.Name;
            existing.Barcode = product.Barcode;
            existing.PiecesInBox = product.PiecesInBox;

            var incomingReceipts = product.ProductReceipts.Where(r => r.Include).ToList();
            var existingReceipts = existing.ProductReceipts.ToList();

            foreach (var incoming in incomingReceipts)
            {
                var match = existingReceipts.FirstOrDefault(e => e.RawMaterialId == incoming.RawMaterialId);
                if (match != null)
                {
                    match.Quantity = incoming.Quantity;
                }
                else
                {
                    existing.ProductReceipts.Add(new ProductReceipt
                    {
                        ProductId = product.Id,
                        RawMaterialId = incoming.RawMaterialId,
                        Quantity = incoming.Quantity
                    });
                }
            }

            foreach (var existingReceipt in existingReceipts)
            {
                if (!incomingReceipts.Any(i => i.RawMaterialId == existingReceipt.RawMaterialId))
                {
                    existing.ProductReceipts.Remove(existingReceipt);
                }
            }

            existing.MaterialPrice = await CalculateMaterialPriceAsync(existing.ProductReceipts);

            _productRepository.Update(existing);
            await _productRepository.SaveAsync();

            return (true, null);
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product != null)
            {
                _productRepository.Delete(product);
                await _productRepository.SaveAsync();
            }
        }

        private async Task<decimal> CalculateMaterialPriceAsync(ICollection<ProductReceipt> receipts)
        {
            var allMaterials = await _rawMaterialRepository.GetAllAsync();
            var materialDict = allMaterials.ToDictionary(x => x.Id, x => x.Price);

            return receipts.Sum(r =>
                materialDict.ContainsKey(r.RawMaterialId)
                    ? materialDict[r.RawMaterialId] * r.Quantity
                    : 0);
        }

        private async Task InitializeStocksForProduct(int productId)
        {
            var warehouses = await _warehouseRepository.GetAllAsync();

            foreach (var warehouse in warehouses)
            {
                var stock = new ProductStock
                {
                    ProductId = productId,
                    WarehouseId = warehouse.Id,
                    Stock = 0,
                    QruicalStock = 0,
                    Price = 0
                };

                await _productStockRepository.AddAsync(stock);
            }

            await _productStockRepository.SaveAsync();
        }

        public async Task<List<ProductStock>> GetCriticalStocksAsync()
        {
            return await _productStockRepository
                .GetQueryable()
                .Include(ps => ps.Product)
                .Include(ps => ps.Warehouse)
                .Where(ps => ps.Stock < ps.QruicalStock)
                .ToListAsync();
        }
    }
}
