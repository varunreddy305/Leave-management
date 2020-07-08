using Leave_management.Contracts;
using Leave_management.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Leave_management.Repository
{
    public class LeaveAllocationRepository : ILeaveAllocationRepository
    {
        private readonly ApplicationDbContext _db;
        public LeaveAllocationRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> CheckAllocation(int leaveTypeId, string employeeId)
        {
            var period = DateTime.Now.Year;
            var allocations = await FindAll();
            IEnumerable<LeaveAllocation> result = allocations.Where(x => x.EmployeeId == employeeId && x.LeaveTypeId == leaveTypeId && x.Period == period);

            //return FindAll().Where(x => x.EmployeeId == employeeId && x.LeaveTypeId == leaveTypeId && x.Period == period).Any();
            return result.Any();
        }

        public async Task<bool> Create(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Add(entity);
            return await Save();
        }

        public async Task<bool> Delete(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Remove(entity);
            return await Save();
        }

        public async Task<ICollection<LeaveAllocation>> FindAll()
        {
            return await _db.LeaveAllocations.Include(x=>x.LeaveType).Include(x=>x.Employee).ToListAsync();
        }

        public async Task<LeaveAllocation> FindById(int id)
        {
            return await _db.LeaveAllocations
                .Include(x=>x.Employee)
                .Include(x=>x.LeaveType)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<LeaveAllocation>> GetLeaveAllocationsByEmployee(string id)
        {
            var leaveAllocations = await FindAll();
            return leaveAllocations.Where(x => x.EmployeeId == id).ToList();
        }

        public async Task<LeaveAllocation> GetLeaveAllocationsByEmployeeAndType(string id, int leaveTypeId)
        {
            var period = DateTime.Now.Year;
            var leaveAllocationOfAnEmployee = await FindAll();
            return leaveAllocationOfAnEmployee.FirstOrDefault(q => q.EmployeeId == id && q.Period == period && q.LeaveTypeId == leaveTypeId);
        }

        public async Task<bool> isExists(int id)
        {
            var isRecordExist = await _db.LeaveAllocations.AnyAsync(x => x.Id == id);
            return isRecordExist;
        }

        public async Task<bool> Save()
        {
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> Update(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Update(entity);
            return await Save();
        }
    }
}
