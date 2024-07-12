var StripePaymentElement = {
  stdOrderTotal: 0,
  stdCurrency: '',
  publishableKey: '',
  createPaymentIntentUrl: '',
  theme: '',
  layout: '',
  emailAddress: '',
  completedPageUrl: '',
  elements: null,
  stripe: null,
  billingAddress: null,
  shippingAddress: null,

  init: async function (stdOrderTotal, stdCurrency, publishableKey, createPaymentIntentUrl, completedPageUrl, theme, layout, billing, shipping) {
    this.stdOrderTotal = stdOrderTotal;
    this.stdCurrency = stdCurrency;
    this.publishableKey = publishableKey;
    this.createPaymentIntentUrl = createPaymentIntentUrl;
    this.theme = theme;
    this.layout = layout;
    this.completedPageUrl = completedPageUrl;
    this.billingAddress = billing;
    this.shippingAddress = shipping;
    this.emailAddress = billing.email;
    this.createPaymentIntent();
    $('#payment-element-intent').closest('form').on('submit', this.handleSubmit.bind(this));
  },

  handleSubmit: async function (e) {
    e?.preventDefault();
    this.showLoader(true);

    const { error } = await this.stripe.confirmPayment({
      elements: this.elements,
      confirmParams: {
        return_url: new URL(this.completedPageUrl, window.location.origin).toString(),
        receipt_email: this.emailAddress,
        shipping: this.shippingAddress,
        payment_method_data: {
          billing_details: this.billingAddress
        }
      }
    });

    // This point will only be reached if there is an immediate error when
    // confirming the payment. Otherwise, your customer will be redirected to
    // your `return_url`. For some payment methods like iDEAL, your customer will
    // be redirected to an intermediate site first to authorize the payment, then
    // redirected to the `return_url`.
    if (error) {
      showMessage(error.message);
    }

    this.showLoader(false);
  },

  showLoader: function (show = true) {
    if (show) {
      $('body').append(`<div class="spe-loader"></div>`);
    }
    else {
      $('.spe-loader').remove();
    }
  },

  createPaymentIntent: async function () {
    this.stripe = Stripe(this.publishableKey);

    const request = await fetch(this.createPaymentIntentUrl, {
      method: 'POST',
      headers: { "Content-Type": "application/json" },
    });

    const { clientSecret } = await request.json();

    const appearance = {
      theme: this.theme,
    };
    this.elements = this.stripe.elements({ appearance, clientSecret });

    const paymentElementOptions = {
      layout: this.layout,
      fields: {
        billingDetails: 'never'
      },
      wallets: {
        applePay: 'never',
        googlePay: 'never',
      },
      defaultValues: {
        billingDetails: this.billingAddress
      },
    };

    const paymentElement = this.elements.create("payment", paymentElementOptions);
    paymentElement.mount("#payment-element");
    this.showLoader(false);
    if (typeof PaymentInfo !== 'undefined') {
      const tmpFn = PaymentInfo.save;
      PaymentInfo.save = () => {
        if ($('#payment-element').length)
          this.handleSubmit();
        else
          tmpFn();
      }
    }
  },
  setPaymentIntent: function (response) {
    $("#PaymentIntentId").val(response.paymentIntent.id);
    $("#PaymentIntentStatus").val(response.paymentIntent.status);
  },
}

function showMessage(messageText) {
  const messageContainer = document.querySelector("#payment-message");

  messageContainer.classList.remove("hidden");
  messageContainer.textContent = messageText;

  setTimeout(function () {
    messageContainer.classList.add("hidden");
    messageText.textContent = "";
  }, 10000);
}
