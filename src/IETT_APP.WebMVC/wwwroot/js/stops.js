document.addEventListener("DOMContentLoaded", function () {
    // ====== District Dropdown ======
    const districtInput = document.getElementById("DistrictInput");
    const districtDataScript = document.getElementById("districtData");

    let districts = [];
    let districtsLower = [];

    if (districtInput && districtDataScript) {
        districts = JSON.parse(districtDataScript.textContent);
        districtsLower = districts.map(d => d.toLowerCase());

        const dropdown = document.createElement("div");
        dropdown.classList.add("dropdown-menu", "w-100", "position-absolute");
        dropdown.style.maxHeight = "200px";
        dropdown.style.overflowY = "auto";
        dropdown.style.zIndex = 1000;
        dropdown.style.display = "none";
        districtInput.parentNode.appendChild(dropdown);

        function updateDropdown() {
            const val = districtInput.value.trim().toLowerCase();
            dropdown.innerHTML = "";
            if (!val) {
                dropdown.style.display = "none";
                return;
            }

            const filtered = districts.filter(d => d.toLowerCase().includes(val));
            filtered.forEach(district => {
                const item = document.createElement("div");
                item.classList.add("dropdown-item");
                item.textContent = district;

                item.addEventListener("mousedown", function (e) {
                    e.preventDefault();
                    districtInput.value = district;
                    dropdown.style.display = "none";
                });

                dropdown.appendChild(item);
            });

            dropdown.style.display = filtered.length ? "block" : "none";
        }

        districtInput.addEventListener("input", updateDropdown);
        districtInput.addEventListener("focus", updateDropdown);
        districtInput.addEventListener("blur", function () {
            setTimeout(() => {
                if (!districtsLower.includes(districtInput.value.trim().toLowerCase())) {
                    districtInput.value = "";
                }
                dropdown.style.display = "none";
            }, 150);
        });

        // Sayfa yüklendiğinde input dolu ise doğru formatta göster
        if (districtsLower.includes(districtInput.value.trim().toLowerCase())) {
            districtInput.value = districts.find(d => d.toLowerCase() === districtInput.value.trim().toLowerCase());
        }
    }

    // ====== Map & Marker ======
    let map, marker;
    const latInput = document.querySelector('input[name="Location.Latitude"]');
    const lngInput = document.querySelector('input[name="Location.Longitude"]');
    const nameInput = document.querySelector('input[name="Name"]');

    function setNameAndDistrictFromPlace(place) {
        if (!place) return;

        // Name
        if (place.name) {
            nameInput.value = place.name;
        }

        // District
        if (place.address_components && districtInput) {
            const districtComp = place.address_components.find(c =>
                c.types.includes("sublocality") || c.types.includes("administrative_area_level_2")
            );
            if (districtComp) {
                const districtName = districtComp.long_name;
                if (districtsLower.includes(districtName.toLowerCase())) {
                    districtInput.value = districts.find(d => d.toLowerCase() === districtName.toLowerCase());
                } else {
                    districtInput.value = "";
                }
            }
        }
    }

    function geocodePosition(pos) {
        const geocoder = new google.maps.Geocoder();
        geocoder.geocode({ location: pos }, (results, status) => {
            if (status === 'OK' && results[0]) {
                const resultPlace = results[0];
                if (resultPlace.place_id) {
                    const service = new google.maps.places.PlacesService(map);
                    service.getDetails({ placeId: resultPlace.place_id }, (place, status2) => {
                        if (status2 === google.maps.places.PlacesServiceStatus.OK) {
                            setNameAndDistrictFromPlace(place);
                        }
                    });
                } else {
                    // fallback
                    const parts = resultPlace.formatted_address.split(',');
                    if (parts.length >= 2 && districtInput) {
                        const districtGuess = parts[parts.length - 2].trim();
                        if (districtsLower.includes(districtGuess.toLowerCase())) {
                            districtInput.value = districts.find(d => d.toLowerCase() === districtGuess.toLowerCase());
                        } else {
                            districtInput.value = "";
                        }
                    }
                    nameInput.value = parts[0].trim();
                }
            }
        });
    }

    function initMap() {
        const initialLat = parseFloat(latInput.value) || 41.015137;
        const initialLng = parseFloat(lngInput.value) || 28.979530;
        const initialPos = { lat: initialLat, lng: initialLng };

        map = new google.maps.Map(document.getElementById("map"), {
            center: initialPos,
            zoom: 13,
        });

        marker = new google.maps.Marker({
            position: initialPos,
            map: map,
            draggable: true
        });

        marker.addListener("dragend", () => {
            const pos = marker.getPosition();
            latInput.value = pos.lat().toFixed(6);
            lngInput.value = pos.lng().toFixed(6);
            geocodePosition(pos);
        });

        map.addListener("click", (e) => {
            marker.setPosition(e.latLng);
            latInput.value = e.latLng.lat().toFixed(6);
            lngInput.value = e.latLng.lng().toFixed(6);
            geocodePosition(e.latLng);
        });

        [latInput, lngInput].forEach(input => {
            input.addEventListener("blur", () => {
                const lat = parseFloat(latInput.value.replace(",", "."));
                const lng = parseFloat(lngInput.value.replace(",", "."));
                if (!isNaN(lat) && !isNaN(lng)) {
                    latInput.value = lat.toFixed(6);
                    lngInput.value = lng.toFixed(6);
                    marker.setPosition({ lat, lng });
                    map.panTo({ lat, lng });
                    geocodePosition({ lat, lng });
                }
            });
        });

        // Google Places Autocomplete
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

            setNameAndDistrictFromPlace(place);
        });
    }

    if (document.getElementById("map")) initMap();

    // ====== Name & Code Field Processing ======
    const codeInput = document.querySelector('input[name="Code"]');
    const form = nameInput.closest("form");

    codeInput.addEventListener("input", () => {
        codeInput.value = codeInput.value.replace(/[^0-9]/g, "");
    });

    form.addEventListener("submit", () => {
        if (nameInput.value) {
            nameInput.value = nameInput.value
                .split(" ")
                .map(word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
                .join(" ");
        }
    });

    form.addEventListener("keydown", function (e) {
        if (e.key === "Enter" && e.target.tagName.toLowerCase() === "input") {
            e.preventDefault();
        }
    });
});
