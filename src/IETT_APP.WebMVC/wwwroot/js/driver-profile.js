$(function () {
    const operatorId = $('input[name="Id"]').val();

    // === 1. PROFİL GÜNCELLEME (MEVCUT KODUN) ===
    $('#saveProfileBtn').off('click').on('click', function (e) {
        e.preventDefault();
        const $btn = $(this);
        $btn.prop('disabled', true).html('<i class="bi bi-hourglass-split"></i> Kaydediliyor...');

        const formData = {
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
            data: JSON.stringify(formData),
            success: function (res) {
                $('#updateProfileModal').modal('hide');
                alert("Bilgileriniz başarıyla güncellendi.");
                window.location.href = window.location.href;
            },
            error: function (xhr) {
                alert("Hata: " + (xhr.responseJSON?.message || "Güncelleme başarısız."));
                $btn.prop('disabled', false).text('Değişiklikleri Kaydet');
            }
        });
    });

    // === 2. PROFİL RESİMİ YÜKLEME ===
    $('#imageInput').off('change').on('change', function () {
        const file = this.files[0];
        if (!file) return;

        if (file.size > 5 * 1024 * 1024) {
            alert("Dosya boyutu 5MB'dan büyük olamaz.");
            return;
        }

        const formData = new FormData();
        // operatorId'nin dışarıda doğru bir şekilde tanımlandığını varsayıyoruz.
        formData.append('id', operatorId);
        formData.append('photo', file);

        // Tüm resim elementlerini tek bir jQuery objesinde topla (Hem önizleme hem navbar/dropdown)
        const $imagesToUpdate = $('#profileImagePreview, #navbarThumbnail, #navbarHeaderImage');

        // Yükleniyor efekti (Resimleri soluklaştır)
        $imagesToUpdate.css('opacity', '0.5');

        $.ajax({
            url: '/Driver/Profile/UploadPhoto/' + operatorId,
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (res) {
                // Yeni resim yolunu Cache'i kırmak için timestamp ile hazırla
                // NOT: Resim yolu artık sunucuda güncellenen claim'den alınacak (res.newUrl)
                const newSrc = "https://localhost:7254" + res.newUrl + "?t=" + new Date().getTime();

                // Tüm resim elementlerini güncelle
                $('#profileImagePreview').attr('src', newSrc);
                $('#navbarThumbnail').attr('src', newSrc);      // Navbar küçük resim
                $('#navbarHeaderImage').attr('src', newSrc);    // Dropdown büyük resim

                // Opaklığı geri getir
                $imagesToUpdate.css('opacity', '1');

                alert("Profil fotoğrafı başarıyla güncellendi.");
            },
            error: function (xhr) {
                // Hata durumunda opaklığı geri getir
                $imagesToUpdate.css('opacity', '1');
                alert("Hata: " + (xhr.responseJSON?.message || "Resim yüklenemedi."));
            }
        });
    });

    // === 3. PROFİL OLUŞTURMA (CREATE) - GÜNCELLENMİŞ ===
    $('#btnCreateSave').off('click').on('click', function (e) {
        e.preventDefault();

        // Form elementini seç
        var $form = $('#createProfileForm');

        // Validasyon kontrolü (jQuery Validate varsa)
        if ($form.valid && !$form.valid()) {
            // Validasyon hatası varsa dur
            // İlk hataya odaklan
            $form.find(".input-validation-error").first().focus();
            return;
        }

        const $btn = $(this);
        const originalText = $btn.html();

        // Butonu kilitle (Kullanıcı tekrar basamasın)
        $btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span> Kaydediliyor...');

        // Form verilerini al
        // Not: $form[0] HTML elementini verir
        var formData = new FormData($form[0]);

        // Checkbox Düzeltmesi (True, False sorununu çözmek için)
        var isChronic = $('input[name="HasChronicDisease"]').is(':checked');
        formData.delete('HasChronicDisease');
        formData.append('HasChronicDisease', isChronic);

        formData.delete('__Invariant');

        $.ajax({
            url: '/Driver/Profile/Create',
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (res) {
                alert(res.message);
                if (res.redirectUrl) {
                    window.location.href = res.redirectUrl;
                } else {
                    window.location.href = '/Driver/Profile';
                }
            },
            error: function (xhr) {
                let errorMsg = "Bir hata oluştu.";
                if (xhr.responseJSON) {
                    if (xhr.responseJSON.errors && Array.isArray(xhr.responseJSON.errors)) {
                        errorMsg = xhr.responseJSON.errors.join("\n");
                    } else if (xhr.responseJSON.message) {
                        errorMsg = xhr.responseJSON.message;
                    }
                } else if (xhr.responseText) {
                    console.log("Sunucu Hatası:", xhr.responseText);
                }

                alert(errorMsg);

                // Hata durumunda butonu tekrar aç
                $btn.prop('disabled', false).html(originalText);
            }
        });
    });
});