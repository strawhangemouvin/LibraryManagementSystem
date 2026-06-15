using LibraryManagementSystem.Models.Entity;
using LibraryManagementSystem.Models.ViewModel;
using System.Collections.Generic;

namespace LibraryManagementSystem.Services.Interface
{
    public interface IBookService
    {
        List<BookViewModel> GetAllBooks();
        BookViewModel GetBookById(int id);
        Book InsertBook(Book book);
        Book UpdateBook(int id, Book book);
        bool DeleteBook(int id);
    }
}