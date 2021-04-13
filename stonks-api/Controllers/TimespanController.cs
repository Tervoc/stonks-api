/*
 * Author(s): Parrish, Christian christian.parrish@ttu.edu
 * Date Created: March 30 2021
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
	[Route("v1/resource/timespan")]
	[ApiController]
	public class TimespanController : ControllerBase {
		[HttpGet]
		[ProducesResponseType(typeof(Timespan), 200)]
		public IActionResult GetTickers() {
			List<Timespan> timespans= new List<Timespan>();

			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				SqlCommand command = new SqlCommand(@"SELECT * FROM [Resource_Timespan] ORDER BY TimespanId ASC;", conn);

				using (SqlDataReader reader = command.ExecuteReader()) {
					if (reader.HasRows) {
						while (reader.Read()) {
							timespans.Add(new Timespan(
								 reader.GetInt32(0),
								 reader.GetString(1)
							));
						}
					} else {
						return Problem("no tickers found");
					}
				}
			}

			return Ok(JsonConvert.SerializeObject(timespans, Formatting.Indented));
		}
	}
}
