﻿using Ex2_OMT.Auth;
using Ex2_OMT.Models;
using Ex2_OMT.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Ex2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        public CategoryController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        [Authorize]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll(string? search = "", int? isDeleted = null, int page = 1)
        {
            var category = await _categoryRepository.GetAll(search, isDeleted, page);
            return Ok(category);
        }

        [HttpPut("Disable")]
        public async Task<IActionResult> DisableCategory(int id)
        {
            var category = await _categoryRepository.DisableCategory(id);
            return Ok(category);
        }

        [HttpPost("GetCategory")]
        public async Task<IActionResult> GetUser(int id)
        {
            var category = await _categoryRepository.GetCategory(id);
            return Ok(category);
        }

        [HttpPost("CreateCategory")]
        public async Task<IActionResult> CreateCategory(string category)
        {
            var categoryCreated = await _categoryRepository.CreateCategory(category);
            return Ok(categoryCreated);
        }

        [HttpPut("EditCategory")]
        public async Task<IActionResult> EditCategory(CategoryDTO category)
        {
            var categoryEdited = await _categoryRepository.EditCategory(category);
            return Ok(categoryEdited);
        }
    }
}
