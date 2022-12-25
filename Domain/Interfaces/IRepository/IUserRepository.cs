using Domain.Entities;

namespace Domain.Interfaces.IRepository;

public interface IUserRepository
{
    Task<IQueryable<User>> GetUsers();
    Task<User> GetUserByName(string name);
}
