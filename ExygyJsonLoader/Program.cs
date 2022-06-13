using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace ExygyJsonLoader
{
    internal class Program
    {
        static readonly string connstr = ConfigurationManager.ConnectionStrings["ExygyDB"].ConnectionString;

        static void Main(string[] args)
        {
            Task t = MainAsync(args);
            t.Wait();
        }

        static async Task MainAsync(string[] args)
        {
            using (var conn = new SqlConnection(connstr))
            {
                await conn.OpenAsync();

                SqlCommand insertProperty = new SqlCommand(@"
                    INSERT INTO Properties (
                        property_id,
                        property_name,
                        property_picture_url)
                    VALUES (
                        @property_id,
                        @property_name,
                        @property_picture_url
                    )",
                conn);

                insertProperty.Parameters.Add("@property_id", System.Data.SqlDbType.UniqueIdentifier);
                insertProperty.Parameters.Add("@property_name", System.Data.SqlDbType.NVarChar, 255);
                insertProperty.Parameters.Add("@property_picture_url", System.Data.SqlDbType.VarChar, 512);
                await insertProperty.PrepareAsync();

                SqlCommand insertUnit = new SqlCommand(@"
                    INSERT INTO Units (
                        property_id,
                        unit_type,
                        unit_min_occupancy,
                        unit_max_occupancy,
                        unit_sqft)
                    VALUES (
                        @property_id,
                        @unit_type,
                        @unit_min_occupancy,
                        @unit_max_occupancy,
                        @unit_sqft
                    ); SELECT @@IDENTITY;",
                conn);

                insertUnit.Parameters.Add("@property_id", System.Data.SqlDbType.UniqueIdentifier);
                insertUnit.Parameters.Add("@unit_type", System.Data.SqlDbType.VarChar, 32);
                insertUnit.Parameters.Add("@unit_min_occupancy", System.Data.SqlDbType.Int);
                insertUnit.Parameters.Add("@unit_max_occupancy", System.Data.SqlDbType.Int);
                insertUnit.Parameters.Add("@unit_sqft", System.Data.SqlDbType.Int);
                await insertUnit.PrepareAsync();

                SqlCommand insertUnitAmenities = new SqlCommand(@"
                    INSERT INTO Unit_Amenities (
                        unit_id,
                        unit_amenity)
                    VALUES (
                        @unit_id,
                        @unit_amenity
                    )",
                conn);

                insertUnitAmenities.Parameters.Add("@unit_id", System.Data.SqlDbType.Int);
                insertUnitAmenities.Parameters.Add("@unit_amenity", System.Data.SqlDbType.NVarChar, 255);
                await insertUnitAmenities.PrepareAsync();

                using (var fileStream = File.OpenRead("mockData.json"))
                using (var streamReader = new StreamReader(fileStream))
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    JArray rootNode = (JArray)JToken.ReadFrom(jsonReader);

                    foreach(var propertyNode in rootNode)
                    {
                        var propertyId = Guid.Parse(propertyNode["id"].Value<string>());
                        insertProperty.Parameters["@property_id"].Value = propertyId;
                        insertProperty.Parameters["@property_name"].Value = propertyNode["name"].Value<string>();
                        insertProperty.Parameters["@property_picture_url"].Value = propertyNode["picture"].Value<string>();
                        await insertProperty.ExecuteNonQueryAsync();

                        foreach(var unitNode in propertyNode["units"])
                        {
                            insertUnit.Parameters["@property_id"].Value = propertyId;
                            insertUnit.Parameters["@unit_type"].Value = unitNode["type"].Value<string>();
                            insertUnit.Parameters["@unit_min_occupancy"].Value = unitNode["minOccupancy"].Value<int>();
                            insertUnit.Parameters["@unit_max_occupancy"].Value = unitNode["maxOccupancy"].Value<int>();
                            insertUnit.Parameters["@unit_sqft"].Value = unitNode["sqft"].Value<int>();
                            var unitId = await insertUnit.ExecuteScalarAsync();

                            foreach(var amenitiesNode in unitNode["amenities"])
                            {
                                insertUnitAmenities.Parameters["@unit_id"].Value = unitId;
                                insertUnitAmenities.Parameters["@unit_amenity"].Value = amenitiesNode.Value<string>();
                                await insertUnitAmenities.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }
            }
        }
    }
}
