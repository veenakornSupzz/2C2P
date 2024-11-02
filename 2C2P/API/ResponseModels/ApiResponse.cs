namespace API.ResponseModels
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public string Error { get; set; }

        public ApiResponse(bool success, string message, T data, string error = null)
        {
            Success = success;
            Message = message;
            Data = data;
            Error = error;
        }

        // สร้าง response สำหรับ Success
        public static ApiResponse<T> SuccessResponse(string message, T data) =>
            new ApiResponse<T>(true, message, data);

        // สร้าง response สำหรับ Error
        public static ApiResponse<T> ErrorResponse(string message, string error) =>
            new ApiResponse<T>(false, message, default, error);
    }
}
