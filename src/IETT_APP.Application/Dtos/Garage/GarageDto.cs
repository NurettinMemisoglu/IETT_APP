namespace IETT_APP.Application.Dtos.Garage
{
    public class GarageDto<T>
    {
        public Guid Id { get; set; }
        public string GarageName { get; set; } = null!;
    }

}
