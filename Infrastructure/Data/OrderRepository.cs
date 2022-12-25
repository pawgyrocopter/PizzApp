using System.Collections;
using System.Data;
using System.Transactions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Dapper;
using Domain.DTOs;
using Domain.Entities;
using Domain.Interfaces.IRepository;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infrastructure.Data;

public class OrderRepository : IOrderRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private const string Connection = "Host=localhost;Database=app;Username=postgres;Password=452828qwe";

    public OrderRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<OrderDto> CreateOrder(Order order)
    {
        // var order = new Order();
        // var users = await _context.Users.ToListAsync();
        // order.User = await _context.Users.FirstOrDefaultAsync(x => x.UserName.Equals(orderDto.Name));
        // var pizzas = new List<PizzaOrder>();
        // foreach (var pizza in orderDto.Pizzas)
        // {
        //     var pOrder = new PizzaOrder();
        //     pOrder.Pizza = await _context.Pizzas.FirstOrDefaultAsync(x => x.Name == pizza.Name);
        //     pOrder.Topings = pizza.Topings
        //         .Select(x => new TopingOrder()
        //         {
        //             Toping = _context.Topings.FirstOrDefaultAsync(t => t.Id == x.Id).Result,
        //             Counter = x.Counter
        //         }).ToList();
        //     pOrder.Pizza.Cost = pizza.Cost;
        //     pOrder.State = State.Pending;
        //     pizzas.Add(pOrder);
        // }
        //
        // order.Pizzas = pizzas;
        using IDbConnection db = new NpgsqlConnection(Connection);
        using var transaction = new TransactionScope();
        try
        {
            var orderId = db.QuerySingle<int>(""" 
                insert into "Orders" 
                values (DEFAULT, @userId , @orderState ) 
                returning "Id"
                """
                , new {userId = order.User.Id, orderState = order.OrderState});

            foreach (var pizzaOrder in order.Pizzas)
            {
                var pizzaId = db.QuerySingle<int>(""" 
                insert into "PizzaOrders" 
                values  (DEFAULT, @pizzaId,@orderId, @pizzaOrderState)
                returning "Id"
                """
                    , new {pizzaId = pizzaOrder.Pizza.Id, orderId = orderId, pizzaOrderState = pizzaOrder.State});

                foreach (var topingOrder in pizzaOrder.Topings)
                {
                    _context.Database.ExecuteSqlInterpolated($""" 
                insert into "TopingOrders" 
                values  (DEFAULT, { topingOrder.Toping.Id} ,{ topingOrder.Counter} , { pizzaId} )
                """ );
                }
            }

            transaction.Complete();
        }
        catch
        {
        }

        //await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
        return _mapper.Map<OrderDto>(order);
    }

    public async Task<IQueryable<Order>> GetOrders()
    {
        using IDbConnection db = new NpgsqlConnection(Connection);
        var ordersDict = new Dictionary<int, Order>();
        db.Query<Order, User, PizzaOrder, Pizza,Photo, Order>(""" 
            select * from "Orders" as orders
            join (select * from "AspNetUsers") as u on u."Id" = orders."UserId"
            join (select * from "PizzaOrders") as po on po."OrderId" = orders."Id"
            join (select * from "Pizzas") as pi on pi."Id" = po."PizzaId"
            join (select * from "Photos") as ph on ph."Id" = pi."PhotoId"
        """
            , (order, user,pizzaOrder, pizza, photo) =>
            {
                if (!ordersDict.TryGetValue(order.Id, out var orderEntity))
                {
                    ordersDict.Add(order.Id, orderEntity = order);
                }
                orderEntity.User = user;
                
                if(orderEntity.Pizzas == null || orderEntity.Pizzas.Count == 0)
                {
                    orderEntity.Pizzas = new List<PizzaOrder>();
                }
                
                if (pizzaOrder != null)
                {
                    if (!orderEntity.Pizzas.Any(x => x.Id == pizzaOrder.Id))
                    {
                        pizzaOrder.Pizza = new Pizza();
                        if (pizza != null) pizzaOrder.Pizza = pizza;
                        pizzaOrder.Pizza.Photo = photo;
                        orderEntity.Pizzas.Add(pizzaOrder);
                    }
                }
                
                return orderEntity;
            });
        var orders1 = ordersDict.Select(x => x.Value).AsQueryable();
        // var orders2 =_context.Orders
        //     .Include(x => x.User)
        //     .Include(x => x.Pizzas)
        //     .ThenInclude(x => x.Pizza).AsQueryable();
        return orders1;
    }

    public async Task<IQueryable<Order>> GerUserOrders(string name)
    {
        using IDbConnection db = new NpgsqlConnection(Connection);
        var ordersDict = new Dictionary<int, Order>();
        db.Query<Order, User, PizzaOrder, Pizza,Photo, Order>(""" 
            select * from "Orders" as orders
            join (select * from "AspNetUsers") as u on u."Id" = orders."UserId"
            join (select * from "PizzaOrders") as po on po."OrderId" = orders."Id"
            join (select * from "Pizzas") as pi on pi."Id" = po."PizzaId"
            join (select * from "Photos") as ph on ph."Id" = pi."PhotoId"
            where u."UserName" = @name
        """
            , (order, user,pizzaOrder, pizza, photo) =>
            {
                if (!ordersDict.TryGetValue(order.Id, out var orderEntity))
                {
                    ordersDict.Add(order.Id, orderEntity = order);
                }
                orderEntity.User = user;
                
                if(orderEntity.Pizzas == null || orderEntity.Pizzas.Count == 0)
                {
                    orderEntity.Pizzas = new List<PizzaOrder>();
                }
                
                if (pizzaOrder != null)
                {
                    if (!orderEntity.Pizzas.Any(x => x.Id == pizzaOrder.Id))
                    {
                        pizzaOrder.Pizza = new Pizza();
                        if (pizza != null) pizzaOrder.Pizza = pizza;
                        pizzaOrder.Pizza.Photo = photo;
                        orderEntity.Pizzas.Add(pizzaOrder);
                    }
                }
                
                return orderEntity;
            }, new {name = name});
        var orders1 = ordersDict.Select(x => x.Value).AsQueryable();
        return orders1;
        return _context.Orders
            .Include(x => x.User)
            .Include(x => x.Pizzas)
            .ThenInclude(x => x.Pizza)
            .Where(x => x.User.UserName.Equals(name));
    }

    public async Task<OrderDto> GetOrderById(int orderId)
    {
        using IDbConnection db = new NpgsqlConnection(Connection);
        var ordersDict = new Dictionary<int, Order>();
        db.Query<Order, User, PizzaOrder, Pizza,Photo, Order>(""" 
            select * from "Orders" as orders
            join (select * from "AspNetUsers") as u on u."Id" = orders."UserId"
            join (select * from "PizzaOrders") as po on po."OrderId" = orders."Id"
            join (select * from "Pizzas") as pi on pi."Id" = po."PizzaId"
            join (select * from "Photos") as ph on ph."Id" = pi."PhotoId"
            where orders."Id" = @orderId
        """
            , (order, user,pizzaOrder, pizza, photo) =>
            {
                if (!ordersDict.TryGetValue(order.Id, out var orderEntity))
                {
                    ordersDict.Add(order.Id, orderEntity = order);
                }
                orderEntity.User = user;
                
                if(orderEntity.Pizzas == null || orderEntity.Pizzas.Count == 0)
                {
                    orderEntity.Pizzas = new List<PizzaOrder>();
                }
                
                if (pizzaOrder != null)
                {
                    if (!orderEntity.Pizzas.Any(x => x.Id == pizzaOrder.Id))
                    {
                        pizzaOrder.Pizza = new Pizza();
                        if (pizza != null) pizzaOrder.Pizza = pizza;
                        pizzaOrder.Pizza.Photo = photo;
                        orderEntity.Pizzas.Add(pizzaOrder);
                    }
                }
                
                return orderEntity;
            }, orderId);
        
        // var order = await _context.Orders
        //     .Include(x => x.User)
        //     .Include(x => x.Pizzas)
        //     .ThenInclude(x => x.Pizza)
        //     .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
        //     .FirstOrDefaultAsync(x => x.OrderId == orderId);
        return _mapper.Map<OrderDto>( ordersDict.Select(x => x.Value).First());
    }

    public async Task<Order> GetOrderByIdAsync(int orderId)
    {
        // return await _context.Orders
        //     .Include(x => x.User)
        //     .Include(x => x.Pizzas)
        //     .ThenInclude(x => x.Pizza)
        //     .FirstOrDefaultAsync(x => x.Id == orderId);
        using IDbConnection db = new NpgsqlConnection(Connection);
        var ordersDict = new Dictionary<int, Order>();
        db.Query<Order, User, PizzaOrder, Pizza,Photo,TopingOrder, Toping, Order>(""" 
            select * from "Orders" as orders
            join (select * from "AspNetUsers") as u on u."Id" = orders."UserId"
            join (select * from "PizzaOrders") as po on po."OrderId" = orders."Id"
            join (select * from "Pizzas") as pi on pi."Id" = po."PizzaId"
            join (select * from "Photos") as ph on ph."Id" = pi."PhotoId"
            left join (select * from "TopingOrders") as top on top."PizzaOrderId" = po."Id"
            left join (select * from "Topings") as tt on tt."Id" = top."TopingId"
            where orders."Id" = @orderId
        """
            , (order, user,pizzaOrder, pizza, photo, topingOrder, toping) =>
            {
                if (!ordersDict.TryGetValue(order.Id, out var orderEntity))
                {
                    ordersDict.Add(order.Id, orderEntity = order);
                }
                orderEntity.User = user;
                
                if(orderEntity.Pizzas == null || orderEntity.Pizzas.Count == 0)
                {
                    orderEntity.Pizzas = new List<PizzaOrder>();
                }
                
                if (pizzaOrder != null)
                {
                    if (!orderEntity.Pizzas.Any(x => x.Id == pizzaOrder.Id))
                    {
                        pizzaOrder.Pizza = new Pizza();
                        if (pizza != null) pizzaOrder.Pizza = pizza;
                        pizzaOrder.Pizza.Photo = photo;
                        orderEntity.Pizzas.Add(pizzaOrder);
                    }
                    if (topingOrder != null)
                    {
                        topingOrder.Toping = toping;
                       var q = orderEntity.Pizzas.FirstOrDefault(x => x.Id == topingOrder.PizzaOrderId);
                       if (q.Topings is null) q.Topings = new List<TopingOrder>();
                       q.Topings.Add(topingOrder);
                       var b = 1;
                    }
                }

               
                
                return orderEntity;
            },new {orderId = orderId});
        
        // var order = await _context.Orders
        //     .Include(x => x.User)
        //     .Include(x => x.Pizzas)
        //     .ThenInclude(x => x.Pizza)
        //     .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
        //     .FirstOrDefaultAsync(x => x.OrderId == orderId);
        return ordersDict.Select(x => x.Value).First();
    }

    public async Task<Order> GetOrderByPizzaId(int pizzaOrderId)
    {
        using IDbConnection db = new NpgsqlConnection(Connection);
        var ordersDict = new Dictionary<int, Order>();
        db.Query<Order,PizzaOrder, Order>(""" 
            select * from "Orders" as orders 
            join (select * from "PizzaOrders") as po on po."OrderId" = orders."Id" 
            where po."Id" = @id
        """
            , (order,pizzaOrder) =>
            {
                if (!ordersDict.TryGetValue(order.Id, out var orderEntity))
                {
                    ordersDict.Add(order.Id, orderEntity = order);
                }

                if(orderEntity.Pizzas == null || orderEntity.Pizzas.Count == 0)
                {
                    orderEntity.Pizzas = new List<PizzaOrder>();
                }
                
                if (pizzaOrder != null)
                {
                    if (!orderEntity.Pizzas.Any(x => x.Id == pizzaOrder.Id))
                    {
                        orderEntity.Pizzas.Add(pizzaOrder);
                    }
                }
                
                return orderEntity;
            }, new {id = pizzaOrderId});
        var orders1 = ordersDict.Select(x => x.Value).First();
        return orders1;
        
        return await _context.Orders
            .Include(p => p.Pizzas)
            .FirstOrDefaultAsync(x => x.Id == pizzaOrderId);
    }
}