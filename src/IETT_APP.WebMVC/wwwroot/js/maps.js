let map;
let marker;

function initMap() {
    const latInput = document.querySelector('input[name="Location.Latitude"]');
    const lngInput = document.querySelector('input[name="Location.Longitude"]');
    const nameInput = document.querySelector('input[name="Name"]');

    const initialLat = parseFloat(latInput.value) || 41.015137;
    const initialLng = parseFloat(lngInput.value) || 28.979530;
    const initialPos = { lat: initialLat, lng: initialLng };

    // 🌍 Harita oluştur
    map = new google.maps.Map(document.getElementById("map"), {
        center: initialPos,
        zoom: 13,
    });

    // 📍 Marker oluştur
    marker = new google.maps.Marker({
        position: initialPos,
        map: map,
        draggable: true
    });

    function setNameFromAddress(address) {
        if (!address) return;
        const firstPart = address.split(',')[0].trim();
        nameInput.value = firstPart;
    }

    // Marker sürüklendiğinde inputları güncelle
    marker.addListener("dragend", () => {
        const pos = marker.getPosition();
        latInput.value = marker.getPosition().lat().toFixed(6);
        lngInput.value = marker.getPosition().lng().toFixed(6);
        const geocoder = new google.maps.Geocoder();
        geocoder.geocode({ location: pos }, (results, status) => {
            if (status === 'OK' && results[0]) {
                setNameFromAddress(results[0].formatted_address);
            }
        });
    });

    // Haritaya tıklayınca marker yerleştir
    map.addListener("click", (e) => {
        marker.setPosition(e.latLng);
        latInput.value = e.latLng.lat().toFixed(6);
        lngInput.value = e.latLng.lng().toFixed(6);

        const geocoder = new google.maps.Geocoder();
        geocoder.geocode({ location: e.latLng }, (results, status) => {
            if (status === 'OK' && results[0]) {
                setNameFromAddress(results[0].formatted_address);
            }
        });
    });

    // Manuel inputtan enlem/boylam girildiğinde marker'ı güncelle
    [latInput, lngInput].forEach(input => {
        input.addEventListener("blur", () => {
            const lat = parseFloat(latInput.value.replace(",", "."));
            const lng = parseFloat(lngInput.value.replace(",", "."));
            if (!isNaN(lat) && !isNaN(lng)) {
                latInput.value = lat.toFixed(6);
                lngInput.value = lng.toFixed(6);
                marker.setPosition({ lat, lng });
                map.panTo({ lat, lng });

                const geocoder = new google.maps.Geocoder();
                geocoder.geocode({ location: { lat, lng } }, (results, status) => {
                    if (status === 'OK' && results[0]) {
                        setNameFromAddress(results[0].formatted_address);
                    }
                });
            }
        });
    });

    // Enter tuşuna basınca form submit olmamasını sağla
    document.querySelectorAll("form").forEach(form => {
        form.addEventListener("keydown", function (e) {
            if (e.key === "Enter") {
                const target = e.target;
                if (target.tagName.toLowerCase() === "input") {
                    e.preventDefault();
                }
            }
        });
    });

    // 📦 Google Places Autocomplete
    const searchInput = document.createElement("input");
    searchInput.type = "text";
    searchInput.placeholder = "Adres veya durak adı girin";
    searchInput.className = "map-search-input";
    map.controls[google.maps.ControlPosition.TOP_LEFT].push(searchInput);

    const autocomplete = new google.maps.places.Autocomplete(searchInput);
    autocomplete.bindTo('bounds', map);

    autocomplete.addListener('place_changed', () => {
        const place = autocomplete.getPlace();
        if (!place.geometry) return;

        map.panTo(place.geometry.location);
        map.setZoom(17);
        marker.setPosition(place.geometry.location);

        latInput.value = place.geometry.location.lat().toFixed(6);
        lngInput.value = place.geometry.location.lng().toFixed(6);

        // İlk virgüle kadar olan kısmı Name input'una yaz
        setNameFromAddress(place.formatted_address || place.name);
    });
}

// Sayfa yüklendiğinde haritayı başlat
document.addEventListener("DOMContentLoaded", () => {
    if (document.getElementById("map")) {
        initMap();
    }
});

document.addEventListener("DOMContentLoaded", function () {
    const nameInput = document.querySelector('input[name="Name"]');
    const codeInput = document.querySelector('input[name="Code"]');
    const form = nameInput.closest("form");

    // Kod alanı sadece sayı kabul etsin
    codeInput.addEventListener("input", () => {
        codeInput.value = codeInput.value.replace(/[^0-9]/g, "");
    });

    // Form submit olmadan önce Name alanını title case yap
    form.addEventListener("submit", () => {
        if (nameInput.value) {
            nameInput.value = nameInput.value
                .split(" ")
                .map(word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
                .join(" ");
        }
    });
});