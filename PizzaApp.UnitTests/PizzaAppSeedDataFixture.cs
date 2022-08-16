using System;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PizzaApp.UnitTests;

public class PizzaAppSeedDataFixture : IDisposable
{
    public DataContext DataContext { get; set; }

    public PizzaAppSeedDataFixture()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "PizzaAppDatabase")
            .Options;
        DataContext = new DataContext(options);
    }

    public void Dispose()
    {
        this.DataContext.Dispose();
    }
}