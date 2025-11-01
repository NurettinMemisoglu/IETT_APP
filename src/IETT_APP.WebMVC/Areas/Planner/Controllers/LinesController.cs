using IETT_APP.Domain.Enums;
using IETT_APP.WebMVC.Areas.Planner.Models;
using IETT_APP.WebMVC.Extensions;
using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace IETT_APP.WebMVC.Areas.Planner.Controllers
{
    [Authorize(Roles = "Planner")]
    [Area("Planner")]
    public class LinesController : Controller
    {
        private readonly ILineApiService _lineApiService;

        public LinesController(ILineApiService lineApiService)
        {
            _lineApiService = lineApiService;
        }

        public async Task<IActionResult> Index()
        {

            var lines = await _lineApiService.GetAllAsync();
            return View(lines.Where(x => !x.IsDeleted).Select(x => x.ToViewModel()).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            var result = string.IsNullOrWhiteSpace(term)
                ? await _lineApiService.GetAllAsync()
                : await _lineApiService.SearchAsync(term);

            // 🔹 Silinmiş hatları filtrele
            result = result.Where(x => !x.IsDeleted).ToList();

            var viewModels = result.Select(x => x.ToViewModel()).ToList();
            return PartialView("_LinesTablePartial", viewModels);
        }


        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.LineTypeList = GetLineTypeSelectList();
            return PartialView("_LineFormPartial", new LineViewModel { IsActive = true });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var line = await _lineApiService.GetByIdAsync(id);
            if (line == null || line.IsDeleted) return NotFound();
            ViewBag.LineTypeList = GetLineTypeSelectList();
            return PartialView("_LineFormPartial", line.ToViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Execute([FromBody] LineViewModel vm)
        {
            Console.WriteLine(await new StreamReader(Request.Body).ReadToEndAsync());
            var errors = ModelState
        .Where(kvp => kvp.Value.Errors.Any())   // Sadece hatası olan alanlar
        .Select(kvp => new
        {
            Key = kvp.Key,                     // Hata olan alanın adı (örn. "Code")
            Errors = kvp.Value.Errors
                         .Select(e => e.ErrorMessage)
                         .ToArray()            // Bu alandaki tüm hata mesajları
        })
        .ToList();

            var dto = vm.ToDto();

            try
            {
                var result = await _lineApiService.CreateOrUpdateAsync(dto);
                Console.WriteLine("GÖNDERİLEN JSON: " + JsonSerializer.Serialize(dto));
                return Ok(result); // Güncellenmiş DTO dönülüyor
            }
            catch (Exception ex)
            {
                return BadRequest("API hata: " + ex.Message);
            }
        }



        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _lineApiService.DeleteAsync(id);
            if (!result) return BadRequest("Silme işlemi başarısız.");
            return Ok(new { message = "Hat başarıyla silindi." });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var lines = await _lineApiService.GetAllAsync();
            return Json(lines);
        }

        private SelectList GetLineTypeSelectList() => new SelectList(
            Enum.GetValues(typeof(LineType)).Cast<LineType>().Select(x => new { Value = (int)x, Text = x.ToDisplayName() }),
            "Value", "Text"
        );
    }

}
