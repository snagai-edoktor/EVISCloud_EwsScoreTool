using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using eDoktor.Common;
using eDoktor.Taikoban.SdncCommon;
using eDoktor.Taikoban.SdncDatabase;

namespace eDoktor.Taikoban.SdncMaster
{
    /// <summary>
    /// SDNCインターフェース処理用データベース操作クラス
    /// </summary>
    public class DB
    {
        #region enum
        #endregion

        #region コンスタント
        private const int DEFAULT_REGISTANT = 99990019;
        private const int DEFAULT_MODIFIEDBY = 99990019;
        private const int SQL_IDX_UNIQUE_CONSTRAINT_VIOLATION = 2601;
        private const int DEFAULT_DEMAND_TREATABILITY_TIME_VALUE = 90 /* 90秒*/;
        private const string CST_CSV_SEPERATOR = ",";
        #endregion

        #region フィールド
        /// <summary>データベース操作用</summary>
        private Database _db;
        /// <summary>登録者id</summary>
        private int _registrant = 0;
        /// <summary>更新者id</summary>
        private int _modifiedBy = 0;
        #endregion

        #region プロパティ
        #endregion

        #region コンストラクター
        public DB(Database db, int registrant, int modifiedBy)
        {
            this._db = db;
            this._registrant = registrant;
            this._modifiedBy = modifiedBy;
        }

        public DB(Database db)
        {
            this._db = db;
            this._registrant = DEFAULT_REGISTANT;
            this._modifiedBy = DEFAULT_MODIFIEDBY;
        }
        #endregion

        #region Public メンバー
        /// <summary>
        /// DB内のSDNC各種設定値テーブルの件数を取得する
        /// </summary>
        /// <param name="dumpFlg">SDNC各種設定値テーブル件数をログ出力するかどうか true:出力する false:出力しない</param>
        /// <returns>件数 (-1:不定　例外発生時の場合あり得る)</returns>
        public int CountSettingRecords(bool dumpFlg)
        {
            int count = -1;
            try
            {

                StringBuilder query = new StringBuilder();
                query.Append("SELECT count('x') AS count ");
                query.Append(" FROM t_sdnc_setting_values ");

                this._db.ExecuteQuery(
                    query.ToString(),
                    delegate(DbDataReader reader)
                    {
                        count = Database.GetInt32(reader, "count", 0);
                    }
                );
            }
            catch (Exception ex)
            {
                Trace.OutputErrorTrace(ex.Message);
                count = -1;
            }

            if (dumpFlg == true)
            {
                Trace.OutputTrace(string.Format("t_sdnc_setting_values レコード件数 = {0}", count));
            }
            return count;
        }

        /// <summary>
        /// ネットワークIDのリストを取得する
        /// </summary>
        /// <returns>取得したログインID</returns>
        public List<SdncDatabase.NetworkList> GetSdncNetworkList()
        {
            List<SdncDatabase.NetworkList> nList = new List<NetworkList>();
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("SELECT ");
                query.Append(" nl.id, nl.network_id, nl.vlan_id, nl.notes  ");
                query.Append(" FROM t_sdnc_network_list nl ");

                this._db.ExecuteQuery(
                    query.ToString(),
                    delegate(DbDataReader reader)
                    {
                        SdncDatabase.NetworkList nl = new NetworkList();
                        nl.SetAllColumn(reader);
                        nList.Add(nl);
                    }
                );
            }
            catch (Exception ex)
            {
                eDoktor.Common.Trace.OutputErrorTrace(ex.Message);
            }
            return nList;
        }

        public int InsertNetworkList(List<SdncDatabase.NetworkList> nList)
        {
            int cnt = 0;
            int r = 0;
            StringBuilder query = new StringBuilder();
            foreach (SdncDatabase.NetworkList nwl in nList)
            {
                query.Clear();
                query.Append("INSERT INTO t_sdnc_network_list ( ");
                query.Append(" id, network_id, vlan_id, notes ");
                query.Append(" ) VALUES ( ");
                query.AppendFormat("  {0}", GetDbSetString(nwl.Id));                    // id
                query.AppendFormat(", {0}", GetDbSetString(nwl.NetworkId));             // network_id
                query.AppendFormat(", {0}", GetDbSetString(nwl.VlanId));                // vlan_id
                query.AppendFormat(", {0}", GetDbSetString(nwl.Notes));                 // notes
                query.Append(");");
                r = this._db.ExecuteNonQuery(query.ToString());
                cnt += r;
            }
            return cnt;
        }

        public int DeleteAllNetworkList()
        {
            StringBuilder query = new StringBuilder();
            query.Append("DELETE FROM ");
            query.Append("t_sdnc_network_list ");
            int r = this._db.ExecuteNonQuery(query.ToString());
            return r;
        }
        #endregion

        #region  GetDbSetString メソッド
        private string GetDbSetString(int target)
        {
            return string.Format("{0}", target);
        }

        private string GetDbSetString(DateTime? target)
        {
            if (target.HasValue)
            {
                return string.Format("'{0}'", Database.EscapeParam(target.Value.ToString()));
            }
            else
            {
                return "null";
            }
        }
        private string GetDbSetString(string target)
        {
            return string.Format("'{0}'", Database.EscapeParam(target));
        }

        private string GetDbSetString(System.Enum target)
        {
            return GetDbSetString(Convert.ToInt32(target));
        }

        private string GetDbSetString(bool target)
        {
            return string.Format("'{0}'", target);
        }
        #endregion
    }
}
