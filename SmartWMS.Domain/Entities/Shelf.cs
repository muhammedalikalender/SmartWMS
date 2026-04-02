namespace SmartWMS.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using SmartWMS.Domain.Common;
using SmartWMS.Domain.Events;
using SmartWMS.Domain.ValueObjects;

public class Shelf : AggregateRoot
{
    public string Code { get; private set; }

    private readonly List<WarehouseItem> _items = new();
    public IReadOnlyCollection<WarehouseItem> Items => _items.AsReadOnly();

    public StabilityIndex StabilityIndex { get; private set; }
    
    // Sistemdeki ideal (beklenen) kütle state'i. Sensör kütlesi ile bu property çakışırsa Phantom/Shrinkage anomalisidir.
    public Mass ExpectedMass { get; private set; } = Mass.Zero;

    // EF Core
    protected Shelf() { }

    // Rehydration constructor for replay/persistence
    public Shelf(Guid id, string code) : base(id)
    {
        Code = code;
        StabilityIndex = StabilityIndex.Stable;
    }

    public Shelf(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Raf kodu (Code) boş olamaz.", nameof(code));

        Id = Guid.NewGuid();
        Code = code;
        StabilityIndex = StabilityIndex.Stable;
    }

    public void AddItem(WarehouseItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        // Eğer ürün başka raftaysa önce o ilişkiden kopmalı
        // veya burada "RelocateTo" fonksiyonunu triggerlayarak ürüne "artık benimsin" diyoruz:
        if (item.ShelfId != this.Id)
            item.RelocateTo(this.Id);

        _items.Add(item);
        
        ExpectedMass += item.Mass;
        
        AddDomainEvent(new ItemAddedDomainEvent(this.Id, item.Id, item.Mass));
    }

    public void RemoveItem(WarehouseItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (_items.Remove(item))
        {
            ExpectedMass -= item.Mass;
            AddDomainEvent(new ItemRemovedDomainEvent(this.Id, item.Id, item.Mass));
        }
    }

    public void ApplySensorStability(StabilityIndex sensedIndex)
    {
        if (sensedIndex == null)
            throw new ArgumentNullException(nameof(sensedIndex));

        if (StabilityIndex.Value != sensedIndex.Value)
        {
            var oldStability = StabilityIndex;
            StabilityIndex = sensedIndex;
            AddDomainEvent(new ShelfStabilityChangedDomainEvent(this.Id, oldStability, sensedIndex));
        }
    }
}