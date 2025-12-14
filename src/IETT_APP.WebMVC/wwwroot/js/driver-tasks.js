$(function () {
    console.log("🚌 Driver Tasks Script Loaded (Hızlı Mod)");

    // ===========================
    // 1. KABUL ET (Accept) - DİREKT ÇALIŞIR
    // ===========================
    $('.btn-accept').click(function () {
        const id = $(this).data('id');
        const $btn = $(this);

        // ONAY KODU SİLİNDİ: Direkt işlem başlıyor...

        $btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span>');

        $.post('/Driver/Tasks/Accept', { id: id })
            .done(function (res) {
                toastr.success(res.message);
                setTimeout(() => window.location.reload(), 1000);
            })
            .fail(function (xhr) {
                toastr.error(xhr.responseJSON?.message || "İşlem başarısız.");
                $btn.prop('disabled', false).html('<i class="bi bi-check-lg"></i> Kabul Et');
            });
    });

    // ===========================
    // 2. REDDET (Reject - Modal)
    // ===========================
    $('.btn-reject').click(function () {
        $('#rejectTaskId').val($(this).data('id'));
        $('#rejectReason').val(''); // Temizle
        new bootstrap.Modal(document.getElementById('rejectModal')).show();
    });

    $('#confirmRejectBtn').click(function () {
        const id = $('#rejectTaskId').val();
        const reason = $('#rejectReason').val();

        if (!reason) {
            toastr.warning("Lütfen bir neden belirtiniz.");
            return;
        }

        const $btn = $(this);
        $btn.prop('disabled', true).text('İşleniyor...');

        $.ajax({
            url: '/Driver/Tasks/Reject?id=' + id,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ Reason: reason }),
            success: function (res) {
                toastr.info("Görev reddedildi.");
                setTimeout(() => window.location.reload(), 1000);
            },
            error: function (xhr) {
                toastr.error(xhr.responseJSON?.message || "Hata oluştu.");
                $btn.prop('disabled', false).text('Görevi Reddet');
            }
        });
    });

    // ===========================
    // 3. BAŞLAT (Start) - DİREKT ÇALIŞIR
    // ===========================
    $('.btn-start').click(function () {
        const id = $(this).data('id');
        const $btn = $(this);

        // ONAY KODU SİLİNDİ: Direkt işlem başlıyor...

        $btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span> Başlatılıyor...');

        $.post('/Driver/Tasks/Start', { id: id })
            .done(function (res) {
                toastr.success(res.message);
                setTimeout(() => window.location.reload(), 1000);
            })
            .fail(function (xhr) {
                toastr.error(xhr.responseJSON?.message || "Başlatılamadı.");
                $btn.prop('disabled', false).html('<i class="bi bi-play-fill fs-5"></i> SEFERİ BAŞLAT');
            });
    });

    // ===========================
    // 4. BİTİR (Complete - Modal)
    // ===========================
    $('.btn-complete').click(function () {
        $('#completeTaskId').val($(this).data('id'));
        $('#passengerInput').val('');
        $('#odometerInput').val('');
        $('#fuelInput').val('');
        $('#driverNoteInput').val('');
        new bootstrap.Modal(document.getElementById('completeModal')).show();
    });

    $('#confirmCompleteBtn').click(function () {
        const id = $('#completeTaskId').val();
        const passengers = $('#passengerInput').val();
        const odometer = $('#odometerInput').val();
        const fuel = $('#fuelInput').val();
        const note = $('#driverNoteInput').val();

        if (!passengers || !odometer) {
            toastr.warning("Yolcu sayısı ve KM bilgisi zorunludur.");
            return;
        }

        const $btn = $(this);
        $btn.prop('disabled', true).text('Kaydediliyor...');

        const payload = {
            PassengerCount: parseInt(passengers),
            EndOdometerInput: parseFloat(odometer),
            FuelLevel: fuel ? parseInt(fuel) : null,
            Note: note
        };

        $.ajax({
            url: '/Driver/Tasks/Complete?id=' + id,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (res) {
                toastr.success(res.message);
                $('#completeModal').modal('hide');
                setTimeout(() => window.location.reload(), 1000);
            },
            error: function (xhr) {
                toastr.error(xhr.responseJSON?.message || "İşlem başarısız.");
                $btn.prop('disabled', false).text('Kaydet ve Bitir');
            }
        });
    });

    // ===========================
    // 5. SORUN BİLDİR (Fail - Modal)
    // ===========================
    $('.btn-fail').click(function () {
        $('#failTaskId').val($(this).data('id'));
        $('#failReason').val('');
        new bootstrap.Modal(document.getElementById('failModal')).show();
    });

    $('#confirmFailBtn').click(function () {
        const id = $('#failTaskId').val();
        const reason = $('#failReason').val();

        if (!reason) {
            toastr.warning("Açıklama giriniz.");
            return;
        }

        const $btn = $(this);
        $btn.prop('disabled', true).text('Bildiriliyor...');

        $.ajax({
            url: '/Driver/Tasks/Fail?id=' + id,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ Reason: reason }),
            success: function (res) {
                toastr.warning(res.message);
                setTimeout(() => window.location.reload(), 1000);
            },
            error: function (xhr) {
                toastr.error(xhr.responseJSON?.message || "Hata.");
                $btn.prop('disabled', false).text('Bildir');
            }
        });
    });

});