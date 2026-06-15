using LibraryManagementSystem.Models.ViewModel;
using System.Collections.Generic;

namespace LibraryManagementSystem.Services.Interface
{
    public interface IReportService
    {
        SummaryReportViewModel GetSummaryReport();
        List<BookReportViewModel> GetBookReport();
        List<BorrowingReportViewModel> GetBorrowingReport();
    }
}