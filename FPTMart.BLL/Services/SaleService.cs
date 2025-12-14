using FPTMart.BLL.DTOs;
using FPTMart.DAL.Entities;
using FPTMart.DAL.Repositories;

namespace FPTMart.BLL.Services;

public class SaleService : ISaleService
{
    private readonly IUnitOfWork _unitOfWork;

    public SaleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<SaleDto>> GetAllSalesAsync()
    {
        var sales = await _unitOfWork.Sales.GetAllWithIncludesAsync(s => s.Customer!, s => s.SaleItems);
        return sales.OrderByDescending(s => s.SaleDate).Select(MapToDto);
    }

    public async Task<SaleDto?> GetSaleByIdAsync(int id)
    {
        var sale = await _unitOfWork.Sales.GetWithItemsAsync(id);
        return sale != null ? MapToDto(sale) : null;
    }

    public async Task<SaleDto?> GetSaleByInvoiceNumberAsync(string invoiceNumber)
    {
        var sale = await _unitOfWork.Sales.GetByInvoiceNumberAsync(invoiceNumber);
        if (sale == null) return null;
        return await GetSaleByIdAsync(sale.Id);
    }

    public async Task<IEnumerable<SaleDto>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var sales = await _unitOfWork.Sales.GetByDateRangeAsync(startDate, endDate);
        return sales.Select(MapToDto);
    }

    public async Task<SaleDto> CreateSaleAsync(SaleDto dto)
    {
        // Generate invoice number
        var invoiceNumber = await _unitOfWork.Sales.GenerateInvoiceNumberAsync();

        var sale = new Sale
        {
            InvoiceNumber = invoiceNumber,
            CustomerId = dto.CustomerId,
            UserId = 1, // TODO: Get from current logged in user
            SaleDate = DateTime.Now,
            SubTotal = dto.SubTotal,
            DiscountAmount = dto.DiscountAmount,
            TotalAmount = dto.TotalAmount,
            PaidAmount = dto.PaidAmount,
            ChangeAmount = dto.ChangeAmount,
            PaymentMethod = dto.PaymentMethod,
            Status = "Completed",
            CreatedAt = DateTime.Now
        };

        // Add sale items
        foreach (var itemDto in dto.Items)
        {
            var saleItem = new SaleItem
            {
                ProductId = itemDto.ProductId,
                ProductCode = itemDto.ProductCode,
                ProductName = itemDto.ProductName,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                DiscountAmount = itemDto.DiscountAmount,
                TotalPrice = itemDto.TotalPrice
            };
            sale.SaleItems.Add(saleItem);

            // Update product stock
            var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);
            if (product != null)
            {
                product.StockQuantity -= itemDto.Quantity;
                _unitOfWork.Products.Update(product);
            }
        }

        // Update customer total purchases
        if (dto.CustomerId.HasValue)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId.Value);
            if (customer != null)
            {
                customer.TotalPurchases += dto.TotalAmount;
                _unitOfWork.Customers.Update(customer);
            }
        }

        await _unitOfWork.Sales.AddAsync(sale);
        await _unitOfWork.SaveChangesAsync();

        dto.Id = sale.Id;
        dto.InvoiceNumber = invoiceNumber;
        dto.SaleDate = sale.SaleDate; // Set correct sale date
        return dto;
    }

    public async Task<bool> CancelSaleAsync(int id)
    {
        var sale = await _unitOfWork.Sales.GetWithItemsAsync(id);
        if (sale == null) return false;

        // Restore stock for each item
        foreach (var item in sale.SaleItems)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
            if (product != null)
            {
                product.StockQuantity += item.Quantity;
                _unitOfWork.Products.Update(product);
            }
        }

        // Restore customer total purchases
        if (sale.CustomerId.HasValue)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(sale.CustomerId.Value);
            if (customer != null)
            {
                customer.TotalPurchases -= sale.TotalAmount;
                _unitOfWork.Customers.Update(customer);
            }
        }

        sale.Status = "Cancelled";
        _unitOfWork.Sales.Update(sale);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<decimal> GetTodayRevenueAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var sales = await _unitOfWork.Sales.GetByDateRangeAsync(today, tomorrow);
        return sales.Where(s => s.Status == "Completed").Sum(s => s.TotalAmount);
    }

    public async Task<int> GetTodaySalesCountAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var sales = await _unitOfWork.Sales.GetByDateRangeAsync(today, tomorrow);
        return sales.Count(s => s.Status == "Completed");
    }

    private static SaleDto MapToDto(Sale s) => new()
    {
        Id = s.Id,
        InvoiceNumber = s.InvoiceNumber,
        CustomerId = s.CustomerId,
        CustomerName = s.Customer?.FullName,
        SaleDate = s.SaleDate,
        SubTotal = s.SubTotal,
        DiscountAmount = s.DiscountAmount,
        TotalAmount = s.TotalAmount,
        PaidAmount = s.PaidAmount,
        ChangeAmount = s.ChangeAmount,
        PaymentMethod = s.PaymentMethod,
        Status = s.Status,
        Items = s.SaleItems.Select(si => new SaleItemDto
        {
            ProductId = si.ProductId,
            ProductCode = si.ProductCode,
            ProductName = si.ProductName,
            Quantity = si.Quantity,
            UnitPrice = si.UnitPrice,
            DiscountAmount = si.DiscountAmount,
            TotalPrice = si.TotalPrice
        }).ToList()
    };
}
