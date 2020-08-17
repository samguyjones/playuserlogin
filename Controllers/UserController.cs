using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Models;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;

namespace DotNetCoreSqlDb.Controllers
{
    public class UserController : Controller
    {
        private readonly MyDatabaseContext _context;

        public UserController(MyDatabaseContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create()
        {
            using StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8);
            dynamic parsedJson = JObject.Parse(await reader.ReadToEndAsync());
            try {
                ValidateUser(parsedJson);
            } catch (ArgumentException ae) {
                return BadRequest(ae.Message);
            }
            Member myMember = new Member();
            myMember.FirstName = parsedJson.firstName;
            myMember.LastName = parsedJson.lastName;
            myMember.Email = parsedJson.email;
            myMember.setPassword((string) parsedJson.password);
            _context.Add(myMember);
            await _context.SaveChangesAsync();
            return Ok("Created");
        }
        

        [HttpPost]
        public async Task<IActionResult> Login() {
            using StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8);
            dynamic parsedJson = JObject.Parse(await reader.ReadToEndAsync());
            Member loginUser = GetUser((string) parsedJson.email, (string) parsedJson.password);
            if (loginUser == null) {
                return BadRequest("No user found matching " + Member.getHash((string) parsedJson.password));
            }
            MemberToken token = new MemberToken();
            token.MemberID=loginUser.ID;
            token.setRandomHash();
            _context.Add(token);
            await _context.SaveChangesAsync();
            return Ok(token.TokenHash);
        }

        private Member GetUser(string email, string password)
        {
            var results = _context.Member.Where(u => 
                    (u.Email == email) && (u.Password == Member.getHash(password)))
                .ToList();
            if (results.Count == 0) {
                return null;
            }
            return results.First();
        }


        private void ValidateUser(dynamic parsedJson) {
            string email = parsedJson.email;
            string password = parsedJson.password;
            var results = _context.Member
                .Where(u => u.Email == email)
                .ToList();
            if (results.Count > 0)
            {
                throw new ArgumentException("User already exists with email " + email);
            }
            if (password.Length < 8)
            {
                throw new ArgumentException("Password needs to be at least eight characters.");
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Member
                .FirstOrDefaultAsync(m => m.ID == id);
            if (member == null)
            {
                return NotFound();
            }

            return Ok(member);
        }
    }
}
