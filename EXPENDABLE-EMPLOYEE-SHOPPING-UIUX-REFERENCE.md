# Expendable Employee Shopping UI/UX Integration Reference

Date: 2026-03-11

## Objective
Design and implement an employee shopping experience for expendables where employees can browse items, search with partial keywords, add items to cart, and submit supply requests.

## Current State Check

### Backend (Expendable module)
Backend capabilities are already largely implemented.

- Expendable module maps full Cart and Supply Request endpoint groups:
  - `api/v{version}/expendable/cart`
  - `api/v{version}/expendable/supply-requests`
- Cart endpoints present for:
  - Get/Create cart
  - Add item
  - Get cart
  - Remove item
  - Clear cart
  - Convert cart to supply request
- Supply request contracts and endpoints exist, including employee-focused query patterns.
- Product search supports keyword filtering on Name, SKU, and Description (partial text via `Contains`).

Key files:
- `src/Modules/Expendable/Modules.Expendable/ExpendableModule.cs`
- `src/Modules/Expendable/Modules.Expendable.Contracts/v1/Cart/CartContracts.cs`
- `src/Modules/Expendable/Modules.Expendable.Contracts/v1/Requests/SupplyRequestContracts.cs`
- `src/Modules/Expendable/Modules.Expendable/Features/v1/Products/SearchProducts/SearchProductsQueryHandler.cs`

### Blazor UI
Employee-facing shopping UI is not fully integrated yet.

- Products page exists and now supports search UI, but it is still admin-oriented CRUD first.
- Cart page is placeholder text.
- Supply requests page is placeholder text.

Key files:
- `src/Playground/Playground.Blazor/Components/Pages/Expendable/ProductsPage.razor`
- `src/Playground/Playground.Blazor/Components/Pages/Expendable/CartPage.razor`
- `src/Playground/Playground.Blazor/Components/Pages/Expendable/SupplyRequestsPage.razor`

### API Client Compatibility Gap (Important)
The generated cart client appears out of sync with backend endpoint routes.

Examples:
- Generated client uses `api/v1/expendable/cart/get-or-create`.
- Backend maps get-or-create to `POST /employee/{employeeId}/cart`.
- Generated client uses `api/v1/expendable/cart/{id}/add-item`.
- Backend maps add item to `POST /{cartId}/items`.

Key files:
- `src/Playground/Playground.Blazor/ApiClient/Generated.cs`
- `src/Modules/Expendable/Modules.Expendable/Features/v1/Cart/GetOrCreateCart/GetOrCreateCartEndpoint.cs`
- `src/Modules/Expendable/Modules.Expendable/Features/v1/Cart/AddToCart/AddToCartEndpoint.cs`

Conclusion: backend feature foundation exists, but employee shopping UI is not complete and cart client/openapi alignment must be fixed before full UI integration.

## Integration Plan

## Phase 1 - Catalog UX (Employee Browse/Search)
1. Create employee-first catalog page (card/list toggle) separate from admin product management concerns.
2. Keep search always visible with partial keyword behavior and debounce.
3. Add filters: category, in-stock only, sort (name, relevance).
4. Add quantity stepper and Add to Cart per product card.

Deliverables:
- Employee catalog page and reusable product card component.
- Search/filter state management with server paging.

## Phase 2 - Cart UX
1. Replace cart placeholder with full cart page:
   - line items, quantity updates, remove item, clear cart
2. Show totals and item count summary.
3. Add convert-to-request action from cart.

Deliverables:
- Functional cart page wired to cart endpoints.
- Error handling for stale items/out-of-stock and optimistic updates.

## Phase 3 - Submit Request UX
1. Add request submission form from cart:
   - department
   - needed-by date
   - business justification
2. Submit via convert-cart-to-request flow.
3. Show request confirmation and navigate to request details/history.

Deliverables:
- End-to-end submit flow from cart to request.
- Confirmation and post-submit UX.

## Phase 4 - My Requests UX
1. Replace supply requests placeholder with request list + details.
2. Include status chips and timeline (Submitted, Approved, Rejected, Fulfilled).
3. Add quick actions where policy allows (cancel draft, clone request).

Deliverables:
- Employee request tracking pages.

## Phase 5 - Hardening and Quality
1. Regenerate and validate OpenAPI clients so cart routes align with backend.
2. Add integration tests for browse/search/cart/submit.
3. Add UI tests for key employee journeys.
4. Add telemetry for search terms, add-to-cart conversion, and submit funnel.

Deliverables:
- Stable API/UI contract and regression coverage.

## Immediate Next Actions (Recommended)
1. Resolve API client route mismatch for cart endpoints (regenerate clients from latest OpenAPI).
2. Implement CartPage with live get/add/remove/clear behavior.
3. Add Add-to-Cart action from the product browsing UI.
4. Wire convert-to-request on cart and replace SupplyRequestsPage placeholder.

## Notes
- Backend already has significant domain and CQRS coverage for cart/request workflows.
- Remaining work is mostly UI integration, API client alignment, and end-to-end polish.

## Feature Blueprint (Shopping-Like UX)

### Employee Experience (Primary)
1. Browse Catalog
  - Product cards with name, SKU, UoM, stock badge, and price.
  - Search and quick filters (category, in-stock, active only).
2. Build Cart
  - Add to cart from catalog.
  - Update quantity, remove line, clear cart.
  - Running subtotal and total item count.
3. Submit Request
  - Convert cart to supply request.
  - Provide needed-by date and business justification.
  - Confirmation with request number and deep-link to details.
4. Track My Requests
  - List of own requests and statuses.
  - Request detail timeline (Submitted, Approved, Rejected, Fulfilled).

### Supply Officer Experience
1. Incoming Queue
  - Search/filter submitted requests by date, employee, status.
2. Decisioning
  - Approve or reject with reason.
3. Fulfillment Visibility
  - Mark fulfilled and expose fulfillment details for employee tracking.

## API-to-UX Mapping (Reference)

### Catalog
- `GET /api/v1/expendable/products/search` (browse/search products)

### Cart
- `POST /api/v1/expendable/cart/employee/{employeeId}/cart` (get/create cart)
- `POST /api/v1/expendable/cart/{cartId}/items` (add item)
- `GET /api/v1/expendable/cart/{cartId}` (view cart)
- `DELETE /api/v1/expendable/cart/{cartId}/items/{productId}` (remove item)
- `DELETE /api/v1/expendable/cart/{cartId}/clear` (clear cart)
- `POST /api/v1/expendable/cart/{cartId}/convert-to-request` (submit request from cart)

### Supply Requests
- `GET /api/v1/expendable/supply-requests/{id}` (request detail)
- `GET /api/v1/expendable/supply-requests/employee/{employeeId}` (employee request history)
- `GET /api/v1/expendable/supply-requests/search` (officer queue/reporting)
- `POST /api/v1/expendable/supply-requests/{id}/submit` (submit)
- `POST /api/v1/expendable/supply-requests/{id}/approve` (approve)
- `POST /api/v1/expendable/supply-requests/{id}/reject` (reject)

## UI Implementation Checklist (Blazor)

Use this checklist as an execution tracker for the employee shopping UX in Playground.Blazor.

### Legend
- Size: S (0.5-1 day), M (1-2 days), L (3-5 days)
- Status: [ ] Not started, [~] In progress, [x] Done

### Phase A - Catalog (Employee Browse/Search)
- [ ] Create employee-first catalog layout in products page or separate page (Size: M)
- [ ] Product card component with name, SKU, UoM, stock, price, quantity input (Size: M)
- [ ] Search input with debounce and server-backed keyword query (Size: S)
- [ ] Filter chips: category, in-stock, active only (Size: M)
- [ ] Sort options: relevance, name, price (Size: S)
- [ ] Add-to-cart CTA from each product card with inline feedback (Size: S)

Estimated effort: 5-7 days

### Phase B - Cart
- [ ] Replace cart placeholder with full cart page integration (Size: M)
- [ ] Load cart by employee and show empty-state UX (Size: S)
- [ ] Quantity update, remove line, clear cart actions (Size: M)
- [ ] Cart summary panel: subtotal, total items, validation hints (Size: S)
- [ ] Error states for unavailable/stale products and retry actions (Size: S)

Estimated effort: 4-6 days

### Phase C - Convert Cart to Supply Request
- [ ] Request submission form modal/section (needed-by + justification) (Size: M)
- [ ] Convert cart to request API integration and success handling (Size: S)
- [ ] Confirmation state with request id and quick links (Size: S)

Estimated effort: 2-3 days

### Phase D - My Requests (Employee)
- [ ] Replace requests placeholder with employee request list (Size: M)
- [ ] Request detail drawer/page with item lines and history (Size: M)
- [ ] Status chips and timeline presentation (Submitted/Approved/Rejected/Fulfilled) (Size: S)
- [ ] Pagination and filters (status/date) (Size: S)

Estimated effort: 4-6 days

### Phase E - Supply Officer Review UX
- [ ] Queue page for incoming requests (search + filters) (Size: M)
- [ ] Approve/reject actions with reason capture (Size: M)
- [ ] Fulfillment marking and result visibility to employee (Size: S)

Estimated effort: 3-5 days

### Cross-Cutting Tasks
- [ ] Regenerate and verify API client routes match current endpoints (Size: S)
- [ ] Consistent loading/empty/error components across pages (Size: S)
- [ ] Authorization-aware UI states (hide/disable by permission) (Size: S)
- [ ] Basic telemetry: search term, add-to-cart, convert-to-request (Size: M)
- [ ] Smoke tests for end-to-end employee flow (Size: M)

Estimated effort: 3-4 days

### Suggested Delivery Sprints
- Sprint 1: Phase A + Phase B core (catalog and cart)
- Sprint 2: Phase C + Phase D (submit and track requests)
- Sprint 3: Phase E + cross-cutting hardening

Total rough effort: 21-31 working days (single developer), can be reduced with parallel frontend/backend/test streams.
