using LibraryManagementSystem.Models.Entity;
using LibraryManagementSystem.Models.ViewModel;
using LibraryManagementSystem.Services.Context;
using LibraryManagementSystem.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryManagementSystem.Services.Impl
{
    public class MemberService : IMemberService
    {
        private LibraryDbContext db = new LibraryDbContext();

        public List<MemberViewModel> GetAllMembers()
        {
            var members = db.Members
                .Join(
                    db.Users,
                    member => member.UserId,
                    user => user.Id,
                    (member, user) => new MemberViewModel
                    {
                        Id = member.Id,
                        UserId = member.UserId,
                        FullName = user.FullName,
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role,
                        Status = user.Status,
                        MemberCode = member.MemberCode,
                        Phone = member.Phone,
                        Address = member.Address,
                        ClassName = member.ClassName,
                        ApprovedBy = member.ApprovedBy,
                        ApprovedAt = member.ApprovedAt
                    }
                )
                .OrderBy(x => x.FullName)
                .ToList();

            return members;
        }

        public List<MemberViewModel> GetPendingMembers()
        {
            var pendingMembers = db.Members
                .Join(
                    db.Users,
                    member => member.UserId,
                    user => user.Id,
                    (member, user) => new MemberViewModel
                    {
                        Id = member.Id,
                        UserId = member.UserId,
                        FullName = user.FullName,
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role,
                        Status = user.Status,
                        MemberCode = member.MemberCode,
                        Phone = member.Phone,
                        Address = member.Address,
                        ClassName = member.ClassName,
                        ApprovedBy = member.ApprovedBy,
                        ApprovedAt = member.ApprovedAt
                    }
                )
                .Where(x => x.Status == "Pending")
                .OrderBy(x => x.FullName)
                .ToList();

            return pendingMembers;
        }

        public object ApproveMember(int id, int approvedBy)
        {
            if (approvedBy <= 0)
            {
                throw new Exception("ApprovedBy wajib diisi");
            }

            var librarian = db.Users.Find(approvedBy);

            if (librarian == null)
            {
                throw new Exception("User pustakawan tidak ditemukan");
            }

            if (librarian.Role != "Librarian" || librarian.Status != "Active")
            {
                throw new Exception("Hanya pustakawan aktif yang bisa menyetujui member");
            }

            var member = db.Members.Find(id);

            if (member == null)
            {
                return null;
            }

            var user = db.Users.Find(member.UserId);

            if (user == null)
            {
                throw new Exception("User dari member ini tidak ditemukan");
            }

            if (user.Status == "Active")
            {
                throw new Exception("Member ini sudah aktif");
            }

            if (user.Status == "Rejected")
            {
                throw new Exception("Member yang sudah ditolak tidak bisa langsung disetujui");
            }

            if (user.Status != "Pending")
            {
                throw new Exception("Member hanya bisa disetujui jika status masih Pending");
            }

            user.Status = "Active";
            user.UpdatedAt = DateTime.Now;

            member.ApprovedBy = approvedBy;
            member.ApprovedAt = DateTime.Now;

            var log = new ActivityLog
            {
                UserId = approvedBy,
                Action = "Approve Member",
                Description = "Pustakawan menyetujui member " + user.FullName,
                CreatedAt = DateTime.Now
            };

            db.ActivityLogs.Add(log);
            db.SaveChanges();

            return new
            {
                message = "Member berhasil disetujui",
                memberId = member.Id,
                userId = user.Id,
                fullName = user.FullName,
                status = user.Status,
                approvedBy = member.ApprovedBy,
                approvedAt = member.ApprovedAt
            };
        }

        public object RejectMember(int id, int rejectedBy)
        {
            if (rejectedBy <= 0)
            {
                throw new Exception("RejectedBy wajib diisi");
            }

            var librarian = db.Users.Find(rejectedBy);

            if (librarian == null)
            {
                throw new Exception("User pustakawan tidak ditemukan");
            }

            if (librarian.Role != "Librarian" || librarian.Status != "Active")
            {
                throw new Exception("Hanya pustakawan aktif yang bisa menolak member");
            }

            var member = db.Members.Find(id);

            if (member == null)
            {
                return null;
            }

            var user = db.Users.Find(member.UserId);

            if (user == null)
            {
                throw new Exception("User dari member ini tidak ditemukan");
            }

            if (user.Status == "Rejected")
            {
                throw new Exception("Member ini sudah ditolak");
            }

            if (user.Status == "Active")
            {
                throw new Exception("Member yang sudah aktif tidak bisa ditolak");
            }

            if (user.Status != "Pending")
            {
                throw new Exception("Member hanya bisa ditolak jika status masih Pending");
            }

            user.Status = "Rejected";
            user.UpdatedAt = DateTime.Now;

            member.ApprovedBy = rejectedBy;
            member.ApprovedAt = DateTime.Now;

            var log = new ActivityLog
            {
                UserId = rejectedBy,
                Action = "Reject Member",
                Description = "Pustakawan menolak member " + user.FullName,
                CreatedAt = DateTime.Now
            };

            db.ActivityLogs.Add(log);
            db.SaveChanges();

            return new
            {
                message = "Member berhasil ditolak",
                memberId = member.Id,
                userId = user.Id,
                fullName = user.FullName,
                status = user.Status,
                rejectedBy = member.ApprovedBy,
                rejectedAt = member.ApprovedAt
            };
        }
    }
}
