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
    //const header = document.querySelector('.header');
    const header = document.getElementsByTagName("header")[0]
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



// Mobile Menu functionality
function initMobileMenu() {
    const mobileMenuButton = document.querySelector('.mobile-menu-button');
    const body = document.body;

    // Create mobile menu elements
    const overlay = document.createElement('div');
    overlay.className = 'mobile-menu-overlay';

    const menuPanel = document.createElement('div');
    menuPanel.className = 'mobile-menu-panel';

    // Build menu panel content
    const isAuthenticated = document.querySelector('.user-button') !== null ||
        document.querySelector('.icon-button[href*="/Profile"]') !== null;

    // Get navigation links from desktop menu
    const desktopNavLinks = document.querySelectorAll('.main-nav .nav-link, .nav .nav-link');
    let navLinksHtml = '';

    desktopNavLinks.forEach(link => {
        const href = link.getAttribute('href');
        const text = link.textContent;
        const isActive = link.classList.contains('active') || link.classList.contains('nav-link-active');
        navLinksHtml += `
            <a href="${href}" class="mobile-nav-link ${isActive ? 'active' : ''}">
                <span class="material-symbols-outlined">${getIconForLink(text)}</span>
                <span>${text}</span>
            </a>
        `;
    });

    // Build user section
    let userSectionHtml = '';
    if (isAuthenticated) {
        const userName = document.querySelector('.user-name')?.textContent || 'Пользователь';
        const userEmail = document.querySelector('.user-email')?.textContent || 'user@example.com';
        userSectionHtml = `
            <div class="mobile-user-section">
                <div class="mobile-user-info">
                    <div class="mobile-user-avatar">
                        <img src="/images/default-avatar.png" alt="User Avatar">
                    </div>
                    <div class="mobile-user-details">
                        <div class="mobile-user-name">${userName}</div>
                        <div class="mobile-user-email">${userEmail}</div>
                    </div>
                </div>
                <button class="mobile-logout" onclick="logout()">
                    <span class="material-symbols-outlined">logout</span>
                    Выйти
                </button>
            </div>
        `;
    } else {
        userSectionHtml = `
            <div class="mobile-auth-section">
                <div class="mobile-auth-buttons">
                    <a href="/Account/Login" class="mobile-login-btn">
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
                <div class="mobile-divider"></div>
                <a href="/Cart" class="mobile-nav-link">
                    <span class="material-symbols-outlined">shopping_cart</span>
                    <span>Корзина</span>
                    <span class="cart-count">0</span>
                </a>
                <a href="/Favorites" class="mobile-nav-link">
                    <span class="material-symbols-outlined">favorite</span>
                    <span>Избранное</span>
                </a>
            </div>
        </div>
        ${userSectionHtml}
    `;

    document.body.appendChild(overlay);
    document.body.appendChild(menuPanel);

    // Helper function to get icon based on link text
    function getIconForLink(text) {
        const textLower = text.toLowerCase();
        if (textLower.includes('каталог') || textLower.includes('catalog')) return 'grid_view';
        if (textLower.includes('как арендовать')) return 'help';
        if (textLower.includes('цены') || textLower.includes('pricing')) return 'price_check';
        if (textLower.includes('поставщикам')) return 'business';
        if (textLower.includes('поддержка') || textLower.includes('support')) return 'support_agent';
        if (textLower.includes('оборудование')) return 'precision_manufacturing';
        if (textLower.includes('категории')) return 'category';
        if (textLower.includes('пользователи')) return 'people';
        if (textLower.includes('отзывы')) return 'reviews';
        return 'chevron_right';
    }

    // Open menu
    function openMenu() {
        overlay.classList.add('active');
        menuPanel.classList.add('active');
        body.classList.add('menu-open');
    }

    // Close menu
    function closeMenu() {
        overlay.classList.remove('active');
        menuPanel.classList.remove('active');
        body.classList.remove('menu-open');
    }

    // Event listeners
    if (mobileMenuButton) {
        mobileMenuButton.addEventListener('click', openMenu);
    }

    overlay.addEventListener('click', closeMenu);
    menuPanel.querySelector('.mobile-menu-close').addEventListener('click', closeMenu);

    // Close menu on link click
    menuPanel.querySelectorAll('.mobile-nav-link').forEach(link => {
        link.addEventListener('click', closeMenu);
    });
}

// Logout function
function logout() {
    // Add your logout logic here
    window.location.href = '/Account/Logout';
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    initMobileMenu();

    // Update cart count from localStorage or API
    updateCartCount();
});

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