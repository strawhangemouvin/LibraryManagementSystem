using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LibraryManagementSystem.Models.Entity;
using LibraryManagementSystem.Services.Context;

namespace LibraryManagementSystem.Controllers.MVC
{
    public class MVCBooksController : Controller
    {
        private LibraryDbContext db = new LibraryDbContext();

        // GET: MVCBooks
        public ActionResult Index(string search)
        {
            var books = db.Books.Include(b => b.Category);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var query = search.Trim().ToLower();
                books = books.Where(b => b.Title.ToLower().Contains(query) ||
                                         b.Author.ToLower().Contains(query) ||
                                         b.Publisher.ToLower().Contains(query) ||
                                         (b.Category != null && b.Category.CategoryName.ToLower().Contains(query)));
                ViewBag.SearchQuery = search;
            }
            return View(books.ToList());
        }

        // GET: MVCBooks/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // GET: MVCBooks/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "CategoryName");
            return View();
        }

        // POST: MVCBooks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CategoryId,Title,Author,Publisher,PublishYear,ISBN,Stock,Description")] Book book, HttpPostedFileBase coverImageFile)
        {
            if (ModelState.IsValid)
            {
                // Process Cover Image
                if (coverImageFile != null && coverImageFile.ContentLength > 0)
                {
                    try
                    {
                        var uploadDir = "~/Uploads/Covers/";
                        var absolutePath = Server.MapPath(uploadDir);
                        if (!System.IO.Directory.Exists(absolutePath))
                        {
                            System.IO.Directory.CreateDirectory(absolutePath);
                        }

                        var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(coverImageFile.FileName);
                        var path = System.IO.Path.Combine(absolutePath, fileName);
                        coverImageFile.SaveAs(path);

                        book.CoverImage = "/Uploads/Covers/" + fileName;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("CoverImage", "Failed to upload image: " + ex.Message);
                    }
                }

                if (ModelState.IsValid)
                {
                    book.AvailableStock = book.Stock;
                    book.CreatedAt = DateTime.Now;
                    db.Books.Add(book);
                    db.SaveChanges();
                    TempData["Success"] = "Book successfully added!";
                    return RedirectToAction("Index");
                }
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "CategoryName", book.CategoryId);
            return View(book);
        }

        // GET: MVCBooks/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "CategoryName", book.CategoryId);
            return View(book);
        }

        // POST: MVCBooks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CategoryId,Title,Author,Publisher,PublishYear,ISBN,Stock,Description")] Book book, HttpPostedFileBase coverImageFile)
        {
            if (ModelState.IsValid)
            {
                var originalBook = db.Books.AsNoTracking().FirstOrDefault(b => b.Id == book.Id);
                if (originalBook == null)
                {
                    return HttpNotFound();
                }

                book.CreatedAt = originalBook.CreatedAt;
                book.CoverImage = originalBook.CoverImage; // retain old image if new one not uploaded

                // Process Cover Image Upload
                if (coverImageFile != null && coverImageFile.ContentLength > 0)
                {
                    try
                    {
                        var uploadDir = "~/Uploads/Covers/";
                        var absolutePath = Server.MapPath(uploadDir);
                        if (!System.IO.Directory.Exists(absolutePath))
                        {
                            System.IO.Directory.CreateDirectory(absolutePath);
                        }

                        // Delete old cover image if it exists
                        if (!string.IsNullOrEmpty(originalBook.CoverImage))
                        {
                            var oldFilePath = Server.MapPath(originalBook.CoverImage);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(coverImageFile.FileName);
                        var path = System.IO.Path.Combine(absolutePath, fileName);
                        coverImageFile.SaveAs(path);

                        book.CoverImage = "/Uploads/Covers/" + fileName;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("CoverImage", "Failed to upload image: " + ex.Message);
                    }
                }

                if (ModelState.IsValid)
                {
                    int stockDiff = book.Stock - originalBook.Stock;
                    book.AvailableStock = originalBook.AvailableStock + stockDiff;
                    
                    if (book.AvailableStock < 0)
                    {
                        book.AvailableStock = 0;
                    }
                    else if (book.AvailableStock > book.Stock)
                    {
                        book.AvailableStock = book.Stock;
                    }

                    book.UpdatedAt = DateTime.Now;
                    db.Entry(book).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Book successfully updated!";
                    return RedirectToAction("Index");
                }
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "CategoryName", book.CategoryId);
            return View(book);
        }

        // GET: MVCBooks/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // POST: MVCBooks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Book book = db.Books.Find(id);
            db.Books.Remove(book);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
