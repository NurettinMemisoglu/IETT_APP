using IETT_APP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IETT_APP.Infrastructure.Persistence.Seed
{
    public static class GarageSeed
    {
        public static async Task SeedGaragesAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (!await context.Set<Garage<Guid>>().AnyAsync())
            {
                var garages = new List<Garage<Guid>>
                {
                    new Garage<Guid>
                    {
                        Id = Guid.NewGuid(),
                        GarageName = "İkitelli Garajı",
                        Capacity = 850,
                        Fileld = 204572,
                        YearStarted = 1986,
                        Location = new Location { Latitude = 41.058146M, Longitude = 28.791750M },
                        IsActive = true
                    },
                    new Garage<Guid>
                    {
                        Id = Guid.NewGuid(),
                        GarageName = "Edirnekapı Garajı",
                        Capacity = 220,
                        Fileld = 60000,
                        YearStarted = 1999,
                        Location = new Location { Latitude = 41.028333M, Longitude = 28.928333M },
                        IsActive = true
                    },
                    new Garage<Guid>
                    {
                        Id = Guid.NewGuid(),
                        GarageName = "Beylikdüzü Garajı",
                        Capacity = 75,
                        Fileld = 10000,
                        YearStarted = 2013,
                        Location = new Location { Latitude = 40.968246M,  Longitude = 28.603029M },
                        IsActive = true
                    },
                    new Garage<Guid>
                    {
                        Id = Guid.NewGuid(),
                        GarageName = "Kağıthane Garajı",
                        Capacity = 250,
                        Fileld = 65000,
                        YearStarted = 1995,
                        Location = new Location { Latitude = 41.065768M, Longitude = 28.958618M },
                        IsActive = true
                    },
                    new Garage<Guid>
                    {
                        Id = Guid.NewGuid(),
                        GarageName = "Anadolu Garajı",
                        Capacity = 350,
                        Fileld = 52200,
                        YearStarted = 1986,
                        Location = new Location { Latitude = 40.979096M,  Longitude = 29.133096M },
                        IsActive = true
                    },
                    new Garage<Guid>
                    {
                        Id = Guid.NewGuid(),
                        GarageName = "Hasanpaşa Şehit Ahmet Dokuyucu Garajı",
                        Capacity = 165,
                        Fileld = 37000,
                        YearStarted = 1984,
                        Location = new Location { Latitude = 40.993680M, Longitude = 29.043701M },
                        IsActive = true
                    },
                    new Garage<Guid>
                    {
                        Id = Guid.NewGuid(),
                        GarageName = "Avcılar Garajı",
                        Capacity = 165,
                        Fileld = 20000,
                        YearStarted = 2007,
                        Location = new Location { Latitude = 40.986003M, Longitude = 28.730519M },
                        IsActive = true
                    },
                    new Garage<Guid>
                    {
                        Id = Guid.NewGuid(),
                        GarageName = "Kurtköy Garajı",
                        Capacity = 350,
                        Fileld = 46679,
                        YearStarted = 2018,
                        Location = new Location { Latitude = 40.929024M, Longitude = 29.318056M },
                        IsActive = true
                    },
                    new Garage<Guid>
                    {
                        Id = Guid.NewGuid(),
                        GarageName = "Yunus (Kartal) Garajı",
                        Capacity = 250,
                        Fileld = 17500,
                        YearStarted = 2014,
                        Location = new Location { Latitude = 40.889038M, Longitude = 29.206976M },
                        IsActive = true
                    },
                    new Garage<Guid>
                    {
                        Id = Guid.NewGuid(),
                        GarageName = "Sultangazi Garajı",
                        Capacity = 250,
                        Fileld = 55000,
                        YearStarted = 2023,
                        Location = new Location { Latitude = 41.110140M, Longitude = 28.877433M },
                        IsActive = true
                    },
                    new Garage<Guid>
                    {
                        Id = Guid.NewGuid(),
                        GarageName = "Sarıgazi Garajı",
                        Capacity = 200,
                        Fileld = 15376,
                        YearStarted = 1999,
                        Location = new Location { Latitude = 41.003925M, Longitude = 29.201200M },
                        IsActive = true
                    },
                    new Garage<Guid>
                    {
                        Id = Guid.NewGuid(),
                        GarageName = "Topkapı Garajı",
                        Capacity = 100,
                        Fileld = 17588,
                        YearStarted = 1955,
                        Location = new Location { Latitude = 41.016667M, Longitude = 28.928611M },
                        IsActive = true
                    },
                    new Garage<Guid>
                    {
                        Id = Guid.NewGuid(),
                        GarageName = "Şahinkaya Garajı",
                        Capacity = 135,
                        Fileld = 15000,
                        YearStarted = 1997,
                        Location = new Location { Latitude = 41.141465M, Longitude = 29.095613M},
                        IsActive = true
                    },
                    new Garage<Guid>
                    {
                        Id = Guid.NewGuid(),
                        GarageName = "Adalar Garajı",
                        Capacity = 115,
                        Fileld = 7230,
                        YearStarted = 2020,
                        Location = new Location { Latitude = 40.874049M, Longitude = 29.129026M},
                        IsActive = true
                    }
                };

                await context.AddRangeAsync(garages);
                await context.SaveChangesAsync();
            }
        }
    }
}
