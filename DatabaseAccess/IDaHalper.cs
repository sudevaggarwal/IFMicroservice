using Dapper;
using DatabaseAccess.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace DatabaseAccess
{
    public interface IDaHalper
    {
        DataSet ExcuteSQL(string query, IList<QueryParameterForSqlMapper> parameterCollection);

        int TableValuedParameterExceution(string storedProcedure, IList<DataTableParameter> dataTableParameterCollection);

        int TableValuedParameterExceution(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection, IList<DataTableParameter> dataTableParameterCollection);

        Task<int> TableValuedParameterExecutionAsync(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection, IList<DataTableParameter> dataTableParameterCollection);

        int TableValuedDataParameterExecution(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection, IList<DataTableParameter> dataTableParameterCollection);

        DataSet DsTableValuedParameterExecution(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection, IList<DataTableParameter> dataTableParameterCollection);

        Task<DataSet> DsTableValuedParameterExecutionAsync(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection, IList<DataTableParameter> dataTableParameterCollection);

        IEnumerable<T> QueryExecution<T>(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection, bool isOverride = false);

        IEnumerable<T> QueryExecution<T>(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection, IList<DataTableParameter> dataTableParameterCollection);

        dynamic QueryExecution(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection);

        Task<IEnumerable<T>> QueryExecutionAsync<T>(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection, bool isOverride = false, string connectionString = "", int ManualCommandTimeOut = 1);

        IList<dynamic> FetchMultipleRecordSet(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection, bool isOverride = false);

        Task<IList<dynamic>> FetchMultipleRecordSetAsync(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection, bool isOverride = false, string connectionString = "");

        Task<DataSet> ExecuteSqlAsync(string query, IList<QueryParameterForSqlMapper> parameterCollection);

        bool BulkCopy(DataTable dt, string tableName, bool isTableCreate = false, bool isOverride = false);

        int ExecuteNonQuery(string SqlQuery, IList<QueryParameterForSqlMapper> parameterCollection);

        Task<bool> BulkCopyAsync(DataTable dt, string tableName, bool isTableCreate = false, bool isOverride = false);

        DateTime? TableValuedDateDataParameterExecution(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection, IList<DataTableParameter> dataTableParameterCollection);

        Task<bool> ExecutionNonQueryAsync(string query, IList<QueryParameterForSqlMapper> parameterCollection);

        Task<T> QueryFirstAsync<T>(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection);

        string ExecuteScalar(string query, IList<QueryParameterForSqlMapper> parameterCollection);

        bool CreateTable(DataTable dt, string TableName, SqlConnection dbConnect, bool isOverride = false);

        Task<bool> CreateTableAsync(DataTable dt, string TableName, SqlConnection dbConnect, bool isOverride = false);
        SqlMapper.GridReader FetchMultipleRecordSetDapper(string storedProcedure, IList<QueryParameterForSqlMapper> parameterCollection, bool isOverride = false);
    }
}
