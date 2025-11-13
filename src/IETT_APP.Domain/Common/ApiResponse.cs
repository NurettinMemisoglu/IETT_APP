namespace IETT_APP.Domain.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }       // İşlemin başarılı olup olmadığı
        public string Message { get; set; }     // Kullanıcıya veya geliştiriciye mesaj
        public T? Data { get; set; }            // Dönen veri
        public List<string>? Errors { get; set; } // Validation veya domain hataları
        public int StatusCode { get; set; }     // HTTP status kodu
        public DateTime Timestamp { get; set; } // Cevabın oluşturulma zamanı

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }

        public string CorrelationId { get; set; }
    }

}
