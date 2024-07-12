var serviceWorker = '/sw.js';

$(document).ready(function () {
  if (typeof vapidPublicKey === 'undefined') {
    errorHandler('Vapid public key is undefined.');
    return;
  }

  if (typeof checkPushManagerSubscription === 'undefined') {
    checkPushManagerSubscription = true;
    return;
  }

  if (Notification.permission === 'granted') {
    initialiseServiceWorker();
    if (sessionStorage.getItem("push-notification-subscribed") != "true")
      subscribe();
  }
  else if (Notification.permission !== 'granted' && Notification.permission !== 'denied' &&
    sessionStorage.getItem("hide-push-notification-bar") != "true") {
    $('#allow-push-notification-bar').show();
  }

  $('#allow-push-notification').click(function () {
    $('#allow-push-notification-bar').hide();
    Notification.requestPermission().then(function (status) {
      sessionStorage.setItem("hide-push-notification-bar", true);
      if (status === 'denied') {
        errorHandler('[Notification.requestPermission] Browser denied permissions to notification api.');
      } else if (status === 'granted') {
        initialiseServiceWorker();
        subscribe();
      }
    });
  });
});

function initialiseServiceWorker() {
  if ('serviceWorker' in navigator) {
    navigator.serviceWorker.register(serviceWorker).then(function (reg) {
      initialiseState(reg)
    });
  } else {
    errorHandler('[initialiseServiceWorker] Service workers are not supported in this browser.');
  }
};

function initialiseState(reg) {
  if (!(reg.showNotification)) {
    errorHandler('[initialiseState] Notifications aren\'t supported on service workers.');
    return;
  }
  if (!('PushManager' in window)) {
    errorHandler('[initialiseState] Push messaging isn\'t supported.');
    return;
  }

  navigator.serviceWorker.ready.then(function (reg) {
    reg.pushManager.getSubscription()
      .then(function (subscription) {
        if (checkPushManagerSubscription) {
          postData(subscription);
        }
      })
      .catch(function (err) {
        console.log('[req.pushManager.getSubscription] Unable to get subscription details.', err);
      });
  });
}

function subscribe() {
  navigator.serviceWorker.ready.then(function (reg) {
    var subscribeParams = { userVisibleOnly: true };

    var applicationServerKey = urlB64ToUint8Array(vapidPublicKey);
    subscribeParams.applicationServerKey = applicationServerKey;
    reg.pushManager.subscribe(subscribeParams)
      .then(function (subscription) {
        postData(subscription);
      })
      .catch(function (e) {
        errorHandler('[subscribe] Unable to subscribe to push', e);
      });
  });
}

function postData(subscription) {
  var data = {};
  data.Endpoint = subscription.endpoint;
  data.P256dh = base64Encode(subscription.getKey('p256dh'));
  data.Auth = base64Encode(subscription.getKey('auth'));
  addAntiForgeryToken(data);
  $.ajax({
    url: '/save-device',
    type: "POST",
    dataType: 'json',
    data: data,
    success: function (result) {
      sessionStorage.setItem("push-notification-subscribed", "true");
    }
  });
}

function errorHandler(message, e) {
  if (typeof e == 'undefined') {
    e = null;
  }
  console.log(message, e);
}

function urlB64ToUint8Array(base64String) {
  var padding = '='.repeat((4 - base64String.length % 4) % 4);
  var base64 = (base64String + padding)
    .replace(/\-/g, '+')
    .replace(/_/g, '/');

  var rawData = window.atob(base64);
  var outputArray = new Uint8Array(rawData.length);

  for (var i = 0; i < rawData.length; ++i) {
    outputArray[i] = rawData.charCodeAt(i);
  }
  return outputArray;
}

function base64Encode(arrayBuffer) {
  return btoa(String.fromCharCode.apply(null, new Uint8Array(arrayBuffer)));
}