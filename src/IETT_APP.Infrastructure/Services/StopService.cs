using IETT_APP.Application.Dtos;
using IETT_APP.Application.Interfaces;
using IETT_APP.Applicaton.Dtos.Stop;
using IETT_APP.Domain.Entities;
using IETT_APP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IETT_APP.Infrastructure.Services
{
    public class StopService : IStopService
    {
        private readonly AppDbContext _context;

        public StopService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<StopDto>> GetAllAsync()
        {
            var stops = await _context.Stops.ToListAsync();
            return stops.Select(MapToDto).ToList();
        }

        public async Task<StopDto?> GetByIdAsync(string id)
        {
            var stop = await _context.Stops.FindAsync(id);
            return stop == null ? null : MapToDto(stop);
        }

        public async Task<StopDto> CreateAsync(CreateStopDto dto)
        {
            // Kodun zaten var mı kontrol et
            if (await _context.Stops.AnyAsync(s => s.Code == dto.Code))
            {
                throw new ArgumentException($"'{dto.Code}' kodu ile başka bir durak zaten mevcut.");
            }

            var stop = new Stop
            {
                Id = Guid.NewGuid().ToString(),
                Code = dto.Code,
                Name = dto.Name,
                District = dto.District,
                StopType = dto.StopType,
                SmartStop = dto.SmartStop,
                Location = new Location
                {
                    Latitude = Convert.ToDecimal(dto.Location.Latitude),
                    Longitude = Convert.ToDecimal(dto.Location.Longitude)
                }
            };

            _context.Stops.Add(stop);
            await _context.SaveChangesAsync();

            return MapToDto(stop);
        }


        public async Task<bool> UpdateAsync(string id, UpdateStopDto dto)
        {
            var stop = await _context.Stops.FindAsync(id);
            if (stop == null) return false;

            // Code güncelle
            if (!string.IsNullOrWhiteSpace(dto.Code))
            {
                // Unique kod kontrolü
                if (await _context.Stops.AnyAsync(s => s.Code == dto.Code && s.Id != id))
                    throw new ArgumentException($"'{dto.Code}' kodu başka bir durak tarafından kullanılıyor.");

                stop.Code = dto.Code;
            }

            // Name güncelle
            if (!string.IsNullOrWhiteSpace(dto.Name))
                stop.Name = dto.Name;
            stop.District = dto.District;
            stop.StopType = dto.StopType;
            stop.SmartStop = dto.SmartStop;

            // Location güncelle
            if (dto.Location != null)
            {
                // Latitude ve Longitude opsiyonel güncelleme
                if (dto.Location.Latitude != 0)
                    stop.Location.Latitude = dto.Location.Latitude;

                if (dto.Location.Longitude != 0)
                    stop.Location.Longitude = dto.Location.Longitude;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
                throw;
            }

            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var stop = await _context.Stops.FindAsync(id);
            if (stop == null) return false;

            _context.Stops.Remove(stop);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<StopDto>> SearchByNameAsync(string name)
        {
            var stops = await _context.Stops
                .Where(s => s.Name.Contains(name))
                .ToListAsync();

            return stops.Select(MapToDto).ToList();
        }

        private StopDto MapToDto(Stop stop)
        {
            return new StopDto
            {
                Id = stop.Id,
                Code = stop.Code,
                Name = stop.Name,
                District = stop.District,
                StopType = stop.StopType,
                SmartStop = stop.SmartStop,
                Location = new LocationDto
                {
                    Latitude = stop.Location.Latitude,
                    Longitude = stop.Location.Longitude
                }
            };
        }

    }
}
