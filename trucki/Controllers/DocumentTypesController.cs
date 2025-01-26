using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.ResponseModels;

namespace trucki.Controllers;


[ApiController]
[Route("api/[controller]")]
public class DocumentTypesController : ControllerBase
{
    private readonly IDocumentTypeService _documentTypeService;

    public DocumentTypesController(IDocumentTypeService documentTypeService)
    {
        _documentTypeService = documentTypeService;
    }

    // GET: api/documenttypes
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var documentTypes = await _documentTypeService.GetAllDocumentTypesAsync();
            var response = ApiResponseModel<IEnumerable<DocumentType>>.Success(
                "Successfully retrieved Document Types",
                documentTypes,
                StatusCodes.Status200OK
            );
            return Ok(response);
        }
        catch (Exception ex)
        {
            // Log exception (ex) as needed
            var errorResponse = ApiResponseModel<string>.Fail(
                $"An error occurred: {ex.Message}",
                StatusCodes.Status500InternalServerError
            );
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    // GET: api/documenttypes/5
    [HttpGet("{id}")]
    [Authorize(Roles = "admin,driver")]
    public async Task<IActionResult> Get(string id)
    {
        try
        {
            var documentType = await _documentTypeService.GetDocumentTypeAsync(id);
            if (documentType == null)
            {
                var notFoundResponse = ApiResponseModel<string>.Fail(
                    "DocumentType not found",
                    StatusCodes.Status404NotFound
                );
                return NotFound(notFoundResponse);
            }

            var successResponse = ApiResponseModel<DocumentType>.Success(
                "Successfully retrieved the DocumentType",
                documentType,
                StatusCodes.Status200OK
            );
            return Ok(successResponse);
        }
        catch (Exception ex)
        {
            var errorResponse = ApiResponseModel<string>.Fail(
                $"An error occurred: {ex.Message}",
                StatusCodes.Status500InternalServerError
            );
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    // POST: api/documenttypes
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create([FromBody] DocumentType model)
    {
        if (!ModelState.IsValid)
        {
            var badRequestResponse = ApiResponseModel<ModelStateDictionary>.Fail(
                "Invalid model state",
                StatusCodes.Status400BadRequest
            );
            return BadRequest(badRequestResponse);
        }

        try
        {
            var createdDocType = await _documentTypeService.CreateDocumentTypeAsync(model);

            var successResponse = ApiResponseModel<DocumentType>.Success(
                "DocumentType created successfully",
                createdDocType,
                StatusCodes.Status201Created
            );
            
            // return CreatedAtAction so that you set location header
            return CreatedAtAction(nameof(Get), new { id = createdDocType.Id }, successResponse);
        }
        catch (Exception ex)
        {
            var errorResponse = ApiResponseModel<string>.Fail(
                $"An error occurred: {ex.Message}",
                StatusCodes.Status500InternalServerError
            );
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    // PUT: api/documenttypes/5
    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(string id, [FromBody] DocumentType model)
    {
        if (!ModelState.IsValid)
        {
            var badRequestResponse = ApiResponseModel<string>.Fail(
                "Invalid model state",
                StatusCodes.Status400BadRequest
            );
            return BadRequest(badRequestResponse);
        }

        try
        {
            // Force the model ID to match the route
            model.Id = id;

            var updatedDocType = await _documentTypeService.UpdateDocumentTypeAsync(model);
            if (updatedDocType == null)
            {
                var notFoundResponse = ApiResponseModel<string>.Fail(
                    "DocumentType not found",
                    StatusCodes.Status404NotFound
                );
                return NotFound(notFoundResponse);
            }

            var successResponse = ApiResponseModel<DocumentType>.Success(
                "DocumentType updated successfully",
                updatedDocType,
                StatusCodes.Status200OK
            );
            return Ok(successResponse);
        }
        catch (Exception ex)
        {
            var errorResponse = ApiResponseModel<string>.Fail(
                $"An error occurred: {ex.Message}",
                StatusCodes.Status500InternalServerError
            );
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    // DELETE: api/documenttypes/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var success = await _documentTypeService.DeleteDocumentTypeAsync(id);
            if (!success)
            {
                var notFoundResponse = ApiResponseModel<string>.Fail(
                    "DocumentType not found",
                    StatusCodes.Status404NotFound
                );
                return NotFound(notFoundResponse);
            }

            // No need to return data, but you can if you want.
            var successResponse = ApiResponseModel<bool>.Success(
                "DocumentType deleted successfully",
                StatusCodes.Status204NoContent
            );
            return StatusCode(StatusCodes.Status204NoContent, successResponse);
        }
        catch (Exception ex)
        {
            var errorResponse = ApiResponseModel<bool>.Fail(
                $"An error occurred: {ex.Message}",
                StatusCodes.Status500InternalServerError
            );
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }
}
