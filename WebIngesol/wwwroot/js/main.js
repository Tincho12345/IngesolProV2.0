import { generarCamposFormulario } from './formulario.js';
import { inicializarBuscadoresYoutubeDinamicos, generarBuscadoresDinamicos } from './buscadorYoutubeDinamico.js';
import { generarEncabezadosTabla, cargarDataTable } from './tablaCarga.js';
import { eliminarRecursoWrapper } from './functions.js';
import { insertarEncabezadoYFiltros, aplicarMascarasInputs } from './encabezado.js';

const ruta = window._ruta || 'AreasTecnicas';
const columnas = window._columnasPersonalizadas || [];
const tituloEntidad = window._ruta || 'Entidad';

// ✅ URL del backend según entorno
const backendOrigin = window._backendOrigin || 'https://localhost:7268';

// 👉 Agregar render a columnas de tipo 'date'
columnas.forEach(col => {
    if (col.type === 'date' && !col.render) {
        col.render = function (data) {
            if (!data) return '';
            const dateObj = new Date(data);
            if (isNaN(dateObj)) return data;
            const day = String(dateObj.getDate()).padStart(2, '0');
            const month = String(dateObj.getMonth() + 1).padStart(2, '0');
            const year = dateObj.getFullYear();
            return `${day}/${month}/${year}`;
        };
    }
});

async function inicializarVista() {
    await generarBuscadoresDinamicos(columnas, ruta);
    generarCamposFormulario(columnas);
    inicializarBuscadoresYoutubeDinamicos(columnas, ruta);
    generarEncabezadosTabla(columnas, window._mostrarColumnasDefault);
    insertarEncabezadoYFiltros();

    // ======================================================
    // 🔍 FILTROS ACUMULADOS - LOCAL, SIN LLAMAR BACKEND
    // ======================================================
    window.filtrosAcumulados = []; // Global

    // ➕ Agregar filtro
    $(document).off('click', '#btnAgregarFiltro').on('click', '#btnAgregarFiltro', function () {
        const valor = $('#inputFiltroAcumulado').val().trim();
        if (valor === '') return;

        window.filtrosAcumulados.push(valor.toLowerCase());
        $('#inputFiltroAcumulado').val('');
        mostrarFiltrosActivos();
        aplicarFiltrosAcumulados();
    });

    // 🧹 Limpiar todos los filtros
    $(document).off('click', '#btnLimpiarFiltros').on('click', '#btnLimpiarFiltros', function () {
        window.filtrosAcumulados = [];
        mostrarFiltrosActivos();
        aplicarFiltrosAcumulados();
    });

    // 🧩 Mostrar los filtros activos visualmente
    function mostrarFiltrosActivos() {
        const cont = $('#filtrosActivos');
        cont.empty();

        window.filtrosAcumulados.forEach((filtro, i) => {
            cont.append(`
        <span class="badge bg-info text-dark me-1" style="font-size: 0.9em;">
            ${filtro}
            <button type="button" class="btn-close btn-close-white btn-sm ms-1" aria-label="Cerrar" data-index="${i}"></button>
        </span>
    `);
        });
    }

    // 🗑️ Quitar filtro individual
    $(document).off('click', '#filtrosActivos .btn-close').on('click', '#filtrosActivos .btn-close', function () {
        const index = $(this).data('index');
        window.filtrosAcumulados.splice(index, 1);
        mostrarFiltrosActivos();
        aplicarFiltrosAcumulados();
    });

    // ⚙️ Registrar filtro en DataTables (una sola vez global)
    if (!$.fn.dataTable.ext.search.some(f => f._name === 'filtroAcumulado')) {
        const filtroAcumulado = function (settings, data) {
            if (settings.nTable.id !== 'tblList') return true;
            if (window.filtrosAcumulados.length === 0) return true;

            const textoFila = data.join(' ').toLowerCase();
            return window.filtrosAcumulados.every(f => textoFila.includes(f));
        };
        filtroAcumulado._name = 'filtroAcumulado';
        $.fn.dataTable.ext.search.push(filtroAcumulado);
    }

    // ⚡ Redibujar tabla localmente sin backend
    window.aplicarFiltrosAcumulados = function () {
        const tabla = $('#tblList').DataTable();
        if (tabla) tabla.draw(false);
    };

    // ⌨️ Enter para agregar filtro
    document.addEventListener('keypress', function (e) {
        if (e.target && e.target.id === 'inputFiltroAcumulado' && e.key === 'Enter') {
            e.preventDefault();
            document.getElementById('btnAgregarFiltro')?.click();
        }
    });

    vincularEventos();
    vincularPreviewImagenes();
}

document.addEventListener('DOMContentLoaded', function () {
    document.addEventListener('keydown', function (e) {
        if (e.altKey && !e.ctrlKey && !e.metaKey) {
            if (document.activeElement.tagName === 'INPUT' || document.activeElement.tagName === 'TEXTAREA') {
                e.preventDefault();
            }
        }
    });
});

window.buscarYT = () => window.cargarDataTableWrapper(); // Para que funcione desde el HTML dinámico
document.addEventListener('click', function (event) {

    // Exportar tabla principal
    if (event.target.id === 'btnExportarExcel') {
        exportarTablaAExcel('tblList');
    }

    // Exportar tabla del modal
    if (event.target.id === 'btnExportarItems') {
        exportarTablaAExcel('tblItemsPresupuesto');
    }
});

function exportarTablaAExcel(idTabla) {
    const tabla = document.getElementById(idTabla);
    if (!tabla) return;

    const data = [];
    const headers = [];

    // Columnas que deben ir como número
    const columnasNumericas = ["Cantidad", "Peso Unitario", "Precio Unitario", "Total"];

    // Obtener encabezados y excluir "Acciones"
    tabla.querySelectorAll('thead th').forEach((th, index) => {
        const text = th.textContent.trim();
        if (text.toLowerCase() !== 'acciones') {
            headers.push({ text, index });
        }
    });

    // Encabezados
    data.push(headers.map(h => h.text));

    // Filas
    tabla.querySelectorAll('tbody tr').forEach(tr => {
        const row = [];

        headers.forEach(h => {
            const cell = tr.children[h.index];
            let value = cell ? cell.textContent.trim() : "";

            if (columnasNumericas.includes(h.text)) {
                if (typeof value === "string") {

                    // 1) Si tiene coma como decimal → convertirla a punto
                    if (value.includes(",")) {
                        // eliminar puntos de miles
                        value = value.replace(/\./g, "");
                        // convertir coma en punto decimal
                        value = value.replace(",", ".");
                    }

                    // 3) Quitar símbolo $
                    value = value.replace(/\$/g, "").trim();
                }

                const num = parseFloat(value);
                value = isNaN(num) ? "" : num;
            }

            row.push(value);
        });

        data.push(row);
    });

    const ws = XLSX.utils.aoa_to_sheet(data);

    // 🔥 Aplicar formato numérico a esas columnas
    columnasNumericas.forEach(colName => {
        const colIndex = headers.findIndex(h => h.text === colName);
        if (colIndex === -1) return;

        for (let i = 1; i < data.length; i++) {
            const cellAddress = XLSX.utils.encode_cell({ r: i, c: colIndex });
            const cell = ws[cellAddress];
            if (cell && typeof cell.v === "number") {
                cell.t = "n";
                cell.z = "0.00"; // formato estándar con 2 decimales
            }
        }
    });

    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Datos');
    XLSX.writeFile(wb, 'datos.xlsx');
}


window.addEventListener('load', () => {
    inicializarVista();

    // 🔹 Al abrir cualquier modal, limpiar los buscadores dentro de ese modal
    document.addEventListener('show.bs.modal', (event) => {
        const modal = event.target;
        modal.querySelectorAll('.dropdown-search-input').forEach(input => {
            input.value = ''; // Limpia el valor
        });
    });

    // 🔹 Al cerrar cualquier modal, limpiar buscadores y cerrar dropdowns
    document.addEventListener('hidden.bs.modal', (event) => {
        const modal = event.target;
        modal.querySelectorAll('.dropdown-search-input').forEach(input => {
            input.value = ''; // 🧹 Limpia el buscador

            const dropdown = input.nextElementSibling;
            if (dropdown && dropdown.tagName === 'SELECT') {
                dropdown.size = 1; // 🔒 Cierra el dropdown
            }
        });

        // 🔹 Limpieza adicional del backdrop y del body (por seguridad)
        if ($('.modal.show').length === 0) {
            $('.modal-backdrop').remove();
            $('body').removeClass('modal-open');
            $('body').css('padding-right', '');
        }
    });
});

aplicarMascarasInputs();
function vincularEventos() {
    $('#AddEntity').on('click', abrirModalCrear);
    $('#formCrearEditar').on('submit', enviarFormulario);
    $('#tblList').on('click', '.btn-editar', cargarParaEditar);
    $('#tblList').on('click', '.btn-eliminar', confirmarEliminacion);
}

function abrirModalCrear() {
    $('#formCrearEditar')[0].reset();
    $('#createEditEntityId').val('');

    // 🟢 CREACIÓN → mensaje informativo en código de barra
    const $codBarra = $('#edit_codigoBarra');
    const $hidden = $('input[name="codigoBarra"]');

    $codBarra
        .text('Se generará automáticamente al guardar')
        .removeClass()
        .addClass('text-muted fst-italic');

    $hidden.val(''); // no enviar nada al backend

    // 👇 mostrar el campo SOLO como informativo
    $codBarra.closest('.mb-3, .col-md-12').show();

    $('#createEditModalLabel').text('Crear ' + tituloEntidad);
    $('#isActiveContainer').hide();

        // ============================================================
    // 🔥 Asignar número de orden automáticamente si es Ordenes
    // ============================================================
    if (window._ruta === 'Ordenes' && window._proximoNumeroOrden) {
        const inputNumero = document.querySelector('#edit_numeroOrden');
        if (inputNumero) {
            inputNumero.value = window._proximoNumeroOrden;
        }
    }

    // 🔹 Limpiar los inputs tipo search-youtube al crear nuevo
    columnas.forEach(col => {
        if ((col.type || '').toLowerCase() === 'search-youtube') {
            const baseId = `edit_${col.data}`;
            const inputVisible = document.getElementById(`${baseId}_input`);
            const inputHidden = document.getElementById(baseId);
            if (inputVisible) inputVisible.value = '';
            if (inputHidden) inputHidden.value = '';
        }
    });

    const modal = new bootstrap.Modal(document.getElementById('createEditModal'), {
        backdrop: 'static', // ❌ Evita que se cierre al hacer clic fuera
        keyboard: false     // ❌ Evita que se cierre con ESC
    });
    modal.show();

    setTimeout(() => {
        const input = $(`#edit_${columnas[0].data}`).get(0);
        if (input) input.focus();
    }, 150);
}
async function enviarFormulario(e) {
    e.preventDefault();

    const $btn = $('#btnGuardarEntidad');
    const $form = $('#formCrearEditar')[0];

    if (!$form.checkValidity()) {
        $form.reportValidity();
        return;
    }

    $btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span> Guardando...');

    const id = $('#createEditEntityId').val();
    const url = id ? `/${ruta}/SaveChanges/${id}` : `/${ruta}/Create`;

    const formData = new FormData($form);

    // Manejar booleanos
    columnas.forEach(col => {
        if ((col.type || '').toLowerCase() === 'boolean') {
            formData.set(col.data, $(`#edit_${col.data}`).is(':checked'));
        }
    });

    formData.set('isActive', $('#editIsActive').is(':checked'));

    // 🔹 Normalizar números y decimales (solo para number y decimal)
    // 🔹 Normalizar decimales para el backend con coma, no con punto
    columnas.forEach(col => {
        const tipo = (col.type || '').toLowerCase();
        if (tipo === 'decimal' || tipo === 'number') {
            const valor = $(`#edit_${col.data}`).val();
            if (valor !== null && valor !== undefined && valor !== '') {
                formData.set(col.data, valor.replace('.', ',')); // <- usar coma
            }
        }
    });

    // Id por defecto si es nuevo
    if (!id || id.trim() === '') {
        formData.set('id', '00000000-0000-0000-0000-000000000000');
    }

    // 🔹 Convertir FormData a objeto para inspeccionar antes del envío
    const formDataObj = {};
    for (let [key, value] of formData.entries()) {
        formDataObj[key] = value;
    }
    console.log("📝 Datos a enviar:", formDataObj);

    try {
        const response = await fetch(url, {
            method: 'POST',
            body: formData
        });

        if (!response.ok) {
            let errorText = 'Error al guardar';
            try {
                const errorData = await response.json();
                errorText = errorData.message || errorText;
            } catch { }
            throw new Error(errorText);
        }

        bootstrap.Modal.getInstance(document.getElementById('createEditModal')).hide();
        await cargarDataTableWrapper();

        Swal.fire({
            toast: true,
            position: 'top-end',
            icon: 'success',
            title: id ? `${tituloEntidad} actualizada con éxito.` : `${tituloEntidad} creada con éxito.`,
            showConfirmButton: false,
            timer: 2000,
            timerProgressBar: true
        });
    } catch (error) {
        Swal.fire('Error', error.message || 'Error al guardar.', 'error');
    } finally {
        $btn.prop('disabled', false).html('Guardar');
    }
}

function fixDateTime(value) {
    if (!value) return "";

    const addHours = (date, h) => {
        const d = new Date(date.getTime());
        d.setHours(d.getHours() + h);
        return d;
    };

    // Caso: "23/11/2025 11:54:10,818"
    if (value.includes("/")) {
        let [fecha, hora] = value.split(" ");
        let [dd, mm, yyyy] = fecha.split("/");
        hora = hora.split(",")[0]; // sin milisegundos

        const [HH, MM, SS] = hora.split(":");
        const fechaJs = new Date(`${yyyy}-${mm}-${dd}T${HH}:${MM}:${SS}`);

        if (isNaN(fechaJs)) return "";

        // ⬅️ Restar 3 horas (Argentina)
        const ajustada = addHours(fechaJs, -3);

        return ajustada.toISOString().slice(0, 16);
    }

    // Caso ISO
    const d = new Date(value);
    if (isNaN(d)) return "";

    // ⬅️ Restar 3 horas (Argentina)
    const ajustada = addHours(d, -3);

    return ajustada.toISOString().slice(0, 16);
}

function cargarParaEditar() {
    const id = $(this).data('id');

    $.get(`/${ruta}/Edit/${id}`, async function (data) {
        $('#createEditEntityId').val(data.id);

        for (const col of columnas) {
            const tipo = (col.type || '').toLowerCase();
            const selector = `#edit_${col.data}`;

            // ==========================
            // 🧠 Campo tipo search-youtube
            // ==========================
            if (tipo === 'search-youtube') {
                const baseId = `edit_${col.data}`;
                const inputVisible = document.getElementById(`${baseId}_input`);
                const inputHidden = document.getElementById(baseId);

                if (inputVisible && inputHidden) {
                    inputHidden.value = data[col.data] || '';

                    const base = col.data.replace(/Id$/, '');
                    const endpoint = `/${ruta}/Obtener${base.charAt(0).toUpperCase() + base.slice(1)}`;

                    // Esperar si el cache aún no se cargó
                    const cacheLista = window?.cacheGlobal?.[endpoint];
                    let lista = [];

                    if (cacheLista && cacheLista.length) {
                        lista = cacheLista;
                    } else {
                        try {
                            const res = await fetch(endpoint);
                            const result = await res.json();
                            lista = Array.isArray(result) ? result : result?.data || [];
                            window.cacheGlobal = window.cacheGlobal || {};
                            window.cacheGlobal[endpoint] = lista;
                        } catch {
                            lista = [];
                        }
                    }

                    const item = lista.find(x => x.id === data[col.data]);
                    inputVisible.value = item ? (item.nombre ?? item.descripcion ?? '') : '';
                }
                continue; // ⬅ evita procesar el campo de nuevo abajo
            }

            // ==========================
            // ✅ Resto de tipos normales
            // ==========================
            if (tipo === 'boolean') {
                $(selector).prop('checked', data[col.data]);

                // 🔥 Actualizar badge Favorito / booleano
                const field = selector.replace('#', '');
                const badge = document.getElementById(`${field}_badge`);

                if (badge) {
                    if (data[col.data]) {
                        badge.textContent = "Sí";
                        badge.className = "badge bg-primary";
                    } else {
                        badge.textContent = "No";
                        badge.className = "badge bg-danger";
                    }
                }
            } else if (tipo === 'file') {
                const previewImg = $(`${selector}_preview`);
                const valor = data.imagePath ? `${backendOrigin}${data.imagePath}` : null;
                valor ? previewImg.attr('src', valor).show() : previewImg.hide();
            } else if (tipo === 'date') {
                $(selector).val(formatearFechaParaInput(data[col.data]));
            } else if (tipo === 'datetime' || tipo === 'datetime-local') {
                $(selector).val(fixDateTime(data[col.data]));
            } else {
                $(selector).val(data[col.data]);
            }
        }

        // 🔥 ACÁ VA
        if (data.codigoBarra) {
            $('#edit_codigoBarra').text(data.codigoBarra);
            $('input[name="codigoBarra"]').val(data.codigoBarra);
        }

        // 👇 MOSTRAR SOLO EN EDICIÓN
        $('#edit_codigoBarra').closest('.mb-3, .col-md-12').show();


        // ==========================
        // 🧩 Configurar modal
        // ==========================
        $('#editIsActive').prop('checked', data.isActive);
        $('#isActiveLabel')
            .text(data.isActive ? 'Activo' : 'Inactivo')
            .toggleClass('text-danger', !data.isActive);

        $('#createEditModalLabel').text('Editar ' + tituloEntidad);
        $('#isActiveContainer').show();

        const modal = new bootstrap.Modal(document.getElementById('createEditModal'), {
            backdrop: 'static',
            keyboard: false
        });
        modal.show();

        setTimeout(() => {
            const input = $(`#edit_${columnas[0].data}`).get(0);
            if (input) input.focus();
        }, 300);
    }).fail(() => {
        Swal.fire('Error', 'No se pudieron cargar los datos', 'error');
    });
}

function formatearFechaParaInput(dateString) {
    if (!dateString) return '';
    const d = new Date(dateString);
    if (isNaN(d)) return '';
    const mes = (d.getMonth() + 1).toString().padStart(2, '0');
    const dia = d.getDate().toString().padStart(2, '0');
    return `${d.getFullYear()}-${mes}-${dia}`;
}

function vincularPreviewImagenes() {
    columnas.forEach(col => {
        if ((col.type || '').toLowerCase() === 'file') {
            const inputId = `#edit_${col.data}`;
            const previewId = `#edit_${col.data}_preview`;

            $(document).off('change', inputId).on('change', inputId, function () {
                const file = this.files[0];
                if (file) {
                    const reader = new FileReader();
                    reader.onload = e => {
                        $(previewId).attr('src', e.target.result).show();
                    };
                    reader.readAsDataURL(file);
                } else {
                    $(previewId).hide();
                }
            });
        }
    });
}

// ✅ Hacer que la función esté disponible globalmente
window.cargarDataTableWrapper = async function () {
    const input = document.getElementById('inputBuscador');
    const valorBuscado = input ? input.value.trim() : '';

    if (window._ruta === 'Materiales' && valorBuscado === '') {
        toastr.warning('La búsqueda requiere un valor. Por favor, ingréselo.');
        return;
    }

    await cargarDataTable(
        window._ruta,
        window._columnasPersonalizadas,
        window._mostrarColumnasDefault,
        window._mostrarBotonContactos,
        window._viewIconAux,
        window._mensaje,
        valorBuscado
    );
};

// Confirmar eliminación con SweetAlert
function confirmarEliminacion() {
    const id = $(this).data('id');

    Swal.fire({
        title: '¿Estás seguro?',
        text: "¡No podrás revertir esto!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
    }).then(result => {
        if (result.isConfirmed) {
            eliminarRecursoWrapper(id); // ⚡ Llamar a la función del backend
        }
    });
}