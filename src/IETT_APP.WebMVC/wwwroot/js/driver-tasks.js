$(function () {
    console.log("🚌 Driver Tasks Script Loaded (Optimize Edilmiş)");

    // Ortak Hata Gösterme Fonksiyonu
    function handleError(xhr, $btn, defaultText) {
        let msg = "İşlem sırasında bir hata oluştu.";

        if (xhr.responseJSON && xhr.responseJSON.message) {
            msg = xhr.responseJSON.message;
        } else if (xhr.responseText) {
            msg = xhr.responseText;
        }

        if (typeof toastr !== 'undefined') {
            toastr.error(msg);
        } else {
            alert(msg);
        }

        if ($btn && defaultText) {
            $btn.prop('disabled', false).html(defaultText);
        }
    }

    // ===========================
    // 1. KABUL ET (Accept)
    // ===========================
    $(document).on('click', '.btn-accept', function () {
        const id = $(this).data('id');
        const $btn = $(this);
        const originalText = $btn.html();

        $btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span>');

        $.post('/Driver/Tasks/Accept', { id: id })
            .done(function (res) {
                if (typeof toastr !== 'undefined') toastr.success(res.message || "Görev kabul edildi.");
                setTimeout(() => window.location.reload(), 1000);
            })
            .fail(function (xhr) {
                handleError(xhr, $btn, originalText);
            });
    });

    // ===========================
    // 2. REDDET (Reject - Modal)
    // ===========================
    $(document).on('click', '.btn-reject', function () {
        $('#rejectTaskId').val($(this).data('id'));
        $('#rejectReason').val('');
        new bootstrap.Modal(document.getElementById('rejectModal')).show();
    });

    $('#confirmRejectBtn').click(function () {
        const id = $('#rejectTaskId').val();
        const reason = $('#rejectReason').val();

        if (!reason || reason.trim().length < 5) {
            if (typeof toastr !== 'undefined') toastr.warning("Lütfen en az 5 karakterlik bir neden belirtiniz.");
            else alert("Lütfen en az 5 karakterlik bir neden belirtiniz.");
            return;
        }

        const $btn = $(this);
        const originalText = $btn.text();
        $btn.prop('disabled', true).text('İşleniyor...');

        $.ajax({
            url: '/Driver/Tasks/Reject?id=' + id,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ Reason: reason }),
            success: function (res) {
                if (typeof toastr !== 'undefined') toastr.info(res.message || "Görev reddedildi.");
                $('#rejectModal').modal('hide');
                setTimeout(() => window.location.reload(), 1000);
            },
            error: function (xhr) {
                handleError(xhr, $btn, originalText);
            }
        });
    });

    // ===========================
    // 3. BAŞLAT (Start)
    // ===========================
    $(document).on('click', '.btn-start', function () {
        const id = $(this).data('id');
        const $btn = $(this);
        const originalText = $btn.html();

        $btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span> Başlatılıyor...');

        $.post('/Driver/Tasks/Start', { id: id })
            .done(function (res) {
                if (typeof toastr !== 'undefined') toastr.success(res.message || "Sefer başlatıldı.");
                setTimeout(() => window.location.reload(), 1000);
            })
            .fail(function (xhr) {
                handleError(xhr, $btn, originalText);
            });
    });

    // ===========================
    // 4. BİTİR (Complete - Modal)
    // ===========================
    $(document).on('click', '.btn-complete', function () {
        const taskId = $(this).data('id');

        // 1. ID'yi ve inputları sıfırla
        $('#completeTaskId').val(taskId);
        $('#passengerInput').val('');
        $('#odometerInput').val('');
        $('#fuelInput').val('');
        $('#driverNoteInput').val('');

        // 2. Yükleniyor bilgisi göster (Eğer elementler varsa)
        if ($('#lastFuelInfo').length) $('#lastFuelInfo').html('<span class="spinner-border spinner-border-sm"></span> Yükleniyor...');
        if ($('#lastKmInfo').length) $('#lastKmInfo').html('<span class="spinner-border spinner-border-sm"></span> Yükleniyor...');

        // 3. Modalı aç
        const modal = new bootstrap.Modal(document.getElementById('completeModal'));
        modal.show();

        // 4. Araç verilerini çek (HOME Controller'dan)
        $.ajax({
            url: '/Driver/Home/GetVehicleInfoForTask?taskId=' + taskId,
            type: 'GET',
            success: function (data) {
                // Yakıt Bilgisi
                if (data.lastFuel !== null && data.lastFuel !== undefined) {
                    $('#lastFuelInfo').html(`<i class="bi bi-clock-history"></i> Mevcut Depo: <strong>%${data.lastFuel}</strong>`);
                } else {
                    $('#lastFuelInfo').text('Veri yok.');
                }

                // Kilometre Bilgisi
                if (data.lastKm !== null && data.lastKm !== undefined) {
                    $('#lastKmInfo').html(`<i class="bi bi-arrow-counterclockwise"></i> Başlangıç: <strong>${data.lastKm} km</strong>`);
                    // Hatalı girişi engellemek için min değerini ayarla
                    $('#odometerInput').attr('min', data.lastKm);
                } else {
                    $('#lastKmInfo').text('Veri yok.');
                }
            },
            error: function () {
                $('#lastFuelInfo').text('Veri çekilemedi.');
                $('#lastKmInfo').text('Veri çekilemedi.');
            }
        });
    });

    $('#confirmCompleteBtn').click(function () {
        const id = $('#completeTaskId').val();
        const passengers = $('#passengerInput').val();
        const odometer = $('#odometerInput').val();
        const fuel = $('#fuelInput').val();
        const note = $('#driverNoteInput').val();

        if (!passengers || !odometer) {
            if (typeof toastr !== 'undefined') toastr.warning("Yolcu sayısı ve KM bilgisi zorunludur.");
            else alert("Yolcu sayısı ve KM bilgisi zorunludur.");
            return;
        }

        const $btn = $(this);
        const originalText = $btn.text();
        $btn.prop('disabled', true).text('Kaydediliyor...');

        // DTO ile uyumlu payload
        const payload = {
            PassengerCount: parseInt(passengers),
            EndOdometerInput: parseFloat(odometer),
            FuelLevel: fuel ? parseInt(fuel) : null,
            DriverNotes: note
        };

        $.ajax({
            url: '/Driver/Tasks/Complete?id=' + id,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (res) {
                if (typeof toastr !== 'undefined') toastr.success(res.message || "Sefer tamamlandı.");
                $('#completeModal').modal('hide');
                setTimeout(() => window.location.reload(), 1000);
            },
            error: function (xhr) {
                handleError(xhr, $btn, originalText);
            }
        });
    });

    // ===========================
    // 5. SORUN BİLDİR (Fail - Modal)
    // ===========================
    $(document).on('click', '.btn-fail', function () {
        $('#failTaskId').val($(this).data('id'));
        $('#failReason').val('');
        new bootstrap.Modal(document.getElementById('failModal')).show();
    });

    $('#confirmFailBtn').click(function () {
        const id = $('#failTaskId').val();
        const reason = $('#failReason').val();

        if (!reason || reason.trim().length < 5) {
            if (typeof toastr !== 'undefined') toastr.warning("Lütfen açıklama giriniz (min 5 karakter).");
            else alert("Lütfen açıklama giriniz (min 5 karakter).");
            return;
        }

        const $btn = $(this);
        const originalText = $btn.text();
        $btn.prop('disabled', true).text('Bildiriliyor...');

        $.ajax({
            url: '/Driver/Tasks/Fail?id=' + id,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ Reason: reason }),
            success: function (res) {
                if (typeof toastr !== 'undefined') toastr.warning(res.message || "Durum bildirildi.");
                $('#failModal').modal('hide');
                setTimeout(() => window.location.reload(), 1000);
            },
            error: function (xhr) {
                handleError(xhr, $btn, originalText);
            }
        });
    });
});