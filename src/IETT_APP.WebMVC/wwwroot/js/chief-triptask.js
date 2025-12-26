$(function () {
    console.log("🚀 Chief TripTask Script Başlatıldı.");

    // === 1. SELECT2 BAŞLAT ===
    $('.select2').select2({
        width: '100%',
        theme: 'bootstrap-5',
        placeholder: "Seçiniz",
        allowClear: true
    });

    // === 2. TARİH SEÇİCİ (FLATPICKR) BAŞLATMA ===
    initDatePickers();

    function initDatePickers() {
        const dateInputs = document.querySelectorAll(".date-input-with-icon");

        if (dateInputs.length > 0 && typeof flatpickr !== 'undefined') {
            dateInputs.forEach(input => {

                // 🔥 KİLİT NOKTA: Disabled/Readonly ise takvimi bağlama! 🔥
                // Bu sayede Edit modunda takvim açılmaz.
                if (input.hasAttribute('disabled') || input.hasAttribute('readonly')) {
                    return;
                }

                // Mevcut değeri al (Düzenleme modu için)
                const existingValue = input.value;

                flatpickr(input, {
                    enableTime: true,             // Saat seçimi aktif
                    dateFormat: "d.m.Y H:i",      // Format: 25.12.2025 14:30
                    time_24hr: true,              // 24 saat formatı
                    locale: "tr",                 // Türkçe dil desteği

                    // "today" -> Bugünden önceki günleri (dün vb.) seçtirmesin.
                    minDate: "today",

                    // Input'ta zaten bir tarih varsa onu varsayılan olarak seçili getir
                    defaultDate: existingValue ? existingValue : null,

                    disableMobile: "true",        // Mobil klavye açılmasın
                    allowInput: false             // Elle yazmayı engelle (sadece seçim)
                });
            });
        }
    }

    // === 3. DROPDOWNLARI DOLDUR ===
    initDropdowns();

    function initDropdowns() {
        if (typeof allLines === 'undefined') {
            console.warn("allLines bulunamadı!");
            return;
        }

        // HATLAR
        populateDropdown('#LineId', allLines, 'id', 'code', 'name');

        // GARAJLAR
        const garageList = (typeof allGarages !== 'undefined' ? allGarages : []).map(g => ({
            id: g.id,
            text: g.garageName || g.name || "İsimsiz Garaj"
        }));
        populateSimpleDropdown('#GarageId', garageList);

        // ŞOFÖRLER
        const driverList = (typeof allDrivers !== 'undefined' ? allDrivers : []).map(d => ({
            id: d.id,
            text: (d.name || "") + " " + (d.surname || "") + " (" + (d.employeeNumber || "-") + ")"
        }));
        populateSimpleDropdown('#DriverId', driverList);

        // Edit Modu İçin Seçimleri Yap
        setInitialValue('#LineId');
        setInitialValue('#GarageId');
        setInitialValue('#DriverId');
        setInitialValue('#Status');
    }

    // --- YARDIMCI FONKSİYONLAR ---
    function populateDropdown(selector, data, valueProp, textProp1, textProp2 = null) {
        const $select = $(selector);
        $select.find('option:not(:first)').remove();
        if (!data || data.length === 0) return;

        data.forEach(item => {
            let val = item[valueProp];
            let text = item[textProp1] || "";
            if (textProp2) text += " - " + (item[textProp2] || "");
            $select.append(new Option(text, val));
        });
    }

    function populateSimpleDropdown(selector, data) {
        const $select = $(selector);
        $select.find('option:not(:first)').remove();
        if (!data || data.length === 0) return;

        data.forEach(item => {
            $select.append(new Option(item.text, item.id));
        });
    }

    function setInitialValue(selector) {
        const val = $(selector).data('selected');
        if (val) $(selector).val(val).trigger('change');
    }

    // === 4. CASCADING: HAT -> GÜZERGAH ===
    $('#LineId').on('change', function () {
        const selectedLineId = $(this).val();
        const $routeSelect = $('#RouteId');

        $routeSelect.empty().append(new Option('-- Güzergah Seçin --', ''));

        if (selectedLineId && typeof allRoutes !== 'undefined') {
            const filteredRoutes = allRoutes.filter(r => r.lineId === selectedLineId);

            filteredRoutes.forEach(r => {
                let dirText = r.direction === 0 ? "Gidiş" : "Dönüş";
                $routeSelect.append(new Option(r.name + " (" + dirText + ")", r.id));
            });
            $routeSelect.prop('disabled', false);
        } else {
            $routeSelect.prop('disabled', true);
        }

        const selectedRoute = $routeSelect.data('selected');
        if (selectedRoute) {
            $routeSelect.val(selectedRoute).trigger('change');
            $routeSelect.removeAttr('data-selected');
        }
    });

    // === 5. CASCADING: GARAJ -> ARAÇ ===
    $('#GarageId').on('change', function () {
        const selectedGarageId = $(this).val();
        const $vehicleSelect = $('#VehicleId');

        $vehicleSelect.empty().append(new Option('-- Araç Seçin --', ''));

        if (selectedGarageId && typeof allVehicles !== 'undefined') {
            const filteredVehicles = allVehicles.filter(v => v.garageId === selectedGarageId);

            filteredVehicles.forEach(v => {
                let text = (v.doorNumber || "") + " - " + (v.plateNumber || "");
                $vehicleSelect.append(new Option(text, v.id));
            });
            $vehicleSelect.prop('disabled', false);
        } else {
            $vehicleSelect.prop('disabled', true);
        }

        const selectedVehicle = $vehicleSelect.data('selected');
        if (selectedVehicle) {
            $vehicleSelect.val(selectedVehicle).trigger('change');
            $vehicleSelect.removeAttr('data-selected');
        }
    });

    // === 6. UX: İPTAL SEÇİLİRSE AÇIKLAMA İSTE ===
    $('#Status').on('change', function () {
        const statusVal = parseInt($(this).val());
        const $reasonInput = $('#StatusReason');

        // Enum: Cancelled(4) veya Incomplete(5)
        if (statusVal === 4 || statusVal === 5) {
            $reasonInput.addClass('is-invalid border-danger');
            $reasonInput.prop('required', true);
            $reasonInput.attr('placeholder', 'Lütfen iptal/yarım kalma nedenini giriniz...');
            $reasonInput.focus();
        } else {
            $reasonInput.removeClass('is-invalid border-danger');
            $reasonInput.prop('required', false);
            $reasonInput.attr('placeholder', 'Durum Açıklaması');
        }
    });

    // ============================================================
    // 🔥 FORM SUBMIT (KAYIT İŞLEMİ) 🔥
    // ============================================================
    $(document).off('click', '#btnSaveTask').on('click', '#btnSaveTask', function (e) {
        e.preventDefault();

        const $btn = $(this);
        const $form = $('#tripTaskForm');

        if ($form.valid && !$form.valid()) {
            $('.input-validation-error').first().focus();
            return;
        }

        const originalText = $btn.html();
        $btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span> İşleniyor...');

        const id = $('#Id').val();
        const isEdit = id && id !== "00000000-0000-0000-0000-000000000000";

        let statusVal = $('#Status').val();
        if (!statusVal) statusVal = 1;

        // PAYLOAD OLUŞTURMA
        const payload = {
            id: id,
            title: $('#Title').val(),
            description: $('#Description').val(),
            status: parseInt(statusVal),
            statusReason: $('#StatusReason').val(),

            lineId: $('#LineId').val(),
            routeId: $('#RouteId').val(),
            garageId: $('#GarageId').val(),
            vehicleId: $('#VehicleId').val(),
            driverId: $('#DriverId').val(),

            passengerCount: parseInt($('#PassengerCount').val()) || 0,
            delayInMinutes: parseInt($('#DelayInMinutes').val()) || 0,
            delayOutMinutes: parseInt($('#DelayOutMinutes').val()) || 0,

            // DİKKAT: Disabled inputlar .val() ile alınamazsa diye hidden inputlar HTML'de var.
            // Ama JS .val() disabled olsa bile değeri okur. Yine de null kontrolü ekledik.
            scheduledDeparture: $('#ScheduledDeparture').val() || null,
            scheduledArrival: $('#ScheduledArrival').val() || null,

            adjustedDeparture: $('#AdjustedDeparture').val() || null,
            adjustedArrival: $('#AdjustedArrival').val() || null,

            actualDeparture: $('#ActualDeparture').val() || null,
            actualArrival: $('#ActualArrival').val() || null
        };

        const url = isEdit ? '/Chief/TripTasks/Edit' : '/Chief/TripTasks/Create';

        $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (res) {
                if (typeof toastr !== 'undefined') {
                    toastr.success(res.message || "İşlem başarılı.");
                } else {
                    alert(res.message || "Başarılı!");
                }

                setTimeout(function () {
                    window.location.href = res.redirectUrl || '/Chief/TripTasks';
                }, 1000);
            },
            error: function (xhr) {
                console.error("AJAX Hatası:", xhr);
                let errorMsg = "Bir hata oluştu.";

                if (xhr.responseJSON) {
                    if (xhr.responseJSON.message) {
                        errorMsg = xhr.responseJSON.message;
                    } else if (xhr.responseJSON.title) {
                        errorMsg = xhr.responseJSON.title;
                    }
                    if (xhr.responseJSON.errors) {
                        if (Array.isArray(xhr.responseJSON.errors)) {
                            errorMsg += "\n" + xhr.responseJSON.errors.join("\n");
                        } else if (typeof xhr.responseJSON.errors === 'object') {
                            for (let key in xhr.responseJSON.errors) {
                                errorMsg += "\n- " + xhr.responseJSON.errors[key];
                            }
                        }
                    }
                } else if (xhr.responseText) {
                    if (xhr.responseText.length > 200) {
                        errorMsg = "Sunucu tarafında teknik bir hata oluştu.";
                    } else {
                        errorMsg = xhr.responseText;
                    }
                }

                if (typeof toastr !== 'undefined') {
                    toastr.error(errorMsg, "Hata", { timeOut: 7000 });
                } else {
                    alert("Hata: " + errorMsg);
                }

                $btn.prop('disabled', false).html(originalText);
            },
            complete: function () {
                $btn.prop('disabled', false).html(originalText);
            }
        });
    });

    // Butonlar
    $('#cancelTripTasksBtn').on('click', function () {
        window.location.href = '/Chief/TripTasks';
    });

    $(document).on('click', '.edit-trip-task', function () {
        const id = $(this).data('id');
        window.location.href = '/Chief/TripTasks/Edit/' + id;
    });

    $(document).off('click', '.delete-trip-task').on('click', '.delete-trip-task', function (e) {
        e.preventDefault(); // Sayfanın yukarı zıplamasını engelle

        const btn = $(this);
        const id = btn.data('id');

        if (!confirm('Bu görevi silmek istediğinize emin misiniz?')) return;

        // Butonu kilitle (Tekrar basılmasın)
        btn.prop('disabled', true);

        $.post('/Chief/TripTasks/Delete/' + id, function (res) {
            // Başarılıysa bildirim göster ve sayfayı yenile
            if (typeof toastr !== 'undefined') toastr.success("Görev silindi.");

            setTimeout(() => {
                window.location.reload();
            }, 500);
        })
            .fail(function (xhr) {
                // Hata varsa
                let msg = xhr.responseJSON?.message || "Silme işlemi başarısız.";
                if (typeof toastr !== 'undefined') toastr.error(msg);
                else alert(msg);

                // Kilidi aç
                btn.prop('disabled', false);
            });
    });

    // ==============================
    // 🔎 ARAMA VE TABLO YENİLEME
    // ==============================

    // Arama Kutusuna Yazınca Tetikle
    $('#searchInput').on('input', function () {
        const term = $(this).val();
        refreshTripTasksTable(term);
    });

    // Tabloyu Sunucudan Yenileyen Fonksiyon
    function refreshTripTasksTable(term = '') {
        $.get('/Chief/TripTasks/Search', { term }, function (data) {

            // 1. Yeni HTML'i konteynerin içine bas
            $('#tripTasksTableContainer').html(data);

            // 2. 🔥 ÖNEMLİ: Yeni gelen veriye sayfalamayı tekrar uygula 🔥
            initPagination();
        });
    }

    // ==========================================
    // 📄 CLIENT-SIDE PAGINATION (İZOLE VE ÇAKIŞMASIZ)
    // ==========================================

    function initPagination() {
        // Her bir tablo container'ını (card) bul ve ayrı ayrı işle
        $('.table-container').each(function () {
            const $container = $(this);
            setupPaginationForContainer($container);
        });
    }

    function setupPaginationForContainer($container) {
        // Sadece bu container içindeki satırları bul
        const $rows = $container.find('.task-row');
        const totalRows = $rows.length;
        const pageSize = 10;
        const totalPages = Math.ceil(totalRows / pageSize);

        // Bu container içindeki kontrolleri bul (Class ile)
        const $footer = $container.find('.pagination-container');
        const $pageInfo = $container.find('.page-info');
        const $pageNum = $container.find('.current-page-num');
        const $btnPrev = $container.find('.btn-prev');
        const $btnNext = $container.find('.btn-next');

        // Veri azsa footer'ı gizle, hepsini göster
        if (totalRows <= pageSize) {
            $footer.hide();
            $rows.removeClass('d-none');
            return;
        } else {
            $footer.show();
        }

        // State (Mevcut Sayfa) - Element üzerinde tutuyoruz
        let currentPage = $container.data('currentPage') || 1;

        function showPage(page) {
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;
            currentPage = page;
            $container.data('currentPage', currentPage); // State güncelle

            // 1. Bu tablodaki TÜM satırları gizle
            $rows.addClass('d-none');

            // 2. İlgili aralığı göster
            const start = (page - 1) * pageSize;
            const end = start + pageSize;
            $rows.slice(start, end).removeClass('d-none');

            // 3. Bilgileri Güncelle
            $pageInfo.text(`${start + 1}-${Math.min(end, totalRows)} arası gösteriliyor (Toplam: ${totalRows})`);
            $pageNum.text(currentPage);

            // 4. Butonları Aktif/Pasif Yap
            if (currentPage === 1) $btnPrev.addClass('disabled');
            else $btnPrev.removeClass('disabled');

            if (currentPage === totalPages) $btnNext.addClass('disabled');
            else $btnNext.removeClass('disabled');
        }

        // İlk yükleme
        showPage(currentPage);

        // Olay Dinleyicileri (Önce temizle sonra ekle)
        $btnPrev.off('click').on('click', function (e) {
            e.preventDefault();
            if (currentPage > 1) showPage(currentPage - 1);
        });

        $btnNext.off('click').on('click', function (e) {
            e.preventDefault();
            if (currentPage < totalPages) showPage(currentPage + 1);
        });
    }

    // Sayfa Yüklendiğinde
    initPagination();
});