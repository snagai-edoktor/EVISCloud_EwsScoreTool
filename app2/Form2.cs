using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace app2
{
    public partial class Form2 : Form
    {
        //UPDATE用
        private TextBox[,] _txtCriteiaValueA;
        private TextBox[,] _txtCriteiaValueB;
        private ComboBox[,] _cmb;
        private ComboBox[] _vitalcode;
        //CREATE用
        private TextBox[,] _BtxtCriteiaValueA;
        private TextBox[,] _BtxtCriteiaValueB;
        private ComboBox[,] _Bcmb;
        private ComboBox[] _Bvitalcode;
        private ComboBox[] _cmbDataTypeUP;
        private ComboBox[] _cmbDataTypeCRE;

        private int[] _score = new int[] { 3, 2, 1, 0, 1, 2, 3 };
        //private TextBox[] _ScoreLv;
        private int[] tarval = new int[] { 0, 0, 0, 1, 1, 1, 1 };
        private SortedDictionary<string, int> EwsName = new SortedDictionary<string, int>();
        private SortedDictionary<string, int> DicVitalcodeandDisplayOrder = new SortedDictionary<string, int>();

        //vital配列10、score配列7、リストを作りたい
        List<Record>[,] GetRecords = new List<Record>[11, 7];
        List<string> VitalCodeName = new List<string>();
        public Form2()
        {
            InitializeComponent();
        }
        private void Form2_Load(object sender, EventArgs e)
        {
            InitControl();
            InitComboBox();
            InitEwsName();
        }
        private void Update_scorearray()
        {
            if (txtScoreLv3L.Text != "") _score[0] = Int32.Parse(txtScoreLv3L.Text);
            if (txtScoreLv2L.Text != "") _score[1] = Int32.Parse(txtScoreLv2L.Text);
            if (txtScoreLv1L.Text != "") _score[2] = Int32.Parse(txtScoreLv1L.Text);
                                         _score[3] = 0;
            if (txtScoreLv1L.Text != "") _score[4] = Int32.Parse(txtScoreLv1L.Text);
            if (txtScoreLv2L.Text != "") _score[5] = Int32.Parse(txtScoreLv2L.Text);
            if (txtScoreLv3L.Text != "") _score[6] = Int32.Parse(txtScoreLv3L.Text);

            for(int i=0;i<3;i++)
            {
                if (_score[i] < _score[i + 1])
                {
                    MessageBox.Show("スコアが間違っています");
                    break;
                }
            }
        }

        //行番号取得用メソッド
        private int GetLineNumber([CallerLineNumber] int intLineNumber = 0)
        {
            return intLineNumber;
        }
        /* shundbg
        private void Create_scorearray()
        {
            if (txtCreScoreLv3L.Text != "") _score[0] = Int32.Parse(txtCreScoreLv3L.Text);
            if (txtCreScoreLv2L.Text != "") _score[1] = Int32.Parse(txtCreScoreLv2L.Text);
            if (txtCreScoreLv1L.Text != "") _score[2] = Int32.Parse(txtCreScoreLv1L.Text);
                                            _score[3] = 0;
            if (txtCreScoreLv1L.Text != "") _score[4] = Int32.Parse(txtCreScoreLv1L.Text);
            if (txtCreScoreLv2L.Text != "") _score[5] = Int32.Parse(txtCreScoreLv2L.Text);
            if (txtCreScoreLv3L.Text != "") _score[6] = Int32.Parse(txtCreScoreLv3L.Text);
            for (int i = 0; i < 3; i++)
            {
                if (_score[i] < _score[i + 1])
                {
                    MessageBox.Show("スコアが間違っています");
                    break;  
                }
            }
        }*/
        /// <summary>
        /// 受け取った入力情報からレコードを作りRecListに追加する
        /// 新規追加用
        /// </summary>
        /// <param name="RecList"></param>
        /// <param name="vitalcode"></param>
        /// <param name="txtA"></param>
        /// <param name="symb"></param>
        /// <param name="txtB"></param>
        /// <param name="score"></param>
        /// <param name="tarval"></param>
        public bool CreatRecord(List<Record> RecList, List<int> intlist, int vitalcode, string txtA, int symb, string txtB, int score, int tarval, int displayorder,int datatype)
        {
            double d;
            int i;
            bool fl = true;
            //string[] words;
            switch (symb)
            {
                case 0://未選択
                    //エラー この関数を呼ぶ前にtxtA,bに中身があるか確認してるからここには来ないはず
                    MessageBox.Show("コンボボックスが未選択です");
                    fl = false;
                    break;
                case 1:// =  memo 1つのレコード作成
                    if(datatype != 2)
                    {
                        //エラー 数値入力なのに単一文字列が含まれている
                        fl = false;
                        string strMsg = GetLineNumber().ToString() + " 行目。";    //この行（サンプルでは50行目）が表示される

                        //メッセージボックスで行番号を表示
                        MessageBox.Show(strMsg
                                        , "情報"
                                        , MessageBoxButtons.OK
                                        , MessageBoxIcon.Information);
                        break;
                    }
                    var record0 = new Record(txtCreateEWSID.Text);
                    record0.Score = score;
                    record0.VitalCode = VitalCodeName[vitalcode];
                    record0.CriteriaValue = txtA;
                    record0.CriteriaSign = 2;
                    record0.Target = tarval;
                    record0.DisplayOrder = displayorder;

                    RecList.Add(record0);
                    break;
                case 2:// , memo レコード数に限りない
                    if (datatype != 2)
                    {
                        //エラー 数値入力なのに単一文字列が含まれている
                        fl = false;
                        string strMsg = GetLineNumber().ToString() + " 行目。";    //この行（サンプルでは50行目）が表示される

                        //メッセージボックスで行番号を表示
                        MessageBox.Show(strMsg
                                        , "情報"
                                        , MessageBoxButtons.OK
                                        , MessageBoxIcon.Information);
                        break;
                    }
                    string[] words = txtA.Split(',');
                    foreach (var word in words)
                    {
                        var record1 = new Record(txtCreateEWSID.Text);
                        record1.Score = score;
                        record1.VitalCode = VitalCodeName[vitalcode];
                        record1.CriteriaValue = word;
                        record1.CriteriaSign = 2;
                        record1.Target = tarval;
                        record1.DisplayOrder = displayorder;

                        RecList.Add(record1);
                    }
                    break;
                case 3:// ~ memo 2つレコードを作ればいい
                    var record21 = new Record(txtCreateEWSID.Text);
                    record21.Score = score;
                    record21.VitalCode = VitalCodeName[vitalcode];
                    record21.CriteriaValue = txtA;
                    record21.CriteriaSign = 1;
                    record21.Target = tarval;
                    record21.DisplayOrder = displayorder;

                    RecList.Add(record21);

                    var record22 = new Record(txtCreateEWSID.Text);
                    record22.Score = score;
                    record22.VitalCode = VitalCodeName[vitalcode];
                    record22.CriteriaValue = txtB;
                    record22.CriteriaSign = 0;
                    record22.Target = tarval;
                    record22.DisplayOrder = displayorder;

                    RecList.Add(record22);


                    //入力制限判定用
                    //double入力
                    if (datatype == 1)
                    {
                        double da;
                        double.TryParse(txtB, out d);
                        double.TryParse(txtA, out da);
                        int a = (int)(da * 10);
                        int b = (int)(d * 10);
                        for (int k = a; k <= b; k++)
                        {
                            intlist.Add(k);
                        }
                    }//int入力
                    else if (datatype == 0)
                    {
                        int ia;
                        int.TryParse(txtB, out i);
                        int.TryParse(txtA, out ia);
                        for (int start = ia; start <= i; start++)
                        {
                            intlist.Add(start);
                        }
                    }
                    else
                    {
                        //エラー　数値入力のはずなのに文字列入力されている
                        ////ファイルの行番号取得           
                        string strMsg = GetLineNumber().ToString() + " 行目";

                        //メッセージボックスで行番号を表示
                        MessageBox.Show(strMsg
                                        , "入力値エラー"
                                        , MessageBoxButtons.OK
                                        , MessageBoxIcon.Information);
                        fl = false; break;
                    }

                    break;
                case 4:// <= memo １つレコード：
                    var record3 = new Record(txtCreateEWSID.Text);
                    record3.Score = score;
                    record3.VitalCode = VitalCodeName[vitalcode];
                    record3.CriteriaValue = txtB;
                    record3.CriteriaSign = 0;
                    record3.Target = tarval;
                    record3.DisplayOrder = displayorder;

                    RecList.Add(record3);

                    //入力制限用
                    //double
                    if (datatype == 1)
                    {
                        double da;
                        double.TryParse(txtB, out d);
                        double.TryParse(txtA, out da);
                        int a = (int)(da * 10);
                        int b = (int)(d * 10);
                        for (int k = a; k <= b; k++)
                        {
                            intlist.Add(k);
                        }
                    }//int入力
                    else if (datatype == 0)
                    {
                        int ia;
                        int.TryParse(txtB, out i);
                        int.TryParse(txtA, out ia);
                        for (int start = ia; start <= i; start++)
                        {
                            intlist.Add(start);
                        }
                    }
                    else
                    {
                        //エラー　数値入力のはずなのに文字列入力されている
                        ////ファイルの行番号取得           
                        string strMsg = GetLineNumber().ToString() + " 行目";

                        //メッセージボックスで行番号を表示
                        MessageBox.Show(strMsg
                                        , "入力値エラー"
                                        , MessageBoxButtons.OK
                                        , MessageBoxIcon.Information);
                        fl = false; break;
                    }

                    break;
                case 5:// >= memo １つレコード：
                    var record4 = new Record(txtCreateEWSID.Text);
                    record4.Score = score;
                    record4.VitalCode = VitalCodeName[vitalcode];
                    record4.CriteriaValue = txtB;
                    record4.CriteriaSign = 1;
                    record4.Target = tarval;
                    record4.DisplayOrder = displayorder;

                    RecList.Add(record4);

                    //入力制限用
                    //double
                    if (double.TryParse(txtB, out d) && txtB.Contains('.'))
                    {
                        intlist.Add((int)(d * 10));
                    }//int
                    else if (int.TryParse(txtB, out i))
                    {
                        intlist.Add(i);
                    }
                    else
                    {
                        //エラー　数値入力のはずなのに文字列入力されている
                        ////ファイルの行番号取得           
                        string strMsg = GetLineNumber().ToString() + " 行目";

                        //メッセージボックスで行番号を表示
                        MessageBox.Show(strMsg
                                        , "入力値エラー"
                                        , MessageBoxButtons.OK
                                        , MessageBoxIcon.Information);
                        fl = false; break;
                    }
                    break;
            }
            return fl;
        }

        /*private void CreatButton_Click(object sender, EventArgs e)
        {
            var RecordList = new List<Record>();
            bool check_input = true;
            Create_scorearray();
            //入力情報をレコードに変換する
            for (int i = 0; i < 10; i++)//iは行数 10の部分を定数にしたほうがきれいかも
            {
                var RecordList_Vital = new List<Record>();
                var intlist = new List<int>();

                for (int j = 0; j < 7; j++)//jは列数　７の部分を定数にしたほうがきれいかも
                {
                    if (_BtxtCriteiaValueA[i, j].Text == "" && _BtxtCriteiaValueB[i, j].Text == "")
                    {
                        continue;
                    }
                    else
                    {
                        check_input = CreatRecord(RecordList, intlist, _Bvitalcode[i].SelectedIndex, _BtxtCriteiaValueA[i, j].Text, _Bcmb[i, j].SelectedIndex, _BtxtCriteiaValueB[i, j].Text, _score[j], tarval[j], i + 1, _cmbDataTypeCRE[i].SelectedIndex);
                    }

                }

                //int double
                if (intlist.Count != 0)
                {
                    bool fl = true;
                    for (int s = 0; s < intlist.Count - 1; s++)
                    {
                        if (intlist[s + 1] - intlist[s] != 1)
                        {
                            fl = false; break;
                        }
                    }
                    //エラー処理
                    if (!fl)
                    {
                        MessageBox.Show("int error");
                    }
                }//string
                else
                {
                }
            }
        
    
            //EVISCloudに接続
            string constr = @"Data Source=192.168.1.174;Initial Catalog=EVISCloud;Integrated Security=False;User ID=sa;Password=P@ssw0rd";
            SqlConnection con = new SqlConnection(constr);
            con.Open();
            try
            {
                //M_EwsTypeへレコード登録
                string sqlstr = $"INSERT INTO M_EwsType (EwsName, WarningThresholds) VALUES('{txtCreateEwsName.Text}','{txtCreateWarningThresolds.Text}')";
                SqlCommand createEwsTypecom = new SqlCommand(sqlstr, con);
                var result1 = createEwsTypecom.ExecuteNonQuery();
                if (result1 == 0) MessageBox.Show("M_EwsType INSERT Failed in CreatButton_Click()");
                foreach (var record in RecordList)
                {
                    //test　テキストボックスに登録する情報を出力、確認用 shundbg
                    int EwsId = record.EWSId;
                    string VitalCode = record.VitalCode;
                    int Score = record.Score;
                    string CriteriaValue = record.CriteriaValue;
                    int CriteriaSign = record.CriteriaSign;
                    int Target = record.Target;
                    txtOutSql.Text += string.Format("{0} / {1} /{2} / {3} / {4} / {5} / {6}  \r\n", EwsId, VitalCode, Score, CriteriaValue, CriteriaSign, Target, record.DisplayOrder);
                    //test 

                    //T_EwsScoreCriteriaへレコード追加
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO T_EwsScoreCriteria(EwsId, SeqNo, VitalCode, Score, CriteriaValue, CriteriaSign, Target, DisplayOrder)");
                    //seqno=1固定でいいんじゃないか？新規追加だし
                    sb.Append($"VALUES( {record.EWSId},1, '{record.VitalCode}', {record.Score}, '{record.CriteriaValue}', {record.CriteriaSign}, {record.Target}, {record.DisplayOrder})");
                    SqlCommand com = new SqlCommand(sb.ToString(), con);
                    var result2 = com.ExecuteNonQuery();
                    if (result2 == 0) MessageBox.Show("T_EwsScoreCriteria to INSERT Failed in CreatButton_Click()");
                }
            }
            finally
            {
                MessageBox.Show("INSERT DONE");
                con.Close();
            }
            
            InitEwsName();
        }
        */

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
                EWSId = int.Parse(ID);
            }

        }
        /// <summary>
        /// DB->app スコア表示用関数 
        /// 変更　btnReadDB_Click -> ReadEwsScoreBoard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadEwsScoreBoard(object sender, EventArgs e)
        {
            var scoreset = new SortedSet<int>();
            scoreset.Add(0);
            var stackrecords = new List<Record>();
            int[] scoreindex = new int[4];
            //表に値が入力済みかチェック
            bool fl_Completed = false;
            for (int i= 0; i< 10; i++)
            {
                for(int j = 0; j<7; j++)
                {
                    if(_txtCriteiaValueA[i, j].Text != "" || _txtCriteiaValueB[i, j].Text != "")
                    {
                        fl_Completed = true;
                        break;
                    }
                }
                if (fl_Completed)
                {
                    ////ファイルの行番号取得           
                    string strMsg = "表に値が書き込まれています、更新しますか？";

                    //メッセージボックスで行番号を表示
                    var resultMessageBox = MessageBox.Show(strMsg
                                    , "エラー"
                                    , MessageBoxButtons.OKCancel
                                    , MessageBoxIcon.Information);

                    if (resultMessageBox == System.Windows.Forms.DialogResult.No)
                    {
                        return;
                    }
                    break;
                }
            }
            AllClear();
            //GetRecords初期化
            for (int i = 0; i < 11; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    GetRecords[i, j] = new List<Record>();
                }
            }

            if (cmbEwsName.SelectedIndex == -1)
            {
                ////ファイルの行番号取得           
                string strMsg = GetLineNumber().ToString() + " 行目";

                //メッセージボックスで行番号を表示
                MessageBox.Show(strMsg
                                , "エラー"
                                , MessageBoxButtons.OK
                                , MessageBoxIcon.Information);

                return;
            }
            //EwsNameに対応するEwsidのレコードを取得しtxtに出力する
            string constr = @"Data Source=192.168.1.174;Initial Catalog=EVISCloud;Integrated Security=False;User ID=sa;Password=P@ssw0rd";
            SqlConnection con = new SqlConnection(constr);
            con.Open();
            try
            {
                //SQL文作成:
                string sqlstr = $"SELECT * FROM T_EwsScoreCriteria WHERE EwsId = {EwsName[cmbEwsName.SelectedItem.ToString()]} AND InvalidFlag = 0";
                sqlstr += $"AND SeqNo = (SELECT MAX(SeqNo) FROM T_EwsScoreCriteria WHERE EwsId = {EwsName[cmbEwsName.SelectedItem.ToString()]})";

                //shundbg 引っ張ってくるのを3000に固定している
                //string sqlstr = $"SELECT * FROM T_EwsScoreCriteria WHERE EwsId =3000 AND InvalidFlag = 0";
                //sqlstr += $"AND SeqNo = (SELECT MAX(SeqNo) FROM T_EwsScoreCriteria WHERE EwsId = 3000)";
                //shundbg
                SqlCommand com = new SqlCommand(sqlstr, con);
                SqlDataReader sdr = com.ExecuteReader();

                txtOutSql.Text += "Get record -------------------------------------------\r\n";
                while (sdr.Read() == true)
                {
                    //EVIS                   
                    int EwsId = (int)sdr["EwsId"];
                    int SeqNo = (int)sdr["SeqNo"];
                    string VitalCode = (string)sdr["VitalCode"];
                    int Score = (int)sdr["Score"];
                    string CriteriaValue = (string)sdr["CriteriaValue"];
                    int CriteriaSign = (int)sdr["CriteriaSign"];
                    int Target = (int)sdr["Target"];
                    int Displayorder = (int)sdr["DisplayOrder"];

                    //setにScoreに使用されているレベル3種類を保存
                    scoreset.Add(Score);

                    var record = new Record(EwsId.ToString());
                    record.EWSId = EwsId;
                    record.SeqNo = SeqNo;
                    record.VitalCode = VitalCode;
                    record.Score = Score;
                    record.CriteriaValue = CriteriaValue;
                    record.CriteriaSign = CriteriaSign;
                    record.Target = Target;
                    record.DisplayOrder = Displayorder;

                    //EWSID,SeqNo保存 
                    EWSID.Text = EwsId.ToString();
                    txtSeqNo.Text = SeqNo.ToString();
                    //各VitalTytpeのDisplayOrderを求める
                    if (DicVitalcodeandDisplayOrder.ContainsKey(VitalCode))
                    {
                        if (DicVitalcodeandDisplayOrder[VitalCode] <= Displayorder)
                        {
                            DicVitalcodeandDisplayOrder[VitalCode] = Displayorder;
                        }
                    }
                    else
                    {
                        DicVitalcodeandDisplayOrder.Add(VitalCode, Displayorder);
                    }
                    stackrecords.Add(record);
                }
                sdr.Close();
                com.Dispose();
            }
            finally
            {
                con.Close();
            }

            //setにある要素からtxtScoreLvを更新する
            int cnt = scoreset.Count();
            IOrderedEnumerable<int> result = scoreset.OrderByDescending(x =>x);
            if(4 < cnt)
            {
                //エラー
            }
            else
            {
                foreach (var score in result)
                {
                    //ここの変更で_score[]の値も変更される？
                    switch (cnt)
                    {
                        case 2:
                            cnt--;
                            txtScoreLv1L.Text = score.ToString();
                            break;
                        case 3:
                            cnt--;
                            txtScoreLv2L.Text = score.ToString();
                            break;
                        case 4:
                            cnt--;
                            txtScoreLv3L.Text = score.ToString();
                            break;
                        default:
                            break;

                    }
                }
            }

            //
            scoreindex = scoreset.ToArray();
            foreach(var record in stackrecords)
            {
                if (record.Target == 0)
                {
                    GetRecords[record.DisplayOrder, 3 - (Array.IndexOf(scoreindex, record.Score))].Add(record);

                }
                else
                {
                    GetRecords[record.DisplayOrder, 3 + (Array.IndexOf(scoreindex, record.Score))].Add(record);
                }
            }



            //GetRecords[0][]に入ってしまった分をvitalcodeをもとに1<=iに振り分ける
            for (int i = 0; i < 7; i++)
            {
                foreach (var record in GetRecords[0, i])
                {
                    GetRecords[DicVitalcodeandDisplayOrder[record.VitalCode], i].Add(record);
                }
            }

            //******修正いる？！！二重forするようなこと？？？？->record配列のどこに要素が入っているかわからないから２重で回すしかない。フラグで早くぬけてるからそんな回してるわけでもない
            //cmbvitalcodeを更新
            for (int i = 0; i < 10; i++)
            {
                //cmbvitalcode クリアする
                _vitalcode[i].Items.Clear();
                bool fl = false;
                //Vitalcode追加,選択
                for (int j = 0; j < 7; j++)
                {
                    if (GetRecords[i + 1, j].Count != 0)
                    {
                        //Getrecordsにはi=1から実データが入ってる
                        _vitalcode[i].Items.Add(GetRecords[i + 1, j].First().VitalCode);
                        _vitalcode[i].SelectedIndex = 0;
                        fl = true;
                    }
                    if (fl) break;
                }

            }

            //shundbg 振り分けたレコードが正しいか確認
            for (int i = 1; i < 11; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    foreach (var s in GetRecords[i, j])
                    {
                        OutRecord(s);
                    }
                    txtOutSql.Text += "|||||||||||||||||||||||||||\r\n";
                }
                txtOutSql.Text += "*******************\r\n";
            }
            //shundbg

            //レコード表示処理
            OutScore();
        }

        /// <summary>
        /// GetRecordsを表形式に表示
        /// </summary>
        private void OutScore()
        {

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    if (GetRecords[i + 1, j].Count() == 0)
                    {
                        continue;
                    }
                    else if (GetRecords[i + 1, j].First().CriteriaSign == 2)
                    {
                        //case 1 ","
                        if (GetRecords[i + 1, j].Count() >= 2)
                        {
                            foreach (var record in GetRecords[i + 1, j])
                            {
                                _txtCriteiaValueA[i, j].Text += record.CriteriaValue + ",";
                            }
                            _txtCriteiaValueA[i, j].Text = _txtCriteiaValueA[i, j].Text.Remove(_txtCriteiaValueA[i, j].Text.Length - 1);

                            _cmb[i, j].SelectedIndex = 2;
                        }
                        //case 0 " "
                        else
                        {
                            _txtCriteiaValueA[i, j].Text += GetRecords[i + 1, j].First().CriteriaValue;
                            _cmb[i, j].SelectedIndex = 1;
                        }
                    }
                    //case2 "～"
                    else if (GetRecords[i + 1, j].Count() == 2)
                    {
                        foreach (var record in GetRecords[i + 1, j])
                        {
                            if (record.CriteriaSign == 1)
                            {
                                _txtCriteiaValueA[i, j].Text += record.CriteriaValue;
                            }
                            else
                            {
                                _txtCriteiaValueB[i, j].Text += record.CriteriaValue;
                            }
                        }
                        _cmb[i, j].SelectedIndex = 3;
                    }
                    //case4,5 "<=" or ">="
                    else if (GetRecords[i + 1, j].Count() == 1)
                    {
                        if (GetRecords[i + 1, j].First().CriteriaSign == 0)
                        {
                            _txtCriteiaValueB[i, j].Text += GetRecords[i + 1, j].First().CriteriaValue;
                            _cmb[i, j].SelectedIndex = 4;
                        }
                        else
                        {
                            _txtCriteiaValueB[i, j].Text += GetRecords[i + 1, j].First().CriteriaValue;
                            _cmb[i, j].SelectedIndex = 5;
                        }
                    }
                    //error
                    else
                    {
                        txtOutSql.Text += string.Format($"OutScore error {i} {j} \r\n");
                    }
                }
            }
        }
        /// <summary>
        /// txtboxに表示テスト用
        /// </summary>
        /// <param name="record"></param>
        private void OutRecord(Record record)
        {
            string outstr = "";

            outstr += string.Format($"EwsId:{record.EWSId}, SeqNo:{record.SeqNo}, VitalCode:{record.VitalCode}, Score:{record.Score}, CriteriaValue:{record.CriteriaValue}, CriteriaSign:{record.CriteriaSign}, Target:{record.Target}, DisplayOrder:{record.DisplayOrder}  \r\n");
            txtOutSql.Text += outstr;
        }

        /// <summary>
        /// 起動時EwsNameコンボボックスにＤＢからEwsName一覧を取ってきて設定
        /// </summary>
        private void InitEwsName()
        {
            EwsName.Clear();
            cmbEwsName.Items.Clear();

            string constr = @"Data Source=192.168.1.174;Initial Catalog=EVISCloud;Integrated Security=False;User ID=sa;Password=P@ssw0rd";

            SqlConnection con = new SqlConnection(constr);
            con.Open();
            try
            {
                string sqlstr = "SELECT Id, EwsName  FROM M_Ewstype WHERE LogicalDeleted = 0";
                SqlCommand com = new SqlCommand(sqlstr, con);
                SqlDataReader sdr = com.ExecuteReader();

                while (sdr.Read() == true)
                {
                    EwsName.Add((string)sdr["EwsName"], (int)sdr["Id"]);
                    string str = (string)sdr["EwsName"];
                    txtOutSql.Text += string.Format($"name{str},id{EwsName[str]} \r\n");
                    cmbEwsName.Items.Add(str);
                }
                sdr.Close();
                com.Dispose();
            }
            finally
            {
                con.Close();
            }
        }
        /// <summary>
        /// VitalName,各記号の初期化
        /// </summary>
        private void InitComboBox()
        {
            for (int i = 0; i < 10; i++)
            {
                //A,Bの間のコンボボックス初期化
                for (int j = 0; j < 7; j++)
                {
                    _cmb[i, j].Items.Clear();
                    _cmb[i, j].Items.Add("");
                    _cmb[i, j].Items.Add("=");
                    _cmb[i, j].Items.Add(",");
                    _cmb[i, j].Items.Add("～");
                    _cmb[i, j].Items.Add("≦");
                    _cmb[i, j].Items.Add("≧");
                    _cmb[i, j].SelectedIndex = 0;
                    /*_Bcmb[i, j].Items.Clear(); shundbg
                    _Bcmb[i, j].Items.Add("");
                    _Bcmb[i, j].Items.Add("=");
                    _Bcmb[i, j].Items.Add(",");
                    _Bcmb[i, j].Items.Add("～");
                    _Bcmb[i, j].Items.Add("≦");
                    _Bcmb[i, j].Items.Add("≧");
                    _Bcmb[i, j].SelectedIndex = 0;
                    */
                }

                _vitalcode[i].SelectedIndex = 0;
                _vitalcode[i].Items.Clear();
                
                /*shundbg
                _Bvitalcode[i].SelectedIndex = 0;
                _Bvitalcode[i].Items.Clear();
                */



                string[] VitalTypeFile = File.ReadAllLines("..\\..\\VitalType.txt");
                var splits = new List<string>();
                int ele = 0;
                foreach (string s in VitalTypeFile)
                {
                    _vitalcode[i].Items.Add(s);
                    //shundbg_Bvitalcode[i].Items.Add(s);
                    ele++;
                }
                //配列に情報を書き込みたいだけなので一度だけの実行でいい
                if (i == 0)
                {
                    //VitalCodeName[0]="..."
                    VitalCodeName.Add("...");
                    //コンボボックス_vitalcodeの要素からVitalNameを抜き出す
                    foreach (string word in _vitalcode[i].Items)
                    {
                        //()に囲まれた文字列をサーチ
                        var Matches = new Regex(@"\((.+?)\)").Matches(word);
                        //VitalCodeName[]に追加
                        foreach (var word2 in Matches)
                        {
                            string s = word2.ToString();
                            //抜き出した文字列が(****)となっているため()を外す
                            string[] charDelete = new string[] { "(", ")" };
                            foreach (var c in charDelete)
                            {
                                s = s.Replace(c, "");
                            }
                            //VitalNameを追加
                            VitalCodeName.Add(s);
                        }

                    }
                }
            }
            //shundbg
            foreach (string s in VitalCodeName)
            {
                txtOutSql.Text += s + "\r\n";
            }
        }

        /// <summary>
        /// コントロールを設定
        /// </summary>
        private void InitControl()
        {
            _txtCriteiaValueA = new TextBox[10, 7];//vitaltype, score
            _txtCriteiaValueB = new TextBox[10, 7];//vitaltype, score
            _cmb = new ComboBox[10, 7];//vitaltype, score
            _vitalcode = new ComboBox[10];

            _BtxtCriteiaValueA = new TextBox[10, 7];//vitaltype, score
            _BtxtCriteiaValueB = new TextBox[10, 7];//vitaltype, score
            _Bcmb = new ComboBox[10, 7];//vitaltype, score
            _Bvitalcode = new ComboBox[10];

            _cmbDataTypeUP = new ComboBox[10];
            _cmbDataTypeCRE = new ComboBox[10];

            //_ScoreLv = new TextBox[7];

            //A 1行目
            _txtCriteiaValueA[0, 0] = txtCriteiaValue11A;
            _txtCriteiaValueA[0, 1] = txtCriteiaValue12A;
            _txtCriteiaValueA[0, 2] = txtCriteiaValue13A;
            _txtCriteiaValueA[0, 3] = txtCriteiaValue14A;
            _txtCriteiaValueA[0, 4] = txtCriteiaValue15A;
            _txtCriteiaValueA[0, 5] = txtCriteiaValue16A;
            _txtCriteiaValueA[0, 6] = txtCriteiaValue17A;
            //A 2行目
            _txtCriteiaValueA[1, 0] = txtCriteiaValue21A;
            _txtCriteiaValueA[1, 1] = txtCriteiaValue22A;
            _txtCriteiaValueA[1, 2] = txtCriteiaValue23A;
            _txtCriteiaValueA[1, 3] = txtCriteiaValue24A;
            _txtCriteiaValueA[1, 4] = txtCriteiaValue25A;
            _txtCriteiaValueA[1, 5] = txtCriteiaValue26A;
            _txtCriteiaValueA[1, 6] = txtCriteiaValue27A;
            //A 3行目
            _txtCriteiaValueA[2, 0] = txtCriteiaValue31A;
            _txtCriteiaValueA[2, 1] = txtCriteiaValue32A;
            _txtCriteiaValueA[2, 2] = txtCriteiaValue33A;
            _txtCriteiaValueA[2, 3] = txtCriteiaValue34A;
            _txtCriteiaValueA[2, 4] = txtCriteiaValue35A;
            _txtCriteiaValueA[2, 5] = txtCriteiaValue36A;
            _txtCriteiaValueA[2, 6] = txtCriteiaValue37A;
            //A 4行目
            _txtCriteiaValueA[3, 0] = txtCriteiaValue41A;
            _txtCriteiaValueA[3, 1] = txtCriteiaValue42A;
            _txtCriteiaValueA[3, 2] = txtCriteiaValue43A;
            _txtCriteiaValueA[3, 3] = txtCriteiaValue44A;
            _txtCriteiaValueA[3, 4] = txtCriteiaValue45A;
            _txtCriteiaValueA[3, 5] = txtCriteiaValue46A;
            _txtCriteiaValueA[3, 6] = txtCriteiaValue47A;
            //A 5行目
            _txtCriteiaValueA[4, 0] = txtCriteiaValue51A;
            _txtCriteiaValueA[4, 1] = txtCriteiaValue52A;
            _txtCriteiaValueA[4, 2] = txtCriteiaValue53A;
            _txtCriteiaValueA[4, 3] = txtCriteiaValue54A;
            _txtCriteiaValueA[4, 4] = txtCriteiaValue55A;
            _txtCriteiaValueA[4, 5] = txtCriteiaValue56A;
            _txtCriteiaValueA[4, 6] = txtCriteiaValue57A;
            //A 6行目
            _txtCriteiaValueA[5, 0] = txtCriteiaValue61A;
            _txtCriteiaValueA[5, 1] = txtCriteiaValue62A;
            _txtCriteiaValueA[5, 2] = txtCriteiaValue63A;
            _txtCriteiaValueA[5, 3] = txtCriteiaValue64A;
            _txtCriteiaValueA[5, 4] = txtCriteiaValue65A;
            _txtCriteiaValueA[5, 5] = txtCriteiaValue66A;
            _txtCriteiaValueA[5, 6] = txtCriteiaValue67A;
            //A 7行目
            _txtCriteiaValueA[6, 0] = txtCriteiaValue71A;
            _txtCriteiaValueA[6, 1] = txtCriteiaValue72A;
            _txtCriteiaValueA[6, 2] = txtCriteiaValue73A;
            _txtCriteiaValueA[6, 3] = txtCriteiaValue74A;
            _txtCriteiaValueA[6, 4] = txtCriteiaValue75A;
            _txtCriteiaValueA[6, 5] = txtCriteiaValue76A;
            _txtCriteiaValueA[6, 6] = txtCriteiaValue77A;
            //A 8行目
            _txtCriteiaValueA[7, 0] = txtCriteiaValue81A;
            _txtCriteiaValueA[7, 1] = txtCriteiaValue82A;
            _txtCriteiaValueA[7, 2] = txtCriteiaValue83A;
            _txtCriteiaValueA[7, 3] = txtCriteiaValue84A;
            _txtCriteiaValueA[7, 4] = txtCriteiaValue85A;
            _txtCriteiaValueA[7, 5] = txtCriteiaValue86A;
            _txtCriteiaValueA[7, 6] = txtCriteiaValue87A;
            //A 9行目
            _txtCriteiaValueA[8, 0] = txtCriteiaValue91A;
            _txtCriteiaValueA[8, 1] = txtCriteiaValue92A;
            _txtCriteiaValueA[8, 2] = txtCriteiaValue93A;
            _txtCriteiaValueA[8, 3] = txtCriteiaValue94A;
            _txtCriteiaValueA[8, 4] = txtCriteiaValue95A;
            _txtCriteiaValueA[8, 5] = txtCriteiaValue96A;
            _txtCriteiaValueA[8, 6] = txtCriteiaValue97A;
            //A 10行目
            _txtCriteiaValueA[9, 0] = txtCriteiaValue101A;
            _txtCriteiaValueA[9, 1] = txtCriteiaValue102A;
            _txtCriteiaValueA[9, 2] = txtCriteiaValue103A;
            _txtCriteiaValueA[9, 3] = txtCriteiaValue104A;
            _txtCriteiaValueA[9, 4] = txtCriteiaValue105A;
            _txtCriteiaValueA[9, 5] = txtCriteiaValue106A;
            _txtCriteiaValueA[9, 6] = txtCriteiaValue107A;


            //B 1行目
            _txtCriteiaValueB[0, 0] = txtCriteiaValue11B;
            _txtCriteiaValueB[0, 1] = txtCriteiaValue12B;
            _txtCriteiaValueB[0, 2] = txtCriteiaValue13B;
            _txtCriteiaValueB[0, 3] = txtCriteiaValue14B;
            _txtCriteiaValueB[0, 4] = txtCriteiaValue15B;
            _txtCriteiaValueB[0, 5] = txtCriteiaValue16B;
            _txtCriteiaValueB[0, 6] = txtCriteiaValue17B;
            //B 2行目
            _txtCriteiaValueB[1, 0] = txtCriteiaValue21B;
            _txtCriteiaValueB[1, 1] = txtCriteiaValue22B;
            _txtCriteiaValueB[1, 2] = txtCriteiaValue23B;
            _txtCriteiaValueB[1, 3] = txtCriteiaValue24B;
            _txtCriteiaValueB[1, 4] = txtCriteiaValue25B;
            _txtCriteiaValueB[1, 5] = txtCriteiaValue26B;
            _txtCriteiaValueB[1, 6] = txtCriteiaValue27B;
            //B 3行目
            _txtCriteiaValueB[2, 0] = txtCriteiaValue31B;
            _txtCriteiaValueB[2, 1] = txtCriteiaValue32B;
            _txtCriteiaValueB[2, 2] = txtCriteiaValue33B;
            _txtCriteiaValueB[2, 3] = txtCriteiaValue34B;
            _txtCriteiaValueB[2, 4] = txtCriteiaValue35B;
            _txtCriteiaValueB[2, 5] = txtCriteiaValue36B;
            _txtCriteiaValueB[2, 6] = txtCriteiaValue37B;
            //B 4行目
            _txtCriteiaValueB[3, 0] = txtCriteiaValue41B;
            _txtCriteiaValueB[3, 1] = txtCriteiaValue42B;
            _txtCriteiaValueB[3, 2] = txtCriteiaValue43B;
            _txtCriteiaValueB[3, 3] = txtCriteiaValue44B;
            _txtCriteiaValueB[3, 4] = txtCriteiaValue45B;
            _txtCriteiaValueB[3, 5] = txtCriteiaValue46B;
            _txtCriteiaValueB[3, 6] = txtCriteiaValue47B;
            //B 5行目
            _txtCriteiaValueB[4, 0] = txtCriteiaValue51B;
            _txtCriteiaValueB[4, 1] = txtCriteiaValue52B;
            _txtCriteiaValueB[4, 2] = txtCriteiaValue53B;
            _txtCriteiaValueB[4, 3] = txtCriteiaValue54B;
            _txtCriteiaValueB[4, 4] = txtCriteiaValue55B;
            _txtCriteiaValueB[4, 5] = txtCriteiaValue56B;
            _txtCriteiaValueB[4, 6] = txtCriteiaValue57B;
            //B 6行目
            _txtCriteiaValueB[5, 0] = txtCriteiaValue61B;
            _txtCriteiaValueB[5, 1] = txtCriteiaValue62B;
            _txtCriteiaValueB[5, 2] = txtCriteiaValue63B;
            _txtCriteiaValueB[5, 3] = txtCriteiaValue64B;
            _txtCriteiaValueB[5, 4] = txtCriteiaValue65B;
            _txtCriteiaValueB[5, 5] = txtCriteiaValue66B;
            _txtCriteiaValueB[5, 6] = txtCriteiaValue67B;
            //B 7行目
            _txtCriteiaValueB[6, 0] = txtCriteiaValue71B;
            _txtCriteiaValueB[6, 1] = txtCriteiaValue72B;
            _txtCriteiaValueB[6, 2] = txtCriteiaValue73B;
            _txtCriteiaValueB[6, 3] = txtCriteiaValue74B;
            _txtCriteiaValueB[6, 4] = txtCriteiaValue75B;
            _txtCriteiaValueB[6, 5] = txtCriteiaValue76B;
            _txtCriteiaValueB[6, 6] = txtCriteiaValue77B;
            //B 8行目
            _txtCriteiaValueB[7, 0] = txtCriteiaValue81B;
            _txtCriteiaValueB[7, 1] = txtCriteiaValue82B;
            _txtCriteiaValueB[7, 2] = txtCriteiaValue83B;
            _txtCriteiaValueB[7, 3] = txtCriteiaValue84B;
            _txtCriteiaValueB[7, 4] = txtCriteiaValue85B;
            _txtCriteiaValueB[7, 5] = txtCriteiaValue86B;
            _txtCriteiaValueB[7, 6] = txtCriteiaValue87B;
            //B 9行目
            _txtCriteiaValueB[8, 0] = txtCriteiaValue91B;
            _txtCriteiaValueB[8, 1] = txtCriteiaValue92B;
            _txtCriteiaValueB[8, 2] = txtCriteiaValue93B;
            _txtCriteiaValueB[8, 3] = txtCriteiaValue94B;
            _txtCriteiaValueB[8, 4] = txtCriteiaValue95B;
            _txtCriteiaValueB[8, 5] = txtCriteiaValue96B;
            _txtCriteiaValueB[8, 6] = txtCriteiaValue97B;
            //B 10行目
            _txtCriteiaValueB[9, 0] = txtCriteiaValue101B;
            _txtCriteiaValueB[9, 1] = txtCriteiaValue102B;
            _txtCriteiaValueB[9, 2] = txtCriteiaValue103B;
            _txtCriteiaValueB[9, 3] = txtCriteiaValue104B;
            _txtCriteiaValueB[9, 4] = txtCriteiaValue105B;
            _txtCriteiaValueB[9, 5] = txtCriteiaValue106B;
            _txtCriteiaValueB[9, 6] = txtCriteiaValue107B;

            //cmb 1行目
            _cmb[0, 0] = cmb11;
            _cmb[0, 1] = cmb12;
            _cmb[0, 2] = cmb13;
            _cmb[0, 3] = cmb14;
            _cmb[0, 4] = cmb15;
            _cmb[0, 5] = cmb16;
            _cmb[0, 6] = cmb17;
            //cmb 2行目
            _cmb[1, 0] = cmb21;
            _cmb[1, 1] = cmb22;
            _cmb[1, 2] = cmb23;
            _cmb[1, 3] = cmb24;
            _cmb[1, 4] = cmb25;
            _cmb[1, 5] = cmb26;
            _cmb[1, 6] = cmb27;
            //cmb 3行目
            _cmb[2, 0] = cmb31;
            _cmb[2, 1] = cmb32;
            _cmb[2, 2] = cmb33;
            _cmb[2, 3] = cmb34;
            _cmb[2, 4] = cmb35;
            _cmb[2, 5] = cmb36;
            _cmb[2, 6] = cmb37;
            //cmb 4行目
            _cmb[3, 0] = cmb41;
            _cmb[3, 1] = cmb42;
            _cmb[3, 2] = cmb43;
            _cmb[3, 3] = cmb44;
            _cmb[3, 4] = cmb45;
            _cmb[3, 5] = cmb46;
            _cmb[3, 6] = cmb47;
            //cmb 5行目
            _cmb[4, 0] = cmb51;
            _cmb[4, 1] = cmb52;
            _cmb[4, 2] = cmb53;
            _cmb[4, 3] = cmb54;
            _cmb[4, 4] = cmb55;
            _cmb[4, 5] = cmb56;
            _cmb[4, 6] = cmb57;
            //cmb 6行目
            _cmb[5, 0] = cmb61;
            _cmb[5, 1] = cmb62;
            _cmb[5, 2] = cmb63;
            _cmb[5, 3] = cmb64;
            _cmb[5, 4] = cmb65;
            _cmb[5, 5] = cmb66;
            _cmb[5, 6] = cmb67;
            //cmb 7行目
            _cmb[6, 0] = cmb71;
            _cmb[6, 1] = cmb72;
            _cmb[6, 2] = cmb73;
            _cmb[6, 3] = cmb74;
            _cmb[6, 4] = cmb75;
            _cmb[6, 5] = cmb76;
            _cmb[6, 6] = cmb77;
            //cmb 8行目
            _cmb[7, 0] = cmb81;
            _cmb[7, 1] = cmb82;
            _cmb[7, 2] = cmb83;
            _cmb[7, 3] = cmb84;
            _cmb[7, 4] = cmb85;
            _cmb[7, 5] = cmb86;
            _cmb[7, 6] = cmb87;
            //cmb 9行目
            _cmb[8, 0] = cmb91;
            _cmb[8, 1] = cmb92;
            _cmb[8, 2] = cmb93;
            _cmb[8, 3] = cmb94;
            _cmb[8, 4] = cmb95;
            _cmb[8, 5] = cmb96;
            _cmb[8, 6] = cmb97;
            //cmb 10行目
            _cmb[9, 0] = cmb101;
            _cmb[9, 1] = cmb102;
            _cmb[9, 2] = cmb103;
            _cmb[9, 3] = cmb104;
            _cmb[9, 4] = cmb105;
            _cmb[9, 5] = cmb106;
            _cmb[9, 6] = cmb107;


            //cmbvital
            _vitalcode[0] = cmbVitalCode1;
            _vitalcode[1] = cmbVitalCode2;
            _vitalcode[2] = cmbVitalCode3;
            _vitalcode[3] = cmbVitalCode4;
            _vitalcode[4] = cmbVitalCode5;
            _vitalcode[5] = cmbVitalCode6;
            _vitalcode[6] = cmbVitalCode7;
            _vitalcode[7] = cmbVitalCode8;
            _vitalcode[8] = cmbVitalCode9;
            _vitalcode[9] = cmbVitalCode10;



            /* shundbg
             * //BControl----------------------------------------------------------
            //A 1行目
            _BtxtCriteiaValueA[0, 0] = BtxtCriteiaValue11A;
            _BtxtCriteiaValueA[0, 1] = BtxtCriteiaValue12A;
            _BtxtCriteiaValueA[0, 2] = BtxtCriteiaValue13A;
            _BtxtCriteiaValueA[0, 3] = BtxtCriteiaValue14A;
            _BtxtCriteiaValueA[0, 4] = BtxtCriteiaValue15A;
            _BtxtCriteiaValueA[0, 5] = BtxtCriteiaValue16A;
            _BtxtCriteiaValueA[0, 6] = BtxtCriteiaValue17A;
            //A 2行目
            _BtxtCriteiaValueA[1, 0] = BtxtCriteiaValue21A;
            _BtxtCriteiaValueA[1, 1] = BtxtCriteiaValue22A;
            _BtxtCriteiaValueA[1, 2] = BtxtCriteiaValue23A;
            _BtxtCriteiaValueA[1, 3] = BtxtCriteiaValue24A;
            _BtxtCriteiaValueA[1, 4] = BtxtCriteiaValue25A;
            _BtxtCriteiaValueA[1, 5] = BtxtCriteiaValue26A;
            _BtxtCriteiaValueA[1, 6] = BtxtCriteiaValue27A;
            //A 3行目
            _BtxtCriteiaValueA[2, 0] = BtxtCriteiaValue31A;
            _BtxtCriteiaValueA[2, 1] = BtxtCriteiaValue32A;
            _BtxtCriteiaValueA[2, 2] = BtxtCriteiaValue33A;
            _BtxtCriteiaValueA[2, 3] = BtxtCriteiaValue34A;
            _BtxtCriteiaValueA[2, 4] = BtxtCriteiaValue35A;
            _BtxtCriteiaValueA[2, 5] = BtxtCriteiaValue36A;
            _BtxtCriteiaValueA[2, 6] = BtxtCriteiaValue37A;
            //A 4行目
            _BtxtCriteiaValueA[3, 0] = BtxtCriteiaValue41A;
            _BtxtCriteiaValueA[3, 1] = BtxtCriteiaValue42A;
            _BtxtCriteiaValueA[3, 2] = BtxtCriteiaValue43A;
            _BtxtCriteiaValueA[3, 3] = BtxtCriteiaValue44A;
            _BtxtCriteiaValueA[3, 4] = BtxtCriteiaValue45A;
            _BtxtCriteiaValueA[3, 5] = BtxtCriteiaValue46A;
            _BtxtCriteiaValueA[3, 6] = BtxtCriteiaValue47A;
            //A 5行目
            _BtxtCriteiaValueA[4, 0] = BtxtCriteiaValue51A;
            _BtxtCriteiaValueA[4, 1] = BtxtCriteiaValue52A;
            _BtxtCriteiaValueA[4, 2] = BtxtCriteiaValue53A;
            _BtxtCriteiaValueA[4, 3] = BtxtCriteiaValue54A;
            _BtxtCriteiaValueA[4, 4] = BtxtCriteiaValue55A;
            _BtxtCriteiaValueA[4, 5] = BtxtCriteiaValue56A;
            _BtxtCriteiaValueA[4, 6] = BtxtCriteiaValue57A;
            //A 6行目
            _BtxtCriteiaValueA[5, 0] = BtxtCriteiaValue61A;
            _BtxtCriteiaValueA[5, 1] = BtxtCriteiaValue62A;
            _BtxtCriteiaValueA[5, 2] = BtxtCriteiaValue63A;
            _BtxtCriteiaValueA[5, 3] = BtxtCriteiaValue64A;
            _BtxtCriteiaValueA[5, 4] = BtxtCriteiaValue65A;
            _BtxtCriteiaValueA[5, 5] = BtxtCriteiaValue66A;
            _BtxtCriteiaValueA[5, 6] = BtxtCriteiaValue67A;
            //A 7行目
            _BtxtCriteiaValueA[6, 0] = BtxtCriteiaValue71A;
            _BtxtCriteiaValueA[6, 1] = BtxtCriteiaValue72A;
            _BtxtCriteiaValueA[6, 2] = BtxtCriteiaValue73A;
            _BtxtCriteiaValueA[6, 3] = BtxtCriteiaValue74A;
            _BtxtCriteiaValueA[6, 4] = BtxtCriteiaValue75A;
            _BtxtCriteiaValueA[6, 5] = BtxtCriteiaValue76A;
            _BtxtCriteiaValueA[6, 6] = BtxtCriteiaValue77A;
            //A 8行目
            _BtxtCriteiaValueA[7, 0] = BtxtCriteiaValue81A;
            _BtxtCriteiaValueA[7, 1] = BtxtCriteiaValue82A;
            _BtxtCriteiaValueA[7, 2] = BtxtCriteiaValue83A;
            _BtxtCriteiaValueA[7, 3] = BtxtCriteiaValue84A;
            _BtxtCriteiaValueA[7, 4] = BtxtCriteiaValue85A;
            _BtxtCriteiaValueA[7, 5] = BtxtCriteiaValue86A;
            _BtxtCriteiaValueA[7, 6] = BtxtCriteiaValue87A;
            //A 9行目
            _BtxtCriteiaValueA[8, 0] = BtxtCriteiaValue91A;
            _BtxtCriteiaValueA[8, 1] = BtxtCriteiaValue92A;
            _BtxtCriteiaValueA[8, 2] = BtxtCriteiaValue93A;
            _BtxtCriteiaValueA[8, 3] = BtxtCriteiaValue94A;
            _BtxtCriteiaValueA[8, 4] = BtxtCriteiaValue95A;
            _BtxtCriteiaValueA[8, 5] = BtxtCriteiaValue96A;
            _BtxtCriteiaValueA[8, 6] = BtxtCriteiaValue97A;
            //A 10行目
            _BtxtCriteiaValueA[9, 0] = BtxtCriteiaValue101A;
            _BtxtCriteiaValueA[9, 1] = BtxtCriteiaValue102A;
            _BtxtCriteiaValueA[9, 2] = BtxtCriteiaValue103A;
            _BtxtCriteiaValueA[9, 3] = BtxtCriteiaValue104A;
            _BtxtCriteiaValueA[9, 4] = BtxtCriteiaValue105A;
            _BtxtCriteiaValueA[9, 5] = BtxtCriteiaValue106A;
            _BtxtCriteiaValueA[9, 6] = BtxtCriteiaValue107A;


            //B 1行目
            _BtxtCriteiaValueB[0, 0] = BtxtCriteiaValue11B;
            _BtxtCriteiaValueB[0, 1] = BtxtCriteiaValue12B;
            _BtxtCriteiaValueB[0, 2] = BtxtCriteiaValue13B;
            _BtxtCriteiaValueB[0, 3] = BtxtCriteiaValue14B;
            _BtxtCriteiaValueB[0, 4] = BtxtCriteiaValue15B;
            _BtxtCriteiaValueB[0, 5] = BtxtCriteiaValue16B;
            _BtxtCriteiaValueB[0, 6] = BtxtCriteiaValue17B;
            //B 2行目
            _BtxtCriteiaValueB[1, 0] = BtxtCriteiaValue21B;
            _BtxtCriteiaValueB[1, 1] = BtxtCriteiaValue22B;
            _BtxtCriteiaValueB[1, 2] = BtxtCriteiaValue23B;
            _BtxtCriteiaValueB[1, 3] = BtxtCriteiaValue24B;
            _BtxtCriteiaValueB[1, 4] = BtxtCriteiaValue25B;
            _BtxtCriteiaValueB[1, 5] = BtxtCriteiaValue26B;
            _BtxtCriteiaValueB[1, 6] = BtxtCriteiaValue27B;
            //B 3行目
            _BtxtCriteiaValueB[2, 0] = BtxtCriteiaValue31B;
            _BtxtCriteiaValueB[2, 1] = BtxtCriteiaValue32B;
            _BtxtCriteiaValueB[2, 2] = BtxtCriteiaValue33B;
            _BtxtCriteiaValueB[2, 3] = BtxtCriteiaValue34B;
            _BtxtCriteiaValueB[2, 4] = BtxtCriteiaValue35B;
            _BtxtCriteiaValueB[2, 5] = BtxtCriteiaValue36B;
            _BtxtCriteiaValueB[2, 6] = BtxtCriteiaValue37B;
            //B 4行目
            _BtxtCriteiaValueB[3, 0] = BtxtCriteiaValue41B;
            _BtxtCriteiaValueB[3, 1] = BtxtCriteiaValue42B;
            _BtxtCriteiaValueB[3, 2] = BtxtCriteiaValue43B;
            _BtxtCriteiaValueB[3, 3] = BtxtCriteiaValue44B;
            _BtxtCriteiaValueB[3, 4] = BtxtCriteiaValue45B;
            _BtxtCriteiaValueB[3, 5] = BtxtCriteiaValue46B;
            _BtxtCriteiaValueB[3, 6] = BtxtCriteiaValue47B;
            //B 5行目
            _BtxtCriteiaValueB[4, 0] = BtxtCriteiaValue51B;
            _BtxtCriteiaValueB[4, 1] = BtxtCriteiaValue52B;
            _BtxtCriteiaValueB[4, 2] = BtxtCriteiaValue53B;
            _BtxtCriteiaValueB[4, 3] = BtxtCriteiaValue54B;
            _BtxtCriteiaValueB[4, 4] = BtxtCriteiaValue55B;
            _BtxtCriteiaValueB[4, 5] = BtxtCriteiaValue56B;
            _BtxtCriteiaValueB[4, 6] = BtxtCriteiaValue57B;
            //B 6行目
            _BtxtCriteiaValueB[5, 0] = BtxtCriteiaValue61B;
            _BtxtCriteiaValueB[5, 1] = BtxtCriteiaValue62B;
            _BtxtCriteiaValueB[5, 2] = BtxtCriteiaValue63B;
            _BtxtCriteiaValueB[5, 3] = BtxtCriteiaValue64B;
            _BtxtCriteiaValueB[5, 4] = BtxtCriteiaValue65B;
            _BtxtCriteiaValueB[5, 5] = BtxtCriteiaValue66B;
            _BtxtCriteiaValueB[5, 6] = BtxtCriteiaValue67B;
            //B 7行目
            _BtxtCriteiaValueB[6, 0] = BtxtCriteiaValue71B;
            _BtxtCriteiaValueB[6, 1] = BtxtCriteiaValue72B;
            _BtxtCriteiaValueB[6, 2] = BtxtCriteiaValue73B;
            _BtxtCriteiaValueB[6, 3] = BtxtCriteiaValue74B;
            _BtxtCriteiaValueB[6, 4] = BtxtCriteiaValue75B;
            _BtxtCriteiaValueB[6, 5] = BtxtCriteiaValue76B;
            _BtxtCriteiaValueB[6, 6] = BtxtCriteiaValue77B;
            //B 8行目
            _BtxtCriteiaValueB[7, 0] = BtxtCriteiaValue81B;
            _BtxtCriteiaValueB[7, 1] = BtxtCriteiaValue82B;
            _BtxtCriteiaValueB[7, 2] = BtxtCriteiaValue83B;
            _BtxtCriteiaValueB[7, 3] = BtxtCriteiaValue84B;
            _BtxtCriteiaValueB[7, 4] = BtxtCriteiaValue85B;
            _BtxtCriteiaValueB[7, 5] = BtxtCriteiaValue86B;
            _BtxtCriteiaValueB[7, 6] = BtxtCriteiaValue87B;
            //B 9行目
            _BtxtCriteiaValueB[8, 0] = BtxtCriteiaValue91B;
            _BtxtCriteiaValueB[8, 1] = BtxtCriteiaValue92B;
            _BtxtCriteiaValueB[8, 2] = BtxtCriteiaValue93B;
            _BtxtCriteiaValueB[8, 3] = BtxtCriteiaValue94B;
            _BtxtCriteiaValueB[8, 4] = BtxtCriteiaValue95B;
            _BtxtCriteiaValueB[8, 5] = BtxtCriteiaValue96B;
            _BtxtCriteiaValueB[8, 6] = BtxtCriteiaValue97B;
            //B 10行目
            _BtxtCriteiaValueB[9, 0] = BtxtCriteiaValue101B;
            _BtxtCriteiaValueB[9, 1] = BtxtCriteiaValue102B;
            _BtxtCriteiaValueB[9, 2] = BtxtCriteiaValue103B;
            _BtxtCriteiaValueB[9, 3] = BtxtCriteiaValue104B;
            _BtxtCriteiaValueB[9, 4] = BtxtCriteiaValue105B;
            _BtxtCriteiaValueB[9, 5] = BtxtCriteiaValue106B;
            _BtxtCriteiaValueB[9, 6] = BtxtCriteiaValue107B;

            //Bcmb 1行目
            _Bcmb[0, 0] = Bcmb11;
            _Bcmb[0, 1] = Bcmb12;
            _Bcmb[0, 2] = Bcmb13;
            _Bcmb[0, 3] = Bcmb14;
            _Bcmb[0, 4] = Bcmb15;
            _Bcmb[0, 5] = Bcmb16;
            _Bcmb[0, 6] = Bcmb17;
            //Bcmb 2行目
            _Bcmb[1, 0] = Bcmb21;
            _Bcmb[1, 1] = Bcmb22;
            _Bcmb[1, 2] = Bcmb23;
            _Bcmb[1, 3] = Bcmb24;
            _Bcmb[1, 4] = Bcmb25;
            _Bcmb[1, 5] = Bcmb26;
            _Bcmb[1, 6] = Bcmb27;
            //Bcmb 3行目
            _Bcmb[2, 0] = Bcmb31;
            _Bcmb[2, 1] = Bcmb32;
            _Bcmb[2, 2] = Bcmb33;
            _Bcmb[2, 3] = Bcmb34;
            _Bcmb[2, 4] = Bcmb35;
            _Bcmb[2, 5] = Bcmb36;
            _Bcmb[2, 6] = Bcmb37;
            //Bcmb 4行目
            _Bcmb[3, 0] = Bcmb41;
            _Bcmb[3, 1] = Bcmb42;
            _Bcmb[3, 2] = Bcmb43;
            _Bcmb[3, 3] = Bcmb44;
            _Bcmb[3, 4] = Bcmb45;
            _Bcmb[3, 5] = Bcmb46;
            _Bcmb[3, 6] = Bcmb47;
            //Bcmb 5行目
            _Bcmb[4, 0] = Bcmb51;
            _Bcmb[4, 1] = Bcmb52;
            _Bcmb[4, 2] = Bcmb53;
            _Bcmb[4, 3] = Bcmb54;
            _Bcmb[4, 4] = Bcmb55;
            _Bcmb[4, 5] = Bcmb56;
            _Bcmb[4, 6] = Bcmb57;
            //Bcmb 6行目
            _Bcmb[5, 0] = Bcmb61;
            _Bcmb[5, 1] = Bcmb62;
            _Bcmb[5, 2] = Bcmb63;
            _Bcmb[5, 3] = Bcmb64;
            _Bcmb[5, 4] = Bcmb65;
            _Bcmb[5, 5] = Bcmb66;
            _Bcmb[5, 6] = Bcmb67;
            //Bcmb 7行目
            _Bcmb[6, 0] = Bcmb71;
            _Bcmb[6, 1] = Bcmb72;
            _Bcmb[6, 2] = Bcmb73;
            _Bcmb[6, 3] = Bcmb74;
            _Bcmb[6, 4] = Bcmb75;
            _Bcmb[6, 5] = Bcmb76;
            _Bcmb[6, 6] = Bcmb77;
            //Bcmb 8行目
            _Bcmb[7, 0] = Bcmb81;
            _Bcmb[7, 1] = Bcmb82;
            _Bcmb[7, 2] = Bcmb83;
            _Bcmb[7, 3] = Bcmb84;
            _Bcmb[7, 4] = Bcmb85;
            _Bcmb[7, 5] = Bcmb86;
            _Bcmb[7, 6] = Bcmb87;
            //Bcmb 9行目
            _Bcmb[8, 0] = Bcmb91;
            _Bcmb[8, 1] = Bcmb92;
            _Bcmb[8, 2] = Bcmb93;
            _Bcmb[8, 3] = Bcmb94;
            _Bcmb[8, 4] = Bcmb95;
            _Bcmb[8, 5] = Bcmb96;
            _Bcmb[8, 6] = Bcmb97;
            //Bcmb 10行目
            _Bcmb[9, 0] = Bcmb101;
            _Bcmb[9, 1] = Bcmb102;
            _Bcmb[9, 2] = Bcmb103;
            _Bcmb[9, 3] = Bcmb104;
            _Bcmb[9, 4] = Bcmb105;
            _Bcmb[9, 5] = Bcmb106;
            _Bcmb[9, 6] = Bcmb107;


            //Bcmbvital
            _Bvitalcode[0] = BcmbVitalCode1;
            _Bvitalcode[1] = BcmbVitalCode2;
            _Bvitalcode[2] = BcmbVitalCode3;
            _Bvitalcode[3] = BcmbVitalCode4;
            _Bvitalcode[4] = BcmbVitalCode5;
            _Bvitalcode[5] = BcmbVitalCode6;
            _Bvitalcode[6] = BcmbVitalCode7;
            _Bvitalcode[7] = BcmbVitalCode8;
            _Bvitalcode[8] = BcmbVitalCode9;
            _Bvitalcode[9] = BcmbVitalCode10;
            */

            //_cmbDataType
            _cmbDataTypeUP[0] = cmbDataTypeUP1;
            _cmbDataTypeUP[1] = cmbDataTypeUP2;
            _cmbDataTypeUP[2] = cmbDataTypeUP3;
            _cmbDataTypeUP[3] = cmbDataTypeUP4;
            _cmbDataTypeUP[4] = cmbDataTypeUP5;
            _cmbDataTypeUP[5] = cmbDataTypeUP6;
            _cmbDataTypeUP[6] = cmbDataTypeUP7;
            _cmbDataTypeUP[7] = cmbDataTypeUP8;
            _cmbDataTypeUP[8] = cmbDataTypeUP9;
            _cmbDataTypeUP[9] = cmbDataTypeUP10;
            
            /* shundbg
             * _cmbDataTypeCRE[0] = cmbDataTypeCRE1;
            _cmbDataTypeCRE[1] = cmbDataTypeCRE2;
            _cmbDataTypeCRE[2] = cmbDataTypeCRE3;
            _cmbDataTypeCRE[3] = cmbDataTypeCRE4;
            _cmbDataTypeCRE[4] = cmbDataTypeCRE5;
            _cmbDataTypeCRE[5] = cmbDataTypeCRE6;
            _cmbDataTypeCRE[6] = cmbDataTypeCRE7;
            _cmbDataTypeCRE[7] = cmbDataTypeCRE8;
            _cmbDataTypeCRE[8] = cmbDataTypeCRE9;
            _cmbDataTypeCRE[9] = cmbDataTypeCRE10;
            */
            

            /*
            _ScoreLv[0] = txtScoreLv3L;
            _ScoreLv[1] = txtScoreLv2L;
            _ScoreLv[2] = txtScoreLv1L;
            //_ScoreLv[3] = ;
            _ScoreLv[4] = txtScoreLv1L;
            _ScoreLv[5] = txtScoreLv2R;
            _ScoreLv[6] = txtScoreLv3R;
            */
        }
        /// <summary>
        /// 表示中の表を削除
        /// </summary>
        private void AllClear()
        {
            //表のテキストボックスを初期化
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    _txtCriteiaValueA[i, j].ResetText();
                    _txtCriteiaValueB[i, j].ResetText();
                    /*_BtxtCriteiaValueA[i, j].ResetText();
                    _BtxtCriteiaValueB[i, j].ResetText();
                    shundbg*/
                    _cmb[i, j].SelectedIndex = 0;

                    //txtcombobox
                    _cmb[i, j].Items.Clear();
                    _cmb[i, j].Items.Add("");
                    _cmb[i, j].Items.Add("=");
                    _cmb[i, j].Items.Add(",");
                    _cmb[i, j].Items.Add("～");
                    _cmb[i, j].Items.Add("≦");
                    _cmb[i, j].Items.Add("≧");
                    _cmb[i, j].SelectedIndex = 0;
                    //削除予定
                    /* shundbg
                    _Bcmb[i, j].Items.Clear();
                    _Bcmb[i, j].Items.Add("");
                    _Bcmb[i, j].Items.Add("=");
                    _Bcmb[i, j].Items.Add(",");
                    _Bcmb[i, j].Items.Add("～");
                    _Bcmb[i, j].Items.Add("≦");
                    _Bcmb[i, j].Items.Add("≧");
                    _Bcmb[i, j].SelectedIndex = 0;
                    */
                }
                _vitalcode[i].Items.Clear();
                //_Bvitalcode[i].Items.Clear();
                _cmbDataTypeUP[i].SelectedIndex = -1;
                //_cmbDataTypeCRE[i].SelectedIndex = -1;shundbg
            }

            txtScoreLv3L.ResetText();
            txtScoreLv2L.ResetText();
            txtScoreLv1L.ResetText();
        }

        /// <summary>
        /// btnUPDATEクリック時処理
        /// 選択されたEwsNameの情報をＤＢから取ってきて表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UPDATE_Click(object sender, EventArgs e)
        {
            var RecordList = new List<Record>();
            //score配列を入力したscoreLVに更新する
            Update_scorearray();
            //入力エラーチェックフラグ
            bool check_input = true;
            for (int i = 0; i < 10; i++)//iは行数 10の部分を定数にしたほうがきれいかも
            {
                var intlist = new List<int>();
                for (int j = 0; j < 7; j++)//jは列数　７の部分を定数にしたほうがきれいかも
                {
                    if (_txtCriteiaValueA[i, j].Text == "" && _txtCriteiaValueB[i, j].Text == "")
                    {
                        continue;
                    }
                    else
                    {
                        if(!CreatRecord_Update(RecordList, intlist, i, _txtCriteiaValueA[i, j].Text, _cmb[i, j].SelectedIndex, _txtCriteiaValueB[i, j].Text, _score[j], tarval[j], i + 1, Convert.ToInt32(txtSeqNo.Text), _cmbDataTypeUP[i].SelectedIndex))
                        {
                            ////ファイルの行番号取得           
                            string errMsg = GetLineNumber().ToString() + " 行目" +" [i,j] = " + i + " " + j;    //この行（サンプルでは50行目）が表示される
                            //メッセージボックスで行番号を表示
                            MessageBox.Show(errMsg
                                            , "エラー"
                                            , MessageBoxButtons.OK
                                            , MessageBoxIcon.Information);
                            check_input = false;
                        }
                    }
                }

                //入力制限判定
                //int double
                if (intlist.Count != 0)
                {
                    bool fl = true;
                    for (int s = 0; s < intlist.Count - 1; s++)
                    {
                        if (intlist[s + 1] - intlist[s] != 1)
                        {
                            fl = false;
                            check_input = false;
                            break;
                        }
                    }
                    //エラー処理
                    if (!fl)
                    {
                        MessageBox.Show("int error");
                    }
                }//string
                else
                {
                }
            }

            //if (!check_input)
            //{
                ////ファイルの行番号取得           
                string strMsg = GetLineNumber().ToString() + " 行目 レコード登録中止";    //この行（サンプルでは50行目）が表示される

                //メッセージボックスで行番号を表示
                MessageBox.Show(strMsg
                                , "情報"
                                , MessageBoxButtons.OK
                                , MessageBoxIcon.Information);
                return ;
            //}
            //EVISCloudに接続
            string constr = @"Data Source=192.168.1.174;Initial Catalog=EVISCloud;Integrated Security=False;User ID=sa;Password=P@ssw0rd";

            SqlConnection con = new SqlConnection(constr);
            con.Open();
            try
            {
                bool first = true;              
                foreach (var record in RecordList)
                {
                    //test　テキストボックスに登録する情報を出力、確認用 shundbg
                    int EwsId = record.EWSId;
                    string VitalCode = record.VitalCode;
                    int Score = record.Score;
                    string CriteriaValue = record.CriteriaValue;
                    int CriteriaSign = record.CriteriaSign;
                    int Target = record.Target;
                    txtOutSql.Text += string.Format("{0} / {1} /{2} / {3} / {4} / {5} / {6}  \r\n", EwsId, VitalCode, Score, CriteriaValue, CriteriaSign, Target, record.DisplayOrder);
                    //test 

                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO T_EwsScoreCriteria(EwsId, SeqNo, VitalCode, Score, CriteriaValue, CriteriaSign, Target, DisplayOrder)");
                    sb.Append($"VALUES( {record.EWSId}, {record.SeqNo}, '{record.VitalCode}', {record.Score}, '{record.CriteriaValue}', {record.CriteriaSign}, {record.Target}, {record.DisplayOrder})");
                    //shundbg
                    //sb.Append($"VALUES( 3000, {record.SeqNo}, '{record.VitalCode}', {record.Score}, '{record.CriteriaValue}', {record.CriteriaSign}, {record.Target}, {record.DisplayOrder})");
                    SqlCommand com = new SqlCommand(sb.ToString(), con);

                    var result = com.ExecuteNonQuery();
                    if (result == 0) MessageBox.Show("T_EwsScoreCriteria to UPDATE Failed in UPDATE_Click()");
                    
                    if (first)
                    {
                        //過去分のinvalidflagを立てる必要がある
                        string sqlreststr = $"UPDATE T_EwsScoreCriteria SET InvalidFlag = 1 WHERE EwsId = {record.EWSId} AND SeqNo = {record.SeqNo - 1}";
                        SqlCommand comreset = new SqlCommand(sqlreststr, con);
                        var resultreset = comreset.ExecuteNonQuery();
                        first = false;
                    }
                }
            }
            finally
            {
                MessageBox.Show("UPDATE Done");
                con.Close();
            }
        }

        /// <summary>
        /// UPDATE用レコード作成
        /// </summary>
        /// <param name="RecList"></param>
        /// <param name="vitalcode"></param>
        /// <param name="txtA"></param>
        /// <param name="symb"></param>
        /// <param name="txtB"></param>
        /// <param name="score"></param>
        /// <param name="tarval"></param>
        /// <param name="displayorder"></param>
        /// <param name="seqno"></param>
        public bool CreatRecord_Update(List<Record> RecList, List<int> intlist, int vitalcode, string txtA, int symb, string txtB, int score, int tarval, int displayorder, int seqno, int datatype)
        {
            double d;
            int i;
            bool fl = true;
            //string[] words;
            switch (symb)
            {
                case 0:
                    //エラー この関数を呼ぶ前にtxtA,bに中身があるか確認してるからここには来ないはず
                    MessageBox.Show("コンボボックスが未選択です");
                    fl = false;
                    break;
                case 1://   "="memo 1つのレコード作成
                    if (datatype != 2)
                    {
                        //エラー 数値入力なのに単一文字列が含まれている
                        fl = false; break;
                    }
                    var record0 = new Record(EWSID.Text);

                    record0.Score = score;
                    record0.VitalCode = _vitalcode[vitalcode].Items[0].ToString();
                    record0.CriteriaValue = txtA;
                    record0.CriteriaSign = 2;
                    record0.Target = tarval;
                    record0.SeqNo = seqno + 1;
                    record0.DisplayOrder = displayorder;

                    RecList.Add(record0);
                    break;
                case 2:// , memo レコード数に限りない
                    if (datatype != 2)
                    {
                        //エラー
                        ////ファイルの行番号取得           
                        string strMsg = GetLineNumber().ToString() + " 行目";    //この行（サンプルでは50行目）が表示される

                        //メッセージボックスで行番号を表示
                        MessageBox.Show(strMsg
                                        , "入力値エラー"
                                        , MessageBoxButtons.OK
                                        , MessageBoxIcon.Information);
                        fl = false; break;
                    }
                    string[] words = txtA.Split(',');
                    foreach (var word in words)
                    {
                        var record1 = new Record(EWSID.Text);
                        record1.Score = score;
                        record1.VitalCode = _vitalcode[vitalcode].Items[0].ToString();
                        record1.CriteriaValue = word;
                        record1.CriteriaSign = 2;
                        record1.Target = tarval;
                        record1.SeqNo = seqno + 1;
                        record1.DisplayOrder = displayorder;

                        RecList.Add(record1);
                    }
                    break;
                case 3:// ~ memo 2つレコードを作ればいい
                    var record21 = new Record(EWSID.Text);
                    record21.Score = score;
                    record21.VitalCode = _vitalcode[vitalcode].Items[0].ToString();
                    record21.CriteriaValue = txtA;
                    record21.CriteriaSign = 1;
                    record21.Target = tarval;
                    record21.SeqNo = seqno + 1;
                    record21.DisplayOrder = displayorder;

                    RecList.Add(record21);

                    var record22 = new Record(EWSID.Text);
                    record22.Score = score;
                    record22.VitalCode = _vitalcode[vitalcode].Items[0].ToString();
                    record22.CriteriaValue = txtB;
                    record22.CriteriaSign = 0;
                    record22.Target = tarval;
                    record22.SeqNo = seqno + 1;
                    record22.DisplayOrder = displayorder;

                    RecList.Add(record22);

                    //入力制限判定用
                    //double入力
                    if (datatype == 1)
                    {
                        double da;
                        double.TryParse(txtB, out d);
                        double.TryParse(txtA, out da);
                        int a = (int)(da * 10);
                        int b = (int)(d * 10);
                        for (int k = a; k <= b; k++)
                        {
                            intlist.Add(k);
                        }
                    }//int入力
                    else if (datatype == 0)
                    {
                        int ia;
                        int.TryParse(txtB, out i);
                        int.TryParse(txtA, out ia);
                        for (int start = ia; start <= i; start++)
                        {
                            intlist.Add(start);
                        }
                    }
                    else
                    {
                        //エラー　数値入力のはずなのに文字列入力されている
                        ////ファイルの行番号取得           
                        string strMsg = GetLineNumber().ToString() + " 行目";

                        //メッセージボックスで行番号を表示
                        MessageBox.Show(strMsg
                                        , "入力値エラー"
                                        , MessageBoxButtons.OK
                                        , MessageBoxIcon.Information);
                        fl = false; break;
                    }

                    break;
                case 4:// <= memo １つレコード：A,Bが空かどうか判別が必要かな一回ききたい
                    var record3 = new Record(EWSID.Text);
                    record3.Score = score;
                    record3.VitalCode = _vitalcode[vitalcode].Items[0].ToString();
                    record3.CriteriaValue = txtB;
                    record3.CriteriaSign = 0;
                    record3.Target = tarval;
                    record3.SeqNo = seqno + 1;
                    record3.DisplayOrder = displayorder;

                    RecList.Add(record3);

                    //入力制限用
                    //double
                    if (datatype == 1)
                    {
                        double da;
                        double.TryParse(txtB, out d);
                        double.TryParse(txtA, out da);
                        int a = (int)(da * 10);
                        int b = (int)(d * 10);
                        for (int k = a; k <= b; k++)
                        {
                            intlist.Add(k);
                        }
                    }//int入力
                    else if (datatype == 0)
                    {
                        int ia;
                        int.TryParse(txtB, out i);
                        int.TryParse(txtA, out ia);
                        for (int start = ia; start <= i; start++)
                        {
                            intlist.Add(start);
                        }
                    }
                    else
                    {
                        //エラー　数値入力のはずなのに文字列入力されている
                        ////ファイルの行番号取得           
                        string strMsg = GetLineNumber().ToString() + " 行目";

                        //メッセージボックスで行番号を表示
                        MessageBox.Show(strMsg
                                        , "入力値エラー"
                                        , MessageBoxButtons.OK
                                        , MessageBoxIcon.Information);
                        fl = false; break;
                    }

                    break;
                case 5:// >= memo １つレコード：A,Bが空かどうか判別が必要一回聞きたいそういう表記になってるだけで入力するときの感覚的にはおかしいかもしれない
                    var record4 = new Record(EWSID.Text);
                    record4.Score = score;
                    record4.VitalCode = _vitalcode[vitalcode].Items[0].ToString();
                    record4.CriteriaValue = txtB;
                    record4.CriteriaSign = 1;//shundbg 
                    record4.Target = tarval;
                    record4.SeqNo = seqno + 1;
                    record4.DisplayOrder = displayorder;

                    RecList.Add(record4);

                    //入力制限用
                    //double
                    if (double.TryParse(txtB, out d) && txtB.Contains('.'))
                    {
                        intlist.Add((int)(d * 10));
                    }//int
                    else if (int.TryParse(txtB, out i))
                    {
                        intlist.Add(i);
                    }
                    else
                    {
                        //エラー　数値入力のはずなのに文字列入力されている
                        ////ファイルの行番号取得           
                        string strMsg = GetLineNumber().ToString() + " 行目";

                        //メッセージボックスで行番号を表示
                        MessageBox.Show(strMsg
                                        , "入力値エラー"
                                        , MessageBoxButtons.OK
                                        , MessageBoxIcon.Information);
                        fl = false; break;
                    }

                    break;
            }
            return fl;
        }
        /// <summary>
        /// Createページ移行時初期処理
        /// 登録可能なEwsID取得・表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Selected_CreatPage(object sender, EventArgs e)
        {

            //(create -> update)
            if ( ((TabControl)sender).SelectedIndex == 0 )
            {

            }
            //(update -> create)
            else if (((TabControl)sender).SelectedIndex == 1 )
            {

            }
            AllClear();//shundbg 入力されているが消してもいいか？って聞く処理がない
            string constr = @"Data Source=192.168.1.174;Initial Catalog=EVISCloud;Integrated Security=False;User ID=sa;Password=P@ssw0rd";
            SqlConnection con = new SqlConnection(constr);
            con.Open();
            try
            {
                 
                string sqlstr = $"SELECT Id FROM M_EwsType WHERE Id = (SELECT MAX(Id) FROM M_EwsType)";
                //string sqlstr = $"SELECT MAX(Id) FROM M_EwsType";
                SqlCommand com = new SqlCommand(sqlstr, con);
                SqlDataReader sdr = com.ExecuteReader();

                while (sdr.Read() == true)
                {
                    //EVIS                   
                    int Id = (int)sdr["Id"];
                    txtCreateEWSID.Text = (Id+1).ToString();
                }
                sdr.Close();
                com.Dispose();
            }
            finally
            {
                con.Close();
            }
        }

        private void txtScoreLv1L_TextChanged(object sender, EventArgs e)
        {
            txtScoreLv1R.Text = txtScoreLv1L.Text;
        }

        private void txtScoreLv2L_TextChanged(object sender, EventArgs e)
        {
            txtScoreLv2R.Text = txtScoreLv2L.Text;
        }

        private void txtScoreLv3L_TextChanged(object sender, EventArgs e)
        {
            txtScoreLv3R.Text = txtScoreLv3L.Text;
        }

        private void txtCreScoreLv3L_TextChanged(object sender, EventArgs e)
        {
            //shundbgtxtCreScoreLv3R.Text = txtCreScoreLv3L.Text; 
        }

        private void txtCreScoreLv2L_TextChanged(object sender, EventArgs e)
        {
            //shundbgtxtCreScoreLv2R.Text = txtCreScoreLv2L.Text;
        }
        private void txtCreScoreLv1L_TextChanged(object sender, EventArgs e)
        {
            //shundbgtxtCreScoreLv1R.Text = txtCreScoreLv1L.Text;
        }
        /// <summary>
        /// DataTypeコンボボックス選択時処理(UPDATEページ)
        /// 数値、文字列の二つの条件に合わせてコンボボックスのアイテムを設定する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbDataTypeUP_SelectedIndexChanged(object sender, EventArgs e)
        {   
            //DataType == String
            if( ((ComboBox)sender).SelectedIndex == 2)
            {
                for (int j = 0; j < 7; j++)
                {
                    int index = Array.IndexOf(_cmbDataTypeUP, ((ComboBox)sender));
                    _cmb[index, j].Items.Clear();
                    _cmb[index, j].Items.Add("");
                    _cmb[index, j].Items.Add("=");
                    _cmb[index, j].Items.Add(",");
                    _cmb[index, j].SelectedIndex = 0;
                }
            }
            else
            {
                for (int j = 0; j < 7; j++)
                {
                    int index = Array.IndexOf(_cmbDataTypeUP, ((ComboBox)sender));
                    _cmb[index, j].Items.Clear();
                    _cmb[index, j].Items.Add("");
                    _cmb[index, j].Items.Add("～");
                    _cmb[index, j].Items.Add("≦");
                    _cmb[index, j].Items.Add("≧");
                    _cmb[index, j].SelectedIndex = 0;
                }
            }

        }

        /// <summary>
        /// DataTypeコンボボックス選択時処理(CREATERページ)
        /// 数値、文字列の二つの条件に合わせてコンボボックスのアイテムを設定する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbDataTypeCRE_SelectedIndexChanged(object sender, EventArgs e)
        {
            //DataType == String
            if (((ComboBox)sender).SelectedIndex == 2)
            {
                for (int j = 0; j < 7; j++)
                {
                    int index = Array.IndexOf(_cmbDataTypeCRE, ((ComboBox)sender));
                    _Bcmb[index, j].Items.Clear();
                    _Bcmb[index, j].Items.Add("");
                    _Bcmb[index, j].Items.Add("=");
                    _Bcmb[index, j].Items.Add(",");
                    _Bcmb[index, j].SelectedIndex = 0;
                }
            }
            else
            {
                for (int j = 0; j < 7; j++)
                {
                    int index = Array.IndexOf(_cmbDataTypeCRE, ((ComboBox)sender));
                    _Bcmb[index, j].Items.Clear();
                    _Bcmb[index, j].Items.Add("");
                    _Bcmb[index, j].Items.Add("～");
                    _Bcmb[index, j].Items.Add("≦");
                    _Bcmb[index, j].Items.Add("≧");
                    _Bcmb[index, j].SelectedIndex = 0;
                }
            }

        }

        private void Selected_CreatPage(object sender, TabControlEventArgs e)
        {

        }
    }
}
