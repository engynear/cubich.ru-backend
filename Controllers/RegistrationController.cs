using System.Security.Claims;
using System.Text;
using cubichi.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("")]
public class RegistrationController : ControllerBase
{
    private readonly NpgsqlConnection _connection;

    public RegistrationController()
    {
        string connectionString = $"Host=localhost;Username=cubich;Password=timoha;Database=cubich";
        _connection = new NpgsqlConnection(connectionString);
        _connection.Open();
    }

    ~RegistrationController()
    {
        _connection.Close();
    }

    [HttpPost, Route("register")]
    public IActionResult Register(UserReg user)
    {
        try
        {
            var command = new NpgsqlCommand($"SELECT 1 FROM \"users\" WHERE username = '{user.UserName}'", _connection);
            if (command.ExecuteScalar() != null)
                return Conflict("User already exists");

            var insertCommand = new NpgsqlCommand($"INSERT INTO \"users\"(username, password) VALUES ('{user.UserName}', encode(digest('{user.Password}', 'sha256'), 'hex'));", _connection);
            insertCommand.ExecuteNonQuery();
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Login method.
    [HttpPost, Route("login")]
    public ActionResult<string> Login(UserLoginDto user)
    {
        try
        {
            var command = new NpgsqlCommand($"SELECT 1 FROM \"users\" WHERE username = '{user.UserName}' AND password = encode(digest('{user.Password}', 'sha256'), 'hex')", _connection);
            if (command.ExecuteScalar() != null)
            {
                string token = CreateToken(user);
                return Ok(token);
            }
            return Unauthorized("Wrong username or password");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private string CreateToken(UserLoginDto user)
    {
        List<Claim> claims = new List<Claim>() {
            new Claim(ClaimTypes.Name, user.UserName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("CubichiPashaTimDrFed"));
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        var token = new JwtSecurityToken(claims: claims,
                                         expires: DateTime.Now.AddDays(1),
                                         signingCredentials: cred);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }

    [HttpPost, Route("UploadSkin")]
    [DisableFormValueModelBinding]
    public IActionResult UploadSkin(IFormFile file)
    {
        try
        {
            var username = User?.Identity?.Name;
            if (username == null)
                return BadRequest("User not found");
            using (var stream = new FileStream(Path.Combine(@"/home/launcher/textures/skins", username + ".png"), FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return Ok("Skin Uploaded Successfully");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost, Route("Ping"), Authorize]
    public ActionResult<string> Ping()
    {
        var username = User?.Identity?.Name;
        return Ok(username);
    }

}