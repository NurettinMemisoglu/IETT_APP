$(function () {

    // === GARAGES YÜKLE ===
    function loadGarages(selectedId, callback) {
        $.get('/Planner/Garages/GetAll', function (garages) {
            const $garageSelect = $('#GarageId');

            $garageSelect.empty();
            $garageSelect.append($('<option>', {
                value: '',
                text: '-- Garaj Seçin --',
                disabled: true
            }));

            garages.forEach(g => {
                const id = g.id || g.Id;
                const name = g.garageName || g.GarageName;
                if (!g.isDeleted && id && name) {
                    const option = $('<option>', {
                        value: id,
                        text: name
                    });
                    $garageSelect.append(option);
                }
            });

            if (selectedId) $garageSelect.val(selectedId);
            if (callback) setTimeout(callback, 200);
        }).fail(function () {
            alert('Garajlar yüklenemedi.');
        });
    }

    // === SAYFA YÜKLENDİĞİNDE DROPDOWN VE CHECKBOXLARI SET ET ===
    $(document).ready(function () {
        const selectedGarageId = $('#GarageId').data('selected');
        loadGarages(selectedGarageId);

        // Enum dropdownlarını edit açıldığında set et
        const serviceStatus = $('#ServiceStatus').data('selected');
        if (serviceStatus != null) $('#ServiceStatus').val(serviceStatus);

        const operator = $('#Operator').data('selected');
        if (operator != null) $('#Operator').val(operator);

        const model = $('#Model').data('selected');
        if (model != null) $('#Model').val(model);

        // Checkbox’ları modelden set et
        $('#HasDisabilityAccess').prop('checked', toBool($('#HasDisabilityAccess').data('checked')));
        $('#HasWiFi').prop('checked', toBool($('#HasWiFi').data('checked')));
        $('#HasBikeRack').prop('checked', toBool($('#HasBikeRack').data('checked')));
        $('#HasMobileCharging').prop('checked', toBool($('#HasMobileCharging').data('checked')));
        $('#HasPassengerInfoSystem').prop('checked', toBool($('#HasPassengerInfoSystem').data('checked')));
        $('#HasCCTV').prop('checked', toBool($('#HasCCTV').data('checked')));
        $('#IsActive').prop('checked', toBool($('#IsActive').data('checked')));
    });

    // === FORM SUBMIT ===
    $(document).on('submit', '#vehicleForm', function (e) {
        e.preventDefault();

        const $form = $(this);
        const id = $form.find('[name="Id"]').val();
        const isEdit = id && id !== "00000000-0000-0000-0000-000000000000";

        const payload = {
            Id: id,
            DoorNumber: $form.find('[name="DoorNumber"]').val(),
            PlateNumber: $form.find('[name="PlateNumber"]').val(),
            Capacity: parseInt($form.find('[name="Capacity"]').val()) || 0,
            GarageId: $form.find('[name="GarageId"]').val() || null,
            ServiceStatus: $form.find('[name="ServiceStatus"]').val() ? parseInt($form.find('[name="ServiceStatus"]').val()) : null,
            Operator: $form.find('[name="Operator"]').val() ? parseInt($form.find('[name="Operator"]').val()) : null,
            Model: $form.find('[name="Model"]').val() ? parseInt($form.find('[name="Model"]').val()) : null,
            Year: parseInt($form.find('[name="Year"]').val()) || 0,
            TotalKm: parseInt($form.find('[name="TotalKm"]').val()) || 0,
            HasDisabilityAccess: $form.find('[name="HasDisabilityAccess"]').is(':checked'),
            HasWiFi: $form.find('[name="HasWiFi"]').is(':checked'),
            HasBikeRack: $form.find('[name="HasBikeRack"]').is(':checked'),
            HasMobileCharging: $form.find('[name="HasMobileCharging"]').is(':checked'),
            HasPassengerInfoSystem: $form.find('[name="HasPassengerInfoSystem"]').is(':checked'),
            HasCCTV: $form.find('[name="HasCCTV"]').is(':checked'),
            IsActive: $form.find('[name="IsActive"]').is(':checked')
        };

        // Validation
        if (!payload.GarageId) { alert("Garaj seçimi zorunludur."); return; }
        if (payload.ServiceStatus == null) { alert("Servis durumu seçimi zorunludur."); return; }
        if (payload.Operator == null) { alert("Operatör seçimi zorunludur."); return; }
        if (payload.Model == null) { alert("Model seçimi zorunludur."); return; }

        $.ajax({
            url: isEdit ? '/Planner/Vehicles/Edit' : '/Planner/Vehicles/Create',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function () { window.location.href = '/Planner/Vehicles'; },
            error: function (xhr) {
                let msg = 'Kaydetme hatası.';
                if (xhr.responseJSON?.message) msg += '\n' + xhr.responseJSON.message;
                alert(msg);
            }
        });
    });


    // === AKTİF/PASİF TOGGLE ===
    $(document).on('click', '.toggle-vehicle-active', function (e) {
        e.preventDefault();
        e.stopPropagation();

        const $btn = $(this);
        const $row = $btn.closest('tr');
        if ($row.data('isDeleted') === true) return;

        var $badge = $btn.find('span');
        var currentActive = $btn.data('active') === true || $btn.data('active') === 'true';
        var newActive = !currentActive;

        var message = currentActive
            ? "Bu aracı pasif yapmak istediğinize emin misiniz?"
            : "Bu aracı aktif yapmak istediğinize emin misiniz?";
        if (!confirm(message)) return;

        // Küçük basma efekti
        $badge.css('transform', 'scale(0.95)');
        setTimeout(function () {
            $badge.css('transform', 'scale(1)');
        }, 100);

        const $features = $row.find('.features-popover'); // tek hücredeki ikon

        // Tablodan tüm required alanları alıyoruz, boolean değerler değişmeden kalıyor
        const payload = {
            Id: $row.data('id'),
            DoorNumber: $row.find('[data-field="doorNumber"]').text().trim() || '---',
            PlateNumber: $row.find('[data-field="plateNumber"]').text().trim() || '---',
            Capacity: parseInt($row.find('[data-field="capacity"]').text()) || 1,
            GarageId: $row.find('[data-field="garage"]').data('garageid') || $row.find('[data-field="garage"]').text().trim(),
            ServiceStatus: parseInt($row.find('[data-field="serviceStatus"]').data('value')) || 0,
            Operator: parseInt($row.find('[data-field="operator"]').data('value')) || 0,
            Model: parseInt($row.find('[data-field="model"]').data('value')) || 0,
            Year: parseInt($row.find('[data-field="year"]').text()) || 2000,
            TotalKm: parseInt($row.find('[data-field="totalKm"]').text()) || 0,
            HasDisabilityAccess: $features.data('hasdisabilityaccess') === true || $features.data('hasdisabilityaccess') === 'true',
            HasWiFi: $features.data('haswifi') === true || $features.data('haswifi') === 'true',
            HasBikeRack: $features.data('hasbikerack') === true || $features.data('hasbikerack') === 'true',
            HasMobileCharging: $features.data('hasmobilecharging') === true || $features.data('hasmobilecharging') === 'true',
            HasPassengerInfoSystem: $features.data('haspassengerinfosystem') === true || $features.data('haspassengerinfosystem') === 'true',
            HasCCTV: $features.data('hascctv') === true || $features.data('hascctv') === 'true',
            IsActive: newActive
        };

        $.ajax({
            url: '/Planner/Vehicles/Edit',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function () {
                // Badge anlık değişiyor, tablo yenilemeye gerek yok
                if (newActive) {
                    $badge.removeClass('bg-secondary').addClass('bg-success').text('Aktif');
                } else {
                    $badge.removeClass('bg-success').addClass('bg-secondary').text('Pasif');
                }
                $btn.data('active', newActive.toString());
            },
            error: function (xhr) {
                alert('Durum güncellenemedi: ' + (xhr.responseText || ''));
            }
        });
    });


    // === SİL ===
    $(document).on('click', '.delete-vehicle', function () {
        const id = $(this).data('id');
        if (!confirm('Bu aracı silmek istediğinize emin misiniz?')) return;
        $.ajax({
            url: '/Planner/Vehicles/Delete/' + id,
            type: 'POST',
            success: function () {
                refreshVehiclesTable();
            },  
            error: function () {
                alert('Silme işlemi başarısız.') 
            }
        });
    });

    // === TABLOYU YENİLE ===
    function refreshVehiclesTable(term = '') {
        $.get('/Planner/Vehicles/Search', { term }, function (data) {
            $('#vehiclesTableContainer').html(data);

            // 1️⃣ Özellikler popover'larını tekrar başlat
            initFeaturePopovers();

            // 2️⃣ Garaj hücrelerini kısalt ve ... ekle
            $('#vehiclesTableContainer td[data-field="garage"]')
                .addClass('vehicle-garage-cell');

            // 3️⃣ İleride başka CSS veya JS işlemleri de eklenebilir
            // Örneğin tooltip, badge vs.
        }).fail(function () {
            alert('Tablo yenilenemedi.');
        });
    }

    // === ARAMA ===
    $('#searchInput').on('input', function () {
        const term = $(this).val();
        refreshVehiclesTable(term);
    });

    // === İPTAL BUTONU ===
    $(document).on('click', '#cancelVehicleBtn', function () {
        window.location.href = '/Planner/Vehicles';
    });


    // === Boolean dönüşümü helper ===
    function toBool(value) {
        return value === true || value === 'true' || value === 'True';
    }

    // === Tıklanabilir özellikler popover (ikon bazlı) ===
    function initFeaturePopovers() {
        $('.features-popover').each(function () {
            const $icon = $(this); // artık button değil, direkt ikon

            // Satır oluşturucu: label solda, ikon sağda
            const makeRow = (label, isTrue) => `
            <li class="feature-row" style="display:flex; justify-content:space-between; align-items:center; min-width:160px; padding:2px 0;">
                <span class="feature-label" style="font-size:14px;">${label}</span>
                <i class="fas ${isTrue ? 'fa-check text-success' : 'fa-times text-danger'} feature-icon" style="font-size:16px;"></i>
            </li>
        `;

            const content = `
            <div style="position:relative; min-width:180px; padding:4px 8px;">
                <button type="button" class="close-popover btn btn-xs btn-danger"
                        style="position:absolute; top:-8px; right:-8px;">&times;</button>
                <ul class="list-unstyled mb-0">
                    ${makeRow('WiFi', toBool($icon.data('haswifi')))}
                    ${makeRow('Bisiklet', toBool($icon.data('hasbikerack')))}
                    ${makeRow('Şarj', toBool($icon.data('hasmobilecharging')))}
                    ${makeRow('BIS', toBool($icon.data('haspassengerinfosystem')))}
                    ${makeRow('Kamera', toBool($icon.data('hascctv')))}
                    ${makeRow('Engelli', toBool($icon.data('hasdisabilityaccess')))}
                </ul>
            </div>
        `;

            $icon.popover({
                html: true,
                content: content,
                placement: 'top',
                trigger: 'focus',
                container: $icon.closest('td'), // sadece hücre içinde
            });
        });

        // Çarpıya tıklayınca popover kapatma
        $(document).off('click', '.close-popover').on('click', '.close-popover', function () {
            const $popoverEl = $(this).closest('.popover');
            if ($popoverEl.length) {
                const popoverIcon = $popoverEl.prev('.features-popover');
                popoverIcon.popover('hide');
            }
        });
    }

    // Sayfa yüklendiğinde popoverları başlat
    $(document).ready(function () {
        initFeaturePopovers();
    });

});
