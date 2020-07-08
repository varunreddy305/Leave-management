using Leave_management.Contracts;
using Leave_management.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Leave_management.Repository
{
    public class LeaveRequestRepository : ILeaveRequestRepository
    {
        private readonly ApplicationDbContext _db;
        public LeaveRequestRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<bool> Create(LeaveRequest entity)
        {
            _db.LeaveRequests.Add(entity);
            return await Save();
        }

        public async Task<bool> Delete(LeaveRequest entity)
        {
            _db.LeaveRequests.Remove(entity);
            return await Save();
        }

        public async Task<ICollection<LeaveRequest>> FindAll()
        {
            return await _db.LeaveRequests
                .Include(x=>x.RequestingEmployee)
                .Include(x=>x.ApprovedBy)
                .Include(x=>x.LeaveType)
                .ToListAsync();
        }

        public async Task<LeaveRequest> FindById(int id)
        {
            return await _db.LeaveRequests
                .Include(x => x.RequestingEmployee)
                .Include(x => x.ApprovedBy)
                .Include(x => x.LeaveType)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<ICollection<LeaveRequest>> GetLeaveRequestOfAnEmployee(string id)
        {
            var leaveTypes = await FindAll();
            return leaveTypes.Where(x => x.RequestingEmployeeId == id).ToList();
        }

        public async Task<bool> isExists(int id)
        {
            var isRecordExist = await _db.LeaveRequests.AnyAsync(x => x.Id == id);
            return isRecordExist;
        }

        public async Task<bool> Save()
        {
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> Update(LeaveRequest entity) 
        {
            _db.LeaveRequests.Update(entity);
            return await Save();
        }
    }
}
