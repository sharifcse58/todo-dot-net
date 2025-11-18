using System;
using System.ComponentModel.DataAnnotations;

namespace MyApiProject.DTOs
{
    public class UserDto
    {
        public string? Id { get; set; }

        public string Name { get; set; } = default!;

        public string Email { get; set; } = default!;
    }
}

