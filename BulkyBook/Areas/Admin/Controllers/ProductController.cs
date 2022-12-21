using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModel;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = SD.Role_Admin)]
	public class ProductController : Controller
    {
        private readonly IUnitOfWork _db;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ProductController(IUnitOfWork db,IWebHostEnvironment hostEnvironment)
        {
            _db = db;
            _hostEnvironment = hostEnvironment;
        }
       
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                product = new(),
                CategoryList = _db.Category.GetAll().Select(i =>
                new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                CoverTypeList = _db.CoverType.GetAll().Select(i=> 
                new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };
            /*Product product = new();
            IEnumerable<SelectListItem> CategoryList = _db.Category.GetAll().Select(
                u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
            IEnumerable<SelectListItem> CoverList = _db.CoverType.GetAll().Select(
                u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });*/
            //create
            if (id == null || id == 0)
            {
                /*ViewBag.CategoryList = CategoryList;
                ViewBag.CoverList = CoverList;*/
                return View(productVM);
            }
            else
            {
                productVM.product = _db.Product.GetFirstOrDefault(u => u.Id == id);
                return View(productVM);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string rootPath = _hostEnvironment.WebRootPath;
                if (file!=null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    string uploads = Path.Combine(rootPath, @"images\products");
                    string extension = Path.GetExtension(file.FileName);
                    if(obj.product.ImageUrl!=null)
                    {
                        var imagePath = Path.Combine(rootPath, obj.product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }
                    obj.product.ImageUrl = @"\images\products\" + fileName + extension;
                }
                if(obj.product.Id==0)
                {
                    _db.Product.Add(obj.product);
                }
                else
                {
                    _db.Product.Update(obj.product);
                }
                _db.Save();
                TempData["success"] = "Product Created";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _db.Product.GetAll(includeProperties:"Category,CoverType");
            return Json(new {  data = productList });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _db.Product.GetFirstOrDefault(u => u.Id == id);
            if(obj==null)
            {
                return Json(new { success = false, message = "Could not delete!" });
            }
            var imagePath = Path.Combine(_hostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
            _db.Product.Remove(obj);
            _db.Save();
            return Json(new { success = true, message = "Deleted Successfully" });

        }
        #endregion
    }
}
