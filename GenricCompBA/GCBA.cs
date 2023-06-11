using GenricCompDA;
using System;

namespace GenricCompBA
{
    public class GCBA:IGCBA
    {
        private readonly IGCDA _iGCDA;
        public GCBA(IGCDA iGCDA)
        {
            _iGCDA = iGCDA;
        }

        public dynamic GetDataSetDetail()
        {
            dynamic result = null;
            result = _iGCDA.GetDataSetDetail();
                return result;
        }
    }
    public interface IGCBA
    {
        dynamic GetDataSetDetail();
    }
}
