(function () {
    var badge = document.getElementById("notificationBadge");
    var list = document.getElementById("notificationList");
    if (!badge && !list) return;

    function setUnreadCount(count) {
        if (!badge) return;
        badge.classList.toggle("d-none", count <= 0);
    }

    function prependNotification(notification) {
        if (!list) return;
        var empty = list.querySelector("[data-empty-state]");
        if (empty) empty.remove();

        var item = document.createElement("a");
        item.href = notification.url || "#";
        item.className = "dropdown-item notification-item";
        item.dataset.notificationId = notification.id;
        item.innerHTML =
            '<div class="fw-semibold">' + notification.title + "</div>" +
            '<div class="small text-muted">' + notification.message + "</div>";
        list.prepend(item);
    }

    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/notifications")
        .withAutomaticReconnect()
        .build();

    connection.on("ReceiveNotification", function (notification) {
        prependNotification(notification);
        fetch("/api/v1/notifications/unread-count")
            .then(function (res) { return res.ok ? res.json() : 0; })
            .then(setUnreadCount)
            .catch(function () {});
    });

    connection.start().catch(function (err) { console.error("SignalR connection failed:", err); });

    fetch("/api/v1/notifications/unread-count")
        .then(function (res) { return res.ok ? res.json() : 0; })
        .then(setUnreadCount)
        .catch(function () {});
})();
