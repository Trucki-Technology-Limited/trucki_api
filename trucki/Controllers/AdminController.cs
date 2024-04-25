﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trucki.Entities;
using trucki.Interfaces.IServices;
using trucki.Models.RequestModel;
using trucki.Models.ResponseModels;

namespace trucki.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpPost("CreateNewBusiness")]
        public async Task<IActionResult> CreateNewBusiness([FromBody] CreateNewBusinessRequestModel model)
        {
            var result = await _adminService.CreateNewBusiness(model);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("GetAllBusiness")]
        public async Task<ActionResult<ApiResponseModel<AllBusinessResponseModel>>> GetAllBusiness()
        {
            var business = await _adminService.GetAllBusiness();
            return StatusCode(business.StatusCode, business);
        }

        [HttpPost("AddRouteToBusiness")]
        public async Task<IActionResult> AddRouteToBusiness([FromBody] AddRouteToBusinessRequestModel model)
        {
            var result = await _adminService.AddRouteToBusiness(model);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("GetBusinessById")]
        public async Task<ActionResult<ApiResponseModel<BusinessResponseModel>>> GetBusinessById(string businessId)
        {
            var business = await _adminService.GetBusinessById(businessId);
            return StatusCode(business.StatusCode, business);
        }

        [HttpPost("EditBusiness")]
        public async Task<ActionResult<ApiResponseModel<bool>>> EditBusiness([FromBody] EditBusinessRequestModel model)
        {
            var response = await _adminService.EditBusiness(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("DeleteBusiness")]
        public async Task<ActionResult<ApiResponseModel<bool>>> DeleteBusiness([FromQuery] string businessId)
        {
            var response = await _adminService.DeleteBusiness(businessId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("DisableBusiness")]
        public async Task<ActionResult<ApiResponseModel<bool>>> DisableBusiness([FromQuery] string businessId)
        {
            var response = await _adminService.DisableBusiness(businessId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("EnableBusiness")]
        public async Task<ActionResult<ApiResponseModel<bool>>> EnableBusiness([FromQuery] string businessId)
        {
            var response = await _adminService.EnableBusiness(businessId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("EditRoute")]
        public async Task<ActionResult<ApiResponseModel<bool>>> EditRoute([FromBody] EditRouteRequestModel model)
        {
            var response = await _adminService.EditRoute(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("DeleteRoute")]
        public async Task<ActionResult<ApiResponseModel<bool>>> DeleteRoute([FromQuery] string routeId)
        {
            var response = await _adminService.DeleteRoute(routeId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("AddManager")]
        public async Task<ActionResult<ApiResponseModel<bool>>> AddManager([FromBody] AddManagerRequestModel model)
        {
            var response = await _adminService.AddManager(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("AddDriver")]
        public async Task<ActionResult<ApiResponseModel<bool>>> AddDriver([FromForm] AddDriverRequestModel model)
        {
            var response = await _adminService.AddDriver(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("EditDriver")]
        public async Task<ActionResult<ApiResponseModel<bool>>> EditDriver([FromForm] EditDriverRequestModel model)
        {
            var response = await _adminService.EditDriver(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetAllManager")]
        public async Task<ActionResult<ApiResponseModel<AllManagerResponseModel>>> GetAllManager()
        {
            var business = await _adminService.GetAllManager();
            return StatusCode(business.StatusCode, business);
        }

        [HttpGet("GetManager")]
        public async Task<ActionResult<ApiResponseModel<AllManagerResponseModel>>> GetManagerById(string id)
        {
            var manager = await _adminService.GetManagerById(id);
            return StatusCode(manager.StatusCode, manager);
        }

        [HttpPost("EditManager")]
        public async Task<ActionResult<ApiResponseModel<bool>>> EditManager([FromBody] EditManagerRequestModel model)
        {
            var response = await _adminService.EditManager(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("DeleteManager")]
        public async Task<ActionResult<ApiResponseModel<bool>>> DeleteManager([FromQuery] string managerId)
        {
            var response = await _adminService.DeactivateManager(managerId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetAllTruckOwners")]
        public async Task<ActionResult<ApiResponseModel<List<TruckOwnerResponseModel>>>> GetAllTruckOwners()
        {
            var response = await _adminService.GetAllTruckOwners();
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("DeleteTruckOwner")]
        public async Task<ActionResult<ApiResponseModel<bool>>> DeleteTruckOwner([FromQuery] string ownerId)
        {
            var response = await _adminService.DeleteTruckOwner(ownerId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("EditTruckOwner")]
        public async Task<ActionResult<ApiResponseModel<bool>>> EditTruckOwner(
            [FromForm] EditTruckOwnerRequestBody model)
        {
            var response = await _adminService.EditTruckOwner(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetTruckOwnerById")]
        public async Task<ActionResult<ApiResponseModel<TruckOwnerResponseModel>>> GetTruckOwnerById(
            [FromQuery] string ownerId)
        {
            var response = await _adminService.GetTruckOwnerById(ownerId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("CreateNewTruckOwner")]
        public async Task<ActionResult<ApiResponseModel<bool>>> CreateNewTruckOwner(
            [FromForm] AddTruckOwnerRequestBody model)
        {
            var response = await _adminService.CreateNewTruckOwner(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetAllDrivers")]
        public async Task<ActionResult<ApiResponseModel<AllDriverResponseModel>>> GetDrivers()
        {
            var response = await _adminService.GetAllDrivers();
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetDriverById")]
        public async Task<ActionResult<ApiResponseModel<AllDriverResponseModel>>> GetDriverById(string id)
        {
            var response = await _adminService.GetDriverById(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("SearchDrivers")]
        public async Task<ActionResult<ApiResponseModel<IEnumerable<AllDriverResponseModel>>>> SearchDrivers(string searchWords)
        {
            var response = await _adminService.SearchDrivers(searchWords);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("DeactivvateDriver")]
        public async Task<ActionResult<ApiResponseModel<bool>>> DeactivateDriver(string driverId)
        {
            var response = await _adminService.DeactivateDriver(driverId);
            return StatusCode(response.StatusCode, response);
        }


        [HttpGet("SearchManagers")]
        public async Task<ActionResult<ApiResponseModel<IEnumerable<AllManagerResponseModel>>>> SearchManagers(string searchWords)
        {
            var response = await _adminService.SearchManagers(searchWords);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("SearchBusinesses")]
        public async Task<ActionResult<ApiResponseModel<IEnumerable<AllManagerResponseModel>>>> SearchBusinesses(string searchWords)
        {
            var response = await _adminService.SearchBusinesses(searchWords);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("CreateNewOfficer")]
        public async Task<ActionResult<ApiResponseModel<string>>> CreateNewOfficer([FromBody] AddOfficerRequestModel model)
        {
            var response = await _adminService.AddOfficer(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetAllFieldOfficers")]
        public async Task<ActionResult<ApiResponseModel<PaginatedListDto<AllOfficerResponseModel>>>> GetAllFieldOfficers(int page, int size)
        {
            var response = await _adminService.GetAllFieldOfficers(page, size);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("EditOfficer")]
        public async Task<ActionResult<ApiResponseModel<bool>>> EditManager([FromBody] EditOfficerRequestModel model)
        {
            var response = await _adminService.EditOfficer(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("AddNewTruck")]
        public async Task<ActionResult<ApiResponseModel<string>>> AddNewTruck([FromForm] AddTruckRequestModel model)
        {
            var response = await _adminService.AddNewTruck(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("EditTruck")]
        public async Task<ActionResult<ApiResponseModel<bool>>> EditTruck([FromForm]EditTruckRequestModel model)
        {
            var response = await _adminService.EditTruck(model);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("DeleteTruck")]
        public async Task<ActionResult<ApiResponseModel<string>>> DeleteTruck(string truckId)
        {
            var response = await _adminService.DeleteTruck(truckId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("GetTruckById")]
        public async Task<ActionResult<ApiResponseModel<AllTruckResponseModel>>> GetTruckById(string truckId)
        {
            var response = await _adminService.GetTruckById(truckId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("SearchTrucks")]
        public async Task<ActionResult<ApiResponseModel<IEnumerable<AllTruckResponseModel>>>> SearchTruck(string? searchWords)
        {
            var response = await _adminService.SearchTruck(searchWords);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("GetAllTrucks")]
        public async Task<ActionResult<ApiResponseModel<IEnumerable<AllTruckResponseModel>>>> GetAllTrucks()
        {
            var response = await _adminService.GetAllTrucks();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("GetTruckDocuments")]
        public async Task<ActionResult<ApiResponseModel<IEnumerable<string>>>> GetTruckDocuments(string truckId)
        {
            var response = await _adminService.GetTruckDocuments(truckId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("AssignDriverToTruck")]
        public async Task<ActionResult<ApiResponseModel<bool>>> AssignDriverToTruck(AssignDriverToTruckRequestModel model)
        {
            var response = await _adminService.AssignDriverToTruck(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("UpdateTruckStatus")]
        public async Task<ActionResult<ApiResponseModel<string>>> UpdateTruckStatus(string truckId, UpdateTruckStatusRequestModel model)
        {
            var response = await _adminService.UpdateTruckStatus(truckId, model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetOfficerById")]
        public async Task<ActionResult<ApiResponseModel<AllOfficerResponseModel>>> GetOfficerById(string officerId)
        {
            var response = await _adminService.GetOfficerById(officerId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("DeleteOfficer")]
        public async Task<ActionResult<ApiResponseModel<string>>> DeleteOfficer(string officerId)
        {
            var response = await _adminService.DeleteOfficers(officerId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("SearchOfficers")]
        public async Task<ActionResult<ApiResponseModel<IEnumerable<AllOfficerResponseModel>>>> SearchOfficer(string? searchWords)
        {
            var response = await _adminService.SearchOfficer(searchWords);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("AddNewCustomer")]
        public async Task<ActionResult<ApiResponseModel<string>>> AddNewCustomer([FromBody] AddCustomerRequestModel model)
        {
            var response = await _adminService.AddNewCustomer(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("EditCustomer")]
        public async Task<ActionResult<ApiResponseModel<string>>> EditCustomer([FromBody] EditCustomerRequestModel model)
        {
            var response = await _adminService.EditCustomer(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetCustomerById")]
        public async Task<ActionResult<ApiResponseModel<AllCustomerResponseModel>>> GetCustomerById(string customerId)
        {
            var response = await _adminService.GetCustomerById(customerId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetAllCustomers")]
        public async Task<ActionResult<ApiResponseModel<List<AllCustomerResponseModel>>>> GetAllCustomers()
        {
            var response = await _adminService.GetAllCustomers();
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("RemoveCustomer")]
        public async Task<ActionResult<ApiResponseModel<string>>> DeleteCustomer(string customerId)
        {
            var response = await _adminService.DeleteCustomer(customerId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("CreateOrder")]
        public async Task<ActionResult<ApiResponseModel<string>>> CreateNewOrder([FromBody] CreateOrderRequestModel model)
        {
            //var userId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            //string managerId = await _adminService.GetManagerIdAsync(userId);
            string managerId = model.ManagerId;

            var response = await _adminService.CreateNewOrder(model, managerId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("UpdateOrder")]
        public async Task<ActionResult<ApiResponseModel<string>>> EditOrder([FromBody] EditOrderRequestModel model)
        {
            var response = await _adminService.EditOrder(model);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("AssignTruckToOrder")]
        public async Task<ActionResult<ApiResponseModel<string>>> AssignTruckToOrders([FromBody] AssignTruckRequestModel model)
        {
            var response = await _adminService.AssignTruckToOrder(model);
            return response;
        }
        [HttpGet("GetAllOrders")]
        public async Task<ActionResult<ApiResponseModel<IEnumerable<AllOrderResponseModel>>>> GetAllOrders()
        {
            var response = await _adminService.GetAllOrders();
            return StatusCode(response.StatusCode, response);   
        }
        [HttpGet("GetOrderById")]
        public async Task<ActionResult<ApiResponseModel<OrderResponseModel>>> GetOrderById(string orderId)
        {
            var response = await _adminService.GetOrderById(orderId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("GetDashboardData")]
        public async Task<ActionResult<ApiResponseModel<DashboardSummaryResponse>>> GetDashboardData()
        {
            var response = await _adminService.GetDashBoardData();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("SearchTruckOwners")]
        public async Task<ActionResult<ApiResponseModel<IEnumerable<TruckOwnerResponseModel>>>> SearchTruckOwners(string searchWords)
        {
            var response = await _adminService.SearchTruckOwners(searchWords);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("SearchCustomers")]
        public async Task<ActionResult<IEnumerable<AllCustomerResponseModel>>> SearchCustomers(string searchWords)
        {
            var response = await _adminService.SearchCustomers(searchWords);
            return StatusCode(response.StatusCode, response);
        }
    }
}