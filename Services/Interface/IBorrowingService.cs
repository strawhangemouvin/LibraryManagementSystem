using LibraryManagementSystem.Models.Entity;
using LibraryManagementSystem.Models.ViewModel;
using System.Collections.Generic;

namespace LibraryManagementSystem.Services.Interface
{
    public interface IBorrowingService
    {
        List<BorrowingViewModel> GetAllBorrowings();
        List<BorrowingViewModel> GetBorrowingsByMember(int memberId);

        object RequestBorrowing(Borrowing borrowing);
        object ApproveBorrowing(int id, int approvedBy);
        object RejectBorrowing(int id, int rejectedBy);
        object ReturnBook(int id, int returnedBy);
    }
}
