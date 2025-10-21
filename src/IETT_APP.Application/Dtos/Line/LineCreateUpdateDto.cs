using IETT_APP.Application.Validation;
using IETT_APP.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace IETT_APP.Application.Dtos.Line
{
    public class LineCreateUpdateDto<T>
    {
        public T? Id { get; set; }

        [Required(ErrorMessage = "Code boş olamaz")]
        [StringLength(50, ErrorMessage = "Code maksimum 50 karakter olabilir")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name boş olamaz")]
        [StringLength(100, ErrorMessage = "Name maksimum 100 karakter olabilir")]
        public string Name { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "VehicleCount negatif olamaz")]
        public int VehicleCount { get; set; }

        // Validate that provided numeric value is one of the defined enum members
        [ValidEnumValue(typeof(LineType), ErrorMessage = "LineType değeri geçersiz. Geçerli değerler: 1 (IETT), 2 (OHO), 3 (METROBUS).")]
        public LineType LineType { get; set; }
    }
}
