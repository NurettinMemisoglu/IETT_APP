let stopsMap, stopsMainMarker;
let stopsMapInitialized = false;

function initStopsMap() {
    const latInput = document.querySelector('input[name="Location.Latitude"]');
    const lngInput = document.querySelector('input[name="Location.Longitude"]');
    const nameInput = document.querySelector('input[name="Name"]');
    const districtInput = document.getElementById("DistrictInput");
    const mapElement = document.getElementById("map");
    if (!mapElement) return;

    const districts = Array.from(document.querySelectorAll('#districtData')).map(e => e.textContent) || [];
    const districtsLower = districts.map(d => d.toLowerCase());

    const initialLat = parseFloat(latInput.value) || 41.015137;
    const initialLng = parseFloat(lngInput.value) || 28.979530;

    stopsMap = new google.maps.Map(mapElement, {
        center: { lat: initialLat, lng: initialLng },
        zoom: 13
    });

    stopsMainMarker = new google.maps.Marker({
        position: { lat: initialLat, lng: initialLng },
        map: stopsMap,
        draggable: true
    });

    function setNameAndDistrictFromPlace(place) {
        if (!place) return;
        if (place.name) nameInput.value = place.name;
        if (place.address_components && districtInput) {
            const districtComp = place.address_components.find(c =>
                c.types.includes("sublocality_level_1") ||
                c.types.includes("sublocality") ||
                c.types.includes("administrative_area_level_2")
            );
            if (districtComp) {
                const districtName = districtComp.long_name.trim();
                const match = districts.find(d => d.toLowerCase() === districtName.toLowerCase());
                districtInput.value = match || districtName;
            } else {
                districtInput.value = "";
            }
        }
    }

    function geocodePosition(pos) {
        const geocoder = new google.maps.Geocoder();
        geocoder.geocode({ location: pos }, (results, status) => {
            if (status === 'OK' && results[0]) {
                const resultPlace = results[0];
                if (resultPlace.place_id) {
                    const service = new google.maps.places.PlacesService(stopsMap);
                    service.getDetails({ placeId: resultPlace.place_id }, (place, status2) => {
                        if (status2 === google.maps.places.PlacesServiceStatus.OK) {
                            setNameAndDistrictFromPlace(place);
                        } else {
                            fallback(resultPlace);
                        }
                    });
                } else {
                    fallback(resultPlace);
                }
            }
            latInput.value = pos.lat().toFixed(6);
            lngInput.value = pos.lng().toFixed(6);
        });
    }

    function fallback(resultPlace) {
        if (!resultPlace) return;
        const parts = resultPlace.formatted_address.split(',');
        if (districtInput && parts.length >= 2) {
            const districtGuess = parts[parts.length - 2].trim();
            const match = districts.find(d => d.toLowerCase() === districtGuess.toLowerCase());
            districtInput.value = match || districtGuess;
        }
        if (nameInput) nameInput.value = parts[0].trim();
    }

    stopsMainMarker.addListener("dragend", () => {
        geocodePosition(stopsMainMarker.getPosition());
    });

    stopsMap.addListener("click", (e) => {
        stopsMainMarker.setPosition(e.latLng);
        geocodePosition(e.latLng);
    });

    document.addEventListener("stopsUpdated", e => updateStopsMarkers(e.detail));

    function updateStopsMarkers(stops) {
        stops.forEach(stop => {
            const marker = new google.maps.Marker({
                position: { lat: parseFloat(stop.Latitude), lng: parseFloat(stop.Longitude) },
                map: stopsMap,
                title: stop.Name
            });
            const info = new google.maps.InfoWindow({
                content: `<strong>${stop.Name}</strong><br/>Lat: ${stop.Latitude}, Lng: ${stop.Longitude}`
            });
            marker.addListener("click", () => {
                stopsMainMarker.setPosition(marker.getPosition());
                stopsMap.panTo(marker.getPosition());
                if (nameInput) nameInput.value = stop.Name;
                latInput.value = parseFloat(stop.Latitude).toFixed(6);
                lngInput.value = parseFloat(stop.Longitude).toFixed(6);
            });
        });
    }

    // Google Places Autocomplete
    const searchInput = document.createElement("input");
    searchInput.type = "text";
    searchInput.placeholder = "Adres veya durak adı girin";
    searchInput.className = "map-search-input";
    stopsMap.controls[google.maps.ControlPosition.TOP_LEFT].push(searchInput);

    const autocomplete = new google.maps.places.Autocomplete(searchInput);
    autocomplete.bindTo('bounds', stopsMap);

    autocomplete.addListener('place_changed', () => {
        const place = autocomplete.getPlace();
        if (!place.geometry) return;

        stopsMap.panTo(place.geometry.location);
        stopsMap.setZoom(17);
        stopsMainMarker.setPosition(place.geometry.location);

        latInput.value = place.geometry.location.lat().toFixed(6);
        lngInput.value = place.geometry.location.lng().toFixed(6);

        setNameAndDistrictFromPlace(place);
    });

}


// Polling ile map elementini bekle
const mapInterval = setInterval(() => {
    const mapElement = document.getElementById("map");
    if (mapElement && !stopsMap) {
        initStopsMap();
        clearInterval(mapInterval);
    }
}, 200);

// Sayfa yüklendiğinde de dene
document.addEventListener("DOMContentLoaded", () => {
    const mapElement = document.getElementById("map");
    if (mapElement && !stopsMap) initStopsMap();
});
