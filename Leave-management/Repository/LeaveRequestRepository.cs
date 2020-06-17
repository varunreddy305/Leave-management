﻿using Leave_management.Contracts;
using Leave_management.Data;
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
        public bool Create(LeaveRequest entity)
        {
            _db.LeaveRequests.Add(entity);
            return Save();
        }

        public bool Delete(LeaveRequest entity)
        {
            _db.LeaveRequests.Remove(entity);
            return Save();
        }

        public ICollection<LeaveRequest> FindAll()
        {
            return _db.LeaveRequests.ToList();
        }

        public LeaveRequest FindById(int id)
        {
            return _db.LeaveRequests.FirstOrDefault(x => x.Id == id);
        }

        public bool isExists(int id)
        {
            var isRecordExist = _db.LeaveRequests.Any(x => x.Id == id);
            return isRecordExist;
        }

        public bool Save()
        {
            return _db.SaveChanges() > 0;
        }

        public bool Update(LeaveRequest entity) 
        {
            _db.LeaveRequests.Update(entity);
            return Save();
        }
    }
}
