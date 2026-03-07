# Entity Flow & Consistency Monitoring - Module 2 Expendable System

## Overview

This document explains how entities flow from **Purchase → Issuance → Inventory**, and how consistency is maintained throughout the entire lifecycle.

---

## Part 1: Entity Design Overview

### Core Entities

```
┌─────────────────────────────────────────────────────────────────┐
│                    SUPPLIER INTERACTION                          │
├─────────────────────────────────────────────────────────────────┤
│ Purchase (Aggregate)                                             │
│  - PurchaseOrderNumber: PO-2026-001                             │
│  - Status: Ordered → Received → PartiallyIssued → FullyIssued   │
│  - LineItems: List<PurchaseLineItem>                            │
│    Each LineItem has: ProductId, Qty, UnitPrice, ReceivedDate   │
│  - Total tracking: TotalItemsReceived, TotalItemsIssued         │
│  - Cost tracking: TotalCost, TotalIssuedValue                   │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                    EMPLOYEE REQUEST                              │
├─────────────────────────────────────────────────────────────────┤
│ SupplyRequest (Aggregate)                                        │
│  - Source: Online (from cart) or WalkIn                         │
│  - Status: Pending → Approved → Allocated → Issued              │
│  - Items: List<SupplyRequestItem>                               │
│    Each Item has: ProductId, Qty, UnitPrice                     │
│    LINKED TO: PurchaseId, PurchaseLineItemId (source)           │
│  - Approval flow: ApprovedBy, ApprovedDate                      │
│  - Allocation: AllocatedBy, AllocatedDate                       │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                   EMPLOYEE POSSESSION                            │
├─────────────────────────────────────────────────────────────────┤
│ EmployeeInventory (Aggregate) - FIFO BATCHES                   │
│  - EmployeeId, EmployeeName                                     │
│  - Items: List<InventoryItem>                                   │
│    FIFO Batches per Product:                                    │
│    • Batch 1: ProductId, Qty, UnitPrice, ReceivedDate, PurchaseId
│    • Batch 2: ProductId, Qty, UnitPrice, ReceivedDate, PurchaseId
│    • ConsumeItems(qty) → FIFO order (oldest ReceivedDate first) │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                  CONSUMPTION TRACKING                            │
├─────────────────────────────────────────────────────────────────┤
│ InventoryConsumption (Audit Entity)                             │
│  - EmployeeId, ProductId, QuantityConsumed                      │
│  - PurchaseId, PurchaseLineItemId (batch source)               │
│  - Cost: UnitPrice × QuantityConsumed                           │
│  - Dates: ReceivedDate, ConsumedDate (tracks FIFO age)          │
└─────────────────────────────────────────────────────────────────┘
```

---

## Part 2: Flow from Purchase → Issuance → Inventory

### Flow Diagram

```
STEP 1: PURCHASE ORDERED
========================
1. Supplier provides quote for 100 units @ $10 each
2. System creates Purchase aggregate
   Purchase (PO-2026-001)
   ├─ Status: Ordered
   ├─ LineItem 1: Product-A, Qty=100, Price=$10
   └─ TotalCost: $1,000

STEP 2: PURCHASE RECEIVED
=========================
Warehouse receives 100 units of Product-A from supplier
Receiving staff marks items as received:

   Purchase (PO-2026-001)
   ├─ Status: Ordered → Received
   ├─ LineItem 1: 
   │  ├─ QuantityOrdered: 100
   │  ├─ QuantityReceived: 100  ← MARKED
   │  ├─ QuantityIssued: 0
   │  ├─ ReceivedDate: 2026-03-01
   │  └─ UnitPrice: $10
   └─ TotalItemsReceived: 100

STEP 3: EMPLOYEE REQUEST (SUPPLY REQUEST)
=========================================
Employee John submits online supply request:

   SupplyRequest (EXP-2026-001)
   ├─ EmployeeId: john-guid
   ├─ EmployeeName: John Smith
   ├─ Source: Online
   ├─ Status: Pending → Approved
   ├─ Items: [
   │  {
   │    ProductId: product-a-guid,
   │    QuantityRequested: 50,
   │    UnitPrice: $10,
   │    PurchaseId: null  ← Not yet linked!
   │    PurchaseLineItemId: null
   │  }
   │ ]
   └─ ApprovedBy: manager-guid

STEP 4: ISSUANCE (CRITICAL STEP)
================================
System matches supply request items to available purchases (FIFO)

BEFORE ISSUANCE:
   Purchase (PO-2026-001)
   ├─ LineItem 1: Qty Received=100, Qty Issued=0, Remaining=100

   Employee Inventory (john-guid)
   └─ Empty (no items yet)

ISSUANCE HANDLER LOGIC (FIFO):
   Request needs: 50 units of Product-A
   Available batches (ordered by ReceivedDate):
     Batch 1: PO-2026-001, Qty=100, Price=$10, ReceivedDate=2026-03-01 ← OLDEST (FIFO)
   
   Allocate to Batch 1: Take 50 units @ $10 = $500
   
   Update Purchase:
   ├─ LineItem 1: QuantityIssued: 50 (was 0)
   ├─ TotalItemsIssued: 50
   ├─ TotalIssuedValue: $500 ($50 × 10)
   ├─ Status: Received → PartiallyIssued
   └─ AssociatedSupplyRequestIds: [EXP-2026-001]
   
   Update SupplyRequest item with source:
   ├─ PurchaseId: po-2026-001-guid
   ├─ PurchaseLineItemId: lineitem-1-guid
   ├─ Quantity was 50, all from one batch (no split)
   
   Add to Employee Inventory:
   ├─ InventoryBatch 1:
   │  ├─ ProductId: product-a-guid
   │  ├─ QuantityReceived: 50
   │  ├─ QuantityIssued: 0
   │  ├─ UnitPrice: $10
   │  ├─ ReceivedDate: 2026-03-01 (batch date from PO)
   │  ├─ PurchaseId: po-2026-001-guid
   │  ├─ PurchaseLineItemId: lineitem-1-guid
   │  └─ Version: 1 (for optimistic locking)
   
   Create Consumption Audit Log:
   ├─ EmployeeId: john-guid
   ├─ ProductId: product-a-guid
   ├─ QuantityConsumed: 50
   ├─ UnitPrice: $10
   ├─ CostConsumed: $500
   ├─ PurchaseId: po-2026-001-guid
   ├─ ReceivedDate: 2026-03-01
   ├─ ConsumedDate: 2026-03-07
   └─ DaysInInventory: 6 days

STEP 5: EMPLOYEE RECEIVES ITEMS
================================
John now has 50 units in his personal inventory

   Employee Inventory (john-guid)
   ├─ InventoryBatch 1:
   │  ├─ ProductId: product-a-guid
   │  ├─ Qty: 50 units
   │  ├─ Price: $10/unit
   │  ├─ ReceivedDate: 2026-03-01 (when purchased from supplier)
   │  └─ Source: PO-2026-001

STEP 6: SECOND PURCHASE (DIFFERENT PRICE)
==========================================
Supplier now charges $12/unit for same product

   Purchase (PO-2026-002)
   ├─ Status: Ordered → Received
   ├─ LineItem 1: Product-A, Qty=100, Price=$12 ← DIFFERENT PRICE!
   ├─ QuantityReceived: 100
   ├─ ReceivedDate: 2026-03-05
   └─ TotalItemsReceived: 100

STEP 7: SECOND ISSUANCE (MIXED PRICES - FIFO!)
==============================================
Another employee (Jane) requests 120 units of Product-A

   SupplyRequest (EXP-2026-002)
   ├─ Items: [{ProductId: product-a, Qty: 120}]
   ├─ Status: Approved
   
ISSUANCE HANDLER LOGIC (FIFO CROSS-PURCHASE):
   Request needs: 120 units
   
   Available batches (ordered by ReceivedDate):
     Batch 1: PO-2026-001, Qty=100 (50 already issued, 50 remaining), 
              ReceivedDate=2026-03-01, Price=$10 ← OLDEST
     Batch 2: PO-2026-002, Qty=100 (0 issued), 
              ReceivedDate=2026-03-05, Price=$12
   
   FIFO Allocation:
     From Batch 1: Take remaining 50 @ $10 = $500
     From Batch 2: Take 70 @ $12 = $840 ← Next oldest batch
     Total: 120 units, Cost: $1,340
   
   Update Purchases:
     PO-2026-001: QuantityIssued += 50 (was 50, now 100 - FULLY ISSUED)
     PO-2026-002: QuantityIssued += 70 (was 0, now 70 - PARTIALLY ISSUED)
   
   Update Jane's Inventory:
     ├─ InventoryBatch 1: (same product from PO-2026-001)
     │  ├─ Qty: 50
     │  ├─ Price: $10
     │  ├─ ReceivedDate: 2026-03-01 ← OLDEST, FIFO
     │  └─ PurchaseId: po-2026-001-guid
     │
     └─ InventoryBatch 2: (same product from PO-2026-002)
        ├─ Qty: 70
        ├─ Price: $12
        ├─ ReceivedDate: 2026-03-05 ← NEWER
        └─ PurchaseId: po-2026-002-guid
   
   Consumption Audit Logs (2 entries):
     Entry 1:
       ├─ EmployeeId: jane-guid
       ├─ Qty: 50
       ├─ Price: $10
       ├─ Cost: $500
       ├─ ReceivedDate: 2026-03-01
       ├─ DaysInInventory: 6 days
       └─ PurchaseId: po-2026-001-guid
     
     Entry 2:
       ├─ EmployeeId: jane-guid
       ├─ Qty: 70
       ├─ Price: $12
       ├─ Cost: $840
       ├─ ReceivedDate: 2026-03-05
       ├─ DaysInInventory: 2 days
       └─ PurchaseId: po-2026-002-guid
```

---

## Part 3: Consistency Monitoring

### Consistency Rule #1: FIFO Batch Separation

**Rule**: Items with different prices from different purchases must be tracked in separate batches.

**How it's maintained**:
```csharp
// During issuance, ConsumeItems() returns which batches were used:
var consumedBatches = inventoryItem.ConsumeItems(120);
// Result: [(PO-001, 50 units @ $10), (PO-002, 70 units @ $12)]

// Each batch is maintained separately with:
public class InventoryBatch
{
    public Guid PurchaseId { get; set; }           // Which PO
    public int QuantityReceived { get; set; }      // Total from this PO
    public int QuantityIssued { get; set; }        // How much issued from this PO
    public DateTime ReceivedDate { get; set; }     // When received (for FIFO)
    public decimal UnitPrice { get; set; }         // Price from this PO
    public int Version { get; set; }               // Optimistic locking
}
```

**Monitoring via Query**:
```csharp
// Get inventory for John
var john = await _inventoryRepo.GetByEmployeeAsync(john-guid);

// His inventory has 2 batches:
john.Items[0].Batches[0] → Product-A: 50 units @ $10 from PO-001
john.Items[0].Batches[1] → Product-A: 70 units @ $12 from PO-002
```

---

### Consistency Rule #2: Quantity Reconciliation

**Rule**: For each batch, `QuantityReceived = QuantityIssued + QuantityInInventory`

**How it's maintained**:
```csharp
public class InventoryBatch
{
    public int QuantityReceived { get; set; }    // 100 from supplier
    public int QuantityIssued { get; set; }      // 50 issued to employees
    public int AvailableQuantity => 
        QuantityReceived - QuantityIssued;       // 50 still in warehouse
}

// BEFORE any issuance fails, we verify:
public void IssueQuantity(int qty)
{
    if (qty > AvailableQuantity)
        throw new InvalidOperationException(
            $"Cannot issue {qty}, only {AvailableQuantity} available");
    QuantityIssued += qty;
}
```

**Monitoring via Query** - FIFO Reconciliation:
```csharp
public async Task<InventoryReconciliationDto> GetFIFOReconciliation()
{
    // For each batch from each purchase:
    foreach (var purchase in purchases)
    {
        foreach (var lineItem in purchase.LineItems)
        {
            // Received from supplier
            int received = lineItem.QuantityReceived;    // 100
            
            // Issued to employees (from InventoryConsumption records)
            int consumed = consumptions
                .Where(c => c.PurchaseId == purchase.Id)
                .Sum(c => c.QuantityConsumed);          // 50
            
            // Still in warehouses/employee inventory
            int remaining = lineItem.QuantityRemaining;  // 50
            
            // This MUST be true:
            Assert.AreEqual(received, consumed + remaining);
            // 100 == 50 + 50 ✓
        }
    }
}
```

---

### Consistency Rule #3: Cost Tracking

**Rule**: Total cost of inventory must equal sum of all consumption costs.

**How it's maintained**:
```csharp
// Purchase LineItem tracks cost:
public class PurchaseLineItem
{
    public int QuantityOrdered { get; set; }     // 100
    public decimal UnitPrice { get; set; }        // $10
    public decimal TotalCost => 
        QuantityOrdered * UnitPrice;              // $1,000
    
    public int QuantityIssued { get; set; }      // 50
    public decimal IssuedValue => 
        QuantityIssued * UnitPrice;               // $500
}

// EmployeeInventory tracks per-batch cost:
public class InventoryBatch
{
    public decimal UnitPrice { get; set; }       // $10
    public int QuantityReceived { get; set; }    // 50 in inventory
    public decimal InventoryValue => 
        QuantityReceived * UnitPrice;            // $500
}

// Consumption records exact cost when issued:
public class InventoryConsumption
{
    public decimal UnitPrice { get; set; }       // $10
    public int QuantityConsumed { get; set; }    // 50
    public decimal CostConsumed => 
        QuantityConsumed * UnitPrice;            // $500
}
```

**Monitoring Equation**:
```
Purchase Total Cost = Issued Value + Remaining Inventory Value

Example:
  Purchase (PO-2026-001): Total = $1,000
  
  After issuance:
  - Issued to employees: 50 units × $10 = $500
  - Remaining in stock: 50 units × $10 = $500
  
  Verify: $1,000 == $500 + $500 ✓
```

---

### Consistency Rule #4: Purchase ↔ SupplyRequest Link

**Rule**: Every supply request item must trace back to a purchase.

**How it's maintained**:
```csharp
public class SupplyRequestItem
{
    public Guid? PurchaseId { get; set; };              // Which PO
    public Guid? PurchaseLineItemId { get; set; };      // Which line in PO
    public DateTime? PurchaseDate { get; set; };        // When purchased
}

// During issuance, these are populated:
var supplyItem = supplyRequest.Items[0];
supplyItem.SetPurchaseSource(
    purchaseId: purchase.Id,
    purchaseLineItemId: lineItem.Id,
    purchaseDate: purchase.PurchaseDate);

// Now we can trace: SupplyRequest → Purchase → Supplier
```

**Monitoring via Query**:
```csharp
// Verify all supply requests have purchase links
var orphanedItems = await _dbContext.SupplyRequestItems
    .Where(i => i.PurchaseId == null)
    .ToListAsync();

// Should be empty (all items must come from purchases)
Assert.AreEqual(0, orphanedItems.Count);
```

---

### Consistency Rule #5: Atomic Issuance (All-or-Nothing)

**Rule**: When issuing multiple batches for one request, either all succeed or none.

**How it's maintained**:
```csharp
using var transaction = await _dbContext.Database
    .BeginTransactionAsync(ct);

try
{
    // FIFO consumption of ALL items
    var consumedBatches = inventoryItem.ConsumeItems(120);
    // Returns: [(batch1, 50), (batch2, 70)]
    
    // Update ALL related entities
    foreach (var (purchaseId, qty, cost) in consumedBatches)
    {
        var purchase = await _purchaseRepo.GetAsync(purchaseId, ct);
        purchase.UpdateIssuedQuantity(qty);  // Updates PO-2026-001 and PO-2026-002
        _purchaseRepo.AddOrUpdate(purchase);
    }
    
    // Save everything at once
    await _dbContext.SaveChangesAsync(ct);
    await transaction.CommitAsync(ct);
    
    // If any of above fails (e.g., data conflict), entire transaction rolls back
}
catch (Exception ex)
{
    await transaction.RollbackAsync(ct);
    // Inventory batches are UNCHANGED
    // Purchase quantities are UNCHANGED
    // Issuance completely failed
}
```

**Monitoring**: Check transaction logs for incomplete operations.

---

### Consistency Rule #6: Optimistic Locking (Prevent Race Conditions)

**Rule**: Concurrent issuances cannot corrupt batch quantities.

**How it's maintained**:
```csharp
public class InventoryBatch
{
    public int Version { get; set; }  // Version token
}

// Scenario: Two threads issue simultaneously
Thread A:
  1. Reads Batch: QuantityReceived=100, QuantityIssued=0, Version=1
  2. Calls IssueQuantity(60) → QuantityIssued=60, Version=2
  3. Saves → Version 2 in database

Thread B (concurrent):
  1. Reads Batch: QuantityReceived=100, QuantityIssued=0, Version=1 ← STALE!
  2. Calls IssueQuantity(50) → QuantityIssued=50, Version=2
  3. Tries to save with Version=1 → CONFLICT!
     (Database has Version=2, not Version=1)
  4. Gets DbUpdateConcurrencyException
  5. Must retry with fresh read
  6. Fresh read: QuantityReceived=100, QuantityIssued=60, Version=2
  7. Only 40 available now, cannot issue 50 → fails appropriately

Result: No corruption, proper handling of race condition
```

---

## Part 4: Monitoring Dashboard Queries

### Query 1: Inventory Status by Employee

```csharp
public async Task<InventoryStatusDto> GetEmployeeInventoryStatus(Guid employeeId)
{
    var inventory = await _inventoryRepo.GetByEmployeeAsync(employeeId);
    
    var status = inventory.Items.SelectMany(item => item.Batches).Select(batch => new
    {
        ProductId = batch.ProductId,
        QuantityAvailable = batch.AvailableQuantity,
        UnitPrice = batch.UnitPrice,
        InventoryValue = batch.AvailableQuantity * batch.UnitPrice,
        ReceivedDate = batch.ReceivedDate,
        PurchaseId = batch.PurchaseId,
        DaysInInventory = (DateTime.UtcNow - batch.ReceivedDate).Days
    }).ToList();
    
    return new InventoryStatusDto
    {
        EmployeeName = inventory.EmployeeName,
        TotalItems = status.Sum(s => s.QuantityAvailable),
        TotalValue = status.Sum(s => s.InventoryValue),
        Batches = status
    };
}
```

### Query 2: FIFO Reconciliation (Purchase → Issued → Inventory)

```csharp
public async Task<FIFOReconciliationDto> ValidateReconciliation()
{
    var purchases = await _purchaseRepo.GetAllAsync();
    var consumptions = await _dbContext.Consumptions.ToListAsync();
    var inventories = await _inventoryRepo.GetAllAsync();
    
    var reconciliation = new List<ReconciliationBatch>();
    
    foreach (var purchase in purchases)
    {
        foreach (var lineItem in purchase.LineItems)
        {
            // What was received from supplier
            int received = lineItem.QuantityReceived;
            
            // What was consumed (sent to employees)
            int consumed = consumptions
                .Where(c => c.PurchaseId == purchase.Id && 
                           c.ProductId == lineItem.ProductId)
                .Sum(c => c.QuantityConsumed);
            
            // What remains in any inventory (employee or warehouse)
            int remaining = inventories
                .SelectMany(i => i.Items)
                .SelectMany(item => item.Batches)
                .Where(b => b.PurchaseId == purchase.Id && 
                           b.ProductId == lineItem.ProductId)
                .Sum(b => b.AvailableQuantity);
            
            var variance = received - (consumed + remaining);
            
            reconciliation.Add(new ReconciliationBatch
            {
                PurchaseId = purchase.Id,
                ProductId = lineItem.ProductId,
                QuantityReceived = received,
                QuantityConsumed = consumed,
                QuantityRemaining = remaining,
                Variance = variance,
                Status = variance == 0 ? "OK" : "MISMATCH"
            });
        }
    }
    
    return new FIFOReconciliationDto
    {
        Batches = reconciliation,
        TotalVariance = reconciliation.Sum(r => Math.Abs(r.Variance)),
        HasErrors = reconciliation.Any(r => r.Variance != 0)
    };
}
```

### Query 3: Cost Accuracy Check

```csharp
public async Task<CostAccuracyDto> ValidateCostAccuracy()
{
    var purchases = await _purchaseRepo.GetAllAsync();
    var consumptions = await _dbContext.Consumptions.ToListAsync();
    var inventories = await _inventoryRepo.GetAllAsync();
    
    var report = new List<CostAccuracyItem>();
    
    foreach (var purchase in purchases)
    {
        decimal totalCostPaid = purchase.LineItems
            .Sum(li => li.QuantityOrdered * li.UnitPrice);  // What we paid
        
        decimal costIssued = consumptions
            .Where(c => c.PurchaseId == purchase.Id)
            .Sum(c => c.CostConsumed);  // What left in consumption
        
        decimal costRemaining = purchase.LineItems
            .Sum(li => li.QuantityRemaining * li.UnitPrice);  // What's left
        
        decimal variance = totalCostPaid - (costIssued + costRemaining);
        
        report.Add(new CostAccuracyItem
        {
            PurchaseId = purchase.Id,
            CostPaid = totalCostPaid,
            CostIssued = costIssued,
            CostRemaining = costRemaining,
            Variance = variance,
            Status = Math.Abs(variance) < 0.01m ? "OK" : "MISMATCH"
        });
    }
    
    return new CostAccuracyDto
    {
        Items = report,
        TotalVariance = report.Sum(r => Math.Abs(r.Variance)),
        HasErrors = report.Any(r => Math.Abs(r.Variance) > 0.01m)
    };
}
```

---

## Part 5: Summary - Consistency Mechanisms

| Mechanism | Purpose | Implementation | Check |
|-----------|---------|-----------------|-------|
| **FIFO Batches** | Separate prices from different POs | InventoryBatch per (ProductId, PurchaseId, ReceivedDate) | Items never merge by product alone |
| **Quantity Validation** | Prevent over-issuance | `if (qty > AvailableQuantity) throw` in IssueQuantity() | PurchaseLineItem.QuantityIssued ≤ QuantityReceived |
| **Atomic Transactions** | All-or-nothing issuance | Database transactions around multi-batch updates | No partial state changes |
| **Optimistic Locking** | Prevent race conditions | Version field incremented on each update | Concurrent threads cannot corrupt quantities |
| **Audit Logs** | Complete trail | InventoryConsumption records every consumption | Can trace which batch, which employee, when |
| **Reconciliation Queries** | Verify integrity | Received = Consumed + Remaining (per batch) | Daily reports detect gaps |
| **Cost Tracking** | Financial accuracy | Each batch maintains UnitPrice independently | Total cost = issued cost + remaining cost |
| **Purchase Links** | Full traceability | SupplyRequestItem → PurchaseId → SupplyRequest | Can trace back: Employee → Request → Purchase → Supplier |

---

## Part 6: Data Flow Example (Complete Journey)

```
DAY 1 - PURCHASE PHASE
======================
Supplier Quote: 100 units of Pen @ $0.50 each
Purchase (PO-2026-001) Created
├─ Status: Ordered
├─ LineItem: Pen, Qty=100, Price=$0.50
└─ TotalCost: $50.00

DAY 2 - RECEIPT PHASE
====================
Warehouse receives 100 pens
Purchase (PO-2026-001)
├─ Status: Ordered → Received
├─ LineItem: QuantityReceived=100, ReceivedDate=2026-03-02
└─ AvailableQuantity: 100

DAY 3 - EMPLOYEE REQUEST PHASE
==============================
Employee Alice requests 30 pens (Online Supply Request)
SupplyRequest (EXP-2026-001)
├─ EmployeeId: alice-guid
├─ Status: Pending → Approved
└─ Item: Pen, Qty=30

DAY 4 - ISSUANCE PHASE (FIFO)
=============================
FIFO matches: Only 1 batch available
├─ PO-2026-001: 100 available @ $0.50, ReceivedDate=2026-03-02

Issue 30 pens to Alice:
1. Update Purchase (PO-2026-001):
   ├─ QuantityIssued: 0 → 30
   ├─ Status: Received → PartiallyIssued
   └─ RemainingQuantity: 100 → 70

2. Add to Alice's Inventory:
   └─ InventoryBatch 1:
      ├─ ProductId: pen-guid
      ├─ Qty: 30
      ├─ Price: $0.50
      ├─ ReceivedDate: 2026-03-02
      ├─ PurchaseId: po-2026-001-guid
      └─ InventoryValue: $15.00

3. Create Audit Entry:
   └─ InventoryConsumption:
      ├─ EmployeeId: alice-guid
      ├─ Qty: 30
      ├─ Cost: $15.00
      ├─ ReceivedDate: 2026-03-02
      └─ ConsumedDate: 2026-03-04

DAY 5 - SECOND EMPLOYEE REQUEST
===============================
Employee Bob requests 50 pens
SupplyRequest (EXP-2026-002)
├─ EmployeeId: bob-guid
└─ Item: Pen, Qty=50

ISSUANCE PHASE (FIFO):
Available batch: PO-2026-001 has 70 remaining @ $0.50

Issue 50 pens to Bob:
1. Update Purchase:
   ├─ QuantityIssued: 30 → 80
   └─ Status: PartiallyIssued

2. Add to Bob's Inventory:
   └─ InventoryBatch 1:
      ├─ Qty: 50
      ├─ Cost: $25.00
      └─ PurchaseId: po-2026-001-guid

3. Create Audit Entry:
   └─ InventoryConsumption:
      ├─ EmployeeId: bob-guid
      ├─ Qty: 50
      └─ Cost: $25.00

REMAINING AFTER DAY 5:
======================
Purchase (PO-2026-001):
├─ QuantityOrdered: 100
├─ QuantityReceived: 100
├─ QuantityIssued: 80  (30 to Alice + 50 to Bob)
└─ QuantityRemaining: 20 @ $0.50 = $10.00

Warehouse Stock:
└─ 20 pens remaining = $10.00 value

Consumption Total: $15.00 + $25.00 = $40.00
Total: $40.00 (issued) + $10.00 (remaining) = $50.00 ✓

RECONCILIATION CHECK:
Received: 100 units
Consumed: 80 units (to employees)
Remaining: 20 units
Total: 100 == 80 + 20 ✓

Cost Check:
Paid: 100 × $0.50 = $50.00
Issued: 80 × $0.50 = $40.00
Remaining: 20 × $0.50 = $10.00
Total: $50.00 == $40.00 + $10.00 ✓
```

---

## Conclusion

The entity design ensures consistency through:

1. **FIFO Batches**: Separate tracking by (Product, Purchase, ReceivedDate)
2. **Quantity Validation**: No over-issuance at any level
3. **Atomic Operations**: All-or-nothing issuance across multiple batches
4. **Optimistic Locking**: Concurrent-safe updates
5. **Complete Audit Trail**: InventoryConsumption tracks every transaction
6. **Reconciliation Queries**: Daily validation that Received = Issued + Remaining
7. **Cost Accuracy**: Every batch tracks its own price independently

This design prevents data loss, cost inaccuracy, and enables full traceability from **Supplier → Purchase → Employee Inventory → Consumption**.
