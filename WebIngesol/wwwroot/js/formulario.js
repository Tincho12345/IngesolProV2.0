    // ✅ formulario.js
    import { aplicarMascarasInputs } from './encabezado.js';

    export function generarCamposFormulario(columnas) {
        const container = $('#dynamicFormFields');
        container.empty();

        const columnasForm = columnas.filter(col => col.includeInForm !== false);
        const usarDosColumnas = columnasForm.length > 5;
        const esMateriales = columnasForm.some(c =>
            normalizeText(c.data) === 'nombre' &&
            columnasForm.some(x => normalizeText(x.data).includes('codigo'))
        );

        function normalizeText(text) {
            return text
                .toLowerCase()
                .normalize("NFD")
                .replace(/[\u0300-\u036f]/g, "");
        }

        function crearCampo(col) {
            const fieldId = `edit_${col.data}`;
            const label = col.label || col.data;
            const tipo = (col.type || 'string').toLowerCase();
            const maskAttr = col.mask ? ` data-mask="${col.mask}"` : '';

            const nombreDataNormalizado = normalizeText(col.data);
            const nombreLabelNormalizado = normalizeText(label);
            const esDescripcion = nombreDataNormalizado.includes('descripcion') || nombreLabelNormalizado.includes('descripcion');

            let input = '';

            switch (tipo) {
                case 'search-youtube': {
                    const base = col.data.replace(/Id$/, '');
                    const labelWhatsapp = col.label || base.charAt(0).toUpperCase() + base.slice(1);

                    input = `
                    <div class="mb-3" style="max-width: 600px; margin: 0 auto;">
                        <label for="${fieldId}_input" class="form-label">${labelWhatsapp}</label>
                        <div class="position-relative">
                            <input type="text" id="${fieldId}_input" class="form-control ps-5 pe-5"
                                   placeholder="Buscar ${labelWhatsapp.toLowerCase()}..." autocomplete="off"
                                   style="background-color: #f0f2f5; border: none; border-radius: 20px; height: 40px;">
                            <i class="fas fa-search position-absolute text-muted"
                               style="left: 14px; top: 50%; transform: translateY(-50%); pointer-events: none;"></i>
                            <button type="button" id="${fieldId}_clear" class="btn position-absolute p-0 border-0 bg-transparent"
                                    style="right: 14px; top: 50%; transform: translateY(-50%); display: none;">
                                <i class="fas fa-times text-secondary"></i>
                            </button>
                            <div id="${fieldId}_sugerencias" class="list-group position-absolute bg-white border shadow-sm rounded-3"
                                 style="top:105%; left:0; display:none; z-index:2000; max-height:220px; overflow-y:auto; font-size:0.9rem;"></div>
                        </div>
                        <div class="text-muted small fst-italic text-center mt-2">
                            💡 Puede escribir partes de la palabra separadas por "%" para búsquedas parciales.<br>
                            Ejemplo: <code>tor%all%cil%</code> → buscará “tornillo allen cab cilind”.
                        </div>
                        <input type="hidden" id="${fieldId}" name="${col.data}" />
                    </div>`;

                    setTimeout(() => {
                        const inputSearch = document.getElementById(`${fieldId}_input`);
                        const clearBtn = document.getElementById(`${fieldId}_clear`);
                        const sugerencias = document.getElementById(`${fieldId}_sugerencias`);

                        if (inputSearch && clearBtn) {
                            inputSearch.addEventListener('input', () => clearBtn.style.display = inputSearch.value ? 'block' : 'none');
                            clearBtn.addEventListener('click', () => {
                                inputSearch.value = '';
                                clearBtn.style.display = 'none';
                                if (sugerencias) sugerencias.style.display = 'none';
                                inputSearch.focus();
                            });
                        }
                    }, 0);

                    break;
                }
                case 'number':
                    input = `<input type="text"
                     inputmode="decimal"
                     class="form-control"
                     id="${fieldId}"
                     name="${col.data}"
                     placeholder="0.00" />`;
                    break;

                case 'date':
                    input = `<input type="date" class="form-control" id="${fieldId}" name="${col.data}" ${col.required !== false ? 'required' : ''} />`;
                    break;

                case 'datetime':
                case 'datetime-local':
                    input = `<input type="datetime-local" class="form-control" id="${fieldId}" name="${col.data}" ${col.required !== false ? 'required' : ''} />`;
                    break;

                case 'boolean':
                    input = `
                    <div class="form-check form-switch">
                        <input type="checkbox" class="form-check-input" id="${fieldId}" name="${col.data}" />
                        <label class="form-check-label" for="${fieldId}">${label}: <span id="${fieldId}_badge" class="badge bg-secondary">No</span></label>
                    </div>
                    <script>
                        setTimeout(() => {
                            const chk = document.getElementById("${fieldId}");
                            const badge = document.getElementById("${fieldId}_badge");
                            if (chk && badge) {
                                const refresh = () => {
                                    badge.textContent = chk.checked ? "Sí" : "No";
                                    badge.className = chk.checked ? "badge bg-primary" : "badge bg-danger";
                                };
                                chk.addEventListener("change", refresh);
                                refresh();
                            }
                        }, 0);
                    </script>`;
                    break;

                case 'dropdown': {
                    const globalKey = `_${col.data.replace('Id', '').toLowerCase()}`;
                    col.options = window[globalKey] || col.options || [];
                    const options = col.options.map(opt => `<option value="${opt.id ?? opt.value}">${opt.nombre ?? opt.label ?? opt.value ?? 'Sin nombre'}</option>`).join('');
                    input = `<label for="${fieldId}" class="form-label">${label}</label>
                             <select class="form-select" id="${fieldId}" name="${col.data}" ${col.required !== false ? 'required' : ''}>
                                 <option value="">Seleccione...</option>${options}
                             </select>`;
                    break;
                }

                case 'select': {
                    const options = col.options.map(opt => `<option value="${opt}">${opt}</option>`).join('');
                    input = `<select class="form-select" id="${fieldId}" name="${col.data}" ${col.required !== false ? 'required' : ''}>
                                <option value="">Seleccione...</option>${options}
                             </select>`;
                    break;
                }

                case 'file':
                    input = `
                    <label for="${fieldId}" class="form-label">${label}:</label>

                    <input type="file" class="form-control mb-3"
                           id="${fieldId}"
                           name="${col.data}"
                           accept="image/*"
                           ${col.required !== false ? 'required' : ''}>

                    <div class="border rounded p-2 bg-light text-center" style="min-height:260px; width:100%; max-width:420px;">
                        <img id="${fieldId}_preview"
                             class="img-fluid rounded shadow-sm"
                             style="max-height:380px; display:none;">
                    </div>
                `;
                    break;

                case 'password':
                    input = `<input type="password" class="form-control" id="${fieldId}" name="${col.data}" ${col.required !== false ? 'required' : ''} />`;
                    break;

                default:
                    if (col.data === 'codigoBarra') {
                        input = `
                    <div class="mb-4 text-center">
                        <label class="form-label fw-semibold mb-2">${label}:</label>

                        <div class="card shadow-sm border-0 mx-auto text-center" style="max-width: 280px;">
                            <div class="card-body">
                                <div id="${fieldId}_qr" class="d-flex justify-content-center mb-3"></div>

                                <div class="fw-bold fs-5 mb-3 text-dark" id="${fieldId}_value">
                                    —
                                </div>

                                <div class="d-flex justify-content-center gap-2">
                                    <button type="button" class="btn btn-outline-secondary btn-sm"
                                            id="${fieldId}_copy">
                                        <i class="fas fa-copy"></i>
                                    </button>

                                    <button type="button" class="btn btn-outline-secondary btn-sm"
                                            id="${fieldId}_download">
                                        <i class="fas fa-download"></i>
                                    </button>
                                </div>
                            </div>

                            <input type="hidden" id="${fieldId}" name="${col.data}" />
                        </div>
                    </div>

                    <script>
                        setTimeout(() => {
                            const hiddenInput = document.getElementById("${fieldId}");
                            const qrContainer = document.getElementById("${fieldId}_qr");
                            const valueSpan = document.getElementById("${fieldId}_value");
                            const btnCopy = document.getElementById("${fieldId}_copy");
                            const btnDownload = document.getElementById("${fieldId}_download");

                            if (!hiddenInput || !qrContainer) return;

                            function renderQR(value) {
                                qrContainer.innerHTML = "";
                                if (!value) {
                                    valueSpan.textContent = "—";
                                    return;
                                }

                                new QRCode(qrContainer, {
                                    text: value.toString(),
                                    width: 160,
                                    height: 160
                                });

                                valueSpan.textContent = value;
                            }

                            const observer = new MutationObserver(() => {
                                renderQR(hiddenInput.value);
                            });

                            observer.observe(hiddenInput, { attributes: true, attributeFilter: ['value'] });

                            btnCopy?.addEventListener('click', () => {
                                navigator.clipboard.writeText(hiddenInput.value);
                            });

                            btnDownload?.addEventListener('click', () => {
                                const img = qrContainer.querySelector('img');
                                if (!img) return;

                                const a = document.createElement('a');
                                a.href = img.src;
                                a.download = 'codigo_qr.png';
                                a.click();
                            });

                            if (hiddenInput.value) {
                                renderQR(hiddenInput.value);
                            }
                        }, 0);
                    </script>
                    `;
                    }   else {
                        input = esDescripcion
                            ? `<textarea class="form-control" id="${fieldId}" name="${col.data}" rows="3" ${col.required !== false ? 'required' : ''}></textarea>`
                            : `<input type="text" class="form-control" id="${fieldId}" name="${col.data}"${maskAttr} ${col.required !== false ? 'required' : ''} />`;
                    }

            }
            const tiposConLabelInterno = ['search-youtube', 'boolean', 'file', 'dropdown', 'select'];

            const tieneLabelInterno =
                tiposConLabelInterno.includes(tipo) || col.data === 'codigoBarra';

            return tieneLabelInterno
                ? `<div class="mb-3">${input}</div>`
                : `<div class="mb-3">
                 <label for="${fieldId}" class="form-label">${label}</label>
                    ${input}
                </div>`;
        }

        // 🔹 Renderizado de columnas
        const row = $('<div class="row"></div>');

        let colFile = null;
        let colCodigoBarra = null;
        let primerCampo = true;

        columnasForm.forEach(col => {

            if (col.data === 'codigoBarra') {
                colCodigoBarra = col;
                return;
            }

            if (col.type === 'file') {
                colFile = col;
                return;
            }

            const nombreDataNormalizado = normalizeText(col.data);
            const nombreLabelNormalizado = normalizeText(col.label || col.data);
            const esDescripcion = nombreDataNormalizado.includes('descripcion') || nombreLabelNormalizado.includes('descripcion');
            const esNombrePrincipal =
                esMateriales &&
                primerCampo &&
                normalizeText(col.data) === 'nombre';

            const colClass =
                esNombrePrincipal
                    ? 'col-12'
                    : usarDosColumnas && !esDescripcion && !col.readOnly
                        ? 'col-md-6'
                        : 'col-md-12';

            primerCampo = false;

            row.append(`<div class="${colClass}">${crearCampo(col)}</div>`);
        });

        // 🔥 FILA ESPECIAL: IMAGEN + CÓDIGO DE BARRA
        if (colFile || colCodigoBarra) {
            row.append(`
            <div class="col-12">
                <div class="row align-items-start">
                    ${colFile ? `<div class="col-md-6 d-flex justify-content-center px-4">${crearCampo(colFile)}</div>` : ''}
                    ${colCodigoBarra ? `<div class="col-md-6 d-flex justify-content-center px-4">${crearCampo(colCodigoBarra)}</div>` : ''}
                </div>
            </div>
        `);
        }

        container.append(row);
        container.closest('.modal-dialog').css({ 'max-width': usarDosColumnas ? '60%' : '600px', 'width': usarDosColumnas ? '60%' : '70%' });
        container.closest('.modal-body').css({ 'overflow-x': 'hidden' });

        container.append(`<input type="hidden" id="createEditEntityId" name="id" />`);
        container.append(`
            <div class="form-check form-switch mb-3" id="isActiveContainer">
                <input type="checkbox" class="form-check-input" id="editIsActive" name="isActive" value="true" checked />
                <label class="form-check-label" for="editIsActive" id="isActiveLabel">Activo</label>
            </div>`);

        $('#editIsActive').on('change', function () {
            $('#isActiveLabel').text($(this).is(':checked') ? 'Activo' : 'Inactivo')
                .toggleClass('text-danger', !$(this).is(':checked'));
        });

        if (typeof window._onFormularioGenerado === 'function') {
            window._onFormularioGenerado();
        }

        aplicarMascarasInputs();

        $('.modal').on('hidden.bs.modal', () => {
            document.querySelectorAll('[id$="_sugerencias"]').forEach(div => div.remove());
        });
    }
