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

			[JsonProperty(PropertyName = "userName")]
			public string Username { get; set; }

			[JsonProperty(PropertyName = "password")]
			public string Password { get; set; }

			[JsonProperty(PropertyName = "email")]
			public string Email { get; set; }

			[JsonProperty(PropertyName = "statusId")]
			public int StatusId { get; }

			public static readonly string[] UpdateNames = { "userName", "password", "email", "statusId" };

			public User(int userId, string userName, string password, string email, int statusId)
			{
				UserId = userId;
				Username = userName;
				Password = password;
				Email = email;
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
