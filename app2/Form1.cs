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

namespace app2
{



    public partial class Form1 : Form
    {
        private TextBox[,] _txtCriteiaValueA;
        private TextBox[,] _txtCriteiaValueB;
        private ComboBox[,] _cmb;
        private ComboBox[] _vitalcode;
        private int[] _score = new int[] { 3, 2, 1, 0, 1, 2, 3 };
        private int[] tarval = new int[] { 0, 0, 0, 1, 1, 1, 1 };
        private SortedDictionary<string, int> EwsName= new SortedDictionary<string, int>();
        private SortedDictionary<string, int> DicVitalcodeandDisplayOrder = new SortedDictionary<string, int>();

        //vital配列10、score配列7、リストを作りたい
        List<Record>[,] GetRecords = new List<Record>[11, 7];
        /*private string[] VitalCodeName = new string[]
        {
            "...", "rr","spo2","oxygen","sbp", "pulse","loc","temperature", "dbp", "6th_sense", "rr_eng", "oxygen_eng", "sbp_eng", "pulse_eng", "loc_eng", "temperature_eng"
        };*/
        //private string[] VitalCodeName;
        List<string> VitalCodeName = new List<string>();
        public Form1()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 受け取った入力情報からレコードを作りRecListに追加する
        ///
        /// </summary>
        /// <param name="RecList"></param>
        /// <param name="vitalcode"></param>
        /// <param name="txtA"></param>
        /// <param name="symb"></param>
        /// <param name="txtB"></param>
        /// <param name="score"></param>
        /// <param name="tarval"></param>
        public void CreatRecord(List<Record> RecList, int vitalcode, string txtA, int symb, string txtB, int score, int tarval, int displayorder, int seqno)
        {
            //string[] words;
            switch (symb)
            {
                case 0://   memo 1つのレコード作成
                    var record0 = new Record(EWSID.Text);
                    record0.Score = score;
                    record0.VitalCode = VitalCodeName[vitalcode];
                    record0.CriteriaValue = txtA;
                    record0.CriteriaSign = 2;
                    record0.Target = tarval;
                    record0.DisplayOrder = displayorder;

                    RecList.Add(record0);
                    break;
                case 1:// , memo レコード数に限りない
                    string[] words = txtA.Split(',');
                    foreach(var word in words)
                    {
                        var record1 = new Record(EWSID.Text);
                        record1.Score = score;
                        record1.VitalCode = VitalCodeName[vitalcode];
                        record1.CriteriaValue = word;
                        record1.CriteriaSign = 2;
                        record1.Target = tarval;
                        record1.DisplayOrder = displayorder;

                        RecList.Add(record1);
                    }
                    break;
                case 2:// ~ memo 2つレコードを作ればいい
                    var record21 = new Record(EWSID.Text);
                    record21.Score = score;
                    record21.VitalCode = VitalCodeName[vitalcode];
                    record21.CriteriaValue = txtA;
                    record21.CriteriaSign = 1;
                    record21.Target = tarval;
                    record21.DisplayOrder = displayorder;

                    RecList.Add(record21);

                    var record22 = new Record(EWSID.Text);
                    record22.Score = score;
                    record22.VitalCode = VitalCodeName[vitalcode];
                    record22.CriteriaValue = txtB;
                    record22.CriteriaSign = 0;
                    record22.Target = tarval;
                    record22.DisplayOrder = displayorder;

                    RecList.Add(record22);
                    break;
                case 3:// <= memo １つレコード：A,Bが空かどうか判別が必要かな一回ききたい
                    var record3 = new Record(EWSID.Text);
                    record3.Score = score;
                    record3.VitalCode = VitalCodeName[vitalcode];
                    record3.CriteriaValue = txtB;
                    record3.CriteriaSign = 0;
                    record3.Target = tarval;
                    record3.DisplayOrder = displayorder;

                    RecList.Add(record3);
                    break;
                case 4:// >= memo １つレコード：A,Bが空かどうか判別が必要一回聞きたいそういう表記になってるだけで入力するときの感覚的にはおかしいかもしれない
                    var record4 = new Record(EWSID.Text);
                    record4.Score = score;
                    record4.VitalCode = VitalCodeName[vitalcode];
                    record4.CriteriaValue = txtB;
                    record4.CriteriaSign = 1;//shundbg 
                    record4.Target = tarval;
                    record4.DisplayOrder = displayorder;

                    RecList.Add(record4);
                    break;
            }
        }
 
        private void button1_Click(object sender, EventArgs e)
        {
            var RecordList = new List<Record>();

            for (int i = 0; i < 10; i++)//iは行数 10の部分を定数にしたほうがきれいかも
            {
                for (int j = 0; j < 7; j++)//jは列数　７の部分を定数にしたほうがきれいかも
                {
                    if (_txtCriteiaValueA[i, j].Text == "" && _txtCriteiaValueB[i, j].Text == "")
                    {
                        continue;
                    }
                    else
                    {
                        CreatRecord(RecordList, _vitalcode[i].SelectedIndex, _txtCriteiaValueA[i, j].Text, _cmb[i, j].SelectedIndex, _txtCriteiaValueB[i, j].Text, _score[j], tarval[j], i + 1, Convert.ToInt32(txtSeqNo.Text));
                    }
                    
                }
            }

            //EVISCloudに接続
            string constr = @"Data Source=192.168.1.174;Initial Catalog=EVISCloud;Integrated Security=False;User ID=sa;Password=P@ssw0rd";

            SqlConnection con = new SqlConnection(constr);
            con.Open();
            try
            {
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
                    //sb.Append($"VALUES({record.EWSId}, {record.SeqNo}, '{record.VitalCode}', {record.Score}, '{record.CriteriaValue}', {record.CriteriaSign}, {record.Target}, {record.DisplayOrder})");
                    sb.Append($"VALUES( {record.EWSId}, {record.SeqNo}, '{record.VitalCode}', {record.Score}, '{record.CriteriaValue}', {record.CriteriaSign}, {record.Target}, {record.DisplayOrder})");
                    SqlCommand com  = new SqlCommand(sb.ToString(), con);

                    //過去分のinvalidflagを立てる必要がある
                    var result = com.ExecuteNonQuery();

                }
            }
            finally
            {
                con.Close();
            }
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

            public Record (string ID) {
                EWSId = int.Parse(ID);
            }
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitControl();
            InitComboBox();
            InitEwsName();
        }
        /// <summary>
        /// DB->app スコア表示用関数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConnectSql_Click(object sender, EventArgs e)
        {
            AllClear();
            //GetRecords初期化
            for (int i=0; i<11; i++)
            {
                for(int j = 0; j<7; j++)
                {
                    GetRecords[i,j] = new List<Record>();
                }
            }
            //EwsNameに対応するEwsidのレコードを取得しtxtに出力する
            string constr = @"Data Source=192.168.1.174;Initial Catalog=EVISCloud;Integrated Security=False;User ID=sa;Password=P@ssw0rd";
            SqlConnection con = new SqlConnection(constr);
            con.Open();
            try
            {
                //SQL文作成:
                //string sqlstr = $"SELECT * FROM T_EwsScoreCriteria WHERE EwsId = {EwsName[cmbEwsName.SelectedItem.ToString()]} AND InvalidFlag = 0";
                //sqlstr += $"AND SeqNo = (SELECT MAX(SeqNo) FROM T_EwsScoreCriteria WHERE EwsId = {EwsName[cmbEwsName.SelectedItem.ToString()]})";

                //test shundbg 
                string sqlstr = $"SELECT * FROM T_EwsScoreCriteria WHERE EwsId =3000 AND InvalidFlag = 0";
                sqlstr += $"AND SeqNo = (SELECT MAX(SeqNo) FROM T_EwsScoreCriteria WHERE EwsId = 3000)";
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
                    //txtOutSql.Text += string.Format($"{EwsId}, {SeqNo}, {VitalCode}, {Score}, {CriteriaValue}, {CriteriaSign}, {Target}, {Displayorder} \r\n");

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

                    //GetRecordリストにDBから受けた情報をスコア別に格納
                    if (Target == 0)
                    {
                        if(Score <=3)
                        {
                            GetRecords[Displayorder, 3 - Score].Add(record);
                        }
                        
                    }
                    else
                    {
                        if(Score <=3)
                        {
                            GetRecords[Displayorder, 3 + Score].Add(record);
                        }
                        else
                        {
                            //error文出したい
                        }
                    }
                }
                sdr.Close();
                com.Dispose();
            }
            finally
            {
                con.Close();
            }

            //GetRecords[0][]に入ってしまった分をvitalcodeをもとに1<=iに振り分ける
            for(int i=0; i < 7; i++)
            {
                foreach (var record in GetRecords[0,i])
                {
                    GetRecords[DicVitalcodeandDisplayOrder[record.VitalCode], i].Add(record);
                }
            }

            //******修正して！！二重forするようなこと？？？？->record配列のどこに要素が入っているかわからないから２重で回すしかない。フラグで早くぬけてるからそんな回してるわけでもない
            //cmbvitalcodeを更新
            for (int i = 0; i < 10; i++)
            {
                //cmbvitalcode クリアする
                _vitalcode[i].Items.Clear();
                bool fl = false;
                //Vitalcode追加,選択
                for (int j = 0; j < 7; j++)
                {
                    if (GetRecords[i+1, j].Count != 0)
                    {
                        //Getrecordsにはi=1から実データが入ってる
                        _vitalcode[i].Items.Add(GetRecords[i+1, j].First().VitalCode);
                        _vitalcode[i].SelectedIndex = 0;
                        fl = true;
                    }
                    if (fl) break;
                }

            }

            //suhndbg 振り分けたレコードが正しいか確認
            for (int i = 1;i < 11; i++)
            {
                for(int j = 0;j < 7; j++)
                {
                    foreach(var s in GetRecords[i, j])
                    {
                        OutRecord(s);
                    }
                    txtOutSql.Text += "|||||||||||||||||||||||||||\r\n";
                }
                txtOutSql.Text += "*******************\r\n";
            }

            //レコード表示処理
            OutScore();


        }

        private void OutScore ()
        {

            for(int i=0; i< 10; i++) 
            {
                for (int j = 0; j < 7; j++)
                {
                    if (GetRecords[i+1, j].Count() == 0)
                    {
                        continue;
                    }
                    else if (GetRecords[i+1, j].First().CriteriaSign == 2)
                    {
                        //case 1 ","
                        if (GetRecords[i + 1, j].Count() >= 2)
                        {
                            foreach (var record in GetRecords[i+1, j])
                            {
                                _txtCriteiaValueA[i, j].Text += record.CriteriaValue + ",";
                            }
                            _txtCriteiaValueA[i, j].Text = _txtCriteiaValueA[i, j].Text.Remove(_txtCriteiaValueA[i, j].Text.Length-1);
                            
                            _cmb[i, j].SelectedIndex = 1;
                        }
                        //case 0 " "
                        else
                        {
                            _txtCriteiaValueA[i, j].Text += GetRecords[i+1, j].First().CriteriaValue;
                            _cmb[i, j].SelectedIndex = 0;
                        }
                    }
                    //case2 "～"
                    else if (GetRecords[i+1, j].Count() == 2)
                    {
                        foreach (var record in GetRecords[i+1, j])
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
                        _cmb[i, j].SelectedIndex = 2;
                    }
                    //case4,5 "<=" or ">="
                    else if (GetRecords[i+1, j].Count() == 1)
                    {
                        if (GetRecords[i+1, j].First().CriteriaSign == 0)
                        {
                            _txtCriteiaValueB[i, j].Text += GetRecords[i + 1, j].First().CriteriaValue;
                            _cmb[i, j].SelectedIndex = 3;
                        }
                        else
                        {
                            _txtCriteiaValueB[i, j].Text += GetRecords[i+1, j].First().CriteriaValue;
                            _cmb[i, j].SelectedIndex = 4;
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
        private void OutRecord (Record record)
        {
            string outstr = "";

            outstr += string.Format($"EwsId:{record.EWSId}, SeqNo:{record.SeqNo}, VitalCode:{record.VitalCode}, Score:{record.Score}, CriteriaValue:{record.CriteriaValue}, CriteriaSign:{record.CriteriaSign}, Target:{record.Target}, DisplayOrder:{record.DisplayOrder}  \r\n");
            txtOutSql.Text += outstr;
        }

        private void InitEwsName()
        {
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
        private void InitComboBox()
        {
            for (int i = 0; i < 10; i++)
            {
                //A,Bの間のコンボボックス初期化
                for (int j = 0; j < 7; j++)
                {
                    _cmb[i, j].Items.Clear();
                    _cmb[i, j].Items.Add("");
                    _cmb[i, j].Items.Add(",");
                    _cmb[i, j].Items.Add("～");
                    _cmb[i, j].Items.Add("≦");
                    _cmb[i, j].Items.Add("≧");
                    _cmb[i, j].SelectedIndex = 0;
                }

                _vitalcode[i].SelectedIndex = 0;
                _vitalcode[i].Items.Clear();



                string[] VitalTypeFile = File.ReadAllLines("..\\..\\VitalType.txt");
                var splits = new List<string>();
                int ele = 0;
                foreach (string s in VitalTypeFile)
                {
                    _vitalcode[i].Items.Add(s);
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

        private void InitControl()
        {
            _txtCriteiaValueA = new TextBox[10, 7];//vitaltype, score
            _txtCriteiaValueB = new TextBox[10, 7];//vitaltype, score
            _cmb = new ComboBox[10, 7];//vitaltype, score
            _vitalcode = new ComboBox[10];

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

        }
        private void AllClear()
        {
            for(int i =0; i< 10; i++)
            {
                for(int j = 0; j < 7; j++)
                {
                    _txtCriteiaValueA[i, j].ResetText();
                    _txtCriteiaValueB[i, j].ResetText();
                    _cmb[i, j].SelectedIndex = 0;
                }
                _vitalcode[i].Items.Clear();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var RecordList = new List<Record>();

            for (int i = 0; i < 10; i++)//iは行数 10の部分を定数にしたほうがきれいかも
            {
                for (int j = 0; j < 7; j++)//jは列数　７の部分を定数にしたほうがきれいかも
                {
                    if (_txtCriteiaValueA[i, j].Text == "" && _txtCriteiaValueB[i, j].Text == "")
                    {
                        continue;
                    }
                    else
                    {
                        CreatRecord_Update(RecordList, i, _txtCriteiaValueA[i, j].Text, _cmb[i, j].SelectedIndex, _txtCriteiaValueB[i, j].Text, _score[j], tarval[j], i + 1, Convert.ToInt32(txtSeqNo.Text));
                    }
                }
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
                    //sb.Append($"VALUES( {record.EWSId}, {record.SeqNo}, '{record.VitalCode}', {record.Score}, '{record.CriteriaValue}', {record.CriteriaSign}, {record.Target}, {record.DisplayOrder})");
                    sb.Append($"VALUES( 3000, {record.SeqNo}, '{record.VitalCode}', {record.Score}, '{record.CriteriaValue}', {record.CriteriaSign}, {record.Target}, {record.DisplayOrder})");
                    SqlCommand com = new SqlCommand(sb.ToString(), con);

                    var result = com.ExecuteNonQuery();

                    if (first)
                    {
                        //過去分のinvalidflagを立てる必要がある
                        string sqlreststr = $"UPDATE T_EwsScoreCriteria SET InvalidFlag = 1 WHERE EwsId = {record.EWSId} AND SeqNo = {record.SeqNo -1}";
                        SqlCommand comreset = new SqlCommand(sqlreststr, con);
                        var resultreset = comreset.ExecuteNonQuery();
                        first = false;
                    }
                }
            }
            finally
            {
                con.Close();
            }
        }

        public void CreatRecord_Update(List<Record> RecList, int vitalcode, string txtA, int symb, string txtB, int score, int tarval, int displayorder, int seqno)
        {
            //string[] words;
            switch (symb)
            {
                case 0://   memo 1つのレコード作成
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
                case 1:// , memo レコード数に限りない
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
                case 2:// ~ memo 2つレコードを作ればいい
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
                    break;
                case 3:// <= memo １つレコード：A,Bが空かどうか判別が必要かな一回ききたい
                    var record3 = new Record(EWSID.Text);
                    record3.Score = score;
                    record3.VitalCode = _vitalcode[vitalcode].Items[0].ToString();
                    record3.CriteriaValue = txtB;
                    record3.CriteriaSign = 0;
                    record3.Target = tarval;
                    record3.SeqNo = seqno + 1;
                    record3.DisplayOrder = displayorder;

                    RecList.Add(record3);
                    break;
                case 4:// >= memo １つレコード：A,Bが空かどうか判別が必要一回聞きたいそういう表記になってるだけで入力するときの感覚的にはおかしいかもしれない
                    var record4 = new Record(EWSID.Text);
                    record4.Score = score;
                    record4.VitalCode = _vitalcode[vitalcode].Items[0].ToString();
                    record4.CriteriaValue = txtB;
                    record4.CriteriaSign = 1;//shundbg 
                    record4.Target = tarval;
                    record4.SeqNo = seqno + 1;
                    record4.DisplayOrder = displayorder;

                    RecList.Add(record4);
                    break;
            }
        }
    }

}
