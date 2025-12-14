namespace IETT_APP.Application.Wrappers
{
    // Veri Dönmeyen İşlemler İçin (Örn: Delete, Update)
    public class ServiceResult
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();

        // JSON serileştirme için boş constructor
        public ServiceResult() { }

        public static ServiceResult Success(string message = "İşlem başarılı.")
        {
            return new ServiceResult { Succeeded = true, Message = message };
        }

        public static ServiceResult Failure(List<string> errors)
        {
            return new ServiceResult { Succeeded = false, Errors = errors };
        }

        public static ServiceResult Failure(string error)
        {
            return new ServiceResult { Succeeded = false, Errors = new List<string> { error } };
        }
    }

    // Veri Dönen İşlemler İçin (Örn: GetById, Create -> Döner DTO)
    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; set; }

        public ServiceResult() { }

        public static ServiceResult<T> Success(T data, string message = "İşlem başarılı.")
        {
            return new ServiceResult<T> { Succeeded = true, Data = data, Message = message };
        }

        public new static ServiceResult<T> Failure(List<string> errors)
        {
            return new ServiceResult<T> { Succeeded = false, Errors = errors };
        }

        public new static ServiceResult<T> Failure(string error)
        {
            return new ServiceResult<T> { Succeeded = false, Errors = new List<string> { error } };
        }
    }
}