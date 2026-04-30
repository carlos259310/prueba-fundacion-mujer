/* ─── Estado ──────────────────────────────────────────── */
let prodIdActual = null;

/* ─── Init ────────────────────────────────────────────── */
document.addEventListener('DOMContentLoaded', async () => {
  await cargarProductos();
  cargarTodoStock();
});

async function cargarProductos() {
  try {
    const res = await fetch('/api/productos?page=1&pageSize=200');
    if (!res.ok) return;
    const data = await res.json();
    const sel = document.getElementById('selectProducto');
    (data.items ?? []).forEach(p => {
      const opt = document.createElement('option');
      opt.value = p.prodId;
      opt.textContent = p.prodNombre;
      sel.appendChild(opt);
    });
  } catch { /* silent */ }
}

/* ─── Carga de stock ──────────────────────────────────── */
async function cargarTodoStock(prodId = null) {
  prodIdActual = prodId;
  setTablaLoading();
  try {
    const url = prodId ? `/api/inventario?prodId=${prodId}` : '/api/inventario';
    const res = await fetch(url);
    if (!res.ok) throw new Error();
    const items = await res.json();
    renderTabla(items);
  } catch {
    setTablaError();
  }
}

function filtrarStock() {
  const val = document.getElementById('selectProducto').value;
  cargarTodoStock(val || null);
}

function recargar() {
  const val = document.getElementById('selectProducto').value;
  cargarTodoStock(val || null);
}

function renderTabla(items) {
  const tbody = document.getElementById('tablaBody');
  if (!items.length) {
    tbody.innerHTML = `
      <tr><td colspan="4" class="text-center py-5 text-muted">
        <i class="bi bi-inbox fs-3 d-block mb-2"></i>
        Sin stock registrado
      </td></tr>`;
    return;
  }
  tbody.innerHTML = items.map(inv => `
    <tr>
      <td class="ps-4 fw-semibold">${esc(inv.prodNombre)}</td>
      <td class="text-muted">${esc(inv.bodNombre)}</td>
      <td>
        <span class="fw-bold fs-5 ${inv.invStock === 0 ? 'text-danger' : 'text-brand'}">${inv.invStock}</span>
        <span class="text-muted small ms-1">uds</span>
      </td>
      <td class="text-end pe-4">
        <button class="btn btn-sm btn-outline-secondary"
          onclick="abrirAjuste(${inv.prodId}, ${inv.bodId}, '${esc(inv.prodNombre).replace(/'/g, "\\'")}', '${esc(inv.bodNombre).replace(/'/g, "\\'")}')">
          <i class="bi bi-sliders me-1"></i>Ajustar
        </button>
      </td>
    </tr>`).join('');
}

/* ─── Modal Ajuste ────────────────────────────────────── */
function abrirAjuste(prodId, bodId, prodNombre, bodNombre) {
  document.getElementById('ajusteProdId').value = prodId;
  document.getElementById('ajusteBodId').value  = bodId;
  document.getElementById('ajusteInfo').textContent = `${prodNombre} — ${bodNombre}`;

  ['ajusteTipo', 'ajusteConcepto', 'ajusteCantidad'].forEach(id => {
    const el = document.getElementById(id);
    el.value = '';
    el.classList.remove('is-invalid');
  });
  document.getElementById('modalError').classList.add('d-none');
  getModal('modalAjuste').show();
}

function actualizarConcepto() {
  document.getElementById('ajusteTipo').classList.remove('is-invalid');
  document.getElementById('ajusteConcepto').value = '';
  document.getElementById('ajusteConcepto').classList.remove('is-invalid');
}

async function guardarAjuste() {
  const prodId   = document.getElementById('ajusteProdId').value;
  const bodId    = document.getElementById('ajusteBodId').value;
  const tipo     = document.getElementById('ajusteTipo').value;
  const concepto = document.getElementById('ajusteConcepto').value;
  const cantidad = parseInt(document.getElementById('ajusteCantidad').value);

  let valido = true;
  if (!tipo)    { document.getElementById('ajusteTipo').classList.add('is-invalid');    valido = false; }
  if (!concepto){ document.getElementById('ajusteConcepto').classList.add('is-invalid'); valido = false; }
  if (!cantidad || cantidad < 1) { document.getElementById('ajusteCantidad').classList.add('is-invalid'); valido = false; }
  if (!valido) return;

  const res = await fetch(`/api/productos/${prodId}/stock/${bodId}`, {
    method:  'PATCH',
    headers: { 'Content-Type': 'application/json' },
    body:    JSON.stringify({ cantidad, tipo, concepto }),
  });

  if (res.ok) {
    getModal('modalAjuste').hide();
    recargar();
    toast('Stock ajustado correctamente', 'success');
  } else {
    const err = await res.json().catch(() => ({}));
    const errEl = document.getElementById('modalError');
    errEl.textContent = err.error ?? err.title ?? 'Error al ajustar el stock.';
    errEl.classList.remove('d-none');
  }
}

/* ─── Helpers ─────────────────────────────────────────── */
function esc(str) {
  return String(str ?? '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

function getModal(id) {
  return bootstrap.Modal.getOrCreateInstance(document.getElementById(id));
}

function setTablaLoading() {
  document.getElementById('tablaBody').innerHTML =
    '<tr><td colspan="4" class="text-center py-5 text-muted"><div class="spinner-border spinner-border-sm me-2"></div>Cargando...</td></tr>';
}

function setTablaError() {
  document.getElementById('tablaBody').innerHTML =
    '<tr><td colspan="4" class="text-center py-5 text-danger"><i class="bi bi-exclamation-circle me-2"></i>Error al cargar el inventario</td></tr>';
}

function toast(msg, tipo = 'success') {
  const el = document.getElementById('toast');
  el.className = `toast align-items-center text-white border-0 bg-${tipo}`;
  document.getElementById('toastMsg').textContent = msg;
  bootstrap.Toast.getOrCreateInstance(el, { delay: 3000 }).show();
}
