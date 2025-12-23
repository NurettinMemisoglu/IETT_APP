using AutoMapper;
using IETT_APP.Application.Dtos.Driver;
using IETT_APP.Domain.Entities;
using IETT_APP.Domain.Enums; // TaskState için gerekli

namespace IETT_APP.Application.Mapping
{
    public class DriverProfile : Profile
    {
        public DriverProfile()
        {
            // === DRIVER MAPPINGS (Mevcut Olanlar) ===
            CreateMap<Driver, DriverDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.User.Surname))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.GarageName, opt => opt.MapFrom(src => src.Garage != null ? src.Garage.GarageName : null));

            CreateMap<CreateDriverDto, Driver>();
            CreateMap<UpdateDriverDto, Driver>();
            CreateMap<CompleteProfileDto, Driver>();
            CreateMap<UpdateDriverProfileDto, Driver>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());


            // === DASHBOARD TASK MAPPING (YENİ VE ÖNEMLİ KISIM) ===
            CreateMap<TripTask, DashboardTaskDto>()
                // 1. Basit String Alanlar
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Title) ? src.Title : $"{src.Line.Code} Seferi"))
                .ForMember(dest => dest.LineCode, opt => opt.MapFrom(src => src.Line != null ? src.Line.Code : "?"))
                .ForMember(dest => dest.LineName, opt => opt.MapFrom(src => src.Line != null ? src.Line.Name : "Hat Bilgisi Yok"))
                .ForMember(dest => dest.RouteName, opt => opt.MapFrom(src => src.Route != null ? src.Route.Name : "Yön Yok"))
                .ForMember(dest => dest.VehiclePlate, opt => opt.MapFrom(src => src.Vehicle != null ? src.Vehicle.PlateNumber : "Atanmadı"))
                .ForMember(dest => dest.DoorNumber, opt => opt.MapFrom(src => src.Vehicle != null ? src.Vehicle.DoorNumber : "-"))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? ""))

                // 2. Zaman Mantığı (Varsa Revize, Yoksa Planlanan)
                .ForMember(dest => dest.TaskDate, opt => opt.MapFrom(src => (src.AdjustedDeparture ?? src.ScheduledDeparture) ?? DateTime.MinValue))

                // 3. Formatlı Saat (HH:mm)
                .ForMember(dest => dest.FormattedTime, opt => opt.MapFrom((src, dest) =>
                {
                    // Kaynak tarih: Adjusted ?? Scheduled
                    var targetDate = src.AdjustedDeparture ?? src.ScheduledDeparture;
                    return targetDate.HasValue ? targetDate.Value.ToString("HH:mm") : "--:--";
                }))

                // 4. Gecikme Hesaplama (Custom Logic)
                .ForMember(dest => dest.DelayMinutes, opt => opt.MapFrom((src, dest) =>
                {
                    var targetDate = src.AdjustedDeparture ?? src.ScheduledDeparture;
                    if (!targetDate.HasValue) return 0;

                    // Görev henüz başlamadıysa "Şu an" ile kıyasla. 
                    // Eğer geçmişse gecikme pozitiftir.
                    return (int)(DateTime.Now - targetDate.Value).TotalMinutes;
                }))

                // 5. Aciliyet ve Buton Metni (AfterMap ile)
                .AfterMap((src, dest) =>
                {
                    // --- ACİLİYET (IsUrgent) ---
                    if (src.Status == TaskState.InProgress)
                    {
                        dest.IsUrgent = true;
                    }
                    else if (src.Status == TaskState.Accepted)
                    {
                        dest.IsUrgent = true;
                    }
                    else if (src.Status == TaskState.Pending)
                    {
                        // 15 dakikadan az kaldıysa veya zamanı geçtiyse ACİL
                        var targetDate = src.AdjustedDeparture ?? src.ScheduledDeparture;
                        if (targetDate.HasValue)
                        {
                            var minutesLeft = (targetDate.Value - DateTime.Now).TotalMinutes;
                            if (minutesLeft < 15) dest.IsUrgent = true;
                        }
                    }

                    // --- BUTON METNİ (ActionButtonText) ---
                    dest.ActionButtonText = src.Status switch
                    {
                        TaskState.Pending => "KABUL ET",
                        TaskState.Accepted => "SEFERİ BAŞLAT",
                        TaskState.InProgress => "SEFERİ BİTİR", // veya "Tamamla"
                        _ => "DETAYLARI GÖR"
                    };
                });
        }
    }
}