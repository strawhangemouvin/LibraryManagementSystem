using LibraryManagementSystem.Services.Impl;
using LibraryManagementSystem.Services.Interface;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace LibraryManagementSystem.Controllers.MVC
{
    public class MVCMembersController : Controller
    {
        private readonly IMemberService memberService;

        public MVCMembersController()
        {
            memberService = new MemberService();
        }

        public ActionResult Index()
        {
            var members = memberService.GetAllMembers();
            return View(members);
        }

        public ActionResult Pending()
        {
            var pendingMembers = memberService.GetPendingMembers();
            return View(pendingMembers);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var member = memberService.GetAllMembers()
                .FirstOrDefault(x => x.Id == id.Value);

            if (member == null)
            {
                return HttpNotFound();
            }

            return View(member);
        }

        public ActionResult Approve(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                int approvedBy = GetCurrentLibrarianId();

                var result = memberService.ApproveMember(id.Value, approvedBy);

                if (result == null)
                {
                    return HttpNotFound();
                }

                TempData["Success"] = "Member successfully approved.";
                return RedirectToAction("Pending");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Pending");
            }
        }

        public ActionResult Reject(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                int rejectedBy = GetCurrentLibrarianId();

                var result = memberService.RejectMember(id.Value, rejectedBy);

                if (result == null)
                {
                    return HttpNotFound();
                }

                TempData["Success"] = "Member successfully rejected.";
                return RedirectToAction("Pending");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Pending");
            }
        }

        public ActionResult Create()
        {
            TempData["Error"] = "Members cannot be created manually. Please use the Registration page.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int? id)
        {
            TempData["Error"] = "Member details cannot be edited from this page.";
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int? id)
        {
            TempData["Error"] = "Members cannot be deleted from this page.";
            return RedirectToAction("Index");
        }

        private int GetCurrentLibrarianId()
        {
            if (Session["UserId"] != null)
            {
                return Convert.ToInt32(Session["UserId"]);
            }

            return 1;
        }
    }
}