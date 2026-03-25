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

    if (videoInput) {
        videoInput.addEventListener("change", async () => {
            const file = videoInput.files[0];
            if (!file) return;
            if (video) {
                video.pause();
                video.querySelectorAll("source").forEach(src => src.src = URL.createObjectURL(file));
                video.load();
            }
            const formData = new FormData();
            formData.append("videoFile", file);
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            token && formData.append("__RequestVerificationToken", token);

            try {
                Swal.fire({ title: "Subiendo video...", allowOutsideClick: false, didOpen: () => Swal.showLoading() });
                const res = await fetch("/VisibleClients/UploadVideo", { method: "POST", body: formData });
                const data = await res.json();
                Swal.close();

                if (data?.fileName && video) {
                    await Swal.fire({ title: "Video subido correctamente", icon: "success", timer: 1500, showConfirmButton: false });
                    video.querySelectorAll("source").forEach(src => src.src = `/videos/${data.fileName}?v=${Date.now()}`);
                    video.load();
                }
            } catch {
                Swal.close();
                await Swal.fire({ title: "Error subiendo el video", icon: "error" });
            }
        });
    }

    if (!window.signalrClientesInit && typeof signalR !== "undefined") {
        window.signalrClientesInit = true;
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/clientsHub")
            .withAutomaticReconnect()
            .build();

        connection.on("VideoActualizado", fileName => {
            if (!video) return;
            video.querySelectorAll("source").forEach(src => src.src = `/videos/${fileName}?v=${Date.now()}`);
            video.load();
        });

        connection.start().then(() => console.log("Conectado a SignalR"))
            .catch(err => console.error("Error conectando a SignalR:", err));
    }
    window.playHeroVideo = playHeroVideo;
});