using AutoMapper;
using Domain.Interfaces.IRepository;

namespace Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    
    public UnitOfWork(DataContext context, IMapper mapper,
        IUserRepository userRepository,
        ITopingRepository topingRepository,
        IPizzaRepository pizzaRepository,
        IOrderRepository orderRepository)
    {
        _context = context;
        _mapper = mapper;
        UserRepository = userRepository;
        PizzaRepository = pizzaRepository;
        TopingRepository = topingRepository;
        OrderRepository = orderRepository;
    }

    public IUserRepository UserRepository { get; } 
    public IPizzaRepository PizzaRepository { get; } 
    public ITopingRepository TopingRepository { get; }
    public IOrderRepository OrderRepository { get; }
    
    public async Task<bool> Complete()
    {
        return await _context.SaveChangesAsync() > 0;
    }

}