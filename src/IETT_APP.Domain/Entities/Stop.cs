using IETT_APP.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IETT_APP.Domain.Entities
{
    public class Stop<T>
    {
        public T Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public StopType StopType { get; set; }
        public SmartStop SmartStop { get; set; }

        public string District { get; set; } = string.Empty;
        public Location Location { get; set; } = null!;

        public ICollection<RouteStop<Guid>> RouteStops { get; set; } = new List<RouteStop<Guid>>();

    }


    public class Location
    {
        [Precision(8, 6)]
        public decimal Latitude { get; set; }
        [Precision(8, 6)]
        public decimal Longitude { get; set; }
    }
}
//id,createddate=now,createdrole=role,isdeleted,isactive