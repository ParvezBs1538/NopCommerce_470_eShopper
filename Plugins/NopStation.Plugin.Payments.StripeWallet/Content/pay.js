var StripeWallet = {
  stdOrderTotal: 0,
  stdCurrency: '',
  stdCountry: '',
  publishableKey: '',
  apiVersion: '2020-08-27',
  createPaymentIntentUrl: '',

  init: async function (stdOrderTotal, stdCurrency, stdCountry, publishableKey, createPaymentIntentUrl) {
    this.stdOrderTotal = stdOrderTotal;
    this.stdCurrency = stdCurrency;
    this.stdCountry = stdCountry;
    this.publishableKey = publishableKey;
    this.createPaymentIntentUrl = createPaymentIntentUrl;
    if (this.validCountry()) {
      await this.createPaymentIntent();

    } else {
      this.hideLoaderAndShowBtn('Your country does not support stripe digital wallets');
    }
  },

  hideLoaderAndShowBtn: function (msz) {
    $("#loader-st").hide();
    $('#payment-request-button').show();
    //$('#payment-request-button').html('Your country does not support stripe digital wallets');
    if (msz != null) {
      $('#payment-request-button').html(msz);
    }
  },

  createPaymentIntent: async function () {
    const stripe = Stripe(this.publishableKey, {
      apiVersion: this.apiVersion,
    });

    var paymentRequest = stripe.paymentRequest({
      country: this.stdCountry,
      currency: this.stdCurrency,
      total: {
        label: 'Order total',
        amount: this.stdOrderTotal,
      },
      requestPayerName: true,
      requestPayerEmail: true
    });

    const elements = stripe.elements();
    const prButton = elements.create('paymentRequestButton', {
      paymentRequest: paymentRequest,
    });

    paymentRequest.canMakePayment().then(function (result) {
      var msz = null;
      if (result) {
        prButton.mount('#payment-request-button');
      } else {
        msz = this.getSpecificBrowserError();
      }
      StripeWallet.hideLoaderAndShowBtn(msz);
    });

    paymentRequest.on('paymentmethod', async (e) => {
      // Make a call to the server to create a new
      // payment intent and store its client_secret.
      const { error: backendError, clientSecret, result } = await fetch(
        this.createPaymentIntentUrl,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
        }
      ).then((r) => r.json());

      if (backendError || result == "fail") {
        e.complete('fail');
        displayPopupNotification(result);
        return;
      }

      // Confirm the PaymentIntent without handling potential next actions (yet).
      await stripe.confirmCardPayment(clientSecret, { payment_method: e.paymentMethod.id, }, { handleActions: false, }).then(async function (response) {
        if (response.error) {
          (response.error.message);
          e.complete('fail');
          return;
        } else if (response.paymentIntent.status === 'requires_action') {
          let { error, paymentIntent } = await stripe.confirmCardPayment(
            clientSecret
          );
          if (error) {
            (error.message);
            return;
          }
        } else if (response.paymentIntent && response.paymentIntent.status === 'requires_capture') {
          StripeWallet.setPaymentIntent(response);
          e.complete('success');
          $(".payment-info-next-step-button").trigger("click");
        }
      });
    });
  },

  currentBrowser: function () {
    return (function (agent) {
      switch (true) {
        case agent.indexOf("edge") > -1: return "ms";
        case agent.indexOf("edg") > -1: return "ms";
        case agent.indexOf("opr") > -1 && !!window.opr: return "opera";
        case agent.indexOf("chrome") > -1 && !!window.chrome: return "chrome";
        case agent.indexOf("trident") > -1: return "Internet Explorer";
        case agent.indexOf("firefox") > -1: return "firefox";
        case agent.indexOf("safari") > -1: return "safari";
        default: return "other";
      }
    })(window.navigator.userAgent.toLowerCase());
  },

  getSpecificBrowserError: function () {
    var browser = this.currentBrowser();
    if (browser == "safari") {
      msz = 'Please add a payment method for apple pay from <a href="https://apps.apple.com/account/billing">here</a>';
    }
    else if (browser == "ms") {
      msz = 'Please add a payment method for microsoft pay from <a href="https://account.microsoft.com/billing/payments">here</a> ';
    }
    else if (browser == "chrome") {
      msz = 'Please add a payment method for gpay from <a href="https://pay.google.com/gp/w/u/0/home/paymentmethods">here</a> .';
    }
    else {
      msz = 'Your browser does not support stripe digital wallets';
    }

    return msz;
  },

  setPaymentIntent: function (response) {
    $("#PaymentIntentId").val(response.paymentIntent.id);
    $("#PaymentIntentStatus").val(response.paymentIntent.status);
  },

  validCountry: function () {
    var countryList = ["AE", "AT", "AU", "BE", "BG", "BR", "CA", "CH", "CI", "CR", "CY", "CZ", "DE", "DK", "DO", "EE", "ES", "FI", "FR", "GB", "GI", "GR", "GT", "HK", "HU", "ID", "IE", "IN", "IT", "JP", "LI", "LT", "LU", "LV", "MT", "MX", "MY", "NL", "NO", "NZ", "PE", "PH", "PL", "PT", "RO", "SE", "SG", "SI", "SK", "SN", "TH", "TT", "US", "UY"];
    return countryList.includes(this.stdCountry);
  },
}


