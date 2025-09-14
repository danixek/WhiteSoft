const toggler = document.querySelector('.navbar-toggler');
const sidebar = document.querySelector('.sidebar');

toggler.addEventListener('click', () => {
    sidebar.classList.toggle('show');
});
