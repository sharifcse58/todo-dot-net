using MyApiProject.Models;

namespace MyApiProject.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(string id);
    Task<User> CreateAsync(User user);
    Task<bool> UpdateAsync(string id, User user);
    Task<bool> DeleteAsync(string id);
}
