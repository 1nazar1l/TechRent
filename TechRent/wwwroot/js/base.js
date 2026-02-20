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

document.addEventListener('DOMContentLoaded', function() {
        const savedTheme = localStorage.getItem('theme') || 'light';
document.documentElement.setAttribute('data-theme', savedTheme);

const themeIcon = document.getElementById('theme-icon');
if (themeIcon) {
    themeIcon.textContent = savedTheme === 'light' ? 'light_mode' : 'dark_mode';
        }
    });