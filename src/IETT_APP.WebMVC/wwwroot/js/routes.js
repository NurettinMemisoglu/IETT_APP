$(function () {

    let linesMap = {};
    let stopsMap = {};

    // === LINE VE STOPLARI YÜKLE, MAP OLUŞTUR ===
    function loadLinesAndStops(callback) {
        $.get('/Planner/Lines/GetAll', function (lines) {
            linesMap = {};
            const lineSelect = $('#LineId');
            lineSelect.empty().append('<option value="">-- Hat Seçin --</option>');
            lines.forEach(line => {
                if (!line.isDeleted) {
                    linesMap[line.id] = line.name;
                    lineSelect.append(new Option(`${line.code} - ${line.name}`, line.id));
                }
            });

            $.get('/Planner/Stops/GetAll', function (stops) {
                stopsMap = {};
                stops.forEach(stop => {
                    if (!stop.isDeleted) stopsMap[stop.id] = stop.name;
                });

                if (callback) callback();
            });
        });
    }

    // === HATLAR VE DURAKLAR YÜKLENDİKTEN SONRA SEÇİMLERİ UYGULA ===
    loadLinesAndStops(function () {
        const selectedLineId = $('#LineId').data('selected');
        const selectedStops = $('#selectedStopsList').data('selected') || [];

        // Hat seçimi
        $('#LineId').val(selectedLineId && selectedLineId !== '00000000-0000-0000-0000-000000000000' ? selectedLineId : '');

        // Durakları göster
        updateSelectedStopsList(selectedStops);
    });

    // === DURAK SEÇ MODALI ===
    $(document).on('click', '#selectStopsBtn', function () {
        const $modalBody = $('#stopsModalBody').empty();

        Object.entries(stopsMap).forEach(([id, name]) => {
            $modalBody.append(`
                <div class="form-check">
                    <input class="form-check-input stop-checkbox" type="checkbox" value="${id}" id="stop_${id}">
                    <label class="form-check-label" for="stop_${id}">${name}</label>
                </div>
            `);
        });

        const selectedStops = $('#selectedStopsList').data('selected') || [];
        selectedStops.forEach(id => $(`#stop_${id}`).prop('checked', true));

        new bootstrap.Modal(document.getElementById('stopsModal'), {
            backdrop: 'static',
            keyboard: false
        }).show();
    });

    // === SEÇİLEN DURAKLARI KAYDET ===
    $(document).on('click', '#saveSelectedStops', function () {
        const selected = $('.stop-checkbox:checked').map((_, el) => ({
            id: el.value,
            name: $(el).next('label').text()
        })).get();

        // Hidden inputları güncelle
        $('#StopIds').length || $('<input>').attr({ type: 'hidden', id: 'StopIds', name: 'StopIds' }).appendTo('#routeForm');
        $('#StopNames').length || $('<input>').attr({ type: 'hidden', id: 'StopNames', name: 'StopNames' }).appendTo('#routeForm');
        $('#StopIds').val(selected.map(x => x.id));
        $('#StopNames').val(selected.map(x => x.name));

        // Div içinde göster ve event tetikle
        updateSelectedStopsList(selected.map(x => x.id));

        // 🔹 Konsola yazdırma (stopsMap veya load edilen duraklar üzerinden)
        const stopsData = selected.map(x => ({
            ID: x.id,
            Name: x.name,
            Lat: $('#map').length ? parseFloat($('#map').data(`lat-${x.id}`)) : 'N/A',
            Lng: $('#map').length ? parseFloat($('#map').data(`lng-${x.id}`)) : 'N/A'
        }));
        console.table(stopsData);

        bootstrap.Modal.getInstance(document.getElementById('stopsModal')).hide();
    });

    // === DURAKLAR DIV GÜNCELLEME FONKSİYONU ===
    function updateSelectedStopsList(selectedIds) {
        if (selectedIds.length > 0) {
            const names = selectedIds.map(id => stopsMap[id]).filter(Boolean);
            $('#selectedStopsList').text(names.join(', ')).data('selected', selectedIds);

            // Harita event’i tetikleme
            const routeId = $('#routeForm [name="Id"]').val();
            const route = window.allRoutesData ? window.allRoutesData.find(r => r.Id === routeId) : null;
            if (route && route.Stops) {
                const selectedStopsData = route.Stops.filter(s => selectedIds.includes(s.Id));
                document.dispatchEvent(new CustomEvent('stopsUpdated', { detail: selectedStopsData }));
            }
        } else {
            $('#selectedStopsList').html('<em>Henüz durak seçilmedi</em>').data('selected', []);
            document.dispatchEvent(new CustomEvent('stopsUpdated', { detail: [] }));
        }
    }    

    // === ROUTE FORM SUBMIT ===
    $(document).on('submit', '#routeForm', function (e) {
        e.preventDefault();

        const payload = {
            Id: $(this).find('[name="Id"]').val(),
            Code: $(this).find('[name="Code"]').val(),
            Name: $(this).find('[name="Name"]').val(),
            LengthInM: parseInt($(this).find('[name="LengthInM"]').val() || 0),
            TimeInMinutes: parseInt($(this).find('[name="TimeInMinutes"]').val() || 0),
            RoutesDirection: parseInt($(this).find('[name="RoutesDirection"]').val()),
            LineId: $(this).find('[name="LineId"]').val(),
            StopIds: $(this).find('[name="StopIds"]').val() ? $(this).find('[name="StopIds"]').val().split(',') : [],
            StopNames: $(this).find('[name="StopNames"]').val() ? $(this).find('[name="StopNames"]').val().split(',') : [],
            IsActive: $(this).find('[name="IsActive"]').is(':checked')
        };

        $.ajax({
            url: '/Planner/Routes/Execute',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function () {
                window.location.href = '/Planner/Routes';
            },
            error: function (xhr) {
                alert('Kaydetme hatası: ' + (xhr.responseText || ''));
            }
        });
    });

    // === İptal butonu ===
    $(document).on('click', '#cancelRouteBtn', function () {
        window.location.href = '/Planner/Routes';
    });

    // === ROUTE EKLE ===
    $('#addRouteBtn').click(function () {
        window.location.href = '/Planner/Routes/Create';
    });

    // === ROUTE DÜZENLE ===
    $(document).on('click', '.edit-route', function () {
        const id = $(this).data('id');
        window.location.href = '/Planner/Routes/Edit/' + id;
    });

    // === ROUTE SİL ===
    $(document).on('click', '.delete-route', function () {
        const id = $(this).data('id');
        if (!confirm('Bu route silmek istediğinize emin misiniz?')) return;

        $.ajax({
            url: '/Planner/Routes/Delete/' + id,
            type: 'POST',
            success: function () { location.reload(); },
            error: function () { alert('Silme işlemi başarısız.'); }
        });
    });

    // === AKTİF / PASİF TOGGLE ===
    $(document).on('click', '.toggle-route-active', function (e) {
        e.preventDefault();  // form submit'ini engelle
        e.stopPropagation(); // başka event zincirlerini kes

        var $btn = $(this);
        var $row = $btn.closest('tr');
        if ($row.data('isDeleted') === true) return;

        var $badge = $btn.find('span');
        var currentActive = $btn.data('active') === true || $btn.data('active') === 'true';
        var newActive = !currentActive;

        var message = currentActive
            ? "Bu hattı pasif yapmak istediğinize emin misiniz?"
            : "Bu hattı aktif yapmak istediğinize emin misiniz?";
        if (!confirm(message)) return;

        // Küçük basma efekti
        $badge.css('transform', 'scale(0.95)');
        setTimeout(function () {
            $badge.css('transform', 'scale(1)');
        }, 100);

        // Satırdan değerleri al
        const stopsDataRaw = $row.find('.view-stops').data('stops');
        const stopIdsRaw = $row.find('.view-stops').data('stopids');

        let stopsData = [];
        let stopIds = [];

        // Eğer string ise split et
        if (typeof stopsDataRaw === "string") {
            stopsData = stopsDataRaw.split(',').map(s => s.trim());
        } else if (Array.isArray(stopsDataRaw)) {
            stopsData = stopsDataRaw;
        }

        if (typeof stopIdsRaw === "string") {
            stopIds = stopIdsRaw.split(',').map(s => s.trim());
        } else if (Array.isArray(stopIdsRaw)) {
            stopIds = stopIdsRaw;
        }

        const payload = {
            Id: $row.data('id'),
            Code: $row.find('.route-code').text(),
            Name: $row.find('.route-name').text(),
            LengthInM: parseInt($row.find('.route-length').text()) || 0,
            TimeInMinutes: parseInt($row.find('.route-time').text()) || 0,
            RoutesDirection: parseInt($row.find('.route-direction').data('value')) || 0,
            LineId: $row.find('.route-line').data('lineid') || null,
            StopIds: stopIds,
            StopNames: stopsData,
            IsActive: newActive
        };


        $.ajax({
            url: '/Planner/Routes/Execute',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function () {
                // Reload yerine sadece badge güncelle
                $btn.data('active', newActive);
                $badge.removeClass('bg-success bg-secondary')
                    .addClass(newActive ? 'bg-success' : 'bg-secondary')
                    .text(newActive ? 'Aktif' : 'Pasif');
            },
            error: function (xhr) {
                alert('Durum güncellenemedi: ' + (xhr.responseText || ''));
            }
        });
    });


    // === ARAMA ===
    $('#searchInput').on('input', function () {
        const term = $(this).val();
        refreshTable(term);
    });

    function refreshTable(term = '') {
        $.get('/Planner/Routes/Search', { term }, function (data) {
            $('#routesTableContainer').html(data);
        }).fail(function () {
            alert('Tablo yenilenemedi.');
        });
    }

    // === DURAKLARI GÖR ===
    $(document).on('click', '.view-stops', function () {
        const stopsJson = $(this).attr('data-stops'); // JSON array
        let stops = [];
        try {
            stops = JSON.parse(stopsJson); // JSON string → array
        } catch (e) {
            console.error('StopNames parse hatası:', e);
        }

        const modalBody = $('#stopsModalBody');
        modalBody.empty();
        stops.forEach(s => modalBody.append(`<div>${s}</div>`));

        // Footer gizli çünkü sadece görüntüleme
        $('#stopsModal .modal-footer').hide();

        // Modalı aç
        const stopsModal = new bootstrap.Modal(document.getElementById('stopsModal'));
        stopsModal.show();
    });


});
