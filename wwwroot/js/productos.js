/* ─── Estado ──────────────────────────────────────────── */
let pagina = 1;
const PAGE_SIZE = 10;
let idEliminar = null;
let filtroTimer = null;

/* ─── Init ────────────────────────────────────────────── */
document.addEventListener('DOMContentLoaded', () => {
  cargarProductos();

  document.getElementById('filtroMarca').addEventListener('keydown', e => {
    if (e.key === 'Enter') { pagina = 1; cargarProductos(); }
  });

  // Contador de caracteres en descripción
  const desc = document.getElementById('prodDescripcion');
  const count = document.getElementById('descCount');
  desc.addEventListener('input', () => {
    const len = desc.value.length;
    count.textContent = `${len} / 500`;
    count.className = len > 450 ? 'form-text text-end text-warning fw-semibold' : 'form-text text-end';
  });
});

/* ─── Carga y render ──────────────────────────────────── */
async function cargarProductos() {
  const marca = document.getElementById('filtroMarca').value.trim();
  const params = new URLSearchParams({ page: pagina, pageSize: PAGE_SIZE });
  if (marca) params.append('marca', marca);

  setTablaLoading();
  try {
    const res = await fetch(`/api/productos?${params}`);
    if (!res.ok) throw new Error();
    const data = await res.json();
    renderTabla(data.items ?? []);
    renderPaginacion(data.total, data.page, data.pageSize);
  } catch {
    setTablaError();
  }
}

function renderTabla(items) {
  const tbody = document.getElementById('tablaBody');
  if (!items.length) {
    tbody.innerHTML = `
      <tr><td colspan="5" class="text-center py-5 text-muted">
        <i class="bi bi-inbox fs-3 d-block mb-2"></i>Sin productos registrados
      </td></tr>`;
    return;
  }
  tbody.innerHTML = items.map(p => `
    <tr>
      <td class="ps-4 text-muted small">${p.prodId}</td>
      <td>
        <div class="fw-semibold">${esc(p.prodNombre)}</div>
        ${p.prodDescripcion ? `<div class="text-muted small">${esc(p.prodDescripcion)}</div>` : ''}
      </td>
      <td><span class="badge-code">${esc(p.prodCodigo)}</span></td>
      <td>${p.prodMarca ? esc(p.prodMarca) : '<span class="text-muted">—</span>'}</td>
      <td class="text-end pe-4">
        <button class="btn btn-sm btn-outline-secondary me-1" title="Editar"
          onclick="abrirEditar(${p.prodId})">
          <i class="bi bi-pencil"></i>
        </button>
        <button class="btn btn-sm btn-outline-danger" title="Eliminar"
          onclick="abrirEliminar(${p.prodId}, '${esc(p.prodNombre).replace(/'/g,"\\'")}')">
          <i class="bi bi-trash"></i>
        </button>
      </td>
    </tr>`).join('');
}

function renderPaginacion(total, page, pageSize) {
  const totalPags = Math.ceil(total / pageSize);
  const desde = Math.min((page - 1) * pageSize + 1, total);
  const hasta = Math.min(page * pageSize, total);
  document.getElementById('paginaInfo').textContent =
    total ? `Mostrando ${desde}–${hasta} de ${total} productos` : '';

  const ul = document.getElementById('paginacion');
  if (totalPags <= 1) { ul.innerHTML = ''; return; }

  let html = `<li class="page-item ${page <= 1 ? 'disabled' : ''}">
    <a class="page-link" onclick="irPagina(${page - 1})">‹</a></li>`;
  for (let i = 1; i <= totalPags; i++) {
    html += `<li class="page-item ${i === page ? 'active' : ''}">
      <a class="page-link" onclick="irPagina(${i})">${i}</a></li>`;
  }
  html += `<li class="page-item ${page >= totalPags ? 'disabled' : ''}">
    <a class="page-link" onclick="irPagina(${page + 1})">›</a></li>`;
  ul.innerHTML = html;
}

function irPagina(p) { pagina = p; cargarProductos(); }

/* ─── Filtro ──────────────────────────────────────────── */
function filtroChange() {
  clearTimeout(filtroTimer);
  filtroTimer = setTimeout(() => { pagina = 1; cargarProductos(); }, 400);
}

function limpiarFiltro() {
  document.getElementById('filtroMarca').value = '';
  pagina = 1;
  cargarProductos();
}

/* ─── Modal Crear ─────────────────────────────────────── */
function abrirModalCrear() {
  document.getElementById('modalTitulo').textContent = 'Nuevo Producto';
  document.getElementById('prodIdEdit').value = '';
  limpiarCampos();
  ocultarError();
  getModal('modalProducto').show();
}

/* ─── Modal Editar ────────────────────────────────────── */
async function abrirEditar(id) {
  try {
    const res = await fetch(`/api/productos/${id}`);
    if (!res.ok) throw new Error();
    const p = await res.json();
    document.getElementById('modalTitulo').textContent = 'Editar Producto';
    document.getElementById('prodIdEdit').value = p.prodId;
    document.getElementById('prodNombre').value = p.prodNombre ?? '';
    document.getElementById('prodCodigo').value = p.prodCodigo ?? '';
    document.getElementById('prodMarca').value = p.prodMarca ?? '';
    document.getElementById('prodDescripcion').value = p.prodDescripcion ?? '';
    ocultarError();
    getModal('modalProducto').show();
  } catch {
    toast('No se pudo cargar el producto', 'danger');
  }
}

/* ─── Guardar (crear / actualizar) ───────────────────── */
async function guardarProducto() {
  const id = document.getElementById('prodIdEdit').value;
  const body = {
    prodNombre:      document.getElementById('prodNombre').value.trim(),
    prodCodigo:      document.getElementById('prodCodigo').value.trim(),
    prodMarca:       document.getElementById('prodMarca').value.trim() || null,
    prodDescripcion: document.getElementById('prodDescripcion').value.trim() || null,
  };

  if (!validarCampos(body)) return;

  const res = await fetch(id ? `/api/productos/${id}` : '/api/productos', {
    method:  id ? 'PUT' : 'POST',
    headers: { 'Content-Type': 'application/json' },
    body:    JSON.stringify(body),
  });

  if (res.ok) {
    getModal('modalProducto').hide();
    cargarProductos();
    toast(id ? 'Producto actualizado correctamente' : 'Producto creado correctamente', 'success');
  } else {
    const err = await res.json().catch(() => ({}));
    if (err.errors) {
      // Resaltar campos con errores de la API
      Object.entries(err.errors).forEach(([campo, msgs]) => {
        const key = campo.charAt(0).toLowerCase() + campo.slice(1);
        const el = document.getElementById(key);
        if (el) { el.classList.add('is-invalid'); }
      });
      mostrarError(Object.values(err.errors).flat().join(' · '));
    } else {
      mostrarError(err.title ?? err.error ?? 'Error al guardar el producto.');
    }
  }
}

function validarCampos(body) {
  let valido = true;

  const nombre = document.getElementById('prodNombre');
  if (!body.prodNombre) {
    nombre.classList.add('is-invalid');
    valido = false;
  } else {
    nombre.classList.remove('is-invalid');
  }

  const codigo = document.getElementById('prodCodigo');
  if (!body.prodCodigo) {
    codigo.classList.add('is-invalid');
    valido = false;
  } else {
    codigo.classList.remove('is-invalid');
  }

  if (!valido) ocultarError();
  return valido;
}

/* ─── Modal Eliminar ──────────────────────────────────── */
function abrirEliminar(id, nombre) {
  idEliminar = id;
  document.getElementById('nombreEliminar').textContent = nombre;
  getModal('modalEliminar').show();
}

async function confirmarEliminar() {
  getModal('modalEliminar').hide();
  const res = await fetch(`/api/productos/${idEliminar}`, { method: 'DELETE' });
  if (res.ok) {
    cargarProductos();
    toast('Producto eliminado', 'success');
  } else {
    toast('No se pudo eliminar el producto', 'danger');
  }
}

/* ─── Helpers ─────────────────────────────────────────── */
function esc(str) {
  return String(str ?? '').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}

function getModal(id) {
  return bootstrap.Modal.getOrCreateInstance(document.getElementById(id));
}

function limpiarCampos() {
  ['prodNombre','prodCodigo','prodMarca','prodDescripcion'].forEach(id => {
    const el = document.getElementById(id);
    el.value = '';
    el.classList.remove('is-invalid', 'is-valid');
  });
  const count = document.getElementById('descCount');
  if (count) count.textContent = '0 / 500';
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
    '<tr><td colspan="5" class="text-center py-5 text-muted"><div class="spinner-border spinner-border-sm me-2"></div>Cargando...</td></tr>';
}

function setTablaError() {
  document.getElementById('tablaBody').innerHTML =
    '<tr><td colspan="5" class="text-center py-5 text-danger"><i class="bi bi-exclamation-circle me-2"></i>Error al cargar productos</td></tr>';
}

function toast(msg, tipo = 'success') {
  const el = document.getElementById('toast');
  el.className = `toast align-items-center text-white border-0 bg-${tipo}`;
  document.getElementById('toastMsg').textContent = msg;
  bootstrap.Toast.getOrCreateInstance(el, { delay: 3000 }).show();
}
