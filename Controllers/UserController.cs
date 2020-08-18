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
using SendGrid;
using SendGrid.Helpers.Mail;


namespace DotNetCoreSqlDb.Controllers
{
    public class UserController : Controller
    {
        private readonly MyDatabaseContext _context;
        private const string SENDGRID_KEY = "";
        private const string USER_URL = "https://user-demo.azurewebsites.net/User";
        private const int MINIMUM_PASSWORD_LENGTH = 8;

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
        public async Task<IActionResult> Login()
        {
            using StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8);
            dynamic parsedJson = JObject.Parse(await reader.ReadToEndAsync());
            Member loginUser = GetUser((string) parsedJson.email);
            if (loginUser == null) {
                return BadRequest("No user found matching " + parsedJson.email);
            }
            if (loginUser.Password != Member.getHash((string) parsedJson.password)) {
               return BadRequest("Password mismatch for " + loginUser.Email);        
            }
            MemberToken token = await CreateTokenForUser(loginUser.ID);
            return Ok(token);
        }

        [HttpGet]
        public IActionResult ProtectedPage()
        {
            string auth = Request.Headers["Authorization"];
            if (auth == null) {
                return BadRequest("Not authorized");
            }
            string[] authParts = auth.Split(' ');
            if (authParts[0] != "Bearer") {
                return BadRequest("Wrong type of Authorization");
            }
            var results = _context.MemberToken.Where(t => t.TokenHash == authParts[1])
                .ToList();
            if (results.Count == 0) {
                return BadRequest("Bad token");
            }
            return View();
        }

        public async Task<IActionResult> ForgotPassword()
        {
            using StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8);
            dynamic parsedJson = JObject.Parse(await reader.ReadToEndAsync());
            Member loginUser = GetUser((string) parsedJson.email);
            if (loginUser == null) {
                return BadRequest("No user found matching " + parsedJson.email);
            }
            var token = await SendLostPasswordEmail(loginUser);
            return Ok(token);
        }

        public IActionResult ResetPassword(int id, string code)
        {
            return View(new ResetPassModel { TokenId = id, ResetCode = code });;
        }

        public async Task<IActionResult> ChangePassword()
        {
            using StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8);
            dynamic parsedJson = JObject.Parse(await reader.ReadToEndAsync());
            int tokenId = Int32.Parse(s: (string) parsedJson.tokenId);
            string resetCode = parsedJson.resetCode;
            string password = parsedJson.password;
            if (password.Length < MINIMUM_PASSWORD_LENGTH) {
                return BadRequest("Password too short");
            }
            MemberToken token = _context.MemberToken.Where(t => t.ID == tokenId)
                .First();
            if (token==null) {
                throw new ArgumentException("Bad Token");
            }
            Member owner = _context.Member.Where(u => u.ID == token.MemberID)
                .First();
            if (resetCode != Member.getHash(token.TokenHash + owner.Password)) {
                throw new ArgumentException("Code Mismatch");
            }
            owner.setPassword(password);
            _context.Update(owner);
            await _context.SaveChangesAsync();
            return Ok("Changed");
        }

        private async Task<Response> SendLostPasswordEmail(Member loginUser)
        {
            MemberToken token = await CreateTokenForUser(loginUser.ID);
            String resetCode = Member.getHash(token.TokenHash + loginUser.Password);
            var resetUrl = USER_URL + "/ResetPassword/" + token.ID + "?code=" + resetCode;
            var client = new SendGridClient(SENDGRID_KEY);
            var from = new EmailAddress("samguyjones@gmail.com", "Do not reply");
            var subject = "Forgot Password";
            var to = new EmailAddress(loginUser.Email, "Registered User");
            var plainText = "Please visit " + resetUrl + " to reset your password";
            var htmlContent = "Visit <a href=\"" + resetUrl + "\">" + resetUrl
                + "</a> to reset your password.";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainText, htmlContent);
            return await client.SendEmailAsync(msg);
            // return token;
        }

        private async Task<MemberToken> CreateTokenForUser(int userId)
        {
            MemberToken token = new MemberToken();
            token.MemberID=userId;
            token.setRandomHash();
            _context.Add(token);
            await _context.SaveChangesAsync();
            return token;
        }

        private Member GetUser(string email)
        {
            var results = _context.Member.Where(u => 
                (u.Email == email))
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
            if (password.Length < MINIMUM_PASSWORD_LENGTH)
            {
                throw new ArgumentException("Password needs to be at least eight characters.");
            }
        }
    }
}
