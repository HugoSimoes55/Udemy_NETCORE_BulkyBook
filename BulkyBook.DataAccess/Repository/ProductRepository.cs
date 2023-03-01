using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository;

public class ProductRepository : Repository<Product>, IProductRepository
{
    private readonly ApplicationDbContext _db;

    public ProductRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public void Update(Product obj)
    {
        var objFromDb = _db.Products.FirstOrDefault(l => l.Id == obj.Id);

        if (objFromDb != null)
        {
            objFromDb.Title = obj.Title;
            objFromDb.Description = obj.Description;
            objFromDb.Author = obj.Author;
            objFromDb.ISBN = obj.ISBN;
            objFromDb.ListPrice = obj.ListPrice;
            objFromDb.Price = obj.Price;
            objFromDb.Price50 = obj.Price50;
            objFromDb.Price100 = obj.Price100;
            objFromDb.CategoryId = obj.CategoryId;
            objFromDb.CoverTypeId = obj.CoverTypeId;

            if (!obj.ImageURL.IsNullOrEmpty())
            {
                objFromDb.ImageURL = obj.ImageURL;
            }
        }
    }
}
