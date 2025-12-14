using FPTMart.BLL.DTOs;
using FPTMart.DAL.Entities;
using FPTMart.DAL.Repositories;

namespace FPTMart.BLL.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _unitOfWork.Products.GetAllWithIncludesAsync(p => p.Category!);
        return products.Select(MapToDto);
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdWithIncludesAsync(id, p => p.Category!);
        return product != null ? MapToDto(product) : null;
    }

    public async Task<ProductDto?> GetProductByCodeAsync(string productCode)
    {
        var product = await _unitOfWork.Products.GetByProductCodeAsync(productCode);
        return product != null ? MapToDto(product) : null;
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByBarcodeAsync(string barcode)
    {
        var products = await _unitOfWork.Products.GetByBarcodeAsync(barcode);
        return products.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string keyword)
    {
        var products = await _unitOfWork.Products.SearchAsync(keyword);
        return products.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync()
    {
        var products = await _unitOfWork.Products.GetLowStockProductsAsync();
        return products.Select(MapToDto);
    }

    public async Task<ProductDto> CreateProductAsync(ProductDto dto)
    {
        // Validate ProductCode uniqueness
        var existingProduct = await _unitOfWork.Products.GetByProductCodeAsync(dto.ProductCode);
        if (existingProduct != null)
        {
            throw new Exception($"Mã sản phẩm '{dto.ProductCode}' đã tồn tại");
        }

        var product = new Product
        {
            ProductCode = dto.ProductCode,
            Barcode = dto.Barcode,
            Name = dto.Name,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            CostPrice = dto.CostPrice,
            SellingPrice = dto.SellingPrice,
            StockQuantity = dto.StockQuantity,
            MinStockLevel = dto.MinStockLevel,
            UnitsPerCase = dto.UnitsPerCase,
            CaseUnit = dto.CaseUnit,
            Unit = dto.Unit,
            ImagePath = dto.ImagePath,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        dto.Id = product.Id;
        return dto;
    }

    public async Task<ProductDto> UpdateProductAsync(ProductDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(dto.Id);
        if (product == null)
            throw new Exception("Không tìm thấy sản phẩm");

        // Validate ProductCode uniqueness (exclude current product)
        if (product.ProductCode != dto.ProductCode)
        {
            var existingProduct = await _unitOfWork.Products.GetByProductCodeAsync(dto.ProductCode);
            if (existingProduct != null && existingProduct.Id != dto.Id)
            {
                throw new Exception($"Mã sản phẩm '{dto.ProductCode}' đã được sử dụng bởi sản phẩm khác");
            }
        }

        product.ProductCode = dto.ProductCode;
        product.Barcode = dto.Barcode;
        product.Name = dto.Name;
        product.Description = dto.Description;
        product.CategoryId = dto.CategoryId;
        product.CostPrice = dto.CostPrice;
        product.SellingPrice = dto.SellingPrice;
        product.MinStockLevel = dto.MinStockLevel;
        product.UnitsPerCase = dto.UnitsPerCase;
        product.CaseUnit = dto.CaseUnit;
        product.Unit = dto.Unit;
        product.ImagePath = dto.ImagePath;
        product.IsActive = dto.IsActive;
        product.UpdatedAt = DateTime.Now;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        return dto;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null) return false;

        // Soft delete
        product.IsActive = false;
        product.UpdatedAt = DateTime.Now;
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<string> GenerateProductCodeAsync()
    {
        // Find the highest existing product code number
        var products = await _unitOfWork.Products.GetAllAsync();
        var maxNumber = 0;
        
        foreach (var p in products)
        {
            if (p.ProductCode.StartsWith("SP") && p.ProductCode.Length >= 4)
            {
                if (int.TryParse(p.ProductCode.Substring(2), out var num))
                {
                    if (num > maxNumber) maxNumber = num;
                }
            }
        }
        
        return $"SP{(maxNumber + 1):D4}";
    }

    private static ProductDto MapToDto(Product p) => new()
    {
        Id = p.Id,
        ProductCode = p.ProductCode,
        Barcode = p.Barcode,
        Name = p.Name,
        Description = p.Description,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.Name,
        CostPrice = p.CostPrice,
        SellingPrice = p.SellingPrice,
        StockQuantity = p.StockQuantity,
        MinStockLevel = p.MinStockLevel,
        UnitsPerCase = p.UnitsPerCase,
        CaseUnit = p.CaseUnit,
        Unit = p.Unit,
        ImagePath = p.ImagePath,
        IsActive = p.IsActive
    };
}
