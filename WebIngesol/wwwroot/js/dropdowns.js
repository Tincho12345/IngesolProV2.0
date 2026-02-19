// 🧠 Generar dinámicamente los dropdowns basados en columnas
export function generarDropdownsDinamicos(columnas, ruta) {
    return columnas
        .filter(c => c.type === 'dropdown' && !Array.isArray(c.options))
        .map(c => {
            const base = c.data.replace(/Id$/, '');
            return {
                endpoint: `/${ruta}/Obtener${base.charAt(0).toUpperCase() + base.slice(1)}`,
                selectId: `edit_${c.data}`
            };
        });
}

// 🔹 Función auxiliar para aplicar selección de dropdown
function aplicarSeleccion(dropdown, input, selectedValue) {
    dropdown.value = selectedValue;

    // Limpiar solo el buscador
    input.value = '';

    // Cerrar lista y quitar foco
    dropdown.size = 1;
    input.blur();

    // Pasar foco al siguiente input/select/textarea
    if (input.form) {
        const formElements = Array.from(input.form.elements)
            .filter(el => ['INPUT', 'SELECT', 'TEXTAREA'].includes(el.tagName));
        const currentIndex = formElements.indexOf(input);
        if (currentIndex >= 0 && currentIndex < formElements.length - 1) {
            formElements[currentIndex + 1].focus();
        }
    }

    // Disparar change del dropdown
    dropdown.dispatchEvent(new Event('change', { bubbles: true }));
}

// 🔹 Cargar dropdowns dinámicos con buscador + teclado + foco siguiente + valor original
export async function cargarDropdownsDinamicos(endpoint, selectId, valorActual = '') {
    const dropdown = document.getElementById(selectId);
    if (!dropdown) return console.warn(`No se encontró elemento con id "${selectId}"`);

    try {
        const res = await fetch(endpoint);
        if (!res.ok) throw new Error(`Error: ${res.status}`);
        const data = await res.json();

        const allOptions = data.map(item => ({
            value: item.id ?? item.value,
            text: item.nombre ?? item.label ?? item.value ?? 'Sin nombre'
        }));

        function renderOptions(filter = '') {
            dropdown.innerHTML = '<option value="">Seleccione...</option>';
            const filtered = allOptions.filter(opt =>
                opt.text.toLowerCase().includes(filter.toLowerCase())
            );
            filtered.forEach(opt => {
                const option = document.createElement('option');
                option.value = opt.value;
                option.textContent = opt.text;
                dropdown.appendChild(option);
            });
            if (valorActual) dropdown.value = valorActual;
            return filtered;
        }

        let filteredOptions = renderOptions();
        let selectedIndex = -1;

        // Contenedor
        let container = dropdown.parentNode;
        if (!container.classList.contains('dropdown-container')) {
            const newContainer = document.createElement('div');
            newContainer.className = 'dropdown-container';
            dropdown.parentNode.insertBefore(newContainer, dropdown);
            newContainer.appendChild(dropdown);
            container = newContainer;
        }

        // Input buscador
        let input = container.querySelector('.dropdown-search-input');
        if (!input) {
            input = document.createElement('input');
            input.type = 'text';
            input.placeholder = '🔍 Buscar...';
            input.className = 'dropdown-search-input form-control mb-2';
            container.insertBefore(input, dropdown);
        }

        // 🔹 Función para resaltar la primera opción
        function highlightFirstMatch() {
            const opts = Array.from(dropdown.options).slice(1);
            if (opts.length > 0) {
                selectedIndex = 0;
                opts.forEach((opt, i) => opt.selected = i === selectedIndex);
            }
        }

        // 🔹 Filtrado dinámico
        input.addEventListener('input', () => {
            selectedIndex = -1;
            filteredOptions = renderOptions(input.value.toLowerCase());
            if (filteredOptions.length > 0) highlightFirstMatch();
            dropdown.size = Math.min(filteredOptions.length + 1, 10); // abrir dropdown al escribir
        });

        // 🔹 Navegación con teclado (input + dropdown)
        input.addEventListener('keydown', (e) => {
            if (!filteredOptions.length) return;

            if (e.key === 'ArrowDown') {
                e.preventDefault();
                selectedIndex = (selectedIndex + 1) % filteredOptions.length;
            } else if (e.key === 'ArrowUp') {
                e.preventDefault();
                selectedIndex = (selectedIndex - 1 + filteredOptions.length) % filteredOptions.length;
            } else if (e.key === 'Enter' || e.key === 'Tab') {
                e.preventDefault();
                if (selectedIndex >= 0) {
                    const selectedValue = filteredOptions[selectedIndex].value;
                    aplicarSeleccion(dropdown, input, selectedValue);
                }
            }

            // Selección visual
            filteredOptions.forEach((opt, i) => {
                const option = Array.from(dropdown.options).find(o => o.value == opt.value);
                if (option) option.selected = i === selectedIndex;
            });
        });

        // 🔹 Selección con mouse//
        dropdown.addEventListener('mousedown', () => {
            if (input) input.value = '';
        });

        // 🔹 Foco: limpiar filtro y mostrar las 10 primeras opciones
        input.addEventListener('focus', () => {
            input.value = ''; // limpiar el texto previo
            filteredOptions = renderOptions(''); // restaurar todas las opciones
            dropdown.size = Math.min(filteredOptions.length + 1, 10); // mostrar hasta 10
            if (filteredOptions.length > 0) highlightFirstMatch();
        });

        // 🔹 Cerrar dropdown al perder el foco
        input.addEventListener('blur', () => setTimeout(() => {
            dropdown.size = 1;
            selectedIndex = -1;
        }, 150));

        // Estilo dropdown
        dropdown.style.position = 'relative';
        dropdown.style.zIndex = 1055;
        dropdown.style.background = 'white';

        // Valor inicial
        if (valorActual) aplicarSeleccion(dropdown, input, valorActual);

    } catch (err) {
        console.error(`Error cargando dropdown desde ${endpoint}:`, err);
    }
}

// 🔹 Función principal que recorre todos los dropdowns dinámicos
export async function generarCamposFormulario(columnas, ruta, valoresActuales = {}) {
    const dropdowns = generarDropdownsDinamicos(columnas, ruta);
    for (const dd of dropdowns) {
        const valor = valoresActuales[dd.selectId.replace('edit_', '')] || '';
        await cargarDropdownsDinamicos(dd.endpoint, dd.selectId, valor);
    }
}

// 🧹 Limpiar filtros y restaurar dropdowns al cerrar el modal
document.addEventListener('hidden.bs.modal', (event) => {
    const modal = event.target;
    modal.querySelectorAll('.dropdown-search-input').forEach(input => {
        input.value = '';
        const dropdown = input.nextElementSibling;
        if (dropdown && dropdown.tagName === 'SELECT') {
            input.dispatchEvent(new Event('input'));
            dropdown.size = 1;
        }
    });
});
