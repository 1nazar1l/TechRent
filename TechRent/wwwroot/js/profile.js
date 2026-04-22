// Мобильное меню
function initMobileSidebar() {
    const sidebar = document.querySelector('.profile-sidebar');
    const overlay = document.createElement('div');
    overlay.className = 'sidebar-overlay';
    document.body.appendChild(overlay);
        
    // Создаем кнопку открытия меню
    const mobileTrigger = document.createElement('button');
    mobileTrigger.className = 'mobile-menu-trigger';
    mobileTrigger.innerHTML = '<span class="material-symbols-outlined">menu</span>';
        
    const profileContainer = document.querySelector('.profile-container');
    if (profileContainer) {
        profileContainer.insertBefore(mobileTrigger, profileContainer.firstChild);
    }
        
    function openMenu() {
        sidebar.classList.add('open');
        overlay.classList.add('active');
        document.body.classList.add('menu-open');
    }
        
    function closeMenu() {
        sidebar.classList.remove('open');
        overlay.classList.remove('active');
        document.body.classList.remove('menu-open');
    }
        
    mobileTrigger.addEventListener('click', openMenu);
    overlay.addEventListener('click', closeMenu);
        
    // Закрываем меню при клике на ссылку
    sidebar.querySelectorAll('.sidebar-link').forEach(link => {
        link.addEventListener('click', closeMenu);
    });
}
    
document.addEventListener('DOMContentLoaded', function() {
    if (window.innerWidth <= 767) {
        initMobileSidebar();
    }
});
    
window.addEventListener('resize', function() {
    if (window.innerWidth > 767) {
        const sidebar = document.querySelector('.profile-sidebar');
        const overlay = document.querySelector('.sidebar-overlay');
        if (sidebar) sidebar.classList.remove('open');
        if (overlay) overlay.classList.remove('active');
        document.body.classList.remove('menu-open');
    }
});
