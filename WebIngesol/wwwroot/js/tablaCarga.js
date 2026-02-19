// =====================================================
// 🧱 ENCABEZADOS DE TABLA
// =====================================================
export function generarEncabezadosTabla(columnas, mostrarColumnasDefault) {
    const headerRow = $('#tableHeaders');
    headerRow.empty();

    columnas.forEach(col => {
        if (col.includeInTable === false) return;

        const widthStyle = col.width ? ` style="width:${col.width};"` : '';
        headerRow.append(`<th${widthStyle}>${col.label || col.data}</th>`);
    });

    if (mostrarColumnasDefault) {
        headerRow.append('<th style="width:10%;">Fecha Creado</th>');
        headerRow.append('<th style="width:10%;">Creado Por</th>');
        headerRow.append('<th style="width:10%;">Fecha Modif.</th>');
        headerRow.append('<th style="width:10%;">Modif Por</th>');
    }

    headerRow.append('<th style="width:70px; min-width:70px;">Estado</th>');
    headerRow.append('<th style="width:120px; min-width:120px; text-align:center;">Acciones</th>');
}

// =====================================================
// 📦 CARGA DATATABLE
// =====================================================
export async function cargarDataTable(
    ruta,
    columnas,
    mostrarColumnasDefault,
    mostrarBotonContactos,
    iconAux,
    mensaje,
    termino = ''
) {
    const tablaContenedor = document.querySelector('.table-responsive');

    try {
        if (tablaContenedor) tablaContenedor.style.display = 'none';

        generarEncabezadosTabla(columnas, mostrarColumnasDefault);

        const params = new URLSearchParams();
        if (termino) params.append('termino', termino);

        const response = await fetch(`/${ruta}/GetAll?${params.toString()}`);
        const data = await response.json();

        // 🔢 Próximo número de orden
        if (ruta === 'Ordenes') {
            const maxNumero = data.length
                ? Math.max(...data.map(x => x.numeroOrden || 0))
                : 0;
            window._proximoNumeroOrden = maxNumero + 1;
        } else {
            window._proximoNumeroOrden = null;
        }

        const backendOrigin = window._backendOrigin || 'https://localhost:7268';

        // =====================================================
        // 🧩 COLUMNAS
        // =====================================================
        const columnasFinales = columnas
            .filter(c => c.includeInTable !== false)
            .map(col => {
                const tipo = (col.type || '').toLowerCase();

                // 🔲 QR SOLO PARA Materiales / codigoBarra
                if (ruta === 'Materiales' && col.data === 'codigoBarra') {
                    return {
                        data: col.data,
                        orderable: false,
                        className: 'text-center',
                        render: (valor, type, row) => {
                            if (!valor) return '';
                            return `
                                <div class="qr-wrapper">
                                    <div id="qr-${row.id}" data-value="${valor}"></div>
                                    <small class="text-muted">${valor}</small>
                                </div>
                            `;
                        }
                    };
                }

                // 🖼️ Imagen / archivo
                if (tipo === 'image' || tipo === 'file') {
                    return {
                        data: col.data,
                        className: col.className || '',
                        render: valor => {
                            if (!valor) return 'Sin imagen';
                            let url = valor.trim();
                            if (!url.startsWith('http')) {
                                url = url.startsWith('/')
                                    ? `${backendOrigin}${url}`
                                    : `${backendOrigin}/${url}`;
                            }
                            return `<img src="${url}" style="max-height:50px;">`;
                        }
                    };
                }

                // ⭐ Favoritos
                if (tipo === 'boolean' && col.data === 'isFavorite') {
                    return {
                        data: col.data,
                        orderable: false,
                        className: 'text-center',
                        render: (valor, type, row) => {
                            const color = valor ? 'text-warning' : 'text-secondary';
                            const title = valor ? 'Quitar de favoritos' : 'Marcar como favorito';
                            return `
                                <i class="fas fa-star favorite-star ${color}"
                                   data-id="${row.id}"
                                   data-value="${valor}"
                                   title="${title}"
                                   style="cursor:pointer; font-size:1.3rem">
                                </i>`;
                        }
                    };
                }

                // ✅ Booleano normal
                if (tipo === 'boolean') {
                    return {
                        data: col.data,
                        className: 'text-center',
                        render: v =>
                            v
                                ? '<span class="badge bg-primary">Sí</span>'
                                : '<span class="badge bg-danger">No</span>'
                    };
                }

                // 📧 Email
                if (col.data === 'correoElectronico') {
                    return {
                        data: col.data,
                        render: v => v ? `<a href="mailto:${v}">${v}</a>` : ''
                    };
                }

                let clase = col.className || '';
                if (col.align) clase += ` text-${col.align}`;

                return {
                    data: col.data,
                    render: col.render,
                    className: clase.trim(),
                    width: col.width || null
                };
            });

        // 📅 Columnas default
        if (mostrarColumnasDefault) {
            columnasFinales.push(
                { data: 'createdDate', render: d => d ? new Date(d).toLocaleString() : '' },
                { data: 'createdBy' },
                { data: 'modifiedDate', render: d => d ? new Date(d).toLocaleString() : '' },
                { data: 'modifiedBy' }
            );
        }

        // 🟢 Estado
        columnasFinales.push({
            data: 'isActive',
            className: 'text-center',
            render: v =>
                v
                    ? '<span class="badge bg-primary">Activo</span>'
                    : '<span class="badge bg-danger">Inactivo</span>'
        });

        // 🎯 ACCIONES (USANDO LAS VARIABLES)
        columnasFinales.push({
            data: 'id',
            orderable: false,
            className: 'text-center',
            render: id => {
                const botones = [];

                if (mostrarBotonContactos) {
                    const claseBoton =
                        mensaje === 'Ver Items del Presupuesto'
                            ? 'btn-items-presupuesto'
                            : 'btn-contactos';

                    botones.push(`
                        <button class="btn btn-outline-contact ${claseBoton}"
                                data-id="${id}"
                                title="${mensaje}">
                            <i class="${iconAux || 'fas fa-address-book'}"></i>
                        </button>
                    `);
                }

                botones.push(`
                    <button class="btn btn-outline-edit btn-editar" data-id="${id}">
                        <i class="fas fa-edit"></i>
                    </button>
                `);

                botones.push(`
                    <button class="btn btn-outline-delete btn-eliminar" data-id="${id}">
                        <i class="fas fa-trash"></i>
                    </button>
                `);

                return botones.join(' ');
            }
        });

        // =====================================================
        // 🚀 DATATABLE INIT
        // =====================================================
        if ($.fn.DataTable.isDataTable('#tblList')) {
            $('#tblList').DataTable().destroy();
        }

        // ===============================
        // 📄 PAGINACIÓN DATATABLE (CON "TODOS")
        // ===============================
        const tabla = $('#tblList').DataTable({
            responsive: true,
            autoWidth: false,
            data,
            columns: columnasFinales,
            order: [],
            pageLength: 10,
            lengthMenu: [[5, 10, 25, 50, 100, -1], [5, 10, 25, 50, 100, 'Todos']],
            dom: 'lfrtip',
            language: {
                url: '//cdn.datatables.net/plug-ins/1.10.21/i18n/Spanish.json'
            }
        });


        // =====================================================
        // 🔁 DRAW
        // =====================================================
        $('#tblList').on('draw.dt', function () {

            $('[data-bs-toggle="tooltip"]').tooltip();

            // 🔲 QR SOLO Materiales
            if (ruta === 'Materiales' && typeof QRCode !== 'undefined') {
                document.querySelectorAll('[id^="qr-"]').forEach(div => {
                    if (div.dataset.qr === 'true') return;

                    new QRCode(div, {
                        text: div.dataset.value,
                        width: 60,
                        height: 60
                    });

                    div.dataset.qr = 'true';
                });
            }
        });

        // =====================================================
        // ⭐ FAVORITOS
        // =====================================================
        $('#tblList')
            .off('click', '.favorite-star')
            .on('click', '.favorite-star', async function () {

                const star = this;
                bloquearTabla();

                if (star.dataset.loading === 'true') return;
                star.dataset.loading = 'true';
                star.style.pointerEvents = 'none';

                const id = star.dataset.id;
                const valorActual = star.dataset.value === 'true';
                const nuevoValor = !valorActual;

                star.classList.toggle('text-warning', nuevoValor);
                star.classList.toggle('text-secondary', !nuevoValor);
                star.dataset.value = nuevoValor;

                try {
                    const response = await fetch(`/Materiales/ToggleFavorite?id=${id}`, {
                        method: 'PATCH',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(nuevoValor)
                    });

                    if (!response.ok) throw new Error();
                    Swal.fire({
                        toast: true,
                        position: 'top-end',
                        icon: 'success',
                        title: nuevoValor ? 'Marcado como favorito ⭐' : 'Quitado de favoritos',
                        showConfirmButton: false,
                        timer: 1500,
                        timerProgressBar: true,
                        didClose: () => {
                            desbloquearTabla();
                        }
                    });

                } catch {
                    star.classList.toggle('text-warning', valorActual);
                    star.classList.toggle('text-secondary', !valorActual);
                    star.dataset.value = valorActual;

                    toastr.error(
                        'Error al actualizar favorito',
                        null,
                        { timeOut: 1500, onHidden: desbloquearTabla }
                    );
                } finally {
                    star.dataset.loading = 'false';
                    star.style.pointerEvents = 'auto';
                }
            });

    } catch (err) {
        console.error('Error DataTable:', err);
    } finally {
        if (tablaContenedor) tablaContenedor.style.display = 'block';
    }
}
