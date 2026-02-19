// =======================
// 🌟 Variables globales
// =======================
let debounceTimer;
let _materialesEncontrados = [];
let _todosLosMateriales = null;
let _presupuestoIdActivo = null;
let cargandoItemExistente = false;

// =======================
// 🌟 Instancias de modales
// =======================
const modalItemsPresupuesto = new bootstrap.Modal(document.getElementById('modalItemsPresupuesto'), {
    backdrop: 'static',
    keyboard: false
});

const modalCrearItemPresupuesto = new bootstrap.Modal(document.getElementById('modalCrearItemPresupuesto'), {
    backdrop: 'static',
    keyboard: false
});

// =======================
// 🌟 Inputs y contenedores
// =======================
const inputMaterial = document.getElementById('materialInput');
const inputMaterialId = document.getElementById('materialIdSeleccionado');
const sugerenciasDiv = document.getElementById('sugerenciasMateriales');
const clearBtn = document.getElementById('clearMaterialInput');

// =======================
// 🔹 Modal principal: abrir items del presupuesto
// =======================
$(document).on('click', '.btn-items-presupuesto', async function () {
    const presupuestoId = $(this).data('id');
    $('#itemsPresupuestoId').val(presupuestoId);

    try {
        if (!_todosLosMateriales) await obtenerMateriales();

        const tabla = $('#tblList').DataTable();
        const fila = $(this).closest('tr');
        const datosFila = tabla.row(fila).data();

        const descripcion = datosFila?.nombre || '';
        const codigo = datosFila?.numeroOrden || '';
        const textoEncabezado = codigo ? `${descripcion} (Nº Orden ${codigo})` : descripcion;
        $('#nombreDinamicoItems').text(textoEncabezado);

        await cargarTablaItemsPresupuesto(presupuestoId);
        modalItemsPresupuesto.show();
    } catch (err) {
        console.error('Error al abrir modal items:', err);
        Swal.fire('Error', 'No se pudieron cargar los materiales.', 'error');
    }
});

// =======================
// 🔹 Autocompletado de materiales (con spinner de carga)
// =======================
async function obtenerMateriales() {
    // 🌀 Mostrar el spinner antes de empezar
    const spinner = document.getElementById("spinnerCarga");
    if (spinner) spinner.style.display = "block";

    // Si ya está cargado en caché, ocultar spinner y devolver directamente
    if (_todosLosMateriales) {
        if (spinner) spinner.style.display = "none";
        return _todosLosMateriales;
    }

    try {
        const res = await fetch("/Materiales/GetAll");
        const data = await res.json();

        // Guarda en memoria cache
        _todosLosMateriales = Array.isArray(data) ? data : data?.data || [];

        return _todosLosMateriales;
    } catch (err) {
        console.error("Error cargando materiales:", err);
        return [];
    } finally {
        // ✅ Ocultar spinner sí o sí al terminar (éxito o error)
        if (spinner) spinner.style.display = "none";
    }
}

function mostrarSugerencias() {
    sugerenciasDiv.innerHTML = '';

    if (!_materialesEncontrados.length) {
        sugerenciasDiv.style.display = 'none';
        return;
    }

    _materialesEncontrados.slice(0, 15).forEach(mat => {
        const codigo = mat.codigoBarra != null ? String(mat.codigoBarra) : '';
        const nombre = mat.nombre ?? '';
        const desc = mat.descripcion ?? '';

        const texto = [codigo, nombre, desc]
            .filter(x => typeof x === 'string' && x.trim() !== '')
            .join(' - ');

        const div = document.createElement('div');
        div.classList.add('list-group-item', 'list-group-item-action');
        div.textContent = texto;
        div.dataset.id = mat.id;
        sugerenciasDiv.appendChild(div);
    });

    sugerenciasDiv.style.display = 'block';
}

// 🔹 Input con debounce (búsqueda)
inputMaterial?.addEventListener('input', (e) => {
    clearTimeout(debounceTimer);

    const termino = e.target.value.trim().toLowerCase();

    // Botón ❌ estilo WhatsApp
    clearBtn.style.display = termino.length > 0 ? 'block' : 'none';

    debounceTimer = setTimeout(async () => {

        if (termino.length < 2) {
            sugerenciasDiv.style.display = 'none';
            return;
        }

        const materiales = await obtenerMateriales();

        _materialesEncontrados = materiales.filter(m => {

            // 🔹 Normalización segura
            const codigo = m.codigoBarra != null
                ? String(m.codigoBarra)
                : '';

            const nombre = m.nombre?.toLowerCase() ?? '';
            const descripcion = m.descripcion?.toLowerCase() ?? '';

            const texto = `${codigo} ${nombre} ${descripcion}`;

            // 🔍 Búsqueda múltiple con %
            if (termino.includes('%')) {
                const partes = termino
                    .split('%')
                    .map(p => p.trim())
                    .filter(Boolean);

                return partes.every(p => texto.includes(p));
            }

            // 🔍 Búsqueda normal
            return (
                codigo.startsWith(termino) ||
                codigo.includes(termino) ||
                nombre.startsWith(termino) ||
                nombre.includes(` ${termino}`) ||
                descripcion.startsWith(termino) ||
                descripcion.includes(` ${termino}`)
            );
        });

        // 🔃 Ordenamiento: nombre que empieza por término primero
        _materialesEncontrados.sort((a, b) => {
            const an = a.nombre?.toLowerCase() ?? '';
            const bn = b.nombre?.toLowerCase() ?? '';

            const aStarts = an.startsWith(termino);
            const bStarts = bn.startsWith(termino);

            if (aStarts !== bStarts) return aStarts ? -1 : 1;
            return an.localeCompare(bn);
        });

        mostrarSugerencias();
    }, 50);
});


// 🔹 Botón ❌ limpiar búsqueda (WhatsApp-style)
clearBtn?.addEventListener('click', () => {
    inputMaterial.value = '';
    inputMaterialId.value = '';
    sugerenciasDiv.style.display = 'none';
    clearBtn.style.display = 'none';
    inputMaterial.focus();
});

// 🔹 Selección desde lista
sugerenciasDiv?.addEventListener('mousedown', (e) => {
    if (!e.target.classList.contains('list-group-item')) return;
    inputMaterial.value = e.target.textContent;
    inputMaterialId.value = e.target.dataset.id;
    sugerenciasDiv.style.display = 'none';
});

// 🔹 Cerrar lista al hacer clic fuera
document.addEventListener('mousedown', (e) => {
    if (!sugerenciasDiv.contains(e.target) && !inputMaterial.contains(e.target)) {
        sugerenciasDiv.style.display = 'none';
    }
});

// 🔹 Validar cambio manual del input
inputMaterial?.addEventListener('change', () => {
    if (cargandoItemExistente) return;

    const input = inputMaterial.value.trim().toLowerCase();

    const encontrado = _materialesEncontrados.find(m => {
        const codigo = m.codigoBarra != null ? String(m.codigoBarra) : '';
        const nombre = m.nombre?.toLowerCase() ?? '';
        const descripcion = m.descripcion?.toLowerCase() ?? '';

        return (
            (codigo && input.includes(codigo)) ||
            (nombre && input.includes(nombre)) ||
            (descripcion && input.includes(descripcion))
        );
    });

    inputMaterialId.value = encontrado ? encontrado.id : '';
});


// =======================
// ➕ Crear nuevo item
// =======================
$('#AddItemPresupuesto').on('click', async () => {
    const presupuestoId = $('#itemsPresupuestoId').val();

    $('#formCrearItemPresupuesto')[0].reset();
    $('#nuevoItemPresupuestoPresupuestoId').val(presupuestoId);
    $('#nuevoItemPresupuestoId').val('');
    $('#materialInput').val('');
    $('#materialIdSeleccionado').val('');
    $('#modalCrearItemPresupuestoLabel').text('Nuevo Item');

    try {
        if (!_todosLosMateriales) await obtenerMateriales();
        modalItemsPresupuesto.hide();
        modalCrearItemPresupuesto.show();
    } catch (err) {
        console.error('Error cargando materiales:', err);
        Swal.fire('Error', 'No se pudieron cargar los materiales.', 'error');
    }
});

$('#tblItemsPresupuesto').on('click', '.img-material', function () {

    const tabla = $('#tblItemsPresupuesto').DataTable();
    const fila = tabla.row($(this).closest('tr')).data();

    // 🔢 Cantidad real
    const cantidad = Number(fila.cantidad) || 0;

    // 📦 Íconos = cantidad real (máx 5)
    const maxIcons = 5;
    const nivel = Math.min(maxIcons, cantidad);

    // 📦📦▫️▫️▫️
    const iconos = '📦'.repeat(nivel) + '▫️'.repeat(maxIcons - nivel);

    Swal.fire({
        width: 900,
        showConfirmButton: false,
        showCloseButton: true,
        allowOutsideClick: false,
        customClass: {
            popup: 'producto-modal'
        },
        html: `
            <div class="producto-wrapper">
                
                <div class="producto-imagen">
                    <img src="${this.src}" alt="${fila.materialNombre}">
                </div>

                <div class="producto-info">
                    <span class="producto-badge">Material</span>

                    <h2 class="producto-titulo">
                        ${fila.materialNombre}
                    </h2>

                    <div class="producto-rating">
                        <span class="producto-iconos">${iconos}</span>
                        <span class="producto-cantidad">
                            (${cantidad} unidades)
                        </span>
                    </div>

                    <div class="producto-precio">
                        ${Number(fila.precioUnitario).toLocaleString('es-AR', {
            style: 'currency',
            currency: 'ARS'
        })}
                    </div>

                    <div class="producto-detalle">
                        <p><strong>Unidad:</strong> ${fila.unidadMedidaNombre}</p>
                        <p><strong>Peso unitario:</strong> ${fila.pesoUnitario}</p>
                        <p><strong>Cantidad en presupuesto:</strong> ${cantidad}</p>
                    </div>
                </div>
            </div>
        `
    });
});

async function cargarTablaItemsPresupuesto(presupuestoId) {
    const tablaId = '#tblItemsPresupuesto';
    if ($.fn.DataTable.isDataTable(tablaId)) $(tablaId).DataTable().clear().destroy();

    try {
        const res = await fetch(`/ItemPresupuestos/GetByPropertyGuid?propertyName=PresupuestoId&guid=${presupuestoId}`);
        const items = await res.json();
        const lista = Array.isArray(items) ? items : items?.data || [];

        $(tablaId).DataTable({
            data: lista,
            destroy: true,
            responsive: true,
            autoWidth: false, // desactiva el cálculo automático de ancho
            columns: [
                { data: 'id', visible: false, width: '0px' },            
                { data: 'codigoBarra', title: 'Cod. Barra', width: '10%' },
                { data: 'materialNombre', title: 'Material', render: m => m ?? '', width: '5%' },
                { data: 'cantidad', title: 'Cantidad', className: 'text-end fuente-grande', render: c => !isNaN(Number(c)) ? Number(c).toFixed(2) : '0,00', width: '10%' },
                { data: 'unidadMedidaNombre', title: 'UM', render: u => u ?? '', width: '10%' },
                { data: 'pesoUnitario', title: 'Peso Unitario', className: 'text-end fuente-grande', render: p => !isNaN(Number(p)) ? Number(p).toFixed(2) : '0,00', width: '10%' },
                {
                    data: 'imagePath',
                    title: 'Imagen',
                    className: 'text-center',
                    width: '10%',
                    orderable: false,
                    render: img => {
                        if (!img) return '—';

                        const backendOrigin = window._backendOrigin || 'https://localhost:7268';
                        let url = img.startsWith('http')
                            ? img
                            : `${backendOrigin}${img.startsWith('/') ? '' : '/'}${img}`;

                        return `
                        <img src="${url}"
                             class="img-material"
                             alt="material"
                             title="Ver imagen"
                        >
                    `;
                    }
                },
                { data: 'precioUnitario', title: 'Precio Unitario', className: 'text-end fuente-grande', render: p => !isNaN(Number(p)) ? Number(p).toLocaleString('es-AR', { style: 'currency', currency: 'ARS' }) : '$0,00', width: '10%' },
                { data: 'total', title: 'Total', className: 'text-end fuente-grande', render: t => !isNaN(Number(t)) ? Number(t).toLocaleString('es-AR', { style: 'currency', currency: 'ARS' }) : '$0,00', width: '10%' },
                {
                    data: 'id', title: 'Acciones', className: 'text-center', orderable: false, width: '10%',
                    render: id =>
                        `<button class="btn btn-outline-edit btn-sm btn-editar-item" data-id="${id}"><i class="fas fa-edit"></i></button>
                 &nbsp;
                        <button class="btn btn-outline-delete btn-sm btn-eliminar-item" data-id="${id}"><i class="fas fa-trash-alt"></i></button>`
                }
            ],
            language: { url: '//cdn.datatables.net/plug-ins/1.10.21/i18n/Spanish.json' }
        });

        const totalGeneral = lista.reduce((sum, item) => sum + (Number(item.total) || 0), 0);
        $('#totalGeneralItems').text(totalGeneral.toLocaleString('es-AR', { style: 'currency', currency: 'ARS' }));

    } catch (err) {
        console.error(err);
        Swal.fire('Error', 'No se pudieron cargar los items del presupuesto.', 'error');
    }
}

// =======================
// ➕ Cargar Favoritos
// =======================
async function cargarTablaItemsFavoritos() {
    const tablaId = '#tblItemsFavoritos';

    if ($.fn.DataTable.isDataTable(tablaId)) {
        $(tablaId).DataTable().clear().destroy();
    }

    if (!_todosLosMateriales) {
        await obtenerMateriales();
    }

    try {

        // 🔥 FILTRAMOS DESDE MEMORIA
        _favoritosBase = _todosLosMateriales.filter(m => m.isFavorite);
        const lista = _favoritosBase;

        // ✅ GUARDAMOS LA INSTANCIA
        const tabla = $(tablaId).DataTable({
            data: lista,
            destroy: true,
            paging: false,
            searching: true,
            info: true,
            ordering: true,
            responsive: true,
            autoWidth: false,

            columns: [
                {
                    data: 'codigoBarra',
                    title: 'Cód. Barra',
                    className: 'text-center',
                    width: '8%' // Forzamos un ancho pequeño
                },
                { data: 'id', visible: false },
                {
                    data: 'nombre',
                    title: 'Material'
                },
                {
                    data: 'unidadNombre',
                    title: 'UM',
                    className: 'text-center',
                    width: '5%',
                    render: u => u ?? ''
                },
                {
                    data: 'pesoUnitario',
                    title: 'Peso Unit.',
                    className: 'text-end',
                    render: p => Number(p || 0).toFixed(2)
                },
                {
                    data: 'precioUnitario',
                    title: 'Precio Unit.',
                    className: 'text-end',
                    render: p =>
                        Number(p || 0).toLocaleString('es-AR', {
                            style: 'currency',
                            currency: 'ARS'
                        })
                },
                {
                    data: null,
                    title: 'Acciones',
                    className: 'text-center py-1', // Agregamos py-1 (padding vertical pequeño de Bootstrap)
                    orderable: false,
                    render: row => `
                    <button class="btn btn-outline-edit btn-xs btn-usar-favorito" 
                            style="padding: 0px 5px;" 
                            data-material-id="${row.id}" 
                            data-delta="1">
                        <i class="fas fa-plus fa-xs"></i>
                    </button>
                    <button class="btn btn-outline-delete btn-xs btn-usar-favorito" 
                            style="padding: 0px 5px;" 
                            data-material-id="${row.id}" 
                            data-delta="-1">
                        <i class="fas fa-minus fa-xs"></i>
                    </button>`
                }
            ],

            language: {
                emptyTable: 'No hay materiales favoritos'
            },

            // 🔥 SOLO encabezados centrados
            initComplete: function () {
                $(tablaId + ' thead th').addClass('text-center');
            }
        });

        // 🔍 FILTRO AVANZADO
        $('#filtroFavoritos')
            .off('input')
            .on('input', function () {

                const termino = this.value;
                const resultados = filtrarFavoritosAvanzado(termino);

                tabla.clear().rows.add(resultados).draw(false);
            });

    } catch (err) {
        console.error('Error cargando materiales favoritos:', err);
    }
}

// =======================
// ✏️ Editar item
// =======================
$('#tblItemsPresupuesto').on('click', '.btn-editar-item', function () {
    const tabla = $('#tblItemsPresupuesto').DataTable();
    const fila = $(this).closest('tr');
    const datosFila = tabla.row(fila).data();

    cargandoItemExistente = true;

    $('#editarItemId').val(datosFila.id);
    $('#nuevoItemPresupuestoPresupuestoId').val(datosFila.presupuestoId);
    $('#cantidadItem').val(datosFila.cantidad);
    $('#precioUnitarioItem').val(datosFila.precioUnitario);
    $('#pesoUnitarioItem').val(datosFila.pesoUnitario);

    const codigoBarra = datosFila.codigoBarra ?? '';
    const nombreMaterial = datosFila.materialNombre ?? '';
    $('#materialInput').val(codigoBarra ? `${codigoBarra} - ${nombreMaterial}` : nombreMaterial);

    $('#materialIdSeleccionado').val(datosFila.materialId);

    const total = (parseFloat(datosFila.cantidad) || 0) * (parseFloat(datosFila.precioUnitario) || 0);

    $('#totalItemLabel').text(
        total.toLocaleString('es-AR', { style: 'currency', currency: 'ARS' })
    );

    $('#modalCrearItemPresupuestoLabel').text('Editar Item');
    modalItemsPresupuesto.hide();
    modalCrearItemPresupuesto.show();

    cargandoItemExistente = false;
});

// =======================
// 🗑️ Eliminar item
// =======================
$(document).on('click', '.btn-eliminar-item', async function (e) {
    e.preventDefault(); e.stopPropagation();
    const id = $(this).data('id');
    const presupuestoId = $('#itemsPresupuestoId').val();

    const confirmacion = await Swal.fire({
        title: '¿Estás seguro?',
        text: "¡No podrás revertir esto!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
    });

    if (!confirmacion.isConfirmed) return;

    try {
        const res = await fetch(`/ItemPresupuestos/Delete/${id}`, { method: 'DELETE' });
        if (!res.ok) throw new Error('Error al eliminar el item.');

        Swal.fire({ icon: 'success', title: '¡Eliminado!', text: 'Item eliminado.', timer: 2000, showConfirmButton: false, toast: true, position: 'top-end', timerProgressBar: true });

        const tabla = $('#tblItemsPresupuesto').DataTable();
        const filaEliminar = $(`#tblItemsPresupuesto button[data-id="${id}"]`).closest('tr');
        tabla.row(filaEliminar).remove().draw(false);
        actualizarTotalItems();
        await recargarFilaPresupuesto(presupuestoId);

    } catch (err) {
        console.error(err);
        Swal.fire('Error', 'No se pudo eliminar el item.', 'error');
    }
});

// =======================
// 🧮 Calcular total automáticamente
// =======================
$('#cantidadItem, #precioUnitarioItem').on('input', () => {
    const matId = $('#materialIdSeleccionado').val();
    if (!matId) return;

    const total = (parseFloat($('#cantidadItem').val()) || 0) * (parseFloat($('#precioUnitarioItem').val()) || 0);
    $('#totalItemLabel').text(total.toLocaleString('es-AR', { style: 'currency', currency: 'ARS' }));
});

function actualizarTotalItems() {
    const tabla = $('#tblItemsPresupuesto').DataTable();
    const data = tabla.rows().data().toArray();
    const totalGeneral = data.reduce((sum, item) => sum + (item.total || 0), 0);
    $('#totalGeneralItems').text(totalGeneral.toLocaleString('es-AR', { style: 'currency', currency: 'ARS' }));
}

// =======================
// 🔁 Modal crear item -> al cerrar recargar tabla y total
// =======================
document.getElementById('modalCrearItemPresupuesto')?.addEventListener('hidden.bs.modal', async () => {
    const presupuestoId = $('#nuevoItemPresupuestoPresupuestoId').val()?.trim();
    if (!presupuestoId) return;

    await recargarFilaPresupuesto(presupuestoId);
    await cargarTablaItemsPresupuesto(presupuestoId);
    $('#totalItemLabel').text('$ 0,00');

    modalItemsPresupuesto.show();

    if (window._guardadoItemExitoso) {
        Swal.fire({ toast: true, position: 'top-end', icon: 'success', title: window._guardadoItemExitoso, showConfirmButton: false, timer: 1000, timerProgressBar: true });
        window._guardadoItemExitoso = false;
    }
});

// =======================
// ♻️ Recargar fila presupuesto
// =======================
async function recargarFilaPresupuesto(presupuestoId) {
    try {
        const tabla = $('#tblList').DataTable();
        const filas = tabla.rows().nodes().toArray();

        for (const fila of filas) {
            const btn = $(fila).find('.btn-items-presupuesto');
            if (btn.data('id') == presupuestoId) {
                const res = await fetch(`/Presupuestos/Edit/${presupuestoId}`);
                if (!res.ok) throw new Error('No se pudo recargar el presupuesto.');
                const presupuestoActualizado = await res.json();
                const filaActual = tabla.row(fila).data();
                filaActual.total = presupuestoActualizado.total;
                tabla.row(fila).data(filaActual).draw(false);
                break;
            }
        }
    } catch (err) {
        console.error('Error actualizando fila presupuesto:', err);
    }
}

// =======================
// 💾 Guardar item (crear o editar)
// =======================
$('#formCrearItemPresupuesto').on('submit', async function (e) {
    e.preventDefault();

    const itemId = $('#editarItemId').val();
    const presupuestoId = $('#nuevoItemPresupuestoPresupuestoId').val();
    const materialId = $('#materialIdSeleccionado').val();

    const cantidad = $('#cantidadItem').val().replace('.', ',');
    const precioUnitario = $('#precioUnitarioItem').val().replace('.', ',');
    const pesoUnitario = $('#pesoUnitarioItem').val().replace('.', ',');

    if (!materialId)
        return Swal.fire('Atención', 'Debe seleccionar un material.', 'warning');

    // 👉 obtener nombre del material desde memoria
    const materialObj = _todosLosMateriales?.find(m => m.id === materialId);
    const materialNombre = materialObj?.nombre ?? 'Material';

    const formData = new FormData();
    formData.append('PresupuestoId', presupuestoId);
    formData.append('MaterialId', materialId);
    formData.append('Cantidad', cantidad);
    formData.append('PrecioUnitario', precioUnitario);
    formData.append('PesoUnitario', pesoUnitario);
    formData.append('UnidadMedidaNombre', 'Sin Unidad');

    const url = itemId
        ? `/ItemPresupuestos/SaveChanges/${itemId}`
        : '/ItemPresupuestos/Create';

    try {
        const res = await fetch(url, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            body: formData
        });

        if (!res.ok) {
            const errorData = await res.json();
            throw new Error(errorData.message || 'Error al guardar item');
        }
        // 🔥🔥🔥 ACÁ SE DISPARA EL TOAST DE ÉXITO 🔥🔥🔥
        Swal.fire({
            toast: true,
            position: 'top-end',
            icon: 'success',
            title: itemId ? 'Item actualizado' : 'Item Agregado',
            html: `
        <div style="display:flex; align-items:center; gap:6px;">
            <div>
                <strong>${materialNombre}</strong><br>
                Cantidad: ${cantidad} | $${precioUnitario}
            </div>
            <span style="font-size:28px;">💾</span>
        </div>`,
            showConfirmButton: false,
            timer: 2500,
            timerProgressBar: true,

            // 👇 ACÁ va la magia UX
            didClose: async () => {

                await recargarFilaPresupuesto(presupuestoId);
                await cargarTablaItemsPresupuesto(presupuestoId);

                // 🔄 LIMPIAR FILTRO FAVORITOS
                $('#filtroFavoritos').val('');

                if ($.fn.DataTable.isDataTable('#tblItemsFavoritos')) {
                    const tablaFav = $('#tblItemsFavoritos').DataTable();
                    tablaFav.clear().rows.add(_favoritosBase).draw(false);
                }

                $('#formCrearItemPresupuesto')[0].reset();
                $('#editarItemId').val('');
            }
        });

    } catch (err) {
        console.error(err);
        Swal.fire('Error', err.message || 'No se pudo guardar el item.', 'error');
    }
});

// =======================
// 🔹 Variables globales
// =======================
let indexSeleccionado = -1;

// =======================
// 🔹 Autocompletado: selección con click
// =======================
sugerenciasDiv?.addEventListener('mousedown', (e) => {
    if (!e.target.classList.contains('list-group-item')) return;

    seleccionarOpcion(e.target);
    sugerenciasDiv.style.display = 'none';
});

// =======================
// 🔹 Navegación por teclado (selecciona SIEMPRE)
// =======================
inputMaterial?.addEventListener('keydown', (e) => {

    const opciones = sugerenciasDiv.querySelectorAll('.list-group-item');
    if (!opciones.length) return;

    switch (e.key) {

        case 'ArrowDown':
            e.preventDefault();
            indexSeleccionado = (indexSeleccionado + 1) % opciones.length;
            actualizarSeleccion(opciones);

            // ⭐ Seleccionar automáticamente el item resaltado
            seleccionarOpcion(opciones[indexSeleccionado]);
            break;

        case 'ArrowUp':
            e.preventDefault();
            indexSeleccionado = (indexSeleccionado - 1 + opciones.length) % opciones.length;
            actualizarSeleccion(opciones);

            // ⭐ Seleccionar automáticamente el item resaltado
            seleccionarOpcion(opciones[indexSeleccionado]);
            break;

        case 'Enter':
        case 'Tab':
        case 'Escape':
            e.preventDefault();

            if (indexSeleccionado < 0 || indexSeleccionado >= opciones.length) {
                indexSeleccionado = 0;
            }

            seleccionarOpcion(opciones[indexSeleccionado]);

            indexSeleccionado = -1;
            sugerenciasDiv.style.display = 'none';
            break;
    }
});

// =======================
// 🔹 Resaltar item activo
// =======================
function actualizarSeleccion(opciones) {
    opciones.forEach((op, i) => {
        op.classList.toggle('active', i === indexSeleccionado);
    });
}

// =======================
// 🔹 Seleccionar opción COMPLETA
// =======================
function seleccionarOpcion(opcion) {
    if (!opcion) return;

    const delta = opcion.dataset.delta !== undefined
        ? parseInt(opcion.dataset.delta, 10)
        : 0;

    // 👉 SOLO sumar/restar si hay delta explícito
    if (inputMaterialId.value === opcion.dataset.id) {

        let cantidad = parseFloat($('#cantidadItem').val());
        if (isNaN(cantidad)) cantidad = 0;

        cantidad += delta;
        if (cantidad < 1) cantidad = 1;

        $('#cantidadItem').val(cantidad);

    } else {

        $('#cantidadItem').val(1);
    }

    const cantidadFinal = parseFloat($('#cantidadItem').val()) || 1;
    const precioInput = parseFloat($('#precioUnitarioItem').val()) || 0;

    $('#totalItemLabel').text(
        (cantidadFinal * precioInput).toLocaleString('es-AR', {
            style: 'currency',
            currency: 'ARS'
        })
    );

    // 👉 MATERIAL DISTINTO: SOLO setear material, NO cantidad
    inputMaterial.value = opcion.textContent;
    inputMaterialId.value = opcion.dataset.id;

    const mat = _todosLosMateriales.find(m => m.id == opcion.dataset.id);

    const precioUnitario = Number(mat?.precioUnitario ?? 0);
    const pesoUnitario = Number(mat?.pesoUnitario ?? 0);

    if (pesoUnitario > 0) {
        $('#precioUnitarioItem').val(precioUnitario);
    }

    $('#pesoUnitarioItem').val(pesoUnitario);

    const cantidadActual = parseFloat($('#cantidadItem').val()) || 0;
    const total = cantidadActual * precioUnitario;

    $('#totalItemLabel').text(
        total.toLocaleString('es-AR', { style: 'currency', currency: 'ARS' })
    );
}

let favoritosCargados = false;
let _favoritosBase = [];

function filtrarFavoritosAvanzado(termino) {
    termino = termino.trim().toLowerCase();

    if (!termino) return _favoritosBase;

    return _favoritosBase.filter(m => {
        const texto = `
            ${m.codigoBarra ?? ''}
            ${m.nombre ?? ''}
            ${m.descripcion ?? ''}
        `.toLowerCase();

        // 🔍 Búsqueda múltiple con %
        if (termino.includes('%')) {
            const partes = termino.split('%').filter(Boolean);
            return partes.every(p => texto.includes(p));
        }

        return texto.includes(termino);
    });
}


document
    .getElementById('modalCrearItemPresupuesto')
    ?.addEventListener('shown.bs.modal', () => {

        $('#filtroFavoritos').val('');

        const tablaId = '#tblItemsFavoritos';

        if (!favoritosCargados) {
            cargarTablaItemsFavoritos();
            favoritosCargados = true;
        } else if ($.fn.DataTable.isDataTable(tablaId)) {
            const tabla = $(tablaId).DataTable();

            // 🔥 restaurar lista completa
            tabla.clear().rows.add(_favoritosBase).draw(false);
        }
    });


// =======================
// ⭐ Usar material favorito
// =======================
$('#tblItemsFavoritos').on('click', '.btn-usar-favorito', function (e) {
    e.preventDefault();
    e.stopPropagation();

    const materialId = $(this).data('material-id');
    const delta = parseInt($(this).data('delta'), 10);

    if (!materialId || !_todosLosMateriales) return;

    const material = _todosLosMateriales.find(m => m.id == materialId);
    if (!material) return;

    const opcionFake = {
        textContent: [
            material.codigoBarra,
            material.nombre,
            material.descripcion
        ].filter(Boolean).join(' - '),
        dataset: {
            id: material.id,
            delta: delta
        }
    };
    seleccionarOpcion(opcionFake);
});
