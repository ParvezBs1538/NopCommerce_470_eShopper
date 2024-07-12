
self.addEventListener('install', async event => { });

self.addEventListener('fetch', async event => { });

self.addEventListener('push', function (event) {
    if (!(self.Notification && self.Notification.permission === 'granted')) {
        return;
    }
    
    if (event.data) {
        var data = {};
        data = event.data.json();

        var title = data.data.title;
        var body = data.body;
        var icon = data.icon;
        var image = data.image;
        var vibrate = data.vibrate;
        var dir = data.dir;
        var url = data.data.url;

        event.waitUntil(self.registration.showNotification(title, {
            body: body,
            icon: icon,
            image: image,
            vibrate: vibrate,
            dir: dir,
            data: {
                url: url
            }
        }));
    }
});

self.addEventListener('notificationclick', function (event) {
    event.notification.close();
    if (clients.openWindow && event.notification.data.url) {
        event.waitUntil(clients.openWindow(event.notification.data.url));
    }
});
