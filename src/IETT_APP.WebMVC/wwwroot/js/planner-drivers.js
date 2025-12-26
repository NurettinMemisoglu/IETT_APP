/* ===============================================
   PLANNER DRIVERS MODULE
   Açıklama: Sürücü listesi, arama ve modal işlemleri
   =============================================== */

$(document).ready(function () {

    // Seçicileri (Selectors) değişkenlerde tutalım, yönetmesi kolay olsun
    const SELECTORS = {
        modal: '#generalModal',
        modalContent: '#generalModalContent',
        modalTrigger: '.btn-modal-trigger',
        search: '#searchInput',
        table: '#driversTable',
        form: '#assignGarageForm'
    };

    // ===============================================
    // 1. MODAL AÇMA İŞLEMİ (AJAX GET)
    // ===============================================
    $(document).on('click', SELECTORS.modalTrigger, function (e) {
        e.preventDefault();

        var url = $(this).attr('href');
        var modalEl = document.querySelector(SELECTORS.modal);

        if (!modalEl) {
            console.error("Modal elementi bulunamadı! ID: " + SELECTORS.modal);
            return;
        }

        // Modalı başlat ve göster
        var modal = new bootstrap.Modal(modalEl);

        // Yükleniyor animasyonu ekle
        $(SELECTORS.modalContent).html(`
            <div class="p-5 text-center">
                <div class="spinner-border text-primary" role="status"></div>
                <div class="mt-2 text-muted small">Veriler yükleniyor, lütfen bekleyiniz...</div>
            </div>
        `);

        modal.show();

        // İçeriği sunucudan çek
        $.get(url)
            .done(function (data) {
                $(SELECTORS.modalContent).html(data);

                // Form validasyonunu (jQuery Unobtrusive) tekrar bağla
                // Bu adım çok önemli, yoksa modal içinde validation çalışmaz
                if ($.validator && $.validator.unobtrusive) {
                    $.validator.unobtrusive.parse(SELECTORS.form);
                }
            })
            .fail(function (xhr) {
                $(SELECTORS.modalContent).html(`
                    <div class="p-4 text-center text-danger">
                        <i class="bi bi-exclamation-triangle-fill fs-1 mb-2"></i>
                        <h5>Bir hata oluştu!</h5>
                        <p class="small text-muted">Sunucu hatası: ${xhr.status}</p>
                        <button type="button" class="btn btn-secondary btn-sm mt-2" data-bs-dismiss="modal">Kapat</button>
                    </div>
                `);
            });
    });

    // ===============================================
    // 2. FORM GÖNDERME İŞLEMİ (AJAX POST)
    // ===============================================
    $(document).on('submit', SELECTORS.form, function (e) {
        e.preventDefault(); // Sayfanın yenilenmesini engelle

        var form = $(this);
        var url = form.attr('action');
        var data = form.serialize();

        // 1. Client-Side Validasyon Kontrolü
        if (!form.valid()) {
            return false; // Form geçerli değilse dur
        }

        // 2. Butonu Kilitle (Çift Tıklamayı Önle) & Loading Göster
        var btn = form.find('button[type="submit"]');
        var originalBtnHtml = btn.html();
        btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span> Kaydediliyor...');

        // 3. AJAX İsteği
        $.ajax({
            type: "POST",
            url: url,
            data: data,
            success: function (response) {
                // Senaryo A: Başarılı (JSON döner)
                if (response.success === true) {
                    $(SELECTORS.modal).modal('hide');

                    // SweetAlert varsa şık bir mesaj göster, yoksa standart alert
                    if (typeof Swal !== 'undefined') {
                        Swal.fire({
                            icon: 'success',
                            title: 'Başarılı!',
                            text: response.message,
                            timer: 1500,
                            showConfirmButton: false
                        }).then(() => {
                            location.reload(); // Tabloyu yenile
                        });
                    } else {
                        // Fallback
                        location.reload();
                    }
                }
                // Senaryo B: Başarısız / Validasyon Hatası (HTML döner)
                else {
                    $(SELECTORS.modalContent).html(response);

                    // HTML değiştiği için validasyonu tekrar bağla
                    if ($.validator && $.validator.unobtrusive) {
                        $.validator.unobtrusive.parse(SELECTORS.form);
                    }
                }
            },
            error: function (xhr) {
                console.error("AJAX Hatası:", xhr);
                // Butonu eski haline getir ki tekrar deneyebilsinler
                btn.prop('disabled', false).html(originalBtnHtml);

                // Kullanıcıya hata mesajı göster (Formun üstüne ekle)
                var errorAlert = `
                    <div class="alert alert-danger alert-dismissible fade show mt-3" role="alert">
                        <strong>Hata!</strong> İşlem sırasında beklenmedik bir sorun oluştu (${xhr.status}).
                        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                    </div>`;

                // Eğer formda zaten alert varsa kaldır, yenisini ekle
                form.find('.alert').remove();
                form.prepend(errorAlert);
            }
        });
    });

    // ===============================================
    // 3. TABLO İÇİ ARAMA (CLIENT-SIDE)
    // ===============================================
    $(SELECTORS.search).on('keyup', function () {
        var value = $(this).val().toLowerCase();
        var rows = $(SELECTORS.table + " tbody tr");
        var hasVisibleRow = false;

        rows.filter(function () {
            var text = $(this).text().toLowerCase();
            var isVisible = text.indexOf(value) > -1;
            $(this).toggle(isVisible);
            if (isVisible) hasVisibleRow = true;
        });

        // Eğer hiç kayıt bulunamadıysa bir mesaj satırı göster (Opsiyonel)
        // Tablonda id="no-records-row" olan gizli bir tr varsa onu açıp kapatabilirsin.
    });

});