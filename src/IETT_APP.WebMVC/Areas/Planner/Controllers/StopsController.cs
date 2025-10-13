using IETT_APP.WebMVC.Areas.Planner.Extensions;
using IETT_APP.WebMVC.Areas.Planner.Models;
using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
        return View();
    }

    // POST: Planner/Stops/Create
    [HttpPost]
    public async Task<IActionResult> Create(StopViewModel model)
    {


        if (!ModelState.IsValid) return View(model);

        await _stopService.CreateAsync(model.ToCreateDto());
        return RedirectToAction("Index", "Home");
    }

    // GET: Planner/Stops/Edit/{id}
    public async Task<IActionResult> Edit(string id)
    {
        var stop = await _stopService.GetByIdAsync(id);
        if (stop == null) return NotFound();

        return View(stop.ToViewModel());
    }

    // POST: Planner/Stops/Edit/{id}
    [HttpPost]
    public async Task<IActionResult> Edit(string id, StopViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        await _stopService.UpdateAsync(id, model.ToUpdateDto());
        return RedirectToAction(nameof(Index));
    }

    // GET: Planner/Stops/Details/{id}
    public async Task<IActionResult> Details(string id)
    {
        var stop = await _stopService.GetByIdAsync(id);
        if (stop == null) return NotFound();
        return View(stop.ToViewModel());
    }

    // POST: Planner/Stops/Delete/{id}
    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        await _stopService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
