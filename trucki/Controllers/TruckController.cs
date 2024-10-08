using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers;
[Route("api/[controller]")]
[ApiController]
public class TruckController: ControllerBase
{
    private readonly ITruckService _truckService;

    public TruckController(ITruckService truckService)
    {
        _truckService = truckService;
    }
    
        [HttpPost("AddNewTruck")]
        [Authorize(Roles = "admin,transporter")]
        public async Task<ActionResult<ApiResponseModel<string>>> AddNewTruck([FromBody] AddTruckRequestModel model)
        {
            var response = await _truckService.AddNewTruck(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("EditTruck")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponseModel<bool>>> EditTruck([FromBody]EditTruckRequestModel model)
        {
            var response = await _truckService.EditTruck(model);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("DeleteTruck")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponseModel<string>>> DeleteTruck(string truckId)
        {
            var response = await _truckService.DeleteTruck(truckId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("GetTruckById")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponseModel<AllTruckResponseModel>>> GetTruckById(string truckId)
        {
            var response = await _truckService.GetTruckById(truckId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("SearchTrucks")]
        [Authorize(Roles = "admin,manager,chiefmanager")]
        public async Task<ActionResult<ApiResponseModel<IEnumerable<AllTruckResponseModel>>>> SearchTruck(string? searchWords)
        {
            var response = await _truckService.SearchTruck(searchWords);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("GetAllTrucks")]
        [Authorize(Roles = "admin,manager,field officer,chiefmanager")]
        public async Task<ActionResult<ApiResponseModel<IEnumerable<AllTruckResponseModel>>>> GetAllTrucks()
        {
            var response = await _truckService.GetAllTrucks();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("GetTruckDocuments")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponseModel<IEnumerable<string>>>> GetTruckDocuments(string truckId)
        {
            var response = await _truckService.GetTruckDocuments(truckId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("AssignDriverToTruck")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponseModel<bool>>> AssignDriverToTruck(AssignDriverToTruckRequestModel model)
        {
            var response = await _truckService.AssignDriverToTruck(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("UpdateTruckStatus")]
        [Authorize(Roles = "admin,manager,chiefmanager")]
        public async Task<ActionResult<ApiResponseModel<string>>> UpdateTruckStatus(string truckId, UpdateTruckStatusRequestModel model)
        {
            var response = await _truckService.UpdateTruckStatus(truckId, model);
            return StatusCode(response.StatusCode, response);
        }
         [HttpGet("GetTrucksByOwnersId")]
        [Authorize(Roles = "transporter")]
        public async Task<ActionResult<ApiResponseModel<List<AllTruckResponseModel>>>> GetTrucksByOwnersId(string ownersId)
        {
            var response = await _truckService.GetTrucksByOwnersId(ownersId);
            return StatusCode(response.StatusCode, response);
        }
        

}