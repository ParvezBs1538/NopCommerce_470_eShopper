$(document).ready(function () {
  NSGoogleTagManager.init()
});

var NSGoogleTagManager = {
  init: function () {
    $('.add-to-cart-button').click(function (e) {
      var isAddToCartClicked = false;
      $(document).ajaxStop(function () {
        if (!isAddToCartClicked) {
          isAddToCartClicked = true;
          NSGoogleTagManager.addToCartCicked(e, true);
        }
      });
    });
    $('.product-box-add-to-cart-button').click(function (e) {
      var isAddToCartClicked = false;
      $(document).ajaxStop(function () {
        if (!isAddToCartClicked) {
          isAddToCartClicked = true;
          NSGoogleTagManager.addToCartCicked(e, false);
        }
      });
    });
    $('.add-to-wishlist-button').click(function (e) {
      var isAddToWishlistClicked = false;
      $(document).ajaxStop(function () {
        if (!isAddToWishlistClicked) {
          isAddToWishlistClicked = true;
          NSGoogleTagManager.addToWishlistCicked(e, true);
        }
      });
    });
  },

  clearDataLayer: function () {
    dataLayer.push(function () {
      this.reset();
    });
  },
  addToCartCicked: function (elem, flag) {
    window.dataLayer = window.dataLayer || [];
    dataLayer.push({
      ecommerce: null
    });
    var productId = elem.currentTarget.dataset.productid;
    if (!productId)
      productId = elem.currentTarget.closest('.product-item').dataset.productid;
    if (!productId) return;
    var quantity = 0;
    if ($('.qty-input').length > 0)
      quantity = $('.qty-input').val();

    $.ajax({
      cache: false,
      type: "GET",
      url: "/GtmEventSend/ProductDetails?productId=" + productId + "&isClickedFromProductDetailsPage=" + flag + "&quantity=" + quantity,
      success: function (val, textStatus, jqXHR) {
        if (!val.Result)
          return;
        var product = {
          'item_id': val.Data.Sku,
          'item_name': val.Data.Name,
          'affiliation': val.Data.Affiliation,
          'coupon': val.Data.Coupon,
          'discount': val.Data.Discount,
          'index': val.Data.Index,
          'item_brand': val.Data.Manufacturer,
          'price': val.Data.Price,
          'quantity': val.Data.Copy,
          'copy': val.Data.Copy
        };

        for (var i = 0; i < val.Data.Categories.length; i++) {
          var categoryKey = 'item_category'  
          if (i > 0) categoryKey = categoryKey + (i + 1);
          product[categoryKey] = val.Data.Categories[i];
        }
        var items = [product];
        dataLayer.push({
          event: 'add_to_cart',
          'var_prodid': [val.Data.Sku],
          'var_pagetype': 'product',
          "var_prodval": val.Data.Price,
          'ecommerce': {
            'currency': val.Data.Currency,
            'value': val.Data.Price,
            'items': items
          }
        });
        NSGoogleTagManager.clearDataLayer();
      },
    });
  },

  addToWishlistCicked: function (elem, flag) {
    window.dataLayer = window.dataLayer || [];
    dataLayer.push({
      ecommerce: null
    });
    var productId = elem.currentTarget.dataset.productid;
    if (!productId) {
      productId = elem.currentTarget.closest('.product-item').dataset.productid;
      flag = false;
    }
    if (!productId) return;
    var shoppingCart = false;
    var quantity = 0;
    if ($('.qty-input').length > 0)
      quantity = $('.qty-input').val();
    $.ajax({
      cache: false,
      type: "GET",
      url: "/GtmEventSend/ProductDetails?productId=" + productId + "&isClickedFromProductDetailsPage=" + flag + "&quantity=" + quantity + "&isShoppingCart=" + shoppingCart,
      success: function (val, textStatus, jqXHR) {
        if (!val.Result)
          return;
        var product = {
          'item_id': val.Data.Sku,
          'item_name': val.Data.Name,
          'affiliation': val.Data.Affiliation,
          'coupon': val.Data.Coupon,
          'discount': val.Data.Discount,
          'index': val.Data.Index,
          'item_brand': val.Data.Manufacturer,
          'price': val.Data.Price,
          'quantity': val.Data.Copy,
          'copy': val.Data.Copy
        };

        for (var i = 0; i < val.Data.Categories.length; i++) {
          var categoryKey = 'item_category' 
          if (i > 0) categoryKey = categoryKey + (i + 1);
          product[categoryKey] = val.Data.Categories[i];
        }
        var items = [product];
        dataLayer.push({
          event: 'add_to_wishlist',
          'var_prodid': [val.Data.Sku],
          'var_pagetype': 'product',
          "var_prodval": val.Data.Price,
          'ecommerce': {
            'currency': val.Data.Currency,
            'value': val.Data.Price,
            'items': items
          }
        });
        NSGoogleTagManager.clearDataLayer();
      },
    });
  },
};

$(document).ready(function () {
  function processProducts(products, promotion = false) {
    var items = [];

    for (var i = 0; i < products.length; i++) {
      var data = products[i];
      var product = {
        'item_id': data.Sku,
        'item_name': data.Name,
        'affiliation': data.Affiliation,
        'coupon': data.Coupon,
        'currency': data.CurrencyCode,
        'discount': data.Discount,
        'index': data.Index,
        'item_brand': data.Brand,
        'item_list_id': data.ItemListId,
        'item_list_name': data.ItemListName,
        'price': data.Price,
        'quantity': data.Quantity,
      };

      for (var j = 0; j < data.Categories.length; j++) {
        var categoryKey = 'item_category' 
        if (j > 0) categoryKey = categoryKey + (j + 1);
        product[categoryKey] = data.Categories[j];
      }
      if (promotion) product['location_id'] = "";
      items.push(product);
    }

    return items;
  }

  //newsletter subscribe-event
  const button = document.querySelector('.newsletter-subscribe-button');
  if (button) {
    button.addEventListener('click', function () {
      var emailInput = document.querySelector('.newsletter-subscribe-text');
      var email = emailInput.value;
      var regex = /^([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})$/;
      if (regex.test(email)) {
        dataLayer.push({
          event: 'newsletter_subscription',
          'source': 'form_footer'
        });
        clearDataLayer();
      }
    });
  }

  //Shipping info added event
  const nextButton = document.querySelector('.shipping-method-next-step-button');

  if (nextButton) {
    nextButton.addEventListener('click', function () {
      var checkedShippingOption = $("input[name='shippingoption']:checked");
      if (checkedShippingOption.length > 0) {
        var checkedShippingVal = checkedShippingOption.val();
        var shippingName = checkedShippingVal.split("___");
        var systemName = shippingName[0];
        $.ajax({
          cache: false,
          type: "POST",
          url: "/GtmEventSend/ShoppingCartDetails",
          success: function (val, textStatus, jqXHR) {
            if (!val.Result)
              return;

            var items = [];
            for (var i = 0; i < val.Products.length; i++) {
              var data = val.Products[i];
              var product = {
                'item_id': data.Sku,
                'item_name': data.Name,
                'affilication': data.Affiliation,
                'coupon': data.Coupon,
                'currency': data.CurrencyCode,
                'discount': data.Discount,
                'index': data.Index,
                'item_brand': data.Brand,
                'item_list_id': data.ItemListId,
                'item_list_name': data.ItemListName,
                'price': data.Price,
                'quantity': data.Quantity,
                'copy': data.Quantity,
              };
              for (var j = 0; j < data.Categories.length; j++) {
                var categoryKey = 'item_category'  
                if (j > 0) categoryKey = categoryKey + (j + 1);
                product[categoryKey] = data.Categories[j];
              }
              items.push(product);
            }
            clearDataLayer();
            dataLayer.push({
              event: 'add_shipping_info',
              'var_prodid': val.ProductIds,
              'var_currency': val.Currency,
              'ecommerce': {
                'currency': val.Currency,
                'value': val.Total,
                'coupon': "",
                'shipping_tier': systemName,
                'items': items
              }
            });
            clearDataLayer();
          },
        });
      }
    });
  }

  const paymentContinueButton = document.querySelector('.payment-method-next-step-button');
  if (paymentContinueButton) {
    paymentContinueButton.addEventListener('click', function () {
      var checkedPaymentOption = $("input[name='paymentmethod']:checked");
      if (checkedPaymentOption.length > 0) {
        var systemName = checkedPaymentOption.val();
        $.ajax({
          cache: false,
          type: "POST",
          url: "/GtmEventSend/ShoppingCartDetails",
          data: { systemName: systemName },
          success: function (val, textStatus, jqXHR) {
            if (!val.Result)
              return;

            var items = [];
            for (var i = 0; i < val.Products.length; i++) {
              var data = val.Products[i];
              var product = {
                'item_id': data.Sku,
                'item_name': data.Name,
                'affilication': data.Affiliation,
                'coupon': data.Coupon,
                'currency': data.CurrencyCode,
                'discount': data.Discount,
                'index': data.Index,
                'item_brand': data.Brand,
                'item_list_id': data.ItemListId,
                'item_list_name': data.ItemListName,
                'price': data.Price,
                'quantity': data.Quantity,
                'copy': data.Quantity,
              };
              for (var j = 0; j < data.Categories.length; j++) {
                var categoryKey = 'item_category' 
                if (j > 0) categoryKey = categoryKey + (j + 1);
                product[categoryKey] = data.Categories[j];
              }
              items.push(product);
            }
            dataLayer.push({
              event: 'add_payment_info',
              'var_prodid': val.ProductIds,
              'var_currency': val.Currency,
              'ecommerce': {
                'currency': val.Currency,
                'value': val.Total,
                'coupon': "",
                'payment_type': val.Name,
                'items': items
              }
            });
            clearDataLayer();
          },
        });
      }
    });
  }

  function clearDataLayer() {
    dataLayer.push(function () {
      this.reset();
    });
  }

  const productItemDivs = document.querySelectorAll('div.product-item');
  var pageType = $(".page_type_gtm").data("page-type");
  if (productItemDivs && pageType !== 'Category') {
    const productIdList = [];
    productItemDivs.forEach(div => {
      const productId = div.getAttribute('data-productid');
      if (productId) {
        productIdList.push(productId);
      }
    });
    $.ajax({
      cache: false,
      type: "POST",
      url: "/GtmEventSend/GetProducts",
      data: { productIds: productIdList },
      success: function (val, textStatus, jqXHR) {
        if (!val.Result || productIdList.length === 0)
          return;

        var items = processProducts(val.Products);
        dataLayer.push({
          event: 'view_item_list',
          'var_prodid': val.ProductIds,
          'var_pagetype': pageType,
          'ecommerce': {
            'items': items,
          }
        });
        clearDataLayer();
      },
    });
  }
});