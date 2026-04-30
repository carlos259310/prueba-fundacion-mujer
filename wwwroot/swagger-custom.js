(function () {
  function injectLink() {
    const wrapper = document.querySelector('.swagger-ui .topbar-wrapper');
    if (!wrapper || wrapper.querySelector('.back-to-ui')) return false;

    wrapper.style.display = 'flex';
    wrapper.style.alignItems = 'center';
    wrapper.style.width = '100%';

    const a = document.createElement('a');
    a.href = '/';
    a.className = 'back-to-ui';
    a.innerHTML = '&#8592; Volver al sistema';
    a.style.cssText = [
      'color:#d8b4fe',
      'font-size:.85rem',
      'font-weight:500',
      'text-decoration:none',
      'margin-left:auto',
      'margin-right:1.5rem',
      'white-space:nowrap',
      'transition:color .15s',
    ].join(';');
    a.addEventListener('mouseover', () => { a.style.color = '#fff'; });
    a.addEventListener('mouseout',  () => { a.style.color = '#d8b4fe'; });

    wrapper.appendChild(a);
    return true;
  }

  const timer = setInterval(() => { if (injectLink()) clearInterval(timer); }, 150);
  setTimeout(() => clearInterval(timer), 8000);
})();
