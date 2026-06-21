using LibraryManagementSystem.Models.Entity;
using LibraryManagementSystem.Models.ViewModel;
using LibraryManagementSystem.Services.Context;
using LibraryManagementSystem.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryManagementSystem.Services.Impl
{
    public class BookService : IBookService
    {
        private LibraryDbContext db = new LibraryDbContext();

        public List<BookViewModel> GetAllBooks()
        {
            var books = db.Books
                .Join(
                    db.Categories,
                    book => book.CategoryId,
                    category => category.Id,
                    (book, category) => new BookViewModel
                    {
                        Id = book.Id,
                        CategoryId = book.CategoryId,
                        CategoryName = category.CategoryName,
                        Title = book.Title,
                        Author = book.Author,
                        Publisher = book.Publisher,
                        PublishYear = book.PublishYear,
                        ISBN = book.ISBN,
                        Stock = book.Stock,
                        AvailableStock = book.AvailableStock,
                        Description = book.Description,
                        CoverImage = book.CoverImage,
                        CreatedAt = book.CreatedAt,
                        UpdatedAt = book.UpdatedAt
                    }
                )
                .OrderBy(x => x.Title)
                .ToList();

            return books;
        }

        public BookViewModel GetBookById(int id)
        {
            var book = db.Books
                .Where(x => x.Id == id)
                .Join(
                    db.Categories,
                    bookData => bookData.CategoryId,
                    category => category.Id,
                    (bookData, category) => new BookViewModel
                    {
                        Id = bookData.Id,
                        CategoryId = bookData.CategoryId,
                        CategoryName = category.CategoryName,
                        Title = bookData.Title,
                        Author = bookData.Author,
                        Publisher = bookData.Publisher,
                        PublishYear = bookData.PublishYear,
                        ISBN = bookData.ISBN,
                        Stock = bookData.Stock,
                        AvailableStock = bookData.AvailableStock,
                        Description = bookData.Description,
                        CoverImage = bookData.CoverImage,
                        CreatedAt = bookData.CreatedAt,
                        UpdatedAt = bookData.UpdatedAt
                    }
                )
                .FirstOrDefault();

            return book;
        }

        public Book InsertBook(Book book)
        {
            if (book == null)
            {
                throw new Exception("Book data cannot be empty");
            }

            if (book.CategoryId <= 0)
            {
                throw new Exception("CategoryId is required");
            }

            var categoryExists = db.Categories.Any(x => x.Id == book.CategoryId);

            if (!categoryExists)
            {
                throw new Exception("Category not found");
            }

            if (string.IsNullOrWhiteSpace(book.Title))
            {
                throw new Exception("Book title is required");
            }

            if (string.IsNullOrWhiteSpace(book.Author))
            {
                throw new Exception("Author is required");
            }

            if (book.Stock < 0)
            {
                throw new Exception("Stock cannot be negative");
            }

            if (book.AvailableStock < 0)
            {
                throw new Exception("Available stock cannot be negative");
            }

            if (book.AvailableStock > book.Stock)
            {
                throw new Exception("Available stock cannot exceed total stock");
            }

            var isbnExists = false;

            if (!string.IsNullOrWhiteSpace(book.ISBN))
            {
                var isbn = book.ISBN.Trim();

                isbnExists = db.Books.Any(x => x.ISBN == isbn);

                if (isbnExists)
                {
                    throw new Exception("ISBN is already used by another book");
                }

                book.ISBN = isbn;
            }

            book.Title = book.Title.Trim();
            book.Author = book.Author.Trim();

            if (!string.IsNullOrWhiteSpace(book.Publisher))
            {
                book.Publisher = book.Publisher.Trim();
            }

            if (!string.IsNullOrWhiteSpace(book.Description))
            {
                book.Description = book.Description.Trim();
            }

            book.CreatedAt = DateTime.Now;
            book.UpdatedAt = null;

            db.Books.Add(book);
            db.SaveChanges();

            return book;
        }

        public Book UpdateBook(int id, Book book)
        {
            if (book == null)
            {
                throw new Exception("Book data cannot be empty");
            }

            var existingBook = db.Books.Find(id);

            if (existingBook == null)
            {
                return null;
            }

            if (book.CategoryId <= 0)
            {
                throw new Exception("CategoryId is required");
            }

            var categoryExists = db.Categories.Any(x => x.Id == book.CategoryId);

            if (!categoryExists)
            {
                throw new Exception("Category not found");
            }

            if (string.IsNullOrWhiteSpace(book.Title))
            {
                throw new Exception("Book title is required");
            }

            if (string.IsNullOrWhiteSpace(book.Author))
            {
                throw new Exception("Author is required");
            }

            if (book.Stock < 0)
            {
                throw new Exception("Stock cannot be negative");
            }

            if (book.AvailableStock < 0)
            {
                throw new Exception("Available stock cannot be negative");
            }

            if (book.AvailableStock > book.Stock)
            {
                throw new Exception("Available stock cannot exceed total stock");
            }

            if (!string.IsNullOrWhiteSpace(book.ISBN))
            {
                var isbn = book.ISBN.Trim();

                var isbnExists = db.Books.Any(x =>
                    x.ISBN == isbn &&
                    x.Id != id
                );

                if (isbnExists)
                {
                    throw new Exception("ISBN is already used by another book");
                }

                existingBook.ISBN = isbn;
            }
            else
            {
                existingBook.ISBN = null;
            }

            existingBook.CategoryId = book.CategoryId;
            existingBook.Title = book.Title.Trim();
            existingBook.Author = book.Author.Trim();

            if (!string.IsNullOrWhiteSpace(book.Publisher))
            {
                existingBook.Publisher = book.Publisher.Trim();
            }
            else
            {
                existingBook.Publisher = null;
            }

            existingBook.PublishYear = book.PublishYear;
            existingBook.Stock = book.Stock;
            existingBook.AvailableStock = book.AvailableStock;

            if (!string.IsNullOrWhiteSpace(book.Description))
            {
                existingBook.Description = book.Description.Trim();
            }
            else
            {
                existingBook.Description = null;
            }

            existingBook.CoverImage = book.CoverImage;
            existingBook.UpdatedAt = DateTime.Now;

            db.SaveChanges();

            return existingBook;
        }

        public bool DeleteBook(int id)
        {
            var book = db.Books.Find(id);

            if (book == null)
            {
                return false;
            }

            var isBorrowed = db.Borrowings.Any(x =>
                x.BookId == id &&
                x.Status == "Borrowed"
            );

            if (isBorrowed)
            {
                throw new Exception("Book cannot be deleted because it is currently borrowed");
            }

            var hasRequestedBorrowing = db.Borrowings.Any(x =>
                x.BookId == id &&
                x.Status == "Requested"
            );

            if (hasRequestedBorrowing)
            {
                throw new Exception("Book cannot be deleted because there are pending borrowing requests");
            }

            db.Books.Remove(book);
            db.SaveChanges();

            return true;
        }
    }
}