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
        private TextBox[,] _txtCriteiaValueA;
        private TextBox[,] _txtCriteiaValueB;
        private ComboBox[,] _cmb;
        private ComboBox[] _vitalcode;
        private TextBox[] _txtDisplayOrder;
        private TextBox[] _txtScore;
        private ComboBox[] _cmbDataTypeUP;

        //DB読込時の値保存用
        private string[] Save_DisplayOrder;
        private int[] Save_DataType;
        private string[] Save_VitalCode;
        private string[,] Save_TxtA;
        private string[,] Save_TxtB;
        private string[,] Save_Cmb;
        private string[] Save_Score;
        

        private int[] _score = new int[7];
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
            InitControl();
            InitComboBox();
            InitEwsName();
            InitTxtBoxColor();

        }
        private void Form2_Load(object sender, EventArgs e)
        {
        }
        /// <summary>
        /// 入力されたスコアを判定用の配列にコピー、コピー時降順になっているかチェック
        /// </summary>
        /// <returns>
        /// True,false=スコアが降順に入力されていない
        /// </returns>
        private bool Update_scorearray()
        {
            if (txtScoreLv3L.Text != "") _score[0] = Int32.Parse(txtScoreLv3L.Text);
            else                         _score[0] = -1;     

            if (txtScoreLv2L.Text != "") _score[1] = Int32.Parse(txtScoreLv2L.Text);
            else                         _score[1] = -1;

            if (txtScoreLv1L.Text != "") _score[2] = Int32.Parse(txtScoreLv1L.Text);
            else                         _score[2] = -1;

                                         _score[3] = 0;

            if (txtScoreLv1L.Text != "") _score[4] = Int32.Parse(txtScoreLv1L.Text);
            else                         _score[4] = -1;

            if (txtScoreLv2L.Text != "") _score[5] = Int32.Parse(txtScoreLv2L.Text);
            else                         _score[5] = -1;

            if (txtScoreLv3L.Text != "") _score[6] = Int32.Parse(txtScoreLv3L.Text);
            else                         _score[6] = -1;




            var ScoreCheck = new List<int>();

            for (int i=0;i<3;i++)
            {
                if (_score[i] != -1) ScoreCheck.Add(_score[i]);




                if (_score[i] == -1 || _score[i+1] == -1) 
                {
                    continue;
                }
                else
                {
                    if ( _score[i] < _score[i + 1] )
                    {
                        MessageBox.Show("スコアが間違っています");
                        return false; 
                    }
                }

                
            }
            return true;           
        }

        //行番号取得用メソッド
        private int GetLineNumber([CallerLineNumber] int intLineNumber = 0)
        {
            return intLineNumber;
        }
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
        public bool CreatRecord(List<Record> RecList, List<int> intlist, string vitalcode, string txtA, string symb, string txtB, int score, int tarval, int displayorder,int datatype)
        {
            double d;
            int i;
            bool fl = true;
            if (datatype == -1)
            {
                //エラー
                ////ファイルの行番号取得           
                string strMsg = GetLineNumber().ToString() + " 行目 DATATYPEが未選択です";    //この行（サンプルでは50行目）が表示される

                //メッセージボックスで行番号を表示
                MessageBox.Show(strMsg
                                , "入力値エラー"
                                , MessageBoxButtons.OK
                                , MessageBoxIcon.Information);
                fl = false;
                return fl;
            }

            if(score == -1)
            {
                //エラー
                ////ファイルの行番号取得           
                string strMsg = GetLineNumber().ToString() + " 行目 SCOREが未入力です";    //この行（サンプルでは50行目）が表示される

                //メッセージボックスで行番号を表示
                MessageBox.Show(strMsg
                                , "入力値エラー"
                                , MessageBoxButtons.OK
                                , MessageBoxIcon.Information);
                fl = false;
                return fl;
            }
            
            switch (symb)
            {
                case ""://未選択
                    //エラー この関数を呼ぶ前にtxtA,bに中身があるか確認してるからここには来ないはず
                    MessageBox.Show("コンボボックスが未選択です");
                    fl = false;
                    break;
                case "=":// =  memo 1つのレコード作成
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
                    record0.VitalCode = vitalcode;
                    record0.CriteriaValue = txtA;
                    record0.CriteriaSign = 2;
                    record0.Target = tarval;
                    record0.DisplayOrder = displayorder;

                    RecList.Add(record0);
                    break;
                case ",":// , memo レコード数に限りない
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
                        record1.VitalCode = vitalcode;
                        record1.CriteriaValue = word;
                        record1.CriteriaSign = 2;
                        record1.Target = tarval;
                        record1.DisplayOrder = displayorder;

                        RecList.Add(record1);
                    }
                    break;
                case "～":// ~ memo 2つレコードを作ればいい
                    var record21 = new Record(txtCreateEWSID.Text);
                    record21.Score = score;
                    record21.VitalCode = vitalcode;
                    record21.CriteriaValue = txtA;
                    record21.CriteriaSign = 1;
                    record21.Target = tarval;
                    record21.DisplayOrder = displayorder;

                    RecList.Add(record21);

                    var record22 = new Record(txtCreateEWSID.Text);
                    record22.Score = score;
                    record22.VitalCode = vitalcode;
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
                case "≦":// <= memo １つレコード：
                    var record3 = new Record(txtCreateEWSID.Text);
                    record3.Score = score;
                    record3.VitalCode = vitalcode;
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
                case "≧":// >= memo １つレコード：
                    var record4 = new Record(txtCreateEWSID.Text);
                    record4.Score = score;
                    record4.VitalCode = vitalcode;
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

        private void CreatButton_Click(object sender, EventArgs e)
        {
            var RecordList = new List<Record>();
            bool check_input = true;
            check_input = Update_scorearray();
            //入力情報をレコードに変換する
            for (int i = 0; i < 10; i++)//iは行数 10の部分を定数にしたほうがきれいかも
            {
                var RecordList_Vital = new List<Record>();
                var intlist = new List<int>();



                for (int j = 0; j < 7; j++)//jは列数　７の部分を定数にしたほうがきれいかも
                {
                    if (_txtCriteiaValueA[i, j].Text == "" && _txtCriteiaValueB[i, j].Text == "")
                    {
                        continue;
                    }else if (_txtDisplayOrder[i].Text == "")
                    {
                        ////ファイルの行番号取得           
                        string errMsg = GetLineNumber().ToString() + " 行目" + " [i] = " + i;    //この行（サンプルでは50行目）が表示される
                                                                                                //メッセージボックスで行番号を表示
                        MessageBox.Show(errMsg
                                        , "DisplayOrder未入力"
                                        , MessageBoxButtons.OK
                                        , MessageBoxIcon.Information);
                        check_input = false;
                        break;
                    }
                    else
                    {
                        check_input = CreatRecord(RecordList, intlist, _vitalcode[i].Text, _txtCriteiaValueA[i, j].Text, _cmb[i, j].Text, _txtCriteiaValueB[i, j].Text, _score[j], tarval[j], Convert.ToInt32(_txtDisplayOrder[i].Text), _cmbDataTypeUP[i].SelectedIndex);
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

            if (RecordList.Count == 0)
            {
                ////ファイルの行番号取得           
                string strMsg = GetLineNumber().ToString() + " 行目 レコードが作成されていません";    //この行（サンプルでは50行目）が表示される

                //メッセージボックスで行番号を表示
                MessageBox.Show(strMsg
                                , "情報"
                                , MessageBoxButtons.OK
                                , MessageBoxIcon.Information);
                check_input = false;
            }

            if (!check_input)
            {
                ////ファイルの行番号取得           
                string strMsg = GetLineNumber().ToString() + " 行目 レコード新規登録中止";    //この行（サンプルでは50行目）が表示される

                //メッセージボックスで行番号を表示
                MessageBox.Show(strMsg
                                , "情報"
                                , MessageBoxButtons.OK
                                , MessageBoxIcon.Information);
                return;
            }
           
            //EVISCloudに接続
            string constr = @"Data Source=192.168.1.174;Initial Catalog=EVISCloud;Integrated Security=False;User ID=sa;Password=P@ssw0rd";
            SqlConnection con = new SqlConnection(constr);
            con.Open();
            int Ewsid = 0;
            try
            {
                //更新後出力用変数
                
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
                    Ewsid = record.EWSId;
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
            EWSID.Text = Ewsid.ToString();
            txtSeqNo.Text = "1";
            cmbEwsName.SelectedIndex = cmbEwsName.FindString(txtCreateEwsName.Text);
            InitTxtBoxColor();
        }
        

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
            int selectindex = cmbEwsName.SelectedIndex;
            string SelectedItem;
            if ( selectindex != -1)
            {
                SelectedItem = cmbEwsName.SelectedItem.ToString();
            }
            else
            {
                return;
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

            //EwsNameに対応するEwsidのレコードを取得しtxtに出力する
            string constr = @"Data Source=192.168.1.174;Initial Catalog=EVISCloud;Integrated Security=False;User ID=sa;Password=P@ssw0rd";
            SqlConnection con = new SqlConnection(constr);
            con.Open();
            try
            {
                //SQL文作成:
                string sqlstr = $"SELECT * FROM T_EwsScoreCriteria WHERE EwsId = {EwsName[SelectedItem]} AND InvalidFlag = 0";
                sqlstr += $"AND SeqNo = (SELECT MAX(SeqNo) FROM T_EwsScoreCriteria WHERE EwsId = {EwsName[SelectedItem]})";

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
                //エラー スコアの項目が多い
            }
            else
            {
                foreach (var score in result)
                {
                    switch (cnt)
                    {
                        case 2:
                            cnt--;
                            txtScoreLv1L.Text = score.ToString();
                            Save_Score[2] = txtScoreLv1L.Text;
                            Save_Score[4] = txtScoreLv1L.Text;
                            break;
                        case 3:
                            cnt--;
                            txtScoreLv2L.Text = score.ToString();
                            Save_Score[1] = txtScoreLv2L.Text;
                            Save_Score[5] = txtScoreLv2L.Text;
                            break;
                        case 4:
                            cnt--;
                            txtScoreLv3L.Text = score.ToString();
                            Save_Score[0] = txtScoreLv3L.Text;
                            Save_Score[6] = txtScoreLv3L.Text;
                            break;
                        default:
                            break;

                    }
                }
            }

            scoreindex = scoreset.ToArray();
            //スコアとターゲット,ディスプレイオーダーによって振り分ける
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



            //GetRecords[0][]に入ってしまった分をvitalcodeをもとに0<iに振り分ける
            for (int j = 0; j < 7; j++)
            {
                foreach (var record in GetRecords[0, j])
                {
                    GetRecords[DicVitalcodeandDisplayOrder[record.VitalCode], j].Add(record);
                }
            }

            //******修正いる？！！二重forするようなこと？？？？->record配列のどこに要素が入っているかわからないから２重で回すしかない。フラグで早くぬけてるからそんな回してるわけでもない
            //cmbvitalcodeを更新
            for (int i = 0; i < 10; i++)
            {
                //cmbvitalcode クリアする
                bool fl = false;
                //Vitalcode追加,選択
                for (int j = 0; j < 7; j++)
                {
                    if (GetRecords[i + 1, j].Count != 0)
                    {
                        //Getrecordsにはi=1から実データが入ってる
                        //VitalCodeコンボボックスに該当するバイタルコードを登録
                        string str = GetRecords[i + 1, j].First().VitalCode;
                        _vitalcode[i].SelectedIndex = _vitalcode[i].FindString(str);
                        Save_VitalCode[i] = str;
                        //DisplayOrderテキストボックスに値を代入
                        _txtDisplayOrder[i].Text = GetRecords[i + 1, j].First().DisplayOrder.ToString();
                        Save_DisplayOrder[i] = GetRecords[i + 1, j].First().DisplayOrder.ToString();
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
            cmbEwsName.SelectedIndex = selectindex;
            InitTxtBoxColor();

        }

        /// <summary>
        /// GetRecordsを表形式に表示
        /// </summary>
        private void OutScore()
        {

            for (int i = 0; i < 10; i++)
            {
                int datatype = 99;
                double d;
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
                            Save_TxtA[i, j] = _txtCriteiaValueA[i, j].Text;

                            _cmb[i, j].SelectedIndex = 2;
                            Save_Cmb[i, j] = ",";
                            datatype = 2;
                        }
                        //case 0 "="
                        else
                        {
                            _txtCriteiaValueA[i, j].Text += GetRecords[i + 1, j].First().CriteriaValue;                       
                            _cmb[i, j].SelectedIndex = 1;
                            datatype = 2;
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
                        if (double.TryParse(_txtCriteiaValueA[i, j].Text, out d) && _txtCriteiaValueA[i, j].Text.Contains('.'))
                        {
                            datatype = 1;
                        }
                        else
                        {
                            datatype = 0;
                        }
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
                        if (double.TryParse(_txtCriteiaValueB[i, j].Text, out d) && _txtCriteiaValueB[i, j].Text.Contains('.'))
                        {
                            datatype = 1;
                        }
                        else
                        {
                            datatype = 0;
                        }
                    }
                    //error
                    else
                    {
                        txtOutSql.Text += string.Format($"OutScore error {i} {j} \r\n");
                    }
                    Save_TxtA[i, j] = _txtCriteiaValueA[i, j].Text;
                    Save_TxtB[i, j] = _txtCriteiaValueB[i, j].Text;
                    Save_Cmb[i, j] = _cmb[i, j].Text;
                }

                if(datatype == 0)
                {
                    _cmbDataTypeUP[i].SelectedIndex = 0;
                }
                else if( datatype == 1)
                {
                    _cmbDataTypeUP[i].SelectedIndex = 1;
                }
                else if (datatype == 2)
                {
                    _cmbDataTypeUP[i].SelectedIndex = 2;
                }
                Save_DataType[i] = _cmbDataTypeUP[i].SelectedIndex;
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
                }

                _vitalcode[i].SelectedIndex = 0;
                _vitalcode[i].Items.Clear();
            }
            //shundbg
            foreach (string s in VitalCodeName)
            {
                txtOutSql.Text += s + "\r\n";
            }

            //登録されているvitalcodeを取ってきてスタックしておく（重複を許していない）
            var vitalcodes = new HashSet<string>();
            string constr = @"Data Source=192.168.1.174;Initial Catalog=EVISCloud;Integrated Security=False;User ID=sa;Password=P@ssw0rd";
            SqlConnection con = new SqlConnection(constr);
            con.Open();
            try
            {
                //vitalcode一覧取得
                string sqlstr = $"SELECT vital_code FROM M_VitalType WHERE vital_code IS NOT NULL";
                SqlCommand com = new SqlCommand(sqlstr, con);
                SqlDataReader sdr = com.ExecuteReader();
                while (sdr.Read() == true)
                {

                    string vitalcode = (string)sdr["vital_code"];
                    vitalcodes.Add(vitalcode);
                }
                sdr.Close();
                com.Dispose();

            }
            finally
            {
                con.Close();
            }

            //_vitalcode[]にaddしておく
            for (int i = 0; i < 10; i++)
            {
                foreach (var code in vitalcodes)
                {
                    _vitalcode[i].Items.Add(code);
                }
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

            _txtDisplayOrder  = new TextBox[10];
            _cmbDataTypeUP    = new ComboBox[10];
            _txtScore = new TextBox[7];
            Save_DisplayOrder = new string[10];
            Save_DataType     = new int[10];
            Save_VitalCode    = new string[10];
            Save_TxtA         = new string[10,7];
            Save_TxtB         = new string[10,7];
            Save_Cmb          = new string[10,7];
            Save_Score        = new string[7]    ;

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

            _txtDisplayOrder[0] = txtDisplayOrder1;
            _txtDisplayOrder[1] = txtDisplayOrder2;
            _txtDisplayOrder[2] = txtDisplayOrder3;
            _txtDisplayOrder[3] = txtDisplayOrder4;
            _txtDisplayOrder[4] = txtDisplayOrder5;
            _txtDisplayOrder[5] = txtDisplayOrder6;
            _txtDisplayOrder[6] = txtDisplayOrder7;
            _txtDisplayOrder[7] = txtDisplayOrder8;
            _txtDisplayOrder[8] = txtDisplayOrder9;
            _txtDisplayOrder[9] = txtDisplayOrder10;

            _txtScore[0] = txtScoreLv3L;
            _txtScore[1] = txtScoreLv2L;
            _txtScore[2] = txtScoreLv1L;
            //_txtScore[3]
            _txtScore[4] = txtScoreLv1R;
            _txtScore[5] = txtScoreLv2R;
            _txtScore[6] = txtScoreLv3R;

            for (int i= 0; i < 10; i++)
            {
                for(int j=0; j < 7; j++)
                {
                    _txtCriteiaValueA[i, j].TextChanged += new System.EventHandler(txtCriteiaValueA_TextChanged);
                    _txtCriteiaValueB[i, j].TextChanged += new System.EventHandler(txtCriteiaValueB_TextChanged);
                    _cmb[i,j].SelectedIndexChanged += new System.EventHandler(symbolcmb_SelectedIndexChanged);
                    _cmb[i, j].FlatStyle = FlatStyle.Popup;
                    if (j != 3)
                    {
                        _txtScore[j].TextChanged += new System.EventHandler(txtScore_TextChanged);
                        Save_Score[j] = null;
                    }
                    //save配列null
                    Save_TxtA[i,j] = null;
                    Save_TxtB[i, j] = null;
                    Save_Cmb[i, j] = "";
                }
                _txtDisplayOrder[i].TextChanged += new System.EventHandler(txtDisplayOrder_TextChanged);
                _cmbDataTypeUP[i].SelectedIndexChanged += new System.EventHandler(DataTypeCmb_SelectedIndexChanged);
                _cmbDataTypeUP[i].FlatStyle = FlatStyle.Popup;
                _vitalcode[i].SelectedIndexChanged += new System.EventHandler(VitalCodeCmb_SelectedIndexChanged);
                _vitalcode[i].FlatStyle = FlatStyle.Popup;

                //save配列Null埋め
                Save_DisplayOrder[i] = null;
                Save_DataType[i] = -1;//あやしいかもしれない
                Save_VitalCode[i] = null;
                
            }
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

                    //txtcombobox
                    _cmb[i, j].Items.Clear();
                    _cmb[i, j].Items.Add("");
                    _cmb[i, j].Items.Add("=");
                    _cmb[i, j].Items.Add(",");
                    _cmb[i, j].Items.Add("～");
                    _cmb[i, j].Items.Add("≦");
                    _cmb[i, j].Items.Add("≧");
                    _cmb[i, j].SelectedIndex = 0;
                    _cmb[i, j].Enabled = true;
                }
                _vitalcode[i].SelectedIndex = -1;
                _cmbDataTypeUP[i].SelectedIndex = -1;
                _txtDisplayOrder[i].ResetText();
            }

            EWSID.ResetText();
            txtSeqNo.ResetText();
            cmbEwsName.SelectedIndex = -1;
            txtCreateEWSID.ResetText();
            txtCreateEwsName.ResetText();
            txtCreateWarningThresolds.ResetText();
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
            bool check_input = true;
            //score配列を入力したscoreLVに更新する
            check_input = Update_scorearray();
            //入力エラーチェックフラグ
            
            for (int i = 0; i < 10; i++)//iは行数 10の部分を定数にしたほうがきれいかも
            {
                var intlist = new List<int>();
                
                for (int j = 0; j < 7; j++)//jは列数　７の部分を定数にしたほうがきれいかも
                {
                    if (_txtCriteiaValueA[i, j].Text == "" && _txtCriteiaValueB[i, j].Text == "")
                    {
                        continue;
                    }
                    else if(_txtDisplayOrder[i].Text == "")
                    {
                        ////ファイルの行番号取得           
                        string errMsg = GetLineNumber().ToString() + " 行目" + " [i] = " + i;    //この行（サンプルでは50行目）が表示される
                                                                                                //メッセージボックスで行番号を表示
                        MessageBox.Show(errMsg
                                        , "DisplayOrder未入力"
                                        , MessageBoxButtons.OK
                                        , MessageBoxIcon.Information);
                        check_input = false;
                        break;
                    }
                    else
                    {
                        if (!CreatRecord_Update(RecordList, intlist, _vitalcode[i].Text, _txtCriteiaValueA[i, j].Text, _cmb[i, j].Text, _txtCriteiaValueB[i, j].Text, _score[j], tarval[j], Convert.ToInt32(_txtDisplayOrder[i].Text), Convert.ToInt32(txtSeqNo.Text), _cmbDataTypeUP[i].SelectedIndex))
                        {
                            ////ファイルの行番号取得           
                            string errMsg = GetLineNumber().ToString() + " 行目" + " [i,j] = " + i + " " + j;    //この行（サンプルでは50行目）が表示される
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

            if (RecordList.Count == 0)
            {
                ////ファイルの行番号取得           
                string strMsg = GetLineNumber().ToString() + " 行目 レコードが作成されていません";    //この行（サンプルでは50行目）が表示される

                //メッセージボックスで行番号を表示
                MessageBox.Show(strMsg
                                , "情報"
                                , MessageBoxButtons.OK
                                , MessageBoxIcon.Information);
                check_input = false;
            }

            if (!check_input)
            {
                ////ファイルの行番号取得           
                string strMsg = GetLineNumber().ToString() + " 行目 レコード登録中止";    //この行（サンプルでは50行目）が表示される

                //メッセージボックスで行番号を表示
                MessageBox.Show(strMsg
                                , "情報"
                                , MessageBoxButtons.OK
                                , MessageBoxIcon.Information);
                return ;
            }
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
        public bool CreatRecord_Update(List<Record> RecList, List<int> intlist, string vitalcode, string txtA, string symb, string txtB, int score, int tarval, int displayorder, int seqno, int datatype)
        {
            double d;
            int i;
            bool fl = true;
            if(datatype == -1)
            {
                //エラー
                ////ファイルの行番号取得           
                string strMsg = GetLineNumber().ToString() + " 行目 DATATYPEが未選択です";    //この行（サンプルでは50行目）が表示される

                //メッセージボックスで行番号を表示
                MessageBox.Show(strMsg
                                , "入力値エラー"
                                , MessageBoxButtons.OK
                                , MessageBoxIcon.Information);
                fl = false;
                return fl;
            }

            if (score == -1)
            {
                //エラー
                ////ファイルの行番号取得           
                string strMsg = GetLineNumber().ToString() + " 行目 SCOREが未入力です";    //この行（サンプルでは50行目）が表示される

                //メッセージボックスで行番号を表示
                MessageBox.Show(strMsg
                                , "入力値エラー"
                                , MessageBoxButtons.OK
                                , MessageBoxIcon.Information);
                fl = false;
                return fl;
            }
            //string[] words;
            switch (symb)
            {
                case "":
                    //エラー この関数を呼ぶ前にtxtA,bに中身があるか確認してるからここには来ないはず
                    MessageBox.Show("コンボボックスが未選択です");
                    fl = false;
                    break;
                case "="://   "="memo 1つのレコード作成
                    if (datatype != 2)
                    {
                        //エラー 数値入力なのに単一文字列が含まれている
                        fl = false; break;
                    }
                    var record0 = new Record(EWSID.Text);

                    record0.Score = score;
                    record0.VitalCode = vitalcode;
                    record0.CriteriaValue = txtA;
                    record0.CriteriaSign = 2;
                    record0.Target = tarval;
                    record0.SeqNo = seqno + 1;
                    record0.DisplayOrder = displayorder;

                    RecList.Add(record0);
                    break;
                case ",":// , memo レコード数に限りない
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
                        record1.VitalCode = vitalcode;
                        record1.CriteriaValue = word;
                        record1.CriteriaSign = 2;
                        record1.Target = tarval;
                        record1.SeqNo = seqno + 1;
                        record1.DisplayOrder = displayorder;

                        RecList.Add(record1);
                    }
                    break;
                case "～"    :// ~ memo 2つレコードを作ればいい
                    var record21 = new Record(EWSID.Text);
                    record21.Score = score;
                    record21.VitalCode = vitalcode;
                    record21.CriteriaValue = txtA;
                    record21.CriteriaSign = 1;
                    record21.Target = tarval;
                    record21.SeqNo = seqno + 1;
                    record21.DisplayOrder = displayorder;

                    RecList.Add(record21);

                    var record22 = new Record(EWSID.Text);
                    record22.Score = score;
                    record22.VitalCode = vitalcode;
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
                case "≦":// <= memo １つレコード：A,Bが空かどうか判別が必要かな一回ききたい
                    var record3 = new Record(EWSID.Text);
                    record3.Score = score;
                    record3.VitalCode = vitalcode;
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
                case "≧":// >= memo １つレコード：
                    var record4 = new Record(EWSID.Text);
                    record4.Score = score;
                    record4.VitalCode = vitalcode;
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

        /// <summary>
        /// DataTypeコンボボックス選択時処理(UPDATEページ)
        /// 数値、文字列の二つの条件に合わせてコンボボックスのアイテムを設定する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbDataTypeUP_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = Array.IndexOf(_cmbDataTypeUP, ((ComboBox)sender));
            //DataType == String
            if ( ((ComboBox)sender).SelectedIndex == 2)
            {
                for (int j = 0; j < 7; j++)
                {
                    bool fl = false;
                    string s = "";
                    if (_cmb[index, j].SelectedIndex != -1)
                    {
                        fl = true;
                        s = _cmb[index, j].Text;
                    }
                    _cmb[index, j].Items.Clear();
                    _cmb[index, j].Items.Add("");
                    _cmb[index, j].Items.Add("=");
                    _cmb[index, j].Items.Add(",");

                    if (fl)
                    {
                        _cmb[index, j].SelectedIndex = _cmb[index, j].FindString(s);
                    }
                    else
                    {
                        _cmb[index, j].SelectedIndex = 0;
                    }
                   
                }
            }
            else if( ((ComboBox)sender).SelectedIndex == -1)
            {
                for (int j = 0; (j < 7); j++)
                {
                    bool fl = false;
                    string s = "";
                    if (_cmb[index, j].SelectedIndex != -1)
                    {
                        fl = true;
                        s = _cmb[index, j].Text;
                    }
                    _cmb[index, j].Items.Clear();
                    _cmb[index, j].Items.Add("");
                    _cmb[index, j].Items.Add("=");
                    _cmb[index, j].Items.Add(",");
                    _cmb[index, j].Items.Add("～");
                    _cmb[index, j].Items.Add("≦");
                    _cmb[index, j].Items.Add("≧");

                    if (fl)
                    {
                        _cmb[index, j].SelectedIndex = _cmb[index, j].FindString(s);
                    }
                    else
                    {
                        _cmb[index, j].SelectedIndex = 0;
                    }
                }
            }
            else
            {
                for (int j = 0; j < 7; j++)
                {
                    bool fl = false;
                    string s = "";
                    if (_cmb[index, j].SelectedIndex != -1)
                    {
                        fl = true;
                        s = _cmb[index, j].Text;
                    }
                    _cmb[index, j].Items.Clear();
                    _cmb[index, j].Items.Add("");
                    _cmb[index, j].Items.Add("～");
                    _cmb[index, j].Items.Add("≦");
                    _cmb[index, j].Items.Add("≧");
                    if (fl)
                    {
                        _cmb[index, j].SelectedIndex = _cmb[index, j].FindString(s);
                    }
                    else
                    {
                        _cmb[index, j].SelectedIndex = 0;
                    }
                }
            }

        }

        private void btnInitforCreate_Click(object sender, EventArgs e)
        {
            //表に値が入力済みかチェックして初期化
            bool fl_Completed = false;
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    if (_txtCriteiaValueA[i, j].Text != "" || _txtCriteiaValueB[i, j].Text != "")
                    {
                        fl_Completed = true;
                        break;
                    }
                }
                if (fl_Completed)
                {
                    ////ファイルの行番号取得           
                    string strMsg = "入力中の値は保存されませんが初期化しますか？";

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
            AllClear();//shundbg 入力されているが消してもいいか？って聞く処理がない
            InitTxtBoxColor();

            //登録されているvitalcodeを取ってきてスタックしておく（重複を許していない）
            var vitalcodes = new HashSet<string>();
            //登録可能なIDをDBからとってくる。登録時に勝手に付けられるから意味ないけど画面で見れたほうがいいかもだから残す
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
                    txtCreateEWSID.Text = (Id + 1).ToString();
                }
                sdr.Close();
                com.Dispose();
            }
            finally
            {
                con.Close();
            }
        }

        private void txtCriteiaValueA_TextChanged(object sender, EventArgs e)
        {
            var ind = new indexx();
            ind = Text_IndexofIandJ( _txtCriteiaValueA, ((TextBox)sender));
            if (Save_TxtA[ind.i,ind.j] != _txtCriteiaValueA[ind.i, ind.j].Text)
            {
                _txtCriteiaValueA[ind.i, ind.j].BackColor = Color.Yellow;
            }
            else
            {
                _txtCriteiaValueA[ind.i, ind.j].BackColor = SystemColors.Window;
            }
        }

        private void txtCriteiaValueB_TextChanged(object sender, EventArgs e)
        {
            var ind = new indexx();
            ind = Text_IndexofIandJ(_txtCriteiaValueB, ((TextBox)sender));
            if (Save_TxtB[ind.i, ind.j] != _txtCriteiaValueB[ind.i, ind.j].Text)
            {
                _txtCriteiaValueB[ind.i, ind.j].BackColor = Color.Yellow;
            }
            else
            {
                _txtCriteiaValueB[ind.i, ind.j].BackColor = SystemColors.Window;
            }
        }
        /// <summary>
        /// ボックスの背景色をデフォルトに戻す
        /// </summary>
        private void InitTxtBoxColor()
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    if(i==0 && j != 3) _txtScore[j].BackColor = SystemColors.Window;
                    _txtCriteiaValueA[i,j].BackColor = SystemColors.Window;
                    _txtCriteiaValueB[i,j].BackColor = SystemColors.Window;
                    _cmb[i,j].BackColor = SystemColors.Window;
                }
                _txtDisplayOrder[i].BackColor = SystemColors.Window;
                _cmbDataTypeUP[i].BackColor = SystemColors.Window;
                _vitalcode[i].BackColor = SystemColors.Window;
            }
        }

        private indexx Text_IndexofIandJ(TextBox[,] list, TextBox obj)
        {
            var res = new indexx();
            for(int i=0; i < 10; i++)
            {
                for(int j = 0; j < 7; j++)
                {
                    if (list[i,j] == obj)
                    {
                        res.i = i;
                        res.j = j;
                        return res;
                    }
                }
            }
            return (res);
        }
        private indexx Combo_IndexofIandJ(ComboBox[,] list, ComboBox obj)
        {
            var res = new indexx();
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    if (list[i, j] == obj)
                    {
                        res.i = i;
                        res.j = j;
                        return res;
                    }
                }
            }
            return (res);
        }
        public class indexx
        {
            public int i;
            public int j;
            public indexx()
            {
                i = 99;
                j = 87;
            }
        }

        private void symbolcmb_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ind = new indexx();
            ind = Combo_IndexofIandJ(_cmb, ((ComboBox)sender));
            if ( ((ComboBox)sender).SelectedItem.ToString() == "≦" || ((ComboBox)sender).SelectedItem.ToString()  == "≧")
            {
                _txtCriteiaValueA[ind.i, ind.j].Enabled = false;
            }
            else
            {
                _txtCriteiaValueA[ind.i, ind.j].Enabled = true;
            }

            //前回値と違っていれば色を変える＊今は全部変わる
            if (Save_Cmb[ind.i, ind.j] != _cmb[ind.i, ind.j].Text)
            {
                _cmb[ind.i, ind.j].BackColor = Color.Yellow;
            }
            else
            {
                _cmb[ind.i, ind.j].BackColor = SystemColors.Window;
            }     
        }

        private void txtDisplayOrder_TextChanged(object sender, EventArgs e)
        {
            for(int i = 0; i<10; i++)
            {
                if( ((TextBox)sender) == _txtDisplayOrder[i] ){
                    if( Save_DisplayOrder[i] != _txtDisplayOrder[i].Text)
                    {
                        _txtDisplayOrder[i].BackColor = Color.Yellow;
                    }
                    else
                    {
                        _txtDisplayOrder[i].BackColor = SystemColors.Window;
                    }
                }
            }
        }

        private void DataTypeCmb_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                if (((ComboBox)sender) == _cmbDataTypeUP[i])
                {
                    if( Save_DataType[i] != _cmbDataTypeUP[i].SelectedIndex)
                    {
                        _cmbDataTypeUP[i].BackColor = Color.Yellow;
                    }
                    else
                    {
                        _cmbDataTypeUP[i].BackColor = SystemColors.Window;
                    }
                }
            }
        }
        private void VitalCodeCmb_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                if ( ((ComboBox)sender) == _vitalcode[i] )
                {
                    if (Save_VitalCode[i] != _vitalcode[i].Text)
                    {
                        _vitalcode[i].BackColor = Color.Yellow;
                    }
                    else
                    {
                        _vitalcode[i].BackColor = SystemColors.Window;
                    }                  
                }
            }
        }

        private void txtScore_TextChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < 7; i++)
            {
                if(i != 3)
                {
                    if(Save_Score[i] != _txtScore[i].Text)
                    {
                        _txtScore[i].BackColor = Color.Yellow;
                    }
                    else
                    {
                        _txtScore[i].BackColor = SystemColors.Window;
                    }
                }
            }
        }

    }
}
