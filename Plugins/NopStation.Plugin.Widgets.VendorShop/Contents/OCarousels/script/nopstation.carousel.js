
var Carousel = {
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

    Carousel.check_carousels();

    $(window).scroll(function () {
      if (!Carousel.loadwait) {
        Carousel.check_carousels();
      }
    });
  },

  check_carousels: function () {
    $(Carousel.containerselector + '[data-loaded!="true"]').each(function () {
      var elem = $(this);
      if (Carousel.chek_element_on_screen(elem)) {
        if (!elem.data('loading')) {
          elem.attr('data-loading', true);
          var carouselid = elem.data('carouselid');
          Carousel.load_carousel_details(carouselid);
        }
      }
    })

    Carousel.loadwait = false;
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
      url: Carousel.carouseldetailsurl,
      success: function (response) {
        var currentElem = $(Carousel.containerselector + '[data-carouselid="' + carouselid + '"]');
        if (response.result) {
          if (response.html == "\r\n\r\n") {
            currentElem.parents(".ocarousel-grid").css("display", "none");
          }
          else {
            currentElem.html(response.html);
          }
        }
        else {
          currentElem.html(Carousel.localized_data.CarouselFailure);
        }
        currentElem.attr('data-loaded', true);
        currentElem.removeClass('carousel-container');
        currentElem.removeAttr('data-loading');
      },
      error: Carousel.ajaxFailure
    });
  },

  ajaxFailure: function () {
    $(Carousel.containerselector).html(Carousel.localized_data.CarouselFailure);
  }
};