(function () {
    'use strict';

    const body = document.body;
    const sidebar = document.getElementById('appSidebar');
    const sidebarToggle = document.querySelector('[data-sidebar-toggle]');
    const sidebarBackdrop = document.querySelector('[data-sidebar-dismiss]');

    function setSidebarOpen(open) {
        if (!sidebar || !sidebarToggle || !sidebarBackdrop) return;

        body.classList.toggle('sidebar-open', open);
        sidebarToggle.setAttribute('aria-expanded', String(open));
        sidebarBackdrop.hidden = !open;
        if (window.innerWidth < 1024) sidebar.setAttribute('aria-hidden', String(!open));
        else sidebar.removeAttribute('aria-hidden');

        if (open) sidebar.querySelector('a, button')?.focus();
        else sidebarToggle.focus();
    }

    sidebarToggle?.addEventListener('click', function () {
        setSidebarOpen(!body.classList.contains('sidebar-open'));
    });

    sidebarBackdrop?.addEventListener('click', function () {
        setSidebarOpen(false);
    });

    sidebar?.querySelectorAll('a').forEach(function (link) {
        link.addEventListener('click', function () {
            if (window.innerWidth < 1024) setSidebarOpen(false);
        });
    });

    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape' && body.classList.contains('sidebar-open')) {
            setSidebarOpen(false);
        }
    });

    window.addEventListener('resize', function () {
        if (window.innerWidth >= 1024) {
            body.classList.remove('sidebar-open');
            sidebarBackdrop?.setAttribute('hidden', '');
            sidebarToggle?.setAttribute('aria-expanded', 'false');
            sidebar?.removeAttribute('aria-hidden');
        } else if (!body.classList.contains('sidebar-open')) {
            sidebar?.setAttribute('aria-hidden', 'true');
        }
    });

    if (sidebar && window.innerWidth < 1024) sidebar.setAttribute('aria-hidden', 'true');

    document.querySelectorAll('[data-password-toggle]').forEach(function (button) {
        button.addEventListener('click', function () {
            const input = document.getElementById(button.getAttribute('aria-controls'));
            if (!input) return;

            const reveal = input.type === 'password';
            input.type = reveal ? 'text' : 'password';
            button.setAttribute('aria-label', reveal ? 'Ẩn mật khẩu' : 'Hiện mật khẩu');
            button.setAttribute('aria-pressed', String(reveal));
        });
    });
})();
