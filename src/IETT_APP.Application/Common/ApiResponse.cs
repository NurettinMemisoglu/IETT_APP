namespace IETT_APP.Application.Common
{
    public class ApiResponse<T>
    {
        // Genel durum
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int? ErrorCode { get; set; } // Domain / HTTP kodları için
        public string? TraceId { get; set; } // Log/monitoring için
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Veri
        public T? Data { get; set; }

        // Liste yanıtları için metadata
        public int? TotalCount { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }

        // Hata detayları
        public List<string>? Errors { get; set; }

        // Başarılı tekil yanıt
        public static ApiResponse<T> SuccessResponse(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        // Başarısız yanıt
        public static ApiResponse<T> FailResponse(
            string? message = null,
            int? errorCode = null,
            List<string>? errors = null,
            string? traceId = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode,
                Errors = errors,
                TraceId = traceId
            };
        }

        // Liste yanıtları için
        public static ApiResponse<T> ListResponse(
            T data,
            int totalCount,
            int page,
            int pageSize,
            string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
