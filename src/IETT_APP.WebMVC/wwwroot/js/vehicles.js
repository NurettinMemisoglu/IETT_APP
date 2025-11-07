$(function () {

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
            GarageId: $form.find('[name="GarageId"]').val(),
            ServiceStatus: parseInt($form.find('[name="ServiceStatus"]').val()) || 0,
            Operator: parseInt($form.find('[name="Operator"]').val()) || 0,
            Model: parseInt($form.find('[name="Model"]').val()) || 0,
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

        $.ajax({
            url: isEdit ? '/Planner/Vehicles/Edit' : '/Planner/Vehicles/Create',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (response) {
                // Başarılıysa listeye dön
                window.location.href = '/Planner/Vehicles';
            },
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
        const id = $row.data('id');
        const isActive = $btn.data('active') === true || $btn.data('active') === 'true';
        const newActive = !isActive;

        if (!confirm(newActive ? "Aracı aktif etmek istiyor musunuz?" : "Aracı pasif yapmak istiyor musunuz?"))
            return;

        const payload = {
            Id: id,
            IsActive: newActive
        };

        $.ajax({
            url: '/Planner/Vehicles/ToggleActive',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function () {
                refreshVehiclesTable();
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
                alert('Silme işlemi başarısız.');
            }
        });
    });


    // === TABLOYU YENİLE ===
    function refreshVehiclesTable(term = '') {
        $.get('/Planner/Vehicles/Search', { term }, function (data) {
            $('#vehiclesTableContainer').html(data);
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

});
