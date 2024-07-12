
var QuickView = {
  loadWaiting: false,
  picturezoomenabled: false,
  productdetailsurl: '',
  modalselector: '',
  modalbuttonselector: '',
  loaderselector: '',
  localized_data: false,

  init: function (picturezoomenabled, productdetailsurl, modalselector, modalbuttonselector, loaderselector, localized_data) {
    this.loadWaiting = false;
    this.picturezoomenabled = picturezoomenabled;
    this.productdetailsurl = productdetailsurl;
    this.modalselector = modalselector;
    this.modalbuttonselector = modalbuttonselector;
    this.loaderselector = loaderselector;
    this.localized_data = localized_data;
  },

  setLoadWaiting: function (display) {
    if (display) {
      $(QuickView.loaderselector).show();
    }
    else {
      $(QuickView.loaderselector).hide();
    }
    this.loadWaiting = display;
  },

  render_quickview_button: function (label) {
    $('.product-item[data-quick-view!="true"]').each(function (i, obj) {
      var productid = $(this).data('productid');
      var html = '<button type="button" class="quick-view-btn" data-productid="' + productid + '" title="' + label + '">' + label + '</button>';
      $(this).children('.picture').children('a').append(html);
      $(this).attr("data-quick-view", true);
    });

    $('.quick-view-btn').click(function (event) {
      event.preventDefault();
      var productid = $(this).data('productid');
      QuickView.load_product_details(productid);
    })
  },

  load_product_details: function (productid) {
    if (this.loadWaiting !== false) {
      return;
    }
    this.setLoadWaiting(true);
    $(QuickView.modalselector + ' .qv-details').html('');
    $(QuickView.modalselector).css('display', 'block');

    $(QuickView.modalbuttonselector).magnificPopup({
      items: {
        src: QuickView.modalselector,
        type: 'inline'
      },
      preloader: true
    });
    $(QuickView.modalbuttonselector).click();

    $.ajax({
      cache: false,
      type: 'POST',
      data: { productId: productid },
      url: QuickView.productdetailsurl,
      success: this.success_process,
      error: this.ajaxFailure,
      complete: this.resetLoadWaiting
    });
  },

  success_process: function (response) {
    QuickView.setLoadWaiting(false);
    $(QuickView.modalselector + ' .qv-details').html(response.html);

    $(QuickView.modalselector + " .mfp-close").on("click", function () {
      $(QuickView.modalselector).hide();
      $(QuickView.modalselector + ' .qv-details').html('');
    });

    $(QuickView.modalselector + ' .item-grid').addClass("owl-carousel");
    $(QuickView.modalselector + ' .item-grid').owlCarousel({
      items: 4,
      margin: 14,
      loop: true,
      autoplay: true,
      autoplayTimeout: 8000,
      nav: true,
      responsive: {
        0: {
          items: 1
        },
        450: {
          items: 2
        },
        768: {
          items: 3
        },
        1200: {
          items: 4
        }
      }
    });
  },

  resetLoadWaiting: function () {
    QuickView.setLoadWaiting(false);
  },

  ajaxFailure: function () {
    alert(this.localized_data.QuickViewFailure);
  }
};