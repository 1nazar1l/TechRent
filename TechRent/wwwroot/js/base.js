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