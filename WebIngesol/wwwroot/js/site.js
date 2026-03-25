document.addEventListener("DOMContentLoaded", () => {
    // ---------- DROPDOWN SUBMENUS ----------
    document.querySelectorAll('.dropdown-submenu').forEach(submenu => {
        submenu.addEventListener('mouseenter', () => {
            const toggle = submenu.querySelector('.dropdown-toggle');
            if (toggle) new bootstrap.Dropdown(toggle).show();
        });
        submenu.addEventListener('mouseleave', () => {
            const toggle = submenu.querySelector('.dropdown-toggle');
            bootstrap.Dropdown.getInstance(toggle)?.hide();
        });
    });

    // ---------- TABLA ----------
    window.bloquearTabla = () => document.getElementById('table-blocker')?.classList.remove('d-none');
    window.desbloquearTabla = () => document.getElementById('table-blocker')?.classList.add('d-none');

    // ---------- GOOGLE MAPS ----------
    window.initGoogleMaps = () => {
        console.log("Google Maps cargado correctamente");
        typeof initCreateMap === "function" && initCreateMap();
        typeof initEditMap === "function" && initEditMap();
    };

    // ---------- HERO VIDEO ----------
    const video = document.getElementById("heroVideo");
    const playBtn = document.getElementById("videoPlayBtn");
    const toggleBtn = document.getElementById("videoToggleBtn");
    const videoInput = document.getElementById("videoInput");
    const heroContent = document.getElementById("heroContent");
    const deleteBtn = document.getElementById("deleteVideoBtn");

    const playHeroVideo = () => video && video.paused && video.play().catch(() => video.controls = true);
    const togglePlay = () => video && (video.paused ? video.play().catch(() => video.controls = true) : video.pause());

    const updateVideoState = () => {
        if (!video || !playBtn || !heroContent) return;
        heroContent.classList.toggle("hidden", !video.paused);
        playBtn.textContent = video.paused ? "▶ Play" : "⏸ Pause";
    };

    if (playBtn) playBtn.addEventListener("click", togglePlay);
    if (video) {
        video.addEventListener("play", updateVideoState);
        video.addEventListener("pause", updateVideoState);
    }
    if (toggleBtn && videoInput) toggleBtn.addEventListener("click", () => videoInput.click());

    if (videoInput) videoInput.addEventListener("change", async () => {
        const file = videoInput.files?.[0];
        if (!file) return;

        video?.pause();
        video?.querySelectorAll("source").forEach(s => s.src = URL.createObjectURL(file));
        video?.load();

        const fd = new FormData();
        fd.append("videoFile", file);
        const token = document.querySelector('[name="__RequestVerificationToken"]')?.value;
        if (token) fd.append("__RequestVerificationToken", token);

        try {
            Swal.fire({ title: "Subiendo video...", allowOutsideClick: false, didOpen: () => Swal.showLoading() });

            const data = await fetch("/VisibleClients/UploadVideo", { method: "POST", body: fd }).then(r => r.json());
            Swal.close();

            if (data?.fileName) {
                await Swal.fire({ title: "Video subido correctamente", icon: "success", timer: 1500, showConfirmButton: false });
                location.reload();
            }

        } catch {
            Swal.close();
            Swal.fire({ title: "Error subiendo el video", icon: "error" });
        }
    });

    if (!window.signalrClientesInit && typeof signalR !== "undefined") {
        window.signalrClientesInit = true;
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/clientsHub")
            .withAutomaticReconnect()
            .build();

        connection.on("VideoActualizado", fileName => {

            if (!fileName) {
                location.reload();
                return;
            }

            if (!video) return;

            video.querySelectorAll("source").forEach(src =>
                src.src = `/videos/${fileName}?v=${Date.now()}`
            );

            video.load();
        });

        connection.start().then(() => console.log("Conectado a SignalR"))
            .catch(err => console.error("Error conectando a SignalR:", err));
    }
    window.playHeroVideo = playHeroVideo;

    // ---------- ELIMINAR VIDEO ----------
    if (deleteBtn) deleteBtn.addEventListener("click", async () => {

        const confirm = await Swal.fire({
            title: "Eliminar video?",
            text: "Esta acción eliminará el video del sitio.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Eliminar",
            cancelButtonText: "Cancelar"
        });

        if (!confirm.isConfirmed) return;

        const token = document.querySelector('[name="__RequestVerificationToken"]')?.value;

        try {

            Swal.fire({
                title: "Eliminando video...",
                allowOutsideClick: false,
                didOpen: () => Swal.showLoading()
            });

            const res = await fetch("/VisibleClients/DeleteVideo", {
                method: "POST",
                headers: {
                    "RequestVerificationToken": token,
                    "Content-Type": "application/json"
                }
            });

            const data = await res.json();
            Swal.close();

            if (data?.success) {
                await Swal.fire({
                    title: "Video eliminado",
                    icon: "success",
                    timer: 1500,
                    showConfirmButton: false
                });

                location.reload();
            }

        } catch {
            Swal.close();
            Swal.fire({
                title: "Error eliminando el video",
                icon: "error"
            });
        }

    });
});