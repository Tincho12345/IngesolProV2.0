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
        const partes = valor.split(',').map(p => p.trim());
        if (partes.length !== 2) { mostrarError("Ubicación inválida"); return {}; }
        const [latitud, longitud] = partes;
        if (isNaN(parseFloat(latitud.replace(',', '.'))) || isNaN(parseFloat(longitud.replace(',', '.')))) { mostrarError("Ubicación inválida"); return {}; }
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
    let vozTimeout, lecturaEnCurso = false;

    function leerTexto(texto, mostrarHora = false) {
        if (!('speechSynthesis' in window) || lecturaEnCurso) return;
        lecturaEnCurso = true;

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
                if (status === 'OK' && results[0]) leerTexto(`Ubicación: ${results[0].formatted_address}`, mostrarHora);
            });
        }, 500);
    }

    // ---------------- MAPAS ----------------
    const watchIdMap = {};

    function initMap({ mapId, searchId, inputId, defaultCenter, zoom = 18 }) {
        const mapDiv = document.getElementById(mapId);
        const ubicacionInput = document.getElementById(inputId);
        if (!mapDiv) return;

        let valor = ubicacionInput?.value || `${defaultCenter.lat},${defaultCenter.lng}`;
        const partes = valor.split(',');
        const center = { lat: parseFloat(partes[0].trim()), lng: parseFloat(partes[1].trim()) };

        const map = new google.maps.Map(mapDiv, { center, zoom });
        const marker = new google.maps.Marker({ position: center, map, draggable: true });

        const actualizar = (coords, leer = true) => {
            if (ubicacionInput) ubicacionInput.value = `${coords.lat().toFixed(8)}, ${coords.lng().toFixed(8)}`;
            if (leer) leerDireccion(coords); // solo leer si no es tiempo real
        };

        map.addListener("click", e => { marker.setPosition(e.latLng); actualizar(e.latLng); });
        marker.addListener("dragend", () => actualizar(marker.getPosition()));

        const searchInput = document.getElementById(searchId);
        if (searchInput) {
            const autocomplete = new google.maps.places.Autocomplete(searchInput);
            autocomplete.addListener("place_changed", () => {
                const place = autocomplete.getPlace();
                if (!place.geometry) return;
                const location = place.geometry.location;
                map.setCenter(location);
                map.setZoom(18);
                marker.setPosition(location);
                actualizar(location); // leer dirección
            });
        }

        window[mapId] = { map, marker };
    }

    // ---------------- USAR UBICACIÓN ACTUAL ----------------
    function usarUbicacionActual(inputId, mapId) {
        const input = document.getElementById(inputId);
        const mapObj = window[mapId];
        if (!navigator.geolocation) { mostrarError("Geolocalización no soportada"); return; }

        navigator.geolocation.getCurrentPosition(
            pos => {
                const coords = new google.maps.LatLng(pos.coords.latitude, pos.coords.longitude);
                if (input) input.value = `${coords.lat().toFixed(8)}, ${coords.lng().toFixed(8)}`;
                if (mapObj) { mapObj.marker.setPosition(coords); mapObj.map.setCenter(coords); mapObj.map.setZoom(18); }
                leerDireccion(coords, true); // leer voz solo al botón
            },
            err => mostrarError("No se pudo obtener la ubicación actual: " + err.message),
            { enableHighAccuracy: true }
        );
    }

    // ---------------- USO TIEMPO REAL ----------------
    function usarUbicacionActualTiempoReal(inputId, mapId) {
        const input = document.getElementById(inputId);
        const mapObj = window[mapId];
        if (!navigator.geolocation) { mostrarError("Geolocalización no soportada"); return; }

        if (watchIdMap[inputId]) navigator.geolocation.clearWatch(watchIdMap[inputId]);

        watchIdMap[inputId] = navigator.geolocation.watchPosition(
            pos => {
                const coords = { lat: pos.coords.latitude, lng: pos.coords.longitude };
                if (input) input.value = `${coords.lat.toFixed(8)}, ${coords.lng.toFixed(8)}`;
                if (mapObj) {
                    mapObj.marker.setPosition(coords);
                    mapObj.map.setCenter(coords);
                    mapObj.map.setZoom(18);
                }
                // NO llamar a leerDireccion aquí para evitar bucle infinito
            },
            err => mostrarError("No se pudo obtener la ubicación en tiempo real: " + err.message),
            { enableHighAccuracy: true, maximumAge: 1000, timeout: 5000 }
        );
    }

    // ---------------- USAR UBICACIÓN ACTUAL ----------------
    function usarUbicacionActual(inputId, mapId) {
        const input = document.getElementById(inputId);
        const mapObj = window[mapId];
        if (!navigator.geolocation) { mostrarError("Geolocalización no soportada"); return; }

        navigator.geolocation.getCurrentPosition(
            pos => {
                const coords = new google.maps.LatLng(pos.coords.latitude, pos.coords.longitude);
                if (input) input.value = `${coords.lat().toFixed(8)}, ${coords.lng().toFixed(8)}`;
                if (mapObj) { mapObj.marker.setPosition(coords); mapObj.map.setCenter(coords); mapObj.map.setZoom(18); }
                leerDireccion(coords, true); // leer voz solo al botón
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
                if (input) input.value = `${coords.lat.toFixed(8)}, ${coords.lng.toFixed(8)}`;
                if (mapObj) { mapObj.marker.setPosition(coords); mapObj.map.setCenter(coords); mapObj.map.setZoom(18); }
            },
            err => mostrarError("No se pudo obtener la ubicación en tiempo real: " + err.message),
            { enableHighAccuracy: true, maximumAge: 1000, timeout: 5000 }
        );
    }

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