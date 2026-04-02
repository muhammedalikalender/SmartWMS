namespace SmartWMS.Domain.Entities;

using System;
using System;
using SmartWMS.Domain.Common;
using SmartWMS.Domain.ValueObjects;

public class WarehouseItem : Entity
{
    public string Sku { get; private set; }
    public string Name { get; private set; }

    public Mass Mass { get; private set; }
    public Guid ShelfId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    // EF Core / ORM requirement
    protected WarehouseItem() { }

    public WarehouseItem(string sku, string name, Mass mass, Guid shelfId)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU boş olamaz.", nameof(sku));
            
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Ürün ismi boş olamaz.", nameof(name));
            
        if (mass == null)
            throw new ArgumentNullException(nameof(mass));
            
        if (shelfId == Guid.Empty)
            throw new ArgumentException("Geçerli bir Shelf ID gereklidir.", nameof(shelfId));

        Id = Guid.NewGuid();
        Sku = sku;
        Name = name;
        Mass = mass;
        ShelfId = shelfId;
        CreatedAt = DateTime.UtcNow;
    }

    // Item another shelf'e taşınırsa
    internal void RelocateTo(Guid newShelfId)
    {
        if (newShelfId == Guid.Empty)
            throw new ArgumentException("Geçerli bir RAF ID'si girilmelidir.", nameof(newShelfId));

        ShelfId = newShelfId;
    }
}