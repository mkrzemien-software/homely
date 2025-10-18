namespace Homely.API.Models.DTOs
{
    /// <summary>
    /// Generic API response wrapper for consistent response format
    /// </summary>
    /// <typeparam name="T">Type of data returned</typeparam>
    public class ApiResponseDto<T>
    {
        /// <summary>
        /// Indicates if the request was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Response data (null if error)
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Error message (null if successful)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Detailed error information
        /// </summary>
        public List<string>? Errors { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Create successful response
        /// </summary>
        public static ApiResponseDto<T> SuccessResponse(T data, int statusCode = 200)
        {
            return new ApiResponseDto<T>
            {
                Success = true,
                Data = data,
                StatusCode = statusCode
            };
        }

        /// <summary>
        /// Create error response
        /// </summary>
        public static ApiResponseDto<T> ErrorResponse(string errorMessage, int statusCode = 400, List<string>? errors = null)
        {
            return new ApiResponseDto<T>
            {
                Success = false,
                ErrorMessage = errorMessage,
                Errors = errors,
                StatusCode = statusCode
            };
        }
    }
}
