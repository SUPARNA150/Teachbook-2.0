using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Teachbook.Web.Data;
using Teachbook.Web.Models.Domain;
using Teachbook.Web.Models.ViewModels;
using Teachbook.Web.Repositories;

namespace Teachbook.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminTagsController : Controller
    {
        private readonly ITagRepository tagRepository;

        public AdminTagsController(ITagRepository tagRepository)
        {
            this.tagRepository = tagRepository;
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Add")]

        //Mannual approach

        //public IActionResult SubmitTag()
        //{
        //    var name = Request.Form["name"];
        //    var displayName = Request.Form["displayName"];
        //    return View("Add");
        //}

        //model binding

        public async Task<IActionResult> Add(AddTagRequest addTagRequest)
        {
            ValidateAddTagRequest(addTagRequest);

            if (ModelState.IsValid == false)
            {
                return View();
            }
            //var Name = addTagRequest.Name;
            //var DisplayName = addTagRequest.DisplayName;

            // Mapping AddTagRequest to Tag domain model
            var tag = new Tag
            {
                Name = addTagRequest.Name,
                DisplayName = addTagRequest.DisplayName
            };
            await tagRepository.AddAsync(tag);

            //await bloggieDbContext.Tags.AddAsync(tag);
            //await bloggieDbContext.SaveChangesAsync();
            //return View("Add");

            //after adding tag it redirect to the tag list page
            return RedirectToAction("List");
        }

        [HttpGet]
        [ActionName("List")]
        public async Task<IActionResult> List(string? searchQuery)
        {
            ViewBag.SearchQuery = searchQuery;

            //use dbcontext to read the tags
            var tags = await tagRepository.GetAllAsync(searchQuery);

            return View(tags);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)   //name of the parameter should match with the route which is present on list.cshtml page
        {
            //1st method
            //var tag = bloggieDbContext.Tags.Find(id);

            var tag = await tagRepository.GetAsync(id);
            if (tag != null)
            {
                var editTagRequest = new EditTagRequest
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    DisplayName = tag.DisplayName
                };
                return View(editTagRequest);
            }
            return View(null);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditTagRequest editTagRequest)
        {
            var tag = new Tag
            {
                Id = editTagRequest.Id,
                Name = editTagRequest.Name,
                DisplayName = editTagRequest.DisplayName
            };

            var updatedTag = await tagRepository.UpdateAsync(tag);
            if (updatedTag != null)
            {
                // Show success notification
                return RedirectToAction("List");
            }
            else
            {
                // Show error notification
            }
            //show error notification
            return RedirectToAction("Edit", new { id = editTagRequest.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(EditTagRequest editTagRequest)
        {
            var deletedTag = await tagRepository.DeleteAsync(editTagRequest.Id);

            if (deletedTag != null)
            {
                // Show success notification
                return RedirectToAction("List");
            }
            //show error notification
            return RedirectToAction("Edit", new { id = editTagRequest.Id });
        }

        private void ValidateAddTagRequest(AddTagRequest request)
        {
            if (request.Name != null && request.DisplayName != null)
            {
                if (request.Name == request.DisplayName)
                {
                    ModelState.AddModelError("DisplayName", "Name cannot be the same as DisplayName");
                }
            }
        }
    }
}
