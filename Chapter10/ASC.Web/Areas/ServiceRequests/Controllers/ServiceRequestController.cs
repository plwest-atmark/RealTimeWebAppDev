﻿using Microsoft.AspNetCore.Mvc;
using ASC.Web.Controllers;
using ASC.Business.Interfaces;
using ASC.Models.Models;
using ASC.Utilities;
using System;
using System.Threading.Tasks;
using ASC.Web.Areas.ServiceRequests.Models;
using AutoMapper;
using ASC.Web.Data;
using System.Linq;
using ASC.Models.BaseTypes;
using Microsoft.AspNetCore.Identity;
using ASC.Web.Models;
using ASC.Web.Services;
using System.Collections.Generic;

namespace ASC.Web.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    public class ServiceRequestController : BaseController
    {
        private readonly IServiceRequestOperations _serviceRequestOperations;
        private readonly IMapper _mapper;
        private readonly IMasterDataCacheOperations _masterData;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        public ServiceRequestController(IServiceRequestOperations operations, 
            IMapper mapper, 
            IMasterDataCacheOperations masterData, 
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            _serviceRequestOperations = operations;
            _mapper = mapper;
            _masterData = masterData;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [HttpGet]
        public async Task<IActionResult> ServiceRequest()
        {
            var masterData = await _masterData.GetMasterDataCacheAsync();
            ViewBag.VehicleTypes = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleType.ToString()).ToList();
            ViewBag.VehicleNames = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleName.ToString()).ToList();
            return View(new NewServiceRequestViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ServiceRequest(NewServiceRequestViewModel request)
        {
            if (!ModelState.IsValid)
            {
                var masterData = await _masterData.GetMasterDataCacheAsync();
                ViewBag.VehicleTypes = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleType.ToString()).ToList();
                ViewBag.VehicleNames = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleName.ToString()).ToList();
                return View(request);
            }

            // Map the view model to Azure model
            var serviceRequest = _mapper.Map<NewServiceRequestViewModel, ServiceRequest>(request);

            // Set RowKey, PartitionKye, RequestedDate, Status properties
            serviceRequest.PartitionKey = HttpContext.User.GetCurrentUserDetails().Email;
            serviceRequest.RowKey = Guid.NewGuid().ToString();
            serviceRequest.RequestedDate = request.RequestedDate;
            serviceRequest.Status = Status.New.ToString();

            await _serviceRequestOperations.CreateServiceRequestAsync(serviceRequest);

            return RedirectToAction("Dashboard", "Dashboard", new { Area = "ServiceRequests" });
        }

        [HttpGet]
        public async Task<IActionResult> ServiceRequestDetails(string id)
        {
            var serviceRequestDetails = await _serviceRequestOperations.GetServiceRequestByRowKey(id);

            // Access Check
            if (HttpContext.User.IsInRole(Roles.Engineer.ToString())
                && serviceRequestDetails.ServiceEngineer != HttpContext.User.GetCurrentUserDetails().Email)
            {
                throw new UnauthorizedAccessException();
            }

            if (HttpContext.User.IsInRole(Roles.User.ToString())
                && serviceRequestDetails.PartitionKey != HttpContext.User.GetCurrentUserDetails().Email)
            {
                throw new UnauthorizedAccessException();
            }

            var serviceRequestAuditDetails = await _serviceRequestOperations.GetServiceRequestAuditByPartitionKey(
                serviceRequestDetails.PartitionKey + "-" + id);

            // Select List Data
            var masterData = await _masterData.GetMasterDataCacheAsync();
            ViewBag.VehicleTypes = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleType.ToString()).ToList();
            ViewBag.VehicleNames = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleName.ToString()).ToList();
            ViewBag.Status = Enum.GetValues(typeof(Status)).Cast<Status>().Select(v => v.ToString()).ToList();
            ViewBag.ServiceEngineers = await _userManager.GetUsersInRoleAsync(Roles.Engineer.ToString());

            return View(new ServiceRequestDetailViewModel
            {
                ServiceRequest = _mapper.Map<ServiceRequest, UpdateServiceRequestViewModel>(serviceRequestDetails),
                ServiceRequestAudit = serviceRequestAuditDetails.OrderByDescending(p => p.Timestamp).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateServiceRequestDetails(UpdateServiceRequestViewModel serviceRequest)
        {
            var originalServiceRequest = await _serviceRequestOperations.GetServiceRequestByRowKey(serviceRequest.RowKey);
            originalServiceRequest.RequestedServices = serviceRequest.RequestedServices;

            // Update Status only if user role is either Admin or Engineer
            // Or Customer can update the status if it is only in Pending Customer Approval.
            if (HttpContext.User.IsInRole(Roles.Admin.ToString()) || 
                HttpContext.User.IsInRole(Roles.Engineer.ToString()) || 
                (HttpContext.User.IsInRole(Roles.User.ToString()) && originalServiceRequest.Status == Status.PendingCustomerApproval.ToString()))
            {
                originalServiceRequest.Status = serviceRequest.Status;
            }

            // Update Service Engineer field only if user role is Admin
            if (HttpContext.User.IsInRole(Roles.Admin.ToString()))
            {
                originalServiceRequest.ServiceEngineer = serviceRequest.ServiceEngineer;
            }

            await _serviceRequestOperations.UpdateServiceRequestAsync(originalServiceRequest);

            if(HttpContext.User.IsInRole(Roles.Admin.ToString()) ||
                HttpContext.User.IsInRole(Roles.Engineer.ToString()) || originalServiceRequest.Status == Status.PendingCustomerApproval.ToString())
            {
                await _emailSender.SendEmailAsync(originalServiceRequest.PartitionKey,
                        "Your Service Request is almost completed!!!",
                        "Please visit the ASC application and review your Service request.");
            }

            return RedirectToAction("ServiceRequestDetails", "ServiceRequest",
                new { Area = "ServiceRequests", Id = serviceRequest.RowKey });
        }

        public async Task<IActionResult> CheckDenialService(DateTime requestedDate)
        {
            var serviceRequests = await _serviceRequestOperations.GetServiceRequestsByRequestedDateAndStatus(
                DateTime.UtcNow.AddDays(-90),
                new List<string>() { Status.Denied.ToString() },
                HttpContext.User.GetCurrentUserDetails().Email);

            if (serviceRequests.Any())
                return Json(data: $"There is a denied service request for you in last 90 days. Please contact ASC Admin.");

            return Json(data: true);
        }

        [HttpGet]
        public IActionResult SearchServiceRequests()
        {
            return View(new SearchServiceRequestsViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> SearchServiceRequestResults(string email, DateTime? requestedDate)
        {
            List<ServiceRequest> results = new List<ServiceRequest>();
            if(String.IsNullOrEmpty(email) && !requestedDate.HasValue)
                return Json(new { data = results });

            if(HttpContext.User.IsInRole(Roles.Admin.ToString()))
                results = await _serviceRequestOperations.GetServiceRequestsByRequestedDateAndStatus(requestedDate, null, email);
            else
                results = await _serviceRequestOperations.GetServiceRequestsByRequestedDateAndStatus(requestedDate, null, email, HttpContext.User.GetCurrentUserDetails().Email);
            
            return Json(new { data = results.OrderByDescending(p => p.RequestedDate).ToList() });
        }
    }
}
