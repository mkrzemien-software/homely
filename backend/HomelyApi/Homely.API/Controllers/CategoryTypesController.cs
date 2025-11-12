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
}
