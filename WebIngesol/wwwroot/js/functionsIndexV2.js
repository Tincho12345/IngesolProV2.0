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

    // ---------------- VISTA PREVIA LOGO SIMPLE ----------------
    window.previewLogoUpload = input => {
        const img = document.getElementById('logo-preview-upload');
        if (input.files && input.files[0]) img.src = URL.createObjectURL(input.files[0]), img.style.display = 'block';
        else img.style.display = 'none';
    };
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

        // ---------- CARGAR UBICACIÓN ----------
        const ubicacionInput = document.getElementById("edit-ubicacion");
        const mapa = document.getElementById("edit-mapa");
        if (ubicacionInput && mapa) {
            let lat = btn.dataset.latitud || '';
            let lng = btn.dataset.longitud || '';

            if (lat && lng) {
                // Reemplazar coma por punto en caso de que existan
                lat = lat.replace(',', '.');
                lng = lng.replace(',', '.');

                // Coordenadas para Google Maps
                const coordenadas = `${lat},${lng}`;

                // Mostrar en el input con punto decimal
                ubicacionInput.value = `${lat}, ${lng}`;

                // Zoom más cercano
                const zoomLevel = 18; // 1-21, mientras más alto más cerca

                // Actualizar iframe con Google Maps embebido y pin
                mapa.src = `https://maps.google.com/maps?q=${coordenadas}&hl=es&z=${zoomLevel}&output=embed`;
            } else {
                ubicacionInput.value = '';
                mapa.src = ""; // mapa vacío si no hay coords
            }
        }

        // ---------- PREVIEW DEL LOGO ----------
        const preview = document.getElementById('logo-preview');
        if (preview) preview.src = btn.dataset.logo || '/img/logo-placeholder.png';
    });


    // ---------------- SUBMIT GENÉRICO AJAX ----------------
    const manejarSubmitAjax = (form, mensajeConfirmacion, mensajeExito, modal) => {
        form.addEventListener("submit", async e => {
            e.preventDefault();

            // ---------- CONFIRMACIÓN ----------
            const confirmacion = await confirmar(mensajeConfirmacion);
            if (!confirmacion.isConfirmed) return;

            // ---------- PARSEAR Y NORMALIZAR UBICACIÓN ----------
            const ubicacionInput = form.querySelector('input[name="ubicacion"]');
            if (ubicacionInput && ubicacionInput.value.trim()) {
                const valor = ubicacionInput.value.trim();

                let latitud, longitud;

                // Determinar si usa punto o coma como decimal
                if (valor.includes('.')) {
                    // Formato con punto decimal: "-26.401509831730852, -54.60864114268127"
                    const partes = valor.split(',').map(p => p.trim());
                    if (partes.length !== 2) {
                        await mostrarError("Ubicación inválida. Debe contener latitud y longitud separadas por coma.");
                        return;
                    }
                    latitud = partes[0].replace('.', ',');   // reemplazar punto por coma para enviar
                    longitud = partes[1].replace('.', ',');
                } else if (valor.includes(',')) {
                    // Formato con coma decimal: "-26,401509831730852, -54,60864114268127"
                    // Usamos regex para extraer correctamente los dos números
                    const match = valor.match(/([+-]?\d+,\d+)\s*,?\s*([+-]?\d+,\d+)/);
                    if (!match) {
                        await mostrarError("Ubicación inválida. Revisa el formato: -26,401509831730852, -54,60864114268127");
                        return;
                    }
                    latitud = match[1];
                    longitud = match[2];
                } else {
                    await mostrarError("Ubicación inválida. Debe contener números con coma o punto decimal.");
                    return;
                }

                // Validar números reemplazando temporalmente la coma por punto
                if (isNaN(parseFloat(latitud.replace(',', '.'))) || isNaN(parseFloat(longitud.replace(',', '.')))) {
                    await mostrarError("Ubicación inválida. Revisa los números ingresados.");
                    return;
                }

                // ---------- CREAR O ACTUALIZAR INPUTS OCULTOS ----------
                const setHiddenInput = (name, value) => {
                    let input = form.querySelector(`input[name="${name}"]`);
                    if (!input) {
                        input = document.createElement('input');
                        input.type = 'hidden';
                        input.name = name;
                        form.appendChild(input);
                    }
                    input.value = value; // enviamos siempre con coma decimal
                };

                setHiddenInput('latitud', latitud);
                setHiddenInput('longitud', longitud);
            }

            // ---------- ENVIAR FORMULARIO ----------
            const ok = await fetchConLoader(
                form.action,
                { method: "POST", body: new FormData(form) },
                mensajeExito
            );
            if (!ok) return;

            // ---------- CERRAR MODAL Y RESET ----------
            if (modal) {
                bootstrap.Modal.getInstance(modal)?.hide();
                form.reset();
            }

            // ---------- RECARGAR CLIENTES ----------
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

// ---------------- MAPA EDIT ----------------
window.initEditMap = function () {
    const mapDiv = document.getElementById("edit-map");
    const ubicacionInput = document.getElementById("edit-ubicacion");

    if (!mapDiv) return;

    const valor = ubicacionInput?.value || "-26.402552934085946,-54.62956946211035";
    const [latStr, lngStr] = valor.split(',').map(s => s.trim());
    const lat = parseFloat(latStr.replace(',', '.'));
    const lng = parseFloat(lngStr.replace(',', '.'));

    window.editMap = new google.maps.Map(mapDiv, {
        center: { lat, lng },
        zoom: 18
    });

    window.editMarker = new google.maps.Marker({
        position: { lat, lng },
        map: window.editMap,
        draggable: true
    });

    // Mover pin al hacer click en el mapa
    window.editMap.addListener("click", function (e) {
        const coords = e.latLng;
        window.editMarker.setPosition(coords);
        if (ubicacionInput) {
            ubicacionInput.value = `${coords.lat()}, ${coords.lng()}`;
        }
    });

    // Mover pin al arrastrarlo
    window.editMarker.addListener("dragend", function () {
        if (ubicacionInput) {
            const pos = window.editMarker.getPosition();
            ubicacionInput.value = `${pos.lat()}, ${pos.lng()}`;
        }
    });
};