using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

namespace CommonUtilitesForAll
{
    public static class ResultSetConverter
    {
        public static IList<T> GetResultSetConvertedToList<T>(IEnumerable<dynamic> resultSet)
        {
            IList<T> dataSet = (IList<T>)resultSet.Select<dynamic, T>(x => BindingProperties(Activator.CreateInstance(typeof(T)), x)).ToList();
            return dataSet;
        }

        private static T BindingProperties<T>(T data,dynamic rowInstance)
        {
            IDictionary<string, object> keyValueCollection = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            IDictionary<string, object> rowData = (IDictionary<string, object>)rowInstance;
            foreach (string key in rowData.Keys)
            {
                keyValueCollection.Add(key.ToLower(), rowData[key]);
            }
            BindDataSetPropertyWithDictionayObject<T>(ref data, keyValueCollection);
            return data;
        }
        private static void BindDataSetPropertyWithDictionayObject<T>(ref T dataSet,IDictionary<string,object> keyValueCollection)
        {
            try
            {
                PropertyInfo[] properties = dataSet.GetType().GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    Type t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    if(t != null && keyValueCollection.ContainsKey(property.Name.ToLower()))
                     {
                        var computedField = keyValueCollection[property.Name.ToLower()];
                        if(computedField != null)
                        {
                            var inputData = Convert.ChangeType(computedField, t);
                            if(t.BaseType != null && !t.IsGenericType && t.Namespace == "System")
                            {
                                property.SetValue(dataSet, inputData, null);
                            }
                            else if(inputData != null)
                            {
                                property.SetValue(dataSet, inputData, null);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw new ServiceException(ex.Message);
            }
        }
    }
}
