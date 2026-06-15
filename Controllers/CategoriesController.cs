using LibraryManagementSystem.Helpers;
using LibraryManagementSystem.Models.Entity;
using LibraryManagementSystem.Services.Impl;
using LibraryManagementSystem.Services.Interface;
using System;
using System.Web.Http;

namespace LibraryManagementSystem.Controllers
{
    [TokenAuth]
    public class CategoriesController : ApiController
    {
        private readonly ICategoryService categoryService;

        public CategoriesController()
        {
            categoryService = new CategoryService();
        }

        [HttpGet]
        [Route("api/categories/GetAll")]
        public IHttpActionResult GetAll()
        {
            var categories = categoryService.GetAllCategories();
            return Ok(categories);
        }

        [HttpGet]
        [Route("api/categories/GetById/{id}")]
        public IHttpActionResult GetById(int id)
        {
            var category = categoryService.GetCategoryById(id);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

        [HttpPost]
        [Route("api/categories/Insert")]
        public IHttpActionResult Insert([FromBody] Category category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = categoryService.InsertCategory(category);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("api/categories/Update/{id}")]
        public IHttpActionResult Update(int id, [FromBody] Category category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = categoryService.UpdateCategory(id, category);

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

        [HttpDelete]
        [Route("api/categories/Delete/{id}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var result = categoryService.DeleteCategory(id);

                if (!result)
                {
                    return NotFound();
                }

                return Ok("Category deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}