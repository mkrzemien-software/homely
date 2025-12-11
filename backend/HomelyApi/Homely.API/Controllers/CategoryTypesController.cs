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
    /// Get all available category types for a household
    /// </summary>
    /// <param name="householdId">Household ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of category types</returns>
    /// <remarks>
    /// Returns all active category types sorted by sort order for the specified household.
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryTypesResponse>> GetCategoryTypes(
        [FromQuery] Guid householdId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (householdId == Guid.Empty)
            {
                return BadRequest(new { error = "Household ID is required" });
            }

            var categoryTypes = await _categoryTypeService.GetActiveCategoryTypesAsync(householdId, cancellationToken);

            // Return in the format specified by API plan: { "data": [...] }
            return Ok(new CategoryTypesResponse { Data = categoryTypes });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category types for household {HouseholdId}", householdId);
            return StatusCode(500, new { error = "An error occurred while retrieving category types" });
        }
    }

    /// <summary>
    /// Get category type by ID for a household
    /// </summary>
    /// <param name="id">Category type ID</param>
    /// <param name="householdId">Household ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Category type details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryTypeDto>> GetCategoryTypeById(
        Guid id,
        [FromQuery] Guid householdId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (householdId == Guid.Empty)
            {
                return BadRequest(new { error = "Household ID is required" });
            }

            var categoryType = await _categoryTypeService.GetCategoryTypeByIdAsync(householdId, id, cancellationToken);

            if (categoryType == null)
            {
                return NotFound(new { error = $"Category type with ID {id} not found in household {householdId}" });
            }

            return Ok(categoryType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category type {CategoryTypeId} for household {HouseholdId}", id, householdId);
            return StatusCode(500, new { error = "An error occurred while retrieving category type" });
        }
    }

    /// <summary>
    /// Create new category type for a household
    /// </summary>
    /// <param name="householdId">Household ID</param>
    /// <param name="createDto">Category type creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created category type</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/category-types?householdId=123e4567-e89b-12d3-a456-426614174000
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
        [FromQuery] Guid householdId,
        [FromBody] CreateCategoryTypeDto createDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (householdId == Guid.Empty)
            {
                return BadRequest(new { error = "Household ID is required" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoryType = await _categoryTypeService.CreateCategoryTypeAsync(householdId, createDto, cancellationToken);

            return CreatedAtAction(
                nameof(GetCategoryTypeById),
                new { id = categoryType.Id, householdId },
                categoryType);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning(ex, "Duplicate category type name: {Name} for household {HouseholdId}", createDto.Name, householdId);
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category type for household {HouseholdId}", householdId);
            return StatusCode(500, new { error = "An error occurred while creating category type" });
        }
    }

    /// <summary>
    /// Update existing category type for a household
    /// </summary>
    /// <param name="id">Category type ID</param>
    /// <param name="householdId">Household ID</param>
    /// <param name="updateDto">Updated category type data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated category type</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/category-types/123e4567-e89b-12d3-a456-426614174000?householdId=123e4567-e89b-12d3-a456-426614174000
    ///     {
    ///         "name": "Updated Home Appliances",
    ///         "description": "Updated description",
    ///         "sortOrder": 15,
    ///         "isActive": true
    ///     }
    ///
    /// </remarks>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CategoryTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryTypeDto>> UpdateCategoryType(
        Guid id,
        [FromQuery] Guid householdId,
        [FromBody] UpdateCategoryTypeDto updateDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (householdId == Guid.Empty)
            {
                return BadRequest(new { error = "Household ID is required" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoryType = await _categoryTypeService.UpdateCategoryTypeAsync(householdId, id, updateDto, cancellationToken);
            return Ok(categoryType);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning(ex, "Duplicate category type name: {Name} for household {HouseholdId}", updateDto.Name, householdId);
            return Conflict(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating category type for household {HouseholdId}", householdId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category type {CategoryTypeId} for household {HouseholdId}", id, householdId);
            return StatusCode(500, new { error = "An error occurred while updating category type" });
        }
    }

    /// <summary>
    /// Delete category type (soft delete) for a household
    /// </summary>
    /// <param name="id">Category type ID to delete</param>
    /// <param name="householdId">Household ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteCategoryType(
        Guid id,
        [FromQuery] Guid householdId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (householdId == Guid.Empty)
            {
                return BadRequest(new { error = "Household ID is required" });
            }

            var success = await _categoryTypeService.DeleteCategoryTypeAsync(householdId, id, cancellationToken);

            if (!success)
            {
                return NotFound(new { error = $"Category type with ID {id} not found in household {householdId}" });
            }

            return Ok(new
            {
                success = true,
                message = "Category type deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category type {CategoryTypeId} for household {HouseholdId}", id, householdId);
            return StatusCode(500, new { error = "An error occurred while deleting category type" });
        }
    }
}
