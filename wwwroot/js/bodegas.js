/* ─── Estado ──────────────────────────────────────────── */
let idEliminar = null;

/* ─── Init ────────────────────────────────────────────── */
document.addEventListener('DOMContentLoaded', cargarBodegas);

/* ─── Carga y render ──────────────────────────────────── */
async function cargarBodegas() {
  setTablaLoading();
  try {
    const res = await fetch('/api/bodegas');
    if (!res.ok) throw new Error();
    const items = await res.json();
    renderTabla(items);
  } catch {
    setTablaError();
  }
}

function renderTabla(items) {
  const tbody = document.getElementById('tablaBody');
  if (!items.length) {
    tbody.innerHTML = `
      <tr><td colspan="4" class="text-center py-5 text-muted">
        <i class="bi bi-inbox fs-3 d-block mb-2"></i>Sin bodegas registradas
      </td></tr>`;
    return;
  }
  tbody.innerHTML = items.map(b => `
    <tr>
      <td class="ps-4 text-muted small">${b.bodId}</td>
      <td class="fw-semibold">${esc(b.bodNombre)}</td>
      <td>
        ${b.bodPrincipal
          ? '<span class="badge text-white" style="background:var(--brand)">Principal</span>'
          : '<span class="badge bg-secondary text-white">Secundaria</span>'}
      </td>
      <td class="text-end pe-4">
        <button class="btn btn-sm btn-outline-secondary me-1" title="Editar"
          onclick="abrirEditar(${b.bodId})">
          <i class="bi bi-pencil"></i>
        </button>
        <button class="btn btn-sm btn-outline-danger" title="Eliminar"
          onclick="abrirEliminar(${b.bodId}, '${esc(b.bodNombre).replace(/'/g, "\\'")}')">
          <i class="bi bi-trash"></i>
        </button>
      </td>
    </tr>`).join('');
}

/* ─── Modal Crear ─────────────────────────────────────── */
function abrirCrear() {
  document.getElementById('modalTitulo').textContent = 'Nueva Bodega';
  document.getElementById('bodIdEdit').value = '';
  document.getElementById('bodNombre').value = '';
  document.getElementById('bodNombre').classList.remove('is-invalid');
  document.getElementById('bodPrincipal').checked = false;
  ocultarError();
  getModal('modalBodega').show();
}

/* ─── Modal Editar ────────────────────────────────────── */
async function abrirEditar(id) {
  try {
    const res = await fetch(`/api/bodegas/${id}`);
    if (!res.ok) throw new Error();
    const b = await res.json();
    document.getElementById('modalTitulo').textContent = 'Editar Bodega';
    document.getElementById('bodIdEdit').value = b.bodId;
    document.getElementById('bodNombre').value = b.bodNombre ?? '';
    document.getElementById('bodNombre').classList.remove('is-invalid');
    document.getElementById('bodPrincipal').checked = b.bodPrincipal;
    ocultarError();
    getModal('modalBodega').show();
  } catch {
    toast('No se pudo cargar la bodega', 'danger');
  }
}

/* ─── Guardar (crear / actualizar) ───────────────────── */
async function guardar() {
  const id       = document.getElementById('bodIdEdit').value;
  const nombre   = document.getElementById('bodNombre').value.trim();
  const principal = document.getElementById('bodPrincipal').checked;

  if (!nombre) {
    document.getElementById('bodNombre').classList.add('is-invalid');
    return;
  }
  document.getElementById('bodNombre').classList.remove('is-invalid');

  const res = await fetch(id ? `/api/bodegas/${id}` : '/api/bodegas', {
    method:  id ? 'PUT' : 'POST',
    headers: { 'Content-Type': 'application/json' },
    body:    JSON.stringify({ bodNombre: nombre, bodPrincipal: principal }),
  });

  if (res.ok) {
    getModal('modalBodega').hide();
    cargarBodegas();
    toast(id ? 'Bodega actualizada correctamente' : 'Bodega creada correctamente', 'success');
  } else {
    const err = await res.json().catch(() => ({}));
    mostrarError(err.title ?? err.error ?? 'Error al guardar la bodega.');
  }
}

/* ─── Modal Eliminar ──────────────────────────────────── */
function abrirEliminar(id, nombre) {
  idEliminar = id;
  document.getElementById('nombreEliminar').textContent = nombre;
  getModal('modalEliminar').show();
}

async function confirmarEliminar() {
  getModal('modalEliminar').hide();
  const res = await fetch(`/api/bodegas/${idEliminar}`, { method: 'DELETE' });
  if (res.ok) {
    cargarBodegas();
    toast('Bodega eliminada', 'success');
  } else {
    const err = await res.json().catch(() => ({}));
    toast(err.error ?? 'No se pudo eliminar la bodega', 'danger');
  }
}

/* ─── Helpers ─────────────────────────────────────────── */
function esc(str) {
  return String(str ?? '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

function getModal(id) {
  return bootstrap.Modal.getOrCreateInstance(document.getElementById(id));
}

function ocultarError() {
  document.getElementById('modalError').classList.add('d-none');
}

function mostrarError(msg) {
  const el = document.getElementById('modalError');
  el.textContent = msg;
  el.classList.remove('d-none');
}

function setTablaLoading() {
  document.getElementById('tablaBody').innerHTML =
    '<tr><td colspan="4" class="text-center py-5 text-muted"><div class="spinner-border spinner-border-sm me-2"></div>Cargando...</td></tr>';
}

function setTablaError() {
  document.getElementById('tablaBody').innerHTML =
    '<tr><td colspan="4" class="text-center py-5 text-danger"><i class="bi bi-exclamation-circle me-2"></i>Error al cargar bodegas</td></tr>';
}

function toast(msg, tipo = 'success') {
  const el = document.getElementById('toast');
  el.className = `toast align-items-center text-white border-0 bg-${tipo}`;
  document.getElementById('toastMsg').textContent = msg;
  bootstrap.Toast.getOrCreateInstance(el, { delay: 3000 }).show();
}
