using Microsoft.AspNetCore.Mvc;
using System;
using System.Reflection.Metadata.Ecma335;
using UserManagementApi.Models;

namespace UserManagementApi.Controllers {

    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase {
        private static readonly List<User> users = [
            new() { Id = 1, Name = "TestName1"},
            new() { Id = 2, Name = "TestName2"}
        ];

        [HttpGet]
        public IActionResult GetUsers() {
            return Ok(users);
        }

        [HttpGet("{id}")]
        public IActionResult GetUser(int id) {
            var user = users.FirstOrDefault(u => u.Id == id);
            if (user == null) {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost]
        public IActionResult CreateUser(User newUser) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            newUser.Id = users.Count + 1;
            users.Add(newUser);
            return CreatedAtAction(nameof(GetUser), new { id = newUser.Id }, newUser);

        }

        [HttpPut("{id}")]
        public ActionResult UpdateUser(int id, User updatedUser) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            var user = users.FirstOrDefault(i => i.Id == id);
            if (user == null) {
                return NotFound();
            }
            user.Name = updatedUser.Name;
            return Ok(user);
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteUser(int id) {
            var user = users.FirstOrDefault(i => i.Id == id);
            if (user == null) {
                return NotFound();
            }
            users.Remove(user);
            return NoContent();
        }
    }
}