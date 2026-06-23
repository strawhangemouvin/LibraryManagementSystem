using LibraryManagementSystem.Models.ViewModel;
using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Services.Interface
{
    public interface IReportService
    {
        SummaryReportViewModel GetSummaryReport();
        List<BookReportViewModel> GetBookReport(DateTime? date = null);
        List<BorrowingReportViewModel> GetBorrowingReport(DateTime? date = null);
        List<FineReportViewModel> GetFineReport(DateTime? date = null);
    }
}