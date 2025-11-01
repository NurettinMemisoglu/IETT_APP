using IETT_APP.Domain.Enums;
using IETT_APP.WebMVC.Areas.Planner.Extensions;
using IETT_APP.WebMVC.Areas.Planner.Models;
using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace IETT_APP.WebMVC.Areas.Planner.Controllers
{
    [Authorize(Roles = "Planner")]
    [Area("Planner")]
    public class StopsController : Controller
    {
        private readonly IStopApiService _stopApiService;

        public StopsController(IStopApiService stopApiService)
        {
            _stopApiService = stopApiService;
        }

        // GET: Planner/Stops
        public async Task<IActionResult> Index(string? search)
        {
            var stopDtos = string.IsNullOrWhiteSpace(search)
                ? await _stopApiService.GetAllAsync()
                : await _stopApiService.SearchByNameAsync(search);

            // StopDto -> StopViewModel dönüşümü
            var stopViewModels = stopDtos.Select(dto => dto.ToViewModel()).ToList();

            return View(stopViewModels);
        }

        // GET: Planner/Stops/Create
        public IActionResult Create()
        {

            var model = new StopViewModel
            {
                Location = new LocationViewModel() // boş ama null değil
            };

            PopulateDropdowns();
            return View();
        }

        // POST: Planner/Stops/Create
        [HttpPost]
        public async Task<IActionResult> Create(StopViewModel model)
        {
            model.Location ??= new LocationViewModel();

            // Name alanını title case yap
            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                model.Name = string.Join(" ", model.Name
                    .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                    .Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower()));
            }

            // Kod alanını sadece sayı olarak temizle
            if (!string.IsNullOrWhiteSpace(model.Code))
            {
                model.Code = new string(model.Code.Where(char.IsDigit).ToArray());
            }
            if (!ModelState.IsValid)
            {
                PopulateDropdowns();
                return View(model);
            }

            await _stopApiService.CreateAsync(model.ToCreateDto());
            return RedirectToAction("Index", "Stops");
        }


        // GET: Planner/Stops/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            var stop = await _stopApiService.GetByIdAsync(id);
            if (stop == null) return NotFound();

            var model = stop.ToViewModel();
            model.Location ??= new LocationViewModel(); // null güvenliği


            // Dropdown’ları doldur
            PopulateDropdowns();

            return View(model);
        }

        // POST: Planner/Stops/Edit/{id}
        [HttpPost]
        public async Task<IActionResult> Edit(string id, StopViewModel model)
        {

            model.Location ??= new LocationViewModel();

            // Name alanını title case yap
            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                model.Name = string.Join(" ", model.Name
                    .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                    .Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower()));
            }

            // Kod alanını sadece sayı olarak temizle
            if (!string.IsNullOrWhiteSpace(model.Code))
            {
                model.Code = new string(model.Code.Where(char.IsDigit).ToArray());
            }

            if (!ModelState.IsValid)
            {
                // Dropdown’ları tekrar doldurmazsan select boxlar boş kalır
                PopulateDropdowns();

                // Hata varsa formu view’e geri gönder
                return View(model);
            }

            await _stopApiService.UpdateAsync(id, model.ToUpdateDto());
            TempData["SuccessMessage"] = "Durak başarıyla güncellendi!";
            return RedirectToAction("Index", "Stops");
        }


        // GET: Planner/Stops/Details/{id}
        public async Task<IActionResult> Details(string id)
        {
            var stop = await _stopApiService.GetByIdAsync(id);
            if (stop == null) return NotFound();

            var model = stop.ToViewModel();

            PopulateDropdowns(); // Eğer dropdown kullanılacaksa

            return View(model); ;
        }

        // POST: Planner/Stops/Delete/{id}
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            await _stopApiService.DeleteAsync(id);
            return RedirectToAction("Index", "Stops");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var stops = await _stopApiService.GetAllAsync();
            return Json(stops);
        }


        private void PopulateDropdowns()
        {
            // StopType dropdown
            ViewBag.StopTypeList = Enum.GetValues(typeof(StopType))
                .Cast<StopType>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
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
                    Value = e.ToString(),
                    Text = e switch
                    {
                        SmartStop.Yes => "Evet",
                        SmartStop.No => "Hayır",
                        _ => e.ToString()
                    }
                }).ToList();

            // District dropdown (İstanbul ilçeleri)
            var districts = new List<string> {
            "Adalar","Arnavutköy","Ataşehir","Avcılar","Bağcılar",
            "Bahçelievler","Bakırköy","Başakşehir","Bayrampaşa","Beşiktaş",
            "Beykoz","Beylikdüzü","Beyoğlu","Büyükçekmece","Çekmeköy",
            "Esenler","Esenyurt","Eyüpsultan","Fatih","Gaziosmanpaşa",
            "Güngören","Kadıköy","Kağıthane","Kartal","Küçükçekmece",
            "Maltepe","Pendik","Sancaktepe","Sarıyer","Silivri",
            "Sultanbeyli","Sultangazi","Şile","Şişli","Tuzla",
            "Ümraniye","Üsküdar","Zeytinburnu"
        };

            ViewBag.DistrictList = districts
                .Select(d => new SelectListItem { Value = d, Text = d })
                .ToList();
        }


    }
}
