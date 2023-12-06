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

        public Record(string ID)
        {
            /*EWSId = (int)reader["EwsId"];
            SeqNo = (int)reader["SeqNo"];
            VitalCode = (string)reader["VitalCode"];
            Score = (int)reader["Score"];
            CriteriaValue = (string)reader["CriteriaValue"];
            CriteriaSign = (int)reader["CriteriaSign"];
            Target = (int)reader["Target"];
            DisplayOrder = (int)reader["DisplayOrder"];*/

            EWSId = int.Parse(ID);

        }
        public Record Create(DbDataReader reader)
        {
            var ret = new Record("0");
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

    public class Record2
    {
        public int EWSId;
        public int Score;
        public int SeqNo;
        public string VitalCode;
        public string CriteriaValue;
        public int CriteriaSign;
        public int Target;
        public int DisplayOrder = 0;

        public Record2(DbDataReader reader)
        {
            EWSId = (int)reader["EwsId"];
            SeqNo = (int)reader["SeqNo"];
            VitalCode = (string)reader["VitalCode"];
            Score = (int)reader["Score"];
            CriteriaValue = (string)reader["CriteriaValue"];
            CriteriaSign = (int)reader["CriteriaSign"];
            Target = (int)reader["Target"];
            DisplayOrder = (int)reader["DisplayOrder"];
            //EWSId = int.Parse(ID);

        }

    }
}
