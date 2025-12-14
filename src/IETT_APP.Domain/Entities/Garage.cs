using IETT_APP.Domain.Common;

namespace IETT_APP.Domain.Entities
{
    public class Garage<T> : BaseEntity<T>
    {
        public string GarageName { get; set; } = null!;
        public int Capacity { get; set; } // Araç kapasitesi
        public int Fileld { get; set; }
        public int YearStarted { get; set; } // Hizmete başlama yılı
        public Location Location { get; set; } = new Location();

        public ICollection<TripTask> TripTasks { get; set; } = new List<TripTask>();
    }


}
