namespace APP.ResponseModels
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public string Error { get; set; }

        // คอนสตรัคเตอร์ที่รับ 4 อาร์กิวเมนต์
        public ApiResponse(bool success, string message, T data, string error = null)
        {
            Success = success;
            Message = message;
            Data = data;
            Error = error;
        }

        // Success response factory method
        public static ApiResponse<T> SuccessResponse(string message, T data) =>
            new ApiResponse<T>(true, message, data);

        // Error response factory method
        public static ApiResponse<T> ErrorResponse(string message, string error) =>
            new ApiResponse<T>(false, message, default, error);
    }
}
