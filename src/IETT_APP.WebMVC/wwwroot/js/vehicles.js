$(function () {

    // === 1. GARAJ LİSTESİNİ YÜKLE ===
    function loadGarages(selectedId) {
        if ($('#GarageId').length === 0) return;

        $.get('/Planner/Garages/GetAll', function (garages) {
            const $garageSelect = $('#GarageId');
            $garageSelect.empty();
            $garageSelect.append($('<option>', {
                value: '',
                text: '-- Garaj Seçin --',
                disabled: true,
                selected: true
            }));

            garages.forEach(g => {
                const id = g.id || g.Id;
                const name = g.garageName || g.GarageName;
                if (!g.isDeleted && id && name) {
                    $garageSelect.append($('<option>', {
                        value: id,
                        text: name
                    }));
                }
            });

            if (selectedId && selectedId !== '' && selectedId !== '00000000-0000-0000-0000-000000000000') {
                $garageSelect.val(selectedId);
            }
        }).fail(function () {
            console.error('Garajlar yüklenemedi.');
        });
    }

    // === GÜNCELLENEN FONKSİYON ===
    // Durum Nedeni Göster/Gizle Fonksiyonu
    function handleStatusReasonVisibility() {
        const statusVal = parseInt($('#ServiceStatus').val());
        const $reasonContainer = $('#statusReasonContainer');

        // Açıklama gerektirmeyen durumlar: 
        // 1 = Servise Hazır
        // 6 = Seferde / Yolda
        const NO_REASON_STATUS_IDS = [1, 6];

        // Eğer seçilen durum bu listede varsa GİZLE
        if (NO_REASON_STATUS_IDS.includes(statusVal)) {
            $reasonContainer.slideUp();
            // İçeriği silmiyoruz (.val('') YOK), böylece yanlışlıkla değişirse veri korunur.
        } else {
            $reasonContainer.slideDown();
        }
    }

    // === 2. SAYFA HAZIR OLDUĞUNDA ===
    $(document).ready(function () {
        const selectedGarageId = $('#GarageId').data('selected');
        if (selectedGarageId) loadGarages(selectedGarageId);

        ['ServiceStatus', 'Operator', 'Model'].forEach(id => {
            const val = $('#' + id).data('selected');
            if (val != null && val !== '') $('#' + id).val(val);
        });

        // Durum Nedeni Görünürlük Kontrolü
        if ($('#ServiceStatus').length > 0) {
            handleStatusReasonVisibility();

            $('#ServiceStatus').on('change', function () {
                handleStatusReasonVisibility();
            });
        }

        initFeaturePopovers();
    });

    // === 3. FORM SUBMIT (KAYDETME ANI) ===
    $(document).on('submit', '#vehicleForm', function (e) {
        e.preventDefault();

        const $form = $(this);
        const idVal = $form.find('[name="Id"]').val();
        const isEdit = idVal && idVal !== "00000000-0000-0000-0000-000000000000";

        const serviceStatusVal = parseInt($form.find('[name="ServiceStatus"]').val()) || 0;

        // Açıklama gerektirmeyen durumlar (1: Hazır, 6: Seferde)
        const NO_REASON_STATUS_IDS = [1, 6];

        // Form verilerini topla
        const payload = {
            Id: idVal,
            DoorNumber: $form.find('[name="DoorNumber"]').val(),
            PlateNumber: $form.find('[name="PlateNumber"]').val(),
            Capacity: parseInt($form.find('[name="Capacity"]').val()) || 0,
            GarageId: $form.find('[name="GarageId"]').val(),
            ServiceStatus: serviceStatusVal,

            // === GÜNCELLENEN KISIM ===
            // Eğer durum 1 veya 6 ise Backend'e NULL gönderiyoruz.
            StatusReason: (NO_REASON_STATUS_IDS.includes(serviceStatusVal)) ? null : $form.find('[name="StatusReason"]').val(),

            Operator: parseInt($form.find('[name="Operator"]').val()) || 0,
            Model: parseInt($form.find('[name="Model"]').val()) || 0,
            Year: parseInt($form.find('[name="Year"]').val()) || 2000,
            TotalKm: parseInt($form.find('[name="TotalKm"]').val()) || 0,
            HasDisabilityAccess: $form.find('[name="HasDisabilityAccess"]').is(':checked'),
            HasWiFi: $form.find('[name="HasWiFi"]').is(':checked'),
            HasBikeRack: $form.find('[name="HasBikeRack"]').is(':checked'),
            HasMobileCharging: $form.find('[name="HasMobileCharging"]').is(':checked'),
            HasPassengerInfoSystem: $form.find('[name="HasPassengerInfoSystem"]').is(':checked'),
            HasCCTV: $form.find('[name="HasCCTV"]').is(':checked'),
            IsActive: $form.find('[name="IsActive"]').is(':checked')
        };

        $.ajax({
            url: isEdit ? '/Planner/Vehicles/Edit' : '/Planner/Vehicles/Create',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function () {
                window.location.href = '/Planner/Vehicles';
            },
            error: function (xhr) {
                let msg = 'Kaydetme hatası.';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    msg += '\n' + xhr.responseJSON.message;
                    if (xhr.responseJSON.details) {
                        xhr.responseJSON.details.forEach(d => {
                            msg += `\n- ${d.errors ? d.errors.join(', ') : ''}`;
                        });
                    }
                }
                alert(msg);
            }
        });
    });

    // === 4. AKTİF/PASİF TOGGLE ===
    $(document).on('click', '.toggle-vehicle-active', function (e) {
        e.preventDefault();
        e.stopImmediatePropagation();

        const $btn = $(this);
        const id = $btn.data('id');
        const currentActive = $btn.data('active') === true || $btn.data('active') === 'true' || $btn.data('active') === 'True';
        const newActive = !currentActive;

        const confirmMsg = currentActive
            ? "Bu aracı PASİF duruma getirmek istiyor musunuz?"
            : "Bu aracı AKTİF duruma getirmek istiyor musunuz?";

        if (!confirm(confirmMsg)) return;

        $.ajax({
            url: '/Planner/Vehicles/GetVehicleJson/' + id,
            type: 'GET',
            success: function (vehicleData) {
                vehicleData.isActive = newActive;
                $.ajax({
                    url: '/Planner/Vehicles/Edit',
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify(vehicleData),
                    success: function () {
                        refreshVehiclesTable($('#searchInput').val());
                    },
                    error: function (xhr) {
                        let err = "Durum güncellenemedi.";
                        if (xhr.responseJSON && xhr.responseJSON.message) err += "\n" + xhr.responseJSON.message;
                        alert(err);
                    }
                });
            },
            error: function () {
                alert("Araç verisi çekilemedi.");
            }
        });
    });

    // === 5. TABLO YENİLEME & ARAMA ===
    let searchTimeout;
    $('#searchInput').on('input', function () {
        const term = $(this).val();
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function () {
            refreshVehiclesTable(term);
        }, 500);
    });

    function refreshVehiclesTable(term = '') {
        $.get('/Planner/Vehicles/Search', { term: term }, function (data) {
            $('#vehiclesTableContainer').html(data);
            initFeaturePopovers();
        }).fail(function () {
            console.error('Tablo yenilenemedi.');
        });
    }

    // === 6. SİLME İŞLEMİ ===
    $(document).on('click', '.delete-vehicle', function () {
        const id = $(this).data('id');
        if (!confirm('Bu aracı silmek istediğinize emin misiniz?')) return;

        $.ajax({
            url: '/Planner/Vehicles/Delete/' + id,
            type: 'POST',
            success: function () {
                refreshVehiclesTable($('#searchInput').val());
            },
            error: function (xhr) {
                alert('Silme işlemi başarısız: ' + (xhr.responseText || ''));
            }
        });
    });

    // === YARDIMCI FONKSİYONLAR ===
    function toBool(value) {
        return value === true || value === 'true' || value === 'True' || value === 1 || value === '1';
    }

    function initFeaturePopovers() {
        $('.features-popover').popover('dispose');
        $('.features-popover').each(function () {
            const $icon = $(this);
            const makeRow = (label, isTrue) => `
                <li class="feature-row" style="display:flex; justify-content:space-between; align-items:center; min-width:180px; padding:3px 0; border-bottom:1px solid #eee;">
                    <span class="feature-label text-muted" style="font-size:0.9rem;">${label}</span>
                    <i class="fas ${isTrue ? 'fa-check-circle text-success' : 'fa-times-circle text-secondary'} feature-icon" style="font-size:1rem;"></i>
                </li>
            `;
            const content = `
                <div style="position:relative; min-width:200px; padding:5px;">
                    <button type="button" class="close-popover btn btn-sm btn-light text-danger position-absolute top-0 end-0 p-0 px-2" style="font-size:1.2rem; line-height:1;">&times;</button>
                    <h6 class="fw-bold mb-2 text-primary" style="font-size:0.85rem; border-bottom:2px solid #f0f0f0; padding-bottom:5px;">Araç Özellikleri</h6>
                    <ul class="list-unstyled mb-0">
                        ${makeRow('WiFi Erişimi', toBool($icon.data('haswifi')))}
                        ${makeRow('Kamera (CCTV)', toBool($icon.data('hascctv')))}
                        ${makeRow('Engelli Erişimi', toBool($icon.data('hasdisabilityaccess')))}
                        ${makeRow('USB Şarj', toBool($icon.data('hasmobilecharging')))}
                        ${makeRow('Bisiklet Aparatı', toBool($icon.data('hasbikerack')))}
                        ${makeRow('Bilgilendirme Ekranı', toBool($icon.data('haspassengerinfosystem')))}
                    </ul>
                </div>
            `;
            $icon.popover({
                html: true,
                content: content,
                placement: 'left',
                trigger: 'manual',
                container: 'body',
                sanitize: false
            });
        });

        $(document).off('click', '.features-popover').on('click', '.features-popover', function (e) {
            e.stopPropagation();
            $('.features-popover').not(this).popover('hide');
            $(this).popover('toggle');
        });

        $(document).off('click', '.close-popover').on('click', '.close-popover', function () {
            $('.features-popover').popover('hide');
        });

        $(document).on('click', function (e) {
            if (!$(e.target).closest('.popover').length && !$(e.target).hasClass('features-popover')) {
                $('.features-popover').popover('hide');
            }
        });
    }
});