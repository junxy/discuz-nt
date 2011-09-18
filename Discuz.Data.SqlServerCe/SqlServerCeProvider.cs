using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlServerCe;

namespace Discuz.Data
{    
    public class SqlServerCeProvider : IDbProvider
    {
        public DbProviderFactory Instance()
        {            
            return SqlCeProviderFactory.Instance;
        }

        public void DeriveParameters(IDbCommand cmd)
        {
            throw new NotSupportedException();
        }

        public DbParameter MakeParam(string ParamName, DbType DbType, Int32 Size)
        {
            return Size > 0 ? new SqlCeParameter(ParamName, (SqlDbType)DbType, Size) :
                new SqlCeParameter(ParamName, (SqlDbType)DbType);

        }

        public bool IsFullTextSearchEnabled()
        {
            return false;
        }

        public bool IsCompactDatabase()
        {
            return false;
        }

        public bool IsBackupDatabase()
        {
            return false;
        }

        public string GetLastIdSql()
        {
            return "SELECT SCOPE_IDENTITY()";
        }

        public bool IsDbOptimize()
        {
            return false;
        }

        public bool IsShrinkData()
        {
            return false;
        }

        public bool IsStoreProc()
        {
            return false;
        }
    }
}
