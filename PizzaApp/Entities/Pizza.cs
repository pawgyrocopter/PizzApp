﻿namespace PizzaApp.Entities;

public class Pizza
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Ingredients { get; set; }
    public int Cost { get; set; }
    public int Weight { get; set; }
    public Photo Photo { get; set; }
}