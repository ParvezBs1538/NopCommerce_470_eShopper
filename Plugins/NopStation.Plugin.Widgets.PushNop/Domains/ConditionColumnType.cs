namespace NopStation.Plugin.Widgets.PushNop.Domains
{
    public enum ConditionColumnType
    {
        SubscribedOnUtc = 20,
        OrderedBeforeDateUtc = 50,
        OrderedAfterDateUtc = 51,
        NeverOrdered = 52,
        TotalNumberOfProductsOrdered = 53,
        TotalSpentAmountOnOrder = 54,
        PurchasedFromCategoryId = 55,
        PurchasedFromVendorId = 56,
        PurchasedFromManufacturerId = 57,
        PurchasedWithDiscountId = 58,
        CustomerEmail = 70,
        CustomerRegisteredOnUtc = 74,
        CustomerLastActivityDateUtc = 75,
        CustomerLastLoginDateUtc = 76
    }
}
