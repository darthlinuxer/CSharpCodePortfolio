using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents.Session;
using RavenConnection.Database;
using RavenConnection.Models;

namespace RavenConnection.API
{
    [ApiController]
    [Route("[controller]")]
    public class APIController : ControllerBase
    {
        private readonly IDocumentStoreHolder _db;

        public APIController(IDocumentStoreHolder store)
        {
            _db = store;
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult GetAllUsers()
        {
            using IDocumentSession session = _db.Store.OpenSession();
            var users = session.Advanced.LoadStartingWith<User>("Users/");
            return Ok(users);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult GetUserById(string id)
        {
            using IDocumentSession session = _db.Store.OpenSession();
            var user = session.Load<User>(id);
            if (user is null) return NotFound(new { error= $"user with Id {id} does not exist!"});            
            return Ok(user);
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult CreateUser(User usr)
        {
            using IDocumentSession session = _db.Store.OpenSession();
            session.Store(usr);
            session.SaveChanges();
            return Ok(new {User = usr , message = "User stored successfully!"});
        }

        [HttpPut]
        [Route("[action]")]
        public IActionResult UpdateUser(User usr)
        {
            using IDocumentSession session = _db.Store.OpenSession();
            var usrFromDb = session.Load<User>(usr.Id);
            if (usrFromDb is null) return NotFound(new {error = $"User with Id {usr.Id} not found!"});
            usrFromDb.Address = usr.Address;
            usrFromDb.City = usr.City;
            usrFromDb.Country = usr.Country;
            usrFromDb.FirstName = usr.FirstName;
            usrFromDb.LastName = usr.LastName;
            usrFromDb.Zip = usr.Zip;
            session.Store(usrFromDb);
            session.SaveChanges();            
            return Ok(new {User = usrFromDb , message = "User updated successfully!"});
        }

    }
}