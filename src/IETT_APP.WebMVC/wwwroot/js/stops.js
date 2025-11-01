// stops.js
document.addEventListener("DOMContentLoaded", () => {
    const selectedStops = [];
    const selectedStopsList = document.getElementById("selectedStopsList");
    const selectStopsBtn = document.getElementById("selectStopsBtn");

    function renderStops() {
        selectedStopsList.innerHTML = selectedStops.map(s => `<div>${s.Name}</div>`).join("");
        // Event yayınla haritaya güncelleme için
        document.dispatchEvent(new CustomEvent("stopsUpdated", { detail: selectedStops }));
    }

    // Başlangıçta modelden gelen durakları ekle
    const initialStops = JSON.parse(selectedStopsList.dataset.selected || "[]");
    if (initialStops.length) {
        initialStops.forEach(s => selectedStops.push(s));
        renderStops();
    }

    // Modal veya buton ile durak ekleme örneği
    selectStopsBtn.addEventListener("click", () => {
        // Örnek durak ekleme (bunu kendi modal seçim mantığınla değiştir)
        const newStop = {
            Id: crypto.randomUUID(),
            Name: "Durak " + (selectedStops.length + 1),
            Latitude: 41.01 + Math.random() * 0.01,
            Longitude: 28.97 + Math.random() * 0.01
        };
        selectedStops.push(newStop);
        renderStops();
    });
});
