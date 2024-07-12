$(document).ready(function () {
  var heidelpay = new heidelpay('s-pub-2a10ifVINFAjpQJ9qW8jBe5OJPBx6Gxa');
  // Use Card payment type
  var Card = heidelpay.Card();
  // Render the card number input field on #card-element-id-number
  Card.create('number', {
    containerId: 'card-element-id-number',
    onlyIframe: false
  });
  // Render the card expiry input field on #card-element-id-expiry
  Card.create('expiry', {
    containerId: 'card-element-id-expiry',
    onlyIframe: false
  });
  // Render the card cvc input field on #card-element-id-cvc
  Card.create('cvc', {
    containerId: 'card-element-id-cvc',
    onlyIframe: false
  });
  var payForm = $("#HandleForPaymentForm").closest("form");
  //var paymentForm = document.getElementById('co-payment-info-form');
  payForm.attr('id', 'payment-form');
  payForm.addClass("heidelpayUI form");
  payForm.attr('novalidate', 'novalidate');
  var paymentForm = document.getElementById('payment-form');
  var paymentButton = document.getElementById('payment-button-id');
  var paymentFields = {};

  //card events handling
  Card.addEventListener('change', function (e) {
    paymentFields[e.type] = e.success;
    paymentButton.disabled = !(paymentFields.number && paymentFields.expiry && paymentFields.cvc);
  });

  // Handle the form submission.
  paymentForm.addEventListener('submit', function (e) {
    e.preventDefault();
    // TODO: Prevent further payment form submissions

    // create payment resource using the entered data
    Card.createResource()
      .then(function (data) {
        // TODO: Successful resource creation: submit the id to the server
        console.log('ResourceID: ' + data.id);
      })
      .catch(function (error) {
        // TODO: Handle error processing
        console.log(error);
      });
  });
});