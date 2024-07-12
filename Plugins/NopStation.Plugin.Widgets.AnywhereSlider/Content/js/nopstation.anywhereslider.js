
var AnywereSlider = {
  sliderdetailsurl: '',
  containerselector: '',
  loaderselector: '',
  loadwait: true,
  localized_data: false,

  init: function (sliderdetailsurl, containerselector, loaderselector, localized_data) {
    this.sliderdetailsurl = sliderdetailsurl;
    this.containerselector = containerselector;
    this.loaderselector = loaderselector;
    this.localized_data = localized_data;
    this.loadwait = true;

    AnywereSlider.check_sliders();

    $(window).scroll(function () {
      if (!AnywereSlider.loadwait) {
        AnywereSlider.check_sliders();
      }
    });
  },

  check_sliders: function () {
    $(AnywereSlider.containerselector + '[data-loaded!="true"]').each(function () {
      var elem = $(this);
      if (AnywereSlider.chek_element_on_screen(elem)) {
        if (!elem.data('loading')) {
          elem.attr('data-loading', true);
          var sliderid = elem.data('sliderid');
          AnywereSlider.load_slider_details(sliderid);
        }
      }
    });

    AnywereSlider.loadwait = false;
  },

  chek_element_on_screen: function (elem) {
    var docViewTop = $(window).scrollTop();
    var docViewBottom = docViewTop + $(window).height();

    var elemTop = elem.offset().top;
    var elemBottom = elemTop + elem.height();

    return ((elemBottom <= docViewBottom && elemBottom >= docViewTop) || (elemTop <= docViewBottom && elemTop >= docViewTop));
  },

  load_slider_details: function (sliderid) {
    $.ajax({
      cache: false,
      type: 'POST',
      data: { sliderId: sliderid },
      url: AnywereSlider.sliderdetailsurl,
      success: function (response) {
        var currentElem = $(AnywereSlider.containerselector + '[data-sliderid="' + sliderid + '"]');
        if (response.result) {
          currentElem.html(response.html);;
        }
        else {
          currentElem.html(AnywereSlider.localized_data.AnywereSliderFailure);
        }
        currentElem.attr('data-loaded', true);
      },
      error: AnywereSlider.ajaxFailure
    });
  },

  ajaxFailure: function () {
    $(AnywereSlider.containerselector).html(AnywereSlider.localized_data.AnywereSliderFailure);
  }
};