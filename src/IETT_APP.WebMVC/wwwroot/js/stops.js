/* =========================================================
   STOPS.JS - Durak Yönetimi Ortak Javascript Dosyası
   Hem Index (Liste) hem de Create/Edit (Seçim) sayfalarında çalışır.
   ========================================================= */

// 1. DURAK SEÇİM MANTIĞI (Genellikle Create/Edit sayfalarında kullanılır)
document.addEventListener("DOMContentLoaded", () => {
    const selectedStopsList = document.getElementById("selectedStopsList");
    const selectStopsBtn = document.getElementById("selectStopsBtn");

    // Sadece bu elemanlar sayfada varsa çalıştır (Hata almamak için)
    if (selectedStopsList && selectStopsBtn) {
        const selectedStops = [];

        function renderStops() {
            selectedStopsList.innerHTML = selectedStops.map(s => `<div>${s.Name}</div>`).join("");
            // Harita güncellemesi için event yayınla
            document.dispatchEvent(new CustomEvent("stopsUpdated", { detail: selectedStops }));
        }

        // Başlangıçta modelden gelen durakları ekle
        const initialStops = JSON.parse(selectedStopsList.dataset.selected || "[]");
        if (initialStops.length) {
            initialStops.forEach(s => selectedStops.push(s));
            renderStops();
        }

        // Modal veya buton ile durak ekleme
        selectStopsBtn.addEventListener("click", () => {
            // Örnek durak ekleme mantığı
            const newStop = {
                Id: crypto.randomUUID(),
                Name: "Durak " + (selectedStops.length + 1),
                Latitude: 41.01 + Math.random() * 0.01,
                Longitude: 28.97 + Math.random() * 0.01
            };
            selectedStops.push(newStop);
            renderStops();
        });
    }
});


// 2. AJAX ARAMA MANTIĞI (Index/Liste Sayfası için)
$(document).ready(function () {
    const $searchInput = $('#searchInput');
    const $tableContainer = $('#stopsTableContainer');

    // Sadece arama kutusu sayfada varsa çalıştır
    if ($searchInput.length > 0) {
        let typingTimer;
        const doneTypingInterval = 300; // 300ms bekle

        // Arama kutusuna yazıldığında tetiklenir
        $searchInput.on('input', function () {
            clearTimeout(typingTimer);
            var term = $(this).val();

            typingTimer = setTimeout(function () {
                refreshTable(term);
            }, doneTypingInterval);
        });

        // Tabloyu sunucudan yenileyen fonksiyon
        function refreshTable(term) {
            // Opaklığı düşür (loading efekti)
            $tableContainer.css('opacity', '0.5');

            // Controller URL'in (Search veya Index) doğru olduğundan emin ol
            // Önceki konuşmamızda tek Index metodu kullanmaya karar vermiştik.
            // Eğer Index kullanıyorsan url: '/Planner/Stops/Index' olmalı.
            // Eğer Search Action'ı ayrıysa '/Planner/Stops/Search' kalmalı.
            $.get('/Planner/Stops/Search', { term: term })
                .done(function (data) {
                    $tableContainer.html(data);
                    $tableContainer.css('opacity', '1');

                    // Tablo yenilendiğinde tooltip'leri tekrar çalıştır (Bootstrap 5)
                    if (typeof bootstrap !== 'undefined') {
                        var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
                        var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
                            return new bootstrap.Tooltip(tooltipTriggerEl);
                        });
                    }
                })
                .fail(function () {
                    console.error("Arama sırasında hata oluştu.");
                    $tableContainer.css('opacity', '1');
                });
        }
    }
});