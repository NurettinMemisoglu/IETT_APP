$(function () {

    // === DROPDOWN YÜKLE ===
    function loadDropdowns(selectedValues = {}) {

        // === LINE ===
        $.get('/Planner/Lines/GetAll', function (lines) {
            const $lineSelect = $('#LineId');
            $lineSelect.empty()
                .append(new Option('-- Hat Seçin --', '', true, true)) // İlk açılışta seçilebilir
                .prop('disabled', false);

            lines.forEach(l => {
                if (!l.isDeleted) $lineSelect.append(new Option(l.name, l.id));
            });

            if (selectedValues.LineId) {
                $lineSelect.val(selectedValues.LineId);
                // Seçim varsa artık "-- Hat Seçin --" seçilemez
                $lineSelect.find('option').first().prop('disabled', true);
            }
        });

        // === ROUTE ===
        $.get('/Planner/Routes/GetAll', function (routes) {
            const $routeSelect = $('#RouteId');
            $routeSelect.empty()
                .append(new Option('-- Önce Hat Seçin --', '', true, true))
                .prop("disabled", true);

            routes.forEach(r => {
                if (!r.isDeleted) {
                    let opt = new Option(r.name, r.id);
                    $(opt).attr("data-lineid", r.lineId);
                    $routeSelect.append(opt);
                }
            });

            if (selectedValues.RouteId) {
                $routeSelect.val(selectedValues.RouteId);
                $routeSelect.prop("disabled", false);
            }
        });

        // === EVENT: Hat değişince Route filtrele ===
        $(document).on("change", "#LineId", function () {
            const selectedLineId = $(this).val();
            const $routeSelect = $('#RouteId');

            // Hat seçilmemişse
            if (!selectedLineId) {
                $routeSelect.empty()
                    .append(new Option('-- Önce Hat Seçin --', '', true, true))
                    .prop("disabled", true);
                return;
            }

            // Hat seçildiyse
            $routeSelect.prop("disabled", false);

            // İlk option "-- Güzergah Seçin --" olsun ve seçilemez
            $routeSelect.find('option').first().text('-- Güzergah Seçin --').prop('disabled', true).prop('selected', true);

            // Filtre dışı optionları disable/enable yap
            $routeSelect.find('option').each(function () {
                const lineId = $(this).data("lineid");
                if (!lineId) return;
                $(this).prop("disabled", lineId !== selectedLineId);
            });

            // Hat seçildikten sonra "-- Hat Seçin --" artık seçilemez
            $(this).find('option').first().prop('disabled', true);
        });

        // === GARAGE ===
        $.get('/Planner/Garages/GetAll', function (garages) {
            const $garageSelect = $('#GarageId');
            $garageSelect.empty()
                .append(new Option('-- Garaj Seçin --', '', true, true)) // Açılışta seçilebilir
                .prop('disabled', false);

            garages.forEach(g => {
                if (!g.isDeleted) $garageSelect.append(new Option(g.garageName, g.id));
            });

            if (selectedValues.GarageId) {
                $garageSelect.val(selectedValues.GarageId);
                // Seçim varsa "-- Garaj Seçin --" seçilemez
                $garageSelect.find('option').first().prop('disabled', true);
            }
        });

        // === VEHICLE ===
        $.get('/Planner/Vehicles/GetAll', function (vehicles) {
            const $vehicleSelect = $('#VehicleId');
            $vehicleSelect.empty()
                .append(new Option('-- Önce Garaj Seçin --', '', true, true))
                .prop("disabled", true);

            vehicles.forEach(v => {
                if (!v.isDeleted) {
                    let opt = new Option(v.doorNumber + ' - ' + v.plateNumber, v.id);
                    $(opt).attr("data-garageid", v.garageId);
                    $vehicleSelect.append(opt);
                }
            });

            if (selectedValues.VehicleId) {
                $vehicleSelect.val(selectedValues.VehicleId);
                $vehicleSelect.prop("disabled", false);
            }
        });

        // === EVENT: Garaj değişince Vehicle filtrele ===
        $(document).on("change", "#GarageId", function () {
            const selectedGarageId = $(this).val();
            const $vehicleSelect = $('#VehicleId');

            if (!selectedGarageId) {
                $vehicleSelect.empty()
                    .append(new Option('-- Önce Garaj Seçin --', '', true, true))
                    .prop("disabled", true);
                return;
            }

            $vehicleSelect.prop("disabled", false);
            $vehicleSelect.find('option').first().text('-- Araç Seçin --').prop('disabled', true).prop('selected', true);

            $vehicleSelect.find('option').each(function () {
                const garageId = $(this).data("garageid");
                if (!garageId) return;
                $(this).prop("disabled", garageId !== selectedGarageId);
            });

            // Garaj seçildikten sonra "-- Garaj Seçin --" artık seçilemez
            $(this).find('option').first().prop('disabled', true);
        });
    }




    // === ROUTE FİLTRELEME ===
    function filterRoutes() {
        const selectedLineId = $('#LineId').val();
        const $routeSelect = $('#RouteId');

        if (!selectedLineId) {
            $routeSelect.prop("disabled", true);
            $routeSelect.val("");
            return;
        }

        $routeSelect.prop("disabled", false);

        $routeSelect.find('option').each(function () {
            const lineId = $(this).data("lineid");

            if (!lineId) return;

            if (lineId !== selectedLineId) {
                $(this).prop("disabled", true);
            } else {
                $(this).prop("disabled", false);
            }
        });
    }



    // === VEHICLE FİLTRELEME ===
    function filterVehicles() {
        const selectedGarageId = $('#GarageId').val();
        const $vehicleSelect = $('#VehicleId');

        if (!selectedGarageId) {
            $vehicleSelect.prop("disabled", true);
            $vehicleSelect.val("");
            return;
        }

        $vehicleSelect.prop("disabled", false);

        $vehicleSelect.find('option').each(function () {
            const garageId = $(this).data("garageid");

            if (!garageId) return;

            if (garageId !== selectedGarageId) {
                $(this).prop("disabled", true);
            } else {
                $(this).prop("disabled", false);
            }
        });
    }



    // === EVENT: Hat değişince Route filtrelensin ===
    $(document).on("change", "#LineId", function () {
        filterRoutes();
        $('#RouteId').val("");
    });


    // === EVENT: Garaj değişince Vehicle filtrelensin ===
    $(document).on("change", "#GarageId", function () {
        filterVehicles();
        $('#VehicleId').val("");
    });



    // === SAYFA YÜKLENDİ ===
    $(document).ready(function () {
        const selectedValues = {
            RouteId: $('#RouteId').data('selected'),
            LineId: $('#LineId').data('selected'),
            VehicleId: $('#VehicleId').data('selected'),
            OperatorId: $('#OperatorId').data('selected'),
            GarageId: $('#GarageId').data('selected')
        };
        loadDropdowns(selectedValues);
    });


    // === FORM SUBMIT ===
    $(document).on('submit', '#tripTaskForm', function (e) {
        e.preventDefault();

        const $form = $(this);
        const id = $form.find('[name="Id"]').val();
        const isEdit = id && id !== "00000000-0000-0000-0000-000000000000";

        const payload = {
            Id: id,
            Title: $form.find('[name="Title"]').val(),
            Description: $form.find('[name="Description"]').val(),
            Status: parseInt($form.find('[name="Status"]').val()),
            PassengerCount: parseInt($form.find('[name="PassengerCount"]').val()) || null,
            DelayInMinutes: parseInt($form.find('[name="DelayInMinutes"]').val()) || null,
            DelayOutMinutes: parseInt($form.find('[name="DelayOutMinutes"]').val()) || null,
            ScheduledDeparture: $form.find('[name="ScheduledDeparture"]').val() ? new Date($form.find('[name="ScheduledDeparture"]').val()).toISOString() : null,
            ScheduledArrival: $form.find('[name="ScheduledArrival"]').val() ? new Date($form.find('[name="ScheduledArrival"]').val()).toISOString() : null,
            RouteId: $form.find('[name="RouteId"]').val() || null,
            LineId: $form.find('[name="LineId"]').val() || null,
            VehicleId: $form.find('[name="VehicleId"]').val() || null,
            OperatorId: $form.find('[name="OperatorId"]').val() || null,
            GarageId: $form.find('[name="GarageId"]').val() || null
        };

        $.ajax({
            url: isEdit ? '/Chief/TripTasks/Edit' : '/Chief/TripTasks/Create',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function () {
                window.location.href = '/Chief/TripTasks';
            },
            error: function (xhr) {
                alert(xhr.responseJSON?.message || "Kaydetme hatası.");
            }
        });
    });


    // === ARAMA ===
    $('#searchInput').on('input', function () {
        refreshTripTasksTable($(this).val());
    });

    // === TABLO YENİLE ===
    function refreshTripTasksTable(term = '') {
        $.get('/Chief/TripTasks/Search', { term }, function (data) {
            $('#tripTasksTableContainer').html(data);
        });
    }

    // === İPTAL ===
    $(document).on('click', '#cancelTripTasksBtn', function () {
        window.location.href = '/Chief/TripTasks';
    });

    // === EDİT ===
    $(document).on('click', '.edit-trip-task', function () {
        window.location.href = '/Chief/TripTasks/Edit/' + $(this).data('id');
    });

    // === SİL ===
    $(document).on('click', '.delete-trip-task', function () {
        if (!confirm('Bu görevi silmek istiyor musunuz?')) return;
        $.post('/Chief/TripTasks/Delete/' + $(this).data('id'), function () {
            refreshTripTasksTable();
        });
    });

});
