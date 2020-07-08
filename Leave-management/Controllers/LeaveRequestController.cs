using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Leave_management.Contracts;
using Leave_management.Data;
using Leave_management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Leave_management.Controllers
{
    [Authorize]
    public class LeaveRequestController : Controller
    {
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly ILeaveTypeRepository _leaveTypeRepo;
        private readonly IMapper _map;
        private readonly ILeaveAllocationRepository _leaveAllocationRepo;
        private readonly UserManager<Employee> _userManager;

        public LeaveRequestController
            (
                ILeaveRequestRepository leaveRequestRepo, 
                IMapper map, 
                UserManager<Employee> user, 
                ILeaveTypeRepository leaveType, 
                ILeaveAllocationRepository leaveAllocationRepo
            )
        {
            _leaveRequestRepository = leaveRequestRepo;
            _map = map;
            _userManager = user;
            _leaveTypeRepo = leaveType;
            _leaveAllocationRepo = leaveAllocationRepo;
        }

        [Authorize(Roles ="Administrator")]
        // GET: LeaveRequestController
        public async Task<ActionResult> Index()
        {
            var leaveRequests =  await _leaveRequestRepository.FindAll();
            var leaveRequestModel = from leaveRequest in leaveRequests
                                    orderby leaveRequest.DateRequested
                                    select leaveRequest;
           var model = _map.Map<List<LeaveRequestViewModel>>(leaveRequestModel);
            var adminVM = new AdminLeaveRequestVM
            {
                TotalRequest = model.Count,
                ApprovedRequest = model.Count(x => x.Approved == true),
                PendingRequest = model.Count(x => x.Approved == null),
                RejectedRequest = model.Count(x=>x.Approved == false),
                LeaveRequests = model,
            };
            return View(adminVM);
        }

        // GET: LeaveRequestController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var leaveRequest = await _leaveRequestRepository.FindById(id);
            var model = _map.Map<LeaveRequestViewModel>(leaveRequest);
            return View(model);
        }

        public async Task<ActionResult> Approve(int id)
        {
            try
            {
                var leaveRequest = await _leaveRequestRepository.FindById(id);
                var allocation =  await _leaveAllocationRepo.GetLeaveAllocationsByEmployeeAndType(leaveRequest.RequestingEmployeeId, leaveRequest.LeaveTypeId);
                int daysRequested = (int)(leaveRequest.EndDate - leaveRequest.StartDate).TotalDays;
                allocation.NumberOfDays -= daysRequested;
                leaveRequest.Approved = true;
                var user = await _userManager.GetUserAsync(User);
                leaveRequest.ApprovedById = user.Id;
                leaveRequest.DateActioned = DateTime.Now;
                await _leaveAllocationRepo.Update(allocation);
                await _leaveRequestRepository.Update(leaveRequest);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Index));
            }
            
        }

        public async Task<ActionResult> Reject(int id)
        {
            try
            {
                var leaveRequest = await _leaveRequestRepository.FindById(id);
                leaveRequest.Approved = false;
                leaveRequest.ApprovedById = _userManager.GetUserId(User);
                leaveRequest.DateActioned = DateTime.Now;
                await _leaveRequestRepository.Update(leaveRequest);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<ActionResult> MyLeave()
        {
            var employee = await _userManager.GetUserAsync(User);
            var leaveAllocations = await _leaveAllocationRepo.GetLeaveAllocationsByEmployee(employee.Id);
            var leaveRequests =  await _leaveRequestRepository.GetLeaveRequestOfAnEmployee(employee.Id);
            var leaveAllocationsModel = _map.Map<List<LeaveAllocationViewModel>>(leaveAllocations);
            var leaveRequestModel = _map.Map<List<LeaveRequestViewModel>>(leaveRequests);
            var myLeaveModel = new MyLeaveRequestVM
            {
                LeaveAllocationViewModels = leaveAllocationsModel,
                LeaveRequests = leaveRequestModel
            };
            return View(myLeaveModel);
        }

        public async Task<ActionResult> CancelRequest(int id)
        {
            try
            {
                var leaveRequest = await _leaveRequestRepository.FindById(id);
                var allocation = await _leaveAllocationRepo.GetLeaveAllocationsByEmployeeAndType(leaveRequest.RequestingEmployeeId, leaveRequest.LeaveTypeId);
                int daysRequested = (int)(leaveRequest.EndDate - leaveRequest.StartDate).TotalDays;
                allocation.NumberOfDays += daysRequested;
                await _leaveAllocationRepo.Update(allocation);
                await _leaveRequestRepository.Delete(leaveRequest);
                return RedirectToAction(nameof(MyLeave));
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(MyLeave));
            }
        }

        // GET: LeaveRequestController/Create
        public async Task<ActionResult> Create()
        {
            var leaveTypes = await _leaveTypeRepo.FindAll();
            //var leaveTypeItems = from leaveType in leaveTypes
            //                     orderby leaveType.Name
            //                     select new SelectListItem { Text = leaveType.Name, Value = leaveType.Id.ToString() };
            var leaveTypeItems = leaveTypes.Select(q => new SelectListItem { Text = q.Name, Value = q.Id.ToString() });
            var model = new CreateLeaveRequestVM
            {
                LeaveTypes = leaveTypeItems,
                StartDate = DateTime.Now.ToString("MM/dd/yyyy"),
                EndDate = DateTime.Now.AddDays(1).ToString("MM/dd/yyyy")
            };
            return View(model);
        }

        // POST: LeaveRequestController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateLeaveRequestVM createLeave)
        {
            try
            {
                var startDate = Convert.ToDateTime(createLeave.StartDate);
                var endDate = Convert.ToDateTime(createLeave.EndDate);
                var leaveTypes = await _leaveTypeRepo.FindAll();
                var leaveTypeItems = leaveTypes.Select(q => new SelectListItem { Text = q.Name, Value = q.Id.ToString() });
                createLeave.LeaveTypes = leaveTypeItems;

                if (!ModelState.IsValid)
                {
                    ModelState.AddModelError("", "Something went wrong");
                    return View(createLeave);
                }
                if (DateTime.Compare(startDate, endDate) > 0)
                {
                    ModelState.AddModelError("", "Cannot be a future date");
                    return View(createLeave);
                }
                var employee = await _userManager.GetUserAsync(User);
                var allocation = await _leaveAllocationRepo.GetLeaveAllocationsByEmployeeAndType(employee.Id, createLeave.LeaveTypeId);
                int daysRequested = (int)(endDate - startDate).TotalDays; 
                if(daysRequested > allocation.NumberOfDays)
                {
                    ModelState.AddModelError("", "You dont have sufficient days");
                    return View(createLeave);
                }
                var leaveRequestModel = new LeaveRequestViewModel
                {
                    LeaveTypeId = createLeave.LeaveTypeId,
                    RequestingEmployeeId = employee.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    Approved = null,
                    DateRequested = DateTime.Now,
                    DateActioned = DateTime.Now
                };
                var leaveRequest = _map.Map<LeaveRequest>(leaveRequestModel);
                var isSuccess = await _leaveRequestRepository.Create(leaveRequest);
                if (!isSuccess)
                {
                    ModelState.AddModelError("", "Something went wrong");
                    return View(createLeave);
                }
                return RedirectToAction(nameof(MyLeave));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Something went wrong");
                return View(createLeave);
            }
        }

        // GET: LeaveRequestController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LeaveRequestController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveRequestController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveRequestController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
