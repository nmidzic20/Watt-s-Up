﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models.Entities;
using backend.Models.Requests;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using backend.Services;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly UserService _userService;
        private readonly CardService _cardService;

        public UsersController(DatabaseContext context, UserService userService, CardService cardService)
        {
            _context = context;
            _userService = userService;
            _cardService = cardService;
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(UserCreateRequest userRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            User newUser = await _userService.CreateUserAsync(userRequest);
            if (newUser == null)
            {
                return Conflict(new { message = "A user with this email already exists." });
            }

            Card newCard = null;
            if(userRequest.Card != null)
            {
                newCard = await _cardService.CreateCardAsync(userRequest.Card, newUser);
                if (newCard == null)
                {
                    return Conflict(new { message = "This card already exists." });
                }
            }

            _context.User.Add(newUser);
            if(newCard != null)
            {
                _context.Card.Add(newCard);
            }
            await _context.SaveChangesAsync();

            return Ok(new { message = $"User {newUser.FirstName} is added to database!" });
        }
    }
}
