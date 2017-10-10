namespace roundhouse.infrastructure.extensions
{
    using System.Text;
    using System.Data.SqlClient;

    public static class ObjectExtensions {

        public static string to_string(this object input)
        {
            if (input == null) return string.Empty;

            var sqlException = input as SqlException;
            if (sqlException != null)
            {
                var msg = new StringBuilder(sqlException.ToString())
                    .AppendLine()
                    .AppendLine("SqlErrors:");
                
                foreach (SqlError error in sqlException.Errors)
                {
                    msg.AppendFormat("Error Number: {0}, Message: {1}", error.Number, error.Message).AppendLine();
                }

                return msg.ToString();
            }

            return input.ToString();
        }
    }
}