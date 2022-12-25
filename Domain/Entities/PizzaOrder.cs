namespace Domain.Entities;


public class PizzaOrder
{
    public int Id { get; set; }
    public Pizza Pizza { get; set; }
    public int PizzaId { get; set; }
    public IList<TopingOrder> Topings { get; set; }
    
    public int OrderId { get; set; }
    public Order Order { get; set; }
    
    public State State { get; set; }
    
}