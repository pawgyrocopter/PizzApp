using System.Data;
using System.Reflection.Metadata.Ecma335;
using Dapper;
using Domain.Entities;
using Domain.Interfaces.IRepository;
using Domain.Interfaces.IServices;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly DataContext _context;
    private const string Connection = "Host=localhost;Database=app;Username=postgres;Password=452828qwe";


    public AdminService(IUnitOfWork unitOfWork, UserManager<User> userManager, DataContext context)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _context = context;
    }

    public async Task<object> GetUsersWithRoles()
    {
        var userRoles = _context.UserRoles.FromSqlRaw(""" 
            select * from "AspNetUserRoles" as userroles 
            left join "AspNetRoles" as roles
            on userroles."RoleId" = roles."Id"
            """).ToList();
        var roles = _context.Roles.FromSqlRaw(""" select * from "AspNetRoles" as roles """).ToList();
        var users = _context.Users.FromSqlRaw(""" select * from "AspNetUsers" as users """).ToList();

        foreach (var role in roles)
        {
            foreach (var userRole in userRoles)
            {
                if (userRole.RoleId == role.Id)
                {
                    userRole.Role = new Role() {Name = role.Name};
                }
            }
        }

        foreach (var user in users)
        {
            foreach (var userRole in userRoles)
            {
                if (userRole.UserId == user.Id)
                {
                    user.UserRoles.Add(userRole);
                }
            }
        }
        return users.Select(x => new
        {
            Id = x.Id,
            Name = x.UserName,
            Roles = x.UserRoles.Select(x => x.Role.Name).ToList()
        });
       

        return userRoles;
    }

    public async Task<object> EditRoles(string userName, string roles)
    {
        using IDbConnection db = new NpgsqlConnection(Connection);
        
        var selectedRoles = roles.Split(',').ToArray();
        //var user = await _userManager.FindByNameAsync(userName);
        var user = db.Query<User>("""
         select * from "AspNetUsers" as users
         where users."UserName" = @userName
         limit 1
         """
        , new {userName}).First();
        if (user == null) throw new ApplicationException("No user");

        //var userRoles = await _userManager.GetRolesAsync(user);
        var userRoles = db.Query<string>("""
         select t."Name" from "AspNetUserRoles" as userRoles
         left join (select roles."Id",roles."Name" from "AspNetRoles" as roles) as t 
         on t."Id" = userRoles."RoleId" 
         where userRoles."UserId" = @id
         """
        , new {id = user.Id} );
        var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
        if (!result.Succeeded) throw new ApplicationException("Failed to add roles");

        result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
        if (!result.Succeeded) throw new ApplicationException("Failed to remove roles");

        return await _userManager.GetRolesAsync(user);
    }
}