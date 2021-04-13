/*
 * Author(s): Parrish, Christian christian.parrish@ttu.edu
 * Date Created: March 30 2021
 * Notes: N/A
*/
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System;
using System.Data.SqlClient;
using System.Security.Claims;

namespace stonks_api.Models {
	public class Ticker {

		[JsonProperty(PropertyName = "tickerId")]
		public int TickerId { get; }

		[JsonProperty(PropertyName = "name")]
		public string Name { get; }

		[JsonProperty(PropertyName = "companyName")]
		public string CompanyName { get; }

		public Ticker(int tickerId, string name, string companyName) {
			TickerId = tickerId;
			Name = name;
			CompanyName = companyName;
		}

		public static Ticker FromTickerId(int id) {
			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				SqlCommand command = new SqlCommand(@"SELECT * FROM Resource_Ticker WHERE TickerId = @param1;", conn);
				command.Parameters.AddWithValue("@param1", id);

				using (SqlDataReader reader = command.ExecuteReader()) {
					if (reader.HasRows) {
						reader.Read();

						return new Ticker(
							reader.GetInt32(0),
							reader.GetString(1),
							reader.GetString(2)
						);
					} else {
						return null;
					}
				}
			}
		}
	}
}
