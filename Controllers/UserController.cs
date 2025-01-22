using Microsoft.AspNetCore.Mvc;
using System;

[Route("[controller]")]
[ApiController]
public class UserController : ControllerBase {
    private static List<User> users = new List<User>();

    [HttpGet]
    public ActionResult<List<User>> GetAll() => users;

    [HttpGet("{id}")]
    public ActionResult<User> GetById(int id) {
        try {
            var user = users.FirstOrDefault(i => i.Id == id) ?? throw new KeyNotFoundException("This user does not exist.");
            return user;
        } catch (KeyNotFoundException ex) {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public ActionResult<User> Create(User newUser) {
        try {
            if (!ModelState.IsValid) throw new BadHttpRequestException("Data format is invalid."); 
            newUser.Id = users.Count + 1;
            users.Add(newUser);
            return CreatedAtAction(nameof(GetById), new { id = newUser.Id }, newUser);
        } catch (BadHttpRequestException ex) {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public ActionResult Update(int id, User updatedUser) {
        try {
            if (!ModelState.IsValid) throw new BadHttpRequestException("Data format is invalid.");
            var user = users.FirstOrDefault(i => i.Id == id) ?? throw new KeyNotFoundException();
            user.Name = updatedUser.Name;
            return Ok(user);
        } catch (KeyNotFoundException) {
            return BadRequest("This user does not exist.");
        } catch (BadHttpRequestException ex) {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(int id) {
        try {
            var user = users.FirstOrDefault(i => i.Id == id) ?? throw new KeyNotFoundException("This user does not exist.");
            users.Remove(user);
            return NoContent();
        } catch (KeyNotFoundException ex) {
            return BadRequest(ex.Message);
        }
    }
}