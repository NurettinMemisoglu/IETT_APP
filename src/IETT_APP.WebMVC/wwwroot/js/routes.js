$(function () {
    let linesMap = {};
    let stopsMap = {};
    let selectedStopsOrder = [];
    let tempStopsOrder = [];
    let latestRouteSummary = { distanceM: 0, durationMin: 0 };

    // =======================================================
    // 1. HELPER FONKSİYONLAR
    // =======================================================

    // Tablo satırındaki butonun verilerini günceller (Cache temizleyerek)
    function setRowStopsData(routeId, stopIds) {
        if (!routeId) return;
        const $row = $(`#routesTableContainer tr[data-id="${routeId}"]`);
        if (!$row.length) return;
        const $btn = $row.find('.view-stops');
        const stopNames = stopIds.map(id => stopsMap[id]?.name || '');

        $btn.removeData('stopids').removeData('stops'); // Eski cache sil
        $btn.data('stopids', stopIds);
        $btn.data('stops', stopNames);
        $btn.attr('data-stopids', JSON.stringify(stopIds));
        $btn.attr('data-stops', JSON.stringify(stopNames));
    }

    // Hatları ve Durakları Yükle
    function loadLinesAndStops(callback) {
        $.get('/Planner/Lines/GetAll', function (lines) {
            linesMap = {};
            const lineSelect = $('#LineId');
            const selectedId = lineSelect.data('selected');

            // Selectbox'ı temizle ve doldur
            lineSelect.empty();
            lineSelect.append($('<option>', { value: '', text: '-- Hat Seçin --', disabled: true }));

            lines.forEach(line => {
                if (!line.isDeleted) {
                    linesMap[line.id] = line.name;
                    lineSelect.append($('<option>', { value: line.id, text: `${line.code} - ${line.name}` }));
                }
            });

            // Seçili değeri ata
            if (selectedId && selectedId !== "00000000-0000-0000-0000-000000000000") {
                lineSelect.val(selectedId);
            } else {
                lineSelect.val('');
            }

            // Durakları Yükle
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

    // =======================================================
    // 2. SAYFA YÜKLENİNCE ÇALIŞACAKLAR
    // =======================================================
    loadLinesAndStops(function () {
        // Eğer Edit sayfasındaysak ve duraklar geldiyse onları işle
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
    });

    // =======================================================
    // 3. AKTİF / PASİF TOGGLE (DÜZELTİLDİ: Çift İstek ve 404 Yok)
    // =======================================================
    // .off() kullanarak önceki eventleri temizliyoruz, böylece çift istek atmaz.
    $(document).off('click', '.toggle-route-active').on('click', '.toggle-route-active', function (e) {
        e.preventDefault();
        e.stopPropagation();

        var $btn = $(this);
        var $row = $btn.closest('tr');
        var routeId = $row.data('id');

        if (!routeId) {
            alert("Hata: Rota ID bulunamadı.");
            return;
        }

        // Durumu TR'den oku (Butondan değil, satır verisinden)
        // Not: HTML'de data-active="@item.IsActive.ToString().ToLower()" olmalı
        var activeData = $row.data('active');
        var currentActive = activeData === true || activeData === "true";
        var newActive = !currentActive;

        var message = currentActive
            ? "Bu hattı PASİF yapmak istediğinize emin misiniz?"
            : "Bu hattı AKTİF yapmak istediğinize emin misiniz?";

        if (!confirm(message)) return;

        // --- VERİ HAZIRLIĞI ---
        // Verileri GetById API'si yerine doğrudan HTML satırından okuyoruz.
        // Bu verilerin _RoutesTablePartial.cshtml içindeki TR elementinde 'data-' olarak tanımlı olması şarttır.

        const stopIds = $row.data('stopids') || [];
        const stopNames = $row.data('stopnames') || [];

        // Backend'in beklediği RouteStops listesini oluştur
        const routeStops = stopIds.map((sid, index) => ({
            StopId: sid,
            Name: stopNames[index] || '',
            Order: index + 1
        }));

        const payload = {
            Id: routeId,
            Code: $row.data('code'),
            Name: $row.data('name'),
            LengthInM: parseInt($row.data('length')) || 0,
            TimeInMinutes: parseInt($row.data('time')) || 0,
            RoutesDirection: parseInt($row.data('direction')) || 0,
            LineId: $row.data('lineid'), // Guid string
            IsActive: newActive,         // Yeni durum
            StopIds: stopIds,
            StopNames: stopNames,
            RouteStops: routeStops
        };

        // Veri kontrolü: LineId olmadan backend hata verir
        if (!payload.LineId) {
            console.error("LineId okunamadı. Payload:", payload);
            alert("Hata: Hat bilgisi okunamadı. Sayfayı yenileyip tekrar deneyin.");
            return;
        }

        // AJAX İsteği
        $.ajax({
            url: '/Planner/Routes/Execute',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function () {
                // Başarılı olursa tabloyu yenile
                refreshTable($('#searchInput').val());
            },
            error: function (xhr) {
                console.error("Execute Hatası:", xhr.responseText);
                var errorMsg = "İşlem başarısız.";
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMsg += "\n" + xhr.responseJSON.message;
                }
                alert(errorMsg);
            }
        });
    });

    // =======================================================
    // 4. TABLO VE ARAMA İŞLEMLERİ
    // =======================================================

    // Arama kutusu (Debounce eklenebilir ama şimdilik düz)
    $('#searchInput').off('input').on('input', function () {
        const term = $(this).val();
        refreshTable(term);
    });

    // Tabloyu yenile
    function refreshTable(term = '') {
        $.get('/Planner/Routes/Search', { term }, function (data) {
            $('#routesTableContainer').html(data);
        }).fail(function () {
            console.error('Tablo yenilenemedi.');
        });
    }

    // =======================================================
    // 5. MODAL VE DURAK SEÇİMİ (CREATE/EDIT SAYFASI İÇİN)
    // =======================================================

    // Modal Aç
    $(document).off('click', '#selectStopsBtn').on('click', '#selectStopsBtn', function () {
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
    });

    // Checkbox değişince
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

    // Modal Kaydet Butonu
    $("#saveSelectedStops").off('click').on('click', function () {
        selectedStopsOrder = [...tempStopsOrder]; // Geçici listeyi ana listeye aktar

        console.log("✅ Duraklar seçildi:", selectedStopsOrder);

        // Form input güncelle
        $("#SelectedStopsOrder").val(JSON.stringify(selectedStopsOrder));

        // Listeyi güncelle
        updateSelectedStopsList(selectedStopsOrder);
        dispatchStopsToMap();

        // Modalı kapat (jQuery ile bootstrap instance bulup kapatma)
        var modalEl = document.getElementById('stopsModal');
        var modal = bootstrap.Modal.getInstance(modalEl);
        if (modal) modal.hide();
    });

    // Durakları Görüntüle Modalı (Index Sayfası İçin)
    $(document).on('click', '.view-stops', function () {
        const stopIdsRaw = $(this).attr('data-stopids') || $(this).data('stopids');
        let stopIds = [];

        if (typeof stopIdsRaw === 'string') {
            try {
                stopIds = JSON.parse(stopIdsRaw);
            } catch {
                stopIds = stopIdsRaw.split(',').map(x => x.trim());
            }
        } else if (Array.isArray(stopIdsRaw)) {
            stopIds = stopIdsRaw;
        }

        const orderedStops = stopIds.map(id => stopsMap[id]).filter(Boolean);
        const names = orderedStops.map(s => s.name);

        // Harita Modalı (Varsa)
        if (typeof showStopsMiniMapModal === 'function') {
            showStopsMiniMapModal(orderedStops);
        }

        const modalBody = document.getElementById('viewStopsModalBody');
        if (modalBody) {
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
        }
    });

    // Seçilen Durak Listesini HTML'e Bas
    function updateSelectedStopsList(selectedIds) {
        const $list = $('#selectedStopsList');
        $list.empty();

        if (!selectedIds || selectedIds.length === 0) {
            $list.html('<em>Henüz durak seçilmedi</em>');
            return;
        }

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

        // Sortable (Sürükle Bırak)
        if ($.fn.sortable) {
            $('#sortableStops').sortable({
                placeholder: "sortable-placeholder",
                update: function () {
                    selectedStopsOrder = $('#sortableStops li').map(function () {
                        return $(this).data('id');
                    }).get();

                    dispatchStopsToMap();
                    $("#SelectedStopsOrder").val(JSON.stringify(selectedStopsOrder));
                }
            });
        }
    }

    // Haritadan Gelen Veri
    document.addEventListener("routeSummaryUpdated", (e) => {
        latestRouteSummary = e.detail;
        const $form = $('#routeForm');
        $form.find('[name="LengthInM"]').val(Math.round(latestRouteSummary.distanceM));
        $form.find('[name="TimeInMinutes"]').val(Math.round(latestRouteSummary.durationMin));
    });

    // Harita Güncelleme Eventi
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

    // =======================================================
    // 6. FORM SUBMIT (CREATE / EDIT)
    // =======================================================
    // Tekrarlı binding'i önlemek için .off kullanıyoruz
    $(document).off('submit', '#routeForm').on('submit', '#routeForm', function (e) {
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

    // =======================================================
    // 7. ROUTE SİLME
    // =======================================================
    $(document).off('click', '.delete-route').on('click', '.delete-route', function () {
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