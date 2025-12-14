using FPTMart.BLL.DTOs;
using FPTMart.DAL.Entities;
using FPTMart.DAL.Repositories;

namespace FPTMart.BLL.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;

    public CustomerService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
    {
        var customers = await _unitOfWork.Customers.GetAllAsync();
        return customers.Where(c => c.IsActive).Select(MapToDto);
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        return customer != null ? MapToDto(customer) : null;
    }

    public async Task<CustomerDto?> GetCustomerByPhoneAsync(string phone)
    {
        var customer = await _unitOfWork.Customers.GetByPhoneAsync(phone);
        return customer != null ? MapToDto(customer) : null;
    }

    public async Task<IEnumerable<CustomerDto>> SearchCustomersAsync(string keyword)
    {
        var customers = await _unitOfWork.Customers.SearchAsync(keyword);
        return customers.Select(MapToDto);
    }

    public async Task<CustomerDto> CreateCustomerAsync(CustomerDto dto)
    {
        var customer = new Customer
        {
            FullName = dto.FullName,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        dto.Id = customer.Id;
        return dto;
    }

    public async Task<CustomerDto> UpdateCustomerAsync(CustomerDto dto)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(dto.Id);
        if (customer == null)
            throw new Exception("Customer not found");

        customer.FullName = dto.FullName;
        customer.Phone = dto.Phone;
        customer.Email = dto.Email;
        customer.Address = dto.Address;
        customer.UpdatedAt = DateTime.Now;

        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync();

        return dto;
    }

    private static CustomerDto MapToDto(Customer c) => new()
    {
        Id = c.Id,
        FullName = c.FullName,
        Phone = c.Phone,
        Email = c.Email,
        Address = c.Address,
        TotalPurchases = c.TotalPurchases,
        IsActive = c.IsActive
    };
}

public class SupplierService : ISupplierService
{
    private readonly IUnitOfWork _unitOfWork;

    public SupplierService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync()
    {
        var suppliers = await _unitOfWork.Suppliers.GetAllAsync();
        return suppliers.Select(s => new SupplierDto
        {
            Id = s.Id,
            Name = s.Name,
            ContactPerson = s.ContactPerson,
            Phone = s.Phone
        });
    }

    public async Task<IEnumerable<SupplierDto>> GetActiveSuppliersAsync()
    {
        var suppliers = await _unitOfWork.Suppliers.GetActiveSuppliersAsync();
        return suppliers.Select(s => new SupplierDto
        {
            Id = s.Id,
            Name = s.Name,
            ContactPerson = s.ContactPerson,
            Phone = s.Phone
        });
    }

    public async Task<SupplierDto?> GetSupplierByIdAsync(int id)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
        if (supplier == null) return null;

        return new SupplierDto
        {
            Id = supplier.Id,
            Name = supplier.Name,
            ContactPerson = supplier.ContactPerson,
            Phone = supplier.Phone
        };
    }

    public async Task<SupplierDto> CreateSupplierAsync(SupplierDto dto)
    {
        var supplier = new Supplier
        {
            Name = dto.Name,
            ContactPerson = dto.ContactPerson,
            Phone = dto.Phone,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        await _unitOfWork.Suppliers.AddAsync(supplier);
        await _unitOfWork.SaveChangesAsync();

        dto.Id = supplier.Id;
        return dto;
    }

    public async Task<SupplierDto> UpdateSupplierAsync(SupplierDto dto)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(dto.Id);
        if (supplier == null)
            throw new Exception("Supplier not found");

        supplier.Name = dto.Name;
        supplier.ContactPerson = dto.ContactPerson;
        supplier.Phone = dto.Phone;
        supplier.UpdatedAt = DateTime.Now;

        _unitOfWork.Suppliers.Update(supplier);
        await _unitOfWork.SaveChangesAsync();

        return dto;
    }
}
