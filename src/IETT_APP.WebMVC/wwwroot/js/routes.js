$(function () {
    let linesMap = {};
    let stopsMap = {};
    let selectedStopsOrder = [];
    let tempStopsOrder = [];

    // === LİNE VE STOPLARI YÜKLE ===
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
                    if (!stop.isDeleted) {
                        stopsMap[stop.id] = {
                            id: stop.id,
                            name: stop.name,
                            lat: stop.location.latitude,
                            lng: stop.location.longitude
                        };
                    }
                });
                if (callback) callback();
            });
        });
    }

    // === SAYFA YÜKLENDİKTEN SONRA ===
    loadLinesAndStops(function () {
        const selectedLineId = $('#LineId').data('selected');
        $('#LineId').val(selectedLineId || '');

        if (window.initialStops && Array.isArray(window.initialStops) && window.initialStops.length > 0) {
            selectedStopsOrder = window.initialStops.map(id => id.toString());

            if (window.initialStopsDetailed && Array.isArray(window.initialStopsDetailed)) {
                window.initialStopsDetailed
                    .sort((a, b) => (a.Order || 0) - (b.Order || 0))
                    .forEach(stop => {
                        const stopId = stop.StopId || stop.Id;
                        if (!stopId) return;
                        if (!selectedStopsOrder.includes(stopId)) selectedStopsOrder.push(stopId);
                        if (!stopsMap[stopId] && stop.Name) {
                            stopsMap[stopId] = {
                                id: stopId,
                                name: stop.Name,
                                lat: stop.Lat || 0,
                                lng: stop.Lng || 0
                            };
                        }
                    });
            }
            updateSelectedStopsList(selectedStopsOrder);
            dispatchStopsToMap();
        } else {
            selectedStopsOrder = [];
            updateSelectedStopsList(selectedStopsOrder);
        }

        // Tablo yüklendiğinde durak sırasını logla
        setTimeout(logRoutesOrder, 200);
    });

    // === FORM MODALI ===
    $(document).on('click', '#selectStopsBtn', function () {
        const $modalBody = $('#stopsModalBody').empty();
        tempStopsOrder = [...selectedStopsOrder];

        Object.entries(stopsMap).forEach(([id, stop]) => {
            const checked = tempStopsOrder.includes(id) ? 'checked' : '';
            $modalBody.append(`
                <div class="form-check mb-1">
                    <input class="form-check-input stop-checkbox" type="checkbox" value="${id}" id="stop_${id}" ${checked}>
                    <label class="form-check-label" for="stop_${id}">
                        ${stop.name} <small class="text-muted">(${stop.lat?.toFixed(5)}, ${stop.lng?.toFixed(5)})</small>
                    </label>
                </div>
            `);
        });

        updateStopsOrderDisplay();

        const modal = new bootstrap.Modal(document.getElementById('stopsModal'), { backdrop: true, keyboard: true });
        modal.show();

        $('#stopsModal').off('hidden.bs.modal').on('hidden.bs.modal', function () {
            selectedStopsOrder = [...tempStopsOrder];
            updateSelectedStopsList(selectedStopsOrder);
            dispatchStopsToMap();
        });
    });

    $(document).on('change', '.stop-checkbox', function () {
        const id = $(this).val();
        if (this.checked) {
            if (!tempStopsOrder.includes(id)) tempStopsOrder.push(id);
        } else {
            tempStopsOrder = tempStopsOrder.filter(x => x !== id);
        }
        updateStopsOrderDisplay();
    });

    function updateStopsOrderDisplay() {
        const $display = $('#selectedStopsOrder');
        if (!$display.length) {
            $('#stopsModal').find('.modal-footer').prepend(`<div id="selectedStopsOrder" class="w-100 text-start small text-muted mb-2"></div>`);
        }
        if (tempStopsOrder.length === 0) {
            $('#selectedStopsOrder').html('<em>Henüz durak seçilmedi</em>');
            return;
        }
        const names = tempStopsOrder.map(id => stopsMap[id]?.name || id);
        $('#selectedStopsOrder').html(`<strong>Sıra:</strong> ${names.join(' → ')}`);
    }

    $(document).on('click', '#saveSelectedStops', function () {
        // Modaldaki son sıra sortable ile eşleşmeli
        selectedStopsOrder = $('#sortableStops li').map(function () { return $(this).data('id'); }).get();
        updateSelectedStopsList(selectedStopsOrder);
        dispatchStopsToMap();
        bootstrap.Modal.getInstance(document.getElementById('stopsModal')).hide();
    });

    // === SEÇİLEN DURAK LİSTESİ ===
    function updateSelectedStopsList(selectedIds) {
        const $list = $('#selectedStopsList');
        $list.empty();

        if (!selectedIds || selectedIds.length === 0) {
            $list.html('<em>Henüz durak seçilmedi</em>');
            return;
        }

        $list.append('<ul id="sortableStops" class="list-group mb-3"></ul>');
        selectedIds.forEach(id => {
            const stop = stopsMap[id];
            if (!stop) return;
            $('#sortableStops').append(`
            <li class="list-group-item d-flex justify-content-between align-items-center" data-id="${id}">
                <span>${stop.name}</span>
                <small class="text-muted">(${stop.lat.toFixed(5)}, ${stop.lng.toFixed(5)})</small>
            </li>
        `);
        });

        // Sortable
        $('#sortableStops').sortable({
            update: function () {
                selectedStopsOrder = $('#sortableStops li').map(function () { return $(this).data('id'); }).get();
                dispatchStopsToMap();

                // Tabloya güncel sırayı aktar
                $('#routesTableContainer .view-stops').each(function () {
                    const $btn = $(this);
                    const rowId = $btn.closest('tr').data('id');
                    if (rowId === $('#routeForm [name="Id"]').val()) {
                        $btn.data('stopids', selectedStopsOrder);
                        const stopNames = selectedStopsOrder.map(id => stopsMap[id]?.name || '').filter(Boolean);
                        $btn.data('stops', stopNames);
                    }
                });

                updateSelectedStopsList(selectedStopsOrder);
            }
        });

        const names = selectedStopsOrder.map(id => stopsMap[id]?.name).filter(Boolean).join(' → ');
        $('#selectedStopsOrder').html(`<strong>Sıra:</strong> ${names || '<em>Henüz durak seçilmedi</em>'}`);
    }

    // === MAP UPDATE ===
    function dispatchStopsToMap() {
        const stopsData = selectedStopsOrder.map(id => ({
            Id: stopsMap[id].id,
            Name: stopsMap[id].name,
            Lat: stopsMap[id].lat,
            Lng: stopsMap[id].lng
        }));
        document.dispatchEvent(new CustomEvent('stopsUpdated', { detail: stopsData }));
    }

    // === TABLODA DURAKLARI GÖR ===
    $(document).on('click', '.view-stops', function () {
        const stopIdsRaw = $(this).data('stopids');
        let stopIds = Array.isArray(stopIdsRaw) ? stopIdsRaw : [];
        const orderedStops = stopIds.map(id => stopsMap[id]).filter(Boolean);
        const names = orderedStops.map(s => s.name);

        const modalBody = document.getElementById('viewStopsModalBody');
        if (!modalBody) return;
        modalBody.innerHTML = names.length ? `<div>${names.join(' → ')}</div>` : '<em>Henüz durak seçilmedi</em>';

        bootstrap.Modal.getOrCreateInstance(document.getElementById('viewStopsModal')).show();
    });

    // === FORM SUBMIT ===
    $(document).on('submit', '#routeForm', function (e) {
        e.preventDefault();

        const orderedStops = selectedStopsOrder.map((id, index) => ({
            StopId: id,
            Order: index + 1,
            Name: stopsMap[id]?.name || ''
        }));

        const payload = {
            Id: $(this).find('[name="Id"]').val(),
            Code: $(this).find('[name="Code"]').val(),
            Name: $(this).find('[name="Name"]').val(),
            LengthInM: parseInt($(this).find('[name="LengthInM"]').val()) || 0,
            TimeInMinutes: parseInt($(this).find('[name="TimeInMinutes"]').val()) || 0,
            RoutesDirection: parseInt($(this).find('[name="RoutesDirection"]').val()) || 0,
            LineId: $(this).find('[name="LineId"]').val() || null,
            IsActive: $(this).find('[name="IsActive"]').is(':checked'),
            StopIds: orderedStops.map(s => s.StopId),
            StopNames: orderedStops.map(s => s.Name)
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

    // === TABLO DURAK SIRASINI LOG ===
    function logRoutesOrder() {
        $('#routesTableContainer').find('tr').each(function () {
            const $row = $(this);
            const stopIdsRaw = $row.find('.view-stops').data('stopids');
            if (!stopIdsRaw) return;

            let stopIds = [];
            if (typeof stopIdsRaw === 'string') {
                try { stopIds = JSON.parse(stopIdsRaw.replace(/&quot;/g, '"')); }
                catch { stopIds = stopIdsRaw.split(',').map(s => s.trim()); }
            } else if (Array.isArray(stopIdsRaw)) stopIds = stopIdsRaw;

            const names = stopIds.map(id => stopsMap[id]?.name || id);
            console.log(`Route ${$row.data('id') || 'unknown'} durak sırası:`, names);
        });
    }

    // === ARAMA ===
    $('#searchInput').on('input', function () {
        const term = $(this).val();
        refreshTable(term);
    });

    function refreshTable(term = '') {
        $.get('/Planner/Routes/Search', { term }, function (data) {
            $('#routesTableContainer').html(data);
            setTimeout(logRoutesOrder, 200);
        });
    }

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

    // === İptal butonu ===
    $(document).on('click', '#cancelRouteBtn', function () {
        window.location.href = '/Planner/Routes';
    });

    // === ROUTE EKLE ===
    $('#addRouteBtn').on('click', function () {
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

});
