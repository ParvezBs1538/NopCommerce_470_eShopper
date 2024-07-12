namespace NopStation.Plugin.Misc.WebApi.Models.ShoppingCart
{
    public class AddToCartResponseModel
    {
        public int TotalShoppingCartProducts { get; set; }

        public int TotalWishListProducts { get; set; }

        public bool RedirectToDetailsPage { get; set; }

        public bool RedirectToWishListPage { get; set; }

        public bool RedirectToShoppingCartPage { get; set; }
    }
}
