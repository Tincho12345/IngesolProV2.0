
// 📌 Evento para abrir el modal de contactos desde la tabla principal
$(document).on('click', '.btn-contactos', async function () {
    const clienteId = $(this).data('id');
    $('#contactosClienteId').val(clienteId);

    const tabla = $('#tblList').DataTable();
    const fila = $(this).closest('tr');
    const datosFila = tabla.row(fila).data();

    const nombre = datosFila?.nombre || '';
    const nombreFantasia = datosFila?.nombreFantasia || '';
    const textoEncabezado = nombreFantasia ? `${nombreFantasia} (${nombre})` : nombre;

    $('#nombreDinamico').text(textoEncabezado);

    await cargarTablaContactos(clienteId);

    const modal = new bootstrap.Modal(document.getElementById('modalContactos'));
    modal.show();
});

// 🗂️ Cargar tabla de contactos de cliente
async function cargarTablaContactos(clienteId) {
    const tablaId = '#tblContactosCliente';

    if ($.fn.DataTable.isDataTable(tablaId)) {
        $(tablaId).DataTable().clear().destroy();
    }

    try {
        const response = await fetch(`/Contactos/GetByPropertyGuid?propertyName=ClienteId&guid=${clienteId}`);
        const contactos = await response.json();
        const lista = Array.isArray(contactos) ? contactos : contactos?.data || [];

        $(tablaId).DataTable({
            destroy: true,
            responsive: true,
            data: lista,
            columns: [
                { data: 'id', visible: false },
                { data: 'nombre', title: 'Nombre Contacto' },
                { data: 'numeroTelefono', title: 'Teléfono' },
                {
                    data: 'id',
                    title: 'Acciones',
                    className: 'text-center',
                    orderable: false,
                    render: id => `
                        <button class="btn btn-outline-edit btn-sm btn-editar-contacto" data-id="${id}">
                            <i class="fas fa-edit"></i>
                        </button>
                        &nbsp;
                        <button class="btn btn-outline-delete btn-sm btn-eliminar-contacto" data-id="${id}">
                            <i class="fas fa-trash-alt"></i>
                        </button>`
                }
            ],
            language: {
                url: '//cdn.datatables.net/plug-ins/1.10.21/i18n/Spanish.json'
            }
        });

    } catch (error) {
        console.error('Error al cargar contactos:', error);
        Swal.fire('Error', 'No se pudieron cargar los contactos.', 'error');
    }
}

// ➕ Crear nuevo contacto
$('#AddContacto').on('click', function () {
    const clienteId = $('#contactosClienteId').val();
    $('#nuevoContactoClienteId').val(clienteId);
    $('#formCrearContacto')[0].reset();
    $('#editarContactoId').val('');
    $('#modalCrearContactoLabel').text('Nuevo Contacto');

    const modalContactos = bootstrap.Modal.getInstance(document.getElementById('modalContactos'));
    if (modalContactos) modalContactos.hide();

    setTimeout(() => {
        const modalCrear = new bootstrap.Modal(document.getElementById('modalCrearContacto'));
        modalCrear.show();
    }, 300);
});

// 💾 Guardar contacto (crear o editar)
$('#formCrearContacto').on('submit', async function (e) {
    e.preventDefault();

    const contactoId = $('#editarContactoId').val();
    const clienteId = $('#nuevoContactoClienteId').val();
    const nombre = $('#nombreContacto').val().trim();
    const numeroTelefono = $('#telefonoContacto').val().trim();

    if (!nombre) {
        Swal.fire('Atención', 'El nombre del contacto es obligatorio.', 'warning');
        return;
    }

    try {
        const formData = new FormData();
        formData.append('ClienteId', clienteId);
        formData.append('Nombre', nombre);
        formData.append('NumeroTelefono', numeroTelefono);

        const url = contactoId
            ? `/Contactos/SaveChanges/${contactoId}`
            : '/Contactos/Create';

        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            body: formData
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Error al guardar contacto');
        }

        window._guardadoContactoExitoso = contactoId
            ? 'Contacto actualizado correctamente'
            : 'Contacto creado correctamente';

        const modalCrear = bootstrap.Modal.getInstance(document.getElementById('modalCrearContacto'));
        if (modalCrear) modalCrear.hide();

        await cargarTablaContactos(clienteId);

        $('#formCrearContacto')[0].reset();
        $('#editarContactoId').val('');

    } catch (error) {
        console.error(error);
        Swal.fire('Error', error.message || 'No se pudo guardar el contacto.', 'error');
    }
});

// ✏️ Editar contacto
$('#tblContactosCliente').on('click', '.btn-editar-contacto', async function () {
    const contactoId = $(this).data('id');
    $('#editarContactoId').val(contactoId);
    if (!contactoId) return;

    try {
        window._guardadoContactoExitoso = false;

        const res = await fetch(`/Contactos/Edit/${contactoId}`);
        if (!res.ok) throw new Error('No se pudo obtener el contacto');
        const contacto = await res.json();

        const modalContactos = bootstrap.Modal.getInstance(document.getElementById('modalContactos'));
        if (modalContactos) modalContactos.hide();

        $('#nuevoContactoClienteId').val(contacto.clienteId || '');
        $('#nombreContacto').val(contacto.nombre || '');
        $('#telefonoContacto').val(contacto.numeroTelefono || '');
        $('#modalCrearContactoLabel').text('Editar Contacto');

        const modalCrear = new bootstrap.Modal(document.getElementById('modalCrearContacto'));
        modalCrear.show();

    } catch (error) {
        console.error(error);
        Swal.fire('Error', 'No se pudo cargar el contacto para editar.', 'error');
    }
});

// 🗑️ Eliminar contacto
$(document).on('click', '.btn-eliminar-contacto', async function (event) {
    event.preventDefault();
    event.stopPropagation();

    const id = $(this).data('id');
    const clienteId = $('#contactosClienteId').val();

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

    if (confirmacion.isConfirmed) {
        try {
            const response = await fetch(`/Contactos/Delete/${id}`, {
                method: 'DELETE'
            });

            if (!response.ok) throw new Error('Error al eliminar contacto.');

            Swal.fire({
                icon: 'success',
                title: '¡Eliminado!',
                text: 'El contacto fue eliminado exitosamente.',
                timer: 2000,
                showConfirmButton: false,
                toast: true,
                position: 'top-end',
                timerProgressBar: true
            });

            await cargarTablaContactos(clienteId);

        } catch (error) {
            console.error('Error al eliminar contacto:', error);
            Swal.fire('Error', 'No se pudo eliminar el contacto.', 'error');
        }
    }
});

// 🔁 Al cerrar el modal de crear contacto
const modalCrearContacto = document.getElementById('modalCrearContacto');
if (modalCrearContacto) {
    modalCrearContacto.addEventListener('hidden.bs.modal', async () => {
        const clienteId = $('#nuevoContactoClienteId').val()?.trim();
        if (!clienteId) return;

        await cargarTablaContactos(clienteId);

        const modalContactos = new bootstrap.Modal(document.getElementById('modalContactos'));
        modalContactos.show();

        if (window._guardadoContactoExitoso) {
            Swal.fire({
                toast: true,
                position: 'top-end',
                icon: 'success',
                title: window._guardadoContactoExitoso,
                showConfirmButton: false,
                timer: 2000,
                timerProgressBar: true
            });
        }

        window._guardadoContactoExitoso = false;
    });

    // 🚨 Resetear mensaje cuando se muestra el modal
    modalCrearContacto.addEventListener('shown.bs.modal', () => {
        window._guardadoContactoExitoso = false;
    });
}
