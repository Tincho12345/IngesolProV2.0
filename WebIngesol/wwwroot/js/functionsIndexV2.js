document.addEventListener("DOMContentLoaded", () => {

    // =====================================================
    // CONFIGURACIÓN GLOBAL
    // =====================================================

    const swalTheme = {
        background: '#1e1e2f',
        color: '#ffffff'
    };

    const MIN_LOADING_TIME = 700;
    const delay = (ms) => new Promise(res => setTimeout(res, ms));

    // =====================================================
    // UTILIDADES SWEETALERT
    // =====================================================

    const mostrarCargando = (mensaje = "Procesando...") => {
        Swal.fire({
            title: mensaje,
            allowOutsideClick: false,
            allowEscapeKey: false,
            didOpen: () => Swal.showLoading(),
            ...swalTheme
        });
    };

    const mostrarExito = (mensaje) => {
        return Swal.fire({
            title: mensaje,
            icon: "success",
            timer: 1500,
            timerProgressBar: true,
            showConfirmButton: false,
            ...swalTheme
        });
    };

    const mostrarError = (mensaje = "Ocurrió un error") => {
        return Swal.fire({
            title: mensaje,
            icon: "error",
            ...swalTheme
        });
    };

    const confirmar = (titulo, texto = "", icon = "question") => {
        return Swal.fire({
            title: titulo,
            text: texto,
            icon,
            showCancelButton: true,
            confirmButtonText: "Confirmar",
            cancelButtonText: "Cancelar",
            reverseButtons: true,
            ...swalTheme
        });
    };

    // =====================================================
    // FETCH CON LOADER GARANTIZADO
    // =====================================================

    const fetchConLoader = async (url, options, mensajeExito) => {
        try {
            mostrarCargando();

            const fetchPromise = fetch(url, options);

            const [response] = await Promise.all([
                fetchPromise,
                delay(MIN_LOADING_TIME)
            ]);

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

    // =====================================================
    // ANTI CACHE EN IMÁGENES
    // =====================================================

    const actualizarImagenesSinCache = () => {
        const timestamp = new Date().getTime();

        document.querySelectorAll("#clients-container img").forEach(img => {
            if (!img.src) return;

            const baseSrc = img.src.split("?")[0];
            img.src = `${baseSrc}?v=${timestamp}`;
        });
    };

    // =====================================================
    // RECARGAR CLIENTES (SIN CACHE)
    // =====================================================

    const recargarClientes = async () => {
        try {
            const response = await fetch(`/Home/ClientesPartial?v=${Date.now()}`, {
                cache: "no-store"
            });

            if (!response.ok) throw new Error();

            const html = await response.text();
            const container = document.getElementById("clients-container");

            if (container) {
                container.innerHTML = html;

                // Forzar refresco de imágenes
                actualizarImagenesSinCache();

                if (typeof AOS !== "undefined") AOS.refresh();
            }

        } catch (error) {
            console.error("Error recargando clientes:", error);
        }
    };

    // =====================================================
    // CARGAR DATOS EN MODAL EDITAR
    // =====================================================

    document.addEventListener("click", (e) => {
        const btn = e.target.closest(".edit-client-btn");
        if (!btn) return;

        const campos = [
            "id", "nombre", "facebook", "twitter",
            "instagram", "telegram", "linkedin",
            "whatsapp", "website"
        ];

        campos.forEach(campo => {
            const input = document.getElementById(`edit-${campo}`);
            if (input) input.value = btn.dataset[campo] || "";
        });
    });

    // =====================================================
    // SUBMIT GENÉRICO AJAX
    // =====================================================

    const manejarSubmitAjax = (form, mensajeConfirmacion, mensajeExito, modal = null) => {
        form.addEventListener("submit", async (e) => {
            e.preventDefault();

            const confirmacion = await confirmar(mensajeConfirmacion);
            if (!confirmacion.isConfirmed) return;

            const ok = await fetchConLoader(
                form.action,
                { method: "POST", body: new FormData(form) },
                mensajeExito
            );

            if (!ok) return;

            if (modal) {
                bootstrap.Modal.getInstance(modal)?.hide();
                form.reset();
            }

            await recargarClientes();
        });
    };

    // =====================================================
    // UPLOAD CLIENTE
    // =====================================================

    const uploadModal = document.getElementById("uploadClienteModal");
    if (uploadModal) {
        const uploadForm = uploadModal.querySelector("form");
        if (uploadForm) {
            manejarSubmitAjax(
                uploadForm,
                "¿Deseas subir este cliente?",
                "Cliente subido correctamente",
                uploadModal
            );
        }
    }

    // =====================================================
    // EDITAR CLIENTE
    // =====================================================

    const editModal = document.getElementById("editClienteModal");
    if (editModal) {
        const editForm = editModal.querySelector("form");
        if (editForm) {
            manejarSubmitAjax(
                editForm,
                "¿Guardar cambios?",
                "Cliente actualizado correctamente",
                editModal
            );
        }
    }

    // =====================================================
    // ELIMINAR CLIENTE
    // =====================================================

    document.addEventListener("click", async (e) => {
        const deleteBtn = e.target.closest(".delete-client-form button");
        if (!deleteBtn) return;

        e.preventDefault();

        const form = deleteBtn.closest("form");
        if (!form) return;

        const confirmacion = await confirmar(
            "¿Eliminar cliente?",
            "Esta acción no se puede deshacer.",
            "warning"
        );

        if (!confirmacion.isConfirmed) return;

        const ok = await fetchConLoader(
            form.action,
            { method: "POST", body: new FormData(form) },
            "Cliente eliminado correctamente"
        );

        if (ok) await recargarClientes();
    });

});