using IETT_APP.Domain.Enums;

namespace IETT_APP.Application.Dtos.Line
{
    public class LineDto<T>
    {
        public T Id { get; set; } = default!;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public LineType LineType { get; set; }
        public int VehicleCount { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }  // nullable olmalı
    }
}
