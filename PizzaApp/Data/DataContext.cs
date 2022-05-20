﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using PizzaApp.Entities;

namespace PizzaApp.Data;

public class DataContext : IdentityDbContext<
    User, Role, int,
    IdentityUserClaim<int>, UserRole,
    IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
{
    public DataContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Photo> Photos { get; set; }
    public DbSet<Pizza> Pizzas { get; set; }
    
    public DbSet<Toping> Topings { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasMany(ur => ur.UserRoles)
            .WithOne(u => u.User)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();

        modelBuilder.Entity<Role>()
            .HasMany(ur => ur.UserRoles)
            .WithOne(u => u.Role)
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired();

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Pizzas)
            .WithOne(p => p.Order)
            .HasForeignKey(p => p.OrderId)
            .IsRequired();
        modelBuilder.Entity<PizzaOrder>()
            .HasMany(p => p.Topings)
            .WithOne(t => t.PizzaOrder)
            .HasForeignKey(t => t.PizzaOrderId);
    }
}