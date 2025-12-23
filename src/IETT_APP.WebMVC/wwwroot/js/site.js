$(document).ready(function () {
    console.log("🚀 Site.js Başlatıldı.");

    // ==========================================
    // 📅 GELİŞMİŞ FLATPICKR + INPUT MASK
    // ==========================================
    window.initDatePickers = function () {
        const selector = ".datetime-picker, .date-input-with-icon";
        if (typeof flatpickr !== 'undefined') {
            flatpickr(selector, {
                enableTime: true, dateFormat: "d.m.Y H:i", time_24hr: true, locale: "tr", allowInput: true, disableMobile: "true", minuteIncrement: 1
            });
        }
        $(document).on('input', selector, function (e) {
            if (e.originalEvent.inputType === 'deleteContentBackward') return;
            var input = $(this);
            var cleanVal = input.val().replace(/\D/g, '');
            var formattedVal = '';
            if (cleanVal.length > 0) formattedVal += cleanVal.substring(0, 2);
            if (cleanVal.length >= 2) formattedVal += '.';
            if (cleanVal.length > 2) formattedVal += cleanVal.substring(2, 4);
            if (cleanVal.length >= 4) formattedVal += '.';
            if (cleanVal.length > 4) formattedVal += cleanVal.substring(4, 8);
            if (cleanVal.length >= 8) formattedVal += ' ';
            if (cleanVal.length > 8) formattedVal += cleanVal.substring(8, 10);
            if (cleanVal.length >= 10) formattedVal += ':';
            if (cleanVal.length > 10) formattedVal += cleanVal.substring(10, 12);
            input.val(formattedVal);
        });
        $(document).on('keypress', selector, function (e) {
            if ($(this).val().length >= 16) e.preventDefault();
        });
    };
    initDatePickers();
    $(document).on('shown.bs.modal', function () { initDatePickers(); });

    // ==========================================
    // 🔑 GLOBAL ŞİFRE DEĞİŞTİRME (AJAX)
    // ==========================================
    $(document).on('click', '#btnChangePassword', function () {
        const $btn = $(this);
        const $form = $('#changePasswordForm');
        const formData = {
            CurrentPassword: $form.find('input[name="CurrentPassword"]').val(),
            NewPassword: $form.find('input[name="NewPassword"]').val(),
            ConfirmPassword: $form.find('input[name="ConfirmPassword"]').val()
        };

        if (!formData.CurrentPassword || !formData.NewPassword) { alert("Lütfen tüm alanları doldurunuz."); return; }
        if (formData.NewPassword !== formData.ConfirmPassword) { alert("Yeni şifreler uyuşmuyor."); return; }

        $btn.prop('disabled', true).html('<i class="spinner-border spinner-border-sm"></i> Güncelleniyor...');

        $.ajax({
            url: '/Auth/ChangePassword', type: 'POST', contentType: 'application/json', data: JSON.stringify(formData),
            success: function (res) { alert(res.message || "Şifre başarıyla değiştirildi."); $form[0].reset(); },
            error: function (xhr) {
                let errorMsg = "İşlem başarısız.";
                if (xhr.responseJSON) {
                    if (xhr.responseJSON.errors && Array.isArray(xhr.responseJSON.errors)) errorMsg = xhr.responseJSON.errors.join("\n");
                    else if (xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                }
                alert(errorMsg);
            },
            complete: function () { $btn.prop('disabled', false).html('<i class="bi bi-save"></i> Şifreyi Güncelle'); }
        });
    });

    // ==========================================
    // 🔔 BİLDİRİM UI YÖNETİMİ
    // ==========================================
    function decreaseNotificationCount() {
        let $badge = $('#notificationCount');
        let $header = $('#notificationHeader');
        let count = parseInt($badge.text() || "0");
        if (count > 0) {
            count = count - 1;
            if (count === 0) { $badge.hide(); $header.text("Bildirim Yok"); }
            else { $badge.text(count).show(); $header.text(count + " Yeni Bildirim"); }
        }
    }

    $(document).on('click', '.notification-item', function (e) {
        let url = $(this).attr('href');
        let id = $(this).data('id');
        if (url && url !== '#') {
            if (id) {
                if (navigator.sendBeacon) {
                    let data = new FormData(); data.append('id', id); navigator.sendBeacon('/MyNotifications/MarkRead', data);
                } else { $.post('/MyNotifications/MarkRead', { id: id }); }
            }
            decreaseNotificationCount();
            window.location.href = url;
        }
    });

    $(document).on('click', '.mark-read-btn', function (e) {
        e.preventDefault(); e.stopPropagation();
        let btn = $(this); let id = btn.data('id');
        let card = $('#notify-' + id);
        if (card.length === 0) card = btn.closest('.list-group-item');

        btn.html('<span class="spinner-border spinner-border-sm"></span>'); btn.prop('disabled', true);

        $.post('/MyNotifications/MarkRead', { id: id })
            .done(function () {
                if (card.length) {
                    card.removeClass('notification-unread'); card.css('border-left-color', 'transparent'); card.css('background-color', '#fff'); card.find('.notify-title').css('font-weight', 'normal');
                }
                btn.parent().html('<span class="text-success small fw-bold"><i class="bi bi-check2-all"></i> Okundu</span>');
                decreaseNotificationCount();
            })
            .fail(function () {
                btn.html('<i class="bi bi-exclamation-circle"></i> Hata'); btn.prop('disabled', false);
            });
    });

    // ==========================================
    // 🔔 SIGNALR SİSTEMİ (ARTIK HER ŞEY İÇERİDE)
    // ==========================================
    (async function startSignalR() {
        console.log("🚀 SignalR Script Başlatılıyor...");

        const token = window.currentUserToken;
        if (!token) return;

        const hubUrl = "https://localhost:7254/hubs/notification?access_token=" + encodeURIComponent(token);

        const connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, { withCredentials: true })
            .configureLogging(signalR.LogLevel.Information)
            .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
            .build();

        // 1. Bildirim Geldiğinde
        connection.on("ReceiveNotification", function (title, message, importance, linkUrl, notificationId) {
            var type = importance ? importance.toLowerCase() : "info";

            if (typeof toastr !== 'undefined') {
                toastr.options = { "closeButton": true, "progressBar": true, "positionClass": "toast-top-right", "timeOut": "5000" };
                if (linkUrl) toastr.options.onclick = function () { window.location.href = linkUrl; };
                if (type === "error") toastr.error(message, title);
                else if (type === "warning") toastr.warning(message, title);
                else if (type === "success") toastr.success(message, title);
                else toastr.info(message, title);
            }

            let $badge = $('#notificationCount');
            let $header = $('#notificationHeader');
            if ($badge.length) {
                let count = parseInt($badge.text() || 0) + 1;
                $badge.text(count).show();
                $header.text(count + " Yeni Bildirim");
            }

            // Şoför Sayfası Yenileme
            if (window.location.pathname.includes("/Driver/Tasks")) {
                if (title.includes("Yeni Görev") || title.includes("Ataması") || type === "error") {
                    console.log("⏳ Sayfa yenilenecek...");
                    setTimeout(() => { window.location.reload(); }, 5000);
                }
            }
        });

        // 2. Görev Güncellendiğinde
        connection.on("TaskUpdated", function (taskId) {
            const currentPath = window.location.pathname.toLowerCase();
            if (currentPath.includes("/triptasks") || currentPath.includes("/driver/tasks") || currentPath.endsWith("/driver") || currentPath.endsWith("/driver/") || currentPath.includes("/driver/home")) {
                if ($('.modal.show').length === 0) {
                    console.log("🔄 Veri güncellendi, sayfa yenileniyor...");
                    setTimeout(() => { window.location.reload(); }, 5000);
                }
            }
        });

        // ==========================================
        // 🖼️ 3. PROFİL RESMİ GÜNCELLEME (İÇERİ ALINDI)
        // ==========================================
        connection.on("ProfileImageUpdated", function (newImageUrl) {
            console.log("📸 Profil resmi güncellendi:", newImageUrl);

            // API Adresini Al
            let finalUrl = newImageUrl;
            if (finalUrl && !finalUrl.startsWith('http')) {
                // 🔴 PORT NUMARASINI KONTROL ET
                finalUrl = "https://localhost:7254" + (finalUrl.startsWith('/') ? finalUrl : '/' + finalUrl);
            }

            // Cache kırmak için timestamp
            finalUrl += "?t=" + new Date().getTime();

            // Sayfadaki tüm profil resimlerini bul ve güncelle
            const $images = $('.user-image, .img-circle, #profileImagePreview, #navbarThumbnail, #navbarHeaderImage, .user-image-live');

            $images.attr('src', finalUrl);

            if (typeof toastr !== 'undefined') {
                toastr.info("Profil fotoğrafınız diğer cihazlarda da güncellendi.");
            }
        });

        try {
            await connection.start();
        } catch (err) {
            console.error("SignalR Hatası:", err);
        }
    })();

    // ==========================================
    // 🔗 HASH SCROLL
    // ==========================================
    if (window.location.hash) {
        var targetId = window.location.hash;
        if (targetId.startsWith("#notify-")) {
            var $target = $(targetId);
            if ($target.length) {
                $('html, body').animate({ scrollTop: $target.offset().top - 200 }, 1000);
                $target.addClass('target-highlight');
                setTimeout(() => { $target.removeClass('target-highlight'); history.replaceState(null, null, ' '); }, 3000);
            }
        }
    }

    // ==========================================
    // 🚪 KESİN ÇALIŞAN SIDEBAR KAYDEDİCİ
    // ==========================================
    $('[data-lte-toggle="sidebar"]').on('click', function () {
        // AdminLTE sınıfı değiştirene kadar 300ms bekle
        setTimeout(function () {
            // Şu an kapalı mı?
            var isClosed = $('body').hasClass('sidebar-collapse');
            var state = isClosed ? 'closed' : 'open';

            // 🔥 EN ÖNEMLİ KISIM: path=/
            // Bu olmazsa sayfa değişimlerinde unutur!
            document.cookie = "SidebarStatus=" + state + "; path=/; max-age=31536000";

            console.log("Sidebar Durumu Kaydedildi: " + state);
        }, 300);
    });
});