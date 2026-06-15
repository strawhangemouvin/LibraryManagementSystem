using LibraryManagementSystem.Helpers;
using LibraryManagementSystem.Models.Entity;
using LibraryManagementSystem.Services.Impl;
using LibraryManagementSystem.Services.Interface;
using System;
using System.Web.Http;

namespace LibraryManagementSystem.Controllers
{
    [TokenAuth]
    public class BorrowingsController : ApiController
    {
        private readonly IBorrowingService borrowingService;

        public BorrowingsController()
        {
            borrowingService = new BorrowingService();
        }

        [HttpGet]
        [Route("api/borrowings/GetAll")]
        public IHttpActionResult GetAll()
        {
            var borrowings = borrowingService.GetAllBorrowings();
            return Ok(borrowings);
        }

        [HttpGet]
        [Route("api/borrowings/GetByMember/{memberId}")]
        public IHttpActionResult GetByMember(int memberId)
        {
            var borrowings = borrowingService.GetBorrowingsByMember(memberId);
            return Ok(borrowings);
        }

        [HttpPost]
        [Route("api/borrowings/Request")]
        public IHttpActionResult RequestBorrowing([FromBody] Borrowing borrowing)
        {
            try
            {
                var result = borrowingService.RequestBorrowing(borrowing);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("api/borrowings/Approve/{id}")]
        public IHttpActionResult Approve(int id, int approvedBy = 1)
        {
            try
            {
                var result = borrowingService.ApproveBorrowing(id, approvedBy);

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

        [HttpPut]
        [Route("api/borrowings/Reject/{id}")]
        public IHttpActionResult Reject(int id, int rejectedBy = 1)
        {
            try
            {
                var result = borrowingService.RejectBorrowing(id, rejectedBy);

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

        [HttpPut]
        [Route("api/borrowings/Return/{id}")]
        public IHttpActionResult ReturnBook(int id, int returnedBy = 1)
        {
            try
            {
                var result = borrowingService.ReturnBook(id, returnedBy);

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
    }
}
