$(function () {
    let linesMap = {};
    let stopsMap = {};
    let selectedStopsOrder = [];
    let tempStopsOrder = [];
    let latestRouteSummary = { distanceM: 0, durationMin: 0 };

    // Helper: keep table row's .view-stops data/attributes in sync
    function setRowStopsData(routeId, stopIds) {
        if (!routeId) return;
        const $row = $(`#routesTableContainer tr[data-id="${routeId}"]`);
        if (!$row.length) return;
        const $btn = $row.find('.view-stops');
        const stopNames = stopIds.map(id => stopsMap[id]?.name || '');

        // Eski cache'i temizle
        $btn.removeData('stopids').removeData('stops');

        // Yeni data’yı hem cache hem DOM’a yaz
        $btn.data('stopids', stopIds);
        $btn.data('stops', stopNames);
        $btn.attr('data-stopids', JSON.stringify(stopIds));
        $btn.attr('data-stops', JSON.stringify(stopNames));
    }


    // === LİNE VE STOPLARI YÜKLE ===
    function loadLinesAndStops(callback) {
        $.get('/Planner/Lines/GetAll', function (lines) {
            linesMap = {};
            const lineSelect = $('#LineId');
            const selectedId = lineSelect.data('selected');

            // Sadece geçerli ID varsa seç
            if (selectedId && selectedId !== "00000000-0000-0000-0000-000000000000") {
                lineSelect.val(selectedId);
            } else {
                lineSelect.val(''); // placeholder seçili kalsın
            }

            // Önce tüm optionları temizle
            lineSelect.empty();

            // Placeholder option
            const placeholder = $('<option>', {
                value: '',
                text: '-- Hat Seçin --',
                disabled: true
            });
            lineSelect.append(placeholder);

            // Gerçek hatları ekle
            lines.forEach(line => {
                if (!line.isDeleted) {
                    linesMap[line.id] = line.name;
                    const option = $('<option>', {
                        value: line.id,
                        text: `${line.code} - ${line.name}`
                    });
                    lineSelect.append(option);
                }
            });

            // Durakları yükle
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

                if (callback) setTimeout(callback, 200);
            });
        });
    }


    // === SAYFA YÜKLENDİKTEN SONRA ===
    loadLinesAndStops(function () {
        const selectedLineId = $('#LineId').data('selected');
        $('#LineId').val(selectedLineId || '');

        if (window.initialStops && Array.isArray(window.initialStops) && window.initialStops.length > 0) {
            console.log("📦 Initial stops:", window.initialStopsDetailed);
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

            // ✅ form input’unu da güncelle
            $("#SelectedStopsOrder").val(JSON.stringify(selectedStopsOrder));

            const currentRouteId = $('#routeForm [name="Id"]').val();
            setRowStopsData(currentRouteId, selectedStopsOrder);
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

    $("#saveSelectedStops").on('click', function () {
        selectedStopsOrder = $("#stopsList .list-group-item").map(function () {
            return $(this).data("stop-id");
        }).get();

        console.log("✅ Modal Kaydet sonrası sıra:", selectedStopsOrder);

        // Formdaki gizli input’a kaydet
        $("#SelectedStopsOrder").val(JSON.stringify(selectedStopsOrder));

        // 👇 Seçilen durakları formun altında göster
        const selectedNames = selectedStopsOrder.map(id => stopsMap[id]?.name || id);
        const html = selectedNames.map(name => `<div class="badge bg-secondary me-1">${name}</div>`).join("");
        $("#selectedStopsList").html(html);

        $("#stopsModal").modal("hide");
    });


    $('#stopsModal').on('show.bs.modal', function () {
        const routeId = $('#routeForm [name="Id"]').val();
        const $row = $(`#routesTableContainer tr[data-id="${routeId}"]`);
        const $btn = $row.find('.view-stops');

        let stopIds = [];
        let stopIdsRaw = $btn.attr('data-stopids') || $btn.data('stopids');
        if (typeof stopIdsRaw === 'string') {
            try {
                stopIds = JSON.parse(stopIdsRaw.replace(/&quot;/g, '"'));
            } catch {
                stopIds = stopIdsRaw.split(',').map(x => x.trim());
            }
        } else if (Array.isArray(stopIdsRaw)) stopIds = stopIdsRaw;

        if (!stopIds.length) stopIds = selectedStopsOrder;

        updateSelectedStopsList(stopIds);
    });



    // === SEÇİLEN DURAK LİSTESİ ===
    function updateSelectedStopsList(selectedIds) {
        const $list = $('#selectedStopsList');
        $list.empty();

        if (!selectedIds || selectedIds.length === 0) {
            $list.html('<em>Henüz durak seçilmedi</em>');
            return;
        }

        // Grid container (list-group yerine)
        $list.append('<ul id="sortableStops" class="stops-grid"></ul>');

        selectedIds.forEach((id, index) => {
            const stop = stopsMap[id];
            if (!stop) return;
            $('#sortableStops').append(`
            <li class="stop-card" data-id="${id}">
                <div class="stop-content">
                    <strong>${index + 1}.</strong> ${stop.name}
                    <br>
                    <small class="text-muted">(${stop.lat.toFixed(5)}, ${stop.lng.toFixed(5)})</small>
                </div>
            </li>
        `);
        });

        // === Sortable aktif et ===
        $('#sortableStops').sortable({
            placeholder: "sortable-placeholder",
            update: function () {
                selectedStopsOrder = $('#sortableStops li').map(function () {
                    return $(this).data('id');
                }).get();

                const names = selectedStopsOrder
                    .map(id => stopsMap[id]?.name || '')
                    .filter(Boolean);

                $('#selectedStopsOrder').html(`<strong>Sıra:</strong> ${names.join(' → ')}`);

                dispatchStopsToMap();

                const currentRouteId = $('#routeForm [name="Id"]').val();
                if (currentRouteId) {
                    setRowStopsData(currentRouteId, selectedStopsOrder);
                }

                const $viewBody = $('#viewStopsModalBody');
                if ($viewBody.is(':visible')) {
                    const html = `
                        <div class="view-stops-grid">
                            ${selectedStopsOrder.map((id, i) => {
                        const s = stopsMap[id];
                        if (!s) return '';
                        return `
                                    <div class="view-stop-card">
                                        <div class="fw-bold">${i + 1}. ${s.name}</div>
                                        <small class="text-muted d-block">(${s.lat.toFixed(5)}, ${s.lng.toFixed(5)})</small>
                                    </div>
                                `;
                    }).join('')}
                        </div>
                    `;
                    $viewBody.html(html);
                    }

                }
        });

        const names = selectedStopsOrder.map(id => stopsMap[id]?.name).filter(Boolean).join(' → ');
        $('#selectedStopsOrder').html(`<strong>Sıra:</strong> ${names || '<em>Henüz durak seçilmedi</em>'}`);
    }



    // === MAP UPDATE ===
    function dispatchStopsToMap() {
        if (Object.keys(stopsMap).length === 0) {
            setTimeout(dispatchStopsToMap, 200);
            return;
        }
        const stopsData = selectedStopsOrder.map(id => ({
            Id: stopsMap[id]?.id,
            Name: stopsMap[id]?.name,
            Lat: stopsMap[id]?.lat,
            Lng: stopsMap[id]?.lng
        })).filter(s => s.Id);
        document.dispatchEvent(new CustomEvent('stopsUpdated', { detail: stopsData }));
    }


    // === TABLODA DURAKLARI GÖR ===
    $(document).on('click', '.view-stops', function () {
        const stopIdsRaw = $(this).data('stopids');
        let stopIds = Array.isArray(stopIdsRaw) ? stopIdsRaw : [];
        const orderedStops = stopIds.map(id => stopsMap[id]).filter(Boolean);

        // Haritayı modal için göster
        showStopsMiniMapModal(orderedStops);

        // Alt kısımda sadece isimler
        const names = orderedStops.map(s => s.name);
        const modalBody = document.getElementById('viewStopsModalBody');
        if (!modalBody) return;

        if (names.length) {
            modalBody.innerHTML = `
                <div style="font-size: 1rem; line-height: 1.5; color: #212529;">
                    ${names.map((name, index) => `
                        <span>${name}</span>
                        ${index < names.length - 1 ? '<span style="margin: 0 6px; color: #6c757d;">→</span>' : ''}
                    `).join('')}
                </div>
            `;
        } else {
            modalBody.innerHTML = '<em>Henüz durak seçilmedi</em>';
        }
    });




    document.addEventListener("routeSummaryUpdated", (e) => {
        latestRouteSummary = e.detail;

        const $form = $('#routeForm');
        $form.find('[name="LengthInM"]').val(Math.round(latestRouteSummary.distanceM));
        $form.find('[name="TimeInMinutes"]').val(Math.round(latestRouteSummary.durationMin));

        console.log("📍 Haritadan gelen rota bilgisi (metre):", latestRouteSummary);
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
            StopNames: orderedStops.map(s => s.Name),
            RouteStops: orderedStops
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
            const $btn = $row.find('.view-stops');

            // Önce attr() oku, sonra data()
            let stopIdsRaw = $btn.attr('data-stopids') || $btn.data('stopids');
            if (!stopIdsRaw) return;

            let stopIds = [];
            if (typeof stopIdsRaw === 'string') {
                try {
                    stopIds = JSON.parse(stopIdsRaw.replace(/&quot;/g, '"'));
                } catch {
                    stopIds = stopIdsRaw.split(',').map(s => s.trim());
                }
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


    // === AKTİF / PASİF TOGGLE ===
    $(document).on('click', '.toggle-route-active', function (e) {
        e.preventDefault();
        e.stopPropagation();

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
                // Table refresh ile güncel UpdatedAt gösterilecek
                refreshTable();
            },
            error: function (xhr) {
                alert('Durum güncellenemedi: ' + (xhr.responseText || ''));
            }
        });
    });

    // === TABLOYU YENİLE ===
    function refreshTable(term = '') {
        $.get('/Planner/Routes/Search', { term }, function (data) {
            $('#routesTableContainer').html(data);
        }).fail(function () {
            alert('Tablo yenilenemedi.');
        });
    }


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
