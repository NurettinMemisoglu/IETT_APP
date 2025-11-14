using IETT_APP.Domain.Enums;
using IETT_APP.WebMVC.Areas.Chief.Extensions;
using IETT_APP.WebMVC.Areas.Chief.Models;
using IETT_APP.WebMVC.Areas.Planner.Extensions;
using IETT_APP.WebMVC.Extensions;
using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IETT_APP.WebMVC.Areas.Chief.Controllers
{
    [Authorize(Roles = "Chief")]
    [Area("Chief")]
    public class TripTasksController : Controller
    {
        private readonly ITripTaskApiService _tripTaskApiService;

        public TripTasksController(ITripTaskApiService tripTaskApiService)
        {
            _tripTaskApiService = tripTaskApiService;
        }

        // ==============================
        // INDEX
        // ==============================
        public async Task<IActionResult> Index()
        {
            try
            {
                var tasks = await _tripTaskApiService.GetAllAsync();

                var activeTasks = tasks
                    .Where(x => !x.IsDeleted)
                    .Select(x => x.ToViewModel())
                    .ToList();

                return View(activeTasks);
            }
            catch
            {
                TempData["ErrorMessage"] = "Görev listesi yüklenirken hata oluştu.";
                return View(new List<TripTaskViewModel>());
            }
        }

        // ==============================
        // SEARCH (partial)
        // ==============================
        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            try
            {
                var result = string.IsNullOrWhiteSpace(term)
                    ? await _tripTaskApiService.GetAllAsync()
                    : await _tripTaskApiService.SearchAsync(term.Trim());

                var viewModels = result
                    .Where(x => !x.IsDeleted)
                    .Select(x => x.ToViewModel())
                    .ToList();

                return PartialView("_TripTasksTablePartial", viewModels);
            }
            catch
            {
                return StatusCode(500, "Görev arama sırasında hata oluştu.");
            }
        }

        // ==============================
        // CREATE (GET)
        // ==============================
        [HttpGet]
        public IActionResult Create()
        {
            // Sadece ENUM → ViewBag ile gider
            ViewBag.TaskStateList = EnumSelectListHelper.ToSelectList<TaskState>();

            // Route, Line, Vehicle, Operator, Garage → JS ile doldurulacak!
            return View(new TripTaskViewModel());
        }

        // ==============================
        // CREATE (POST)
        // ==============================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TripTaskViewModel vm)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Model doğrulama hatası" });

            var dto = vm.ToCreateDto();

            try
            {
                var result = await _tripTaskApiService.CreateAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "API create hata", detail = ex.Message });
            }
        }

        // ==============================
        // EDIT (GET)
        // ==============================
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var task = await _tripTaskApiService.GetByIdAsync(id);

                if (task == null || task.IsDeleted)
                    return NotFound();

                ViewBag.TaskStateList = EnumSelectListHelper.ToSelectList<TaskState>();

                // DROPDOWN'lar yine JS tarafından doldurulacak
                return View(task.ToViewModel());
            }
            catch
            {
                TempData["ErrorMessage"] = "Görev düzenleme sayfası hata verdi.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ==============================
        // EDIT (POST)
        // ==============================
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] TripTaskViewModel vm)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Model doğrulama hatası" });

            var dto = vm.ToUpdateDto();

            try
            {
                var result = await _tripTaskApiService.UpdateAsync(dto.Id, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "API edit hata", detail = ex.Message });
            }
        }

        // ==============================
        // DELETE (GET)
        // ==============================
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var task = await _tripTaskApiService.GetByIdAsync(id);

                if (task == null || task.IsDeleted)
                    return NotFound();

                return View(task.ToViewModel());
            }
            catch
            {
                TempData["ErrorMessage"] = "Silme sayfası yüklenemedi.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ==============================
        // DELETE (POST)
        // ==============================
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var result = await _tripTaskApiService.DeleteAsync(id);

                if (!result)
                {
                    TempData["ErrorMessage"] = "Görev silinemedi.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["SuccessMessage"] = "Görev silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["ErrorMessage"] = "Görev silinirken hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
