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

                TempData["Success"] = "Member berhasil disetujui.";
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

                TempData["Success"] = "Member berhasil ditolak.";
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
            TempData["Error"] = "Member tidak dibuat manual. Silakan gunakan halaman Register.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int? id)
        {
            TempData["Error"] = "Data member tidak diedit dari halaman ini.";
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int? id)
        {
            TempData["Error"] = "Member tidak dapat dihapus dari halaman ini.";
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