using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace stonks_api.Utilities
{
	public class Authentication
	{
		public static bool IsTokenValid(string token)
		{
			JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

			TokenValidationParameters validationParameters = new TokenValidationParameters();
			validationParameters.IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(Startup.JWTSecret));
			validationParameters.ValidIssuer = "stonks-api";
			validationParameters.ValidAudience = "stonks-client";
			validationParameters.ClockSkew = TimeSpan.Zero;

			SecurityToken validatedToken;

			try
			{
				tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
			}
			catch (Exception)
			{
				return false;
			}

			return true;
		}

		public static Dictionary<string, string> ReadToken(string token)
		{
			JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
			Dictionary<string, string> tokenContents = new Dictionary<string, string>();

			foreach (Claim claim in tokenHandler.ReadJwtToken(token).Claims)
			{
				tokenContents.Add(claim.Type, claim.Value);
			}

			return tokenContents;
		}

		public static string GenerateVerificationCode(int userId)
		{
			Random generator = new Random();
			string verificationCode = string.Empty;
			bool verificationCodeChecked = false;

			SqlCommand command;

			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString))
			{
				conn.Open();

				while (!verificationCodeChecked)
				{
					verificationCode = generator.Next(0, 1000000).ToString("D6");

					command = new SqlCommand(@"SELECT COUNT(*) FROM PhoneVerification WHERE Code = @code AND (ExpirationTimestamp < CURRENT_TIMESTAMP OR UsedFlag = 1)", conn);
					command.Parameters.AddWithValue("@code", verificationCode);

					using (SqlDataReader reader = command.ExecuteReader())
					{
						if (reader.HasRows)
						{
							reader.Read();

							if (reader.GetInt32(0) == 0)
							{
								verificationCodeChecked = true;
							}
						}
					}
				}

				command = new SqlCommand("INSERT INTO PhoneVerification (Code, UserId, ExpirationTimestamp, UsedFlag) VALUES (@code, @userId, DATEADD(MINUTE, 30, CURRENT_TIMESTAMP), 0)", conn);
				command.Parameters.AddWithValue("@code", verificationCode);
				command.Parameters.AddWithValue("@userId", userId);

				int rows = command.ExecuteNonQuery();

				if (rows != 0)
				{
					return verificationCode;
				}
				else
				{
					return null;
				}
			}
		}
	}
}
