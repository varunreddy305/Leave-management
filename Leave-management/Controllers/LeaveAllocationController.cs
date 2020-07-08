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
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Leave_management.Controllers
{
    [Authorize]
    public class LeaveAllocationController : Controller
    {
        private readonly ILeaveAllocationRepository _leaveAllocationRepository;
        private readonly ILeaveTypeRepository _leaveTypeRepository;
        private readonly IMapper _map;
        private readonly UserManager<Employee> _userManager;
        private int count;

        public LeaveAllocationController(ILeaveAllocationRepository leaveRepo, ILeaveTypeRepository leaveTypeRepo, IMapper mapper, UserManager<Employee> userManager)
        {
            _leaveAllocationRepository = leaveRepo;
            _leaveTypeRepository = leaveTypeRepo;
            _map = mapper;
            _userManager = userManager;
        }

        // GET: LeaveAllocationController
        public async Task<ActionResult> Index()
        {
            var leaveTypes = await _leaveTypeRepository.FindAll();
            var mappedLeaveTypes = _map.Map<List<LeaveType>, List<LeaveTypeViewModel>>(leaveTypes.ToList());
            var model = new CreateLeaveAllocationVM { LeaveTypes = mappedLeaveTypes, NumberUpdated = count };
            return View(model);
        }

        public async Task<ActionResult> SetLeave(int id)
        {
            var leaveType = await _leaveTypeRepository.FindById(id);
            var employee = await _userManager.GetUsersInRoleAsync("Employee");
            foreach (var emp in employee)
            {
                var allocationCheck = await _leaveAllocationRepository.CheckAllocation(id, emp.Id);
                if (allocationCheck)
                    continue;
                var allocation = new LeaveAllocationViewModel
                {
                    DataCreated = DateTime.Now,
                    EmployeeId = emp.Id,
                    LeaveTypeId = id,
                    NumberOfDays = leaveType.DefaultDays,
                    Period = DateTime.Now.Year
                };
                count++;
                var leaveAllocation = _map.Map<LeaveAllocation>(allocation);
                await _leaveAllocationRepository.Create(leaveAllocation);
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<ActionResult> ListEmployees()
        {
            var employee = await _userManager.GetUsersInRoleAsync("Employee");
            var model = _map.Map < List < EmployeeViewModel >> (employee);
            return View(model);
        }

        // GET: LeaveAllocationController/Details/5
        public async Task<ActionResult> Details(string id)
        {
            var employee = _map.Map<EmployeeViewModel>(await _userManager.FindByIdAsync(id));
            var allocations = _map.Map<List<LeaveAllocationViewModel>>(await _leaveAllocationRepository.GetLeaveAllocationsByEmployee(id));
            var model = new ViewAllocationVM { Employee = employee, LeaveAllocations = allocations };
            return View(model);
        }

        // GET: LeaveAllocationController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LeaveAllocationController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
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

        // GET: LeaveAllocationController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var leaveAllocations = _map.Map<EditLeaveAllocationViewModel>(await _leaveAllocationRepository.FindById(id));
            return View(leaveAllocations);
        }

        // POST: LeaveAllocationController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditLeaveAllocationViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ModelState.AddModelError("", "Something went wrong");
                    return View(model);
                }
                var record = await _leaveAllocationRepository.FindById(model.Id);
                record.NumberOfDays = model.NumberOfDays;
                var isSucess = await _leaveAllocationRepository.Update(record);
                if (!isSucess)
                {
                    ModelState.AddModelError("", "Error while saving");
                    return View(model);
                }
                return RedirectToAction(nameof(Details),new { id = model.EmployeeId});
            }
            catch
            {
                ModelState.AddModelError("", "Something went wrong");
                return View(model);
            }
        }

        // GET: LeaveAllocationController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveAllocationController/Delete/5
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
