using MyApiProject.Models;

namespace MyApiProject.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync(int page = 1, int pageSize = 10);
    Task<User?> GetByIdAsync(string id);
    Task<User> CreateAsync(User user);
    Task<bool> UpdateAsync(string id, User user);
    Task<bool> DeleteAsync(string id);
    Task<long> GetUserCountAsync();
    Task TruncateUsersAsync();

    // Bulk insert method to add multiple users at once
    Task BulkInsertAsync(IEnumerable<User> users);
    Task BulkInsertLoopAsync(IEnumerable<User> users);

    Task<IEnumerable<User>> SearchUsersAsync(
        IEnumerable<UserSearchFilter> filters,
        int page,
        int pageSize
    );



}
