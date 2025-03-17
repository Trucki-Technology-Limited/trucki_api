using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.ResponseModels;

namespace trucki.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TermsAndConditionsController : ControllerBase
{
    private readonly ITermsAndConditionsService _service;

    public TermsAndConditionsController(ITermsAndConditionsService service)
    {
        _service = service;
    }

    // GET: api/termsandconditions
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<IEnumerable<TermsAndConditions>>>> GetAllTermsAndConditions()
    {
        try
        {
            var terms = await _service.GetAllTermsAndConditionsAsync();
            var response = ApiResponseModel<IEnumerable<TermsAndConditions>>.Success(
                "Terms and conditions retrieved successfully",
                terms,
                StatusCodes.Status200OK
            );
            return Ok(response);
        }
        catch (Exception ex)
        {
            var errorResponse = ApiResponseModel<IEnumerable<TermsAndConditions>>.Fail(
                $"An error occurred: {ex.Message}",
                StatusCodes.Status500InternalServerError
            );
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    // GET: api/termsandconditions/type/{documentType}
    [HttpGet("GetTermsAndConditionsByDocumentType")]
    public async Task<ActionResult<ApiResponseModel<IEnumerable<TermsAndConditions>>>> GetTermsAndConditionsByDocumentType(string documentType)
    {
        try
        {
            var terms = await _service.GetTermsAndConditionsByDocumentTypeAsync(documentType);
            var response = ApiResponseModel<IEnumerable<TermsAndConditions>>.Success(
                "Terms and conditions retrieved successfully",
                terms,
                StatusCodes.Status200OK
            );
            return Ok(response);
        }
        catch (Exception ex)
        {
            var errorResponse = ApiResponseModel<IEnumerable<TermsAndConditions>>.Fail(
                $"An error occurred: {ex.Message}",
                StatusCodes.Status500InternalServerError
            );
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    // GET: api/termsandconditions/{id}
    [HttpGet("GetTermsAndConditionsById")]
    [Authorize(Roles = "admin,driver")]
    public async Task<ActionResult<ApiResponseModel<TermsAndConditions>>> GetTermsAndConditionsById(string id)
    {
        try
        {
            var terms = await _service.GetTermsAndConditionsByIdAsync(id);
            if (terms == null)
            {
                var notFoundResponse = ApiResponseModel<TermsAndConditions>.Fail(
                    "Terms and conditions not found",
                    StatusCodes.Status404NotFound
                );
                return NotFound(notFoundResponse);
            }

            var response = ApiResponseModel<TermsAndConditions>.Success(
                "Terms and conditions retrieved successfully",
                terms,
                StatusCodes.Status200OK
            );
            return Ok(response);
        }
        catch (Exception ex)
        {
            var errorResponse = ApiResponseModel<TermsAndConditions>.Fail(
                $"An error occurred: {ex.Message}",
                StatusCodes.Status500InternalServerError
            );
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    // GET: api/termsandconditions/current/{documentType}
    [HttpGet("GetCurrentTermsAndConditions")]
    [Authorize(Roles = "admin,driver")]
    public async Task<ActionResult<ApiResponseModel<TermsAndConditions>>> GetCurrentTermsAndConditions(string documentType)
    {
        try
        {
            var currentTerms = await _service.GetCurrentTermsAndConditionsAsync(documentType);
            if (currentTerms == null)
            {
                var notFoundResponse = ApiResponseModel<TermsAndConditions>.Fail(
                    "Current terms and conditions not found for this document type",
                    StatusCodes.Status404NotFound
                );
                return NotFound(notFoundResponse);
            }

            var response = ApiResponseModel<TermsAndConditions>.Success(
                "Current terms and conditions retrieved successfully",
                currentTerms,
                StatusCodes.Status200OK
            );
            return Ok(response);
        }
        catch (Exception ex)
        {
            var errorResponse = ApiResponseModel<TermsAndConditions>.Fail(
                $"An error occurred: {ex.Message}",
                StatusCodes.Status500InternalServerError
            );
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    // GET: api/termsandconditions/current
    [HttpGet("GetAllCurrentTermsAndConditions")]
    [Authorize(Roles = "admin,driver")]
    public async Task<ActionResult<ApiResponseModel<IEnumerable<TermsAndConditions>>>> GetAllCurrentTermsAndConditions()
    {
        try
        {
            var currentTerms = await _service.GetAllCurrentTermsAndConditionsAsync();
            var response = ApiResponseModel<IEnumerable<TermsAndConditions>>.Success(
                "All current terms and conditions retrieved successfully",
                currentTerms,
                StatusCodes.Status200OK
            );
            return Ok(response);
        }
        catch (Exception ex)
        {
            var errorResponse = ApiResponseModel<IEnumerable<TermsAndConditions>>.Fail(
                $"An error occurred: {ex.Message}",
                StatusCodes.Status500InternalServerError
            );
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    // POST: api/termsandconditions
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<TermsAndConditions>>> CreateTermsAndConditions([FromBody] TermsAndConditions termsAndConditions)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var badRequestResponse = ApiResponseModel<TermsAndConditions>.Fail(
                    "Invalid model state",
                    StatusCodes.Status400BadRequest
                );
                return BadRequest(badRequestResponse);
            }

            var createdTerms = await _service.CreateTermsAndConditionsAsync(termsAndConditions);
            var response = ApiResponseModel<TermsAndConditions>.Success(
                "Terms and conditions created successfully",
                createdTerms,
                StatusCodes.Status201Created
            );
            
            return CreatedAtAction(
                nameof(GetTermsAndConditionsById), 
                new { id = createdTerms.Id }, 
                response
            );
        }
        catch (Exception ex)
        {
            var errorResponse = ApiResponseModel<TermsAndConditions>.Fail(
                $"An error occurred: {ex.Message}",
                StatusCodes.Status500InternalServerError
            );
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    // PUT: api/termsandconditions/{id}
    [HttpPut("UpdateTermsAndConditions")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<TermsAndConditions>>> UpdateTermsAndConditions(string id, [FromBody] TermsAndConditions termsAndConditions)
    {
        try
        {
            if (id != termsAndConditions.Id)
            {
                var badRequestResponse = ApiResponseModel<TermsAndConditions>.Fail(
                    "ID in URL does not match ID in the request body",
                    StatusCodes.Status400BadRequest
                );
                return BadRequest(badRequestResponse);
            }

            if (!ModelState.IsValid)
            {
                var badRequestResponse = ApiResponseModel<TermsAndConditions>.Fail(
                    "Invalid model state",
                    StatusCodes.Status400BadRequest
                );
                return BadRequest(badRequestResponse);
            }

            var updatedTerms = await _service.UpdateTermsAndConditionsAsync(termsAndConditions);
            var response = ApiResponseModel<TermsAndConditions>.Success(
                "Terms and conditions updated successfully",
                updatedTerms,
                StatusCodes.Status200OK
            );
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            var notFoundResponse = ApiResponseModel<TermsAndConditions>.Fail(
                ex.Message,
                StatusCodes.Status404NotFound
            );
            return NotFound(notFoundResponse);
        }
        catch (Exception ex)
        {
            var errorResponse = ApiResponseModel<TermsAndConditions>.Fail(
                $"An error occurred: {ex.Message}",
                StatusCodes.Status500InternalServerError
            );
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    // DELETE: api/termsandconditions/{id}
    [HttpDelete("DeleteTermsAndConditions")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<bool>>> DeleteTermsAndConditions(string id)
    {
        try
        {
            var result = await _service.DeleteTermsAndConditionsAsync(id);
            if (!result)
            {
                var notFoundResponse = ApiResponseModel<bool>.Fail(
                    "Terms and conditions not found",
                    StatusCodes.Status404NotFound
                );
                return NotFound(notFoundResponse);
            }
            
            var response = ApiResponseModel<bool>.Success(
                "Terms and conditions deleted successfully",
                true,
                StatusCodes.Status204NoContent
            );
            return StatusCode(StatusCodes.Status204NoContent, response);
        }
        catch (InvalidOperationException ex)
        {
            var badRequestResponse = ApiResponseModel<bool>.Fail(
                ex.Message,
                StatusCodes.Status400BadRequest
            );
            return BadRequest(badRequestResponse);
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

    // PUT: api/termsandconditions/{id}/make-current
    [HttpPut("MakeTermsAndConditionsCurrent")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseModel<TermsAndConditions>>> MakeTermsAndConditionsCurrent(string id)
    {
        try
        {
            var terms = await _service.MakeTermsAndConditionsCurrentAsync(id);
            if (terms == null)
            {
                var notFoundResponse = ApiResponseModel<TermsAndConditions>.Fail(
                    "Terms and conditions not found",
                    StatusCodes.Status404NotFound
                );
                return NotFound(notFoundResponse);
            }
            
            var response = ApiResponseModel<TermsAndConditions>.Success(
                "Terms and conditions set as current successfully",
                terms,
                StatusCodes.Status200OK
            );
            return Ok(response);
        }
        catch (Exception ex)
        {
            var errorResponse = ApiResponseModel<TermsAndConditions>.Fail(
                $"An error occurred: {ex.Message}",
                StatusCodes.Status500InternalServerError
            );
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }
}