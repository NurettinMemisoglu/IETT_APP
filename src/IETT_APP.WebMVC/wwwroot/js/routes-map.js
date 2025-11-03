// === routes-map.js ===

let routesMap;
let routeMarkers = [];
let directionsService;
let directionsRenderer;

// Haritayı başlat
function initRoutesMap() {
    const mapElement = document.getElementById("map");
    if (!mapElement) return;

    routesMap = new google.maps.Map(mapElement, {
        center: { lat: 41.015137, lng: 28.979530 }, // İstanbul merkez
        zoom: 11
    });

    directionsService = new google.maps.DirectionsService();
    directionsRenderer = new google.maps.DirectionsRenderer({
        map: routesMap,
        suppressMarkers: false
    });

    // stopsUpdated event'ini dinle
    document.addEventListener("stopsUpdated", (e) => {
        const stops = e.detail;
        updateRouteMarkers(stops);
    });
}

// Mevcut markerları temizle
function clearRouteMarkers() {
    routeMarkers.forEach(m => m.setMap(null));
    routeMarkers = [];
    if (directionsRenderer) {
        directionsRenderer.setMap(null);
    }
    // yeni renderer oluştur
    directionsRenderer = new google.maps.DirectionsRenderer({
        map: routesMap,
        suppressMarkers: false
    });
}

// Yeni durak listesine göre marker ekle ve rota çiz
function updateRouteMarkers(stops) {
    if (!routesMap) return;

    clearRouteMarkers();

    if (!stops || stops.length === 0) return;

    const bounds = new google.maps.LatLngBounds();

    stops.forEach(stop => {
        const lat = parseFloat(stop.Lat) || parseFloat(stop.Latitude);
        const lng = parseFloat(stop.Lng) || parseFloat(stop.Longitude);
        if (isNaN(lat) || isNaN(lng)) return;

        const position = { lat, lng };

        const marker = new google.maps.Marker({
            position,
            map: routesMap,
            title: stop.Name,
        });

        const info = new google.maps.InfoWindow({
            content: `<strong>${stop.Name}</strong><br/>Lat: ${lat.toFixed(6)}, Lng: ${lng.toFixed(6)}`
        });

        marker.addListener("click", () => info.open(routesMap, marker));

        routeMarkers.push(marker);
        bounds.extend(position);
    });

    // Markerları kapsayacak şekilde haritayı ortala
    routesMap.fitBounds(bounds);

    // Eğer 2 veya daha fazla durak varsa rota çiz
    if (routeMarkers.length >= 2) {
        drawOptimizedRoute(routeMarkers);
    }
}

// Google Directions API kullanarak en kısa rotayı çiz
function drawOptimizedRoute(markers) {
    if (!directionsService || !directionsRenderer) return;

    const origin = markers[0].getPosition();
    const destination = markers[markers.length - 1].getPosition();

    const waypoints = markers.slice(1, -1).map(m => ({
        location: m.getPosition(),
        stopover: true
    }));

    directionsService.route(
        {
            origin,
            destination,
            waypoints,
            optimizeWaypoints: false, // <-- 🔹 TRUE idi, FALSE yapıyoruz
            travelMode: google.maps.TravelMode.DRIVING
        },
        (response, status) => {
            if (status === google.maps.DirectionsStatus.OK) {
                directionsRenderer.setMap(routesMap);
                directionsRenderer.setDirections(response);

                // Mesafe ve süre hesaplama
                const route = response.routes[0];
                let totalDistance = 0;
                let totalDuration = 0;

                route.legs.forEach(leg => {
                    totalDistance += leg.distance.value; // metre
                    totalDuration += leg.duration.value; // saniye
                });

                const distanceM = totalDistance;
                const durationMin = Math.round(totalDuration / 60);

                // Konsola yaz
                console.log(`🛣️ Toplam mesafe: ${distanceM} Metre`);
                console.log(`⏱️ Toplam süre: ${durationMin} dakika`);

                // İstersen HTML'e de ekleyebilirsin:
                showRouteSummary(distanceM, durationMin);
            } else {
                console.error("Rota hesaplanamadı:", status);
            }
        }
    );
}

// Mesafe & süre bilgilerini sayfada göster
function showRouteSummary(distanceKm, durationMin) {
    let summaryEl = document.getElementById("route-summary");
    if (!summaryEl) {
        summaryEl = document.createElement("div");
        summaryEl.id = "route-summary";
        summaryEl.style.position = "absolute";
        summaryEl.style.bottom = "10px";
        summaryEl.style.left = "10px";
        summaryEl.style.background = "rgba(255,255,255,0.9)";
        summaryEl.style.padding = "8px 12px";
        summaryEl.style.borderRadius = "8px";
        summaryEl.style.fontSize = "14px";
        summaryEl.style.boxShadow = "0 2px 6px rgba(0,0,0,0.2)";
        routesMap.controls[google.maps.ControlPosition.LEFT_BOTTOM].push(summaryEl);
    }
    summaryEl.innerHTML = `
        <strong>Toplam Mesafe:</strong> ${distanceKm} km<br/>
        <strong>Tahmini Süre:</strong> ${durationMin} dk
    `;
}

// Polling ile harita elementini bekle
const routesMapInterval = setInterval(() => {
    const mapElement = document.getElementById("map");
    if (mapElement && !routesMap) {
        initRoutesMap();
        clearInterval(routesMapInterval);
    }
}, 200);

// Sayfa yüklendiğinde kontrol et
document.addEventListener("DOMContentLoaded", () => {
    const mapElement = document.getElementById("map");
    if (mapElement && !routesMap) initRoutesMap();
});
