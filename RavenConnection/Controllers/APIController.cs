using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Raven.Client.Documents.Session;
using RavenConnection.Database;
using RavenConnection.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

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
        public IActionResult GetAllUsersWithoutPagination()
        {
            using IDocumentSession session = _db.Store.OpenSession();
            var users = session.Advanced.LoadStartingWith<User>("Users/");
            return Ok(users);
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> PopulateDatabase()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new System.Uri("https://my.api.mockaroo.com/Users?key=69bb7b40");
            client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            var users = await client.GetFromJsonAsync<IEnumerable<User>>(client.BaseAddress);

            using IDocumentSession session = _db.Store.OpenSession();
            foreach (var user in users) session.Store(user);
            session.SaveChanges();

            return Ok(users);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult GetAllUsersWithPagination(int page = 1, int pageSize = 10)
        {
            using IDocumentSession session = _db.Store.OpenSession();
            var users = session
                .Query<User>()
                .Statistics(out QueryStatistics stats)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new { users = users, stats = stats });
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult GetAllUsersWithName(string name, int page = 1, int pageSize = 10)
        {
            try{
            using (IDocumentSession session = _db.Store.OpenSession())
            {
                var users = session
                                  .Query<User>()
                                  .Statistics(out QueryStatistics stats)
                                  .Where<User>(x => x.FirstName == name)
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToList();
                return Ok(new { users = users, stats = stats });
            }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult GetUserById(string id)
        {
            using IDocumentSession session = _db.Store.OpenSession();
            var user = session.Load<User>(id);
            if (user is null) return NotFound(new { error = $"user with Id {id} does not exist!" });
            return Ok(user);
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult CreateUser(User usr)
        {
            using IDocumentSession session = _db.Store.OpenSession();
            session.Store(usr);
            session.SaveChanges();
            return Ok(new { User = usr, message = "User stored successfully!" });
        }

        [HttpPut]
        [Route("[action]")]
        public IActionResult UpdateUser(User usr)
        {
            using IDocumentSession session = _db.Store.OpenSession();
            var usrFromDb = session.Load<User>(usr.Id);
            if (usrFromDb is null) return NotFound(new { error = $"User with Id {usr.Id} not found!" });
            usrFromDb.Address = usr.Address;
            usrFromDb.City = usr.City;
            usrFromDb.Country = usr.Country;
            usrFromDb.FirstName = usr.FirstName;
            usrFromDb.LastName = usr.LastName;
            usrFromDb.Zip = usr.Zip;
            session.SaveChanges();
            return Ok(new { User = usrFromDb, message = "User updated successfully!" });
        }

    }
}