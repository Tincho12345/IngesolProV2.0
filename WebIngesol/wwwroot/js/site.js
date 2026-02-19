// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
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


// 🔽🔽🔽 AGREGAR DESDE ACÁ (SOLO ESTO) 🔽🔽🔽

window.bloquearTabla = function () {
    document.getElementById('table-blocker')?.classList.remove('d-none');
};

window.desbloquearTabla = function () {
    document.getElementById('table-blocker')?.classList.add('d-none');
};
