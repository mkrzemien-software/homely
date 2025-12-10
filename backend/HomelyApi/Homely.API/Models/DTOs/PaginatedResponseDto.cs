namespace Homely.API.Models.DTOs
{
    /// <summary>
    /// Pagination metadata for paginated responses
    /// </summary>
    public class PaginationDto
    {
        /// <summary>
        /// Current page number (1-based)
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Total number of items across all pages
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int ItemsPerPage { get; set; }
    }

    /// <summary>
    /// Generic paginated response wrapper for list endpoints
    /// </summary>
    /// <typeparam name="T">Type of items in the data array</typeparam>
    public class PaginatedResponseDto<T>
    {
        /// <summary>
        /// Array of items for the current page
        /// </summary>
        public IEnumerable<T> Data { get; set; } = new List<T>();

        /// <summary>
        /// Pagination metadata
        /// </summary>
        public PaginationDto Pagination { get; set; } = new PaginationDto();

        /// <summary>
        /// Create paginated response from PagedResult
        /// </summary>
        public static PaginatedResponseDto<T> FromPagedResult<TEntity>(
            Repositories.Base.PagedResult<TEntity> pagedResult,
            Func<TEntity, T> mapper)
        {
            return new PaginatedResponseDto<T>
            {
                Data = pagedResult.Items.Select(mapper),
                Pagination = new PaginationDto
                {
                    CurrentPage = pagedResult.PageNumber,
                    TotalPages = pagedResult.TotalPages,
                    TotalItems = pagedResult.TotalCount,
                    ItemsPerPage = pagedResult.PageSize
                }
            };
        }
    }
}
