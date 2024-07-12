
var SmartSlider = {
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

    SmartSlider.check_sliders();

    $(window).scroll(function () {
      if (!SmartSlider.loadwait) {
        SmartSlider.check_sliders();
      }
    });
  },

  check_sliders: function () {
    $(SmartSlider.containerselector + '[data-loaded!="true"]').each(function () {
      var elem = $(this);
      if (SmartSlider.chek_element_on_screen(elem)) {
        if (!elem.data('loading')) {
          elem.attr('data-loading', true);
          var sliderid = elem.data('sliderid');
          SmartSlider.load_slider_details(sliderid);
        }
      }
    });

    SmartSlider.loadwait = false;
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
      url: SmartSlider.sliderdetailsurl,
      success: function (response) {
        var currentElem = $(SmartSlider.containerselector + '[data-sliderid="' + sliderid + '"]');
        if (response.result) {
          currentElem.html(response.html);;
        }
        else {
          currentElem.html(SmartSlider.localized_data.SmartSliderFailure);
        }
        currentElem.attr('data-loaded', true);
      },
      error: SmartSlider.ajaxFailure
    });
  },

  ajaxFailure: function () {
    $(SmartSlider.containerselector).html(SmartSlider.localized_data.SmartSliderFailure);
  }
};