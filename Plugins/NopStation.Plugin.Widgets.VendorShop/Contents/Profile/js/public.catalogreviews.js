var CatalogReviews = {
  settings: {
    ajax: true,
    fetchUrl: false,
    browserPath: false,
  },

  params: {
    jqXHR: false,
  },

  init: function (settings) {
    this.settings = $.extend({}, this.settings, settings);
  },

  getReviews: function (pageNumber) {
    if (this.params.jqXHR && this.params.jqXHR.readyState !== 4) {
      this.params.jqXHR.abort();
    }

    var urlBuilder = createReviewsURLBuilder(this.settings.browserPath);

    if (pageNumber) {
      urlBuilder.addParameter('pagenumber', pageNumber);
    }

    var beforePayload = {
      urlBuilder
    };
    $(this).trigger({ type: "before", payload: beforePayload });

    this.setBrowserHistory(urlBuilder.build());

    if (!this.settings.ajax) {
      setLocation(urlBuilder.build());
    } else {
      this.setLoadWaiting(1);

      var self = this;
      this.params.jqXHR = $.ajax({
        cache: false,
        url: urlBuilder.addBasePath(this.settings.fetchUrl).build(),
        type: 'GET',
        success: function (response) {
          $('.reviews-wrapper').html(response);
          $('html, body').animate({ scrollTop: $('#vendor-shop-tabs').offset().top }, 'slow');
          $(self).trigger({ type: "loaded" });
        },
        error: function () {
          $(self).trigger({ type: "error" });
        },
        complete: function () {
          self.setLoadWaiting();
        }
      });
    }
  },

  setLoadWaiting(enable) {
    var $busyEl = $('.ajax-reviews-busy');
    if (enable) {
      $busyEl.show();
    } else {
      $busyEl.hide();
    }
  },

  setBrowserHistory(url) {
    window.history.replaceState({ path: url }, '', url);
  }
}

function createReviewsURLBuilder(basePath) {
  return {
    params: {
      basePath: basePath,
      query: {}
    },

    addBasePath: function (url) {
      this.params.basePath = url;
      return this;
    },

    addParameter: function (name, value) {
      this.params.query[name] = value;
      return this;
    },

    build: function () {
      var query = $.param(this.params.query);
      var url = this.params.basePath;

      return url.indexOf('?') !== -1
        ? url + '&' + query
        : url + '?' + query;
    }
  }
}