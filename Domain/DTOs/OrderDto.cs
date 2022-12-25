using Domain.Entities;

namespace Domain.DTOs;

public class OrderDto
{
    public string Name { get; set; }
    public int OrderId { get; set; }
    public OrderState OrderState { get; set; }
    public IEnumerable<PizzaDto>? Pizzas { get; set; }
}