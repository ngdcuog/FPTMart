using FPTMart.BLL.DTOs;
using FPTMart.DAL.Entities;
using FPTMart.DAL.Repositories;

namespace FPTMart.BLL.Services;

public class StockService : IStockService
{
    private readonly IUnitOfWork _unitOfWork;

    public StockService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<StockInDto>> GetAllStockInsAsync()
    {
        var stockIns = await _unitOfWork.StockIns.GetAllWithIncludesAsync(s => s.Supplier!, s => s.StockInItems);
        return stockIns.OrderByDescending(s => s.StockInDate).Select(MapToDto);
    }

    public async Task<StockInDto?> GetStockInByIdAsync(int id)
    {
        var stockIn = await _unitOfWork.StockIns.GetWithItemsAsync(id);
        return stockIn != null ? MapToDto(stockIn) : null;
    }

    public async Task<StockInDto> CreateStockInAsync(StockInDto dto)
    {
        var stockInNumber = await _unitOfWork.StockIns.GenerateStockInNumberAsync();

        var stockIn = new StockIn
        {
            StockInNumber = stockInNumber,
            SupplierId = dto.SupplierId,
            UserId = 1, // TODO: Get from current logged in user
            StockInDate = DateTime.Now,
            TotalAmount = dto.TotalAmount,
            Notes = dto.Notes,
            Status = "Completed",
            CreatedAt = DateTime.Now
        };

        // Add items and update product stock
        foreach (var itemDto in dto.Items)
        {
            var stockInItem = new StockInItem
            {
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                CostPrice = itemDto.CostPrice, // This is price per case
                TotalPrice = itemDto.TotalPrice
            };
            stockIn.StockInItems.Add(stockInItem);

            // Update product stock and cost price
            var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);
            if (product != null)
            {
                product.StockQuantity += itemDto.Quantity;
                
                // Convert case price to unit price for consistency with SellingPrice
                // CostPrice (per case) / UnitsPerCase = CostPrice (per unit)
                if (product.UnitsPerCase > 0)
                {
                    product.CostPrice = itemDto.CostPrice / product.UnitsPerCase;
                }
                else
                {
                    product.CostPrice = itemDto.CostPrice;
                }
                
                product.UpdatedAt = DateTime.Now;
                _unitOfWork.Products.Update(product);
            }
        }

        await _unitOfWork.StockIns.AddAsync(stockIn);
        await _unitOfWork.SaveChangesAsync();

        dto.Id = stockIn.Id;
        dto.StockInNumber = stockInNumber;
        return dto;
    }

    public async Task AdjustInventoryAsync(int productId, int quantityChange, string adjustmentType, string? reason, int userId)
    {
        var adjustment = new InventoryAdjustment
        {
            ProductId = productId,
            UserId = userId,
            AdjustmentType = adjustmentType,
            QuantityChange = quantityChange,
            Reason = reason,
            AdjustmentDate = DateTime.Now,
            CreatedAt = DateTime.Now
        };

        await _unitOfWork.InventoryAdjustments.AddAsync(adjustment);

        // Update product stock
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product != null)
        {
            product.StockQuantity += quantityChange;
            product.UpdatedAt = DateTime.Now;
            _unitOfWork.Products.Update(product);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private static StockInDto MapToDto(StockIn s) => new()
    {
        Id = s.Id,
        StockInNumber = s.StockInNumber,
        SupplierId = s.SupplierId,
        SupplierName = s.Supplier?.Name,
        StockInDate = s.StockInDate,
        TotalAmount = s.TotalAmount,
        Notes = s.Notes,
        Items = s.StockInItems.Select(si => new StockInItemDto
        {
            ProductId = si.ProductId,
            ProductCode = si.Product?.ProductCode ?? "",
            ProductName = si.Product?.Name ?? "",
            UnitsPerCase = si.Product?.UnitsPerCase ?? 1,
            CaseUnit = si.Product?.CaseUnit ?? "Thùng",
            Unit = si.Product?.Unit ?? "Cái",
            CaseQuantity = si.Product?.UnitsPerCase > 0 ? si.Quantity / (si.Product?.UnitsPerCase ?? 1) : si.Quantity,
            CostPrice = si.CostPrice
        }).ToList()
    };
}
