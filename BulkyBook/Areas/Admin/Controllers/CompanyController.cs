using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Magnum.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = SD.Role_Admin)]
	public class CompanyController : Controller
    {
        private readonly IUnitOfWork _db;
        public CompanyController(IUnitOfWork db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            if(id==0 || id==null)
            {
                Company obj = new Company();
                return View(obj);
            }
            else
            {
                var obj = _db.Company.GetFirstOrDefault(u => u.Id == id);
                return View(obj);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company obj)
        {
            if(ModelState.IsValid)
            {
                if(obj.Id==0)
                {
                    _db.Company.Add(obj);
                }
                else
                {
                    _db.Company.Update(obj);
                }
                _db.Save();
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var companyModel = _db.Company.GetAll();
            return Json(new {data = companyModel});
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _db.Company.GetFirstOrDefault(u => u.Id == id);
            if(obj!=null)
            {
                _db.Company.Remove(obj);
            }
            if (obj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _db.Save();
            return Json(new { success = true, message = "Deleted successfully" });
        }
        #endregion
    }
}
