using IETT_APP.Domain.Enums;
using IETT_APP.WebMVC.Areas.Planner.Extensions;
using IETT_APP.WebMVC.Areas.Planner.Models;
using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

[Area("Planner")]
public class StopsController : Controller
{
    private readonly IStopService _stopService;

    public StopsController(IStopService stopService)
    {
        _stopService = stopService;
    }

    // GET: Planner/Stops
    public async Task<IActionResult> Index(string? search)
    {
        var stopDtos = string.IsNullOrWhiteSpace(search)
            ? await _stopService.GetAllAsync()
            : await _stopService.SearchByNameAsync(search);

        // StopDto -> StopViewModel dönüşümü
        var stopViewModels = stopDtos.Select(dto => dto.ToViewModel()).ToList();

        return View(stopViewModels);
    }

    // GET: Planner/Stops/Create
    public IActionResult Create()
    {
        PopulateDropdowns();
        return View();
    }

    // POST: Planner/Stops/Create
    [HttpPost]
    public async Task<IActionResult> Create(StopViewModel model)
    {
        if (!ModelState.IsValid)
        {
            PopulateDropdowns();
            return View(model);
        }

        await _stopService.CreateAsync(model.ToCreateDto());
        return RedirectToAction("Index", "Home");
    }


    // GET: Planner/Stops/Edit/{id}
    public async Task<IActionResult> Edit(string id)
    {
        var stop = await _stopService.GetByIdAsync(id);
        if (stop == null) return NotFound();

        var model = stop.ToViewModel();

        // Dropdown’ları doldur
        PopulateDropdowns();

        return View(model);
    }

    // POST: Planner/Stops/Edit/{id}
    [HttpPost]
    public async Task<IActionResult> Edit(string id, StopViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        await _stopService.UpdateAsync(id, model.ToUpdateDto());
        return RedirectToAction("Index", "Home");
    }

    // GET: Planner/Stops/Details/{id}
    public async Task<IActionResult> Details(string id)
    {
        var stop = await _stopService.GetByIdAsync(id);
        if (stop == null) return NotFound();

        var model = stop.ToViewModel();

        PopulateDropdowns(); // Eğer dropdown kullanılacaksa

        return View(model); ;
    }

    // POST: Planner/Stops/Delete/{id}
    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        await _stopService.DeleteAsync(id);
        return RedirectToAction("Index", "Home");
    }

    private void PopulateDropdowns()
    {
        // StopType dropdown
        ViewBag.StopTypeList = Enum.GetValues(typeof(StopType))
            .Cast<StopType>()
            .Select(e => new SelectListItem
            {
                Value = e.ToString(), // <-- enum adı string olarak
                Text = e switch
                {
                    StopType.AcikDurak => "Açık Durak",
                    StopType.KapaliDurak => "Kapalı Durak",
                    StopType.FullKapaliDurak => "Full Kapalı Durak",
                    _ => e.ToString()
                }
            }).ToList();

        // SmartStop dropdown
        ViewBag.SmartStopList = Enum.GetValues(typeof(SmartStop))
            .Cast<SmartStop>()
            .Select(e => new SelectListItem
            {
                Value = e.ToString(), // <-- enum adı string olarak
                Text = e switch
                {
                    SmartStop.Yes => "Evet",
                    SmartStop.No => "Hayır",
                    _ => e.ToString()
                }
            }).ToList();
    }

}

