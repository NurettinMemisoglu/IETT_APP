$(document).ready(function () {

    // ============================================================
    // 1. ORTAK AYARLAR (DATEPICKER)
    // ============================================================
    // Flatpickr varsa ve sayfada .datepicker class'lı eleman varsa çalıştır
    if (typeof flatpickr !== 'undefined' && $(".datepicker").length > 0) {
        flatpickr(".datepicker", {
            locale: "tr",
            dateFormat: "Y-m-d", // Sunucu formatı
            altInput: true,
            altFormat: "d F Y",  // Görünen format
            allowInput: false,
            disableMobile: true
        });
    }

    // ============================================================
    // 2. CREATE SAYFASI (PROFİL OLUŞTURMA - AJAX)
    // ============================================================
    // Not: Create.cshtml dosyasındaki buton ID'si "btnInlineSave" veya "btnCreateSave" olabilir.
    // İkisini de kapsayacak şekilde seçim yapalım.
    const $btnCreate = $('#btnCreateSave, #btnInlineSave');

    if ($btnCreate.length > 0) {

        // A. KRONİK RAHATSIZLIK GÖSTER/GİZLE
        function toggleChronicDetails() {
            if ($('#chronicCheck').is(':checked')) {
                $('#chronicDetails').slideDown();
            } else {
                $('#chronicDetails').slideUp();
                // Kapandığında içini temizlemek istersen:
                // $('#chronicDetails input').val(''); 
            }
        }

        // Sayfa yüklendiğinde çalıştır (Hata sonrası dönüşlerde açık kalsın diye)
        toggleChronicDetails();

        // Değişiklik olduğunda çalıştır
        $('#chronicCheck').on('change', function () {
            toggleChronicDetails();
        });

        // B. KAYDETME BUTONU (AJAX)
        $btnCreate.off('click').on('click', function (e) {
            // 1. Tarayıcı varsayılan işlemini durdur
            e.preventDefault();
            e.stopPropagation();

            const $btn = $(this);
            const originalText = $btn.html();
            const $form = $('#createProfileForm');

            // 2. jQuery Validation Kontrolü
            if ($form.valid && !$form.valid()) {
                if (typeof toastr !== 'undefined') toastr.warning("Lütfen zorunlu alanları doldurunuz.");
                // İlk hatalı alana odaklan
                $form.find('.input-validation-error').first().focus();
                return;
            }

            console.log("Validasyon geçti, AJAX isteği hazırlanıyor...");

            // 3. Butonu Kilitle
            $btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>İşleniyor...');

            // 4. Form Verisini Hazırla
            const formData = new FormData($form[0]);

            formData.delete('HasChronicDisease');

            // Sonra sadece doğru olan değeri (True/False) ekliyoruz
            const isChecked = $('#chronicCheck').is(':checked');
            formData.append('HasChronicDisease', isChecked);

            $.ajax({
                url: '/Driver/Profile/Create',
                type: 'POST',
                data: formData,
                contentType: false, // Multipart için false olmalı
                processData: false, // Dosya gönderimi için false olmalı
                success: function (data) {
                    console.log("İşlem Başarılı:", data);

                    if (typeof Swal !== 'undefined') {
                        Swal.fire({
                            icon: 'success',
                            title: 'Başarılı!',
                            text: data.message || "Profil oluşturuldu.",
                            timer: 2000,
                            showConfirmButton: false
                        }).then(() => {
                            window.location.href = data.redirectUrl ? data.redirectUrl : '/Driver/Profile/Index';
                        });
                    } else {
                        alert("Başarılı: " + (data.message || "İşlem tamamlandı."));
                        window.location.href = data.redirectUrl || '/Driver/Profile/Index';
                    }
                },
                error: function (xhr) {
                    console.error("Hata:", xhr);

                    // Butonu eski haline getir
                    $btn.prop('disabled', false).html(originalText);

                    // Hata Mesajını Ayıkla
                    let userMessage = "Bir hata oluştu.";

                    try {
                        if (xhr.responseJSON) {
                            if (xhr.responseJSON.message) {
                                userMessage = xhr.responseJSON.message;
                            }
                            if (xhr.responseJSON.errors && Array.isArray(xhr.responseJSON.errors)) {
                                userMessage += "\n" + xhr.responseJSON.errors.join("\n");
                            }
                        } else if (xhr.responseText) {
                            if (xhr.responseText.length < 500) userMessage = xhr.responseText;
                        }
                    } catch (err) {
                        console.error("Hata parse edilemedi", err);
                    }

                    // Hatalı (Kırmızı) SweetAlert
                    if (typeof Swal !== 'undefined') {
                        Swal.fire({
                            icon: 'error',
                            title: 'Hata!',
                            html: userMessage.replace(/\n/g, '<br>')
                        });
                    } else {
                        alert("HATA:\n" + userMessage);
                    }
                }
            });
        });
    }

    // ============================================================
    // 3. INDEX SAYFASI (PROFİL GÜNCELLEME & FOTOĞRAF)
    // ============================================================
    const $operatorInput = $('input[name="Id"]');

    if ($operatorInput.length > 0) {
        const operatorId = $operatorInput.val();

        // A. Profil Bilgileri Güncelleme
        $('#saveProfileBtn').off('click').on('click', function (e) {
            e.preventDefault();
            const $btn = $(this);
            const originalText = $btn.text();

            $btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span> Kaydediliyor...');

            const jsonData = {
                Id: operatorId,
                PhoneNumber: $('input[name="PhoneNumber"]').val(),
                Address: $('textarea[name="Address"]').val(),
                EmergencyContactName: $('input[name="EmergencyContactName"]').val(),
                EmergencyContactPhone: $('input[name="EmergencyContactPhone"]').val(),
                BloodType: $('select[name="BloodType"]').val(),
                HasChronicDisease: $('input[name="HasChronicDisease"]').is(':checked'),
                HealthNotes: $('textarea[name="HealthNotes"]').val()
            };

            $.ajax({
                url: '/Driver/Profile/Update',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(jsonData),
                success: function (res) {
                    $('#updateProfileModal').modal('hide');
                    if (typeof toastr !== 'undefined') toastr.success("Bilgileriniz başarıyla güncellendi.");
                    else alert("Bilgileriniz güncellendi.");
                    setTimeout(() => window.location.reload(), 1500);
                },
                error: function (xhr) {
                    let msg = xhr.responseJSON?.message || "Güncelleme başarısız.";
                    if (typeof toastr !== 'undefined') toastr.error(msg);
                    else alert(msg);
                    $btn.prop('disabled', false).text(originalText);
                }
            });
        });

        // B. Profil Resmi Yükleme
        $('#imageInput').off('change').on('change', function () {
            const file = this.files[0];
            if (!file) return;

            if (file.size > 5 * 1024 * 1024) {
                if (typeof toastr !== 'undefined') toastr.warning("Dosya boyutu 5MB'dan büyük olamaz.");
                return;
            }

            const formData = new FormData();
            formData.append('id', operatorId);
            formData.append('photo', file);

            const $imagesToUpdate = $('#profileImagePreview, .user-image, #navbarThumbnail, #navbarHeaderImage, .user-image-live');
            $imagesToUpdate.css('opacity', '0.5');

            $.ajax({
                url: '/Driver/Profile/UploadPhoto',
                type: 'POST',
                data: formData,
                contentType: false,
                processData: false,
                success: function (res) {
                    const timestamp = new Date().getTime();
                    let rawPath = res.newUrl || res.path || res.Path;

                    if (!rawPath) {
                        if (typeof toastr !== 'undefined') toastr.error("Resim yolu alınamadı.");
                        $imagesToUpdate.css('opacity', '1');
                        return;
                    }

                    let finalSrc = rawPath;
                    if (!rawPath.startsWith('http')) {
                        if (!rawPath.startsWith('/')) rawPath = '/' + rawPath;
                        finalSrc = window.location.origin + rawPath;
                    }
                    finalSrc += "?t=" + timestamp;

                    $imagesToUpdate.attr('src', finalSrc);
                    $imagesToUpdate.css('opacity', '1');

                    if (typeof toastr !== 'undefined') toastr.success("Profil fotoğrafı güncellendi.");
                },
                error: function (xhr) {
                    $imagesToUpdate.css('opacity', '1');
                    let msg = xhr.responseJSON?.message || "Resim yüklenemedi.";
                    if (typeof toastr !== 'undefined') toastr.error(msg);
                }
            });
        });
    }
});