document.addEventListener("DOMContentLoaded", function () {
    const input = document.getElementById("DistrictInput");
    const districtDataScript = document.getElementById("districtData");

    if (!input || !districtDataScript) return;

    const districts = JSON.parse(districtDataScript.textContent);
    const districtsLower = districts.map(d => d.toLowerCase());

    // Dropdown oluştur
    const dropdown = document.createElement("div");
    dropdown.classList.add("dropdown-menu", "w-100", "position-absolute");
    dropdown.style.maxHeight = "200px";
    dropdown.style.overflowY = "auto";
    dropdown.style.zIndex = 1000;
    dropdown.style.display = "none";
    input.parentNode.appendChild(dropdown);

    function updateDropdown() {
        const val = input.value.trim().toLowerCase();
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
                e.preventDefault(); // Blur olayını engelle
                input.value = district;
                dropdown.style.display = "none";
            });

            dropdown.appendChild(item);
        });

        dropdown.style.display = filtered.length ? "block" : "none";
    }



    input.addEventListener("input", updateDropdown);
    input.addEventListener("focus", updateDropdown);

    input.addEventListener("blur", function () {
        setTimeout(() => {
            // Girilen değer veri setinde yoksa temizle
            if (!districtsLower.includes(input.value.trim().toLowerCase())) {
                input.value = "";
            }
            dropdown.style.display = "none";
        }, 150);
    });


    // Sayfa yüklendiğinde input dolu ise dropdown'ı buna göre ayarla
    if (districtsLower.includes(input.value.trim().toLowerCase())) {
        input.value = districts.find(d => d.toLowerCase() === input.value.trim().toLowerCase());
    }
});
