using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Exygy2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExygyPropertiesController : ControllerBase
    {
        private readonly ILogger<ExygyPropertiesController> _logger;
        //static readonly string connstr = ConfigurationManager.ConnectionStrings["ExygyDB"].ConnectionString;

        private readonly IConfiguration configuration;

        private readonly string connstr;

        public ExygyPropertiesController(ILogger<ExygyPropertiesController> logger, IConfiguration config)
        {
            _logger = logger;
            configuration = config;
            connstr = configuration.GetConnectionString("ExygyDB");
        }

        [HttpGet]
        public ExygyPage Get(int recordsPerPage = 10, int page = 1, string propertyName = null, int? minOccupancy = null, int? maxOccupancy = null, [FromQuery] string[] amenities = null)
        {
            ExygyPage exygyPage = new()
            {
                recordsPerPage = recordsPerPage,
                page = page
            };

            using (var conn = new SqlConnection(connstr))
            {
                conn.Open();

                using(var reader = new SqlCommand(
                    @"SELECT DISTINCT 
                        ua.unit_amenity
	                FROM Unit_Amenities ua",
                    conn).ExecuteReader())
                {
                    while(reader.Read())
                    {
                        exygyPage.validAmenities.Add((string)reader["unit_amenity"]);
                    }
                }

                var select = @"
                    SELECT
                        p.property_id,
                        p.property_name,
                        p.property_picture_url,
                     t1.avg_unit_sqft,
                     t1.min_min_occupancy,
                     t1.max_max_occupancy";

                var sql = @"
                    FROM
                        Properties p
                     JOIN (
                      SELECT
                       p1.property_id,
                       AVG(u1.unit_sqft) avg_unit_sqft,
                       MIN(u1.unit_min_occupancy) min_min_occupancy,
                       MAX(u1.unit_max_occupancy) max_max_occupancy
                      FROM
                       properties p1
                      JOIN units u1
                      ON
                       p1.property_id = u1.property_id
                      GROUP BY
                       p1.property_id) t1
                        ON p.property_id = t1.property_id";


                var sqlSelectCmd = new SqlCommand
                {
                    Connection = conn
                };

                if (amenities != null && amenities.Any())
                {
                    var amenitiesInParams = String.Join(
                        ",",
                        amenities.Select((amenity, index) => $"@amenity_{index}"));
                    sql += @"
                        JOIN (
                      SELECT DISTINCT
                       u2.property_id
                      FROM
                       Units u2
                       JOIN Unit_Amenities ua
                       ON u2.unit_id = ua.unit_id
                      WHERE
                       ua.unit_amenity IN (" + amenitiesInParams + @")
                     ) t2
                     ON p.property_id = t2.property_id";
                    int index = 0;
                    foreach (var amenity in amenities)
                    {
                        sqlSelectCmd.Parameters.AddWithValue($"@amenity_{index}", amenity);
                        index++;
                    }
                }

                if (!String.IsNullOrWhiteSpace(propertyName) || minOccupancy.HasValue || maxOccupancy.HasValue)
                {
                    List<string> wheres = new List<string>();
                    if (!String.IsNullOrWhiteSpace(propertyName))
                    {
                        wheres.Add("p.property_name LIKE @propertyName");
                        sqlSelectCmd.Parameters.AddWithValue("@propertyName", propertyName + "%");
                    }

                    if (minOccupancy.HasValue)
                    {
                        wheres.Add("t1.min_min_occupancy >= @minOccupancy");
                        sqlSelectCmd.Parameters.AddWithValue("@minOccupancy", minOccupancy.Value);
                    }

                    if (minOccupancy.HasValue)
                    {
                        wheres.Add("t1.max_max_occupancy <= @maxOccupancy");
                        sqlSelectCmd.Parameters.AddWithValue("@maxOccupancy", maxOccupancy.Value);
                    }

                    sql += $" WHERE {String.Join(" AND ", wheres)}";
                }

                sqlSelectCmd.CommandText = "SELECT COUNT(*) " + sql;
                exygyPage.recordCount = (int)sqlSelectCmd.ExecuteScalar();

                var offset = @"
                    ORDER BY
                        p.property_name
                    OFFSET @offset ROWS
                    FETCH NEXT @fetch ROWS ONLY";

                sqlSelectCmd.CommandText = select + sql + offset;
                using (var reader = sqlSelectCmd
                    .WithData("@offset", (page - 1) * recordsPerPage)
                    .WithData("@fetch", recordsPerPage)
                    .ExecuteReader())
                {
                    while (reader.Read())
                    {
                        exygyPage.properties.Add(new ExygyProperty
                        {
                            id = (Guid)reader["property_id"],
                            name = (string)reader["property_name"],
                            pictureUrl = (string)reader["property_picture_url"]
                        });
                    }
                }

                foreach (var property in exygyPage.properties)
                {
                    using (var reader = new SqlCommand(
                       @"
                        SELECT
                         u.unit_type,
                         AVG(u.unit_sqft) avg_unit_sqft,
                         MIN(u.unit_min_occupancy) min_min_occupancy,
                         MAX(u.unit_max_occupancy) max_max_occupancy
                        FROM
                         Units u
                        WHERE
                         u.property_id = @property_id
                        GROUP BY
                         u.unit_type
                        ORDER BY
                         AVG(u.unit_sqft)",
                       conn)
                   .WithData("@property_id", property.id)
                   .ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            property.unitsTypes.Add(new ExygyPropertyUnitType
                            {
                                unitType = (string)reader["unit_type"],
                                avgUnitSqft = (int)reader["avg_unit_sqft"],
                                minMinOccupancy = (int)reader["min_min_occupancy"],
                                maxMaxOccupancy = (int)reader["max_max_occupancy"]
                            });
                        }
                    }
                }

                return exygyPage;
            }
        }
    }
}
