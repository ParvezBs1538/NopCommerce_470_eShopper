$(function () {
  NopstationGoogleAnalytics.init()
});

var NopstationGoogleAnalytics = {
  init: function () {
    $('.product-item').click(function (e) {
      NopstationGoogleAnalytics.productClicked(e);
    });

    $('#newsletter-subscribe-button').on('click', function () {
      NopstationGoogleAnalytics.newsLetterClicked();
    });
  },

  newsLetterClicked: function () {
    if ($('#newsletter_subscribe').is(':checked')) {
      var email = $("#newsletter-email").val();
      gtag('event', 'newsletter', {
        'status': 'unsubscribe',
        'email': email
      });
    }
    else {
      var email = $("#newsletter-email").val();
      gtag('event', 'newsletter', {
        'status': 'subscribe',
        'email': email
      });
    }
  },

  productClicked: function (elem) {
    var productId = elem.currentTarget.dataset.productid;
    var productName = elem.currentTarget.querySelector('.product-title a').textContent;
    gtag('event', 'product-item-clicked', {
      'product-id': productId,
      'product-name': productName
    });
  },
};
