document.addEventListener("DOMContentLoaded", () => {

    // ============================
    // Subir Cliente via AJAX
    // ============================
    const uploadModal = document.getElementById("uploadClienteModal");
    if (uploadModal) {
        const uploadForm = uploadModal.querySelector("form");
        if (uploadForm) {
            uploadForm.addEventListener("submit", async (e) => {
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
                if (!isConfirmed) return;

                const formData = new FormData(uploadForm);

                try {
                    const response = await fetch(uploadForm.action, {
                        method: 'POST',
                        body: formData
                    });

                    if (response.ok) {
                        Swal.fire({
                            title: 'Cliente subido',
                            icon: 'success',
                            background: '#1e1e2f',
                            color: '#ffffff'
                        });

                        // Cerrar modal
                        const modalInstance = bootstrap.Modal.getInstance(uploadModal);
                        modalInstance.hide();

                        // Recargar la sección de clientes sin ir al top
                        await recargarClientes();
                    } else {
                        Swal.fire({
                            title: 'Error al subir',
                            icon: 'error',
                            background: '#1e1e2f',
                            color: '#ffffff'
                        });
                    }
                } catch (err) {
                    console.error(err);
                }
            });
        }
    }

    // ============================
    // Eliminar Cliente via AJAX
    // ============================
    document.addEventListener("click", async (e) => {
        if (e.target.closest(".delete-client-form button")) {
            e.preventDefault();
            const form = e.target.closest("form");
            const { isConfirmed } = await Swal.fire({
                title: '¿Estás seguro que deseas eliminar este cliente?',
                text: 'Esta acción no se puede deshacer.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Sí, eliminar',
                cancelButtonText: 'Cancelar',
                reverseButtons: true,
                background: '#1e1e2f',
                color: '#ffffff'
            });
            if (!isConfirmed) return;

            const formData = new FormData(form);
            try {
                const response = await fetch(form.action, {
                    method: 'POST',
                    body: formData
                });

                if (response.ok) {
                    Swal.fire({
                        title: 'Cliente eliminado',
                        icon: 'success',
                        background: '#1e1e2f',
                        color: '#ffffff'
                    });

                    // Recargar la sección de clientes
                    await recargarClientes();
                } else {
                    Swal.fire({
                        title: 'Error al eliminar',
                        icon: 'error',
                        background: '#1e1e2f',
                        color: '#ffffff'
                    });
                }
            } catch (err) {
                console.error(err);
            }
        }
    });

    // ============================
    // Función para recargar solo la sección de clientes
    // ============================
    async function recargarClientes() {
        try {
            const response = await fetch("/Home/ClientesPartial");
            if (!response.ok) return;

            const html = await response.text();
            const container = document.getElementById("clients-container");
            if (container) {
                container.innerHTML = html;
                if (AOS) AOS.refresh();
            }
        } catch (err) {
            console.error(err);
        }
    }
});