document.addEventListener("DOMContentLoaded", () => {

    const modalEl = document.getElementById("uploadClienteModal");
    if (!modalEl) return;

    const formCliente = modalEl.querySelector("form");
    if (!formCliente) return;

    formCliente.addEventListener("submit", async (e) => {
        e.preventDefault();

        const { isConfirmed } = await Swal.fire({
            title: '¿Deseas subir este cliente?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Sí, subir',
            cancelButtonText: 'Cancelar',
            reverseButtons: true,
            background: '#1e1e2f',
            color: '#ffffff'
        });

        if (isConfirmed) {
            formCliente.submit();
        }
    });

});