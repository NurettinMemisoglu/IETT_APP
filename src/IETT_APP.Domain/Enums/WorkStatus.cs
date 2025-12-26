using System.ComponentModel.DataAnnotations;

namespace IETT_APP.Domain.Enums
{
    public enum WorkStatus
    {
        [Display(Name = "Görev Bekleniyor")]
        Available = 0,

        [Display(Name = "Direksiyon Başında")]
        Working = 1,

        [Display(Name = "Yıllık İzin")]
        OnVacation = 2,

        [Display(Name = "Raporlu")]
        MedicalLeave = 3,

        [Display(Name = "İdari İzin")]
        AdministrativeLeave = 4,

        [Display(Name = "Mola")]
        Resting = 5,

        [Display(Name = "Açığa Alındı")]
        Suspended = 6,

        [Display(Name = "Mesai Dışı")]
        OffDuty = 7,

        [Display(Name = "Kayıt Tamamlanmadı")]
        RegistrationIncomplete = 8
    }
}