document.addEventListener("DOMContentLoaded", () => {

    const swalTheme = { background: '#1e1e2f', color: '#ffffff' };
    const MIN_LOADING_TIME = 700;
    const delay = ms => new Promise(res => setTimeout(res, ms));

    // ---------------- SWEETALERT WRAPPERS ----------------
    const mostrarCargando = mensaje => Swal.fire({ title: mensaje || "Procesando...", allowOutsideClick: false, allowEscapeKey: false, didOpen: () => Swal.showLoading(), ...swalTheme });
    const mostrarExito = mensaje => Swal.fire({ title: mensaje, icon: "success", timer: 1500, timerProgressBar: true, showConfirmButton: false, ...swalTheme });
    const mostrarError = mensaje => Swal.fire({ title: mensaje || "Ocurrió un error", icon: "error", ...swalTheme });
    const confirmar = (titulo, texto = "", icon = "question") => Swal.fire({ title: titulo, text: texto, icon, showCancelButton: true, confirmButtonText: "Confirmar", cancelButtonText: "Cancelar", reverseButtons: true, ...swalTheme });

    // ---------------- FETCH CON LOADER ----------------
    const fetchConLoader = async (url, options, mensajeExito) => {
        try {
            mostrarCargando();
            const [response] = await Promise.all([fetch(url, options), delay(MIN_LOADING_TIME)]);
            if (!response.ok) throw new Error("Error en la operación");
            Swal.close();
            await mostrarExito(mensajeExito);
            return true;
        } catch (error) {
            Swal.close();
            await mostrarError(error.message);
            return false;
        }
    };

    // ---------------- ANTI CACHE IMÁGENES ----------------
    const actualizarImagenesSinCache = () => {
        const timestamp = Date.now();
        document.querySelectorAll("#clients-container img").forEach(img => {
            if (!img.src) return;
            img.src = img.src.split("?")[0] + "?v=" + timestamp;
        });
    };

    // ---------------- RECARGAR CLIENTES ----------------
    const recargarClientes = async () => {
        try {
            const response = await fetch(`/Home/ClientesPartial?v=${Date.now()}`, { cache: "no-store" });
            if (!response.ok) throw new Error();
            const html = await response.text();
            const container = document.getElementById("clients-container");
            if (container) {
                container.innerHTML = html;
                actualizarImagenesSinCache();
                if (typeof AOS !== "undefined") AOS.refresh();
            }
        } catch (error) {
            console.error("Error recargando clientes:", error);
        }
    };

    // ---------------- VISTA PREVIA LOGO ----------------
    const previewLogo = (input, previewId) => {
        const preview = document.getElementById(previewId);
        if (!preview) return;
        if (input.files && input.files[0]) {
            const reader = new FileReader();
            reader.onload = e => preview.src = e.target.result;
            reader.readAsDataURL(input.files[0]);
        } else {
            preview.src = '/img/logo-placeholder.png';
        }
    };

    window.previewLogoUpload = input => previewLogo(input, 'logo-preview-upload');
    window.previewLogo = input => previewLogo(input, 'logo-preview');

    // ---------------- RELLENAR FORMULARIO EDITAR ----------------
    document.addEventListener("click", e => {
        const btn = e.target.closest(".edit-client-btn");
        if (!btn) return;
        const campos = ["id", "nombre", "facebook", "twitter", "instagram", "telegram", "linkedin", "whatsapp", "website"];
        campos.forEach(campo => {
            const input = document.getElementById(`edit-${campo}`);
            if (input) input.value = btn.dataset[campo] || "";
        });
        // Preview del logo
        const preview = document.getElementById('logo-preview');
        if (preview) preview.src = btn.dataset.logo || '/img/logo-placeholder.png';
    });

    // ---------------- SUBMIT GENÉRICO AJAX ----------------
    const manejarSubmitAjax = (form, mensajeConfirmacion, mensajeExito, modal) => {
        form.addEventListener("submit", async e => {
            e.preventDefault();
            const confirmacion = await confirmar(mensajeConfirmacion);
            if (!confirmacion.isConfirmed) return;
            const ok = await fetchConLoader(form.action, { method: "POST", body: new FormData(form) }, mensajeExito);
            if (!ok) return;
            if (modal) {
                bootstrap.Modal.getInstance(modal)?.hide();
                form.reset();
            }
            await recargarClientes();
        });
    };

    // ---------------- MODALES ----------------
    const uploadModal = document.getElementById("uploadClienteModal");
    if (uploadModal) {
        const uploadForm = uploadModal.querySelector("form");
        if (uploadForm) manejarSubmitAjax(uploadForm, "¿Deseas subir este cliente?", "Cliente subido correctamente", uploadModal);

        uploadModal.addEventListener("hidden.bs.modal", () => {
            uploadForm.reset();
            document.getElementById('logo-preview-upload').src = '/img/logo-placeholder.png';
        });
    }

    const editModal = document.getElementById("editClienteModal");
    if (editModal) {
        const editForm = editModal.querySelector("form");
        if (editForm) manejarSubmitAjax(editForm, "¿Guardar cambios?", "Cliente actualizado correctamente", editModal);

        editModal.addEventListener("hidden.bs.modal", () => {
            editForm.reset();
            document.getElementById('logo-preview').src = '/img/logo-placeholder.png';
        });
    }

    // ---------------- ELIMINAR CLIENTE ----------------
    document.addEventListener("click", async e => {
        const deleteBtn = e.target.closest(".delete-client-form button");
        if (!deleteBtn) return;
        e.preventDefault();
        const form = deleteBtn.closest("form");
        if (!form) return;
        const confirmacion = await confirmar("¿Eliminar cliente?", "Esta acción no se puede deshacer.", "warning");
        if (!confirmacion.isConfirmed) return;
        const ok = await fetchConLoader(form.action, { method: "POST", body: new FormData(form) }, "Cliente eliminado correctamente");
        if (ok) await recargarClientes();
    });

});