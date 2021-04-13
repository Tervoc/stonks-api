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
	public class Timespan {

		[JsonProperty(PropertyName = "timespanId")]
		public int TimespanId { get; }

		[JsonProperty(PropertyName = "name")]
		public string Name { get; }

		public Timespan(int timespanId, string name) {
			TimespanId = timespanId;
			Name = name;
		}

		public static Timespan FromTimespanId(int timespanId) {
			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				SqlCommand command = new SqlCommand(@"SELECT * FROM Resource_Timespan WHERE TimespanId = @param1;", conn);
				command.Parameters.AddWithValue("@param1", timespanId);

				using (SqlDataReader reader = command.ExecuteReader()) {
					if (reader.HasRows) {
						reader.Read();

						return new Timespan(
							reader.GetInt32(0),
							reader.GetString(1)
						);
					} else {
						return null;
					}
				}
			}
		}
	}
}
