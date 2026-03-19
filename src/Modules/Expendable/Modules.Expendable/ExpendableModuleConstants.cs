namespace FSH.Modules.Expendable;

public static class ExpendableModuleConstants
{
    public const string SchemaName = "expendable";
    public const string MigrationsTable = "__EFMigrationsHistory";

    /// <summary>Permission constants for Expendable module</summary>
    public static class Permissions
    {
        public const string ViewAll = "Permissions.Expendable.View";
        public const string CreateAll = "Permissions.Expendable.Create";
        public const string UpdateAll = "Permissions.Expendable.Update";
        public const string DeleteAll = "Permissions.Expendable.Delete";

        public static class Products
        {
            public const string View = Contracts.ExpendablePermissionConstants.Products.View;
            public const string Create = Contracts.ExpendablePermissionConstants.Products.Create;
            public const string Update = Contracts.ExpendablePermissionConstants.Products.Update;
            public const string Delete = Contracts.ExpendablePermissionConstants.Products.Delete;
            public const string Activate = Contracts.ExpendablePermissionConstants.Products.Activate;
            public const string Deactivate = Contracts.ExpendablePermissionConstants.Products.Deactivate;
        }

        public static class Purchases
        {
            public const string View = Contracts.ExpendablePermissionConstants.Purchases.View;
            public const string Create = Contracts.ExpendablePermissionConstants.Purchases.Create;
            public const string Update = Contracts.ExpendablePermissionConstants.Purchases.Update;
            public const string Delete = Contracts.ExpendablePermissionConstants.Purchases.Delete;
            public const string Approve = Contracts.ExpendablePermissionConstants.Purchases.Approve;
            public const string Receive = Contracts.ExpendablePermissionConstants.Purchases.Receive;
        }

        public static class SupplyRequests
        {
            public const string View = Contracts.ExpendablePermissionConstants.SupplyRequests.View;
            public const string Create = Contracts.ExpendablePermissionConstants.SupplyRequests.Create;
            public const string Update = Contracts.ExpendablePermissionConstants.SupplyRequests.Update;
            public const string Delete = Contracts.ExpendablePermissionConstants.SupplyRequests.Delete;
            public const string Approve = Contracts.ExpendablePermissionConstants.SupplyRequests.Approve;
            public const string Reject = Contracts.ExpendablePermissionConstants.SupplyRequests.Reject;
        }

        public static class ShoppingCarts
        {
            public const string View = Contracts.ExpendablePermissionConstants.ShoppingCarts.View;
            public const string Create = Contracts.ExpendablePermissionConstants.ShoppingCarts.Create;
            public const string Edit = Contracts.ExpendablePermissionConstants.ShoppingCarts.Edit;
            public const string Clear = Contracts.ExpendablePermissionConstants.ShoppingCarts.Clear;
            public const string Convert = Contracts.ExpendablePermissionConstants.ShoppingCarts.Convert;
        }

        public static class Inventory
        {
            public const string View = Contracts.ExpendablePermissionConstants.Inventory.View;
            public const string Receive = Contracts.ExpendablePermissionConstants.Inventory.Receive;
            public const string Consume = Contracts.ExpendablePermissionConstants.Inventory.Consume;
            public const string ViewReports = Contracts.ExpendablePermissionConstants.Inventory.ViewReports;
        }
    }

    /// <summary>Feature flag constants for Expendable module</summary>
    public static class Features
    {
        public const string ModuleName = "Expendable";
        public const string ProductManagement = $"{ModuleName}:ProductManagement";
        public const string PurchaseOrders = $"{ModuleName}:PurchaseOrders";
        public const string SupplyRequests = $"{ModuleName}:SupplyRequests";
        public const string ShoppingCart = $"{ModuleName}:ShoppingCart";
        public const string InventoryTracking = $"{ModuleName}:InventoryTracking";
    }
}

