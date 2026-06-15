using LibraryManagementSystem.Helpers;
using LibraryManagementSystem.Services.Impl;
using LibraryManagementSystem.Services.Interface;
using System;
using System.Web.Http;

namespace LibraryManagementSystem.Controllers
{
    [TokenAuth]
    public class MembersController : ApiController
    {
        private readonly IMemberService memberService;

        public MembersController()
        {
            memberService = new MemberService();
        }

        [HttpGet]
        [Route("api/members/GetAll")]
        public IHttpActionResult GetAll()
        {
            var members = memberService.GetAllMembers();
            return Ok(members);
        }

        [HttpGet]
        [Route("api/members/GetPending")]
        public IHttpActionResult GetPending()
        {
            var pendingMembers = memberService.GetPendingMembers();
            return Ok(pendingMembers);
        }

        [HttpPut]
        [Route("api/members/Approve/{id}")]
        public IHttpActionResult Approve(int id, int approvedBy = 1)
        {
            try
            {
                var result = memberService.ApproveMember(id, approvedBy);

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
        [Route("api/members/Reject/{id}")]
        public IHttpActionResult Reject(int id, int rejectedBy = 1)
        {
            try
            {
                var result = memberService.RejectMember(id, rejectedBy);

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
