using LibraryManagementSystem.Models.Entity;
using System.Collections.Generic;

namespace LibraryManagementSystem.Services.Interface
{
    public interface ICategoryService
    {
        List<Category> GetAllCategories();
        Category GetCategoryById(int id);
        Category InsertCategory(Category category);
        Category UpdateCategory(int id, Category category);
        bool DeleteCategory(int id);
    }
}