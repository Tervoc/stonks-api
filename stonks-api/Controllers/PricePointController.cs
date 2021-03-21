/*
 * Author(s): Parrish, Christian christian.parrish@ttu.edu
 * Date Created: March 09 2021
 * Notes: N/A
*/
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

namespace stonks_api.Controllers {
	[Route("v1/pricePoint")]
	[ApiController]
	public class PricePointController : ControllerBase {
		[HttpGet]
		public IActionResult GetPricePoint([FromQuery][Required] string type, [FromQuery][Required] string identifier/*, [FromHeader][Required] string token*/) {
			/*if (!Authentication.IsTokenValid(token)) {
				return Problem("token is not valid");
			}*/

			PricePoint pricePoint;

			if (type.ToLower().Equals("id")) {
				if (int.TryParse(identifier, out int pricePointId)) {
					pricePoint = Models.PricePoint.FromPricePointId(pricePointId);
				} else {
					return Problem("identifier must be an integer when type is id");
				}
			} /*else if (type.ToLower().Equals("tickerId")) {
				user = Models.User.FromEmail(identifier);
			}*/ else {
				return Problem("invalid type");
			}

			if (pricePoint == null) {
				return NotFound("not found");
			} else {
				return Ok(JsonConvert.SerializeObject(pricePoint, Formatting.Indented));
			}
		}

		[HttpGet]
		[Route("price/{tickerId}")]
		public IActionResult GetTickerPriceOverTimespan([FromRoute][Required] int tickerId, [FromQuery][Required] int timespanId/*, [FromHeader][Required] string token*/) {
			/*if (!Authentication.IsTokenValid(token)) {
				return Problem("token is not valid");
			}*/

			List<PricePoint> pricePoints = new List<PricePoint>();

			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();
				
				SqlCommand command = new SqlCommand(@"SELECT * FROM [PricePoint] WHERE (TickerId = @tickerId) AND (TimespanId = @timespanId) ORDER BY Date ASC;", conn);
				command.Parameters.AddWithValue("@tickerId", tickerId);
				command.Parameters.AddWithValue("@timespanId", timespanId);


				using (SqlDataReader reader = command.ExecuteReader()) {
					if (reader.HasRows) {
						while (reader.Read()) {
							DateTimeOffset tickTime = new DateTimeOffset(reader.GetDateTime(8).Ticks, new TimeSpan(-5, 0, 0));
							//DateTime tickTimer = new DateTime(tickTime.ToUnixTimeSeconds());

							PricePoint pricePoint = new PricePoint(
								reader.GetInt32(0),
								reader.GetInt32(1),
								reader.GetInt32(2),
								reader.GetDouble(3),
								reader.GetDouble(4),
								reader.GetDouble(5),
								reader.GetDouble(6),
								reader.GetDouble(7),
								//reader.GetDateTime(8).ToString("yyyy-MM-dd"),// HH:mm:ss"),
								tickTime.ToUnixTimeMilliseconds(),
								//tickTime.ToString(),
								reader.GetDouble(9),
								reader.GetInt32(10)
							);

							pricePoints.Add(pricePoint);
						}
					} else {
						return Problem("Invalid ticker or timespan");
					}
				}
			}

			if (pricePoints == null) {
				return NotFound("no prices found");
			} else {
				return Ok(JsonConvert.SerializeObject(pricePoints, Formatting.Indented));
			}
		}



		[HttpPatch("{id}")]
		public IActionResult UpdatePricePoint([FromRoute] int id, [FromHeader][Required] string token, [FromBody] Dictionary<string, string> patch) {
			if (!Authentication.IsTokenValid(token)) {
				return Problem("token is not valid");
			}
			foreach (string key in patch.Keys) {
				if (Array.IndexOf(Models.PricePoint.UpdateNames, key) == -1) {
					return BadRequest("invalid key");
				}
			}

			SqlCommand command = QueryBuilder.UpdateBuilder(patch, "[PricePoint]", "PricePointId", id);

			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				command.Connection = conn;

				int rows = command.ExecuteNonQuery();

				if (rows == 0) {
					return Problem("could not process");
				}
				return Ok();
			}
		}
	}
}
