document.addEventListener("DOMContentLoaded", () => {

    const mostrarExitoConProgreso = (mensaje) => {
        return Swal.fire({
            title: mensaje,
            icon: 'success',
            timer: 1500,
            timerProgressBar: true,
            showConfirmButton: false,
            background: '#1e1e2f',
            color: '#ffffff'
        });
    };

    const recargarClientes = async () => {
        try {
            const response = await fetch("/Home/ClientesPartial");
            if (!response.ok) return;

            const html = await response.text();
            const container = document.getElementById("clients-container");
            if (container) {
                container.innerHTML = html;
                if (typeof AOS !== "undefined") AOS.refresh();
            }
        } catch (error) { console.error("Error recargando clientes:", error); }
    };

    // EDITAR CLIENTE (cargar modal)
    document.addEventListener("click", (e) => {
        const btn = e.target.closest(".edit-client-btn");
        if (!btn) return;

        document.getElementById("edit-id").value = btn.dataset.id || "";
        document.getElementById("edit-nombre").value = btn.dataset.nombre || "";
        document.getElementById("edit-facebook").value = btn.dataset.facebook || "";
        document.getElementById("edit-twitter").value = btn.dataset.twitter || "";
        document.getElementById("edit-instagram").value = btn.dataset.instagram || "";
        document.getElementById("edit-telegram").value = btn.dataset.telegram || "";
        document.getElementById("edit-linkedin").value = btn.dataset.linkedin || "";
        document.getElementById("edit-whatsapp").value = btn.dataset.whatsapp || "";
        document.getElementById("edit-website").value = btn.dataset.website || "";
    });

    // SUBIR CLIENTE (AJAX)
    const uploadModal = document.getElementById("uploadClienteModal");
    if (uploadModal) {
        const uploadForm = uploadModal.querySelector("form");
        if (uploadForm) {
            uploadForm.addEventListener("submit", async (e) => {
                e.preventDefault();
                const confirm = await Swal.fire({
                    title: '¿Deseas subir este cliente?',
                    icon: 'question',
                    showCancelButton: true,
                    confirmButtonText: 'Sí, subir',
                    cancelButtonText: 'Cancelar',
                    reverseButtons: true,
                    background: '#1e1e2f',
                    color: '#ffffff'
                });
                if (!confirm.isConfirmed) return;

                const formData = new FormData(uploadForm);
                try {
                    const response = await fetch(uploadForm.action, { method: 'POST', body: formData });
                    if (!response.ok) throw new Error();
                    await mostrarExitoConProgreso('Cliente subido');
                    bootstrap.Modal.getInstance(uploadModal)?.hide();
                    await recargarClientes();
                } catch {
                    Swal.fire({ title: 'Error al subir', icon: 'error', background: '#1e1e2f', color: '#ffffff' });
                }
            });
        }

        uploadModal.addEventListener('hidden.bs.modal', () => uploadForm?.reset());
    }

    // ELIMINAR CLIENTE (AJAX)
    document.addEventListener("click", async (e) => {
        const deleteBtn = e.target.closest(".delete-client-form button");
        if (!deleteBtn) return;

        e.preventDefault();
        const form = deleteBtn.closest("form");
        const confirm = await Swal.fire({
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
        if (!confirm.isConfirmed) return;

        const formData = new FormData(form);
        try {
            const response = await fetch(form.action, { method: 'POST', body: formData });
            if (!response.ok) throw new Error();
            await mostrarExitoConProgreso('Cliente eliminado');
            await recargarClientes();
        } catch {
            Swal.fire({ title: 'Error al eliminar', icon: 'error', background: '#1e1e2f', color: '#ffffff' });
        }
    });

    // EDITAR CLIENTE (AJAX)
    const editModal = document.getElementById("editClienteModal");
    if (editModal) {
        const editForm = editModal.querySelector("form");
        if (editForm) {
            editForm.addEventListener("submit", async (e) => {
                e.preventDefault();
                const confirm = await Swal.fire({
                    title: '¿Guardar cambios?',
                    icon: 'question',
                    showCancelButton: true,
                    confirmButtonText: 'Sí, guardar',
                    cancelButtonText: 'Cancelar',
                    reverseButtons: true,
                    background: '#1e1e2f',
                    color: '#ffffff'
                });
                if (!confirm.isConfirmed) return;

                const formData = new FormData(editForm);
                try {
                    const response = await fetch(editForm.action, { method: 'POST', body: formData });
                    if (!response.ok) throw new Error();
                    await mostrarExitoConProgreso('Cliente actualizado');
                    bootstrap.Modal.getInstance(editModal)?.hide();
                    await recargarClientes();
                } catch {
                    Swal.fire({ title: 'Error al actualizar', icon: 'error', background: '#1e1e2f', color: '#ffffff' });
                }
            });
        }
    }
});