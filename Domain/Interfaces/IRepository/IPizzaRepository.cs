using Domain.DTOs;
using Domain.Entities;

namespace Domain.Interfaces.IRepository;

public interface IPizzaRepository
{
    Task<IQueryable<Pizza>> GetPizzas();
    Task<Pizza> GetPizza(string name);
    Task<Pizza> AddPizza(Pizza pizza, Photo photo);//should be changed
    Task<Pizza> UpdatePizza(Pizza pizza);
    Task<PizzaDto> UpdatePizzaOrderState(int pizzaId, int state);
    Task<IQueryable<PizzaOrder>> GetPizzasByOrderId(int orderId);

    Task<Pizza> GetPizzaByName(string pizzaName);

    Pizza GetPizzaById(int id);

    Task<PizzaOrder> GetPizzaOrderById(int pizzaOrderId);
}