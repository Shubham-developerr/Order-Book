using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Product obj)
        {
            Product product = _db.Products.FirstOrDefault(u => u.Id == obj.Id);
            /*_db.Products.Update(obj);*/
            if(product!=null)
            {
                product.Title = obj.Title;
                product.ISBN = obj.ISBN;
                product.ListPrice = obj.ListPrice;
                product.Price = obj.Price;
                product.Price100 = obj.Price100;
                product.Price50 = obj.Price50;
                product.Author = obj.Author;
                product.Description = obj.Description;
                if(obj.ImageUrl!=null)
                {
                    product.ImageUrl = obj.ImageUrl;  
                }
            }

        }
    }
}
