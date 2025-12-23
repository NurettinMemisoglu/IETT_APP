$(function () {

    // === TEMA AYARLAYICI ===
    function setModalTheme(mode) {
        var $modalContent = $('#lineModalContent');
        var $modalTitle = $('#modalTitle');

        // 1. Önceki tüm tema sınıflarını temizle (Yeşil veya Sarı kalıntısı olmasın)
        $modalContent.removeClass('modal-theme-create modal-theme-edit');
        $modalTitle.removeClass('text-success text-warning');

        if (mode === 'create') {
            // YEŞİL SINIFI EKLE (CSS dosyasındaki border çalışacak)
            $modalContent.addClass('modal-theme-create');
            $modalTitle.addClass('text-success');
            $modalTitle.html('<i class="bi bi-plus-lg me-2"></i>Yeni Hat Ekle');
        }
        else if (mode === 'edit') {
            // SARI SINIFI EKLE
            $modalContent.addClass('modal-theme-edit');
            $modalTitle.addClass('text-warning');
            $modalTitle.html('<i class="bi bi-pencil-square me-2"></i>Hattı Düzenle');
        }
    }

    // === BUTTON CLICK EVENTLERİ ===
    $('#addLineBtn').click(function () {
        setModalTheme('create'); // Yeşil yap

        $('#modalFormContainer').html('<div class="text-center py-4"><div class="spinner-border text-success"></div></div>');
        $('#lineModal').modal('show');

        $.get('/Planner/Lines/Create', function (formHtml) {
            $('#modalFormContainer').html(formHtml);
        });
    });

    $(document).on('click', '.edit-line', function () {
        setModalTheme('edit'); // Sarı yap

        $('#modalFormContainer').html('<div class="text-center py-4"><div class="spinner-border text-warning"></div></div>');
        $('#lineModal').modal('show');

        const id = $(this).data('id');
        $.get('/Planner/Lines/Edit/' + id, function (formHtml) {
            $('#modalFormContainer').html(formHtml);
        });
    });

    // === FORM SUBMIT (CREATE / UPDATE) ===
    $(document).on('submit', '#lineForm', function (e) {
        e.preventDefault();

        const payload = {
            Id: $(this).find('[name="Id"]').val(),
            Code: $(this).find('[name="Code"]').val(),
            Name: $(this).find('[name="Name"]').val(),
            LineType: parseInt($(this).find('[name="LineType"]').val()),
            VehicleCount: parseInt($(this).find('[name="VehicleCount"]').val()),
            IsActive: $(this).find('[name="IsActive"]').is(':checked')
        };

        $.ajax({
            url: '/Planner/Lines/Execute',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (res) {
                $('#lineModal').modal('hide');
                // Modal tamamen kapandıktan sonra tabloyu yenile
                $('#lineModal').one('hidden.bs.modal', function () {
                    refreshTable();
                });
            },
            error: function (xhr) {
                console.log(xhr.responseText);
                alert('Kaydetme hatası: ' + (xhr.responseText || ''));
            }
        });
    });

    // === HAT SİL ===
    $(document).on('click', '.delete-line', function () {
        const id = $(this).data('id');
        if (!confirm('Bu hattı silmek istediğinize emin misiniz?')) return;

        $.ajax({
            url: '/Planner/Lines/Delete/' + id,
            type: 'POST',
            success: function () {
                refreshTable();
            },
            error: function () {
                alert('Silme işlemi başarısız.');
            }
        });
    });

    // === ARAMA ===
    $('#searchInput').on('input', function () {
        const term = $(this).val();
        refreshTable(term);
    });

    // === TABLOYU YENİLE ===
    function refreshTable(term = '') {
        $.get('/Planner/Lines/Search', { term }, function (data) {
            $('#linesTableContainer').html(data);
        }).fail(function () {
            alert('Tablo yenilenemedi.');
        });
    }

    // === AKTİF / PASİF TOGGLE (DÜZELTİLDİ) ===
    $(document).on('click', '.toggle-line-active', function (e) {
        e.preventDefault();
        e.stopPropagation();

        var $btn = $(this);
        var $row = $btn.closest('tr'); // Satırı bul

        // Verileri TR üzerindeki data- attribute'lardan oku
        var currentActive = $btn.data('active') === true || $btn.data('active') === 'true';
        var newActive = !currentActive;

        var message = currentActive
            ? "Bu hattı PASİF yapmak istediğinize emin misiniz?"
            : "Bu hattı AKTİF yapmak istediğinize emin misiniz?";

        if (!confirm(message)) return;

        // Payload oluştur - Artık veriler garanti dolu gelecek
        var payload = {
            Id: $row.data('id'),
            Code: $row.data('code'),           // string
            Name: $row.data('name'),           // string
            LineType: parseInt($row.data('linetype')), // int
            VehicleCount: parseInt($row.data('vehiclecount')), // int
            IsActive: newActive
        };

        // Veri kontrolü (Debug için tarayıcı konsoluna basar)
        console.log("Gönderilecek Payload:", payload);

        $.ajax({
            url: '/Planner/Lines/Execute',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function () {
                // Başarılı olursa tabloyu yenile
                refreshTable($('#searchInput').val()); // Varsa arama terimini koru
            },
            error: function (xhr) {
                alert('Durum güncellenemedi: ' + (xhr.responseText || 'Sunucu hatası'));
            }
        });
    });
});