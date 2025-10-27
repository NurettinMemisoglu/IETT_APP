$(function () {

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

        const payload = {
            Id: $(this).find('[name="Id"]').val(),
            Code: $(this).find('[name="Code"]').val(),
            Name: $(this).find('[name="Name"]').val(),
            LineType: $(this).find('[name="LineType"]').val(),
            VehicleCount: $(this).find('[name="VehicleCount"]').val(),
            IsActive: $(this).find('[name="IsActive"]').is(':checked')
        };

        $.ajax({
            url: '/Planner/Lines/Execute',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
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

    // === AKTİF / PASİF TOGGLE ===
    $(document).on('click', '.toggle-line-active', function () {
        var $btn = $(this);
        var $row = $btn.closest('tr');
        if ($row.data('isDeleted') === true) return;

        var $badge = $btn.find('span');
        var currentActive = $btn.data('active') === true || $btn.data('active') === 'true';
        var newActive = !currentActive;

        var message = currentActive
            ? "Bu hattı pasif yapmak istediğinize emin misiniz?"
            : "Bu hattı aktif yapmak istediğinize emin misiniz?";
        if (!confirm(message)) return;

        // Küçük basma efekti
        $badge.css('transform', 'scale(0.95)');
        setTimeout(function () {
            $badge.css('transform', 'scale(1)');
        }, 100);

        // Satırdan değerleri al
        var payload = {
            Id: $row.data('id'),
            Code: $row.find('.line-code').text(),
            Name: $row.find('.line-name').text(),
            LineType: parseInt($row.find('.line-type').data('value')), // enum int
            VehicleCount: parseInt($row.find('.line-vehicleCount').text()),
            IsActive: newActive
        };

        $.ajax({
            url: '/Planner/Lines/Execute',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function () {
                refreshTable();
            },
            error: function (xhr) {
                alert('Durum güncellenemedi: ' + (xhr.responseText || ''));
            }
        });
    });

    // === Sayfa açılır açılmaz toggle buton durumunu ayarla ===
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
