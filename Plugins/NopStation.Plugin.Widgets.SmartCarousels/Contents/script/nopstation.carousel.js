
var SmartCarousel = {
  carouseldetailsurl: '',
  containerselector: '',
  loaderselector: '',
  loadwait: true,
  localized_data: false,

  init: function (carouseldetailsurl, containerselector, loaderselector, localized_data) {
    this.carouseldetailsurl = carouseldetailsurl;
    this.containerselector = containerselector;
    this.loaderselector = loaderselector;
    this.localized_data = localized_data;
    this.loadwait = true;

    SmartCarousel.check_carousels();

    $(window).scroll(function () {
      if (!SmartCarousel.loadwait) {
        SmartCarousel.check_carousels();
      }
    });
  },

  check_carousels: function () {
    $(SmartCarousel.containerselector + '[data-loaded!="true"]').each(function () {
      var elem = $(this);
      if (SmartCarousel.chek_element_on_screen(elem)) {
        if (!elem.data('loading')) {
          elem.attr('data-loading', true);
          var carouselid = elem.data('carouselid');
          SmartCarousel.load_carousel_details(carouselid);
        }
      }
    })

    SmartCarousel.loadwait = false;
  },

  chek_element_on_screen: function (elem) {
    var docViewTop = $(window).scrollTop();
    var docViewBottom = docViewTop + $(window).height();

    var elemTop = elem.offset().top;
    var elemBottom = elemTop + elem.height();

    return ((elemBottom <= docViewBottom && elemBottom >= docViewTop) || (elemTop <= docViewBottom && elemTop >= docViewTop));
  },

  load_carousel_details: function (carouselid) {
    $.ajax({
      cache: false,
      type: 'POST',
      data: { carouselId: carouselid },
      url: SmartCarousel.carouseldetailsurl,
      success: function (response) {
        var currentElem = $(SmartCarousel.containerselector + '[data-carouselid="' + carouselid + '"]');
        if (response.result) {
          currentElem.html(response.html);;
        }
        else {
          currentElem.html(SmartCarousel.localized_data.SmartCarouselFailure);
        }
        currentElem.attr('data-loaded', true);
        currentElem.removeClass('carousel-container');
        currentElem.removeAttr('data-loading');
      },
      error: SmartCarousel.ajaxFailure
    });
  },

  ajaxFailure: function () {
    $(SmartCarousel.containerselector).html(SmartCarousel.localized_data.SmartCarouselFailure);
  }
};