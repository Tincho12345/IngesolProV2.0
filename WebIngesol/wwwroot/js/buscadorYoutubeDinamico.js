// ✅ buscadorYoutubeDinamico.js
// Autocompletado dinámico estilo YouTube, con comportamiento avanzado (búsqueda parcial con % y cache)

const debounceTimers = {};
const cacheGlobal = {};
const resultadosPorInput = {};

// ====================================================
// 🔹 Cargar datos del backend y guardarlos en cache
// ====================================================
export async function generarBuscadoresDinamicos(columnas, ruta) {
    for (const col of columnas) {
        if ((col.type || '').toLowerCase() !== 'search-youtube') continue;

        const base = col.data.replace(/Id$/, '');
        const endpoint = `/${ruta}/Obtener${base.charAt(0).toUpperCase() + base.slice(1)}`;

        if (cacheGlobal[endpoint]) continue;

        try {
            const res = await fetch(endpoint);
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const data = await res.json();
            cacheGlobal[endpoint] = Array.isArray(data) ? data : data?.data || [];
        } catch (err) {
            console.error(`❌ Error cargando ${endpoint}:`, err);
            cacheGlobal[endpoint] = [];
        }
    }
}

// ====================================================
// 🔹 Inicializar campos autocompletables
// ====================================================
export function inicializarBuscadoresYoutubeDinamicos(columnas = [], ruta) {
    columnas.forEach(col => {
        if ((col.type || '').toLowerCase() !== 'search-youtube') return;

        const base = col.data.replace(/Id$/, '');
        const endpoint = `/${ruta}/Obtener${base.charAt(0).toUpperCase() + base.slice(1)}`;
        const baseId = `edit_${col.data}`;
        const input = document.getElementById(`${baseId}_input`);
        const hidden = document.getElementById(baseId);
        const sugerenciasDiv = document.getElementById(`${baseId}_sugerencias`);
        const clearBtn = document.getElementById(`${baseId}_clear`);
        if (!input || !hidden || !sugerenciasDiv) return;

        resultadosPorInput[input.id] = [];

        // 🕐 Manejar tipeo con debounce
        input.addEventListener('input', async e => {
            clearTimeout(debounceTimers[input.id]);
            const termino = e.target.value.trim().toLowerCase();
            const lista = cacheGlobal[endpoint] || [];

            // 🔹 Mostrar/ocultar botón limpiar
            if (clearBtn) {
                clearBtn.style.display = input.value ? 'block' : 'none';
            }


            if (termino.length === 0) {
                resultadosPorInput[input.id] = lista.slice(0, 15);
                mostrarSugerencias(resultadosPorInput[input.id], sugerenciasDiv);
                return;
            }

            debounceTimers[input.id] = setTimeout(async () => {
                let encontrados = lista.filter(m => {
                    const nombre = m.nombre?.toLowerCase() || '';
                    const descripcion = m.descripcion?.toLowerCase() || '';
                    const texto = `${nombre} ${descripcion}`;
                    if (termino.includes('%')) {
                        const partes = termino.split('%').filter(p => p);
                        return partes.every(p => texto.includes(p));
                    }
                    return texto.includes(termino);
                });

                resultadosPorInput[input.id] = encontrados;
                mostrarSugerencias(encontrados, sugerenciasDiv);
            }, 50);
        });

        // ✅ Mostrar lista completa al enfocar si está vacío
        input.addEventListener('focus', () => {
            const termino = input.value.trim();
            const lista = cacheGlobal[endpoint] || [];
            if (termino.length === 0) {
                resultadosPorInput[input.id] = lista.slice(0, 15);
                mostrarSugerencias(resultadosPorInput[input.id], sugerenciasDiv);
            }
        });

        // ==================================
        // 🔹 Botón limpiar input
        // ==================================
        if (clearBtn) {
            clearBtn.addEventListener('click', () => {
                input.value = '';
                hidden.value = '';
                clearBtn.style.display = 'none';
                sugerenciasDiv.style.display = 'none';
                resultadosPorInput[input.id] = cacheGlobal[endpoint] || [];
                input.focus();
            });
        }

        // ==================================
        // 🔹 Click en sugerencia seleccionada
        // ==================================
        sugerenciasDiv.addEventListener('mousedown', e => {
            const item = e.target.closest('.list-group-item');
            if (!item) return;
            input.value = item.textContent.trim();
            hidden.value = item.dataset.id || '';
            sugerenciasDiv.style.display = 'none';
        });

        // ==================================
        // 🔹 Clic fuera del input → ocultar
        // ==================================
        document.addEventListener('mousedown', e => {
            if (!sugerenciasDiv.contains(e.target) && !input.contains(e.target)) {
                sugerenciasDiv.style.display = 'none';
            }
        });

        // ==================================
        // 🔹 Validar texto manual al salir
        // ==================================
        input.addEventListener('change', () => {
            const texto = input.value.trim().toLowerCase();
            const match = resultadosPorInput[input.id].find(m =>
                (m.nombre && m.nombre.toLowerCase().includes(texto)) ||
                (m.descripcion && m.descripcion.toLowerCase().includes(texto))
            );
            hidden.value = match ? match.id : '';
        });

        // ==================================
        // 🔹 Navegación con teclado
        // ==================================
        let indexSeleccionado = -1;

        input.addEventListener('keydown', e => {
            const opciones = sugerenciasDiv.querySelectorAll('.list-group-item');
            if (!opciones.length) return;

            switch (e.key) {
                case 'ArrowDown':
                    e.preventDefault();
                    indexSeleccionado = (indexSeleccionado + 1) % opciones.length;
                    actualizarSeleccion(opciones, indexSeleccionado);
                    break;
                case 'ArrowUp':
                    e.preventDefault();
                    indexSeleccionado = (indexSeleccionado - 1 + opciones.length) % opciones.length;
                    actualizarSeleccion(opciones, indexSeleccionado);
                    break;
                case 'Enter':
                case 'Tab':
                    if (indexSeleccionado >= 0 && indexSeleccionado < opciones.length) {
                        e.preventDefault();
                        seleccionarOpcion(opciones[indexSeleccionado], input, hidden, sugerenciasDiv);
                    }
                    indexSeleccionado = -1;
                    sugerenciasDiv.style.display = 'none';
                    break;
                case 'Escape':
                    sugerenciasDiv.style.display = 'none';
                    indexSeleccionado = -1;
                    break;
            }
        });
    });
}


// ====================================================
// 🧩 Mostrar lista de sugerencias
// ====================================================
function mostrarSugerencias(lista, contenedor) {
    contenedor.classList.add(
        'list-group',
        'bg-white',
        'border',
        'shadow-sm',
        'rounded-3'
    );
    contenedor.style.position = 'absolute';
    contenedor.style.zIndex = '9999';
    contenedor.style.maxHeight = '250px';
    contenedor.style.overflowY = 'auto';
    contenedor.style.display = 'block';
    contenedor.innerHTML = '';

    if (!lista.length) {
        contenedor.style.display = 'none';
        return;
    }

    lista.slice(0, 15).forEach(item => {
        const texto = item.nombre ?? item.descripcion ?? 'Sin nombre';
        const div = document.createElement('div');
        div.classList.add('list-group-item', 'list-group-item-action');
        div.textContent = texto;
        div.dataset.id = item.id;
        contenedor.appendChild(div);
    });

    // ✅ Localizar el input correcto (más confiable)
    const inputId = contenedor.id.replace('_sugerencias', '_input');
    const input = document.getElementById(inputId);
    if (input) {
        const rect = input.getBoundingClientRect();

        // 🧩 Ancho exacto del input o ligeramente menor (por ejemplo 95%)
        const ancho = Math.round(rect.width * 0.95);

        // Mover fuera del modal
        document.body.appendChild(contenedor);

        // ✅ Alinear visualmente justo debajo del input
        contenedor.style.top = `${rect.bottom + window.scrollY}px`;
        contenedor.style.left = `${rect.left + window.scrollX + (rect.width - ancho) / 2}px`;
        contenedor.style.width = `${ancho}px`;
        contenedor.style.boxSizing = 'border-box';
    }

    contenedor.style.display = 'block';
}


// ====================================================
// 🔹 Funciones auxiliares de teclado
// ====================================================
function actualizarSeleccion(opciones, index) {
    opciones.forEach((op, i) => op.classList.toggle('active', i === index));
}

function seleccionarOpcion(opcion, input, hidden, contenedor) {
    if (!opcion) return;
    input.value = opcion.textContent.trim();
    hidden.value = opcion.dataset.id;
    contenedor.style.display = 'none';
}
