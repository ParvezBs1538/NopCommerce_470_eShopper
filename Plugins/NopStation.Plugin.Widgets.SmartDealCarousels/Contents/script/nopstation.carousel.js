
var SmartDealCarousel = {
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

    SmartDealCarousel.check_carousels();

    $(window).scroll(function () {
      if (!SmartDealCarousel.loadwait) {
        SmartDealCarousel.check_carousels();
      }
    });
  },

  check_carousels: function () {
    $(SmartDealCarousel.containerselector + '[data-loaded!="true"]').each(function () {
      var elem = $(this);
      if (SmartDealCarousel.chek_element_on_screen(elem)) {
        if (!elem.data('loading')) {
          elem.attr('data-loading', true);
          var carouselid = elem.data('dealcarouselid');
          SmartDealCarousel.load_carousel_details(carouselid);
        }
      }
    })

    SmartDealCarousel.loadwait = false;
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
      url: SmartDealCarousel.carouseldetailsurl,
      success: function (response) {
        var currentElem = $(SmartDealCarousel.containerselector + '[data-dealcarouselid="' + carouselid + '"]');
        if (response.result) {
          currentElem.html(response.html);;
        }
        else {
          currentElem.html(SmartDealCarousel.localized_data.SmartDealCarouselFailure);
        }
        currentElem.attr('data-loaded', true);
        currentElem.removeClass('carousel-container');
        currentElem.removeAttr('data-loading');
      },
      error: SmartDealCarousel.ajaxFailure
    });
  },

  ajaxFailure: function () {
    $(SmartDealCarousel.containerselector).html(SmartDealCarousel.localized_data.SmartDealCarouselFailure);
  }
};