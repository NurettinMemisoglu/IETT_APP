using IETT_APP.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IETT_APP.Domain.Entities
{
    public class Stop
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public StopType StopType { get; set; }
        public SmartStop SmartStop { get; set; }
        public Location Location { get; set; } = null!;
    }


    public class Location
    {
        [Precision(18, 16)]
        public decimal Latitude { get; set; }
        [Precision(18, 16)]
        public decimal Longitude { get; set; }
    }
}
