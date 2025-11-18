using Microsoft.AspNetCore.Mvc;
using Serilog;
using Bogus;
//using RabbitMQ.Client;
using System.Text;
//using Newtonsoft.Json;
using MyApiProject.Models;
using MyApiProject.Repositories;
using Microsoft.AspNetCore.Connections;
using MongoDB.Bson.IO;
using MyApiProject.Data;
using MyApiProject.DTOs;

namespace MyApiProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        //private readonly IConnectionFactory _rabbitFactory;
        //, IConnectionFactory rabbitFactory
        public TestController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
            //_rabbitFactory = rabbitFactory;
        }

        // Test simple server health
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            Log.Information("Ping test endpoint hit at {Time}", DateTime.UtcNow);
            return Ok(new { message = "Server is alive 🚀", timestamp = DateTime.UtcNow });
        }

        // Test Serilog logging
        [HttpGet("log")]
        public IActionResult TestLog()
        {
            Log.Information("Test log endpoint hit at {Time}", DateTime.UtcNow);
            Log.Warning("This is a test warning message");
            Log.Error("This is a test error log");
            return Ok(new { message = "Serilog logs written to console and file" });
        }

        // Test Fake Data Generation
        [HttpGet("faker")]
        public IActionResult GenerateFakeUsers([FromQuery] int count = 5)
        {
            var faker = new Faker<User>()
                .RuleFor(u => u.Name, f => f.Name.FullName())
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.Role, f => f.Name.JobTitle())
                .RuleFor(u => u.CreatedAt, f => f.Date.Recent());

            var users = faker.Generate(count);

            Log.Information("Generated {Count} fake users for testing", count);

            return Ok(users);
        }

        // Test direct Mongo insert (no RabbitMQ)
        [HttpPost("insert-direct")]
        public async Task<IActionResult> InsertDirect([FromQuery] int count = 10)
        {
            var faker = new Faker<User>()
                .RuleFor(u => u.Name, f => f.Name.FullName())
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.Role, f => f.Name.JobTitle());

            var users = faker.Generate(count);

            await _userRepo.BulkInsertAsync(users);

            Log.Information("Inserted {Count} users directly into MongoDB", count);

            return Ok(new { message = $"Inserted {count} users directly into MongoDB" });
        }

        // Test RabbitMQ Producer (publish message)
        //[HttpPost("rabbitmq/send")]
        //public IActionResult SendToRabbit([FromQuery] int count = 100)
        //{
        //    // Generate fake users
        //    var faker = new Faker<User>()
        //        .RuleFor(u => u.Name, f => f.Name.FullName())
        //        .RuleFor(u => u.Email, f => f.Internet.Email())
        //        .RuleFor(u => u.Role, f => f.Name.JobTitle())
        //        .RuleFor(u => u.CreatedAt, f => f.Date.Recent());

        //    var users = faker.Generate(count);

        //    using var connection = _rabbitFactory.CreateConnection();
        //    using var channel = connection.CreateModel();

        //    channel.QueueDeclare(
        //        queue: "bulk_user_insert",
        //        durable: false,
        //        exclusive: false,
        //        autoDelete: false,
        //        arguments: null
        //    );

        //    var message = JsonConvert.SerializeObject(users);
        //    var body = Encoding.UTF8.GetBytes(message);

        //    channel.BasicPublish(
        //        exchange: "",
        //        routingKey: "bulk_user_insert",
        //        basicProperties: null,
        //        body: body
        //    );

        //    Log.Information("Published {Count} fake users to RabbitMQ queue", count);

        //    return Ok(new { message = $"{count} users published to RabbitMQ queue 'bulk_user_insert'" });
        //}

        // Test Error Handling (simulate exception)
        [HttpGet("error")]
        public IActionResult SimulateError()
        {
            try
            {
                throw new InvalidOperationException("Simulated test exception for error handling.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An intentional error occurred during testing");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            // Country data test
            var countriesData = CountryData.Get();

            var response = new List<CountryDto>();

            foreach (var country in countriesData)
            {
                response.Add(new CountryDto
                {
                    Id = country.Id,
                    Name = country.Name
                });
            }


            var users = UserData.Get();

            var userRes = new List<UserDto>();


            foreach (var user in users)
            {

                userRes.Add(new UserDto { Id = user.Id, Name = user.Name, Email = user.Email });

            }


            return Ok(userRes);
        }
    }
}
