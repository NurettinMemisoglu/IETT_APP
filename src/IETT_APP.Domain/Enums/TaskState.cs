using System.ComponentModel.DataAnnotations;

namespace IETT_APP.Domain.Enums
{
    public enum TaskState
    {
        [Display(Name = "Beklemede")]
        Pending = 1,
        [Display(Name = "Devam Ediyor")]
        InProgress = 2,
        [Display(Name = "Tamamlandı")]
        Completed = 3,
        [Display(Name = "İptal Edildi")]
        Cancelled = 4,
        [Display(Name = "Yarım Kaldı")]
        Incomplete = 5,
    }
}