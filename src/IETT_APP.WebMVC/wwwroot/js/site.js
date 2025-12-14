$(document).ready(function () {
    console.log("🚀 Site.js Başlatıldı.");

    // ==========================================
    // 📅 GELİŞMİŞ FLATPICKR + INPUT MASK (OTOMATİK NOKTALAMA)
    // ==========================================
    window.initDatePickers = function () {
        const selector = ".datetime-picker, .date-input-with-icon";

        // 1. Önce Flatpickr'ı başlat
        if (typeof flatpickr !== 'undefined') {
            flatpickr(selector, {
                enableTime: true,
                dateFormat: "d.m.Y H:i", // Format: 14.12.2025 14:30
                time_24hr: true,
                locale: "tr",
                allowInput: true,       // Elle yazmaya izin ver
                disableMobile: "true",
                minuteIncrement: 1
            });
        }

        // 2. Input Maskeleme Mantığı (Otomatik Nokta ve İki Nokta)
        $(document).on('input', selector, function (e) {
            var input = $(this);
            var val = input.val();

            // Eğer silme tuşuna basılıyorsa (backspace) müdahale etme
            if (e.originalEvent.inputType === 'deleteContentBackward') return;

            // Sadece rakamları al, geri kalan her şeyi temizle
            var cleanVal = val.replace(/\D/g, '');

            // Formatı oluştur (dd.MM.yyyy HH:mm)
            var formattedVal = '';

            if (cleanVal.length > 0) {
                // Gün
                formattedVal += cleanVal.substring(0, 2);
            }
            if (cleanVal.length >= 2) {
                formattedVal += '.'; // İlk nokta
            }
            if (cleanVal.length > 2) {
                // Ay
                formattedVal += cleanVal.substring(2, 4);
            }
            if (cleanVal.length >= 4) {
                formattedVal += '.'; // İkinci nokta
            }
            if (cleanVal.length > 4) {
                // Yıl
                formattedVal += cleanVal.substring(4, 8);
            }
            if (cleanVal.length >= 8) {
                formattedVal += ' '; // Tarih ve Saat arası boşluk
            }
            if (cleanVal.length > 8) {
                // Saat
                formattedVal += cleanVal.substring(8, 10);
            }
            if (cleanVal.length >= 10) {
                formattedVal += ':'; // Saat iki nokta
            }
            if (cleanVal.length > 10) {
                // Dakika
                formattedVal += cleanVal.substring(10, 12);
            }

            // Input değerini güncelle
            input.val(formattedVal);
        });

        // 3. Maksimum karakter sınırını zorla (Fazla basılmasını engelle)
        $(document).on('keypress', selector, function (e) {
            if ($(this).val().length >= 16) {
                e.preventDefault();
            }
        });
    };

    // Fonksiyonu çalıştır
    initDatePickers();

    // Modal açılınca tekrar çalıştır
    $(document).on('shown.bs.modal', function () {
        initDatePickers();
    });


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

        if (!formData.CurrentPassword || !formData.NewPassword) {
            alert("Lütfen tüm alanları doldurunuz.");
            return;
        }
        if (formData.NewPassword !== formData.ConfirmPassword) {
            alert("Yeni şifreler uyuşmuyor.");
            return;
        }

        $btn.prop('disabled', true).html('<i class="spinner-border spinner-border-sm"></i> Güncelleniyor...');

        $.ajax({
            url: '/Auth/ChangePassword',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function (res) {
                alert(res.message || "Şifre başarıyla değiştirildi.");
                $form[0].reset();
            },
            error: function (xhr) {
                let errorMsg = "İşlem başarısız.";
                if (xhr.responseJSON) {
                    if (xhr.responseJSON.errors && Array.isArray(xhr.responseJSON.errors)) {
                        errorMsg = xhr.responseJSON.errors.join("\n");
                    } else if (xhr.responseJSON.message) {
                        errorMsg = xhr.responseJSON.message;
                    }
                }
                alert(errorMsg);
            },
            complete: function () {
                $btn.prop('disabled', false).html('<i class="bi bi-save"></i> Şifreyi Güncelle');
            }
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
            if (count === 0) {
                $badge.hide();
                $header.text("Bildirim Yok");
            } else {
                $badge.text(count).show();
                $header.text(count + " Yeni Bildirim");
            }
        }
    }

    $(document).on('click', '.notification-item', function (e) {
        let url = $(this).attr('href');
        let id = $(this).data('id');

        if (url && url !== '#') {
            if (id) {
                if (navigator.sendBeacon) {
                    let data = new FormData();
                    data.append('id', id);
                    navigator.sendBeacon('/MyNotifications/MarkRead', data);
                } else {
                    $.post('/MyNotifications/MarkRead', { id: id });
                }
            }
            decreaseNotificationCount();
            window.location.href = url;
        }
    });

    $(document).on('click', '.mark-read-btn', function (e) {
        e.preventDefault();
        e.stopPropagation();

        let btn = $(this);
        let id = btn.data('id');

        let card = $('#notify-' + id);
        if (card.length === 0) {
            card = btn.closest('.list-group-item');
        }

        btn.html('<span class="spinner-border spinner-border-sm"></span>');
        btn.prop('disabled', true);

        $.post('/MyNotifications/MarkRead', { id: id })
            .done(function () {
                if (card.length) {
                    card.removeClass('notification-unread');
                    card.css('border-left-color', 'transparent');
                    card.css('background-color', '#fff');
                    card.find('.notify-title').css('font-weight', 'normal');
                }
                btn.parent().html('<span class="text-success small fw-bold"><i class="bi bi-check2-all"></i> Okundu</span>');
                decreaseNotificationCount();
            })
            .fail(function () {
                btn.html('<i class="bi bi-exclamation-circle"></i> Hata');
                btn.prop('disabled', false);
            });
    });

    // ==========================================
    // 🔔 SIGNALR BİLDİRİM SİSTEMİ
    // ==========================================

    (async function startSignalR() {
        console.log("🚀 SignalR Script Başlatılıyor...");

        const token = window.currentUserToken;
        if (!token) {
            return;
        }

        const hubUrl = "https://localhost:7254/hubs/notification?access_token=" + encodeURIComponent(token);

        const connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, { withCredentials: true })
            .configureLogging(signalR.LogLevel.Information)
            .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
            .build();

        connection.on("ReceiveNotification", function (title, message, importance, linkUrl, notificationId) {
            var type = importance ? importance.toLowerCase() : "info";

            // Toastr
            if (typeof toastr !== 'undefined') {
                toastr.options = {
                    "closeButton": true,
                    "progressBar": true,
                    "positionClass": "toast-top-right",
                    "timeOut": "7000",
                };
                if (linkUrl) {
                    toastr.options.onclick = function () { window.location.href = linkUrl; };
                }

                if (type === "error") toastr.error(message, title);
                else if (type === "warning") toastr.warning(message, title);
                else if (type === "success") toastr.success(message, title);
                else toastr.info(message, title);
            }

            // Zil Sayacı
            let $badge = $('#notificationCount');
            let $header = $('#notificationHeader');
            if ($badge.length) {
                let count = parseInt($badge.text() || 0) + 1;
                $badge.text(count).show();
                $header.text(count + " Yeni Bildirim");
            }

            // Dropdown Listeye Ekle
            if ($('#notificationList').length) {
                let iconColorClass = "text-primary";
                if (type === "error") iconColorClass = "text-danger";
                else if (type === "warning") iconColorClass = "text-warning";
                else if (type === "success") iconColorClass = "text-success";

                let dropdownHtml = `
                <a href="/MyNotifications/Index#notify-${notificationId}" class="dropdown-item notification-item" data-id="${notificationId}">
                    <div class="d-flex align-items-start">
                        <div class="flex-shrink-0 me-3">
                            <div class="notification-icon-box">
                                <i class="bi bi-bell-fill fs-5 ${iconColorClass}"></i>
                            </div>
                        </div>
                        <div class="flex-grow-1">
                            <div class="d-flex justify-content-between align-items-center mb-1">
                                <span class="notification-title" style="font-weight: 600;">${title}</span>
                                <span class="notification-time small text-muted">Şimdi</span>
                            </div>
                            <p class="notification-msg mb-0 text-muted small">${message.substring(0, 60)}${message.length > 60 ? '...' : ''}</p>
                        </div>
                    </div>
                </a>
                <div class="dropdown-divider m-0"></div>`;
                $('#notificationList').prepend(dropdownHtml);
            }

            // Bildirimler Sayfasına Ekle
            const fullList = $('#fullNotificationList');
            if (fullList.length) {
                $('#emptyNotificationState').hide();
                let borderClass = "border-start border-4 ";
                if (type === "error") borderClass += "border-danger";
                else if (type === "warning") borderClass += "border-warning";
                else borderClass += "border-info";

                let iconClass = "text-primary";
                if (type === "error") iconClass = "text-danger";
                else if (type === "warning") iconClass = "text-warning";

                let pageHtml = `
                    <div class="card notification-card p-3 notification-unread mb-2 ${borderClass}" id="notify-${notificationId}" style="display:none;">
                        <div class="d-flex align-items-start">
                            <div class="notify-icon-box me-3 mt-1">
                                <i class="bi bi-info-circle-fill fs-4 ${iconClass}"></i>
                            </div>
                            <div class="flex-grow-1">
                                <div class="d-flex justify-content-between align-items-center mb-1">
                                    <h6 class="notify-title mb-0" style="font-weight: 600;">${title}</h6>
                                    <span class="notify-time small text-muted"><i class="bi bi-clock me-1"></i> Şimdi</span>
                                </div>
                                <p class="notify-body m-0 text-muted">${message}</p>
                            </div>
                            <div class="ms-3 d-flex flex-column align-items-end gap-2 notify-actions">
                                ${linkUrl ? `<a href="${linkUrl}" class="btn btn-sm btn-outline-primary rounded-pill px-3 view-btn" data-id="${notificationId}">İncele</a>` : ''}
                                <button class="btn btn-sm btn-light text-muted border rounded-pill px-3 mark-read-btn" data-id="${notificationId}" title="Okundu işaretle">
                                    <i class="bi bi-check2"></i>
                                </button>
                            </div>
                        </div>
                    </div>
                `;
                $(pageHtml).prependTo(fullList).fadeIn(500);
            }

            // Şoför Sayfası Yenileme
            if (window.location.pathname.includes("/Driver/Tasks")) {
                if (title.includes("Yeni Görev") || title.includes("Ataması") || type === "error") {
                    console.log("⏳ Sayfa yenilenecek, bildirimin okunması bekleniyor...");
                    setTimeout(() => {
                        window.location.reload();
                    }, 7000);
                }
            }
        });

        connection.on("TaskUpdated", function (taskId) {
            if (window.location.pathname.includes("/TripTasks") || window.location.pathname.includes("/Driver/Tasks")) {
                if ($('.modal.show').length === 0) {
                    setTimeout(() => { window.location.reload(); }, 1000);
                }
            }
        });

        try {
            await connection.start();
        } catch (err) {
            console.error("SignalR Hatası:", err);
        }
    })();

    // ==========================================
    // 🔗 HASH SCROLL & HIGHLIGHT
    // ==========================================
    if (window.location.hash) {
        var targetId = window.location.hash;
        if (targetId.startsWith("#notify-")) {
            var $target = $(targetId);
            if ($target.length) {
                $('html, body').animate({
                    scrollTop: $target.offset().top - 200
                }, 1000);
                $target.addClass('target-highlight');
                setTimeout(() => {
                    $target.removeClass('target-highlight');
                    history.replaceState(null, null, ' ');
                }, 3000);
            }
        }
    }
});