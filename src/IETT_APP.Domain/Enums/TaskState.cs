using System.ComponentModel.DataAnnotations;

namespace IETT_APP.Domain.Enums
{
    public enum TaskState
    {
        [Display(Name = "Beklemede")]
        Pending = 1,
        [Display(Name = "Kabul Edildi")]
        Accepted = 2,
        [Display(Name = "Devam Ediyor")]
        InProgress = 3,
        [Display(Name = "Tamamlandı")]
        Completed = 4,
        [Display(Name = "İptal Edildi")]
        Cancelled = 5,
        [Display(Name = "Yarım Kaldı")]
        Incomplete = 6,
    }
}