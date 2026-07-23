// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function () {
    const sidebarToggle = document.getElementById("sidebarToggle");

    if (!sidebarToggle) {
        return;
    }

    sidebarToggle.addEventListener("click", function () {
        if (window.innerWidth <= 820) {
            document.body.classList.toggle("mobile-sidebar-open");
        } else {
            document.body.classList.toggle("sidebar-collapsed");
        }
    });

    document.addEventListener("click", function (event) {
        const sidebar = document.getElementById("adminSidebar");

        if (
            window.innerWidth <= 820 &&
            document.body.classList.contains("mobile-sidebar-open") &&
            sidebar &&
            !sidebar.contains(event.target) &&
            !sidebarToggle.contains(event.target)
        ) {
            document.body.classList.remove("mobile-sidebar-open");
        }
    });
});