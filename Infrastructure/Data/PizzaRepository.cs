using System.Data;
using AutoMapper;
using Dapper;
using Domain.DTOs;
using Domain.Entities;
using Domain.Interfaces.IRepository;
using Domain.Interfaces.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql;

namespace Infrastructure.Data;

public class PizzaRepository : IPizzaRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IPhotoService _photoService;
    private const string Connection = "Host=localhost;Database=app;Username=postgres;Password=452828qwe";
    public PizzaRepository(DataContext context, IMapper mapper, IPhotoService photoService)
    {
        _context = context;
        _mapper = mapper;
        _photoService = photoService;
    }

    public async Task<IQueryable<Pizza>> GetPizzas()
    {
        using IDbConnection db = new NpgsqlConnection(Connection);
        var testPizzas = db.Query<Pizza, Photo, Pizza>("""
         select * from "Pizzas" as pizzas
         join 
         (select * from "Photos") as t on t."Id" = pizzas."PhotoId"
         """
        , (pizza, photo) => { pizza.Photo = photo;
            return pizza;
        }).AsQueryable();

        return testPizzas;
        
        return _context.Pizzas
            .Include(p => p.Photo);
    }


    public async Task<Pizza> GetPizza(string name)
    {
        // var pizza = await _context.Pizzas
        //     .Include(p => p.Photo)
        //     .FirstOrDefaultAsync(x => x.Name == name);
        // return pizza;
//         var photos = _context.Photos.FromSqlRaw("""select * from "Photos" """).ToList();
//         var pizza = _context.Pizzas.FromSqlInterpolated($"""
//              select * from "Pizzas" as pizzas
//              where pizzas."Name" = { name}   
//           """ );

        using IDbConnection db = new NpgsqlConnection(Connection);
        var pizza = db.Query<Pizza, Photo, Pizza>("""
              select * from "Pizzas" as pizzas  
              join (select * from "Photos" as photos) as t
             on t."Id" = pizzas."PhotoId" 
             where pizzas."Name" = @name
             """  
            ,
            (pizza, photo) => { pizza.Photo = photo;
                return pizza;
            }
            ,new {name} );
        
        return pizza.First();
    }

    public async Task<Pizza> GetPizzaByName(string pizzaName)
    {
        using IDbConnection db = new NpgsqlConnection(Connection);
        var pizza = db.Query<Pizza>("""
              select * from "Pizzas" as pizzas
             where pizzas."Name" = @name  
             """  
            ,new {name = pizzaName});
        
        return pizza.First();
        
        return _context.Pizzas
            .FromSqlInterpolated($""" select * from "Pizzas" as pizzas where pizzas."Name" = { pizzaName}    """ )
            .First();
        return await _context.Pizzas.FirstOrDefaultAsync(x => x.Name.Equals(pizzaName));
    }

    public Pizza GetPizzaById(int id)
    {
        using IDbConnection db = new NpgsqlConnection(Connection);
        var pizza = db.Query<Pizza>("""
              select * from "Pizzas" as pizzas
             where pizzas."Id" = @id  
             """  
            ,new {id = id});
        
        return pizza.First();
        return _context.Pizzas.FromSqlInterpolated($"""select * from "Pizzas" as pizzas where pizzas."Id"= { id}  """ )
            .First();
    }


    public async Task<Pizza> AddPizza(Pizza pizza, Photo photo)
    {
        using IDbConnection db = new NpgsqlConnection(Connection);
        await _context.Database.ExecuteSqlInterpolatedAsync($""" 
            insert into "Photos"
            values(DEFAULT, { photo.Url}  , { photo.PublicId}  )
""" );
        var a = _context.Photos
            .FromSqlInterpolated($"""select * from "Photos" as photos where photos."PublicId" = { photo.PublicId}  """ )
            .First().Id;
        await _context.Database.ExecuteSqlInterpolatedAsync($"""
        insert into "Pizzas"
        values(DEFAULT, { pizza.Name}  , { pizza.Ingredients}  , { pizza.Cost}  , { pizza.Weight} , { a}  , {
            pizza.State} )
        """ );

        return pizza;
    }

    public async Task<Pizza> UpdatePizza(Pizza pizza)
    {
        //_context.Pizzas.Update(pizza);
        _context.Database.ExecuteSqlInterpolated($"""
            update "Pizzas"
            set "Id" = "Id", "Name" = { pizza.Name} ,  "Ingredients" = { pizza.Ingredients} ,  "Cost" = { pizza.Cost}
            ,  "Weight" = { pizza.Weight} , "State" = { pizza.State}  
            where "Id" = { pizza.Id} 
            """ );
        return pizza;
    }

    public async Task<PizzaDto> UpdatePizzaOrderState(int pizzaId, int state)
    {
        var pizza = await _context.PizzaOrders.Include(x => x.Order).ThenInclude(d => d.Pizzas)
            .FirstOrDefaultAsync(x => x.Id == pizzaId);
        
        var order = await _context.Orders.Include(p => p.Pizzas).FirstOrDefaultAsync(x => x.Id == pizza.OrderId);
        if (pizza == null) return null;
        pizza.State = state switch
        {
            0 => State.Pending,
            1 => State.InProgress,
            2 => State.Ready,
            _ => State.Canceled
        };
        bool check = pizza.Order.Pizzas.All(x => x.State == State.Ready);
        if (check)
        {
            pizza.Order.OrderState = OrderState.Ready;
        }

        return _mapper.Map<PizzaDto>(pizza);
    }

    public async Task<PizzaOrder> GetPizzaOrderById(int pizzaOrderId)
    {
        return await _context.PizzaOrders.Include(x => x.Order).ThenInclude(d => d.Pizzas)
            .FirstOrDefaultAsync(x => x.Id == pizzaOrderId);
    }

    public async Task<IQueryable<PizzaOrder>> GetPizzasByOrderId(int orderId)
    {
        return _context.PizzaOrders.FromSqlInterpolated($"""
        select * from "PizzaOrders" as pizzas
        where pizzas."OrderId" = {orderId}
        """);
        return _context.PizzaOrders.Where(x => x.OrderId == orderId);
    }
}