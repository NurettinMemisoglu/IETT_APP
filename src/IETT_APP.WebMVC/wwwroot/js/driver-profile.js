$(document).ready(function () {

    // ============================================================
    // 1. ORTAK AYARLAR VE GÖRÜNÜRLÜK FONKSİYONLARI
    // ============================================================

    // --- KRONİK RAHATSIZLIK GÖSTER/GİZLE MANTIĞI ---
    function toggleChronicDetails() {
        const isChecked = $('#chronicCheck').is(':checked');
        const $container = $('#chronicDetails');
        const $noteInput = $('textarea[name="HealthNotes"]');

        if (isChecked) {
            $container.slideDown();
        } else {
            $container.slideUp();
            $noteInput.val('');
        }
    }

    // Sayfa yüklendiğinde çalıştır
    toggleChronicDetails();

    // Checkbox değiştiğinde çalıştır
    $('#chronicCheck').on('change', function () {
        toggleChronicDetails();
    });

    // --- DATEPICKER AYARLARI ---
    if (typeof flatpickr !== 'undefined' && $(".datepicker").length > 0) {
        flatpickr(".datepicker", {
            locale: "tr",
            dateFormat: "Y-m-d",
            altInput: true,
            altFormat: "d F Y",
            allowInput: false,
            disableMobile: true
        });
    }

    // ============================================================
    // 2. CREATE SAYFASI (PROFİL OLUŞTURMA - AJAX)
    // ============================================================
    const $btnCreate = $('#btnCreateSave, #btnInlineSave');

    if ($btnCreate.length > 0) {
        $btnCreate.off('click').on('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            const $btn = $(this);
            const originalText = $btn.html();
            const $form = $('#createProfileForm');

            // Validasyon
            if ($form.valid && !$form.valid()) {
                if (typeof toastr !== 'undefined') toastr.warning("Lütfen zorunlu alanları doldurunuz.");
                return;
            }

            $btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>İşleniyor...');

            const formData = new FormData($form[0]);

            // --- CREATE MANTIĞI ---

            // 1. Checkbox seçili mi?
            let isChronic = $('#chronicCheck').is(':checked');

            // 2. Not alanını al
            let rawNotes = $form.find('textarea[name="HealthNotes"]').val();
            const healthNotes = (rawNotes || "").trim();

            // 3. KURAL: Eğer seçili ama not boşsa -> Hastalık yok say.
            if (isChronic && healthNotes === "") {
                isChronic = false;
            }

            // 4. Form verisini güncelle
            formData.delete('HasChronicDisease');
            formData.append('HasChronicDisease', isChronic);

            if (!isChronic) {
                formData.set('HealthNotes', '');
            }

            $.ajax({
                url: '/Driver/Profile/Create',
                type: 'POST',
                data: formData,
                contentType: false,
                processData: false,
                timeout: 600000, // 🔥 EKLENEN KISIM: 10 Dakika bekleme süresi (Büyük dosyalar için şart)
                success: function (data) {
                    if (typeof Swal !== 'undefined') {
                        Swal.fire({
                            icon: 'success',
                            title: 'Başarılı!',
                            // Controller'dan gelen "message" alanını kullanıyoruz
                            text: data.message || "Profil oluşturuldu.",
                            timer: 2000,
                            showConfirmButton: false
                        }).then(() => {
                            // Controller'dan gelen "redirectUrl" alanını kullanıyoruz
                            window.location.href = data.redirectUrl ? data.redirectUrl : '/Driver/Profile/Index';
                        });
                    } else {
                        window.location.href = data.redirectUrl || '/Driver/Profile/Index';
                    }
                },
                error: function (xhr) {
                    $btn.prop('disabled', false).html(originalText);
                    let userMessage = "Bir hata oluştu.";
                    try {
                        if (xhr.responseJSON) {
                            if (xhr.responseJSON.message) userMessage = xhr.responseJSON.message;
                            if (xhr.responseJSON.errors && Array.isArray(xhr.responseJSON.errors)) {
                                userMessage += "\n" + xhr.responseJSON.errors.join("\n");
                            }
                        }
                    } catch (err) { console.error(err); }

                    if (typeof Swal !== 'undefined') {
                        Swal.fire({ icon: 'error', title: 'Hata!', html: userMessage.replace(/\n/g, '<br>') });
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

            let isChronic = $('#chronicCheck').is(':checked');
            let healthNotes = $('textarea[name="HealthNotes"]').val() || "";

            if (isChronic && healthNotes.trim() === "") {
                isChronic = false;
                healthNotes = "";
            }

            const jsonData = {
                Id: operatorId,
                PhoneNumber: $('input[name="PhoneNumber"]').val(),
                Email: $('input[name="Email"]').val(),
                Address: $('textarea[name="Address"]').val(),
                EmergencyContactName: $('input[name="EmergencyContactName"]').val(),
                EmergencyContactPhone: $('input[name="EmergencyContactPhone"]').val(),
                BloodType: $('select[name="BloodType"]').val(),
                HasChronicDisease: isChronic,
                HealthNotes: healthNotes
            };

            $.ajax({
                url: '/Driver/Profile/Update',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(jsonData),
                success: function (res) {
                    $('#updateProfileModal').modal('hide');
                    if (typeof toastr !== 'undefined') toastr.success(res.message || "Bilgileriniz başarıyla güncellendi.");
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
                    // Controller "newUrl" dönüyor
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

                    if (typeof toastr !== 'undefined') toastr.success(res.message || "Profil fotoğrafı güncellendi.");
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