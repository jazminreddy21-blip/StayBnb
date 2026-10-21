using Microsoft.EntityFrameworkCore;
using StayBnb.Data;
using StayBnb.Models;

namespace StayBnb.DataSeeders
{
    public static class SamplePropertySeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            string hostId)
        {
            if (!await context.Properties.AnyAsync())
            {
                var properties = new List<Property>
                {
                    new Property
                    {
                        Title = "Modern Coastal Apartment",
                        Description = "A bright, comfortable apartment near the beach.",
                        City = "Durban",
                        Address = "Umhlanga, Durban",
                        PropertyType = "Apartment",
                        PricePerNight = 950,
                        CleaningFee = 150,
                        ServiceFee = 100,
                        IsActive = true,
                        HostId = hostId
                    },
                    new Property
                    {
                        Title = "Cosy Cape Town Getaway",
                        Description = "A welcoming space for a relaxing city break.",
                        City = "Cape Town",
                        Address = "Sea Point, Cape Town",
                        PropertyType = "Apartment",
                        PricePerNight = 1200,
                        CleaningFee = 180,
                        ServiceFee = 120,
                        IsActive = true,
                        HostId = hostId
                    },
                    new Property
                    {
                        Title = "Peaceful Johannesburg Home",
                        Description = "A spacious home suitable for a weekend escape.",
                        City = "Johannesburg",
                        Address = "Sandton, Johannesburg",
                        PropertyType = "House",
                        PricePerNight = 850,
                        CleaningFee = 130,
                        ServiceFee = 90,
                        IsActive = true,
                        HostId = hostId
                    }
                };

                context.Properties.AddRange(properties);
                await context.SaveChangesAsync();
            }

            var sampleProperties = await context.Properties
                .Where(p =>
                    p.Title == "Modern Coastal Apartment" ||
                    p.Title == "Cosy Cape Town Getaway" ||
                    p.Title == "Peaceful Johannesburg Home")
                .Include(p => p.Images)
                .ToListAsync();

            var images = new Dictionary<string, string>
            {
                { "Modern Coastal Apartment", "/images/Durban.png" },
                { "Cosy Cape Town Getaway", "/images/CPT.png" },
                { "Peaceful Johannesburg Home", "/images/JOZI.png" }
            };

            foreach (var property in sampleProperties)
            {
                if (!property.Images.Any() &&
                    images.TryGetValue(property.Title, out var imageUrl))
                {
                    context.PropertyImages.Add(new PropertyImage
                    {
                        PropertyId = property.Id,
                        ImageUrl = imageUrl,
                        IsPrimary = true
                    });
                }
            }

            await context.SaveChangesAsync();
        }
    }
}