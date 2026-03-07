# Data Consistency & Integrity Analysis - Module 2 Expendable

## Executive Summary

**Status**: ⚠️ **CRITICAL ISSUES FOUND** - The system has significant data consistency gaps in the flow from purchase receipt through employee inventory to issuance.

---

## Issues Found

### 🔴 CRITICAL ISSUE #1: InventoryItem Merges Different Purchase Prices

**Location**: EmployeeInventory.InventoryItem

**Problem**: 
The `InventoryItem` class aggregates quantities by `ProductId` ONLY, merging items from DIFFERENT PURCHASES at different prices:
```csharp
public class InventoryItem : ValueObject
{
    public Guid ProductId { get; private set; }
    public int CurrentQuantity { get; private set; }
    public decimal UnitPrice { get; private set; } // ← SINGLE price, but from different purchases!
    public DateTime ReceivedDate { get; private set; }
}
```

**Data Flow Issue**:
1. Purchase 1 (PO-2026-001): Product A @ $10/unit, 50 units → Issued to employee
2. Purchase 2 (PO-2026-002): Product A @ $12/unit, 30 units → Issued to employee  
3. Both added to same InventoryItem
4. InventoryItem stores ONE price - which purchase? Last one wins!
5. Loss of cost tracking and audit trail

**Example Scenario**:
```
Purchase 1 (PO-2026-001): Product A, 50 units @ $10/unit = $500
Purchase 2 (PO-2026-002): Product A, 30 units @ $12/unit = $360
Total Cost Paid: $860

When both issued to employee, InventoryItem stores:
- ProductId: ProductA-ID
- CurrentQuantity: 80 units
- UnitPrice: $12 (from PO-2026-002) OR $10 (from PO-2026-001)? ← AMBIGUOUS!
- ReceivedDate: 2026-03-01 OR 2026-03-05? ← AMBIGUOUS!
- PurchaseId: MISSING! ← Lost link to source

Result: Cannot determine actual cost of 80 units in employee inventory
```

**Impact**:
- ❌ Cost accounting is inaccurate
- ❌ Cannot determine actual cost of employee's inventory
- ❌ Audit trail broken for cost purposes
- ❌ Cannot support FIFO/LIFO valuation methods

---

### 🔴 CRITICAL ISSUE #2: EmployeeInventory.ReceiveItems Merges Different Purchase Sources

**Location**: `EmployeeInventory.ReceiveItems(Guid productId, int quantity, decimal unitPrice)`

**Problem**:
```csharp
public void ReceiveItems(Guid productId, int quantity, decimal unitPrice)
{
    var existingItem = Items.FirstOrDefault(i => i.ProductId == productId);
    // ↑ Matches ONLY on ProductId, completely ignores purchase source!
    
    if (existingItem != null)
    {
        existingItem.AddQuantity(quantity); // Merges from different purchase orders!
    }
    else
    {
        Items.Add(InventoryItem.Create(productId, quantity, unitPrice));
    }
}
```

**Data Consistency Break**:
- Issuance from PO-2026-001 (Product A @ $10): 50 units → InventoryItem created with $10
- Later issuance from PO-2026-002 (Product A @ $12): 30 units → SAME InventoryItem updated
- System merges them into 80 units, but which price is stored? **Last one (or first one - undefined!)**
- **Violates the purchase-source tracking principle** - cannot determine which batch items came from

**Contradicts Design**:
The design explicitly states:
- Items tracked by ProductId + UnitPrice
- Purchase reference maintained (PurchaseId, PurchaseLineItemId)
- But EmployeeInventory contradicts this by:
  - Ignoring UnitPrice in matching logic
  - Losing PurchaseId link
  - Losing PurchaseLineItemId link

---

### 🟠 MAJOR ISSUE #3: No Tracking of Purchase Source in EmployeeInventory

**Location**: EmployeeInventory and SupplyRequestItem integration

**Problem**:
The issuance handler properly tracks purchase source:
```csharp
// In IssueSupplyRequestCommandHandler:
public Guid? PurchaseId { get; private set; } // ← SupplyRequestItem has this
public Guid? PurchaseLineItemId { get; private set; } // ← SupplyRequestItem has this
```

But when items move to EmployeeInventory, **this link is LOST**:
```csharp
foreach (var item in supplyRequest.Items)
{
    // ← No PurchaseId or PurchaseLineItemId passed!
    inventory.ReceiveItems(item.ProductId, item.QuantityRequested, item.UnitPrice);
}
```

**Impact**:
- ❌ Cannot trace which purchase batch an employee's items came from
- ❌ Cannot perform reconciliation
- ❌ Cannot validate expiration batches or serial numbers
- ❌ Audit trail incomplete

---

### 🟠 MAJOR ISSUE #4: No Link Between SupplyRequest Items and Purchase Line Items

**Location**: SupplyRequestItem and Purchase relationship

**Problem**:
SupplyRequestItem has purchase tracking fields:
```csharp
public Guid? PurchaseId { get; private set; }
public Guid? PurchaseLineItemId { get; private set; }
```

But the issuance handler NEVER populates these:
```csharp
foreach (var item in supplyRequest.Items)
{
    inventory.ReceiveItems(item.ProductId, item.QuantityRequested, item.UnitPrice);
    // ← item.PurchaseId and item.PurchaseLineItemId never set!
}
```

**Consequence**:
- Purchase.IssueItems() updates Purchase quantity
- But SupplyRequestItem is NOT updated with purchase reference
- Data inconsistency between master and detail records

---

### 🟡 MODERATE ISSUE #5: Purchase.TotalItemsReceived Not Validated Against Sum of LineItems

**Location**: Purchase entity `ReceiveItems()` method

**Problem**:
```csharp
public void ReceiveItems(Guid lineItemId, int quantityReceived)
{
    var lineItem = LineItems.FirstOrDefault(i => i.Id == lineItemId)
        ?? throw new InvalidOperationException($"Line item {lineItemId} not found");
    
    lineItem.MarkReceived(quantityReceived);
    TotalItemsReceived += quantityReceived; // ← Accumulator, no validation
    
    UpdateStatus();
}
```

**Issue**:
- If LineItem.QuantityReceived = 50 units
- But quantityReceived parameter = 100 units (more than ordered)
- `lineItem.MarkReceived(100)` throws in MarkReceived
- But if it didn't, TotalItemsReceived would accumulate incorrectly

**Also Missing**:
- No validation that all line items are received before changing status to "Received"
- Partial receipts don't have a status to indicate "Partially Received"

---

### 🟡 MODERATE ISSUE #6: QuantityRemaining Calculation Not Thread-Safe

**Location**: PurchaseLineItem

**Problem**:
```csharp
public int QuantityRemaining => QuantityReceived - QuantityIssued;
```

This is a computed property with no locking. In concurrent scenarios:
- Thread A reads QuantityRemaining = 10
- Thread B issues 5 units
- Thread A issues 8 units (thinking 10 available)
- Actual result: 3 units issued + 7 pending = over-issuance

**Also**:
- No optimistic locking or version numbers
- No prevention of concurrent ReceiveItems + IssueItems

---

### 🟡 MODERATE ISSUE #7: ConsumeItems in EmployeeInventory Never Called

**Location**: EmployeeInventory.ConsumeItems()

**Problem**:
```csharp
public void ConsumeItems(Guid productId, int quantity)
{
    var item = Items.FirstOrDefault(i => i.ProductId == productId)
        ?? throw new InvalidOperationException($"Product {productId} not in inventory");
    
    item.ConsumeQuantity(quantity);
}
```

**Finding**: This method is DEFINED but never called in the IssueSupplyRequestCommandHandler or anywhere else. 

**Questions**:
- How do employees consume/use items from inventory?
- Is there a separate consumption tracking system?
- Should issuance reduce inventory immediately?
- Or does inventory represent "issued but unused"?

**Current Flow Gap**:
- Items issued → Added to employee inventory
- Items never consumed/removed from inventory
- Employee inventory only grows, never decreases

---

### 🟠 MAJOR ISSUE #8: Issuance Handler Missing Transaction Rollback Logic

**Location**: `IssueSupplyRequestCommandHandler.Handle()`

**Problem**:
```csharp
foreach (var group in issuanceGroups)
{
    var qtyRemaining = group.QuantityNeeded;
    
    foreach (var purchase in group.Purchases.OrderBy(p => p.PurchaseDate))
    {
        // ... issue from purchase ...
        purchase.IssueItems(lineItem.Id, qtyToIssue);
        await _purchaseRepo.AddOrUpdateAsync(purchase, ct); // ← Save each time!
    }
}

// Later...
inventory.ReceiveItems(...); // ← If this fails, purchases already updated!
await _inventoryRepo.AddOrUpdateAsync(inventory, ct);
```

**Issue**:
- Each `_purchaseRepo.AddOrUpdateAsync()` is a separate database operation
- If the final inventory update fails, purchases are already updated
- Data becomes inconsistent

**Should be**:
- Wrap entire operation in database transaction
- OR collect all changes and persist atomically at the end

---

### 🟡 MODERATE ISSUE #9: InventoryItem.AddQuantity Doesn't Track Receipt Dates

**Location**: EmployeeInventory.InventoryItem

**Problem**:
```csharp
public class InventoryItem : ValueObject
{
    public DateTime ReceivedDate { get; private set; } // ← SINGLE date for all quantities
    
    public void AddQuantity(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive");
        CurrentQuantity += quantity;
        // ← ReceivedDate NOT updated
    }
}
```

**Issue**:
- Item received @ 2026-03-01: 50 units
- Item received @ 2026-03-05: 30 units  
- InventoryItem.ReceivedDate = 2026-03-01
- **Oldest date is kept** - cannot identify which batch is newer
- Cannot support expiration tracking or FIFO consumption

---

### 🔴 CRITICAL ISSUE #10: No Validation of Quantity Consistency: Receipt → Inventory → Issuance

**Problem**: No system-wide validation that quantities balance:

**Desired Consistency**:
```
For each Product at each Price:
  Sum of (QuantityReceived in Purchases) 
  = Sum of (Quantity in EmployeeInventories) + Sum of (ConsumedQuantity)
  
Also:
  Sum of (QuantityIssued in Purchases) 
  = Sum of (Quantity in EmployeeInventories) + Sum of (ConsumedQuantity)
```

**Current State**: 
- ❌ No constraint
- ❌ No query to validate this
- ❌ No audit report
- ❌ Potential for quantities to "disappear"

---

## Data Flow Diagram - Current Issues

```
PURCHASE RECEIPT (✓ Good tracking)
  Purchase 1 (PO-2026-001):
    └─ LineItem: Product A @ $10 (500 units received)
        ✓ Tracked with price and quantity
  
  Purchase 2 (PO-2026-002):
    └─ LineItem: Product A @ $12 (500 units received)
        ✓ Tracked with different price

                    ↓ ISSUANCE (✓ Good logic)

ISSSUANCE HANDLER (✓ Proper allocation)
  Request: 250 units of Product A
  Groups by ProductId + UnitPrice
  Issues 250 @ $10 from PO-2026-001
  Updates Purchase 1 quantities
    ✓ Correct cost: $2,500
  
  Later request: 250 units of Product A
  Issues 250 @ $12 from PO-2026-002
  Updates Purchase 2 quantities
    ✓ Correct cost: $3,000

                    ↓ WRITE TO INVENTORY (✗ Data Loss!)

FIRST ISSUANCE (250 @ $10 from PO-2026-001)
  EmployeeInventory
    InventoryItem 1
      ProductId: Product A ✓
      CurrentQuantity: 250 units ✓
      UnitPrice: $10 ✓
      ReceivedDate: 2026-03-01 ✓
      PurchaseId: PO-2026-001 ← (SHOULD HAVE THIS)

            ↓ SECOND ISSUANCE (250 @ $12 from PO-2026-002)
            ↓ INVENTORY.RECEIVEITEMS CALLED AGAIN

SECOND ISSUANCE MERGED (✗ Data Loss!)
  EmployeeInventory
    InventoryItem (SAME ITEM, MERGED!)
      ProductId: Product A ✓
      CurrentQuantity: 500 units ✓ (250+250)
      UnitPrice: ??? ($10 or $12?)  ✗ Which one was kept?
      ReceivedDate: ??? (2026-03-01 or 2026-03-05?) ✗ Unknown!
      PurchaseId: ??? (PO-2026-001 or PO-2026-002?) ✗ Lost!
    
RESULT: 
  ✗ Cannot calculate actual cost: Is it 500 @ $10 ($5,000) or 500 @ $12 ($6,000)?
  ✗ Audit trail: BROKEN - cannot trace which items came from which PO
  ✗ Inventory valuation: IMPOSSIBLE (FIFO/LIFO/weighted average all fail)
  ✗ Batch tracking: LOST - cannot identify expiration dates per batch
  ✗ Consumption tracking: INCOMPLETE (ConsumeItems never called)
  
ACTUAL REALITY:
  Total Cost Paid: $5,000 (PO-001) + $3,000 (PO-002) = $8,000
  Employee Has: 500 units
  System Records: 500 units @ ??? = ??? cost
  MISSING: $2,500 cost differentiation!
```

---

## Recommended Fixes

### Fix #1: Implement FIFO with Purchase Batch Tracking (CRITICAL)

When items have different prices from different purchases, use FIFO (First In, First Out) to consume oldest batches first:

```csharp
// InventoryBatch value object for FIFO tracking
public class InventoryBatch : ValueObject
{
    public Guid PurchaseId { get; private set; }
    public Guid PurchaseLineItemId { get; private set; }
    public int QuantityReceived { get; private set; }
    public int QuantityIssued { get; private set; }
    public decimal UnitPrice { get; private set; }
    public DateTime ReceivedDate { get; private set; }
    
    public int AvailableQuantity => QuantityReceived - QuantityIssued;
    
    public void IssueQuantity(int quantity)
    {
        if (quantity > AvailableQuantity)
            throw new InvalidOperationException("Not enough quantity in batch");
        QuantityIssued += quantity;
    }
}

// Updated InventoryItem to hold multiple batches
public class InventoryItem : AggregateRoot
{
    public Guid ProductId { get; private set; }
    public List<InventoryBatch> Batches { get; private set; } = [];
    
    public int TotalQuantity => Batches.Sum(b => b.AvailableQuantity);
    public decimal AverageCost => Batches.Any() 
        ? Batches.Sum(b => b.UnitPrice * b.AvailableQuantity) / TotalQuantity 
        : 0;
    
    // FIFO consumption: Take from oldest batch first
    public void ReceiveItems(
        Guid productId,
        int quantity,
        decimal unitPrice,
        Guid purchaseId,
        Guid purchaseLineItemId,
        DateTime receivedDate)
    {
        ProductId = productId;
        var batch = InventoryBatch.Create(
            purchaseId, 
            purchaseLineItemId,
            quantity, 
            unitPrice, 
            receivedDate);
        Batches.Add(batch);
        // ✓ Batches stay separate by purchase
    }
    
    // FIFO issuance: consume oldest batch first
    public List<(Guid PurchaseId, int Quantity, decimal Cost)> ConsumeItems(int quantityNeeded)
    {
        var result = new List<(Guid, int, decimal)>();
        var remaining = quantityNeeded;
        
        // Sort by ReceivedDate ascending (FIFO)
        foreach (var batch in Batches.OrderBy(b => b.ReceivedDate))
        {
            if (remaining <= 0) break;
            
            int quantityFromBatch = Math.Min(remaining, batch.AvailableQuantity);
            batch.IssueQuantity(quantityFromBatch);
            
            result.Add((
                batch.PurchaseId,
                quantityFromBatch,
                quantityFromBatch * batch.UnitPrice));
            
            remaining -= quantityFromBatch;
        }
        
        if (remaining > 0)
            throw new InvalidOperationException($"Insufficient inventory: need {quantityNeeded}, have {TotalQuantity}");
        
        return result;
    }
}
```

**Benefits of FIFO Batch Tracking**:
- ✓ Each purchase price tracked separately and independently
- ✓ FIFO ensures oldest items consumed first (freshness/expiration)
- ✓ Accurate cost calculation per issuance (knows exact source PO)
- ✓ Audit trail preserved (can trace item → batch → purchase)
- ✓ Inventory valuation using actual costs, not merged values

**Example with Different Prices**:
```
Employee Inventory for Product A:
  Batch 1: 250 units @ $10 (from PO-2026-001, received 2026-03-01)
  Batch 2: 250 units @ $12 (from PO-2026-002, received 2026-03-05)

When consuming 300 units:
  FIFO takes:
    - 250 units @ $10 from Batch 1 (cost: $2,500) ✓
    - 50 units @ $12 from Batch 2 (cost: $600) ✓
  Total Cost: $3,100 (not $3,000 or $3,600, but correct $3,100!)
  Remaining: 200 units @ $12
```

---

### Fix #2: Update Issuance Handler to Use FIFO (CRITICAL)

Modify IssueSupplyRequestCommandHandler to use ConsumeItems() FIFO:

```csharp
foreach (var productGroup in supplyRequest.Items.GroupBy(i => i.ProductId))
{
    var totalQuantity = productGroup.Sum(i => i.QuantityRequested);
    var inventoryItem = inventory.Items.FirstOrDefault(
        i => i.ProductId == productGroup.Key);
    
    if (inventoryItem == null)
        return Result.Failure("Product not in inventory");
    
    // FIFO consumes oldest batches first
    var consumedBatches = inventoryItem.ConsumeItems(totalQuantity);
    
    foreach (var (purchaseId, qty, cost) in consumedBatches)
    {
        var purchase = await _purchaseRepo.GetAsync(purchaseId, ct);
        purchase.UpdateIssuedQuantity(qty);
        _purchaseRepo.AddOrUpdate(purchase);
    }
}
```

### Fix #3: Create InventoryConsumption Audit Log (CRITICAL)

Create audit trail for each FIFO consumption:

```csharp
public class InventoryConsumption : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid EmployeeId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid PurchaseId { get; private set; }  // ← Which batch/PO
    public Guid PurchaseLineItemId { get; private set; }
    public int QuantityConsumed { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal CostConsumed { get; private set; }  // qty × unitPrice
    public DateTime ReceivedDate { get; private set; }  // When received (batch date)
    public DateTime ConsumedDate { get; private set; }  // When consumed
    public int DaysInInventory => (ConsumedDate - ReceivedDate).Days;
    public string? Reason { get; private set; }
    
    // Auditing fields...
}

// DbContext addition:
public DbSet<InventoryConsumption> Consumptions { get; set; }
```

This creates complete audit trail: Purchase → Batch → Consumption (when issued) → Employee

### Fix #4: Update Purchase to Track FIFO Batch Quantities (MAJOR)

Ensure Purchase tracks quantities issued from each batch correctly:

```csharp
public class Purchase : AggregateRoot<Guid>, IHasTenant, IAuditableEntity
{
    // Existing fields...
    public List<PurchaseLineItem> LineItems { get; private set; }
}

public class PurchaseLineItem : ValueObject
{
    public Guid ProductId { get; private set; }
    public int QuantityOrdered { get; private set; }
    public int QuantityReceived { get; private set; }
    public int QuantityIssued { get; private set; }  // ← Track per lineitem
    public int AvailableQuantity => QuantityReceived - QuantityIssued;
    public decimal UnitPrice { get; private set; }
    public DateTime? ReceivedDate { get; private set; }  // When this batch received
    
    public void IssueQuantity(int qty)
    {
        if (qty > AvailableQuantity)
            throw new InvalidOperationException("Cannot issue more than available");
        QuantityIssued += qty;
    }
}

public enum PurchaseStatus
{
    Ordered,
    PartiallyReceived,  // ← Some line items received
    Received,           // All line items received
    PartiallyIssued,    // Some received items issued
    FullyIssued         // All received items issued
}
```

### Fix #5: Add Transaction Wrapping Around FIFO Consumption (MAJOR)

Wrap FIFO batch consumption in transaction to ensure atomicity:

```csharp
public async Task<Result> Handle(IssueSupplyRequestCommand request, CancellationToken ct)
{
    using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
    
    try
    {
        var supplyRequest = await _supplyRequestRepo.GetAsync(request.SupplyRequestId, ct);
        var inventory = await _inventoryRepo.GetByEmployeeAsync(supplyRequest.EmployeeId, ct);
        var purchasesToUpdate = new List<Purchase>();
        
        // FIFO consumption of all items
        foreach (var productGroup in supplyRequest.Items.GroupBy(i => i.ProductId))
        {
            var inventoryItem = inventory.Items.FirstOrDefault(i => i.ProductId == productGroup.Key);
            var totalQuantity = productGroup.Sum(i => i.QuantityRequested);
            
            // ConsumeItems uses FIFO batches
            var consumedBatches = inventoryItem.ConsumeItems(totalQuantity);
            
            foreach (var (purchaseId, qty, cost) in consumedBatches)
            {
                var purchase = await _purchaseRepo.GetAsync(purchaseId, ct);
                var lineItem = purchase.LineItems.First(li => 
                    li.ProductId == productGroup.Key && 
                    li.AvailableQuantity >= qty);
                
                lineItem.IssueQuantity(qty);
                purchasesToUpdate.Add(purchase);
                
                // Log consumption
                var consumption = InventoryConsumption.Create(
                    supplyRequest.EmployeeId,
                    productGroup.Key,
                    purchase.Id,
                    lineItem.Id,
                    qty,
                    lineItem.UnitPrice,
                    lineItem.ReceivedDate.Value);
                _dbContext.Consumptions.Add(consumption);
            }
        }
        
        // Update all in one shot
        supplyRequest.MarkAsIssued();
        _supplyRequestRepo.AddOrUpdate(supplyRequest);
        _inventoryRepo.AddOrUpdate(inventory);
        purchasesToUpdate.ForEach(p => _purchaseRepo.AddOrUpdate(p));
        
        await _dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        
        return Result.Success();
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync(ct);
        return Result.Failure($"Issuance failed: {ex.Message}");
    }
}
```

**Key Points**:
- ✓ FIFO consumption is atomic (all-or-nothing)
- ✓ Prevents partial state updates
- ✓ Rollback if any batch cannot satisfy quantity

### Fix #6: Implement FIFO Batch Reconciliation Query (MAJOR)

Verify that received quantities = inventory quantities + consumed quantities:

```csharp
public async Task<InventoryReconciliationDto> GetFIFOReconciliation(string tenantId, CancellationToken ct)
{
    var purchases = await _purchaseRepo.GetAsync(p => p.TenantId == tenantId, ct);
    var inventories = await _inventoryRepo.GetAsync(i => i.TenantId == tenantId, ct);
    var consumptions = await _dbContext.Consumptions
        .Where(c => c.TenantId == tenantId)
        .ToListAsync(ct);
    
    var reconciliation = new InventoryReconciliationDto();
    
    foreach (var product in GetDistinctProducts(purchases, inventories))
    {
        // Per PURCHASE BATCH (FIFO)
        var batches = purchases
            .SelectMany(p => p.LineItems.Where(li => li.ProductId == product.Id)
                .Select(li => new 
                { 
                    PurchaseId = p.Id,
                    BatchDate = li.ReceivedDate,
                    QuantityReceived = li.QuantityReceived,
                    QuantityIssued = li.QuantityIssued,
                    UnitPrice = li.UnitPrice
                }));
        
        foreach (var batch in batches.OrderBy(b => b.BatchDate)) // FIFO order
        {
            // Consumed from this batch
            var consumed = consumptions
                .Where(c => c.PurchaseId == batch.PurchaseId && c.ProductId == product.Id)
                .Sum(c => c.QuantityConsumed);
            
            // Still in inventory from this batch
            var inInventory = batch.QuantityReceived - batch.QuantityIssued;
            
            reconciliation.Batches.Add(new ReconciliationBatch
            {
                ProductId = product.Id,
                PurchaseId = batch.PurchaseId,
                ReceivedDate = batch.BatchDate,
                QuantityReceived = batch.QuantityReceived,
                QuantityConsumed = consumed,
                QuantityInInventory = inInventory,
                Total = consumed + inInventory,
                Status = (consumed + inInventory == batch.QuantityReceived) 
                    ? "OK" 
                    : "MISMATCH"
            });
        }
    }
    
    return reconciliation;
}
```

**Reconciliation Equation**:
```
For each batch (Purchase + LineItem):
  QuantityReceived = QuantityConsumed + QuantityInInventory + QuantityUnaccounted
  If (QuantityUnaccounted > 0) → Data integrity issue
```

### Fix #7: Lock Batch Quantities During FIFO Consumption (MODERATE)

Add optimistic locking to prevent race conditions during concurrent issuances:

```csharp
public class InventoryBatch : ValueObject
{
    public Guid PurchaseId { get; private set; }
    public Guid PurchaseLineItemId { get; private set; }
    public int QuantityReceived { get; private set; }
    public int QuantityIssued { get; private set; }
    public decimal UnitPrice { get; private set; }
    public DateTime ReceivedDate { get; private set; }
    public int Version { get; private set; }  // ← Optimistic concurrency
    
    public int AvailableQuantity => QuantityReceived - QuantityIssued;
    
    public void IssueQuantity(int quantity)
    {
        if (quantity > AvailableQuantity)
            throw new ConcurrencyException(
                $"Batch only has {AvailableQuantity} available (attempted {quantity})");
        QuantityIssued += quantity;
        Version++;  // Increment for optimistic locking
    }
}

// DbContext configuration:
modelBuilder.Entity<InventoryBatch>()
    .Property(b => b.Version)
    .IsConcurrencyToken();  // EF will throw DbUpdateConcurrencyException
```

**Example Flow**:
```
1. Thread A reads Batch: AvailableQuantity = 100, Version = 1
2. Thread B reads Batch: AvailableQuantity = 100, Version = 1
3. Thread A issues 80 units → Version = 2, saves
4. Thread B issues 50 units → Tries to update Version 1 → CONFLICT!
   (Actually only 20 available, not 100)
5. Result: Thread B gets ConcurrencyException, must retry with fresh read
```

---

## Summary Table - FIFO Batch Approach

| Fix | Severity | What It Does | Impact |
|-----|----------|--------------|--------|
| Fix #1: FIFO Batch Tracking | 🔴 CRITICAL | Multiple batches per product by ReceivedDate | Enables FIFO consumption order |
| Fix #2: FIFO Consumption Handler | 🔴 CRITICAL | Uses ConsumeItems() with batch sorting | Correct cost per issuance |
| Fix #3: InventoryConsumption Audit | 🔴 CRITICAL | Logs each batch consumed with cost | Complete audit trail |
| Fix #4: Purchase Batch Quantities | 🟠 MAJOR | Tracks QuantityIssued per batch | Prevents over-issuance per batch |
| Fix #5: Transaction Wrapping | 🟠 MAJOR | Atomic FIFO consumption | All-or-nothing issuance |
| Fix #6: FIFO Reconciliation Query | 🟠 MAJOR | Validates Received = Consumed + Inventory | Detects data gaps |
| Fix #7: Optimistic Locking | 🟡 MODERATE | Version field on InventoryBatch | Prevents concurrent race conditions |

**Summary of Issues Resolved**:
- ✅ Different prices from different POs tracked separately (batches)
- ✅ FIFO ensures oldest batches consumed first
- ✅ Cost tracking accurate (each batch has UnitPrice × Quantity)
- ✅ Audit trail complete (batch → purchase → consumption)
- ✅ Reconciliation possible (received = consumed + remaining)
- ✅ Concurrency safe (optimistic locking prevents race conditions)

---

## Testing Strategy - FIFO Batch Scenarios

### Test #1: FIFO Consumption Order
```
1. Create Purchase 1: Product A, 100 units @ $10, received 2026-03-01
2. Create Purchase 2: Product A, 50 units @ $12, received 2026-03-05
3. Employee inventory now has 2 batches
4. Create SupplyRequest: Product A, 120 units
5. Consume items → FIFO ConsumeItems()
   Expected: Takes 100 @ $10 from batch 1 (oldest), then 20 @ $12 from batch 2
   Cost: (100 × $10) + (20 × $12) = $1,240 ✓
6. Verify InventoryConsumption audit log shows both batches
```

### Test #2: FIFO Reconciliation
```
1. Setup: 100 units @ $10 (PO-1), 50 units @ $12 (PO-2)
2. Issue 80 units (FIFO: 80 @ $10 from PO-1)
3. Run GetFIFOReconciliation()
   Expected Results:
   - PO-1 Batch: Received 100, Consumed 80, Remaining 20 ✓
   - PO-2 Batch: Received 50, Consumed 0, Remaining 50 ✓
   - Total: Received 150 = Consumed 80 + Remaining 70 ✓
4. Verify reconciliation status = "OK" for both batches
```

### Test #3: Concurrent FIFO Issuance (Race Condition)
```
1. Create Purchase: 100 units @ $10 (Version 1)
2. Thread A issues 60 units
   - Reads batch (Version 1, Available 100)
   - Calls IssueQuantity(60) → Version = 2
   - Saves successfully
3. Thread B issues 50 units (concurrently)
   - Reads batch (Version 1, Available 100)  ← Stale read!
   - Calls IssueQuantity(50) → Version = 2
   - Tries to save → CONFLICT! (Current version is 2)
   - Gets ConcurrencyException, must retry
   - On retry: reads fresh (Version 2, Available 40)
   - Fails: 50 > 40 available → InvalidOperationException ✓
   - Result: Only 60 issued, Thread B notified of shortage
```

### Test #4: Batch Cost Tracking
```
1. Create Purchase 1: 100 units @ $10 = $1,000 total
2. Create Purchase 2: 100 units @ $12 = $1,200 total
3. Issue 150 units to employee (FIFO)
4. Verify InventoryConsumption records:
   - Batch 1: 100 units × $10 = $1,000 consumed cost
   - Batch 2: 50 units × $12 = $600 consumed cost
   - Total: $1,600 (not $1,500 or $1,800)
5. Verify Purchase quantities updated:
   - PO-1: QuantityIssued = 100
   - PO-2: QuantityIssued = 50
```

---

## Conclusion

The system has **strong purchase tracking and issuance logic** but the EmployeeInventory consolidates items too aggressively, losing purchase and price information. The solution is to implement **FIFO batch tracking** where each InventoryItem maintains separate batches by (ProductId, PurchaseId, ReceivedDate), and issuance uses FIFO ordering to consume oldest batches first.

**Key Changes**:
- ✅ InventoryBatch value object tracks each purchase separately
- ✅ ConsumeItems() implements FIFO (sorted by ReceivedDate)
- ✅ InventoryConsumption audit logs each batch consumed with exact cost
- ✅ Transaction wrapping ensures atomic FIFO operations
- ✅ Reconciliation query validates Received = Consumed + Remaining
- ✅ Optimistic locking prevents concurrent race conditions

**Implementation Priority**:
1. **Immediate (P0)**: Fix #1 (FIFO Batches) + Fix #2 (FIFO Handler) + Fix #3 (Audit)
2. **Next (P1)**: Fix #4 (Batch Quantities) + Fix #5 (Transactions) + Fix #6 (Reconciliation)
3. **Then (P2)**: Fix #7 (Optimistic Locking)

**Expected Outcome**:
When different prices of same item occur across different purchases, the system will:
1. Track them in separate batches (no merging)
2. Consume in FIFO order (oldest first)
3. Record exact cost per consumption
4. Enable reconciliation (Received = Issued + Remaining)
5. Prevent concurrent corruption via optimistic locks
