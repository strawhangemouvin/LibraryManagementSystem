using LibraryManagementSystem.Models.Entity;
using LibraryManagementSystem.Models.ViewModel;
using LibraryManagementSystem.Services.Context;
using LibraryManagementSystem.Services.Interface;
using LibraryManagementSystem.Helpers;
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
                    (member, user) => new
                    {
                        member,
                        user
                    }
                )
                .Select(data => new MemberViewModel
                {
                    Id = data.member.Id,
                    UserId = data.member.UserId,

                    FullName = data.user.FullName,
                    Username = data.user.Username,
                    Email = data.user.Email,
                    Role = data.user.Role,
                    Status = data.user.Status,

                    MemberCode = data.member.MemberCode,
                    Address = data.member.Address,
                    ClassName = data.member.ClassName,

                    ApprovedBy = data.member.ApprovedBy,
                    ApprovedByName = db.Users
                        .Where(u =>
                            u.Id == data.member.ApprovedBy &&
                            u.Role == "Librarian"
                        )
                        .Select(u => u.FullName)
                        .FirstOrDefault(),

                    ApprovedAt = data.member.ApprovedAt
                })
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
                    (member, user) => new
                    {
                        member,
                        user
                    }
                )
                .Where(data => data.user.Status == "Pending")
                .Select(data => new MemberViewModel
                {
                    Id = data.member.Id,
                    UserId = data.member.UserId,

                    FullName = data.user.FullName,
                    Username = data.user.Username,
                    Email = data.user.Email,
                    Role = data.user.Role,
                    Status = data.user.Status,

                    MemberCode = data.member.MemberCode,
                    Address = data.member.Address,
                    ClassName = data.member.ClassName,

                    ApprovedBy = data.member.ApprovedBy,
                    ApprovedByName = db.Users
                        .Where(u =>
                            u.Id == data.member.ApprovedBy &&
                            u.Role == "Librarian"
                        )
                        .Select(u => u.FullName)
                        .FirstOrDefault(),

                    ApprovedAt = data.member.ApprovedAt
                })
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

            var emailStatus = SendApprovedEmail(user);

            return new
            {
                message = "Member berhasil disetujui",
                memberId = member.Id,
                userId = user.Id,
                fullName = user.FullName,
                email = user.Email,
                status = user.Status,
                approvedBy = member.ApprovedBy,
                approvedByName = librarian.FullName,
                approvedAt = member.ApprovedAt,
                emailStatus = emailStatus
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
                email = user.Email,
                status = user.Status,
                rejectedBy = member.ApprovedBy,
                rejectedByName = librarian.FullName,
                rejectedAt = member.ApprovedAt
            };
        }
        private string SendApprovedEmail(User user)
        {
            try
            {
                EmailHelper.SendEmail(
                    user.Email,
                    "Akun Anda Telah Disetujui",
                    $@"
            <h3>Akun Member Disetujui</h3>
            <p>Halo <b>{user.FullName}</b>,</p>
            <p>Akun Anda pada <b>Library Management System</b> telah disetujui oleh pustakawan.</p>
            <p>Status akun Anda sekarang: <b>Active</b>.</p>
            <p>Anda sudah dapat login menggunakan username dan password yang telah dibuat saat registrasi.</p>
            <br/>
            <p>Terima kasih.</p>
            "
                );

                return "Email pemberitahuan approval berhasil dikirim.";
            }
            catch (Exception ex)
            {
                return "Member berhasil di-approve, tetapi email gagal dikirim: " + ex.Message;
            }
        }
    }
}