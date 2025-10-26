using System.Data;
using System.Data.SqlClient;

namespace OnlineExamSystem.Common
{
    public static class SqlHelper
    {
        static string connectionString = "Data Source=VISHAL_KUMBHAR\\SQLEXPRESS;Initial Catalog=OnlineExamSystem;Trusted_Connection=True;Connect Timeout=300;";

        /// <summary>
        /// Executes a stored procedure with parameters and optional output parameter.
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="storedProcedureName">SP name</param>
        /// <param name="parameters">Dictionary of parameter names and values</param>
        /// <param name="outputParamName">Optional output parameter name</param>
        /// <returns>Value of output parameter if provided; otherwise null</returns>

        public static object ExecuteStoredProcedure(
    string storedProcedureName,
    Dictionary<string, object> parameters,
    string outputParamName = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required.");

            if (string.IsNullOrWhiteSpace(storedProcedureName))
                throw new ArgumentException("Stored procedure name is required.");

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(storedProcedureName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.CommandTimeout = 300;
                // Add parameters
                if (parameters != null)
                {
                    foreach (var kvp in parameters)
                    {
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
                    }
                }

                // Add output parameter if needed
                SqlParameter outputParam = null;
                if (!string.IsNullOrWhiteSpace(outputParamName))
                {
                    outputParam = new SqlParameter(outputParamName, SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outputParam);
                }

                conn.Open();                  // synchronous
                cmd.ExecuteNonQuery();        // synchronous

                if (outputParam != null)
                    return outputParam.Value;

                return null;
            }
        }


        public static DataTable ExecuteStoredProcedureSelect(
     string storedProcedureName,
     Dictionary<string, object> parameters = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required.");

            if (string.IsNullOrWhiteSpace(storedProcedureName))
                throw new ArgumentException("Stored procedure name is required.");

            var dt = new DataTable();

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(storedProcedureName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // Add parameters
                if (parameters != null)
                {
                    foreach (var kvp in parameters)
                    {
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
                    }
                }

                // Open connection synchronously
                conn.Open();

                // Execute reader synchronously
                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }

            return dt;
        }


        public static void ExecuteStoredProcedureEditExam(string storedProcedureName, Dictionary<string, object> parameters)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(storedProcedureName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                foreach (var kvp in parameters)
                {
                    cmd.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
                }

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }


    }
}
