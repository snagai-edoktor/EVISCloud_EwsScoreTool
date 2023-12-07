using eDoktor.Common;
using System;
using eDoktor.Taikoban.RSA;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eDoktor.Taikoban.AppExtension;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace app2
{
    public class DB
    {
        #region 暗号化用
        private static CryptoParams CryptoParams;
        private static bool gettingCryptParms = false;
        #endregion

        #region Constructor
        public DB()
        {
            eDoktor.Common.Configuration.Reload();
            //データベース接続情報設定
            var connectionStringSettings = eDoktor.Common.Configuration.ConnectionStringSettings(Properties.Settings.Default.ConnectionStringLabel);
            connectionStringSettings.ConnectionString = eDoktor.Taikoban.RSA.Crypto.DecryptString(connectionStringSettings.ConnectionString);
            this.Database = new Database(connectionStringSettings);
        }
        #endregion

        #region Properties
        public Database Database { get; set; }
        #endregion

        #region レコード取得用

        public List<Record> GetRecords(int EwsNo)
        {
            var retList = new List<Record>();
            
            try
            {
                var query = new StringBuilder();
                query.Append($"SELECT * FROM T_EwsScoreCriteria ");
                query.Append($"WHERE EwsId = {EwsNo} ");
                query.Append($"AND InvalidFlag = 0 ");
                query.Append($"AND SeqNo = (SELECT MAX(SeqNo) FROM T_EwsScoreCriteria WHERE EwsId = {EwsNo})");

                var rec = new Record();
                this.Database.ExecuteQuery(
                    (command) =>
                    {
                        //SQL設定
                        command.CommandType = System.Data.CommandType.Text;
                        command.CommandText = query.ToString();
                    },
                    (reader) => 
                    { 
                        retList.Add(rec.Create(reader));
                    });
            }
            catch ( Exception ex )
            {
                eDoktor.Common.Trace.OutputExceptionTrace(ex);
            }

            return retList;
        }
        #endregion

        #region T_EwsScoreCriteriaへレコード追加
        public int InsertRecord(List<Record> recordlist)
        {
            int cnt = 0;
            int r = 0;
            foreach(Record record in recordlist)
            {
                //T_EwsScoreCriteriaへレコード追加
                var sb = new StringBuilder();
                sb.Append("INSERT INTO T_EwsScoreCriteria(EwsId, SeqNo, VitalCode, Score, CriteriaValue, CriteriaSign, Target, DisplayOrder)");
                sb.Append($"VALUES( {record.EWSId}, {record.SeqNo}, '{record.VitalCode}', {record.Score}, '{record.CriteriaValue}', {record.CriteriaSign}, {record.Target}, {record.DisplayOrder})");
                r = this.Database.ExecuteNonQuery(sb.ToString());
                if (r == 0)
                {
                    MessageBox.Show("T_EwsScoreCriteria to INSERT Failed in CreatButton_Click()");
                    break;
                }
                cnt++;
            }
            return cnt;
        }
        #endregion

        #region M_EwsTypeへレコード登録,EwdName新規登録
        public int InsertEwsName(string EwsName, string WarningThresolds)
        {
            int r = 0;   
            var sb = new StringBuilder();
            sb.Append($"INSERT INTO M_EwsType (EwsName, WarningThresholds) VALUES('{EwsName}','{WarningThresolds}')");
            r = this.Database.ExecuteNonQuery(sb.ToString());
            if (r == 0) MessageBox.Show("M_EwsType to INSERT Failed in CreatButton_Click()");
            
            return r;
        }
        #endregion

        #region T_EwsScoreCriteriaへレコード追加後に以前までのレコードのInvalidFlagをあげておく
        //SeqNoはフラグを下げたいNoが引数としている
        public int UpdateInvalidFlag(int EwsId, int SeqNo)
        {
            int r = 0;
            var sb = new StringBuilder();
            sb.Append($"UPDATE T_EwsScoreCriteria SET InvalidFlag = 1 WHERE EwsId = {EwsId} AND SeqNo = {SeqNo}");
            r = this.Database.ExecuteNonQuery(sb.ToString());

            return r;

        }
        #endregion

        #region M_EwsTypeから登録可能な最新のIDを取得する
        //<returns>IDを返す</returns>
        public int GetEwsId()
        {
            int r = 0;
            try
            {
                var query = new StringBuilder();
                query.Append($"SELECT Id FROM M_EwsType WHERE Id = (SELECT MAX(Id) FROM M_EwsType)");
                this.Database.ExecuteQuery(
                    (command) =>
                    {
                        //SQL設定
                        command.CommandType = System.Data.CommandType.Text;
                        command.CommandText = query.ToString();
                    },
                    (reader) =>
                    {
                        int Id = (int)reader["Id"];
                        r = (Id + 1);
                    });
            }
            catch (Exception ex)
            {
                eDoktor.Common.Trace.OutputExceptionTrace(ex);
            }

            return r;
        }
        #endregion

        #region M_VitalTypeからバイタルコードを取得する
        public HashSet<string> GetVitalCode()
        {
            var vitalcode = new HashSet<string>();
            try
            {
                var query = new StringBuilder();
                query.Append($"SELECT vital_code FROM M_VitalType WHERE vital_code IS NOT NULL");
                this.Database.ExecuteQuery(
                    (command) =>
                    {
                        //SQL設定
                        command.CommandType = System.Data.CommandType.Text;
                        command.CommandText = query.ToString();
                    },
                    (reader) =>
                    {
                        vitalcode.Add((string)reader["vital_code"]);
                    });
            }
            catch (Exception ex)
            {
                eDoktor.Common.Trace.OutputExceptionTrace(ex);
            }

            return vitalcode;
        }
        #endregion

        #region M_EwstypeからEwsNameを取得しディクショナリとコンボボックスにセットする
        public void SetEwsName(SortedDictionary<string, int> dic, ComboBox cmb)
        {
            try
            {
                var query = new StringBuilder();
                query.Append($"SELECT Id, EwsName  FROM M_Ewstype WHERE LogicalDeleted = 0");
                this.Database.ExecuteQuery(
                    (command) =>
                    {
                        //SQL設定
                        command.CommandType = System.Data.CommandType.Text;
                        command.CommandText = query.ToString();
                    },
                    (reader) =>
                    {
                        dic.Add((string)reader["EwsName"], (int)reader["Id"]);
                        cmb.Items.Add((string)reader["EwsName"]);
                    });
            }
            catch (Exception ex)
            {
                eDoktor.Common.Trace.OutputExceptionTrace(ex);
            }
        }
        #endregion
    }
}
