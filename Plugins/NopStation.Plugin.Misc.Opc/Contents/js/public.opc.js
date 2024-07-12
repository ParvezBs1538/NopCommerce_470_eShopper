var Checkout = {
  loadWaiting: false,
  failureUrl: false,
  localized_data: false,
  settings: false,

  init: function (failureUrl, localized_data, settings) {
    this.loadWaiting = false;
    this.failureUrl = failureUrl;
    this.localized_data = localized_data;
    this.settings = settings;
  },

  ajaxFailure: function () {
    /*location.href = Checkout.failureUrl;*/
    alert(Checkout.localized_data.CommonError);
  }
};

var Billing = {
  updateAddressUrl: false,
  saveAddressUrl: false,
  editAddressUrl: false,
  selectedStateId: 0,
  selector: false,

  init: function (updateAddressUrl, saveAddressUrl, editAddressUrl, selector) {
    this.updateAddressUrl = updateAddressUrl;
    this.saveAddressUrl = saveAddressUrl;
    this.editAddressUrl = editAddressUrl;
    this.selector = selector;
  },
  newAddress: function (event) {

    var selctedBillingAddressId = event.options[event.selectedIndex].value;
    var option_text = event.options[event.selectedIndex].text;
    var billingAddressCountryId = event.options[event.selectedIndex].getAttribute('data-country');

    var isShipToSameAddress = $(Billing.selector.shipToSameAddressSelector).is(':checked');


    if (selctedBillingAddressId === undefined || selctedBillingAddressId === null || selctedBillingAddressId === '') {
      Billing.setBillingAddress(0).then((data) => {
        Billing.editAddress(0, "billing", true, isShipToSameAddress).then((data) => {
          PaymentMethod.loadPaymentMethods($(Billing.selector.bililngNewAddressCountrySelector).val()).then((data) => {
            if ($(Billing.selector.shipToSameAddressSelector).is(":checked")) {
              Shipping.setShippingAddress(0).then((data) => {
                ShippingMethod.loadShippingMethod().then((data) => {
                  ConfirmOrder.loadOrderTotal();
                })
              });
            } else {
              ConfirmOrder.loadOrderTotal();
            }
          });
        })
      }).catch((error) => {
        alert(error);
      });
    } else {
      Billing.setBillingAddress(selctedBillingAddressId).then((data) => {
        Billing.editAddress(selctedBillingAddressId, "billing", false, isShipToSameAddress).then((data) => {

          PaymentMethod.loadPaymentMethods(billingAddressCountryId).then((data) => {
            ConfirmOrder.loadOrderTotal();
          });
        })
      })
        .catch((error) => {
          alert(error);
        });

      //if ship to same address
      if ($(Billing.selector.billingAddressSelectSelector).val().length != 0 && $(Billing.selector.shipToSameAddressSelector).is(":checked")) {
        ShippingMethod.loadShippingMethodByAddress($(Billing.selector.billingAddressSelectSelector).val()).then((data) => {
          ConfirmOrder.loadOrderTotal();
        }).catch((error) => {
          alert(error);
        });
      }
    }

  },
  editAddress: function (addressId, addressType, isNew, shipToSameAddress) {

    return new Promise((resolve, reject) => {
      $.ajax({
        cache: false,
        type: "GET",
        url: Billing.editAddressUrl,
        data: {
          addressId: addressId,
          addressType: addressType,
          isNew: isNew,
          isShipToSameAddress: shipToSameAddress
        },
        success: function (data, textStatus, jqXHR) {
          if (!data.error) {
            $(Billing.selector.billingAddressSelector).html(data.html);
          }
          resolve(data);
          Billing.initializeCountrySelect();
        },
        complete: function (jqXHR, textStatus) {
          $("#billing-new-address-form").show();
        },
        error: function (error) {
          reject(error);
        },
      })
    });
  },
  setSelectedStateId: function (id) {
    this.selectedStateId = id;
  },
  resetBillingForm: function () {
    $(':input', '#billing-new-address-form')
      .not(':button, :submit, :reset, :hidden')
      .removeAttr('checked').removeAttr('selected')
    $(':input', '#billing-new-address-form')
      .not(':checkbox, :radio, select')
      .val('');

    $('.address-id', '#billing-new-address-form').val('0');
    $('select option[value="0"]', '#billing-new-address-form').prop('selected', true);
  },
  resetSelectedAddress: function () {
    var selectElement = $(Billing.selector.billingAddressSelectSelector);
    if (selectElement) {
      selectElement.val('');
    }
  },
  toggleShippingAddress: function (shipToSameAddress) {
    if (shipToSameAddress.checked) {
      if ($(Billing.selector.billingAddressSelectSelector).val().length != 0) {
        Shipping.setShippingAddress($(Billing.selector.billingAddressSelectSelector).val()).then((data) => {
          ShippingMethod.loadShippingMethodByAddress($(Billing.selector.billingAddressSelectSelector).val()).then((data) => {
            ConfirmOrder.loadOrderTotal();

          })
        })
          .catch((error) => {
            alert(error.statusText);
          });
      }
      else {
        Shipping.setShippingAddress(0).then((data) => {
          ShippingMethod.loadShippingMethod().then((data) => {
            ConfirmOrder.loadOrderTotal();
          })
        })
          .catch((error) => {
            alert(error.statusText);
          });
      }
      $('#checkout-step-shipping').hide();

    } else {
      $('#checkout-step-shipping').show();
      $(document).trigger({
        type: "shiptosame_address_changed",
        changedData: false
      });
    }
  },
  modifyAddress: function () {

    var data = $(Billing.selector.billingFormSelector).serializeArray();
    data.push({
      name: 'addressType',
      value: 'billing'
    });

    return new Promise((resolve, reject) => {
      $.ajax({
        cache: false,
        url: this.updateAddressUrl,
        data: data,
        type: 'post',
        success: function (response, textStatus, jqXHR) {
          resolve(response);
        },
        error: function (error) {
          reject(error);
        },
      });
    })
  },
  setBillingAddress: function (billingAddressId) {
    return new Promise((resolve, reject) => {
      $.ajax({
        cache: false,
        url: this.saveAddressUrl,
        data: {
          billingAddressId: billingAddressId
        },
        type: 'post',
        success: function (response, textStatus, jqXHR) {
          resolve(response);
        },
        error: function (error) {
          reject(error);
        }
      })
    });
  },
  initializeCountrySelect: function () {
    if ($('#checkout-step-billing').has('select[data-trigger="opc-country-select"]')) {
      $('#checkout-step-billing select[data-trigger="opc-country-select"]').countrySelect();
    }
  }
};
var Shipping = {
  updateShippingAddressUrl: false,
  saveAddressUrl: false,
  editAddressUrl: false,
  selectedStateId: 0,
  selector: false,

  init: function (updateShippingAddressUrl, saveAddressUrl, editAddressUrl, selector) {
    this.updateShippingAddressUrl = updateShippingAddressUrl;
    this.saveAddressUrl = saveAddressUrl;
    this.editAddressUrl = editAddressUrl;
    this.selector = selector;
  },
  newAddress: function (event) {

    var selctedShippingAddressId = event.options[event.selectedIndex].value;

    if (selctedShippingAddressId < 1) {
      Shipping.setShippingAddress(0).then((data) => {
        Shipping.editAddress(0, "shipping", true).then((data) => {
          ShippingMethod.loadShippingMethod().then((data) => {
            ConfirmOrder.loadOrderTotal();
          });

        })
      })
        .catch((error) => {
          alert(error);
        });
    } else {

      Shipping.setShippingAddress(selctedShippingAddressId).then((data) => {
        Shipping.editAddress(selctedShippingAddressId, "shipping", false).then((data) => {
          ShippingMethod.loadShippingMethodByAddress(selctedShippingAddressId).then((data) => {
            ConfirmOrder.loadOrderTotal();
          });

        })
      })
        .catch((error) => {
          alert(error);
        });

    }

  },
  editAddress: function (addressId, addressType, isNew) {

    return new Promise((resolve, reject) => {
      $.ajax({
        cache: false,
        type: "GET",
        url: Shipping.editAddressUrl,
        data: {
          addressId: addressId,
          addressType: addressType,
          isNew: isNew
        },
        success: function (data, textStatus, jqXHR) {
          if (!data.error) {
            $(Shipping.selector.shippingAddressSelector).html(data.html);

          }
          resolve(data);
          Shipping.initializeCountrySelect();
        },
        complete: function (jqXHR, textStatus) {
          $("#shipping-new-address-form").show();
        },
        error: function (error) {
          reject(error);
        },
      })
    });
  },
  setSelectedStateId: function (id) {
    this.selectedStateId = id;
  },
  resetShippingForm: function () {
    $(':input', '#shipping-new-address-form')
      .not(':button, :submit, :reset, :hidden')
      .removeAttr('checked').removeAttr('selected')
    $(':input', '#shipping-new-address-form')
      .not(':checkbox, :radio, select')
      .val('');

    $('.address-id', '#shipping-new-address-form').val('0');
    $('select option[value="0"]', '#shipping-new-address-form').prop('selected', true);
  },
  togglePickUpInStore: function (pickupInStoreInput, onlyone) {
    if (pickupInStoreInput.checked) {
      $('#pickup-points-form').show();
      $('#shipping-addresses-form').hide();
      $('#ship-to-same-address').hide();
      //hide shipping method if pickup checked
      $('#checkout-step-shipping-method').hide();
      if (onlyone === 1) {
        ShippingMethod.setPickupInStore(true, $('#pickup-points-id').val()).then((data) => {
          ConfirmOrder.loadOrderTotal();
        }).catch((error) => {
          alert(error);
        });
      } else {
        ShippingMethod.setPickupInStore(true, $('#pickup-points-select').val()).then((data) => {
          ConfirmOrder.loadOrderTotal();
        }).catch((error) => {
          alert(error);
        });
      }
    } else {
      $('#pickup-points-form').hide();
      $('#shipping-addresses-form').show();
      $('#checkout-step-shipping-method').show();
      $('#ship-to-same-address').show();
      if (onlyone === 1) {
        ShippingMethod.setPickupInStore(false, $('#pickup-points-id').val()).then((data) => {
          ShippingMethod.loadShippingMethod().then((data) => {
            ConfirmOrder.loadOrderTotal();
          });
        }).catch((error) => {
          alert(error);
        });
      } else {
        ShippingMethod.setPickupInStore(false, $('#pickup-points-select').val()).then((data) => {
          ShippingMethod.loadShippingMethod().then((data) => {
            ConfirmOrder.loadOrderTotal();
          });
        }).catch((error) => {
          alert(error);
        });
      }
    }
  },
  resetSelectedAddress: function () {
    var selectElement = $(Shipping.selector.shippingAddressSelectSelector);
    if (selectElement) {
      selectElement.val('');
    }
  },
  modifyAddress: function () {

    var data = $(Shipping.selector.shippingFormSelector).serializeArray();
    data.push({
      name: 'addressType',
      value: 'shipping'
    });

    return new Promise((resolve, reject) => {
      $.ajax({
        cache: false,
        url: this.updateShippingAddressUrl,
        data: data,
        type: 'post',
        success: function (response, textStatus, jqXHR) {
          resolve(response);
        },
        error: function (error) {
          reject(error);
        },
      });
    })
  },
  setShippingAddress: function (shippingAddressId) {
    return new Promise((resolve, reject) => {
      $.ajax({
        cache: false,
        url: this.saveAddressUrl,
        data: {
          addressId: shippingAddressId
        },
        type: 'post',
        success: function (response, textStatus, jqXHR) {
          if (response.error) {
            alert(response.message);
          }
          else {
            resolve(response);
          }
        },
        error: function (error) {
          reject(error);
        }
      })
    });
  },
  initializeCountrySelect: function () {
    if ($('#checkout-step-shipping').has('select[data-trigger="opc-country-select"]')) {
      console.log('abcc country select');
      $('#checkout-step-shipping select[data-trigger="opc-country-select"]').countrySelect();
    }
  }
};
var ShippingMethod = {

  loadShippingMethodUrl: false,
  loadShippingMethodByAddressUrl: false,
  updateShippingMethodUrl: false,
  setPicupInStoreUrl: false,
  selector: false,
  init: function (loadShippingMethodUrl, loadShippingMethodByAddressUrl, updateShippingMethodUrl, setPicupInStoreUrl, selector) {
    this.loadShippingMethodUrl = loadShippingMethodUrl;
    this.loadShippingMethodByAddressUrl = loadShippingMethodByAddressUrl;
    this.updateShippingMethodUrl = updateShippingMethodUrl;
    this.setPicupInStoreUrl = setPicupInStoreUrl;
    this.selector = selector;
  },
  loadShippingMethod: function () {
    return new Promise((resolve, reject) => {
      $.ajax({
        cache: false,
        url: this.loadShippingMethodUrl,
        type: 'get',
        success: function (response, textStatus, jqXHR) {
          if (response.error) {
            alert(response.message);
          } else {
            if (response.update_section != undefined) {
              if (response.count != 0) {
                $('#checkout-' + response.update_section.name + '-load').html(response.update_section.html);
              }
            }
            resolve(response);
          }
        },
        error: function (error) {
          reject(error);
        }
      })
    });
  },
  loadShippingMethodByAddress: function (addressId) {

    return new Promise((resolve, reject) => {
      $.ajax({
        cache: false,
        url: this.loadShippingMethodByAddressUrl,
        data: {
          addressId: addressId
        },
        type: 'get',
        success: function (response, textStatus, jqXHR) {
          if (response.error) {
            alert(response.message);
          } else {
            if (response.update_section != undefined) {
              if (response.count != 0) {
                $('#checkout-' + response.update_section.name + '-load').html(response.update_section.html);
                if (response.update_section.shippingrequired != undefined && !response.update_section.shippingrequired) {
                  $('#checkout-step-shipping').hide();
                  $('#checkout-step-shipping-method').hide();
                  $('#ship-to-same-address').hide();
                }
              }

            }
            resolve(response);
          }
        },
        error: function (error) {
          reject(error);
        }
      })
    });
  },
  validate: function () {
    var methods = document.getElementsByName('shippingoption');
    if (methods.length == 0) {
      alert('Your order cannot be completed at this time as there is no shipping methods available for it. Please make necessary changes in your shipping address.');
      return false;
    }

    for (var i = 0; i < methods.length; i++) {
      if (methods[i].checked) {
        return true;
      }
    }
    alert('Please specify shipping method.');
    return false;
  },
  updateShippingMethod(shippingMethodName, shippingMethodSystemName) {
    var model = {
      SelectedName: shippingMethodName,
      ShippingMethodSystemName: shippingMethodSystemName
    };

    return new Promise((resolve, reject) => {
      $.ajax({
        cache: false,
        url: this.updateShippingMethodUrl,
        data: model,
        type: 'post',
        success: function (response, textStatus, jqXHR) {
          if (response.error) {
            alert(response.message);
          } else {
            resolve(response);
          }
        },
        error: function (error) {
          reject(error);
        }
      })
    });
  },
  setPickupInStore: function (isSelected, pickupPoint) {
    var model = {
      PickUpInStore: isSelected,
      PickUpPoint: pickupPoint
    };
    return new Promise((resolve, reject) => {
      $.ajax({
        cache: false,
        url: this.setPicupInStoreUrl,
        data: model,
        type: 'post',
        success: function (response, textStatus, jqXHR) {
          resolve(response);
        },
        error: function (error) {
          reject(error);
        }
      })
    });
  }
};

var PaymentMethod = {
  getPaymentMethodUrl: false,
  getPaymentMethodByAddressUrl: false,
  useRewardPointsUrl: false,
  updatePaymentUrl: false,
  loadPaymentInfoUrl: false,
  selector: false,

  init: function (getPaymentMethodUrl, getPaymentMethodByAddressUrl, useRewardPointsUrl, updatePaymentUrl, loadPaymentInfoUrl, selector) {
    this.getPaymentMethodUrl = getPaymentMethodUrl;
    this.getPaymentMethodByAddressUrl = getPaymentMethodByAddressUrl;
    this.useRewardPointsUrl = useRewardPointsUrl;
    this.updatePaymentUrl = updatePaymentUrl;
    this.loadPaymentInfoUrl = loadPaymentInfoUrl;
    this.selector = selector;
  },
  loadPaymentMethods: function (countryId) {
    return new Promise((resolve, reject) => {
      $.ajax({
        cache: false,
        url: this.getPaymentMethodUrl,
        data: {
          countryId: countryId
        },
        type: 'get',
        success: function (response, textStatus, jqXHR) {
          if (response.error) {
            alert(response.message);
          } else {
            if (response.update_section != undefined) {
              if (response.count != 0) {
                $('#checkout-' + response.update_section.name + '-load').html(response.update_section.html);
              }
            }
            resolve(response);
          }
        },
        error: function (error) {
          reject(error);
        }
      })
    });
  },
  loadPaymentMethodsByAddress: function (addressId) {
    return new Promise((resolve, reject) => {
      $.ajax({
        cache: false,
        url: this.getPaymentMethodByAddressUrl,
        data: {
          addressId: addressId
        },
        type: 'get',
        success: function (response, textStatus, jqXHR) {
          if (response.error) {
            alert(response.message);
          } else {
            if (response.update_section != undefined) {
              if (response.count != 0) {
                $('#checkout-' + response.update_section.name + '-load').html(response.update_section.html);
              }
            }
            resolve(response);
          }
        },
        error: function (error) {
          reject(error);
        },
      })
    });
  },
  toggleUseRewardPoints: function (useRewardPointsInput) {
    if (useRewardPointsInput.checked) {
      var model = {
        UseRewardPoints: true
      };
      $.ajax({
        cache: false,
        url: this.useRewardPointsUrl,
        data: model,
        type: 'post',
        success: function (response, textStatus, jqXHR) {
          if ($(Billing.selector.billingAddressSelectSelector).val() !== "" && $(Billing.selector.billingAddressSelectSelector).val() > 0) {
            var selectedBillingAddressCountry = $(Billing.selector.billingAddressSelectSelector).attr('data-country')
            PaymentMethod.loadPaymentMethods(selectedBillingAddressCountry).then((data) => {
              ConfirmOrder.loadOrderTotal();
            });
          } else if ($(Billing.selector.bililngNewAddressCountrySelector).val() >= 0 && $(Billing.selector.billingAddressSelectSelector).val() === "") {
            PaymentMethod.loadPaymentMethods($(Billing.selector.bililngNewAddressCountrySelector).val()).then((data) => {
              ConfirmOrder.loadOrderTotal();
            });
          } else {
            PaymentMethod.loadPaymentMethods(0).then((data) => {
              ConfirmOrder.loadOrderTotal();
            });
          }

        },
        error: Checkout.ajaxFailure
      });
    } else {

      var model = {
        UseRewardPoints: false
      };
      $.ajax({
        cache: false,
        url: this.useRewardPointsUrl,
        data: model,
        type: 'post',
        success: function (response, textStatus, jqXHR) {
          if ($(Billing.selector.billingAddressSelectSelector).val() !== "" && $(Billing.selector.billingAddressSelectSelector).val() > 0) {
            var selectedBillingAddressCountry = $(Billing.selector.billingAddressSelectSelector).attr('data-country')
            PaymentMethod.loadPaymentMethods(selectedBillingAddressCountry).then((data) => {
              ConfirmOrder.loadOrderTotal();
            });
          } else if ($(Billing.selector.bililngNewAddressCountrySelector).val() >= 0 && $(Billing.selector.billingAddressSelectSelector).val() === "") {
            PaymentMethod.loadPaymentMethods($(Billing.selector.bililngNewAddressCountrySelector).val()).then((data) => {
              ConfirmOrder.loadOrderTotal();
            });
          } else {
            PaymentMethod.loadPaymentMethods(0).then((data) => {
              ConfirmOrder.loadOrderTotal();
            });
          }
        },
        error: Checkout.ajaxFailure
      });
    }
  },
  updatePaymentMethod: function (paymentMethodSystemName) {
    var model = {
      PaymentMethodSystemName: paymentMethodSystemName
    };
    $.ajax({
      cache: false,
      url: this.updatePaymentUrl,
      data: model,
      type: 'post',
      success: function (response, textStatus, jqXHR) {
        if (response.error) {
          alert(response.message)
        } else {
          PaymentMethod.loadPaymentMethodInfo(response);
        }
      },
      error: Checkout.ajaxFailure
    });
  },
  loadPaymentMethodInfo: function (response) {
    if (response.paymentMethodSystemName != null) {
      $.ajax({
        cache: false,
        url: this.loadPaymentInfoUrl,
        data: {
          paymentMethodSystemName: response.paymentMethodSystemName
        },
        type: 'post',
        success: function (response, textStatus, jqXHR) {
          if (response.error) {
            alert(response.message)
          } else {
            if (response.update_section != undefined) {
              if (response.count != 0) {
                $(PaymentMethod.selector.paymentMethodInfoSelector).html(response.update_section.html);
                ConfirmOrder.loadOrderTotal();
              }
            }
          }
        },
        error: Checkout.ajaxFailure
      });
    }
  },

  validate: function () {
    var methods = document.getElementsByName('paymentmethod');
    if (methods.length == 0) {
      alert('Your order cannot be completed at this time as there is no payment methods available for it.');
      return false;
    }

    for (var i = 0; i < methods.length; i++) {
      if (methods[i].checked) {
        return true;
      }
    }
    alert('Please specify payment method.');
    return false;
  }
};
var DiscountCoupon = {
  applyDiscountUrl: false,
  removeDiscountUrl: false,
  init: function (applyDiscountUrl, removeDiscountUrl) {
    this.applyDiscountUrl = applyDiscountUrl;
    this.removeDiscountUrl = removeDiscountUrl;
  },

  applyDiscount: function (discount) {
    return new Promise((resolve, reject) => {
      $.ajax({
        cache: false,
        url: this.applyDiscountUrl,
        data: discount,
        type: 'post',
        success: function (response, textStatus, jqXHR) {
          if (response.error) {
            alert(response.message);
          } else {
            if (response.update_section != undefined) {
              if (response.count != 0) {
                $('#' + response.update_section.name).html(response.update_section.html);
              }
            }
            resolve(response);
          }
        },
        error: function (error) {
          reject(error);
        },
      })
    });
  },
  removeDiscount: function (discount) {
    return new Promise((resolve, reject) => {
      $.ajax({
        cache: false,
        url: this.removeDiscountUrl,
        data: discount,
        type: 'post',
        success: function (response, textStatus, jqXHR) {
          if (response.error) {
            alert(response.message);
          } else {
            if (response.update_section != undefined) {
              if (response.count != 0) {
                $('#' + response.update_section.name).html(response.update_section.html);
              }
            }
            resolve(response);
          }
        },
        error: function (error) {
          reject(error);
        }
      })
    });
  }
};
var GiftCard = {
  applyGiftCardUrl: false,
  removeGiftCardUrl: false,
  init: function (applyGiftCardUrl, removeGiftCardUrl) {
    this.applyGiftCardUrl = applyGiftCardUrl;
    this.removeGiftCardUrl = removeGiftCardUrl;
  },
  applyGiftCard: function (giftcard) {
    return new Promise((resolve, reject) => {
      $.ajax({
        cache: false,
        url: this.applyGiftCardUrl,
        data: giftcard,
        type: 'post',
        success: function (response, textStatus, jqXHR) {
          if (response.error) {
            alert(response.message);
          } else {
            if (response.update_section != undefined) {
              if (response.count != 0) {
                $('#' + response.update_section.name).html(response.update_section.html);
              }
            }
            resolve(response);
          }
        },
        error: function (error) {
          reject(error);
        },
      })
    });
  },
  removeGiftCard: function (giftcard) {
    return new Promise((resolve, reject) => {
      $.ajax({
        cache: false,
        url: this.removeGiftCardUrl,
        data: giftcard,
        type: 'post',
        success: function (response, textStatus, jqXHR) {
          if (response.error) {
            alert(response.message);
          } else {
            if (response.update_section != undefined) {
              if (response.count != 0) {
                $('#' + response.update_section.name).html(response.update_section.html);
              }
            }
            resolve(response);
          }
        },
        error: function (error) {
          reject(error);
        }
      })
    });
  }
};
var ShoppingCart = {
  getShoppingCartItemsUrl: false,
  updateShoppingCartItemsUrl: false,
  deleteShoppingCartItemsUrl: false,
  getFlyOutCartUrl: false,
  localized_data: false,
  checkoutAttributeUrl: false,
  selector: false,
  init: function (getShoppingCartItemsUrl, updateShoppingCartItemsUrl, deleteShoppingCartItemsUrl, getFlyOutCartUrl, localized_data, checkoutAttributeUrl, selector) {
    this.getShoppingCartItemsUrl = getShoppingCartItemsUrl;
    this.updateShoppingCartItemsUrl = updateShoppingCartItemsUrl;
    this.deleteShoppingCartItemsUrl = deleteShoppingCartItemsUrl;
    this.getFlyOutCartUrl = getFlyOutCartUrl;
    this.localized_data = localized_data;
    this.checkoutAttributeUrl = checkoutAttributeUrl;
    this.selector = selector;
  },
  GetShoppingCartItems: function () {
    return new Promise((resolve, reject) => {
      $.ajax({
        cache: false,
        url: this.getShoppingCartItemsUrl,
        type: 'get',
        success: function (response, textStatus, jqXHR) {
          if (response.update_section != undefined) {
            if (response.count != 0) {
              $('#' + response.update_section.name).html(response.update_section.html);
            }
          }
          resolve(response);
        },
        error: function (error) {
          reject(error);
        }
      })
    });
  },
  cartQuantityChange: function (shoppingCartItemId, isPlusIconPressed, isItemEditable) {
    var currentQuantity = $("#itemquantity" + shoppingCartItemId).val();
    var incrementedQuantity = 0;
    if (isPlusIconPressed) {
      incrementedQuantity = ++currentQuantity;
    } else {
      incrementedQuantity = --currentQuantity;
    }
    var model = {
      CartItemId: shoppingCartItemId,
      CartItemQuantity: incrementedQuantity
    };

    ShoppingCart.UpdateCartItem(model).then((data) => {
      var totalItems = data.count;
      if (data.warnings.length === 0) {
        ShoppingCart.GetShoppingCartItems().then((data) => {
          if ($(Billing.selector.billingAddressSelectSelector).val() !== "" && $(Billing.selector.shipToSameAddressSelector).is(":checked") && $(Billing.selector.billingAddressSelectSelector).val() > 0) {
            var selectedBillingAddressCountry = $(Billing.selector.billingAddressSelectSelector).attr('data-country')
            PaymentMethod.loadPaymentMethods(selectedBillingAddressCountry).then((data) => {
              ShippingMethod.loadShippingMethodByAddress($(Billing.selector.billingAddressSelectSelector).val());
            });
          }
          if ($(Billing.selector.billingAddressSelectSelector).val() !== "" && $(Billing.selector.billingAddressSelectSelector).val() > 0 && !$(Billing.selector.shipToSameAddressSelector).is(":checked")) {
            var selectedBillingAddressCountry = $(Billing.selector.billingAddressSelectSelector).attr('data-country')
            PaymentMethod.loadPaymentMethods(selectedBillingAddressCountry);
          }
          if ($(Shipping.selector.shippingAddressSelectSelector).val() !== "" && $(Shipping.selector.shippingAddressSelectSelector).val() > 0) {
            ShippingMethod.loadShippingMethodByAddress($(Shipping.selector.shippingAddressSelectSelector).val())
          }
          if ($(Billing.selector.bililngNewAddressCountrySelector).val() >= 0 && $(Billing.selector.billingAddressSelectSelector).val() === "") {
            PaymentMethod.loadPaymentMethods($(Billing.selector.bililngNewAddressCountrySelector).val());
          }

          //if ($('#shipping-address-select').val() === "" && !$(Billing.selector.shipToSameAddressSelector).is(":checked")) {
          //  ShippingMethod.loadShippingMethod();
          //}

        }).then((data) => {
          ConfirmOrder.loadOrderTotal().then((data) => {
            ShoppingCart.GetFlyOutCart().then((data) => {
              $('.header-links .cart-qty').html(totalItems);
            });
          });
        });
      } else {
        displayBarNotification(data.warnings, 'error', 0);
      }
    });
  },
  cartQuantityChangeDropdown: function (selectedValue, itemId) {
    var incrementedQuantity = selectedValue;
    var model = {
      CartItemId: itemId,
      CartItemQuantity: incrementedQuantity
    };

    ShoppingCart.UpdateCartItem(model).then((data) => {
      var totalItems = data.count;
      if (data.warnings.length === 0) {
        ShoppingCart.GetShoppingCartItems().then((data) => {
          if ($(Billing.selector.billingAddressSelectSelector).val() !== "" && $(Billing.selector.shipToSameAddressSelector).is(":checked") && $(Billing.selector.billingAddressSelectSelector).val() > 0) {
            var selectedBillingAddressCountry = $(Billing.selector.billingAddressSelectSelector).attr('data-country')
            PaymentMethod.loadPaymentMethods(selectedBillingAddressCountry).then((data) => {
              ShippingMethod.loadShippingMethodByAddress($(Billing.selector.billingAddressSelectSelector).val());
            });
          }
          if ($(Billing.selector.billingAddressSelectSelector).val() !== "" && $(Billing.selector.billingAddressSelectSelector).val() > 0 && !$(Billing.selector.shipToSameAddressSelector).is(":checked")) {
            var selectedBillingAddressCountry = $(Billing.selector.billingAddressSelectSelector).attr('data-country')
            PaymentMethod.loadPaymentMethods(selectedBillingAddressCountry);
          }
          if ($(Shipping.selector.shippingAddressSelectSelector).val() !== "" && $(Shipping.selector.shippingAddressSelectSelector).val() > 0) {
            ShippingMethod.loadShippingMethodByAddress($(Shipping.selector.shippingAddressSelectSelector).val())
          }
          if ($(Billing.selector.bililngNewAddressCountrySelector).val() >= 0 && $(Billing.selector.billingAddressSelectSelector).val() === "") {
            PaymentMethod.loadPaymentMethods($(Billing.selector.bililngNewAddressCountrySelector).val());
          }

          //if ($('#shipping-address-select').val() === "" && !$(Billing.selector.shipToSameAddressSelector).is(":checked")) {
          //  ShippingMethod.loadShippingMethod();
          //}

        }).then((data) => {
          ConfirmOrder.loadOrderTotal().then((data) => {
            ShoppingCart.GetFlyOutCart().then((data) => {
              $('.header-links .cart-qty').html(totalItems);
            });
          });
        });
      } else {
        displayBarNotification(data.warnings, 'error', 0);
      }
    });
  },
  UpdateCartItem: function (model) {
    return new Promise((resolve, reject) => {
      $.ajax({
        cache: false,
        url: this.updateShoppingCartItemsUrl,
        data: model,
        type: 'post',
        success: function (response, textStatus, jqXHR) {
          resolve(response);
        },
        error: function (error) {
          reject(error);
        }
      })
    });
  },
  DeleteCartItem: function (cartItem) {
    var model = {
      CartItemId: cartItem
    };
    return new Promise((resolve, reject) => {
      $.ajax({
        cache: false,
        url: this.deleteShoppingCartItemsUrl,
        data: model,
        type: 'post',
        success: function (response, textStatus, jqXHR) {
          var totalItems = response.CartCount;
          if (response.CartCount == 0) {
            $('#opc').html(ShoppingCart.localized_data.EmptyShoppingCart); //EmptyShoppingCart
            ShoppingCart.GetFlyOutCart().then((data) => {
              $('.header-links .cart-qty').html(response.CartCount);
            });
          } else {
            ShoppingCart.GetShoppingCartItems().then((data) => {
              var selectedBillingAddressCountry = $(Billing.selector.billingAddressSelectSelector).attr('data-country');
              if ($(Billing.selector.billingAddressSelectSelector).val() !== "" && $(Billing.selector.shipToSameAddressSelector).is(":checked") && $(Billing.selector.billingAddressSelectSelector).val() > 0) {

                PaymentMethod.loadPaymentMethods(selectedBillingAddressCountry).then((data) => {
                  ShippingMethod.loadShippingMethodByAddress($(Billing.selector.billingAddressSelectSelector).val());
                });
              }
              if ($(Billing.selector.billingAddressSelectSelector).val() !== "" && $(Billing.selector.billingAddressSelectSelector).val() > 0 && !$(Billing.selector.shipToSameAddressSelector).is(":checked")) {
                PaymentMethod.loadPaymentMethods(selectedBillingAddressCountry);
              }
              if ($(Shipping.selector.shippingAddressSelectSelector).val() !== "" && $(Shipping.selector.shippingAddressSelectSelector).val() > 0) {
                ShippingMethod.loadShippingMethodByAddress($(Shipping.selector.shippingAddressSelectSelector).val())
              }
              if ($(Billing.selector.bililngNewAddressCountrySelector).val() >= 0 && $(Billing.selector.billingAddressSelectSelector).val() === "") {
                PaymentMethod.loadPaymentMethods($(Billing.selector.bililngNewAddressCountrySelector).val());
              }

              //if ($('#shipping-address-select').val() === "" && !$(Billing.selector.shipToSameAddressSelector).is(":checked")) {
              //  ShippingMethod.loadShippingMethod();
              //}

            }).then((data) => {
              ConfirmOrder.loadOrderTotal().then((data) => {
                ShoppingCart.GetFlyOutCart().then((data) => {
                  $('.header-links .cart-qty').html(totalItems);
                  ShoppingCart.loadCheckoutAttributes();
                });
              });
            });
          }
        },
        error: function (error) {
          reject(error);
        }
      })
    });
  },
  GetFlyOutCart: function () {
    return new Promise((resolve, reject) => {
      $.ajax({
        cache: false,
        url: this.getFlyOutCartUrl,
        type: 'get',
        success: function (response, textStatus, jqXHR) {
          $(ShoppingCart.selector.flyoutCartSelector).replaceWith(response);
          resolve(response);
        },
        error: Checkout.ajaxFailure
      })
    });
  },
  loadCheckoutAttributes: function () {
    return new Promise((resolve, reject) => {
      $.ajax({
        cache: false,
        url: this.checkoutAttributeUrl,
        type: 'get',
        success: function (response, textStatus, jqXHR) {
          if (response.error) {
            alert(response.message);
          } else {
            if (response.update_section != undefined) {
              if (response.count != 0) {
                $('#' + response.update_section.name + '-load').html(response.update_section.html);
                $('#' + response.update_section.selectedcheckoutattributename + '-load').html(response.update_section.selectedcheckoutattributehtml);

                if (response.update_section.attributeCount === 0) {
                  $('#checkout-attributes').html('');
                }
              }
            }
            resolve(response);
          }
        },
        error: function (error) {
          reject(error);
        }
      })
    });
  },
};
var ConfirmOrder = {
  saveUrl: false,
  loadOrderTotalUrl: false,
  loadOrderReviewUrl: false,
  selector: false,
  init: function (loadOrderTotalUrl, loadOrderReviewUrl, saveUrl, selector) {
    this.loadOrderTotalUrl = loadOrderTotalUrl;
    this.loadOrderReviewUrl = loadOrderReviewUrl;
    this.saveUrl = saveUrl;
    this.selector = selector;
  },
  loadOrderTotal: function () {
    return new Promise((resolve, reject) => {
      $.ajax({
        cache: false,
        url: this.loadOrderTotalUrl,
        type: 'get',
        success: function (response, textStatus, jqXHR) {
          if (response.update_section != undefined) {
            if (response.count != 0) {
              $('#' + response.update_section.name).html(response.update_section.html);
            }
          }
          $(document).trigger("order-total-loaded");
          resolve(response);
        },
        error: function (error) {
          reject(error);
        }
      })
    });
  },
  loadOrderReview: function () {
    return new Promise((resolve, reject) => {
      $.ajax({
        cache: false,
        url: this.loadOrderReviewUrl,
        type: 'get',
        success: function (response, textStatus, jqXHR) {
          if (response.update_section != undefined) {
            if (response.count != 0) {
              $('.' + response.update_section.name).html(response.update_section.html);
            }
          }
          resolve(response);
        },
        error: function (error) {
          reject(error);
        }
      })
    });
  },
  save: function () {
    var termOfServiceOk = true;
    if ($('#termsofservice').length > 0) {
      //terms of service element exists
      if (!$('#termsofservice').is(':checked')) {
        $("#terms-of-service-warning-box").dialog();
        termOfServiceOk = false;
      } else {
        termOfServiceOk = true;
      }
    }
    if (termOfServiceOk) {

      $("#confirm-order-button").prop('disabled', true);

      var data = $("#opc-billing-form, #opc-shipping-form, #opc-shipping-method-form, #opc-payment-method-form, #opc-checkoutattribute-form").serialize();
      var postData = data;
      addAntiForgeryToken(postData);
      $.ajax({
        cache: false,
        url: this.saveUrl,
        data: postData,
        type: "POST",
        success: function (response) {
          if (response.error) {
            if (response.Exc) {
              alert(response.Exc);
            }
            var errorString = '<ul>';
            $.each(response.Errors, function (key, value) {
              errorString += '<li class="error-message" style="color:red">' + value + '</li>';
            });
            errorString += '</ul>';
            $('#error-message-list').html(errorString);

            $("#confirm-order-button").prop('disabled', true);
            return false;
          } else {
            if (response.RedirectUrl) {
              window.location.replace(response.RedirectUrl);
            }
          }
        },
        complete: function (response) {
          $("#confirm-order-button").prop('disabled', false);
        },
        error: Checkout.ajaxFailure
      });
    } else {
      return false;
    }
  }
};

function delay(fn, ms) {
  let timer = 0
  return function (...args) {
    clearTimeout(timer)
    timer = setTimeout(fn.bind(this, ...args), ms || 0)
  }
}

$(document).ready(function () {

  $(document).ajaxStart(function () {
    $("#loading-overlay").show();
  });

  $(document).ajaxComplete(function () {
    $("#loading-overlay").hide();
  });

  var billingFieldsSettings = Checkout.settings.SaveBillingAddressOnChangeFields;

  var updatePaymentMethodsOnChangeBillingAddressFields = Checkout.settings.UpdatePaymentMethodsOnChangeBillingAddressFields.map(i => {
    return "BillingNewAddress_" + i;
  });
  var updateShippingMethodsOnChangeShippingAddressFields = Checkout.settings.UpdateShippingMethodsOnChangeShippingAddressFields.map(i => {
    return "ShippingNewAddress_" + i;
  });
  var updatePaymentMethodsOnChangeShippingAddressFields = Checkout.settings.UpdatePaymentMethodsOnChangeShippingAddressFields.map(i => {
    return "ShippingNewAddress_" + i;
  });

  var billingInputFields = billingFieldsSettings.map(i => {
    return "#BillingNewAddress_" + i;
  }).join(', ');

  $(document).on('change', billingInputFields, delay(function (e, isEdit) {

    var addressOptions = $(Billing.selector.billingAddressSelectSelector);
    var addressId = $(Billing.selector.billingAddressSelectSelector).val();

    var shippingAddressSelector = $(Shipping.selector.shippingAddressSelectSelector);
    var selectedShippingAddressId = $(Shipping.selector.shippingAddressSelectSelector).val();

    if (addressId === undefined || addressId === null || addressId === '')
      addressId = 0;

    Billing.modifyAddress().then((data) => {

      if (!data.error) {
        if (data.selectlist != null && !jQuery.isEmptyObject(data.selectlist)) {
          addressOptions.empty();
          shippingAddressSelector.empty();
          $.each(data.selectlist, function (index, region) {

            addressOptions.append("<option value='" + region.Value + "'" + (region.Selected ? " selected" : "") + " data-country='" + region.CountryId + "'>" + region.Text + "</option>");
            shippingAddressSelector.append("<option value='" + region.Value + "'" + (region.Value === selectedShippingAddressId ? " selected" : "") + " data-country='" + region.CountryId + "'>" + region.Text + "</option>");
          });
        }

        if (data.countryId != undefined) {
          $("#billing-new").attr('data-country', data.countryId);
        }

        $(Billing.selector.billingAddressSelector).html(data.html);

        if (data.addressId != undefined && data.addressId > 0) {
          $("#BillingNewAddress_Id").val(data.addressId);
        }

        $("#billing-new-address-form .message-error").html("");

        if ($.inArray(e.currentTarget.id, updatePaymentMethodsOnChangeBillingAddressFields) !== -1) {

          PaymentMethod.loadPaymentMethods($(Billing.selector.bililngNewAddressCountrySelector).val()).then((data) => {

            if ($(Billing.selector.shipToSameAddressSelector).is(":checked")) {
              ShippingMethod.loadShippingMethod().then((data) => {
                //load order total here. 
                ConfirmOrder.loadOrderTotal();
              });
            } else {
              ConfirmOrder.loadOrderTotal();
            }

          });
        }
      } else {
        $(Billing.selector.billingAddressSelector).html(data.html);
      }

      Billing.initializeCountrySelect();

    })
      .catch((error) => {
        alert(error.statusText);
      });

  }, 500));

  /* shipping address start */
  var shippingFieldsSettings = Checkout.settings.SaveShippingAddressOnChangeFields;

  var shippingInputFields = shippingFieldsSettings.map(i => {
    console.log(i);
    return "#ShippingNewAddress_" + i;
  }).join(', ');

  $(document).on('change', shippingInputFields, delay(function (e, isEdit) {
    if (!($("#PickupInStore").is(":checked"))) {
      console.log("shippingInputFields: " + shippingFieldsSettings);
      console.log("Changed input field with ID: " + e.currentTarget.id);
      var addressOptions = $(Shipping.selector.shippingAddressSelectSelector);
      var addressId = $(Shipping.selector.shippingAddressSelectSelector).val();

      var billingAddressSelector = $(Billing.selector.billingAddressSelectSelector);
      var selectedBillingAddressId = $(Billing.selector.billingAddressSelectSelector).val();
      if (addressId === undefined || addressId === null || addressId === '')
        addressId = 0;

      Shipping.modifyAddress().then((data) => {

        if (!data.error) {
          if (data.selectlist != null && !jQuery.isEmptyObject(data.selectlist)) {
            addressOptions.empty();
            billingAddressSelector.empty();
            $.each(data.selectlist, function (index, region) {

              addressOptions.append("<option value='" + region.Value + "'" + (region.Selected ? " selected" : "") + " data-country='" + region.CountryId + "'>" + region.Text + "</option>");
              billingAddressSelector.append("<option value='" + region.Value + "'" + (region.Value === selectedBillingAddressId ? " selected" : "") + " data-country='" + region.CountryId + "'>" + region.Text + "</option>");
            });

          }

          $(Shipping.selector.shippingAddressSelector).html(data.html);

          if (data.addressId != undefined && data.addressId > 0) {
            $("#ShippingNewAddress_Id").val(data.addressId);
          }

          $("#shipping-new-address-form .message-error").html("");

          if ($.inArray(e.currentTarget.id, updateShippingMethodsOnChangeShippingAddressFields) !== -1) {

            ShippingMethod.loadShippingMethod().then((data) => {
              if ($.inArray(e.currentTarget.id, updatePaymentMethodsOnChangeShippingAddressFields) !== -1) {
                PaymentMethod.loadPaymentMethods($(Billing.selector.bililngNewAddressCountrySelector).val()).then((data) => {
                  ConfirmOrder.loadOrderTotal();
                });

              } else {
                ConfirmOrder.loadOrderTotal();
              }
            });
          }
        }
        else {
          $(Shipping.selector.shippingAddressSelector).html(data.html);
        }
        Shipping.initializeCountrySelect();

      })
        .catch((error) => {
          alert(error.statusText);
        });
    }

  }, 1000));


  //shipping address end 

});

$(document).bind('order-total-loaded', function (e) {

  if (Checkout.settings.ShowShoppingCartInCheckout && Checkout.settings.ShowOrderReviewDataInCheckout) {
    ConfirmOrder.loadOrderReview();
  }

});

$(document).bind('shiptosame_address_changed', function (e) {
  var data = e.changedData;
  if (!data) {

    $(Shipping.selector.shippingAddressSelectSelector).trigger('change');
  }
});

//Country select function
+function ($) {
  'use strict';
  if ('undefined' == typeof (jQuery)) {
    throw new Error('jQuery JS required');
  }

  function countrySelectHandler() {
    var $this = $(this);
    console.log(this);
    var selectedItem = $this.val();
    var stateProvince = $($this.data('stateprovince'));
    console.log(stateProvince[0].id);
    if (stateProvince.length == 0)
      return;

    var loading = $($this.data('loading'));
    loading.show();
    $.ajax({
      cache: false,
      type: "GET",
      url: $this.data('url'),
      data: {
        'countryId': selectedItem,
        'addSelectStateItem': "true"
      },
      success: function (data, textStatus, jqXHR) {
        stateProvince.html('');
        $.each(data,
          function (id, option) {
            stateProvince.append($('<option></option>').val(option.id).html(option.name));
          });
      },
      error: function (jqXHR, textStatus, errorThrown) {
        alert('Failed to retrieve states.');
      },
      complete: function (jqXHR, textStatus) {

        var stateId = stateProvince[0].id === "ShippingNewAddress_StateProvinceId" ? ((typeof Shipping !== "undefined") ? Shipping.selectedStateId : 0) :
          ((typeof Billing !== "undefined") ? Billing.selectedStateId : 0);

        $('#' + stateProvince[0].id + ' option[value=' + stateId + ']').prop('selected', true);
        loading.hide();
      }
    });
  }
  if ($(document).has('[data-trigger="opc-country-select"]')) {
    $('select[data-trigger="opc-country-select"]').change(countrySelectHandler);
  }
  $.fn.countrySelect = function () {
    this.each(function () {
      $(this).change(countrySelectHandler);
    });
  }
}(jQuery);