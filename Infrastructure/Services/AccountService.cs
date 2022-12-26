using System.Data;
using System.Transactions;
using Dapper;
using Domain.DTOs;
using Domain.Entities;
using Domain.Interfaces.IServices;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly DataContext _context;
    private const string Connection = "Host=localhost;Database=app;Username=postgres;Password=452828qwe";

    public AccountService(UserManager<User> userManager, SignInManager<User> signInManager,
        ITokenService tokenService, DataContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _context = context;
    }

    public async Task<UserDto> Register(RegisterDto registerDto)
    {
        using IDbConnection db = new NpgsqlConnection(Connection);

        if (await UserExists(registerDto.Email)) throw new UnauthorizedAccessException("Email is already in use");

        var user = new User()
        {
            Email = registerDto.Email,
            PhoneNumber = registerDto.PhoneNumber,
            UserName = registerDto.Name,
            Adress = registerDto.Adress
        };

        user.Email = registerDto.Email.ToLower();

        
            //var result = await _userManager.CreateAsync(user, registerDto.Password);

          var userId =  db.QuerySingle<int>("""
         insert into "AspNetUsers"
         values (DEFAULT,@adress, @userName, @nuserName, @email,'123', false, @password, 'qwe', 'qwe', @phoneNumber, false, false,null, true, 0)
         returning "Id"
         """,
                new
                {
                    adress = user.Adress,
                    userName = user.UserName,
                    email = user.Email,
                    nuserName = user.UserName.ToUpper(),
                    password = registerDto.Password,
                    phoneNumber = user.PhoneNumber
                });

           // if (!result.Succeeded) throw new ApplicationException(result.Errors.ToString());

           // var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
            db.Query<UserRole>("""
             insert into "AspNetUserRoles"
             values(@id, @roleId)
             """
            ,new
            {
                id = userId,
                roleId = 1
            });
           // if (!roleResult.Succeeded) throw new ApplicationException(result.Errors.ToString());
        

        //_context.SaveChangesAsync();
        return new UserDto()
        {
            Id = user.Id,
            Email = registerDto.Email,
            PhoneNumber = registerDto.PhoneNumber,
            Name = registerDto.Name,
            Adress = registerDto.Adress,
            Token = await _tokenService.CreateToken(user)
        };
    }

    public async Task<UserDto> Login(LoginDto loginDto)
    {
        
        using IDbConnection db = new NpgsqlConnection(Connection);

        var user = db.Query<User>("""
         select * from "AspNetUsers" as users
         where users."Email" ilike @email and users."PasswordHash" = @pass
         """
        , new
        {
         email = loginDto.Email,
         pass = loginDto.Password
        }).First();

        if (user.Id <= 0) throw new UnauthorizedAccessException();
        // var user = await _userManager.Users
        //     .FirstOrDefaultAsync(x => x.Email.Equals(loginDto.Email.ToLower()));
        //
        //
        //
        // if (user == null) throw new UnauthorizedAccessException("Email doesn't exist");
        //
        // var result = await _signInManager
        //     .CheckPasswordSignInAsync(user, loginDto.Password, false);
        //
        // if (!result.Succeeded) throw new UnauthorizedAccessException();

        return new UserDto()
        {
            Id = user.Id,
            Name = user.UserName,
            Token = await _tokenService.CreateToken(user),
        };
    }

    private async Task<bool> UserExists(string username)
    {
        var user = _context.Users.FromSqlInterpolated(
            $"""select * from "AspNetUsers" where "Email" = { username.ToLower()}   """ );

        return user.Any();
    }
}