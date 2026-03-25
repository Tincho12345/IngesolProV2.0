document.addEventListener("DOMContentLoaded", () => {

    // ---------------- CONFIG ----------------
    const swalTheme = { background: '#1e1e2f', color: '#ffffff' };
    const MIN_LOADING_TIME = 700;
    const delay = ms => new Promise(res => setTimeout(res, ms));

    // ---------------- SIGNALR ----------------
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/clientsHub")
        .withAutomaticReconnect()
        .build();

    connection.on("ClientesActualizados", recargarClientes);
    connection.start().then(() => console.log("Conectado a SignalR"))
        .catch(err => console.error("Error conectando a SignalR:", err));

    // ---------------- SWEETALERT ----------------
    const mostrarCargando = msg => Swal.fire({ title: msg || "Procesando...", allowOutsideClick: false, allowEscapeKey: false, didOpen: () => Swal.showLoading(), ...swalTheme });
    // Solo devuelve la promesa, no hacemos await aquí
    const mostrarExito = msg => Swal.fire({
        title: msg,
        icon: "success",
        timer: 1500,
        timerProgressBar: true,
        showConfirmButton: false,
        ...swalTheme
    });
    const mostrarError = msg => Swal.fire({ title: msg || "Ocurrió un error", icon: "error", ...swalTheme });
    const confirmar = (title, text = "", icon = "question") => Swal.fire({ title, text, icon, showCancelButton: true, confirmButtonText: "Confirmar", cancelButtonText: "Cancelar", reverseButtons: true, ...swalTheme });

    // ---------------- FETCH CON LOADER ----------------
    async function fetchConLoader(url, options) {  // quitamos mensajeExito de aquí
        try {
            mostrarCargando();
            const [response] = await Promise.all([fetch(url, options), delay(MIN_LOADING_TIME)]);
            if (!response.ok) throw new Error("Error en la operación");
            Swal.close(); // solo cerrar loader
            return true;
        } catch (error) {
            Swal.close();
            await mostrarError(error.message);
            return false;
        }
    }

    // ---------------- RECARGAR CLIENTES ----------------
    async function recargarClientes() {
        try {
            const response = await fetch(`/Home/ClientesPartial?v=${Date.now()}`, { cache: "no-store" });
            if (!response.ok) throw new Error();
            const html = await response.text();
            const container = document.getElementById("clients-container");
            if (container) {
                container.innerHTML = html;
                document.querySelectorAll("#clients-container img").forEach(img => { if (img.src) img.src = img.src.split("?")[0] + "?v=" + Date.now(); });
                if (typeof AOS !== "undefined") AOS.refresh();
            }
        } catch (error) {
            console.error("Error recargando clientes:", error);
        }
    }

    // ---------------- VISTAS PREVIA LOGO ----------------
    window.previewLogoUpload = input => {
        const img = document.getElementById('logo-preview-upload');
        if (input.files && input.files[0]) img.src = URL.createObjectURL(input.files[0]), img.style.display = 'block';
        else img.style.display = 'none';
    };

    window.previewLogo = input => {
        const preview = document.getElementById('logo-preview');
        if (!preview) return;
        if (input.files && input.files[0]) preview.src = URL.createObjectURL(input.files[0]);
        else preview.src = '/img/logo-placeholder.png';
    };

    // ---------------- RELLENAR FORMULARIO EDITAR ----------------
    document.addEventListener("click", e => {
        const btn = e.target.closest(".edit-client-btn");
        if (!btn) return;

        ["id", "nombre", "facebook", "twitter", "instagram", "telegram", "linkedin", "whatsapp", "website"]
            .forEach(campo => { const input = document.getElementById(`edit-${campo}`); if (input) input.value = btn.dataset[campo] || ""; });

        const ubicacionInput = document.getElementById("edit-ubicacion");
        if (ubicacionInput) {
            const lat = btn.dataset.latitud || '';
            const lng = btn.dataset.longitud || '';
            ubicacionInput.value = (lat && lng) ? `${lat.replace(',', '.')}, ${lng.replace(',', '.')}` : '';
        }

        const preview = document.getElementById('logo-preview');
        if (preview) preview.src = btn.dataset.logo || '/img/logo-placeholder.png';
    });

    // ---------------- SUBMIT AJAX ----------------
    function manejarSubmitAjax(form, mensajeConfirmacion, mensajeExito, modal) {
        form.addEventListener("submit", async e => {
            e.preventDefault();
            const confirmacion = await confirmar(mensajeConfirmacion);
            if (!confirmacion.isConfirmed) return;

            const ubicacionInput = form.querySelector('input[name="ubicacion"]');
            if (ubicacionInput?.value.trim()) {
                const { latitud, longitud } = parseUbicacion(ubicacionInput.value.trim());
                if (!latitud || !longitud) return;
                setHiddenInput(form, 'latitud', latitud);
                setHiddenInput(form, 'longitud', longitud);
            }

            const ok = await fetchConLoader(form.action, { method: "POST", body: new FormData(form) });
            if (!ok) return;

            if (modal) { bootstrap.Modal.getInstance(modal)?.hide(); form.reset(); }

            // <-- Esperar que el SweetAlert termine antes de recargar clientes
            await mostrarExito(mensajeExito);
            await recargarClientes();
        });
    }

    function setHiddenInput(form, name, value) {
        let input = form.querySelector(`input[name="${name}"]`);
        if (!input) { input = document.createElement('input'); input.type = 'hidden'; input.name = name; form.appendChild(input); }
        input.value = value;
    }

    function parseUbicacion(valor) {
        valor = valor.trim();
        if (!valor) {
            mostrarError("Ubicación vacía");
            return {};
        }

        let latitud, longitud;

        // Detectamos si los decimales usan punto
        const tienePunto = valor.includes('.');

        if (tienePunto) {
            // Caso: decimales con punto → separar por última coma
            const ultimaComaIndex = valor.lastIndexOf(',');
            if (ultimaComaIndex === -1) {
                mostrarError("Ubicación inválida");
                return {};
            }
            latitud = valor.substring(0, ultimaComaIndex).trim();
            longitud = valor.substring(ultimaComaIndex + 1).trim();
            // Convertimos puntos a comas para enviar al backend
            if (!latitud.includes(',')) latitud = latitud.replace('.', ',');
            if (!longitud.includes(',')) longitud = longitud.replace('.', ',');
        } else {
            // Caso: decimales con coma → separar por la **coma que divide lat/lng**
            // Suponemos que la coma separadora es la que está entre los dos números
            // Regex: tomar primer número y segundo número
            const match = valor.match(/^(-?\d+,\d+)\s*,\s*(-?\d+,\d+)$/);
            if (!match) {
                mostrarError("Ubicación inválida");
                return {};
            }
            latitud = match[1].trim();
            longitud = match[2].trim();
        }

        // Validar que sean números válidos usando parseFloat (punto como decimal)
        const latNum = parseFloat(latitud.replace(',', '.'));
        const lngNum = parseFloat(longitud.replace(',', '.'));
        if (isNaN(latNum) || isNaN(lngNum)) {
            mostrarError("Ubicación inválida");
            return {};
        }

        // Retornamos los valores listos para enviar al backend
        return { latitud, longitud };
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

        const ok = await fetchConLoader(form.action, { method: "POST", body: new FormData(form) });
        if (!ok) return;

        // <-- Ocultar modal si corresponde
        const modal = deleteBtn.closest('.modal');
        if (modal) bootstrap.Modal.getInstance(modal)?.hide();

        // <-- Mostrar SweetAlert de éxito
        await mostrarExito("Cliente eliminado correctamente");
    });

    // ---------------- VOZ ----------------
    let lecturaEnCurso = false;
    let vozTimeout;

    function leerTexto(texto, mostrarHora = false, coords = null) {
        if (!('speechSynthesis' in window) || lecturaEnCurso) return;
        lecturaEnCurso = true;

        if (coords) {
            const lat = typeof coords.lat === 'function' ? coords.lat() : coords.lat;
            const lng = typeof coords.lng === 'function' ? coords.lng() : coords.lng;
            if (lat.toFixed(8) === '-26.84640483' && lng.toFixed(8) === '-65.26726009') {
                texto += ". Por acá vive el tucumano";
            }
        }

        const utterance = new SpeechSynthesisUtterance(texto);
        utterance.lang = 'es-ES';
        utterance.onend = () => {
            lecturaEnCurso = false;
            if (mostrarHora) {
                const hora = new Date().toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' });
                const temperatura = "25°C";
                const utteranceFinal = new SpeechSynthesisUtterance(`Hora: ${hora}, temperatura: ${temperatura}.`);
                utteranceFinal.lang = 'es-ES';
                window.speechSynthesis.speak(utteranceFinal);
            }
        };
        window.speechSynthesis.speak(utterance);
    }

    function leerDireccion(coords, mostrarHora = false) {
        clearTimeout(vozTimeout);
        vozTimeout = setTimeout(() => {
            const geocoder = new google.maps.Geocoder();
            geocoder.geocode({ location: coords }, (results, status) => {
                if (status === 'OK' && results[0]) {
                    leerTexto(`Ubicación: ${results[0].formatted_address}`, mostrarHora, coords);
                }
            });
        }, 500);
    }

    // ---------------- MAPAS ----------------
    const watchIdMap = {};

    function normalizarCoords(coords) {
        return {
            lat: typeof coords.lat === 'function' ? coords.lat() : coords.lat,
            lng: typeof coords.lng === 'function' ? coords.lng() : coords.lng
        };
    }

    // ---------------- FUNCIONES GEO ----------------
    function actualizarMapaYInput(coords, input, mapObj, leerVoz = false) {
        const { lat, lng } = normalizarCoords(coords);
        if (input) input.value = `${lat.toFixed(8)}, ${lng.toFixed(8)}`;
        if (mapObj) {
            mapObj.marker.setPosition(coords);
            mapObj.map.setCenter(coords);
        }
        if (leerVoz) leerDireccion(coords, true);
    }

    function usarUbicacionActual(inputId, mapId, leerVoz = true) {
        const input = document.getElementById(inputId);
        const mapObj = window[mapId];
        if (!navigator.geolocation) { mostrarError("Geolocalización no soportada"); return; }

        navigator.geolocation.getCurrentPosition(
            pos => {
                const coords = new google.maps.LatLng(pos.coords.latitude, pos.coords.longitude);
                actualizarMapaYInput(coords, input, mapObj, leerVoz);
            },
            err => mostrarError("No se pudo obtener la ubicación actual: " + err.message),
            { enableHighAccuracy: true }
        );
    }

    function usarUbicacionActualTiempoReal(inputId, mapId) {
        const input = document.getElementById(inputId);
        const mapObj = window[mapId];
        if (!navigator.geolocation) { mostrarError("Geolocalización no soportada"); return; }

        if (watchIdMap[inputId]) navigator.geolocation.clearWatch(watchIdMap[inputId]);

        watchIdMap[inputId] = navigator.geolocation.watchPosition(
            pos => {
                const coords = { lat: pos.coords.latitude, lng: pos.coords.longitude };
                actualizarMapaYInput(coords, input, mapObj, false); // NO leer voz en tiempo real
            },
            err => mostrarError("No se pudo obtener la ubicación en tiempo real: " + err.message),
            { enableHighAccuracy: true, maximumAge: 1000, timeout: 5000 }
        );
    }

    // Función para actualizar mapa y voz al cambiar el input manualmente
    function escucharCambioInputUbicacion(inputId, mapId) {
        const input = document.getElementById(inputId);
        if (!input) return;

        const actualizarDesdeInput = () => {
            const mapObj = window[mapId]; // <- mover dentro para asegurarse que existe
            if (!mapObj) return;           // <- si no existe, no hacemos nada

            const valor = input.value.trim();
            if (!valor) return;

            const { latitud, longitud } = parseUbicacion(valor);
            if (!latitud || !longitud) return;

            const coords = new google.maps.LatLng(
                parseFloat(latitud.replace(',', '.')),
                parseFloat(longitud.replace(',', '.'))
            );
            actualizarMapaYInput(coords, input, mapObj, true);
        };

        input.addEventListener("input", actualizarDesdeInput);
        input.addEventListener("paste", () => setTimeout(actualizarDesdeInput, 0));
        input.addEventListener("keyup", actualizarDesdeInput);
    }

    // ---------------- INICIALIZACIÓN DE MAPA ----------------
    function initMap({ mapId, searchId, inputId, defaultCenter, zoom = 18 }) {
        const mapDiv = document.getElementById(mapId);
        const ubicacionInput = document.getElementById(inputId);
        if (!mapDiv) return;

        let valor = ubicacionInput?.value || `${defaultCenter.lat},${defaultCenter.lng}`;
        const partes = valor.split(',');
        const center = { lat: parseFloat(partes[0].trim()), lng: parseFloat(partes[1].trim()) };

        const map = new google.maps.Map(mapDiv, { center, zoom });
        const marker = new google.maps.Marker({ position: center, map, draggable: true });

        // Lectura de voz al mover el pin
        marker.addListener("dragend", () => actualizarMapaYInput(marker.getPosition(), ubicacionInput, { map, marker }, true));
        map.addListener("click", e => actualizarMapaYInput(e.latLng, ubicacionInput, { map, marker }, true));

        if (searchId) {
            const searchInput = document.getElementById(searchId);
            if (searchInput) {
                const autocomplete = new google.maps.places.Autocomplete(searchInput);
                autocomplete.addListener("place_changed", () => {
                    const place = autocomplete.getPlace();
                    if (!place.geometry) return;

                    const location = place.geometry.location;

                    marker.setPosition(location);
                    map.setCenter(location);

                    actualizarMapaYInput(location, ubicacionInput, { map, marker }, true);
                });
            }
        }

        // 🔥 Aquí agregamos el listener de input MANUAL
        if (ubicacionInput) {
            escucharCambioInputUbicacion(inputId, mapId);
        }

        window[mapId] = { map, marker };
    }

    // ---------------- LIMPIAR FORMULARIOS AL CERRAR MODALES ----------------
    function limpiarModal(modalId) {
        const modal = document.getElementById(modalId);
        if (!modal) return;

        modal.addEventListener("hidden.bs.modal", () => {

            const form = modal.querySelector("form");
            if (form) form.reset();

            // limpiar previews de logos
            const previewUpload = modal.querySelector("#logo-preview-upload");
            if (previewUpload) {
                previewUpload.src = "";
                previewUpload.style.display = "none";
            }

            const previewEdit = modal.querySelector("#logo-preview");
            if (previewEdit) {
                previewEdit.src = "/img/logo-placeholder.png";
            }

            // limpiar ubicación
            const ubicacion = modal.querySelector('input[name="ubicacion"]');
            if (ubicacion) ubicacion.value = "";

        });
    }

    limpiarModal("uploadClienteModal");
    limpiarModal("editClienteModal");

    // ---------------- INICIALIZAR MODALES ----------------
    [
        { modalId: "uploadClienteModal", formMsg: "¿Deseas subir este cliente?", successMsg: "Cliente subido correctamente", mapId: "create-map", searchId: "create-search", inputId: "create-ubicacion" },
        { modalId: "editClienteModal", formMsg: "¿Guardar cambios?", successMsg: "Cliente actualizado correctamente", mapId: "edit-map", searchId: "edit-search", inputId: "edit-ubicacion" }
    ].forEach(cfg => {
        const modal = document.getElementById(cfg.modalId);
        if (!modal) return;
        const form = modal.querySelector("form");
        if (form) manejarSubmitAjax(form, cfg.formMsg, cfg.successMsg, modal);
        modal.addEventListener("shown.bs.modal", () => setTimeout(() => initMap({ mapId: cfg.mapId, searchId: cfg.searchId, inputId: cfg.inputId, defaultCenter: { lat: -26.402552934085946, lng: -54.62956946211035 }, zoom: 15 }), 500));
    });

    // ---------------- BOTONES UBICACIÓN ACTUAL ----------------
    document.getElementById('btn-create-ubicacion-actual')?.addEventListener('click', () => usarUbicacionActual('create-ubicacion', 'create-map'));
    document.getElementById('btn-edit-ubicacion-actual')?.addEventListener('click', () => usarUbicacionActual('edit-ubicacion', 'edit-map'));

});