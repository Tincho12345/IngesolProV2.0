// Elimina una fila del DataTable (solo interno)
function eliminarFilaDataTable(id) {
    const tabla = $('#tblList').DataTable();
    tabla.rows().every(function () {
        const data = this.data();
        if (data && data.id === id) {
            this.remove();
        }
    });
    tabla.draw(false);
}

// Wrapper que llama al backend y luego actualiza el DataTable
export async function eliminarRecursoWrapper(id) {
    try {
        const response = await fetch(`/${window._ruta}/Delete/${id}`, {
            method: 'DELETE'
        });

        if (!response.ok) {
            throw new Error(`Error al eliminar: ${response.statusText}`);
        }

        const data = await response.json();

        // Mostrar toast con resultado
        Swal.fire({
            toast: true,
            position: 'top-end',
            icon: data.success ? 'success' : 'error',
            title: data.message,
            showConfirmButton: false,
            timer: 2000,
            timerProgressBar: true
        });

        if (data.success) {
            // 🔹 Solo eliminar fila si el backend confirma
            eliminarFilaDataTable(id);
        }

    } catch (error) {
        Swal.fire('Error', error.message || 'Ocurrió un error al eliminar', 'error');
        console.error('Error al eliminar recurso:', error);
    }
}
