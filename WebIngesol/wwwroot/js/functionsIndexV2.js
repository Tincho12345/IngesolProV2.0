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

    if (!window.signalrClientesInit) {
        window.signalrClientesInit = true;
        let skipNextUpdate = false;

        connection.on("ClientesActualizados", async () => {
            if (skipNextUpdate) { skipNextUpdate = false; return; }
            await recargarClientes();
        });

        connection.start()
            .then(() => console.log("Conectado a SignalR"))
            .catch(err => console.error("Error conectando a SignalR:", err));
    }

    // ---------------- SWEETALERT ----------------
    const mostrarCargando = msg => Swal.fire({
        title: msg || "Procesando...",
        allowOutsideClick: false,
        allowEscapeKey: false,
        didOpen: () => Swal.showLoading(),
        ...swalTheme
    });

    const mostrarExito = msg => Swal.fire({
        title: msg,
        icon: "success",
        timer: 1500,
        timerProgressBar: true,
        showConfirmButton: false,
        ...swalTheme
    });

    const mostrarError = msg => Swal.fire({
        title: msg || "Ocurrió un error",
        icon: "error",
        ...swalTheme
    });

    const confirmar = (title, text = "", icon = "question") => Swal.fire({
        title,
        text,
        icon,
        showCancelButton: true,
        confirmButtonText: "Confirmar",
        cancelButtonText: "Cancelar",
        reverseButtons: true,
        ...swalTheme
    });

    // ---------------- FETCH CON LOADER ----------------
    async function fetchConLoader(url, options) {
        try {
            mostrarCargando();
            const [response] = await Promise.all([fetch(url, options), delay(MIN_LOADING_TIME)]);
            if (!response.ok) throw new Error("Error en la operación");
            Swal.close();
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
            if (!response.ok) throw new Error("No se pudo cargar la lista de clientes");

            const html = await response.text();
            const container = document.getElementById("clients-container");
            if (!container) return;

            container.innerHTML = html;
            container.querySelectorAll("img").forEach(img => {
                if (img.src) img.src = img.src.split("?")[0] + "?v=" + Date.now();
            });

            if (typeof AOS !== "undefined") AOS.refresh();
        } catch (error) {
            console.error("Error recargando clientes:", error);
        }
    }

    function manejarSubmitAjax(form, mensajeConfirmacion, mensajeExito, modal) {
        form.addEventListener("submit", async e => {
            e.preventDefault();
            const confirmacion = await confirmar(mensajeConfirmacion);
            if (!confirmacion.isConfirmed) return;

            const ubicacionInput = form.querySelector('input[name="ubicacion"]');
            if (ubicacionInput?.value.trim()) {
                let { latitud, longitud } = parseUbicacion(ubicacionInput.value.trim());
                if (!latitud || !longitud) return;

                // ✅ Convertir puntos a comas para enviar al backend
                latitud = latitud.replace('.', ',');
                longitud = longitud.replace('.', ',');

                setHiddenInput(form, 'latitud', latitud);
                setHiddenInput(form, 'longitud', longitud);
            }

            const ok = await fetchConLoader(form.action, { method: "POST", body: new FormData(form) });
            if (!ok) return;

            if (modal) { bootstrap.Modal.getInstance(modal)?.hide(); form.reset(); }

            await mostrarExito(mensajeExito);

            // ⚡ SignalR: avisar a otros
            connection.invoke("NotificarActualizacionClientes").catch(err => console.error(err));
        });
    }

    // ---------------- ELIMINAR CLIENTE ----------------
    document.addEventListener("click", async e => {
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

        const ok = await fetchConLoader(form.action, { method: "POST", body: new FormData(form) });
        if (!ok) return;

        const cardWrapper = deleteBtn.closest(".glass-card-wrapper");
        if (cardWrapper) {
            const altura = cardWrapper.offsetHeight;
            const estilo = getComputedStyle(cardWrapper);
            const marginTop = parseFloat(estilo.marginTop);
            const marginBottom = parseFloat(estilo.marginBottom);

            cardWrapper.style.transition = "transform 1.2s cubic-bezier(0.25,1,0.5,1), opacity 1.2s cubic-bezier(0.25,1,0.5,1), margin 1.2s cubic-bezier(0.25,1,0.5,1), height 1.2s cubic-bezier(0.25,1,0.5,1)";
            cardWrapper.style.height = `${altura}px`;
            cardWrapper.style.marginTop = `${marginTop}px`;
            cardWrapper.style.marginBottom = `${marginBottom}px`;

            cardWrapper.offsetHeight; // fuerza reflow

            requestAnimationFrame(() => {
                cardWrapper.style.transform = "translateY(-20px) scale(0.95)";
                cardWrapper.style.opacity = "0";
                cardWrapper.style.height = "0px";
                cardWrapper.style.marginTop = "0px";
                cardWrapper.style.marginBottom = "0px";
            });

            cardWrapper.addEventListener("transitionend", () => cardWrapper.remove(), { once: true });
        }

        const modal = deleteBtn.closest('.modal');
        if (modal) bootstrap.Modal.getInstance(modal)?.hide();

        await mostrarExito("Cliente eliminado correctamente");

        connection.invoke("NotificarActualizacionClientes").catch(err => console.error(err));
    });

    // ---------------- VISTA PREVIA LOGO ----------------
    window.previewLogoUpload = input => {
        const img = document.getElementById('logo-preview-upload');
        if (!img) return;
        if (input.files && input.files[0]) {
            img.src = URL.createObjectURL(input.files[0]);
            img.style.display = 'block';
        } else {
            img.style.display = 'none';
        }
    };

    window.previewLogo = input => {
        const preview = document.getElementById('logo-preview');
        if (!preview) return;
        preview.src = (input.files && input.files[0]) ? URL.createObjectURL(input.files[0]) : '/img/logo-placeholder.png';
    };

    // ---------------- RELLENAR FORMULARIO EDITAR ----------------
    document.addEventListener("click", e => {
        const btn = e.target.closest(".edit-client-btn");
        if (!btn) return;

        ["id", "nombre", "facebook", "twitter", "instagram", "telegram", "linkedin", "whatsapp", "website"]
            .forEach(campo => {
                const input = document.getElementById(`edit-${campo}`);
                if (input) input.value = btn.dataset[campo] || "";
            });

        const ubicacionInput = document.getElementById("edit-ubicacion");
        if (ubicacionInput) {
            const lat = btn.dataset.latitud || '';
            const lng = btn.dataset.longitud || '';
            ubicacionInput.value = (lat && lng) ? `${lat.replace(',', '.')}, ${lng.replace(',', '.')}` : '';
        }

        const preview = document.getElementById('logo-preview');
        if (preview) preview.src = btn.dataset.logo || '/img/logo-placeholder.png';
    });

    function setHiddenInput(form, name, value) {
        let input = form.querySelector(`input[name="${name}"]`);
        if (!input) {
            input = document.createElement('input');
            input.type = 'hidden';
            input.name = name;
            form.appendChild(input);
        }
        input.value = value;
    }

    function parseUbicacion(valor) {
        valor = valor.trim();
        if (!valor) { mostrarError("Ubicación vacía"); return {}; }

        let latitud, longitud;
        const tienePunto = valor.includes('.');

        if (tienePunto) {
            const ultimaComaIndex = valor.lastIndexOf(',');
            if (ultimaComaIndex === -1) { mostrarError("Ubicación inválida"); return {}; }
            latitud = valor.substring(0, ultimaComaIndex).trim();
            longitud = valor.substring(ultimaComaIndex + 1).trim();
        } else {
            const match = valor.match(/^(-?\d+,\d+)\s*,\s*(-?\d+,\d+)$/);
            if (!match) { mostrarError("Ubicación inválida"); return {}; }
            latitud = match[1].trim();
            longitud = match[2].trim();
        }

        const latNum = parseFloat(latitud.replace(',', '.'));
        const lngNum = parseFloat(longitud.replace(',', '.'));
        if (isNaN(latNum) || isNaN(lngNum)) { mostrarError("Ubicación inválida"); return {}; }

        return { latitud, longitud };
    }

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
                window.speechSynthesis.speak(new SpeechSynthesisUtterance(`Hora: ${hora}, temperatura: ${temperatura}.`));
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
        return { lat: typeof coords.lat === 'function' ? coords.lat() : coords.lat, lng: typeof coords.lng === 'function' ? coords.lng() : coords.lng };
    }

    function actualizarMapaYInput(coords, input, mapObj, leerVoz = false) {
        const { lat, lng } = normalizarCoords(coords);
        if (input) input.value = `${lat.toFixed(8)}, ${lng.toFixed(8)}`;
        if (mapObj) { mapObj.marker.setPosition(coords); mapObj.map.setCenter(coords); }
        if (leerVoz) leerDireccion(coords, true);
    }

    function usarUbicacionActual(inputId, mapId, leerVoz = true) {
        const input = document.getElementById(inputId);
        const mapObj = window[mapId];
        if (!navigator.geolocation) { mostrarError("Geolocalización no soportada"); return; }

        navigator.geolocation.getCurrentPosition(
            pos => actualizarMapaYInput(new google.maps.LatLng(pos.coords.latitude, pos.coords.longitude), input, mapObj, leerVoz),
            err => mostrarError("No se pudo obtener la ubicación actual: " + err.message),
            { enableHighAccuracy: true }
        );
    }

    function initMap({ mapId, searchId, inputId, defaultCenter, zoom = 18 }) {
        const mapDiv = document.getElementById(mapId);
        const ubicacionInput = document.getElementById(inputId);
        if (!mapDiv) return;

        const valor = ubicacionInput?.value || `${defaultCenter.lat},${defaultCenter.lng}`;
        const partes = valor.split(',');
        const center = { lat: parseFloat(partes[0].trim()), lng: parseFloat(partes[1].trim()) };

        const map = new google.maps.Map(mapDiv, { center, zoom });
        const marker = new google.maps.Marker({ position: center, map, draggable: true });

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

        if (ubicacionInput) escucharCambioInputUbicacion(inputId, mapId);
        window[mapId] = { map, marker };
    }

    function escucharCambioInputUbicacion(inputId, mapId) {
        const input = document.getElementById(inputId);
        if (!input) return;

        ["input", "paste", "keyup"].forEach(evt => input.addEventListener(evt, () => {
            const valor = input.value.trim();
            if (!valor) return;
            const { latitud, longitud } = parseUbicacion(valor);
            if (!latitud || !longitud) return;

            const mapObj = window[mapId];
            if (!mapObj) return;

            const coords = new google.maps.LatLng(parseFloat(latitud.replace(',', '.')), parseFloat(longitud.replace(',', '.')));
            actualizarMapaYInput(coords, input, mapObj, true);
        }));
    }

    // ---------------- LIMPIAR FORMULARIOS AL CERRAR MODALES ----------------
    function limpiarModal(modalId) {
        const modal = document.getElementById(modalId);
        if (!modal) return;

        modal.addEventListener("hidden.bs.modal", () => {
            const form = modal.querySelector("form");
            if (form) form.reset();

            const previewUpload = modal.querySelector("#logo-preview-upload");
            if (previewUpload) { previewUpload.src = ""; previewUpload.style.display = "none"; }

            const previewEdit = modal.querySelector("#logo-preview");
            if (previewEdit) previewEdit.src = "/img/logo-placeholder.png";

            const ubicacion = modal.querySelector('input[name="ubicacion"]');
            if (ubicacion) ubicacion.value = "";
        });
    }

    ["uploadClienteModal", "editClienteModal"].forEach(limpiarModal);

    // ---------------- INICIALIZAR MODALES Y FORMULARIOS ----------------
    [
        { modalId: "uploadClienteModal", formMsg: "¿Deseas subir este cliente?", successMsg: "Cliente subido correctamente", mapId: "create-map", searchId: "create-search", inputId: "create-ubicacion" },
        { modalId: "editClienteModal", formMsg: "¿Guardar cambios?", successMsg: "Cliente actualizado correctamente", mapId: "edit-map", searchId: "edit-search", inputId: "edit-ubicacion" }
    ].forEach(cfg => {
        const modal = document.getElementById(cfg.modalId);
        if (!modal) return;
        const form = modal.querySelector("form");
        if (form) manejarSubmitAjax(form, cfg.formMsg, cfg.successMsg, modal);
        modal.addEventListener("shown.bs.modal", () => setTimeout(() => initMap({
            mapId: cfg.mapId,
            searchId: cfg.searchId,
            inputId: cfg.inputId,
            defaultCenter: { lat: -26.40255293, lng: -54.62956946 },
            zoom: 15
        }), 500));
    });

    // ---------------- BOTONES UBICACIÓN ACTUAL ----------------
    document.getElementById('btn-create-ubicacion-actual')?.addEventListener('click', () => usarUbicacionActual('create-ubicacion', 'create-map'));
    document.getElementById('btn-edit-ubicacion-actual')?.addEventListener('click', () => usarUbicacionActual('edit-ubicacion', 'edit-map'));

});