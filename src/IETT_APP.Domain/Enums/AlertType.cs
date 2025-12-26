namespace IETT_APP.Domain.Enums
{
    public enum AlertType
    {
        Info = 0,       // Mavi (Bilgi)
        Warning = 1,    // Sarı (Gecikme vb.)
        Danger = 2,     // Kırmızı (İptal/Arıza)
        Success = 3     // Yeşil (Tamamlandı - opsiyonel)
    }
}
