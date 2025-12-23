$(function () {
    console.log("🚌 Driver Tasks Script Loaded (Optimize Edilmiş)");

    // Ortak Hata Gösterme Fonksiyonu
    function handleError(xhr, $btn, defaultText) {
        let msg = "İşlem sırasında bir hata oluştu.";

        if (xhr.responseJSON && xhr.responseJSON.message) {
            msg = xhr.responseJSON.message;
        } else if (xhr.responseText) {
            // Bazen sunucu JSON yerine düz text hata dönebilir
            msg = xhr.responseText;
        }

        toastr.error(msg);

        if ($btn && defaultText) {
            $btn.prop('disabled', false).html(defaultText);
        }
    }

    // ===========================
    // 1. KABUL ET (Accept)
    // ===========================
    $('.btn-accept').click(function () {
        const id = $(this).data('id');
        const $btn = $(this);
        const originalText = $btn.html();

        $btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span>');

        // URL: /Driver/Tasks/Accept?id=... (MVC Controller bunu anlar)
        $.post('/Driver/Tasks/Accept', { id: id })
            .done(function (res) {
                toastr.success(res.message || "Görev kabul edildi.");
                setTimeout(() => window.location.reload(), 1000);
            })
            .fail(function (xhr) {
                handleError(xhr, $btn, originalText);
            });
    });

    // ===========================
    // 2. REDDET (Reject - Modal)
    // ===========================
    $('.btn-reject').click(function () {
        $('#rejectTaskId').val($(this).data('id'));
        $('#rejectReason').val('');
        new bootstrap.Modal(document.getElementById('rejectModal')).show();
    });

    $('#confirmRejectBtn').click(function () {
        const id = $('#rejectTaskId').val();
        const reason = $('#rejectReason').val();

        if (!reason || reason.trim().length < 5) {
            toastr.warning("Lütfen en az 5 karakterlik bir neden belirtiniz.");
            return;
        }

        const $btn = $(this);
        const originalText = $btn.text();
        $btn.prop('disabled', true).text('İşleniyor...');

        // Controller: [FromBody] RejectTripRequestDto bekliyor
        $.ajax({
            url: '/Driver/Tasks/Reject?id=' + id, // ID QueryString'den gider
            type: 'POST',
            contentType: 'application/json', // JSON olarak gönderiyoruz
            data: JSON.stringify({ Reason: reason }), // Body
            success: function (res) {
                toastr.info(res.message || "Görev reddedildi.");
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
    $('.btn-start').click(function () {
        const id = $(this).data('id');
        const $btn = $(this);
        const originalText = $btn.html();

        $btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span> Başlatılıyor...');

        $.post('/Driver/Tasks/Start', { id: id })
            .done(function (res) {
                toastr.success(res.message || "Sefer başlatıldı.");
                setTimeout(() => window.location.reload(), 1000);
            })
            .fail(function (xhr) {
                handleError(xhr, $btn, originalText);
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
        const originalText = $btn.text();
        $btn.prop('disabled', true).text('Kaydediliyor...');

        // DTO ile birebir uyumlu payload
        const payload = {
            PassengerCount: parseInt(passengers),
            EndOdometerInput: parseFloat(odometer), // Controller "EndOdometerInput" bekliyor
            FuelLevel: fuel ? parseInt(fuel) : null,
            DriverNotes: note // Controller "DriverNotes" bekliyor (eskiden Note idi, dikkat!)
        };

        $.ajax({
            url: '/Driver/Tasks/Complete?id=' + id,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (res) {
                toastr.success(res.message || "Sefer tamamlandı.");
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
    $('.btn-fail').click(function () {
        $('#failTaskId').val($(this).data('id'));
        $('#failReason').val('');
        new bootstrap.Modal(document.getElementById('failModal')).show();
    });

    $('#confirmFailBtn').click(function () {
        const id = $('#failTaskId').val();
        const reason = $('#failReason').val();

        if (!reason || reason.trim().length < 5) {
            toastr.warning("Lütfen açıklama giriniz (min 5 karakter).");
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
                toastr.warning(res.message || "Durum bildirildi.");
                $('#failModal').modal('hide');
                setTimeout(() => window.location.reload(), 1000);
            },
            error: function (xhr) {
                handleError(xhr, $btn, originalText);
            }
        });
    });
});