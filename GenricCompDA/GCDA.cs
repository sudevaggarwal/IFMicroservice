using CommonUtilitesForAll;
using CommonUtilitesForAll.Models;
using DatabaseAccess;
using DatabaseAccess.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static DatabaseAccess.DaHalper;

namespace GenricCompDA
{
    public class GCDA:IGCDA
    {
        private readonly IDaHalper _daHalper;
        public GCDA(IDaHalper daHalper)
        {
            _daHalper = daHalper;
        }

        public DataSetDetail GetDataSetDetail()
        {
            DataSetDetail dataSetDetail = new DataSetDetail();
            IList<QueryParameterForSqlMapper> parameterCollection = null;
            //DataSet result = _daHalper.ExcuteSQL("usp_DataSetDetail", parameterCollection);
            //if(result != null && result.Tables.Count > 0)
            //{
            //    dataSetDetail.dept = result.Tables[0].AsEnumerable().Select(e => new Dept
            //    {
            //        id = e.Field<int>("id"),
            //        name = e.Field<string>("name")
            //    }).ToList();
            //    dataSetDetail.num = result.Tables[1].AsEnumerable().Select(x => x.Field<int>("num")).FirstOrDefault();
            //    dataSetDetail.name = result.Tables[3].AsEnumerable().Select(x => x.Field<string>("name")).FirstOrDefault();
            //    dataSetDetail.employee = CommonMethod.ConvertToList<Employee>(result.Tables[2]);
            //}
          //  int usp_num = _daHalper.ExecuteNonQuery("usp_num",parameterCollection);
          //  string usp_string = _daHalper.ExecuteScalar("usp_string", parameterCollection);
          var fatchRecord = _daHalper.FetchMultipleRecordSet("usp_DataSetDetail", parameterCollection);
            if(fatchRecord != null)
            {
                dataSetDetail.dept = fatchRecord[0].Count > 0 ? ResultSetConverter.GetResultSetConvertedToList<Dept>(fatchRecord[0]) : null;
                //dataSetDetail.name = fatchRecord[3].Count > 0 ? ResultSetConverter.GetResultSetConvertedToList<string>(fatchRecord[3]) : null;
                //dataSetDetail.num = fatchRecord[1].Count > 0 ? ResultSetConverter.GetResultSetConvertedToList<int>(fatchRecord[1]) : null;
            }
             var QueryExecutionRes = _daHalper.QueryExecution<Dept>("usp_SingleData", parameterCollection);
            //var fatchRecord = _daHalper.FetchMultipleRecordSetDapper("usp_DataSetDetail", parameterCollection);

            //    dataSetDetail.dept = fatchRecord.Read<Dept>().ToList();
            //    dataSetDetail.employee = fatchRecord.Read<Employee>().ToList();
            //dataSetDetail.name = fatchRecord.ReadFirst<string>();
            //dataSetDetail.num=fatchRecord.ReadFirst<int>();

            return dataSetDetail;
        }
    }

    public interface IGCDA
    {
        DataSetDetail GetDataSetDetail();
    }
}
