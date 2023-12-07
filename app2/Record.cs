using eDoktor.Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace app2
{
    public class Record
    {
        public int EWSId;
        public int Score;
        public int SeqNo;
        public string VitalCode;
        public string CriteriaValue;
        public int CriteriaSign;
        public int Target;
        public int DisplayOrder = 0;

        public Record Create(DbDataReader reader)
        {
            var ret = new Record();
            ret.EWSId = (int)reader["EwsId"];
            ret.SeqNo = (int)reader["SeqNo"];
            ret.VitalCode = (string)reader["VitalCode"];
            ret.Score = (int)reader["Score"];
            ret.CriteriaValue = (string)reader["CriteriaValue"];
            ret.CriteriaSign = (int)reader["CriteriaSign"];
            ret.Target = (int)reader["Target"];
            ret.DisplayOrder = (int)reader["DisplayOrder"];

            return ret;
        }
    }
}
