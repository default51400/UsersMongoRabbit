namespace BLL.Models
{
    public class GenericResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T Items { get; set; }

        public GenericResponse<T> Success(string message, T items) => new GenericResponse<T>
        {
            IsSuccess = true,
            Message = message,
            Items = items
        };

        public GenericResponse<T> Error(string message, T items) => new GenericResponse<T>
        {
            IsSuccess = false,
            Message = message,
            Items = items
        };
    }
}