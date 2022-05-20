﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaApp.DTOs;
using PizzaApp.Interfaces;

namespace PizzaApp.Data;

public class TopingRepository : ITopingRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public TopingRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ActionResult<IEnumerable<TopingDto>>> GetTopings()
    {
        return await _mapper.ProjectTo<TopingDto>(_context.Topings).ToListAsync();
    }
}