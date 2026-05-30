// base.js - общие скрипты для всех страниц

// Theme Toggle
function toggleTheme() {
    const htmlElement = document.documentElement;
    const currentTheme = htmlElement.getAttribute('data-theme');
    const newTheme = currentTheme === 'light' ? 'dark' : 'light';

    htmlElement.setAttribute('data-theme', newTheme);

    const themeIcon = document.getElementById('theme-icon');
    if (themeIcon) {
        themeIcon.textContent = newTheme === 'light' ? 'light_mode' : 'dark_mode';
    }

    localStorage.setItem('theme', newTheme);
}

// Load saved theme
document.addEventListener('DOMContentLoaded', function () {
    const savedTheme = localStorage.getItem('theme') || 'light';
    document.documentElement.setAttribute('data-theme', savedTheme);

    const themeIcon = document.getElementById('theme-icon');
    if (themeIcon) {
        themeIcon.textContent = savedTheme === 'light' ? 'light_mode' : 'dark_mode';
    }

    // Initialize header scroll behavior
    initHeaderScroll();
});

// Header show/hide on scroll
function initHeaderScroll() {
    const header = document.getElementsByTagName("header")[0];
    let lastScrollY = window.scrollY;
    let ticking = false;

    // Добавляем CSS для sticky header
    if (!document.querySelector('#header-scroll-styles')) {
        const style = document.createElement('style');
        style.id = 'header-scroll-styles';
        style.textContent = `
            .header {
                position: fixed;
                top: 0;
                transition: transform 0.3s ease;
                will-change: transform;
            }
            .header.header-hidden {
                transform: translateY(-100%);
            }
        `;
        document.head.appendChild(style);
    }

    function updateHeader() {
        const currentScrollY = window.scrollY;

        // Скролл вниз и прокручено больше 100px
        if (currentScrollY > 100 && currentScrollY > lastScrollY) {
            header.classList.add('header-hidden');
        }
        // Скролл вверх - показываем header
        else if (currentScrollY < lastScrollY) {
            header.classList.remove('header-hidden');
        }

        lastScrollY = currentScrollY;
        ticking = false;
    }

    window.addEventListener('scroll', () => {
        if (!ticking) {
            window.requestAnimationFrame(() => {
                updateHeader();
            });
            ticking = true;
        }
    });
}

// Функция для получения anti-forgery token
function getAntiForgeryToken() {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenInput ? tokenInput.value : '';
}

// Mobile Menu functionality
function initMobileMenu() {
    const mobileMenuButton = document.querySelector('.mobile-menu-button');
    const body = document.body;

    // Create mobile menu elements
    const overlay = document.createElement('div');
    overlay.className = 'mobile-menu-overlay';

    const menuPanel = document.createElement('div');
    menuPanel.className = 'mobile-menu-panel';

    // Проверяем авторизацию
    const isAuthenticated = document.querySelector('.user-button') !== null ||
        document.querySelector('.icon-button[href*="/Profile"]') !== null ||
        document.querySelector('a.icon-button[asp-controller="Profile"]') !== null;

    // Проверяем роль администратора
    const isAdmin = document.querySelector('.nav-link[href*="/Admin"]') !== null;

    // Проверяем роль поставщика
    const isSupplier = document.querySelector('.nav-link[href*="/Supplier"]') !== null;

    // Определяем активную страницу
    const currentPath = window.location.pathname;

    // Список пунктов меню в правильном порядке (как в десктопной версии)
    const menuItems = [
        { name: 'Каталог', url: '/Equipment', icon: 'grid_view' },
        { name: 'Как арендовать', url: '#', icon: 'help' },
        { name: 'Поддержка', url: '#', icon: 'support_agent' }
    ];

    // Добавляем пункт "Поставщикам" если есть права
    if (isSupplier || isAdmin) {
        menuItems.push({ name: 'Поставщикам', url: '/Supplier', icon: 'business' });
    }

    // Добавляем пункт "Админ" если есть права
    if (isAdmin) {
        menuItems.push({ name: 'Админ', url: '/Admin', icon: 'admin_panel_settings' });
    }

    // Строим HTML для навигации (только пункты, которые есть в десктопе)
    let navLinksHtml = '';

    menuItems.forEach(item => {
        const isActive = currentPath === item.url ||
            (item.url !== '/' && item.url !== '#' && currentPath.startsWith(item.url));
        navLinksHtml += `
            <a href="${item.url}" class="mobile-nav-link ${isActive ? 'active' : ''}">
                <span class="material-symbols-outlined">${item.icon}</span>
                <span>${item.name}</span>
            </a>
        `;
    });

    // Строим пользовательскую секцию
    let userSectionHtml = '';
    if (isAuthenticated) {
        // Получаем имя пользователя
        let userName = 'Пользователь';
        const userNameElement = document.querySelector('.user-name') ||
            document.querySelector('.user-details .user-name');
        if (userNameElement) {
            userName = userNameElement.textContent;
        }

        // Используем форму для выхода как в десктопной версии
        userSectionHtml = `
            <div class="mobile-user-section">
                <form action="/Account/Logout" method="post" style="width: 100%;">
                    <input type="hidden" name="__RequestVerificationToken" value="${getAntiForgeryToken()}">
                    <button type="submit" class="mobile-logout">
                        <span class="material-symbols-outlined">logout</span>
                        Выйти
                    </button>
                </form>
            </div>
        `;
    } else {
        userSectionHtml = `
            <div class="mobile-auth-section">
                <div class="mobile-auth-buttons">
                    <a href="/Account/Auth" class="mobile-login-btn">
                        <span class="material-symbols-outlined">login</span>
                        Войти
                    </a>
                    <a href="/Account/Register" class="mobile-signup-btn">
                        <span class="material-symbols-outlined">person_add</span>
                        Регистрация
                    </a>
                </div>
            </div>
        `;
    }

    // Собираем панель меню (без лишних элементов)
    menuPanel.innerHTML = `
        <div class="mobile-menu-header">
            <div class="mobile-menu-logo">
                <div class="logo-icon">
                    <span class="material-symbols-outlined">precision_manufacturing</span>
                </div>
                <span class="logo-text">TECHRENT</span>
            </div>
            <button class="mobile-menu-close">
                <span class="material-symbols-outlined">close</span>
            </button>
        </div>
        <div class="mobile-nav">
            <div class="mobile-nav-links">
                ${navLinksHtml}
            </div>
        </div>
        ${userSectionHtml}
    `;

    document.body.appendChild(overlay);
    document.body.appendChild(menuPanel);

    // Открытие меню
    function openMenu() {
        overlay.classList.add('active');
        menuPanel.classList.add('active');
        body.classList.add('menu-open');
    }

    // Закрытие меню
    function closeMenu() {
        overlay.classList.remove('active');
        menuPanel.classList.remove('active');
        body.classList.remove('menu-open');
    }

    // Обработчики событий
    if (mobileMenuButton) {
        mobileMenuButton.addEventListener('click', openMenu);
    }

    overlay.addEventListener('click', closeMenu);

    const closeButton = menuPanel.querySelector('.mobile-menu-close');
    if (closeButton) {
        closeButton.addEventListener('click', closeMenu);
    }

    // Закрытие меню при клике на ссылку
    menuPanel.querySelectorAll('.mobile-nav-link').forEach(link => {
        link.addEventListener('click', closeMenu);
    });
}

// Update cart count
function updateCartCount() {
    const cartCount = localStorage.getItem('cartCount') || 0;
    const cartBadges = document.querySelectorAll('.cart-badge, .cart-count');
    cartBadges.forEach(badge => {
        if (parseInt(cartCount) > 0) {
            badge.textContent = cartCount;
            badge.style.display = 'flex';
        } else {
            badge.style.display = 'none';
        }
    });
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    // Инициализируем мобильное меню
    initMobileMenu();

    // Обновляем счетчик корзины
    updateCartCount();
});