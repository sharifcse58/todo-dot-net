using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MyApiProject.Models;
using Serilog;

namespace MyApiProject.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(MongoDbContext ctx)
    {
        _users = ctx.Users;
    }

    public async Task<IEnumerable<User>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var skip = (page - 1) * pageSize;

        var users = await _users.Find(_ => true)
            .SortByDescending(u => u.CreatedAt)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync();

        return users;
    }


    public async Task<User?> GetByIdAsync(string id) =>
        await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

    public async Task<User> CreateAsync(User user)
    {
        await _users.InsertOneAsync(user);
        return user;
    }

    public async Task<bool> UpdateAsync(string id, User user)
    {
        user.Id = id;
        var result = await _users.ReplaceOneAsync(u => u.Id == id, user);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _users.DeleteOneAsync(u => u.Id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }

    //public async Task BulkInsertAsync(IEnumerable<User> users)
    //{
    //    if (users == null || !users.Any()) return;

    //    // Use MongoDB bulk insert for efficiency
    //    await _users.InsertManyAsync(users, new InsertManyOptions
    //    {
    //        IsOrdered = false // continue on errors
    //    });
    //}



    //public async Task BulkInsertAsync(IEnumerable<User> users)
    //{
    //    if (users == null || !users.Any()) return;

    //    await _seederLock.WaitAsync(); // ensures one bulk insert runs at a time
    //    try
    //    {
    //        const int batchSize = 2000;
    //        var userList = users.ToList();

    //        for (int i = 0; i < userList.Count; i += batchSize)
    //        {
    //            var batch = userList.Skip(i).Take(batchSize).ToList();
    //            await _users.InsertManyAsync(batch, new InsertManyOptions { IsOrdered = false });
    //        }
    //    }
    //    finally
    //    {
    //        _seederLock.Release();
    //    }
    //}

    private static readonly SemaphoreSlim _seederLock = new SemaphoreSlim(1, 1);

    public async Task BulkInsertAsync(IEnumerable<User> users)
    {
        if (users == null || !users.Any()) return;

        await _seederLock.WaitAsync(); // ensures one bulk insert runs at a time

        // Log the start of the bulk insert
        Log.Information("Starting bulk insert of {UserCount} users", users.Count());

        try
        {
            // Insert users in bulk, using MongoDB's InsertManyAsync method
            await _users.InsertManyAsync(users, new InsertManyOptions { IsOrdered = false });

            // Log the successful completion of the bulk insert
            Log.Information("Successfully inserted {UserCount} users", users.Count());
        }
        catch (Exception ex)
        {
            // Log any errors that occur during the insert
            Log.Error(ex, "Error occurred while inserting users");
            throw;
        }finally
        {
            _seederLock.Release();
        }
    }

    public async Task BulkInsertLoopAsync(IEnumerable<User> users)
    {
        if (users == null || !users.Any()) return;

       // await _seederLock.WaitAsync(); // ensures one insert process runs at a time

        Log.Information("Starting single insert loop for {UserCount} users", users.Count());

        try
        {
            int successCount = 0;
            int failCount = 0;

            foreach (var user in users)
            {
                try
                {
                    // Insert one user at a time
                    await _users.InsertOneAsync(user);
                    successCount++;
                }
                catch (Exception ex)
                {
                    failCount++;
                    Log.Warning(ex, "Failed to insert user with Email: {Email}", user.Email);
                }
            }

            Log.Information("Inserted {SuccessCount} users successfully. {FailCount} failed.",
                successCount, failCount);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error occurred during user insertion loop");
            throw;
        }
        finally
        {
           // _seederLock.Release();
        }
    }



    public async Task<long> GetUserCountAsync()
    {
        return await _users.CountDocumentsAsync(_ => true); // count all documents
    }


    public async Task TruncateUsersAsync()
    {
        await _users.DeleteManyAsync(_ => true); // delete all users
    }




}
