document.addEventListener('DOMContentLoaded', function () {
    const dropdownSubmenus = document.querySelectorAll('.dropdown-submenu');

    dropdownSubmenus.forEach(function (submenu) {
        submenu.addEventListener('mouseenter', function () {
            const menu = submenu.querySelector('.dropdown-menu');
            if (menu) {
                new bootstrap.Dropdown(submenu.querySelector('.dropdown-toggle')).show();
            }
        });

        submenu.addEventListener('mouseleave', function () {
            const menu = submenu.querySelector('.dropdown-menu');
            if (menu) {
                bootstrap.Dropdown.getInstance(submenu.querySelector('.dropdown-toggle'))?.hide();
            }
        });
    });
});

window.bloquearTabla = function () {
    document.getElementById('table-blocker')?.classList.remove('d-none');
};

window.desbloquearTabla = function () {
    document.getElementById('table-blocker')?.classList.add('d-none');
};

window.initGoogleMaps = function () {
    console.log("Google Maps cargado correctamente");

    if (typeof initCreateMap === "function") {
        initCreateMap();
    }

    if (typeof initEditMap === "function") {
        initEditMap();
    }
};