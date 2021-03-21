/*
 * Author(s): Parrish, Christian christian.parrish@ttu.edu
 * Date Created: March 09 2021
 * Notes: N/A
*/
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System;
using System.Data.SqlClient;
using System.Security.Claims;

namespace stonks_api.Models {
	public class PricePoint {
		[JsonProperty(PropertyName = "pricePointId")]
		public int PricePointId { get; }

		[JsonProperty(PropertyName = "tickerId")]
		public int TickerId { get; }

		[JsonProperty(PropertyName = "timespanId")]
		public int TimespanId { get; }

		[JsonProperty(PropertyName = "volumeWeighted")]
		public double VolumeWeighted{ get; }

		[JsonProperty(PropertyName = "open")]
		public double Open { get; }

		[JsonProperty(PropertyName = "close")]
		public double Close { get; }

		[JsonProperty(PropertyName = "low")]
		public double Low { get; }

		[JsonProperty(PropertyName = "high")]
		public double High { get; }

		[JsonProperty(PropertyName = "date")]
		public long Date { get; }

		[JsonProperty(PropertyName = "volume")]
		public double Volume { get; }

		[JsonProperty(PropertyName = "statusId")]
		public int StatusId { get; set; }

		public static readonly string[] UpdateNames = { "statusId" };

		public PricePoint(int pricePointId, int tickerId, int timespanId, double volumeWeighted, double open, double close, double low, double high, long date, double volume, int statusId) {
			PricePointId = pricePointId;
			TimespanId = timespanId;
			TickerId = tickerId;
			VolumeWeighted = volumeWeighted;
			Open = open;
			Close = close;
			Low = low;
			High = high;
			Date = date;
			Volume = volume; 
			StatusId = statusId;
		}

		public static PricePoint FromPricePointId(int pricePointId) {
			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				SqlCommand command = new SqlCommand(@"SELECT * FROM [PricePoint] WHERE PricePointId = @pricePointId;", conn);
				command.Parameters.AddWithValue("@pricePointId", pricePointId);

				using (SqlDataReader reader = command.ExecuteReader()) {
					if (reader.HasRows) {
						reader.Read();
						DateTimeOffset tickTime = new DateTimeOffset(reader.GetDateTime(8).Ticks, new TimeSpan(-5, 0, 0));

						return new PricePoint(
							reader.GetInt32(0),
							reader.GetInt32(1),
							reader.GetInt32(2),
							reader.GetDouble(3),
							reader.GetDouble(4),
							reader.GetDouble(5),
							reader.GetDouble(6),
							reader.GetDouble(7),
							//reader.GetDateTime(8).ToString("d"),
							tickTime.ToUnixTimeMilliseconds(),
							//tickTime.ToString(),
							reader.GetDouble(9),
							reader.GetInt32(10)
						);
					} else {
						return null;
					}
				}
			}
		}
	}
}
