using System.Data.OracleClient;
using NHibernate.Driver;
using NHibernate.SqlTypes;
using roundhouse.infrastructure.logging;

namespace roundhouse.databases.oracle
{
    public class RoundhousEOracleDriver : OracleClientDriver
    {
        protected override void InitializeParameter(System.Data.IDbDataParameter dbParam, string name, SqlType sqlType)
        {
            base.InitializeParameter(dbParam, name, sqlType);

            //http://thebasilet.blogspot.be/2009/07/nhibernate-oracle-clobs.html
            //System.Data.OracleClient.dll driver generates an exception
            //we set the IDbDataParameter.Value = (string whose length: 4000 > length > 2000 )
            //when we set the IDbDataParameter.DbType = DbType.String
            //when DB Column is of type NCLOB/CLOB
            //The Above is the default behavior for NHibernate.OracleClientDriver
            //So we use the built-in StringClobSqlType to tell the driver to use the NClob Oracle type
            //This will work for both NCLOB/CLOBs without issues.
            //Mapping file will need to be update to use StringClob as the property type
            if ((sqlType is StringClobSqlType))
            {
                ((OracleParameter)dbParam).OracleType = OracleType.NClob;
            }
        }
    }
}
