$(document).ready(function () {

    // === HAT EKLE ===
    $('#addLineBtn').click(function () {
        $.get('/Planner/Lines/Create', function (formHtml) {
            $('#modalTitle').text('Yeni Hat Ekle');
            $('#modalBody').html(formHtml);
            $('#lineModal').modal('show');
        });
    });

    // === HAT DÜZENLE ===
    $(document).on('click', '.edit-line', function () {
        const id = $(this).data('id');
        $.get('/Planner/Lines/Edit/' + id, function (formHtml) {
            $('#modalTitle').text('Hattı Düzenle');
            $('#modalBody').html(formHtml);
            $('#lineModal').modal('show');
        });
    });

    // === FORM SUBMIT (CREATE / UPDATE) ===
    $(document).on('submit', '#lineForm', function (e) {
        e.preventDefault();
        var formData = new FormData(this);

        $.ajax({
            url: '/Planner/Lines/Execute',
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (res) {
                $('#lineModal').modal('hide');

                $('#lineModal').on('hidden.bs.modal', function () {
                    refreshTable();
                    $(this).off('hidden.bs.modal');
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

        $.post('/Planner/Lines/Delete/' + id, function () {
            refreshTable();
        }).fail(function () {
            alert('Silme işlemi başarısız.');
        });
    });

    // === ARAMA ===
    $('#searchInput').on('input', function () {
        const term = $(this).val();
        $.get('/Planner/Lines/Search', { term }, function (data) {
            $('#linesTableContainer').html(data);
        }).fail(function () {
            alert('Arama hatası.');
        });
    });

    // === TABLOYU YENİLE ===
    function refreshTable() {
        $.get('/Planner/Lines/Search', function (data) {
            $('#linesTableContainer').html(data);
        }).fail(function () {
            alert('Tablo yenilenemedi.');
        });
    }

    // === AKTİF / PASİF TOGGLE ===
    $(document).on('click', '.toggle-line-active', function () {
        var $btn = $(this);
        var $row = $btn.closest('tr');
        var $badge = $btn.find('span');

        var currentActive = $btn.data('active') === true || $btn.data('active') === 'true';
        var newActive = !currentActive;

        // ✅ Onay sorusu
        var message = currentActive
            ? "Bu hattı pasif yapmak istediğinize emin misiniz?"
            : "Bu hattı aktif yapmak istediğinize emin misiniz?";
        if (!confirm(message)) return;

        // 🔹 Küçük basma efekti
        $badge.css('transform', 'scale(0.95)');
        setTimeout(function () {
            $badge.css('transform', 'scale(1)');
        }, 100);

        // Satırdan tüm değerleri al
        var id = $row.data('id');
        var code = $row.find('.line-code').text();
        var name = $row.find('.line-name').text();
        var lineType = $row.find('.line-type').data('value');
        var vehicleCount = $row.find('.line-vehicleCount').text();

        // FormData oluştur
        var formData = new FormData();
        formData.append('Id', id);
        formData.append('Code', code);
        formData.append('Name', name);
        formData.append('LineType', lineType);
        formData.append('VehicleCount', vehicleCount);
        formData.append('IsActive', newActive);

        // AJAX isteği
        $.ajax({
            url: '/Planner/Lines/Execute',
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (res) {
                $btn.data('active', newActive);

                if (newActive) {
                    $btn.removeClass('btn-secondary').addClass('btn-success');
                    $btn.html('<span class="badge bg-success rounded-pill" style="transition: transform 0.1s;">Aktif</span>');
                } else {
                    $btn.removeClass('btn-success').addClass('btn-secondary');
                    $btn.html('<span class="badge bg-secondary rounded-pill" style="transition: transform 0.1s;">Pasif</span>');
                }
            },
            error: function (xhr) {
                alert('Durum güncellenemedi: ' + (xhr.responseText || ''));
            }
        });
    });

    $(document).ready(function () {
        // Sayfa açılır açılmaz butonların durumuna göre renk ve badge ayarla
        $('.toggle-line-active').each(function () {
            var $btn = $(this);
            var isActive = $btn.data('active') === true || $btn.data('active') === 'true';

            if (isActive) {
                $btn.removeClass('btn-secondary').addClass('btn-success')
                    .html('<span class="badge bg-success rounded-pill" style="transition: transform 0.1s;">Aktif</span>');
            } else {
                $btn.removeClass('btn-success').addClass('btn-secondary')
                    .html('<span class="badge bg-secondary rounded-pill" style="transition: transform 0.1s;">Pasif</span>');
            }
        });
    });

});
