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
                throw new Exception("ApprovedBy is required");
            }

            var librarian = db.Users.Find(approvedBy);

            if (librarian == null)
            {
                throw new Exception("Librarian user not found");
            }

            if (librarian.Role != "Librarian" || librarian.Status != "Active")
            {
                throw new Exception("Only active librarians can approve members");
            }

            var member = db.Members.Find(id);

            if (member == null)
            {
                return null;
            }

            var user = db.Users.Find(member.UserId);

            if (user == null)
            {
                throw new Exception("User for this member not found");
            }

            if (user.Status == "Active")
            {
                throw new Exception("This member is already active");
            }

            if (user.Status == "Rejected")
            {
                throw new Exception("Rejected members cannot be directly approved");
            }

            if (user.Status != "Pending")
            {
                throw new Exception("Member can only be approved if status is Pending");
            }

            user.Status = "Active";
            user.UpdatedAt = DateTime.Now;

            member.ApprovedBy = approvedBy;
            member.ApprovedAt = DateTime.Now;

            var log = new ActivityLog
            {
                UserId = approvedBy,
                Action = "Approve Member",
                Description = "Librarian approved member " + user.FullName,
                CreatedAt = DateTime.Now
            };

            db.ActivityLogs.Add(log);
            db.SaveChanges();

            var emailStatus = SendApprovedEmail(user);

            return new
            {
                message = "Member successfully approved",
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
                throw new Exception("RejectedBy is required");
            }

            var librarian = db.Users.Find(rejectedBy);

            if (librarian == null)
            {
                throw new Exception("Librarian user not found");
            }

            if (librarian.Role != "Librarian" || librarian.Status != "Active")
            {
                throw new Exception("Only active librarians can reject members");
            }

            var member = db.Members.Find(id);

            if (member == null)
            {
                return null;
            }

            var user = db.Users.Find(member.UserId);

            if (user == null)
            {
                throw new Exception("User for this member not found");
            }

            if (user.Status == "Rejected")
            {
                throw new Exception("This member is already rejected");
            }

            if (user.Status == "Active")
            {
                throw new Exception("Active members cannot be rejected");
            }

            if (user.Status != "Pending")
            {
                throw new Exception("Member can only be rejected if status is Pending");
            }

            user.Status = "Rejected";
            user.UpdatedAt = DateTime.Now;

            member.ApprovedBy = rejectedBy;
            member.ApprovedAt = DateTime.Now;

            var log = new ActivityLog
            {
                UserId = rejectedBy,
                Action = "Reject Member",
                Description = "Librarian rejected member " + user.FullName,
                CreatedAt = DateTime.Now
            };

            db.ActivityLogs.Add(log);
            db.SaveChanges();

            return new
            {
                message = "Member successfully rejected",
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
                    "Your Account Has Been Approved",
                    $@"
            <h3>Member Account Approved</h3>
            <p>Hello <b>{user.FullName}</b>,</p>
            <p>Your account on <b>Library Management System</b> has been approved by the librarian.</p>
            <p>Your account status is now: <b>Active</b>.</p>
            <p>You can now log in using the username and password created during registration.</p>
            <br/>
            <p>Thank you.</p>
            "
                );

                return "Approval notification email sent successfully.";
            }
            catch (Exception ex)
            {
                return "Member successfully approved, but email failed to send: " + ex.Message;
            }
        }

        public object ToggleMemberStatus(int id, int updatedBy)
        {
            if (updatedBy <= 0)
            {
                throw new Exception("Librarian ID is required.");
            }

            var librarian = db.Users.Find(updatedBy);
            if (librarian == null)
            {
                throw new Exception("Librarian user not found.");
            }

            if (librarian.Role != "Librarian" || librarian.Status != "Active")
            {
                throw new Exception("Only active librarians can update member status.");
            }

            var member = db.Members.Find(id);
            if (member == null)
            {
                return null;
            }

            var user = db.Users.Find(member.UserId);
            if (user == null)
            {
                throw new Exception("User for this member not found.");
            }

            if (user.Status != "Active" && user.Status != "Inactive")
            {
                throw new Exception("Only active or inactive members can be toggled.");
            }

            string oldStatus = user.Status;
            string newStatus = oldStatus == "Active" ? "Inactive" : "Active";

            user.Status = newStatus;
            user.UpdatedAt = DateTime.Now;

            var log = new ActivityLog
            {
                UserId = updatedBy,
                Action = newStatus == "Active" ? "Activate Member" : "Deactivate Member",
                Description = $"Librarian toggled member {user.FullName} from {oldStatus} to {newStatus}",
                CreatedAt = DateTime.Now
            };

            db.ActivityLogs.Add(log);
            db.SaveChanges();

            return new
            {
                success = true,
                message = $"Member successfully {(newStatus == "Active" ? "activated" : "deactivated")}.",
                memberId = member.Id,
                status = user.Status
            };
        }
    }
}