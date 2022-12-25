using AutoMapper;
using Domain.Entities;
using Domain.Interfaces.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class UserRepository : IUserRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public UserRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task<IQueryable<User>> GetUsers()
    {
        return _context.Users.FromSqlInterpolated($"""select * from "AspNetUsers" """);
    }

    public async Task<User> GetUserByName(string name)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.UserName == name);
    }
}