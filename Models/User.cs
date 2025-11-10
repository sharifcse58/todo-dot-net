using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MyApiProject.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [Required, StringLength(50)]
    public string Name { get; set; } = default!;

    [Required, EmailAddress, StringLength(120)]
    public string Email { get; set; } = default!;

    [StringLength(30)]
    public string? Role { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}
