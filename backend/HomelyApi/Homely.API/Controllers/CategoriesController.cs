using Homely.API.Models.DTOs.Categories;
using Homely.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Homely.API.Controllers;

/// <summary>
/// Categories management controller
/// </summary>
[ApiController]
[Route("api/categories")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(
        ICategoryService categoryService,
        ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    /// <summary>
    /// Get all active categories
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active categories</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetActiveCategories(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await _categoryService.GetActiveCategoriesAsync(cancellationToken);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active categories");
            return StatusCode(500, new { error = "An error occurred while retrieving active categories" });
        }
    }

    /// <summary>
    /// Get all categories (including inactive)
    /// </summary>
    /// <param name="categoryTypeId">Optional filter by category type ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of categories</returns>
    [HttpGet]
    [ProducesResponseType(typeof(CategoriesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoriesResponse>> GetAllCategories(
        [FromQuery] int? categoryTypeId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IEnumerable<CategoryDto> categories;

            if (categoryTypeId.HasValue)
            {
                categories = await _categoryService.GetCategoriesByCategoryTypeAsync(categoryTypeId.Value, cancellationToken);
            }
            else
            {
                categories = await _categoryService.GetAllCategoriesAsync(cancellationToken);
            }

            // Return in the format specified by API plan: { "data": [...] }
            return Ok(new CategoriesResponse { Data = categories });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return StatusCode(500, new { error = "An error occurred while retrieving categories" });
        }
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Category details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryDto>> GetCategoryById(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(id, cancellationToken);

            if (category == null)
            {
                return NotFound(new { error = $"Category with ID {id} not found" });
            }

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving category" });
        }
    }

    /// <summary>
    /// Create new category
    /// </summary>
    /// <param name="createDto">Category creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created category</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/categories
    ///     {
    ///         "categoryTypeId": 1,
    ///         "name": "Boiler Maintenance",
    ///         "description": "Regular boiler inspection and maintenance",
    ///         "sortOrder": 10,
    ///         "isActive": true
    ///     }
    ///
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryDto>> CreateCategory(
        [FromBody] CreateCategoryDto createDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = await _categoryService.CreateCategoryAsync(createDto, cancellationToken);

            return CreatedAtAction(
                nameof(GetCategoryById),
                new { id = category.Id },
                category);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning(ex, "Duplicate category name: {Name}", createDto.Name);
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, new { error = "An error occurred while creating category" });
        }
    }

    /// <summary>
    /// Update existing category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="updateDto">Updated category data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated category</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/categories/1
    ///     {
    ///         "categoryTypeId": 1,
    ///         "name": "Updated Boiler Maintenance",
    ///         "description": "Updated description",
    ///         "sortOrder": 15,
    ///         "isActive": true
    ///     }
    ///
    /// </remarks>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(
        int id,
        [FromBody] UpdateCategoryDto updateDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = await _categoryService.UpdateCategoryAsync(id, updateDto, cancellationToken);
            return Ok(category);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning(ex, "Duplicate category name: {Name}", updateDto.Name);
            return Conflict(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating category");
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", id);
            return StatusCode(500, new { error = "An error occurred while updating category" });
        }
    }

    /// <summary>
    /// Delete category (soft delete)
    /// </summary>
    /// <param name="id">Category ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteCategory(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _categoryService.DeleteCategoryAsync(id, cancellationToken);

            if (!success)
            {
                return NotFound(new { error = $"Category with ID {id} not found" });
            }

            return Ok(new
            {
                success = true,
                message = "Category deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            return StatusCode(500, new { error = "An error occurred while deleting category" });
        }
    }

    /// <summary>
    /// Update sort order for multiple categories
    /// </summary>
    /// <param name="updateDto">List of category ID and sort order updates</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/categories/sort-order
    ///     {
    ///         "items": [
    ///             { "id": 1, "sortOrder": 0 },
    ///             { "id": 2, "sortOrder": 1 },
    ///             { "id": 3, "sortOrder": 2 }
    ///         ]
    ///     }
    ///
    /// </remarks>
    [HttpPatch("sort-order")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateCategoriesSortOrder(
        [FromBody] UpdateCategoriesSortOrderDto updateDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (updateDto.Items == null || updateDto.Items.Count == 0)
            {
                return BadRequest(new { error = "Items list cannot be empty" });
            }

            await _categoryService.UpdateCategoriesSortOrderAsync(updateDto, cancellationToken);

            return Ok(new
            {
                success = true,
                message = "Categories sort order updated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating categories sort order");
            return StatusCode(500, new { error = "An error occurred while updating categories sort order" });
        }
    }
}
