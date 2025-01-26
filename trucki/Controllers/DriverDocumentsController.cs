using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;
using trucki.Services;

namespace trucki.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DriverDocumentsController : ControllerBase
    {
        private readonly IDriverDocumentService _driverDocumentService;

        public DriverDocumentsController(IDriverDocumentService driverDocumentService)
        {
            _driverDocumentService = driverDocumentService;
        }


        // GET: api/DriverDocuments/{driverId}
        [HttpGet("{driverId}")]
        public async Task<IActionResult> GetAllForDriver(string driverId)
        {
            try
            {
                var docs = await _driverDocumentService.GetDriverDocumentsAsync(driverId);
                var response = ApiResponseModel<IEnumerable<DriverDocument>>.Success(
                    "Driver documents retrieved",
                    docs,
                    StatusCodes.Status200OK
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = ApiResponseModel<string>.Fail(ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        // POST: api/DriverDocuments
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDriverDocumentRequest model)
        {
            if (!ModelState.IsValid)
            {
                var badRequest = ApiResponseModel<string>.Fail(
                    "Invalid input",
                    StatusCodes.Status400BadRequest
                );
                return BadRequest(badRequest);
            }

            try
            {
                // Map the request model to the DriverDocument entity
                var driverDocument = new DriverDocument
                {
                    DriverId = model.DriverId,
                    DocumentTypeId = model.DocumentTypeId,
                    FileUrl = model.FileUrl
                    // ApprovalStatus, SubmittedAt, etc. will be set in service or defaults
                };

                var created = await _driverDocumentService.CreateDriverDocumentAsync(driverDocument);

                var success = ApiResponseModel<DriverDocument>.Success(
                    "Driver document created successfully",
                    created,
                    StatusCodes.Status201Created
                );
                return StatusCode(StatusCodes.Status201Created, success);
            }
            catch (Exception ex)
            {
                var error = ApiResponseModel<string>.Fail(ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        // PUT: api/DriverDocuments/approve/{documentId}
        [HttpPut("approve/{documentId}")]
        public async Task<IActionResult> Approve(string documentId)
        {
            try
            {
                var doc = await _driverDocumentService.ApproveDocumentAsync(documentId);
                if (doc == null)
                {
                    var notFound = ApiResponseModel<string>.Fail(
                        "Driver document not found",
                        StatusCodes.Status404NotFound
                    );
                    return NotFound(notFound);
                }

                var success = ApiResponseModel<DriverDocument>.Success(
                    "Driver document approved",
                    doc,
                    StatusCodes.Status200OK
                );
                return Ok(success);
            }
            catch (Exception ex)
            {
                var error = ApiResponseModel<string>.Fail(ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        // PUT: api/DriverDocuments/reject/{documentId}
        [HttpPut("reject/{documentId}")]
        public async Task<IActionResult> Reject(string documentId, [FromBody] string reason)
        {
            try
            {
                var doc = await _driverDocumentService.RejectDocumentAsync(documentId, reason);
                if (doc == null)
                {
                    var notFound = ApiResponseModel<string>.Fail(
                        "Driver document not found",
                        StatusCodes.Status404NotFound
                    );
                    return NotFound(notFound);
                }

                var success = ApiResponseModel<DriverDocument>.Success(
                    "Driver document rejected",
                    doc,
                    StatusCodes.Status200OK
                );
                return Ok(success);
            }
            catch (Exception ex)
            {
                var error = ApiResponseModel<string>.Fail(ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        // DELETE: api/DriverDocuments/{documentId}
        [HttpDelete("{documentId}")]
        public async Task<IActionResult> Delete(string documentId)
        {
            try
            {
                var success = await _driverDocumentService.DeleteDriverDocumentAsync(documentId);
                if (!success)
                {
                    var notFound = ApiResponseModel<string>.Fail(
                        "Driver document not found",
                        StatusCodes.Status404NotFound
                    );
                    return NotFound(notFound);
                }

                var response = ApiResponseModel<string>.Success(
                    "Driver document deleted successfully",
                    StatusCodes.Status204NoContent
                );
                return StatusCode(StatusCodes.Status204NoContent, response);
            }
            catch (Exception ex)
            {
                var error = ApiResponseModel<string>.Fail(ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        // GET: api/DriverDocuments/required/{driverId}
        [HttpGet("required/{driverId}")]
        public async Task<IActionResult> GetRequiredDocumentTypes(string driverId)
        {
            try
            {
                var requiredTypes = await _driverDocumentService.GetRequiredDocumentTypesForDriverAsync(driverId);

                var success = ApiResponseModel<IEnumerable<DocumentType>>.Success(
                    "Required document types retrieved",
                    requiredTypes,
                    StatusCodes.Status200OK
                );
                return Ok(success);
            }
            catch (Exception ex)
            {
                var error = ApiResponseModel<string>.Fail(ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        [HttpGet("summary/{driverId}")]
        [Authorize(Roles = "driver")]
        public async Task<IActionResult> GetDriverDocumentSummary(string driverId)
        {
            try
            {
                var summary = await _driverDocumentService.GetDriverDocumentSummaryAsync(driverId);

                // If you want to handle the case where the driver doesn't exist, 
                // you can check if summary is empty or if the driver was null in the service.
                // For instance, you could throw an exception or return a 404 from the service.
                // Alternatively, you can do a quick check here:

                if (!summary.Any())
                {
                    // Could mean driver doesn't exist or no required docs found
                    // Decide how you want to handle it. For example:
                    var notFoundResponse = ApiResponseModel<string>.Fail(
                        "Driver not found or no required documents configured",
                        StatusCodes.Status404NotFound
                    );
                    return NotFound(notFoundResponse);
                }

                // Return success response
                var successResponse = ApiResponseModel<IEnumerable<DriverDocumentStatusDto>>.Success(
                    "Driver document summary retrieved",
                    summary,
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
        // ... other actions (GetAllForDriver, Approve, Reject, etc.) remain the same ...
    }
}
