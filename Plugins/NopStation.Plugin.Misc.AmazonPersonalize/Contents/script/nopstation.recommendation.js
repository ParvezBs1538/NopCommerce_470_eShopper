
var Recommendation = {
  recommendationdetailsurl: '',
  containerselector: '',
  loaderselector: '',
  loadwait: true,
  localized_data: false,

  init: function (recommendationdetailsurl, containerselector, loaderselector, localized_data) {
    this.recommendationdetailsurl = recommendationdetailsurl;
    this.containerselector = containerselector;
    this.loaderselector = loaderselector;
    this.localized_data = localized_data;
    this.loadwait = true;

    Recommendation.check_recommendations();

    $(window).scroll(function () {
      if (!Recommendation.loadwait) {
        Recommendation.check_recommendations();
      }
    });
  },

  check_recommendations: function () {
    $(Recommendation.containerselector + '[data-loaded!="true"]').each(function () {
      var elem = $(this);
      if (Recommendation.chek_element_on_screen(elem)) {
        if (!elem.data('loading')) {
          elem.attr('data-loading', true);
          var recommendationlid = elem.data('recommendationid');
          var productid = elem.data('viewedproductid');
          Recommendation.load_recommendation_details(recommendationlid, productid);
        }
      }
    })

    Recommendation.loadwait = false;
  },

  chek_element_on_screen: function (elem) {
    var docViewTop = $(window).scrollTop();
    var docViewBottom = docViewTop + $(window).height();

    var elemTop = elem.offset().top;
    var elemBottom = elemTop + elem.height();

    return ((elemBottom <= docViewBottom && elemBottom >= docViewTop) || (elemTop <= docViewBottom && elemTop >= docViewTop));
  },

  load_recommendation_details: function (recommendationlid, productid) {
    $.ajax({
      cache: false,
      type: 'POST',
      data: { recommendationId: recommendationlid, productId: productid },
      url: Recommendation.recommendationdetailsurl,
      success: function (response) {
        var currentElem = $(Recommendation.containerselector + '[data-recommendationid="' + recommendationlid + '"]');
        if (response.result) {
          currentElem.html(response.html);;
        }
        else {
          response.message != '' ? currentElem.html(response.message) : currentElem.html(Recommendation.localized_data.RecommendationFailure);
        }
        currentElem.attr('data-loaded', true);
        currentElem.removeClass('recommendation-container');
        currentElem.removeAttr('data-loading');
      },
      error: Recommendation.ajaxFailure
    });
  },

  ajaxFailure: function () {
    $(Recommendation.containerselector).html(Recommendation.localized_data.RecommendationFailure);
  }
};