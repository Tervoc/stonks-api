/*
 * Author(s): Parrish, Christian christian.parrish@ttu.edu
 * Date Created: February something 2021
 * Notes: N/A
*/
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;

namespace stonks_api.Models
{
		public class User
		{
			[JsonProperty(PropertyName = "userId")]
			public int UserId { get; }

			[JsonProperty(PropertyName = "username")]
			public string Username { get; set; }

			[JsonProperty(PropertyName = "email")]
			public string Email { get; set; }

			[JsonProperty(PropertyName = "password")]
			public string Password { get; set; }

			[JsonProperty(PropertyName = "statusId")]
			public int StatusId { get; }

			public static readonly string[] UpdateNames = { "userId", "username", "password", "email", "statusId" };

			public User(int userId, string username, string email, string password, int statusId)
			{
				UserId = userId;
				Username = username;
				Email = email;
				Password = password;
				StatusId = statusId;
			}

			public static User FromUserId(int userId)
			{
				using (SqlConnection conn = new SqlConnection(Startup.ConnectionString))
				{
					conn.Open();

					SqlCommand command = new SqlCommand(@"SELECT * FROM [User] WHERE UserId = @param1;", conn);
					command.Parameters.AddWithValue("@param1", userId);

					using (SqlDataReader reader = command.ExecuteReader())
					{
						if (reader.HasRows)
						{
							reader.Read();

							return new User(
								reader.GetInt32(0),
								reader.GetString(1),
								reader.GetString(2),
								reader.GetString(3),
								reader.GetInt32(4)
							);
						}
						else
						{
							return null;
						}
					}
				}
			}

			public static User FromEmail(string email)
			{
				using (SqlConnection conn = new SqlConnection(Startup.ConnectionString))
				{
					conn.Open();

					SqlCommand command = new SqlCommand(@"SELECT * FROM [User] WHERE Email = @param1;", conn);
					command.Parameters.AddWithValue("@param1", email);

					using (SqlDataReader reader = command.ExecuteReader())
					{
						if (reader.HasRows)
						{
							reader.Read();

							return new User(
								reader.GetInt32(0),
								reader.GetString(1),
								reader.GetString(2),
								reader.GetString(3),
								reader.GetInt32(4)
							);
						}
						else
						{
							return null;
						}
					}
				}
			}

			public List<Claim> ToClaims()
			{
				return new List<Claim> {
				new Claim("userId", UserId.ToString()),
				new Claim("email", Email),
				new Claim("userName", Username),
				new Claim("statusId", StatusId.ToString())
			};
			}

		}
	}
