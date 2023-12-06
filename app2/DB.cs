using eDoktor.Common;
using System;
using eDoktor.Taikoban.RSA;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eDoktor.Taikoban.AppExtension;

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

                var rec = new Record("0");
                this.Database.ExecuteQuery(
                    (command) =>
                    {
                        //SQL設定
                        command.CommandType = System.Data.CommandType.Text;
                        command.CommandText = query.ToString();
                        //パラメータ設定
                        //command.Parameters.Add();
                        //command.Parameters.Add(Database.CreateParameter(nameof(App.AuthUserId), System.Data.DbType.String, App.AuthUserId));
                    },
                    (reader) => 
                    {
                        //retList.Add(new Record2(reader));  
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
    }
}
