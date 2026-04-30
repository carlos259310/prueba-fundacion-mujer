/* ─── Estado ──────────────────────────────────────────── */
let histPagina = 1;
const HIST_PAGE_SIZE = 10;
let bodegaMap = {};   // bodId → bodNombre

/* ─── Init ────────────────────────────────────────────── */
document.addEventListener('DOMContentLoaded', async () => {
  await Promise.allSettled([cargarProductosEnSelects(), cargarBodegas()]);
});

async function cargarProductosEnSelects() {
  try {
    const res = await fetch('/api/productos?page=1&pageSize=200');
    if (!res.ok) return;
    const data  = await res.json();
    const items = data.items ?? [];
    ['histProducto', 'repProducto', 'movProducto'].forEach(selId => {
      const sel = document.getElementById(selId);
      items.forEach(p => {
        const opt = document.createElement('option');
        opt.value = p.prodId;
        opt.textContent = p.prodNombre;
        sel.appendChild(opt);
      });
    });
  } catch { /* silent */ }
}

async function cargarBodegas() {
  try {
    const res = await fetch('/api/bodegas');
    if (!res.ok) return;
    const bodegas = await res.json();
    bodegaMap = Object.fromEntries(bodegas.map(b => [b.bodId, b.bodNombre]));
    ['movBodegaInicial', 'movBodegaFinal'].forEach(selId => {
      const sel = document.getElementById(selId);
      bodegas.forEach(b => {
        const opt = document.createElement('option');
        opt.value = b.bodId;
        opt.textContent = b.bodNombre;
        sel.appendChild(opt);
      });
    });
  } catch { /* silent */ }
}

/* ─── Tabs ────────────────────────────────────────────── */
function cambiarTab(tab, el) {
  document.querySelectorAll('#tabsMov .nav-link').forEach(a => a.classList.remove('active'));
  el.classList.add('active');
  document.getElementById('tabHistorial').classList.toggle('d-none', tab !== 'historial');
  document.getElementById('tabReporte').classList.toggle('d-none', tab !== 'reporte');
  return false;
}

/* ─── Historial ───────────────────────────────────────── */
async function cargarHistorial() {
  histPagina = 1;
  await fetchHistorial(document.getElementById('histProducto').value, 1);
}

async function fetchHistorial(prodId, pagina) {
  const tbody = document.getElementById('tablaHistorial');
  if (!prodId) {
    tbody.innerHTML = `<tr><td colspan="7" class="text-center py-5 text-muted">
      <i class="bi bi-clock-history fs-3 d-block mb-2"></i>
      Selecciona un producto para ver el historial
    </td></tr>`;
    document.getElementById('histInfo').textContent = '';
    document.getElementById('histPaginacion').innerHTML = '';
    return;
  }

  tbody.innerHTML = `<tr><td colspan="7" class="text-center py-5 text-muted">
    <div class="spinner-border spinner-border-sm me-2"></div>Cargando...
  </td></tr>`;

  try {
    const res = await fetch(`/api/movimientos/${prodId}?page=${pagina}&pageSize=${HIST_PAGE_SIZE}`);
    if (!res.ok) throw new Error();
    const data = await res.json();
    renderHistorial(data.items ?? [], data.total, data.page, data.pageSize, prodId);
  } catch {
    tbody.innerHTML = `<tr><td colspan="7" class="text-center py-5 text-danger">
      <i class="bi bi-exclamation-circle me-2"></i>Error al cargar el historial
    </td></tr>`;
  }
}

function renderHistorial(items, total, page, pageSize, prodId) {
  const tbody = document.getElementById('tablaHistorial');
  if (!items.length) {
    tbody.innerHTML = `<tr><td colspan="7" class="text-center py-5 text-muted">
      <i class="bi bi-inbox fs-3 d-block mb-2"></i>Sin movimientos registrados para este producto
    </td></tr>`;
    document.getElementById('histInfo').textContent = '';
    document.getElementById('histPaginacion').innerHTML = '';
    return;
  }

  const tipoBgMap = { Entrada: '#15803d', Salida: '#b91c1c', Traslado: '#7e22ce' };

  tbody.innerHTML = items.map(m => {
    const fecha = new Date(m.movFecha).toLocaleString('es-CO', {
      day: '2-digit', month: '2-digit', year: 'numeric',
      hour: '2-digit', minute: '2-digit',
    });
    const tipoBg  = tipoBgMap[m.movTipo] ?? '#6b7280';
    const origen  = m.movBodegaInicial ? (bodegaMap[m.movBodegaInicial] ?? `Bodega #${m.movBodegaInicial}`) : '—';
    const destino = m.movBodegaFinal   ? (bodegaMap[m.movBodegaFinal]   ?? `Bodega #${m.movBodegaFinal}`)   : '—';
    return `
    <tr>
      <td class="ps-4 text-muted small">${esc(fecha)}</td>
      <td class="fw-semibold">${esc(m.prodNombre)}</td>
      <td><span class="badge text-white" style="background:${tipoBg}">${esc(m.movTipo)}</span></td>
      <td class="text-muted small">${esc(m.movConcepto)}</td>
      <td class="text-muted small">${esc(origen)}</td>
      <td class="text-muted small">${esc(destino)}</td>
      <td class="text-end pe-4 fw-semibold">${m.movCantidad}</td>
    </tr>`;
  }).join('');

  const desde = (page - 1) * pageSize + 1;
  const hasta = Math.min(page * pageSize, total);
  document.getElementById('histInfo').textContent =
    `Mostrando ${desde}–${hasta} de ${total} movimientos`;

  renderHistPaginacion(total, page, pageSize, prodId);
}

function renderHistPaginacion(total, page, pageSize, prodId) {
  const totalPags = Math.ceil(total / pageSize);
  const ul = document.getElementById('histPaginacion');
  if (totalPags <= 1) { ul.innerHTML = ''; return; }

  let html = `<li class="page-item ${page <= 1 ? 'disabled' : ''}">
    <a class="page-link" href="#" onclick="irHistPagina(${page - 1}, '${prodId}'); return false;">‹</a></li>`;
  for (let i = 1; i <= totalPags; i++) {
    html += `<li class="page-item ${i === page ? 'active' : ''}">
      <a class="page-link" href="#" onclick="irHistPagina(${i}, '${prodId}'); return false;">${i}</a></li>`;
  }
  html += `<li class="page-item ${page >= totalPags ? 'disabled' : ''}">
    <a class="page-link" href="#" onclick="irHistPagina(${page + 1}, '${prodId}'); return false;">›</a></li>`;
  ul.innerHTML = html;
}

function irHistPagina(p, prodId) {
  histPagina = p;
  fetchHistorial(prodId, p);
}

/* ─── Reporte ─────────────────────────────────────────── */
async function generarReporte() {
  const desde  = document.getElementById('repDesde').value;
  const hasta  = document.getElementById('repHasta').value;
  const prodId = document.getElementById('repProducto').value;
  const errEl  = document.getElementById('repError');

  errEl.classList.add('d-none');
  document.getElementById('repResumen').classList.add('d-none');
  document.getElementById('repVacio').classList.add('d-none');

  if (!desde || !hasta) {
    errEl.textContent = 'Las fechas de inicio y fin son obligatorias.';
    errEl.classList.remove('d-none');
    return;
  }
  if (hasta < desde) {
    errEl.textContent = 'La fecha "Hasta" debe ser mayor o igual a "Desde".';
    errEl.classList.remove('d-none');
    return;
  }

  const params = new URLSearchParams({ fechaDesde: desde, fechaHasta: hasta });
  if (prodId) params.append('prodId', prodId);

  try {
    const res = await fetch(`/api/movimientos/reporte?${params}`);
    if (!res.ok) throw new Error();
    const data = await res.json();
    renderReporte(data);
  } catch {
    errEl.textContent = 'Error al generar el reporte.';
    errEl.classList.remove('d-none');
  }
}

function renderReporte(data) {
  document.getElementById('repTotal').textContent       = data.totalMovimientos;
  document.getElementById('repEntradas').textContent    = data.totalEntradas;
  document.getElementById('repSalidas').textContent     = data.totalSalidas;
  document.getElementById('repTrasladados').textContent = data.totalTrasladados;

  if (!data.detalle?.length) {
    document.getElementById('repVacio').classList.remove('d-none');
    return;
  }

  document.getElementById('tablaReporte').innerHTML = data.detalle.map(d => `
    <tr>
      <td class="ps-4">${d.fecha}</td>
      <td class="text-center" style="color:#15803d"><strong>${d.entradas}</strong></td>
      <td class="text-center" style="color:#b91c1c"><strong>${d.salidas}</strong></td>
      <td class="text-center" style="color:#7e22ce"><strong>${d.traslados}</strong></td>
      <td class="text-end pe-4 fw-semibold">${d.total}</td>
    </tr>`).join('');

  document.getElementById('repResumen').classList.remove('d-none');
}

/* ─── Modal Registrar ─────────────────────────────────── */
function abrirRegistrar() {
  ['movProducto', 'movTipo', 'movConcepto', 'movCantidad', 'movBodegaInicial', 'movBodegaFinal'].forEach(id => {
    const el = document.getElementById(id);
    el.value = '';
    el.classList.remove('is-invalid');
  });
  document.getElementById('campoOrigen').classList.add('d-none');
  document.getElementById('campoDestino').classList.add('d-none');
  document.getElementById('modalError').classList.add('d-none');
  getModal('modalMovimiento').show();
}

function onTipoChange() {
  const tipo = document.getElementById('movTipo').value;
  document.getElementById('movTipo').classList.remove('is-invalid');
  const needsOrigen  = tipo === 'Salida'  || tipo === 'Traslado';
  const needsDestino = tipo === 'Entrada' || tipo === 'Traslado';
  document.getElementById('campoOrigen').classList.toggle('d-none', !needsOrigen);
  document.getElementById('campoDestino').classList.toggle('d-none', !needsDestino);
  document.getElementById('movBodegaInicial').value = '';
  document.getElementById('movBodegaInicial').classList.remove('is-invalid');
  document.getElementById('movBodegaFinal').value = '';
  document.getElementById('movBodegaFinal').classList.remove('is-invalid');
}

function limpiarInvalid(id) {
  document.getElementById(id).classList.remove('is-invalid');
}

async function registrarMovimiento() {
  const prodId    = document.getElementById('movProducto').value;
  const tipo      = document.getElementById('movTipo').value;
  const concepto  = document.getElementById('movConcepto').value;
  const cantidad  = parseInt(document.getElementById('movCantidad').value);
  const bodInicial = document.getElementById('movBodegaInicial').value;
  const bodFinal   = document.getElementById('movBodegaFinal').value;

  const needsOrigen  = tipo === 'Salida'  || tipo === 'Traslado';
  const needsDestino = tipo === 'Entrada' || tipo === 'Traslado';

  let valido = true;
  if (!prodId)   { document.getElementById('movProducto').classList.add('is-invalid');  valido = false; }
  if (!tipo)     { document.getElementById('movTipo').classList.add('is-invalid');       valido = false; }
  if (!concepto) { document.getElementById('movConcepto').classList.add('is-invalid');   valido = false; }
  if (!cantidad || cantidad < 1) { document.getElementById('movCantidad').classList.add('is-invalid'); valido = false; }
  if (needsOrigen  && !bodInicial) { document.getElementById('movBodegaInicial').classList.add('is-invalid'); valido = false; }
  if (needsDestino && !bodFinal)   { document.getElementById('movBodegaFinal').classList.add('is-invalid');   valido = false; }
  if (!valido) return;

  const body = {
    prodId:           parseInt(prodId),
    movTipo:          tipo,
    movConcepto:      concepto,
    movCantidad:      cantidad,
    movBodegaInicial: needsOrigen  ? parseInt(bodInicial) : null,
    movBodegaFinal:   needsDestino ? parseInt(bodFinal)   : null,
  };

  const res = await fetch('/api/movimientos', {
    method:  'POST',
    headers: { 'Content-Type': 'application/json' },
    body:    JSON.stringify(body),
  });

  if (res.ok) {
    getModal('modalMovimiento').hide();
    const histProd = document.getElementById('histProducto').value;
    if (histProd && histProd === prodId) fetchHistorial(prodId, 1);
    toast('Movimiento registrado correctamente', 'success');
  } else {
    const err = await res.json().catch(() => ({}));
    const errEl = document.getElementById('modalError');
    if (err.errors) {
      errEl.textContent = Object.values(err.errors).flat().join(' · ');
    } else {
      errEl.textContent = err.error ?? err.title ?? 'Error al registrar el movimiento.';
    }
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

function toast(msg, tipo = 'success') {
  const el = document.getElementById('toast');
  el.className = `toast align-items-center text-white border-0 bg-${tipo}`;
  document.getElementById('toastMsg').textContent = msg;
  bootstrap.Toast.getOrCreateInstance(el, { delay: 3000 }).show();
}
