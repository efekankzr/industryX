$(document).ready(function () {
    // DataTable kurulumu
    $('#Table').DataTable({
        language: {
            url: '/lib/datatables/i18n/Turkish.json'
        }
    });

    // Sayfa animasyonu
    const main = document.querySelector("main");
    if (main) {
        main.style.opacity = 0;
        setTimeout(() => {
            main.style.transition = "opacity 0.5s ease-in-out";
            main.style.opacity = 1;
        }, 100);
    }
});
