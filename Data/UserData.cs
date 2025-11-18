using System;
using MyApiProject.Models;

namespace MyApiProject.Data
{
    public class UserData
    {

        public static List<User> Get()
        {

            var users = new[]
            {
                new{Id = "asdfadsfasdfasdfads", UserName = "Meherin", Email = "adfas@gmail.com"},
                new{Id = "23sdfasdfsdfgsdfgsdfg", UserName = "Mehedi", Email = "adfassss@gmail.com"}
            };

            return users.Select(u => new User { Id = u.Id, Name = u.UserName, Email = u.Email }).ToList();

        }

    }
}

