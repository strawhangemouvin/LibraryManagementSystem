using LibraryManagementSystem.Models.ViewModel;
using System.Collections.Generic;

namespace LibraryManagementSystem.Services.Interface
{
    public interface IMemberService
    {
        List<MemberViewModel> GetAllMembers();
        List<MemberViewModel> GetPendingMembers();

        object ApproveMember(int id, int approvedBy);
        object RejectMember(int id, int rejectedBy);
    }
}