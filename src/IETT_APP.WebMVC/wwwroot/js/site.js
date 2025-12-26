$(document).ready(function () {
    console.log("🚀 Site.js Başlatıldı.");

    // ==========================================
    // 🍬 1. SWEETALERT2 AYARLARI & TOASTR KÖPRÜSÜ
    // ==========================================

    // Standart Toast Ayarı
    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.addEventListener('mouseenter', Swal.stopTimer)
            toast.addEventListener('mouseleave', Swal.resumeTimer)
        }
    });

    // Toastr Adaptörü (Eski kodların bozulmaması için)
    window.toastr = {
        options: {}, // Eski ayarlara hata vermemesi için boş obje
        success: function (message, title) {
            Toast.fire({ icon: 'success', title: title || 'Başarılı', text: message });
        },
        error: function (message, title) {
            Toast.fire({ icon: 'error', title: title || 'Hata', text: message });
        },
        warning: function (message, title) {
            Toast.fire({ icon: 'warning', title: title || 'Dikkat', text: message });
        },
        info: function (message, title) {
            Toast.fire({ icon: 'info', title: title || 'Bilgi', text: message });
        },
        // clear ve remove fonksiyonlarını boş geçiyoruz ki hata vermesin
        clear: function () { },
        remove: function () { }
    };

    // ==========================================
    // 📨 2. SUNUCU MESAJLARINI KONTROL ET (TempData)
    // ==========================================
    // Layout'a eklediğimiz hidden inputlardan değerleri oku
    var serverSuccess = $('#server-success-msg').val();
    var serverError = $('#server-error-msg').val();

    if (serverSuccess) {
        window.toastr.success(serverSuccess);
    }

    if (serverError) {
        window.toastr.error(serverError);
    }

    // ==========================================
    // 📅 3. GELİŞMİŞ FLATPICKR + INPUT MASK
    // ==========================================
    window.initDatePickers = function () {
        const selector = ".datetime-picker, .date-input-with-icon";

        if (typeof flatpickr !== 'undefined') {
            flatpickr(selector, {
                enableTime: true,
                dateFormat: "d.m.Y H:i",
                time_24hr: true,
                locale: "tr",
                allowInput: true,
                disableMobile: "true",
                minuteIncrement: 1,
                // 👇 BURASI GÜNCELLENDİ 👇
                onReady: function (selectedDates, dateStr, instance) {
                    // Elementler oluşmamışsa dur
                    if (!instance.hourElement || !instance.minuteElement) return;

                    const $hour = $(instance.hourElement);
                    const $minute = $(instance.minuteElement);

                    // SAAT INPUT KONTROLÜ
                    $hour.on('input keyup', function (e) {
                        let val = $(this).val();

                        // 1. Sadece rakam olmasına izin ver (Opsiyonel güvenlik)
                        val = val.replace(/\D/g, '');

                        // 2. Eğer 2 karakterden fazlaysa, anında kırp
                        if (val.length > 2) {
                            val = val.substring(0, 2);
                            $(this).val(val); // Inputu güncelle
                        }

                        // 3. Eğer tam 2 karakter olduysa dakikaya atla
                        if (val.length === 2) {
                            $minute.focus();
                            // Dakikadaki mevcut "00" veya sayıyı seç (üzerine yazmak için)
                            $minute.select();
                        }
                    });

                    // DAKİKA INPUT KONTROLÜ (Sadece sınırlandırma)
                    $minute.on('input keyup', function () {
                        let val = $(this).val().replace(/\D/g, '');
                        if (val.length > 2) {
                            $(this).val(val.substring(0, 2));
                        }
                    });

                    // Kullanıcı tıkladığında içindekini seç (Kolay düzeltme için)
                    $hour.on('focus click', function () { $(this).select(); });
                    $minute.on('focus click', function () { $(this).select(); });
                }
                // 👆 BURASI GÜNCELLENDİ 👆
            });
        }

        // ... (Alttaki manuel maskeleme kodları aynen kalabilir) ...
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

    // ==========================================
    // 🔑 4. GLOBAL ŞİFRE DEĞİŞTİRME (AJAX)
    // ==========================================
    $(document).on('click', '#btnChangePassword', function () {
        const $btn = $(this);
        const $form = $('#changePasswordForm');
        const formData = {
            CurrentPassword: $form.find('input[name="CurrentPassword"]').val(),
            NewPassword: $form.find('input[name="NewPassword"]').val(),
            ConfirmPassword: $form.find('input[name="ConfirmPassword"]').val()
        };

        if (!formData.CurrentPassword || !formData.NewPassword) { window.toastr.warning("Lütfen tüm alanları doldurunuz."); return; }
        if (formData.NewPassword !== formData.ConfirmPassword) { window.toastr.warning("Yeni şifreler uyuşmuyor."); return; }

        $btn.prop('disabled', true).html('<i class="spinner-border spinner-border-sm"></i> Güncelleniyor...');

        $.ajax({
            url: '/Auth/ChangePassword', type: 'POST', contentType: 'application/json', data: JSON.stringify(formData),
            success: function (res) {
                window.toastr.success(res.message || "Şifre başarıyla değiştirildi.");
                $form[0].reset();
            },
            error: function (xhr) {
                let errorMsg = "İşlem başarısız.";
                if (xhr.responseJSON) {
                    if (xhr.responseJSON.errors && Array.isArray(xhr.responseJSON.errors)) errorMsg = xhr.responseJSON.errors.join("\n");
                    else if (xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                }
                window.toastr.error(errorMsg);
            },
            complete: function () { $btn.prop('disabled', false).html('<i class="bi bi-save"></i> Şifreyi Güncelle'); }
        });
    });

    // ==========================================
    // 🔔 5. BİLDİRİM UI YÖNETİMİ
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
    // 📡 6. SIGNALR SİSTEMİ
    // ==========================================
    (async function startSignalR() {
        console.log("🚀 SignalR Script Başlatılıyor...");

        const token = window.currentUserToken; // Token Layout'ta window objesine atanmış olmalı
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

            // Not: Köprü sayesinde toastr artık SweetAlert2 kullanıyor.
            // Ancak click eventi SweetAlert'te farklı handle edilir.
            // Basitlik için sadece mesajı gösteriyoruz, linke gitme manuel yapılır
            // veya Swal config ile güncellenebilir.

            if (type === "error") window.toastr.error(message, title);
            else if (type === "warning") window.toastr.warning(message, title);
            else if (type === "success") window.toastr.success(message, title);
            else window.toastr.info(message, title);

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

        // 3. Profil Resmi Güncelleme
        connection.on("ProfileImageUpdated", function (newImageUrl) {
            console.log("📸 Profil resmi güncellendi:", newImageUrl);
            let finalUrl = newImageUrl;
            if (finalUrl && !finalUrl.startsWith('http')) {
                finalUrl = "https://localhost:7254" + (finalUrl.startsWith('/') ? finalUrl : '/' + finalUrl);
            }
            finalUrl += "?t=" + new Date().getTime();
            const $images = $('.user-image, .img-circle, #profileImagePreview, #navbarThumbnail, #navbarHeaderImage, .user-image-live');
            $images.attr('src', finalUrl);

            window.toastr.info("Profil fotoğrafınız diğer cihazlarda da güncellendi.");
        });

        try {
            await connection.start();
        } catch (err) {
            console.error("SignalR Hatası:", err);
        }
    })();

    // ==========================================
    // 🔗 7. HASH SCROLL
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
    // 🚪 8. SIDEBAR KAYDEDİCİ
    // ==========================================
    $('[data-lte-toggle="sidebar"]').on('click', function () {
        setTimeout(function () {
            var isClosed = $('body').hasClass('sidebar-collapse');
            var state = isClosed ? 'closed' : 'open';
            document.cookie = "SidebarStatus=" + state + "; path=/; max-age=31536000";
            console.log("Sidebar Durumu Kaydedildi: " + state);
        }, 300);
    });


});