using LibraryManagementSystem.Services.Impl;
using LibraryManagementSystem.Services.Interface;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace LibraryManagementSystem.Controllers.MVC
{
    public class ReportsMvcController : Controller
    {
        private readonly IReportService reportService;

        public ReportsMvcController()
        {
            reportService = new ReportService();
        }

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Librarian")
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            var summary = reportService.GetSummaryReport();
            return View(summary);
        }

        public ActionResult Books(string date)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Librarian")
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            DateTime? parsedDate = null;
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime temp))
            {
                parsedDate = temp;
            }

            var books = reportService.GetBookReport(parsedDate);
            ViewBag.SelectedDate = date;
            return View(books);
        }

        public ActionResult Borrowings(string date)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Librarian")
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            DateTime? parsedDate = null;
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime temp))
            {
                parsedDate = temp;
            }

            var borrowings = reportService.GetBorrowingReport(parsedDate);
            ViewBag.SelectedDate = date;
            return View(borrowings);
        }

        public ActionResult Fines(string date)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Librarian")
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            DateTime? parsedDate = null;
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime temp))
            {
                parsedDate = temp;
            }

            var fines = reportService.GetFineReport(parsedDate);
            ViewBag.SelectedDate = date;
            return View(fines);
        }

        [HttpGet]
        public ActionResult ExportBooksExcel(string date)
        {
            if (Session["UserId"] == null || Session["Role"]?.ToString() != "Librarian")
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            DateTime? parsedDate = null;
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime temp))
            {
                parsedDate = temp;
            }

            var books = reportService.GetBookReport(parsedDate);
            string dateStr = parsedDate.HasValue ? parsedDate.Value.ToString("dd-MM-yyyy") : "Semua_Waktu";

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Laporan Buku");

                // Title Block
                worksheet.Cells["A1"].Value = "LAPORAN HARIAN DAFTAR BUKU";
                worksheet.Cells["A1"].Style.Font.Size = 16;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(58, 31, 36));
                
                worksheet.Cells["A2"].Value = "Per Tanggal: " + (parsedDate.HasValue ? parsedDate.Value.ToString("dd MMMM yyyy") : "Semua Waktu");
                worksheet.Cells["A2"].Style.Font.Size = 11;
                worksheet.Cells["A2"].Style.Font.Italic = true;

                worksheet.Cells["A3"].Value = "Dibuat Oleh: Pustakawan (Moon Books)";
                worksheet.Cells["A3"].Style.Font.Size = 10;
                worksheet.Cells["A3"].Style.Font.Color.SetColor(System.Drawing.Color.Gray);

                // Table Headers
                string[] headers = { "No", "ID Buku", "Judul Buku", "Penulis", "Kategori", "Penerbit", "Tahun Terbit", "Total Stok", "Stok Tersedia", "Tanggal Terdaftar" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cells[5, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(201, 122, 126));
                    cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Data Rows
                int row = 6;
                int no = 1;
                foreach (var book in books)
                {
                    worksheet.Cells[row, 1].Value = no++;
                    worksheet.Cells[row, 2].Value = book.Id;
                    worksheet.Cells[row, 3].Value = book.Title;
                    worksheet.Cells[row, 4].Value = book.Author;
                    worksheet.Cells[row, 5].Value = book.CategoryName ?? "-";
                    worksheet.Cells[row, 6].Value = book.Publisher ?? "-";
                    worksheet.Cells[row, 7].Value = book.PublishYear;
                    worksheet.Cells[row, 8].Value = book.Stock;
                    worksheet.Cells[row, 9].Value = book.AvailableStock;
                    worksheet.Cells[row, 10].Value = book.CreatedAt.ToString("dd-MM-yyyy HH:mm");

                    // Apply styles to data rows
                    for (int i = 1; i <= headers.Length; i++)
                    {
                        var cell = worksheet.Cells[row, i];
                        cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        if (i == 1 || i == 2 || i == 7 || i == 8 || i == 9 || i == 10)
                        {
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }
                    }
                    row++;
                }

                if (books.Any())
                {
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                }

                var fileBytes = package.GetAsByteArray();
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Laporan_Buku_{dateStr}.xlsx");
            }
        }

        [HttpGet]
        public ActionResult ExportBorrowingsExcel(string date)
        {
            if (Session["UserId"] == null || Session["Role"]?.ToString() != "Librarian")
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            DateTime? parsedDate = null;
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime temp))
            {
                parsedDate = temp;
            }

            var borrowings = reportService.GetBorrowingReport(parsedDate);
            string dateStr = parsedDate.HasValue ? parsedDate.Value.ToString("dd-MM-yyyy") : "Semua_Waktu";

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Laporan Peminjaman");

                // Title Block
                worksheet.Cells["A1"].Value = "LAPORAN HARIAN TRANSAKSI PEMINJAMAN";
                worksheet.Cells["A1"].Style.Font.Size = 16;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(58, 31, 36));

                worksheet.Cells["A2"].Value = "Per Tanggal: " + (parsedDate.HasValue ? parsedDate.Value.ToString("dd MMMM yyyy") : "Semua Waktu");
                worksheet.Cells["A2"].Style.Font.Size = 11;
                worksheet.Cells["A2"].Style.Font.Italic = true;

                worksheet.Cells["A3"].Value = "Dibuat Oleh: Pustakawan (Moon Books)";
                worksheet.Cells["A3"].Style.Font.Size = 10;
                worksheet.Cells["A3"].Style.Font.Color.SetColor(System.Drawing.Color.Gray);

                // Table Headers
                string[] headers = { "No", "ID Pinjam", "Nama Anggota", "Judul Buku", "Tgl Pengajuan", "Tgl Pinjam", "Tgl Jatuh Tempo", "Tgl Kembali", "Status", "Catatan" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cells[5, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(201, 122, 126));
                    cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Data Rows
                int row = 6;
                int no = 1;
                foreach (var item in borrowings)
                {
                    worksheet.Cells[row, 1].Value = no++;
                    worksheet.Cells[row, 2].Value = item.BorrowingId;
                    worksheet.Cells[row, 3].Value = item.MemberName;
                    worksheet.Cells[row, 4].Value = item.BookTitle;
                    worksheet.Cells[row, 5].Value = item.RequestDate.ToString("dd-MM-yyyy");
                    worksheet.Cells[row, 6].Value = item.BorrowDate?.ToString("dd-MM-yyyy") ?? "-";
                    worksheet.Cells[row, 7].Value = item.DueDate?.ToString("dd-MM-yyyy") ?? "-";
                    worksheet.Cells[row, 8].Value = item.ReturnDate?.ToString("dd-MM-yyyy") ?? "-";
                    worksheet.Cells[row, 9].Value = item.Status;
                    worksheet.Cells[row, 10].Value = string.IsNullOrWhiteSpace(item.Notes) ? "-" : item.Notes;

                    // Apply styles to data rows
                    for (int i = 1; i <= headers.Length; i++)
                    {
                        var cell = worksheet.Cells[row, i];
                        cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        if (i == 1 || i == 2 || (i >= 5 && i <= 9))
                        {
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }
                    }
                    row++;
                }

                if (borrowings.Any())
                {
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                }

                var fileBytes = package.GetAsByteArray();
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Laporan_Peminjaman_{dateStr}.xlsx");
            }
        }

        [HttpGet]
        public ActionResult ExportFinesExcel(string date)
        {
            if (Session["UserId"] == null || Session["Role"]?.ToString() != "Librarian")
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            DateTime? parsedDate = null;
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime temp))
            {
                parsedDate = temp;
            }

            var fines = reportService.GetFineReport(parsedDate);
            string dateStr = parsedDate.HasValue ? parsedDate.Value.ToString("dd-MM-yyyy") : "Semua_Waktu";

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Laporan Denda");

                // Title Block
                worksheet.Cells["A1"].Value = "LAPORAN HARIAN DENDA PERPUSTAKAAN";
                worksheet.Cells["A1"].Style.Font.Size = 16;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(58, 31, 36));

                worksheet.Cells["A2"].Value = "Per Tanggal: " + (parsedDate.HasValue ? parsedDate.Value.ToString("dd MMMM yyyy") : "Semua Waktu");
                worksheet.Cells["A2"].Style.Font.Size = 11;
                worksheet.Cells["A2"].Style.Font.Italic = true;

                worksheet.Cells["A3"].Value = "Dibuat Oleh: Pustakawan (Moon Books)";
                worksheet.Cells["A3"].Style.Font.Size = 10;
                worksheet.Cells["A3"].Style.Font.Color.SetColor(System.Drawing.Color.Gray);

                // Table Headers
                string[] headers = { "No", "ID Pinjam", "Nama Anggota", "Username", "Judul Buku", "Tgl Pinjam", "Tgl Jatuh Tempo", "Tgl Kembali", "Keterlambatan (Hari)", "Jumlah Denda", "Status Denda" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cells[5, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(201, 122, 126));
                    cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Data Rows
                int row = 6;
                int no = 1;
                decimal totalFine = 0;

                foreach (var item in fines)
                {
                    worksheet.Cells[row, 1].Value = no++;
                    worksheet.Cells[row, 2].Value = item.BorrowingId;
                    worksheet.Cells[row, 3].Value = item.MemberName;
                    worksheet.Cells[row, 4].Value = item.Username;
                    worksheet.Cells[row, 5].Value = item.BookTitle;
                    worksheet.Cells[row, 6].Value = item.BorrowDate?.ToString("dd-MM-yyyy") ?? "-";
                    worksheet.Cells[row, 7].Value = item.DueDate?.ToString("dd-MM-yyyy") ?? "-";
                    worksheet.Cells[row, 8].Value = item.ReturnDate?.ToString("dd-MM-yyyy") ?? "-";
                    worksheet.Cells[row, 9].Value = item.LateDays ?? 0;
                    worksheet.Cells[row, 10].Value = item.FineAmount ?? 0;
                    worksheet.Cells[row, 10].Style.Numberformat.Format = "#,##0";
                    worksheet.Cells[row, 11].Value = item.FineStatus;

                    totalFine += item.FineAmount ?? 0;

                    // Apply styles to data rows
                    for (int i = 1; i <= headers.Length; i++)
                    {
                        var cell = worksheet.Cells[row, i];
                        cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        if (i == 1 || i == 2 || (i >= 6 && i <= 9) || i == 11)
                        {
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }
                    }
                    row++;
                }

                // Summary Row
                if (fines.Any())
                {
                    worksheet.Cells[row, 1, row, 9].Merge = true;
                    worksheet.Cells[row, 1].Value = "TOTAL DENDA";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[row, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    worksheet.Cells[row, 10].Value = totalFine;
                    worksheet.Cells[row, 10].Style.Font.Bold = true;
                    worksheet.Cells[row, 10].Style.Numberformat.Format = "#,##0";
                    worksheet.Cells[row, 10].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    worksheet.Cells[row, 11].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    row++;
                }

                if (fines.Any())
                {
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                }

                var fileBytes = package.GetAsByteArray();
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Laporan_Denda_{dateStr}.xlsx");
            }
        }

        public ActionResult PrintBooks(string date)
        {
            if (Session["UserId"] == null || Session["Role"]?.ToString() != "Librarian")
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            DateTime? parsedDate = null;
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime temp))
            {
                parsedDate = temp;
            }

            var books = reportService.GetBookReport(parsedDate);
            ViewBag.SelectedDate = parsedDate;
            return View(books);
        }

        public ActionResult PrintBorrowings(string date)
        {
            if (Session["UserId"] == null || Session["Role"]?.ToString() != "Librarian")
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            DateTime? parsedDate = null;
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime temp))
            {
                parsedDate = temp;
            }

            var borrowings = reportService.GetBorrowingReport(parsedDate);
            ViewBag.SelectedDate = parsedDate;
            return View(borrowings);
        }

        public ActionResult PrintFines(string date)
        {
            if (Session["UserId"] == null || Session["Role"]?.ToString() != "Librarian")
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            DateTime? parsedDate = null;
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime temp))
            {
                parsedDate = temp;
            }

            var fines = reportService.GetFineReport(parsedDate);
            ViewBag.SelectedDate = parsedDate;
            return View(fines);
        }
    }
}