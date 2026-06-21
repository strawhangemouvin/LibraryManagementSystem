using LibraryManagementSystem.Models.Entity;
using LibraryManagementSystem.Services.Context;
using LibraryManagementSystem.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryManagementSystem.Services.Impl
{
    public class CategoryService : ICategoryService
    {
        private LibraryDbContext db = new LibraryDbContext();

        public List<Category> GetAllCategories()
        {
            var categories = db.Categories
                .OrderBy(x => x.CategoryName)
                .ToList();

            return categories;
        }

        public Category GetCategoryById(int id)
        {
            var category = db.Categories.Find(id);
            return category;
        }

        public Category InsertCategory(Category category)
        {
            if (category == null)
            {
                throw new Exception("Category data cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                throw new Exception("Category name is required");
            }

            var categoryName = category.CategoryName.Trim();

            var categoryExists = db.Categories.Any(x => x.CategoryName == categoryName);

            if (categoryExists)
            {
                throw new Exception("Category name is already in use");
            }

            category.CategoryName = categoryName;

            if (!string.IsNullOrWhiteSpace(category.Description))
            {
                category.Description = category.Description.Trim();
            }
            else
            {
                category.Description = null;
            }

            db.Categories.Add(category);
            db.SaveChanges();

            return category;
        }

        public Category UpdateCategory(int id, Category category)
        {
            if (category == null)
            {
                throw new Exception("Category data cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                throw new Exception("Category name is required");
            }

            var existingCategory = db.Categories.Find(id);

            if (existingCategory == null)
            {
                return null;
            }

            var categoryName = category.CategoryName.Trim();

            var categoryExists = db.Categories.Any(x =>
                x.CategoryName == categoryName &&
                x.Id != id
            );

            if (categoryExists)
            {
                throw new Exception("Category name is already in use");
            }

            existingCategory.CategoryName = categoryName;

            if (!string.IsNullOrWhiteSpace(category.Description))
            {
                existingCategory.Description = category.Description.Trim();
            }
            else
            {
                existingCategory.Description = null;
            }

            db.SaveChanges();

            return existingCategory;
        }

        public bool DeleteCategory(int id)
        {
            var category = db.Categories.Find(id);

            if (category == null)
            {
                return false;
            }

            var hasBooks = db.Books.Any(x => x.CategoryId == id);

            if (hasBooks)
            {
                throw new Exception("Category cannot be deleted because it is still referenced by books");
            }

            db.Categories.Remove(category);
            db.SaveChanges();

            return true;
        }
    }
}
