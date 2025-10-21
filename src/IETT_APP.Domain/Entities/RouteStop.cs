namespace IETT_APP.Domain.Entities
{
    public class RouteStop<T>
    {
        public T RouteId { get; set; }
        public Route<T> Route { get; set; } = null!;

        public Guid StopId { get; set; }
        public Stop<T> Stop { get; set; } = null!;

        // Opsiyonel: hattın durak sırası
        public int Order { get; set; }
    }
}
