using Dapper;
using DatabaseAccess.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseAccess
{
    public class DaHalper : IDaHalper
    {
        private readonly ConnectionString _conString;
        private const int sqlCommandTimeout = 300;

        public DaHalper(IOptions<ConnectionString> conString)
        {
            _conString = conString.Value;
        }
        public static class CommonMethod
        {
            public static List<T> ConvertToList<T>(DataTable dt)
            {
                var columnNames = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName.ToLower()).ToList();
                var properties = typeof(T).GetProperties();
                return dt.AsEnumerable().Select(row => {
                    var objT = Activator.CreateInstance<T>();
                    foreach (var pro in properties)
                    {
                        if (columnNames.Contains(pro.Name.ToLower()))
                        {
                            try
                            {
                                pro.SetValue(objT, row[pro.Name]);
                            }
                            catch (Exception ex) { }
                        }
                    }
                    return objT;
                }).ToList();
            }
        }
        private async Task<SqlConnection> CreateDatabaseConnection(bool isOverride=false,string connectionString = "")
        {
            SqlConnection sql = null;
            try
            {
                string dbConnection = !isOverride ? Convert.ToString(_conString.dbConn) :
                    string.IsNullOrEmpty(connectionString) ? Convert.ToString(_conString.dbConn) : connectionString;
                if(dbConnection != null && dbConnection != string.Empty)
                {
                    sql = new SqlConnection(dbConnection);
                    await sql.OpenAsync();
                }
            }
            catch(Exception ex)
            {
                throw  new DBExecption(ex.Message);
            }
            return sql;
        }

        private static DynamicParameters ConvertToDynamicParameters(IList<QueryParameterForSqlMapper> parameterCollection)
        {
            DynamicParameters dynamicParameters = null;
            try
            {
                if(parameterCollection != null && parameterCollection.Any())
                {
                    dynamicParameters = new DynamicParameters();
                    foreach (QueryParameterForSqlMapper parameter in parameterCollection)
                    {
                        dynamicParameters.Add(parameter.Name, parameter.Value, parameter.DbType, parameter.ParameterDirection);
                    }
                }
            }
            catch(Exception ex)
            {
                throw new DBExecption(ex.Message);
            }
            return dynamicParameters;
        }
        public bool BulkCopy(DataTable dt, string tableName, bool isTableCreate = false, bool isOverride = false)
        {
            bool result = false;
            try
            {
                using(SqlConnection dbConnection = CreateDatabaseConnection(isOverride).Result)
                {
                    if (isTableCreate)
                    {
                        CreateTable(dt, tableName, dbConnection, isOverride);
                    }
                    using(SqlBulkCopy s = new SqlBulkCopy(dbConnection))
                    {
                        s.BulkCopyTimeout = 0;
                        s.DestinationTableName = tableName;
                        foreach (object column in dt.Columns)
                        {
                            s.ColumnMappings.Add(column.ToString(), column.ToString());
                        }
                        s.WriteToServer(dt);
                        result = true;
                    }
                }
            }
            catch(Exception ex)
            {
                throw new DBExecption(ex.Message);
            }
            return result;
        }

        public async Task<bool> BulkCopyAsync(DataTable dt, string tableName, bool isTableCreate = false, bool isOverride = false)
        {
            bool result = false;
            try
            {
                using (SqlConnection dbConnection = CreateDatabaseConnection(isOverride).Result)
                {
                    if (isTableCreate)
                    {
                        CreateTable(dt, tableName, dbConnection, isOverride);
                    }
                    using (SqlBulkCopy s = new SqlBulkCopy(dbConnection))
                    {
                        s.DestinationTableName = tableName;
                        foreach (var colum in dt.Columns)
                        {
                            s.ColumnMappings.Add(colum.ToString(), colum.ToString());
                            s.BatchSize = 300;
                            s.BulkCopyTimeout = 0;
                            await s.WriteToServerAsync(dt);
                            result = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DBExecption(ex.Message);
            }
            return result;
        }

        public bool CreateTable(DataTable dt, string TableName, SqlConnection dbConnect, bool isOverride = false)
        {
            bool result = false;
            try
            {
                var createTableBuilder = new StringBuilder("Create table [" + TableName + "]");
                createTableBuilder.AppendLine("(");
                foreach (DataColumn dc in dt.Columns)
                {
                    createTableBuilder.AppendLine("[" + dc.ColumnName + "] varchar(max),");
                }
                createTableBuilder.Remove(createTableBuilder.Length - 1, 1);
                createTableBuilder.AppendLine(")");
                var createTableCommand = new SqlCommand(createTableBuilder.ToString(), dbConnect);
                createTableCommand.CommandTimeout = sqlCommandTimeout;
                createTableCommand.ExecuteNonQuery();
                result = true;
            }
            catch(Exception ex)
            {
                throw new DBExecption(ex.Message);
            }
            return result;
        }

        public async Task<bool> CreateTableAsync(DataTable dt, string TableName, SqlConnection dbConnect, bool isOverride = false)
        {
            bool result = false;
            try
            {
                var createTableBuilder = new StringBuilder("Create table [" + TableName + "]");
                createTableBuilder.AppendLine("(");
                foreach (DataColumn dc in dt.Columns)
                {
                    createTableBuilder.AppendLine("[" + dc.ColumnName + "] varchar(max),");
                }
                createTableBuilder.Remove(createTableBuilder.Length - 1, 1);
                createTableBuilder.AppendLine(")");
                var createTableCommand = new SqlCommand(createTableBuilder.ToString(), dbConnect);
                createTableCommand.CommandTimeout = sqlCommandTimeout;
               await createTableCommand.ExecuteNonQueryAsync();
                result = true;
            }
            catch (Exception ex)
            {
                throw new DBExecption(ex.Message);
            }
            return result;
        }

        public DataSet DsTableValuedParameterExecution(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection, IList<DataTableParameter> dataTableParameterCollection)
        {
            DataSet resultSet = new DataSet();
            using (SqlConnection conn = CreateDatabaseConnection().Result)
            {
                using (SqlCommand cmd = new SqlCommand(storedProcedure, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = sqlCommandTimeout;
                    foreach (QueryParameterForSqlMapper param in parameterCollection)
                    {
                        cmd.Parameters.AddWithValue(param.Name, param.Value);
                        cmd.Parameters[param.Name].Direction = param.ParameterDirection;
                    }
                    if (dataTableParameterCollection != null && dataTableParameterCollection.Any())
                    {
                        foreach (DataTableParameter dataTableParameter in dataTableParameterCollection)
                        {
                            cmd.Parameters.AddWithValue(dataTableParameter.ParameterName, dataTableParameter.DataTable);
                        }
                    }
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(resultSet);
                    }
                }
            }
            return resultSet;
        }

        public async Task<DataSet> DsTableValuedParameterExecutionAsync(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection, IList<DataTableParameter> dataTableParameterCollection)
        {
            DataSet resultSet = new DataSet();
            using (SqlConnection conn = CreateDatabaseConnection().Result)
            {
                using (SqlCommand cmd = new SqlCommand(storedProcedure, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = sqlCommandTimeout;
                    foreach (QueryParameterForSqlMapper param in parameterCollection)
                    {
                        cmd.Parameters.AddWithValue(param.Name, param.Value);
                        cmd.Parameters[param.Name].Direction = param.ParameterDirection;
                    }
                    if(dataTableParameterCollection != null && dataTableParameterCollection.Any())
                    {
                        foreach (DataTableParameter dataTableParameter in dataTableParameterCollection)
                        {
                            cmd.Parameters.AddWithValue(dataTableParameter.ParameterName, dataTableParameter.DataTable);
                        }
                    }
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(resultSet);
                    }
                }
            }
            return resultSet;
        }

        public DataSet ExcuteSQL(string query, IList<QueryParameterForSqlMapper> parameterCollection)
        {
            DataSet resultSet = new DataSet();
            using(SqlConnection conn = CreateDatabaseConnection().Result)
            {
                using(SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = sqlCommandTimeout;
                    if(parameterCollection != null && parameterCollection.Any())
                    {
                        foreach(QueryParameterForSqlMapper param in parameterCollection)
                        {
                            cmd.Parameters.AddWithValue(param.Name, param.Value);
                            cmd.Parameters[param.Name].Direction = param.ParameterDirection;
                        }
                    }
                    using(SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(resultSet);
                    }
                }
            }
            return resultSet;
        }

        public int ExecuteNonQuery(string SqlQuery, IList<QueryParameterForSqlMapper> parameterCollection)
        {
            int result = 0;
            try
            {
                using(SqlConnection conn = CreateDatabaseConnection().Result)
                {
                    using(SqlCommand cmd = new SqlCommand(SqlQuery, conn))
                    {
                        cmd.CommandText = SqlQuery;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = sqlCommandTimeout;
                        foreach (QueryParameterForSqlMapper queryParameterForSqlMapper in parameterCollection)
                        {
                            cmd.Parameters.AddWithValue(queryParameterForSqlMapper.Name, queryParameterForSqlMapper.Value);
                        }
                        result = cmd.ExecuteNonQuery();
                    }
                }
            }
           catch(Exception ex)
            {
                throw new DBExecption(ex.Message);
            }
            return result;
        }

        public string ExecuteScalar(string query, IList<QueryParameterForSqlMapper> parameterCollection)
        {
            var resultSet = "";
            using(SqlConnection conn = CreateDatabaseConnection().Result)
            {
                var cmd = new SqlCommand(query, conn)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = sqlCommandTimeout
                };
                if(parameterCollection != null && parameterCollection.Any())
                {
                    foreach (QueryParameterForSqlMapper param in parameterCollection)
                    {
                        cmd.Parameters.AddWithValue(param.Name, param.Value);
                        cmd.Parameters[param.Name].Direction = param.ParameterDirection;
                    }
                }
                resultSet = (string)cmd.ExecuteScalar();
                cmd.Dispose();
            }
            return resultSet;
        }

        public async Task<DataSet> ExecuteSqlAsync(string query, IList<QueryParameterForSqlMapper> parameterCollection)
        {
            DataSet resultSet = new DataSet();
            using(SqlConnection conn = await CreateDatabaseConnection())
            {
                using(SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = sqlCommandTimeout;
                    if(parameterCollection != null && parameterCollection.Any())
                    {
                        foreach (QueryParameterForSqlMapper param in parameterCollection)
                        {
                            cmd.Parameters.AddWithValue(param.Name, param.Value);
                            cmd.Parameters[param.Name].Direction = param.ParameterDirection;
                        }
                    }
                    using(SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(resultSet);
                    }
                }
            }
            return resultSet;
        }

        public async Task<bool> ExecutionNonQueryAsync(string query, IList<QueryParameterForSqlMapper> parameterCollection)
        {
           using(SqlConnection conn = await CreateDatabaseConnection())
            {
                SqlCommand cmd = new SqlCommand(query, conn)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = sqlCommandTimeout
                };
                if(parameterCollection != null && parameterCollection.Any())
                {
                    foreach (QueryParameterForSqlMapper param  in parameterCollection)
                    {
                        cmd.Parameters.AddWithValue(param.Name, param.Value);
                        cmd.Parameters[param.Name].Direction = param.ParameterDirection;
                    }
                }
                dynamic effectedRows = null;
                try
                {
                    effectedRows = await cmd.ExecuteNonQueryAsync();
                    effectedRows = effectedRows > 0;
                    cmd.Dispose();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
                return effectedRows;
            }
        }

        public IList<dynamic> FetchMultipleRecordSet(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection, bool isOverride = false)
        {
            IList<dynamic> dataCollection = null;
            try
            {
                if (!string.IsNullOrEmpty(storedProcedure))
                {
                    DynamicParameters dynamicParameters = ConvertToDynamicParameters(parameterCollection);
                    using(SqlConnection sql = CreateDatabaseConnection(isOverride).Result)
                    {
                        var resultSet = sql.QueryMultiple(storedProcedure, dynamicParameters, null, sqlCommandTimeout, commandType: CommandType.StoredProcedure);
                        dataCollection = new List<dynamic>();
                        while (!resultSet.IsConsumed)
                        {
                            dataCollection.Add(resultSet.Read());
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw new DBExecption(ex.Message);
            }
            return dataCollection;
        }
        public SqlMapper.GridReader FetchMultipleRecordSetDapper(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection, bool isOverride = false)
        {
           SqlMapper.GridReader dataCollection = null;
            try
            {
                if (!string.IsNullOrEmpty(storedProcedure))
                {
                    DynamicParameters dynamicParameters = ConvertToDynamicParameters(parameterCollection);
                    SqlConnection sql = CreateDatabaseConnection(isOverride).Result;
                    
                        dataCollection = sql.QueryMultiple(storedProcedure, dynamicParameters, null, sqlCommandTimeout, commandType: CommandType.StoredProcedure);
                       
                    
                }
            }
            catch (Exception ex)
            {
                throw new DBExecption(ex.Message);
            }
            return dataCollection;
        }
        public async Task<IList<dynamic>> FetchMultipleRecordSetAsync(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection, bool isOverride = false, string connectionString = "")
        {
            IList<dynamic> dataCollection = null;
            try
            {
                if (!string.IsNullOrEmpty(storedProcedure))
                {
                    DynamicParameters dynamicParameters = ConvertToDynamicParameters(parameterCollection);
                    using (SqlConnection sql = CreateDatabaseConnection(isOverride).Result)
                    {
                        var resultSet = await sql.QueryMultipleAsync(storedProcedure, dynamicParameters, null, sqlCommandTimeout, commandType: CommandType.StoredProcedure);
                        dataCollection = new List<dynamic>();
                        while (!resultSet.IsConsumed)
                        {
                            dataCollection.Add(resultSet.Read());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DBExecption(ex.Message);
            }
            return dataCollection;
        }

        public IEnumerable<T> QueryExecution<T>(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection, bool isOverride = false)
        {
            IEnumerable<T> resultSet = null;
            try
            {
                if (!string.IsNullOrEmpty(storedProcedure))
                {
                    DynamicParameters dynamicParameters = ConvertToDynamicParameters(parameterCollection);
                    using(SqlConnection sql = CreateDatabaseConnection(isOverride).Result)
                    {
                        resultSet = sql.Query<T>(storedProcedure, dynamicParameters, null, true, sqlCommandTimeout, CommandType.StoredProcedure).AsEnumerable();
                    }
                }
            }
            catch(Exception ex)
            {
                throw new DBExecption(ex.Message);
            }
            return resultSet;
        }

        public IEnumerable<T> QueryExecution<T>(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection, IList<DataTableParameter> dataTableParameterCollection)
        {
            IEnumerable<T> resultSet = null;
            try
            {
                if (!string.IsNullOrEmpty(storedProcedure))
                {
                    DynamicParameters dynamicParameters = ConvertToDynamicParameters(parameterCollection);
                    if(dataTableParameterCollection != null && dataTableParameterCollection.Any())
                    {
                        foreach (DataTableParameter dataTableParameter in dataTableParameterCollection)
                        {
                            dynamicParameters.Add(dataTableParameter.ParameterName, dataTableParameter.DataTable.AsTableValuedParameter());
                        }
                    }
                    using(SqlConnection sql = CreateDatabaseConnection().Result)
                    {
                        resultSet = sql.Query<T>(storedProcedure, dynamicParameters, null, true, sqlCommandTimeout, CommandType.StoredProcedure).AsEnumerable();
                    }
                }
            }
            catch(Exception ex)
            {
                throw new DBExecption(ex.Message);
            }
            return resultSet;
        }

        public dynamic QueryExecution(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection)
        {
            dynamic resultSet = null;
            try
            {
                if (!String.IsNullOrEmpty(storedProcedure))
                {
                    DynamicParameters dynamicParameters = ConvertToDynamicParameters(parameterCollection);
                    using(SqlConnection sql = CreateDatabaseConnection().Result)
                    {
                        resultSet = sql.Query(storedProcedure, dynamicParameters, null, true, sqlCommandTimeout, CommandType.StoredProcedure);
                    }
                }
            }
            catch(Exception ex)
            {
                throw new DBExecption(ex.Message);
            }
            return resultSet;
        }

        public async Task<IEnumerable<T>> QueryExecutionAsync<T>(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection, bool isOverride = false, string connectionString = "", int ManualCommandTimeOut = 1)
        {
            IEnumerable<T> resultSet = null;
            int CommanTimeout = ManualCommandTimeOut != 1 ? ManualCommandTimeOut : sqlCommandTimeout;
            try
            {
                if (!string.IsNullOrEmpty(storedProcedure))
                {
                    DynamicParameters dynamicParameters = ConvertToDynamicParameters(parameterCollection);
                    using (SqlConnection sql = await CreateDatabaseConnection(isOverride,connectionString))
                    {
                       var result = await sql.QueryAsync<T>(storedProcedure, dynamicParameters, null, CommanTimeout, CommandType.StoredProcedure);
                        resultSet = result.AsEnumerable();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DBExecption(ex.Message);
            }
            return resultSet;
        }

        public async Task<T> QueryFirstAsync<T>(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection)
        {
            dynamic result = null;
            if (!string.IsNullOrEmpty(storedProcedure))
            {
                DynamicParameters dynamicParameters = ConvertToDynamicParameters(parameterCollection);
                using(SqlConnection sql = await CreateDatabaseConnection())
                {
                    result = await sql.QueryFirstAsync<T>(storedProcedure, dynamicParameters, null, sqlCommandTimeout, CommandType.StoredProcedure);
                }
            }
            return result;
        }

        public int TableValuedDataParameterExecution(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection, IList<DataTableParameter> dataTableParameterCollection)
        {
            int result = 0;
            string outputParamName = String.Empty;
            try
            {
                using (SqlConnection conn = CreateDatabaseConnection().Result)
                {
                    using (SqlCommand cmd = new SqlCommand(storedProcedure, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = sqlCommandTimeout;
                        foreach (QueryParameterForSqlMapper param in parameterCollection)
                        {
                            if (param.ParameterDirection == ParameterDirection.Output)
                            {
                                cmd.Parameters.Add(param.Name, SqlDbType.VarChar, param.Size);
                                cmd.Parameters[param.Name].Direction = param.ParameterDirection;
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue(param.Name, param.Value);
                                cmd.Parameters[param.Name].Direction = param.ParameterDirection;
                            }
                        }
                        if (dataTableParameterCollection != null && dataTableParameterCollection.Any())
                        {
                            foreach (DataTableParameter dataTableParameter in dataTableParameterCollection)
                            {
                                cmd.Parameters.AddWithValue(dataTableParameter.ParameterName, dataTableParameter.DataTable);
                            }
                        }
                        result = cmd.ExecuteNonQuery();
                        if (outputParamName != "")
                        {
                            result = Convert.ToInt32(cmd.Parameters[outputParamName].Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DBExecption(ex.Message);
            }
            return result;
        }

        public DateTime? TableValuedDateDataParameterExecution(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection, IList<DataTableParameter> dataTableParameterCollection)
        {
            DateTime? result = null;
            string outputParamName = "";
            try
            {
                using(SqlConnection conn = CreateDatabaseConnection().Result)
                {
                    using(SqlCommand cmd = new SqlCommand(storedProcedure, conn))
                    {
                        foreach (QueryParameterForSqlMapper param in parameterCollection) 
                        {
                            if(param.ParameterDirection == ParameterDirection.Output)
                            {
                                cmd.Parameters.Add(param.Name, SqlDbType.DateTime, param.Size);
                                cmd.Parameters[param.Name].Direction = param.ParameterDirection;
                                outputParamName = param.Name;
                            }
                        else
                            {
                                cmd.Parameters.AddWithValue(param.Name, param.Value);
                                cmd.Parameters[param.Name].Direction = param.ParameterDirection;
                        }
                        }
                        if(dataTableParameterCollection != null && dataTableParameterCollection.Any())
                        {
                            foreach (DataTableParameter dataTableParameter in dataTableParameterCollection)
                            {
                                cmd.Parameters.AddWithValue(dataTableParameter.ParameterName, dataTableParameter.DataTable);
                            }
                        }
                        cmd.ExecuteNonQuery();
                        if(outputParamName != "")
                        {
                            result = DateTime.Parse(cmd.Parameters[outputParamName].Value.ToString());
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw new DBExecption(ex.Message);
            }
            return result;
        }

        public int TableValuedParameterExceution(string storedProcedure, IList<DataTableParameter> dataTableParameterCollection)
        {
            int result = 0;
            try
            {
                using(SqlConnection conn = CreateDatabaseConnection().Result)
                {
                    using(SqlCommand cmd = new SqlCommand(storedProcedure, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = sqlCommandTimeout;
                        foreach (DataTableParameter dataTableParameter in dataTableParameterCollection)
                        {
                            cmd.Parameters.AddWithValue(dataTableParameter.ParameterName, dataTableParameter.DataTable);
                        }
                        result = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch(Exception ex)
            {
                throw new DBExecption(ex.Message);
            }
            return result;
        }

        public int TableValuedParameterExceution(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection, IList<DataTableParameter> dataTableParameterCollection)
        {
            int result = 0;
            try
            {
                using(SqlConnection conn = CreateDatabaseConnection().Result)
                {
                    using (SqlCommand cmd = new SqlCommand(storedProcedure, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = sqlCommandTimeout;
                        foreach (QueryParameterForSqlMapper param in parameterCollection)
                        {
                            cmd.Parameters.AddWithValue(param.Name,param.Value);
                            cmd.Parameters[param.Name].Direction = param.ParameterDirection;
                        }
                        
                        if(dataTableParameterCollection != null && dataTableParameterCollection.Any())
                        {
                            foreach (DataTableParameter dataTableParameter in dataTableParameterCollection)
                            {
                                cmd.Parameters.AddWithValue(dataTableParameter.ParameterName, dataTableParameter.DataTable);
                            }
                        }
                        result = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch(Exception ex)
            {
                throw new DBExecption(ex.Message);
            }
            return result;
        }

        public async Task<int> TableValuedParameterExecutionAsync(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection, IList<DataTableParameter> dataTableParameterCollection)
        {
            int result = 0;
            try
            {
                using (SqlConnection conn = CreateDatabaseConnection().Result)
                {
                    using (SqlCommand cmd = new SqlCommand(storedProcedure, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = sqlCommandTimeout;
                        foreach (QueryParameterForSqlMapper param in parameterCollection)
                        {
                            cmd.Parameters.AddWithValue(param.Name, param.Value);
                            cmd.Parameters[param.Name].Direction = param.ParameterDirection;
                        }

                        if (dataTableParameterCollection != null && dataTableParameterCollection.Any())
                        {
                            foreach (DataTableParameter dataTableParameter in dataTableParameterCollection)
                            {
                                cmd.Parameters.AddWithValue(dataTableParameter.ParameterName, dataTableParameter.DataTable);
                            }
                        }
                        result = await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DBExecption(ex.Message);
            }
            return result;
        }
    }
}
