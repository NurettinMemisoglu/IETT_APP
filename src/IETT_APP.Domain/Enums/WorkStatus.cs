namespace IETT_APP.Domain.Enums
{
    /// <summary>
    /// Personelin anlık çalışma ve mevcudiyet durumunu belirtir.
    /// </summary>
    public enum WorkStatus
    {
        // 0: Göreve Hazır / Müsait (İş bekliyor)
        Available = 0,

        // 1: Şu an Çalışıyor / Görevde (Seferde veya Mesai Başında)
        // 'OnDuty' yerine 'Working' yaptık ki ofis personeline de uysun.
        Working = 1,

        // 2: Yıllık İzinli
        OnVacation = 2,

        // 3: Raporlu / Hasta (Sağlık İzni)
        MedicalLeave = 3,

        // 4: İdari veya Mazeret İzni (Düğün, Cenaze vb.)
        AdministrativeLeave = 4,

        // 5: İstirahatte / Mola (Vardiya arası veya günlük mola)
        Resting = 5,

        // 6: Açığa Alınmış / Pasif (Disiplin süreci vb.)
        Suspended = 6,

        // 7: Görev Dışı (Mesai saati bitti, eve gitti)
        OffDuty = 7
    }
}