/*
 * Author(s): Parrish, Christian christian.parrish@ttu.edu
 * Date Created: April 13 2021
 * Notes: N/A
*/
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System;
using System.Data.SqlClient;
using System.Security.Claims;

namespace stonks_api.Models {
	public class Prediction {
		[JsonProperty(PropertyName = "predictionId")]
		public int PredictionId { get; }

		[JsonProperty(PropertyName = "closeTimestamp")]
		public long CloseTimestamp { get; }

		[JsonProperty(PropertyName = "timespanId")]
		public int TimespanId { get; }

		[JsonProperty(PropertyName = "tickerId")]
		public int TickerId { get; }

		[JsonProperty(PropertyName = "statusId")]
		public int StatusId { get; set; }

		[JsonProperty(PropertyName = "close")]
		public double Close { get; }

		public static readonly string[] UpdateNames = { "statusId" };

		public Prediction(int predictionId, long closeTimestamp, int timespanId, int tickerId, int statusId, double close) {
			PredictionId = predictionId;
			CloseTimestamp = closeTimestamp;
			TimespanId = timespanId;
			TickerId = tickerId;
			StatusId = statusId;
			Close = close;
		}

		public static Prediction FromPricePointId(int id) {
			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				SqlCommand command = new SqlCommand(@"SELECT * FROM dbo.Predictions WHERE PredictionId = @id;", conn);
				command.Parameters.AddWithValue("@id", id);

				using (SqlDataReader reader = command.ExecuteReader()) {
					if (reader.HasRows) {
						reader.Read();
						DateTimeOffset tickTime = new DateTimeOffset(reader.GetDateTime(1).Ticks, new TimeSpan(-5, 0, 0));

						return new Prediction(
							reader.GetInt32(0),
							tickTime.ToUnixTimeMilliseconds(),
							reader.GetInt32(2),
							reader.GetInt32(3),
							reader.GetInt32(4),
							reader.GetDouble(5)
							
						);
					} else {
						return null;
					}
				}
			}
		}
	}
}
