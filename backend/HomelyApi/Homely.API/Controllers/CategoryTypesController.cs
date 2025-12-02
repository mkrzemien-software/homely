using Homely.API.Models.DTOs.Categories;
using Homely.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Homely.API.Controllers;

/// <summary>
/// Category types management controller
/// Handles high-level category types like Technical Inspections, Waste Collection, Medical Visits
/// </summary>
[ApiController]
[Route("api/category-types")]
[Produces("application/json")]
public class CategoryTypesController : ControllerBase
{
    private readonly ICategoryTypeService _categoryTypeService;
    private readonly ILogger<CategoryTypesController> _logger;

    public CategoryTypesController(
        ICategoryTypeService categoryTypeService,
        ILogger<CategoryTypesController> logger)
    {
        _categoryTypeService = categoryTypeService;
        _logger = logger;
    }

    /// <summary>
    /// Get all available category types
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of category types</returns>
    /// <remarks>
    /// Returns all active category types sorted by sort order.
    /// Category types represent high-level classifications such as:
    /// - Technical Inspections (Przeglądy techniczne)
    /// - Waste Collection (Wywóz śmieci)
    /// - Medical Visits (Wizyty medyczne)
    ///
    /// Sample response:
    ///
    ///     {
    ///       "data": [
    ///         {
    ///           "id": 1,
    ///           "name": "Przeglądy techniczne",
    ///           "description": "Technical inspections and maintenance",
    ///           "sortOrder": 1
    ///         },
    ///         {
    ///           "id": 2,
    ///           "name": "Wywóz śmieci",
    ///           "description": "Waste collection schedule",
    ///           "sortOrder": 2
    ///         }
    ///       ]
    ///     }
    ///
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(CategoryTypesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryTypesResponse>> GetCategoryTypes(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var categoryTypes = await _categoryTypeService.GetActiveCategoryTypesAsync(cancellationToken);

            // Return in the format specified by API plan: { "data": [...] }
            return Ok(new CategoryTypesResponse { Data = categoryTypes });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category types");
            return StatusCode(500, new { error = "An error occurred while retrieving category types" });
        }
    }

    /// <summary>
    /// Get category type by ID
    /// </summary>
    /// <param name="id">Category type ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Category type details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryTypeDto>> GetCategoryTypeById(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var categoryType = await _categoryTypeService.GetCategoryTypeByIdAsync(id, cancellationToken);

            if (categoryType == null)
            {
                return NotFound(new { error = $"Category type with ID {id} not found" });
            }

            return Ok(categoryType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category type {CategoryTypeId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving category type" });
        }
    }

    /// <summary>
    /// Create new category type
    /// </summary>
    /// <param name="createDto">Category type creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created category type</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/category-types
    ///     {
    ///         "name": "Home Appliances",
    ///         "description": "Maintenance for home appliances",
    ///         "sortOrder": 10,
    ///         "isActive": true
    ///     }
    ///
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryTypeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryTypeDto>> CreateCategoryType(
        [FromBody] CreateCategoryTypeDto createDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoryType = await _categoryTypeService.CreateCategoryTypeAsync(createDto, cancellationToken);

            return CreatedAtAction(
                nameof(GetCategoryTypeById),
                new { id = categoryType.Id },
                categoryType);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning(ex, "Duplicate category type name: {Name}", createDto.Name);
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category type");
            return StatusCode(500, new { error = "An error occurred while creating category type" });
        }
    }

    /// <summary>
    /// Update existing category type
    /// </summary>
    /// <param name="id">Category type ID</param>
    /// <param name="updateDto">Updated category type data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated category type</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/category-types/1
    ///     {
    ///         "name": "Updated Home Appliances",
    ///         "description": "Updated description",
    ///         "sortOrder": 15,
    ///         "isActive": true
    ///     }
    ///
    /// </remarks>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CategoryTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryTypeDto>> UpdateCategoryType(
        int id,
        [FromBody] UpdateCategoryTypeDto updateDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoryType = await _categoryTypeService.UpdateCategoryTypeAsync(id, updateDto, cancellationToken);
            return Ok(categoryType);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning(ex, "Duplicate category type name: {Name}", updateDto.Name);
            return Conflict(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating category type");
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category type {CategoryTypeId}", id);
            return StatusCode(500, new { error = "An error occurred while updating category type" });
        }
    }

    /// <summary>
    /// Delete category type (soft delete)
    /// </summary>
    /// <param name="id">Category type ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteCategoryType(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _categoryTypeService.DeleteCategoryTypeAsync(id, cancellationToken);

            if (!success)
            {
                return NotFound(new { error = $"Category type with ID {id} not found" });
            }

            return Ok(new
            {
                success = true,
                message = "Category type deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category type {CategoryTypeId}", id);
            return StatusCode(500, new { error = "An error occurred while deleting category type" });
        }
    }
}
