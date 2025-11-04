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
function showRouteSummary(distanceM, durationMin) {
    // 🔹 Global değişkende sakla
    routeSummary = { distanceM, durationMin };

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

    // 🔸 Mesafeyi artık metre olarak gösteriyoruz
    summaryEl.innerHTML = `
        <strong>Toplam Mesafe:</strong> ${Math.round(distanceM)} m<br/>
        <strong>Tahmini Süre:</strong> ${durationMin} dk
    `;

    // 🔹 routes.js'e gönder
    document.dispatchEvent(new CustomEvent("routeSummaryUpdated", { detail: routeSummary }));
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


    // 🔹 Duraklar Modalı için Mini Harita
let stopsMiniMap;
let stopsMiniMarkers = [];
let stopsMiniDirectionsRenderer;
let stopsMiniDirectionsService;

function showStopsMiniMap(stops) {
    if (!stops || stops.length === 0) return;

    // Önceki haritayı temizle
    if (stopsMiniMap) {
        stopsMiniMarkers.forEach(m => m.setMap(null));
        stopsMiniMarkers = [];
        if (stopsMiniDirectionsRenderer) stopsMiniDirectionsRenderer.setMap(null);
    }

    // Mini haritayı oluştur
    stopsMiniMap = new google.maps.Map(document.getElementById("stopsMiniMap"), {
        center: { lat: parseFloat(stops[0].Lat), lng: parseFloat(stops[0].Lng) },
        zoom: 12,
        disableDefaultUI: true,
        draggable: false,
        scrollwheel: false,
        keyboardShortcuts: false
    });

    stopsMiniDirectionsService = new google.maps.DirectionsService();
    stopsMiniDirectionsRenderer = new google.maps.DirectionsRenderer({
        map: stopsMiniMap,
        suppressMarkers: false,
        draggable: false,
        preserveViewport: true
    });

    const bounds = new google.maps.LatLngBounds();

    // Marker ekle
    stops.forEach(stop => {
        const position = { lat: parseFloat(stop.Lat), lng: parseFloat(stop.Lng) };
        const marker = new google.maps.Marker({
            position,
            map: stopsMiniMap,
            title: stop.Name
        });
        stopsMiniMarkers.push(marker);
        bounds.extend(position);
    });

    stopsMiniMap.fitBounds(bounds);

    // Rota çiz
    if (stopsMiniMarkers.length >= 2) {
        const origin = stopsMiniMarkers[0].getPosition();
        const destination = stopsMiniMarkers[stopsMiniMarkers.length - 1].getPosition();
        const waypoints = stopsMiniMarkers.slice(1, -1).map(m => ({ location: m.getPosition(), stopover: true }));

        stopsMiniDirectionsService.route({
            origin,
            destination,
            waypoints,
            travelMode: google.maps.TravelMode.DRIVING,
            optimizeWaypoints: false
        }, (response, status) => {
            if (status === 'OK') {
                stopsMiniDirectionsRenderer.setDirections(response);
            } else {
                console.error("Mini rota çizilemedi:", status);
            }
        });
    }

    // Alt kısımda durak isimlerini göster
    const stopNames = stops.map(s => s.Name);
    document.getElementById("viewStopsModalBody").innerHTML = `<strong>Sıra:</strong> ${stopNames.join(" → ")}`;
}

function showStopsMiniMapModal(stops) {
    if (!stops || stops.length === 0) return;

    // Önceki haritayı temizle
    if (stopsMiniMap) {
        stopsMiniMarkers.forEach(m => m.setMap(null));
        stopsMiniMarkers = [];
        if (stopsMiniDirectionsRenderer) stopsMiniDirectionsRenderer.setMap(null);
    }

    const validStops = stops
        .map(s => {
            const lat = parseFloat(s.lat ?? s.Lat ?? s.Latitude);
            const lng = parseFloat(s.lng ?? s.Lng ?? s.Longitude);
            if (isNaN(lat) || isNaN(lng)) return null;
            return { ...s, Lat: lat, Lng: lng };
        })
        .filter(s => s !== null);

    if (!validStops.length) return;

    stopsMiniMap = new google.maps.Map(document.getElementById("stopsMiniMap"), {
        center: { lat: validStops[0].Lat, lng: validStops[0].Lng },
        zoom: 12,
        disableDefaultUI: true,
        draggable: false,
        scrollwheel: false,
        keyboardShortcuts: false,
        gestureHandling: "none",   // Dokunmayı engeller
        streetViewControl: false
    });

    stopsMiniDirectionsService = new google.maps.DirectionsService();
    stopsMiniDirectionsRenderer = new google.maps.DirectionsRenderer({
        map: stopsMiniMap,
        suppressMarkers: false,
        draggable: false,
        preserveViewport: true
    });

    const bounds = new google.maps.LatLngBounds();

    validStops.forEach(stop => {
        const position = { lat: stop.Lat, lng: stop.Lng };
        const marker = new google.maps.Marker({
            position,
            map: stopsMiniMap,
            title: stop.Name
        });
        stopsMiniMarkers.push(marker);
        bounds.extend(position);
    });

    stopsMiniMap.fitBounds(bounds);

    if (stopsMiniMarkers.length >= 2) {
        const origin = stopsMiniMarkers[0].getPosition();
        const destination = stopsMiniMarkers[stopsMiniMarkers.length - 1].getPosition();
        const waypoints = stopsMiniMarkers.slice(1, -1).map(m => ({ location: m.getPosition(), stopover: true }));

        stopsMiniDirectionsService.route({
            origin,
            destination,
            waypoints,
            travelMode: google.maps.TravelMode.DRIVING,
            optimizeWaypoints: false
        }, (response, status) => {
            if (status === 'OK') {
                stopsMiniDirectionsRenderer.setDirections(response);
            } else {
                console.error("Mini rota çizilemedi:", status);
            }
        });
    }
}

// Modal buton tıklama
$(document).on('click', '.view-stops', function () {
    const stops = $(this).data('stops'); // JSON array veya string
    const parsedStops = typeof stops === 'string' ? JSON.parse(stops) : stops;

    $('#viewStopsModal').modal('show');

    // Modal için sadece görüntüleme modunda harita
    showStopsMiniMapModal(parsedStops);
});
