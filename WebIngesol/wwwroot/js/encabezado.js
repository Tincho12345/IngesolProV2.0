// encabezado.js
export function insertarEncabezadoYFiltros() {
    const container = document.querySelector('.container-fluid .row.mb-1');
    if (!container) return;

    const isEmpleado = window._userRole?.trim() === '&#x1F464; Empleados';
    const rutaActual = window._ruta?.trim() || "";

    const mostrarBotonCrear =
        !isEmpleado ||
        (
            isEmpleado &&
            ["RegistrosViaticos", "RegistrosMovilidad", "RegistrosAsistencias"].includes(rutaActual)
        );

    container.innerHTML = `
        <div class="w-100 mb-4 d-flex justify-content-center">
        <h2 class="text-primary mb-0 text-center">
            <i class="${window._viewIcon || 'fas fa-user-cog'}"></i>
            Lista de ${window._viewTitles || 'Entidades'}
        </h2>
        </div>

        <!-- BUSCADOR + FILTROS -->
        <div class="row mb-2">
            <!-- BUSCADOR -->
            <div class="col-12 col-md-6 mb-2 mb-md-0">
                <div class="d-flex justify-content-center">
                <form class="buscador-youtube d-flex w-100" style="max-width:600px" onsubmit="return false;">
                    <input
                        id="inputBuscador"
                        type="search"
                        class="form-control buscador-input"
                        placeholder="Buscar..."
                    />
                    <button
                        class="btn buscador-btn"
                        type="button"
                        onclick="buscarYT()">
                        <i class="fas fa-search"></i>
                    </button>
                </form>
                </div>
            </div>

            <!-- FILTROS -->
            <div id="divFiltros"
                 class="col-12 col-md-6 d-flex justify-content-center justify-content-md-end">
                <div class="w-100" style="max-width:600px;">
                    
                    <!-- fila filtros -->
                    <div class="d-flex gap-2 flex-wrap align-items-center">
                        <input
                            type="text"
                            id="inputFiltroAcumulado"
                            class="form-control"
                            placeholder="Criterio de Filtro..."
                            style="max-width:300px"
                        />

                        <button id="btnAgregarFiltro" class="btn btn-primary btn-sm" title="Agregar filtro">
                            ➕🔍
                        </button>

                        <button id="btnLimpiarFiltros" class="btn btn-primary btn-sm" title="Limpiar filtros">
                            ❌🔍
                        </button>

                        <button id="btnExportarExcel" class="btn btn-primary btn-sm" title="Exportar a Excel">
                            📊 📤
                        </button>
                    </div>
                    <!-- botón crear -->
                    ${mostrarBotonCrear ? `
                    <div class="mt-2">
                        <button
                            id="AddEntity"
                            class="btn btn-primary btn-sm text-white w-100 btn-crear-entity d-flex align-items-center justify-content-center"
                            title="Crear ${window._viewTitle || 'Orden'}">
                            <span class="icon-crear me-1">➕📄</span>
                            <span>Agregar ${window._viewTitle || 'Orden'}</span>
                        </button>
                    </div>
                    ` : ''}
                </div>
            </div>
        </div>

        <!-- FILTROS ACTIVOS -->
        <div class="w-100 my-1 d-flex justify-content-end">
            <div id="filtrosActivos"></div>
        </div>
    `;

    const input = document.getElementById('inputBuscador');
    if (input) {
        input.addEventListener('keydown', e => {
            if (e.key === 'Enter') {
                e.preventDefault();
                buscarYT();
            }
        });
    }
}

export function aplicarMascarasInputs() {
    document.querySelectorAll('[data-mask]').forEach(input => {
        input.addEventListener('input', () => {
            const mask = input.dataset.mask;
            const digits = input.value.replace(/\D/g, '');
            let result = '';
            let digitIndex = 0;

            for (let i = 0; i < mask.length; i++) {
                if (mask[i] === '9') {
                    if (digitIndex < digits.length) {
                        result += digits[digitIndex++];
                    } else {
                        break;
                    }
                } else {
                    result += mask[i];
                }
            }

            input.value = result;
        });
    });
}
