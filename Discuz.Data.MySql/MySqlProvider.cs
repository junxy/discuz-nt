using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace Discuz.Data
{
    public class MySqlProvider : IDbProvider
    {
        public DbProviderFactory Instance()
        {
            return MySqlClientFactory.Instance;
        }

        public void DeriveParameters(IDbCommand cmd)
        {
            if (cmd as MySqlCommand != null)
            {
                MySqlCommandBuilder.DeriveParameters(cmd as MySqlCommand);
            }
        }

        public DbParameter MakeParam(string ParamName, DbType DbType, int Size)
        {
            //return Size > 0 ? new MySqlParameter(ParamName, ConvertToMySqlDbType(ref DbType), Size) :
            //    new MySqlParameter(ParamName, ConvertToMySqlDbType(ref DbType));

            return Size > 0 ? new MySqlParameter(ParamName, (MySqlDbType)DbType, Size) :
                new MySqlParameter(ParamName, (MySqlDbType)DbType);
        }

        #region

        /// <summary>
        /// 数据库类型 DbType => MySqlDbType
        /// via:\\Source\MySql.Data\parameter.cs line:490
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        private static MySqlDbType ConvertToMySqlDbType(ref DbType dbType)
        {
            switch (dbType)
            {
                case DbType.Guid:
                    return MySqlDbType.Guid;
                case DbType.AnsiString:
                case DbType.String:
                    return MySqlDbType.VarChar;
                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                    return MySqlDbType.String;
                case DbType.Boolean:
                case DbType.Byte:
                    return MySqlDbType.UByte;
                case DbType.SByte:
                    return MySqlDbType.Byte;
                case DbType.Date:
                    return MySqlDbType.Date;
                case DbType.DateTime:
                    return MySqlDbType.DateTime;
                case DbType.Time:
                    return MySqlDbType.Time;
                case DbType.Single:
                    return MySqlDbType.Float;
                case DbType.Double:
                    return MySqlDbType.Double;
                case DbType.Int16:
                    return MySqlDbType.Int16;
                case DbType.UInt16:
                    return MySqlDbType.UInt16;
                case DbType.Int32:
                    return MySqlDbType.Int32;
                case DbType.UInt32:
                    return MySqlDbType.UInt32;
                case DbType.Int64:
                    return MySqlDbType.Int64;
                case DbType.UInt64:
                    return MySqlDbType.UInt64;
                case DbType.Decimal:
                case DbType.Currency:
                    return MySqlDbType.Decimal;
                case DbType.Object:
                case DbType.VarNumeric:
                case DbType.Binary:
                default:
                    return MySqlDbType.Blob;
            }
        }

        #endregion

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
            return "SELECT LAST_INSERT_ID()";
        }

        public bool IsDbOptimize()
        {
            return true;
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