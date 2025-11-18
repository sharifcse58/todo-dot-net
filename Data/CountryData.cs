using System;
using MyApiProject.Models;
namespace MyApiProject.Data
{
    public class CountryData
    {
        public static List<Country> Get()
        {

            var countries = new[]
            {
                new{ Id = 1, Name = "Jahid"},
                new{ Id = 1, Name = "Arif"}
            };

            return countries.Select(c => new Country { Id = c.Id, Name = c.Name }).ToList();

        }
    }
}

