using Leave_management.Contracts;
using Leave_management.Data;
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

        public bool CheckAllocation(int leaveTypeId, string employeeId)
        {
            var period = DateTime.Now.Year;
            IEnumerable<LeaveAllocation> result = from leaveAllocation in _db.LeaveAllocations.ToList()
                                                  where (leaveAllocation.EmployeeId == employeeId && leaveAllocation.LeaveTypeId == leaveTypeId && leaveAllocation.Period == period)
                                                  select leaveAllocation;

            //return FindAll().Where(x => x.EmployeeId == employeeId && x.LeaveTypeId == leaveTypeId && x.Period == period).Any();
            return result.Any();
        }

        public bool Create(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Add(entity);
            return Save();
        }

        public bool Delete(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Remove(entity);
            return Save();
        }

        public ICollection<LeaveAllocation> FindAll()
        {
            return _db.LeaveAllocations.ToList();
        }

        public LeaveAllocation FindById(int id)
        {
            return _db.LeaveAllocations.FirstOrDefault(x => x.Id == id);
        }

        public bool isExists(int id)
        {
            var isRecordExist = _db.LeaveAllocations.Any(x => x.Id == id);
            return isRecordExist;
        }

        public bool Save()
        {
            return _db.SaveChanges() > 0;
        }

        public bool Update(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Update(entity);
            return Save();
        }
    }
}
