using LibraryManagementSystem.Helpers;
using LibraryManagementSystem.Models.Entity;
using LibraryManagementSystem.Services.Impl;
using LibraryManagementSystem.Services.Interface;
using System;
using System.Web.Http;

namespace LibraryManagementSystem.Controllers
{
    [TokenAuth]
    public class BooksController : ApiController
    {
        private readonly IBookService bookService;

        public BooksController()
        {
            bookService = new BookService();
        }

        [HttpGet]
        [Route("api/books/GetAll")]
        public IHttpActionResult GetAll()
        {
            var books = bookService.GetAllBooks();
            return Ok(books);
        }

        [HttpGet]
        [Route("api/books/GetById/{id}")]
        public IHttpActionResult GetById(int id)
        {
            var book = bookService.GetBookById(id);

            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }

        [HttpPost]
        [Route("api/books/Insert")]
        public IHttpActionResult Insert([FromBody] Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = bookService.InsertBook(book);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("api/books/Update/{id}")]
        public IHttpActionResult Update(int id, [FromBody] Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = bookService.UpdateBook(id, book);

                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("api/books/Delete/{id}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var result = bookService.DeleteBook(id);

                if (!result)
                {
                    return NotFound();
                }

                return Ok("Book deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}