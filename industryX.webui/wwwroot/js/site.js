(document).ready(function () {
    $('#Table').DataTable({
        language: {
            url: '/lib/datatables/i18n/Turkish.json'
        }
    });
});

document.addEventListener("DOMContentLoaded", () => {
    const main = document.querySelector("main");
    if (main) {
        main.style.opacity = 0;
        setTimeout(() => {
            main.style.transition = "opacity 0.5s ease-in-out";
            main.style.opacity = 1;
        }, 100);
    }
});
