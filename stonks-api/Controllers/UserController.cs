using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Microsoft.AspNetCore.JsonPatch.Adapters;
using stonks_api.Models;
using stonks_api.Utilities;

namespace stonks_api.Controllers
{
	[Route("v1/user")]
	[ApiController]
	public class UserController : ControllerBase
	{
		[HttpGet]//post create patch update
		public IActionResult GetUser([FromQuery][Required] string type, [FromQuery][Required] string identifier)
		{
			User user = null;

			if (type.ToLower().Equals("id"))
			{
				int userId;

				if (int.TryParse(identifier, out userId))
				{
					user = Models.User.FromUserId(userId);
				}
				else
				{
					return Problem("identifier must be an integer when type is id");
				}
			}
			else if (type.ToLower().Equals("email"))
			{
				user = Models.User.FromEmail(identifier);
			}
			else
			{
				return Problem("invalid type");
			}

			if (user == null)
			{
				return NotFound("not found");
			}
			else
			{
				return Ok(JsonConvert.SerializeObject(user, Formatting.Indented));
			}
		}

		[HttpPost]
		public IActionResult CreateUser([FromBody][Required] User user)
		{
			if (user.Email == string.Empty || user.Email == null || user.Password == string.Empty || user.Password == null)
			{
				return Problem("could not process");
			}

			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString))
			{
				conn.Open();

				PasswordHasher hasher = new PasswordHasher();

				SqlCommand command = new SqlCommand(@"INSERT INTO [User] (FirstName, LastName, PreferredName, Password, Email, StatusId) VALUES (@userName, @email, @password, @statusId);", conn);
				command.Parameters.AddWithValue("@userName", user.Username);
				command.Parameters.AddWithValue("@email", user.Email);
				command.Parameters.AddWithValue("@password", hasher.Hash(user.Password));
				command.Parameters.AddWithValue("@statusId", 1);

				int rows = command.ExecuteNonQuery();

				if (rows == 0)
				{
					return Problem("error creating");
				}

			}

			return Ok();
		}

		[HttpPatch("{id}")]
		public IActionResult UpdateUser([FromRoute] int id, [FromBody] Dictionary<string, string> patch)
		{
			foreach (string key in patch.Keys)
			{
				if (Array.IndexOf(Models.User.UpdateNames, key) == -1)
				{
					return BadRequest("invalid key");
				}
			}

			if (patch.ContainsKey("password"))
			{
				PasswordHasher hasher = new PasswordHasher();
				patch["password"] = hasher.Hash(patch["password"]);
			}

			SqlCommand command = QueryBuilder.UpdateBuilder(patch, "[User]", "UserId", id);

			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString))
			{
				conn.Open();

				command.Connection = conn;

				int rows = command.ExecuteNonQuery();

				if (rows != 0)
				{
					return Ok();
				}
				else
				{
					return Problem("could not process");
				}
			}
		}

		[HttpGet]
		[Route("login")]
		public IActionResult LoginUser([FromQuery][Required] string email, [FromQuery][Required] string password)
		{
			PasswordHasher hasher = new PasswordHasher();

			User user = Models.User.FromEmail(email);

			if (user == null)
			{
				return Problem(detail: "invalid email or password");
			}
			else if (hasher.Check(user.Password, password).Verified)
			{
				var tokenHandler = new JwtSecurityTokenHandler();
				var token = new JwtSecurityToken(
					issuer: "stonks-api",
					audience: "stonks-client",
					claims: user.ToClaims(),
					expires: DateTime.Now.AddHours(6),
					signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Convert.FromBase64String(Startup.JWTSecret)), SecurityAlgorithms.HmacSha256Signature)
				);

				return Ok(new Dictionary<string, string> { { "token", tokenHandler.WriteToken(token) } });
			}
			else
			{
				return Problem(detail: "invalid email or password");
			}
		}
	}
}
