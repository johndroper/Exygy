using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Exygy2
{
    public static class SqlHelpers
    {
        public static SqlConnection Init(this SqlConnection conn)
        {
            conn.Open();
            return conn;
        }

        public static async Task<SqlConnection> InitAsync(this SqlConnection conn)
        {
            await conn.OpenAsync();
            return conn;
        }

        public static SqlParameter[] ToArray(this SqlParameterCollection collection)
        {
            SqlParameter[] parameters = new SqlParameter[collection.Count];
            collection.CopyTo(parameters, 0);
            return parameters;
        }

        public static SqlCommand WithData(this SqlCommand col, string key, object value)
        {
            col.Parameters.AddWithValue(key, value);
            return col;
        }

        public static SqlCommand AddParam(this SqlCommand col, string key, SqlDbType sqlDbType)
        {
            col.Parameters.Add(key, sqlDbType);
            return col;
        }

        public static SqlCommand AddParam(this SqlCommand col, string key, SqlDbType sqlDbType, int size)
        {
            col.Parameters.Add(key, sqlDbType, size);
            return col;
        }

        public static SqlCommand Prep(this SqlCommand col)
        {
            col.Prepare();
            return col;
        }

        public static Tuple<T1, T2> ExecuteTuple<T1, T2>(this SqlCommand col)
        {
            using (var reader = col.ExecuteReader())
            {
                if (reader.Read())
                {
                    return new Tuple<T1, T2>(
                        (T1)reader[0],
                        (T2)reader[1]);
                }
                return null;
            }
        }

        public static Tuple<T1, T2, T3> ExecuteTuple<T1, T2, T3>(this SqlCommand col)
        {
            using (var reader = col.ExecuteReader())
            {
                if (reader.Read())
                {
                    return new Tuple<T1, T2, T3>(
                        (T1)reader[0],
                        (T2)reader[1],
                        (T3)reader[2]);
                }
                return null;
            }
        }

        public static Tuple<T1, T2, T3, T4> ExecuteTuple<T1, T2, T3, T4>(this SqlCommand col)
        {
            using (var reader = col.ExecuteReader())
            {
                if (reader.Read())
                {
                    return new Tuple<T1, T2, T3, T4>(
                        (T1)reader[0],
                        (T2)reader[1],
                        (T3)reader[2],
                        (T4)reader[3]);
                }
                return null;
            }
        }

        public static DataTable AddCol(this DataTable dataTable, string name, Type type)
        {
            dataTable.Columns.Add(name, type);
            return dataTable;
        }
    }
}
