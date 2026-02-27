document.addEventListener("DOMContentLoaded", () => {

    // ============================
    // SweetAlert para Subir Cliente
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
            if (isConfirmed) uploadForm.submit();
        });
        }
    }

    // ============================
    // SweetAlert para Eliminar Cliente
    // ============================
    // SweetAlert para Eliminar Cliente
    const deleteForms = document.querySelectorAll(".delete-client-form");
    deleteForms.forEach((form) => {
        form.addEventListener("submit", async (e) => {
            e.preventDefault();
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
            if (isConfirmed) form.submit();
        });
    });
});