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
using System.Runtime.InteropServices.WindowsRuntime;

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

        private DB _db;

        private int[] _score = new int[7];
        private int[] tarval = new int[] { 0, 0, 0, 1, 1, 1, 1 };
        private SortedDictionary<string, int> EwsName = new SortedDictionary<string, int>();
        public SortedDictionary<string, int> DicVitalcodeandDisplayOrder = new SortedDictionary<string, int>();

        //vital配列10、score配列7、リストを作りたい
        List<Record>[,] GetRecords = new List<Record>[11, 7];

        public Form2()
        {
            this._db = new DB();
            InitializeComponent();
            InitControl();
            InitComboBox();
            InitEwsName();
            InitTxtBoxColor();
        }
        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private bool Check_Completed()
        {
            bool fl_Completed = false;
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    if (_txtCriteiaValueA[i, j].Text != "" || _txtCriteiaValueB[i, j].Text != "" || _cmb[i, j].SelectedIndex != 0)
                    {
                        fl_Completed = true;
                        break;
                    }
                }
            }
            return fl_Completed;
        }

        private void Form2_FormClosing(Object sender, FormClosingEventArgs e)
        {
            
            if (Check_Completed())
            {
                ////ファイルの行番号取得           
                string strMsg = "表に値が書き込まれています、終了しますか？";

                //メッセージボックスで行番号を表示
                var resultMessageBox = MessageBox.Show(strMsg
                                , "エラー"
                                , MessageBoxButtons.OKCancel
                                , MessageBoxIcon.Information);

                if (resultMessageBox == System.Windows.Forms.DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }

        }

        /// <summary>
        /// 入力されたスコアを判定用の配列にコピー、コピー時降順になっているかチェック
        /// </summary>
        /// <returns>
        /// True,false=スコアが降順に入力されていない
        /// </returns>
        private int Update_scorearray()
        {
            int sum = 0;
            if (txtScoreLv3L.Text != "")
            {
                if(!Int32.TryParse(txtScoreLv3L.Text, out _score[0]))
                {
                    sum += 4;
                }
                
            }
            else _score[0] = -1;

            if (txtScoreLv2L.Text != "")
            {
                if (!Int32.TryParse(txtScoreLv2L.Text, out _score[1]))
                {
                    sum += 2;
                }

            }
            else _score[1] = -1;

            if (txtScoreLv1L.Text != "")
            {
                if (!Int32.TryParse(txtScoreLv1L.Text, out _score[2]))
                {
                    sum += 1;
                }

            }
            else _score[2] = -1;

           _score[3] = 0;

            if (txtScoreLv1R.Text != "")
            {
                if (!Int32.TryParse(txtScoreLv1R.Text, out _score[4]))
                {
                   // return 12;//スコアの入力値は数値である必要があります。
                }

            }
            else _score[4] = -1;

            if (txtScoreLv2R.Text != "")
            {
                if (!Int32.TryParse(txtScoreLv2R.Text, out _score[5]))
                {
                    //return 12;//スコアの入力値は数値である必要があります。
                }

            }
            else _score[5] = -1;

            if (txtScoreLv3R.Text != "")
            {
                if (!Int32.TryParse(txtScoreLv3R.Text, out _score[6]))
                {
                    //return 12;//スコアの入力値は数値である必要があります。
                }

            }
            else _score[6] = -1;

            if( 0 < sum)
            {
                return 50 + sum;
            }
            var ScoreCheck = new List<int>();

            for (int i=0;i<3;i++)
            {
                if (_score[i] != -1) ScoreCheck.Add(_score[i]);                
            }

            if (ScoreCheck.Count == 0)
            {
                return 2;//スコアが未設定です。
            }
            else
            {
                for (int i = 0; i < ScoreCheck.Count - 1; i++)
                {
                    if (ScoreCheck[i] <= ScoreCheck[i + 1])
                    {
                        return 1;//スコアが0に向かって降順に設定されていません。
                    }
                }
            }
            return 0;           
        }

        /// <summary>
        /// SeqNo=1を指定してレコードを作成する　create用
        /// 新規追加用
        /// </summary>
        /// <param name="RecList"></param>
        /// <param name="vitalcode"></param>
        /// <param name="txtA"></param>
        /// <param name="symb"></param>
        /// <param name="txtB"></param>
        /// <param name="score"></param>
        /// <param name="tarval"></param>
        public int CreatRecord(List<Record> RecList, List<int> intlist, int EwsId,  string vitalcode, string txtA, string symb, string txtB, int score, int tarval, int displayorder,int datatype)
        {
            return CreatRecord(RecList,intlist, EwsId, vitalcode, txtA, symb, txtB, score, tarval, displayorder, 1, datatype);
        }

        /// <summary>
        /// レコード作成　
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
        public int CreatRecord(List<Record> RecList, List<int> intlist, int EwsId, string vitalcode, string txtA, string symb, string txtB, int score, int tarval, int displayorder, int seqno, int datatype)
        {
            double d;
            int i;
            int fl = 0;

            if (score == -1)
            {
                //エラー　入力値に対するスコアがありません。
                return 7;
            }
            //string[] words;
            switch (symb)
            {
                case "":
                    //エラー 記号コンボボックスが未設定です。ここには来ないが
                    fl = 6;
                    break;
                case "="://   "="memo 1つのレコード作成
                    var record0 = new Record();

                    record0.EWSId = EwsId;
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
                    string[] words = txtA.Split(',');
                    foreach (var word in words)
                    {
                        var record1 = new Record();

                        record1.EWSId = EwsId;
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
                case "～":// ~ memo 2つレコードを作ればいい
                    //入力制限判定用
                    //double入力
                    if (double.TryParse(txtB, out d) && txtB.Contains('.'))
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
                    else if (int.TryParse(txtB, out i))
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
                        if (!(double.TryParse(txtA, out d) && txtA.Contains('.')) && !(int.TryParse(txtA, out i)))//shundbg 少し不安な条件式かも
                        {
                            //エラー　データタイプが数値型の場合文字列の入力は出来ません。
                            fl = 100;
                            break;
                        }
                        //エラー　データタイプが数値型の場合文字列の入力は出来ません。
                        fl = 11;
                        break;
                    }

                    if (!(double.TryParse(txtA, out d) && txtA.Contains('.')) && !(int.TryParse(txtA, out i)))//shundbg 少し不安な条件式かも
                    {
                        //エラー　データタイプが数値型の場合文字列の入力は出来ません。
                        fl = 8;
                        break;
                    }

                    var record21 = new Record();

                    record21.EWSId = EwsId;
                    record21.Score = score;
                    record21.VitalCode = vitalcode;
                    record21.CriteriaValue = txtA;
                    record21.CriteriaSign = 1;
                    record21.Target = tarval;
                    record21.SeqNo = seqno + 1;
                    record21.DisplayOrder = displayorder;

                    RecList.Add(record21);

                    var record22 = new Record();

                    record22.EWSId = EwsId;
                    record22.Score = score;
                    record22.VitalCode = vitalcode;
                    record22.CriteriaValue = txtB;
                    record22.CriteriaSign = 0;
                    record22.Target = tarval;
                    record22.SeqNo = seqno + 1;
                    record22.DisplayOrder = displayorder;

                    RecList.Add(record22);

                    break;
                case "≦":// <= memo １つレコード：A,Bが空かどうか判別が必要かな一回ききたい
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
                        //エラー　データタイプが数値型の場合文字列の入力は出来ません。
                        fl = 8;
                        break;
                    }
                    var record3 = new Record();
                    record3.EWSId = EwsId;
                    record3.Score = score;
                    record3.VitalCode = vitalcode;
                    record3.CriteriaValue = txtB;
                    record3.CriteriaSign = 0;
                    record3.Target = tarval;
                    record3.SeqNo = seqno + 1;
                    record3.DisplayOrder = displayorder;

                    RecList.Add(record3);

                    break;
                case "≧":// >= memo １つレコード：
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
                        //エラー　データタイプが数値型の場合文字列の入力は出来ません。
                        fl = 11;
                        break;
                    }
                    var record4 = new Record();
                    record4.EWSId = EwsId;
                    record4.Score = score;
                    record4.VitalCode = vitalcode;
                    record4.CriteriaValue = txtB;
                    record4.CriteriaSign = 1;//shundbg 
                    record4.Target = tarval;
                    record4.SeqNo = seqno + 1;
                    record4.DisplayOrder = displayorder;

                    RecList.Add(record4);

                    break;
            }
            return fl;
        }

        private void CreatButton_Click()
        {
            var RecordList = new List<Record>();
            var ErrorList  = new List<ErrorRecord>();
            
            //エラーチェック変数 0がエラーなし
            int check_input = 0;

            if(cmbEwsName.SelectedIndex == 0)
            {
                //Ewsname,WarnigThreshholdsが入力済みか確認
                if (txtCreateEwsName.Text == "")
                {
                    var err = new ErrorRecord(16, 99, 99);
                    ErrorList.Add(err);
                    check_input = 16;
                }
                if (txtCreateWarningThresolds.Text == "")
                {
                    var err = new ErrorRecord(17, 99, 99);
                    ErrorList.Add(err);
                    check_input = 17;
                }
            }

            //Scoreの入力が正常値か確認
            check_input = Update_scorearray();
            if (check_input != 0)
            {
                if(check_input > 50)
                {
                    int num = check_input % 10;
                    for(int i= 0; i<3; i++)
                    {
                        //scorelv,３か所のフラグを調べて立っている部分にエラーを出す。
                        if( (num & ( 1 << i)) > 0)
                        {
                            var err = new ErrorRecord(12, i, 99);
                            ErrorList.Add(err);
                        }
                    }
                }
                else
                {
                    var err = new ErrorRecord(check_input, 99, 99);
                    ErrorList.Add(err);
                }
            }
            //DisplayOrderの入力値が正常値か確認
            check_input = Check_DisplayOrder(ErrorList);

            //レコード登録時に必要な情報でエラーがあった場合レコード処理の前に処理を終わる
            if (ErrorList.Count != 0)
            {
                //ErrorListにたまったエラーを処理して背景色に反映＋テキストログに残す
                foreach (var err in ErrorList)
                {
                    Error_ColorChange(err);
                    txtOutSql.Text += $"Errid = {err.ErrorId}  i = {err.i},  j = {err.j} ErrMessage = {ErrorMessage[err.ErrorId]} \r\n";
                }
                ////ファイルの行番号取得           
                string strMsg = "入力値が間違っているため終了します。";
                //メッセージボックスで行番号を表示
                MessageBox.Show(strMsg
                                , "エラー"
                                , MessageBoxButtons.OK
                                , MessageBoxIcon.Information);
                return;
            }

            //DisplayOrderに重複がないか確認
            List<int>[] DispList = new List<int>[10];
            for (int i = 0; i < 10; i++)
            {
                DispList[i] = new List<int>();

            }
            for (int i = 0; i < 10; i++)
            {
                if (_txtDisplayOrder[i].Text != "")
                {
                    DispList[Convert.ToInt32(_txtDisplayOrder[i].Text) - 1].Add(i);
                }
            }
            for (int i = 0; i < 10; i++)
            {
                if (DispList[i].Count >= 2)
                {
                    foreach (var ii in DispList[i])
                    {
                        var err = new ErrorRecord(13, ii, 99);
                        ErrorList.Add(err);
                    }
                }
            }


            //入力情報をレコードに変換する
            for (int i = 0; i < 10; i++)//iは行数 10の部分を定数にしたほうがきれいかも
            {
                var RecordList_Vital = new List<Record>();
                var intlist = new List<int>();
                bool exrec = false;

                for (int j = 0; j < 7; j++)//jは列数　７の部分を定数にしたほうがきれいかも
                {
                    if (_txtCriteiaValueA[i, j].Text == "" && _txtCriteiaValueB[i, j].Text == "")
                    {
                        continue;
                    }
                    else if (_cmb[i, j].SelectedIndex == -1)
                    {
                        check_input = 6;//入力記号が設定されていません。
                        var err = new ErrorRecord(check_input, i, j);
                        ErrorList.Add(err);
                        continue;
                    }
                    else
                    {
                        exrec = true;
                        //新規登録選択時
                        if(cmbEwsName.SelectedIndex == 0)
                        {
                            check_input = CreatRecord(RecordList, intlist, Convert.ToInt32(txtCreateEWSID.Text), _vitalcode[i].Text, _txtCriteiaValueA[i, j].Text, _cmb[i, j].Text, _txtCriteiaValueB[i, j].Text, _score[j], tarval[j], Convert.ToInt32(_txtDisplayOrder[i].Text), _cmbDataTypeUP[i].SelectedIndex);
                        }
                        //データ更新選択時
                        else
                        {
                            check_input = CreatRecord(RecordList, intlist, Convert.ToInt32(EWSID.Text), _vitalcode[i].Text, _txtCriteiaValueA[i, j].Text, _cmb[i, j].Text, _txtCriteiaValueB[i, j].Text, _score[j], tarval[j], Convert.ToInt32(_txtDisplayOrder[i].Text), Convert.ToInt32(txtSeqNo.Text), _cmbDataTypeUP[i].SelectedIndex);
                        }
                    }
                    if (check_input != 0)
                    {
                        if(check_input == 100)
                        {
                            var errA = new ErrorRecord(8, i, j);
                            ErrorList.Add(errA);
                            var errB = new ErrorRecord(11, i, j);
                            ErrorList.Add(errB);
                        }
                        else
                        {
                            var err = new ErrorRecord(check_input, i, j);
                            ErrorList.Add(err);
                        }    
                    }
                }

                //その行が未入力ならエラーなしでいい
                if(exrec)
                {
                    if (_txtDisplayOrder[i].Text == "")
                    {
                        check_input = 3;//ディスプレイオーダーが設定されていません。
                        var err = new ErrorRecord(check_input, i, 99);
                        ErrorList.Add(err);
                    }
                    if (_cmbDataTypeUP[i].SelectedIndex == -1)
                    {
                        check_input = 4;//データタイプが設定されていません。
                        var err = new ErrorRecord(check_input, i, 99);
                        ErrorList.Add(err);
                    }
                    if (_vitalcode[i].SelectedIndex == -1)
                    {
                        check_input = 5;//バイタルコードが設定されていません。
                        var err = new ErrorRecord(check_input, i, 99);
                        ErrorList.Add(err);
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
                        //エラー　数値が連続していません。9
                        check_input = 9;
                        var err = new ErrorRecord(check_input, i, 99);
                        ErrorList.Add(err);
                    }
                }
            }

            if ( RecordList.Count == 0 )
            {
                check_input = 10;
                var err = new ErrorRecord(check_input, 99, 99);
                ErrorList.Add(err);
            }

            if ( ErrorList.Count != 0 )
            {
                //ErrorListにたまったエラーを処理して背景色に反映＋テキストログに残す
                foreach(var err in ErrorList)
                {
                    Error_ColorChange(err);
                    txtOutSql.Text += $"Errid = {err.ErrorId}  i = {err.i},  j = {err.j} ErrMessage = {ErrorMessage[err.ErrorId]} \r\n";
                }
                ////ファイルの行番号取得           
                string strMsg = "入力値が間違っているため終了します。";
                //メッセージボックスで行番号を表示
                MessageBox.Show(strMsg
                                , "エラー"
                                , MessageBoxButtons.OK
                                , MessageBoxIcon.Information);
                return;
            }

            int r;
       
            
            #region DB登録処理
            if (cmbEwsName.SelectedIndex == 0)
            {
                //M_EwsTypeへレコード登録
                this._db.InsertEwsName(txtCreateEwsName.Text, txtCreateWarningThresolds.Text);
                //T_EwsScoreCriteriaへレコード追加
                r = this._db.InsertRecord(RecordList);

                if (r != 0)
                {
                    MessageBox.Show("INSERT DONE");
                }
            }
            else
            {
                int EwsId = RecordList.First().EWSId;
                int SeqNo = RecordList.First().SeqNo;
                r = this._db.InsertRecord(RecordList);
                if(r != 0)
                {
                    this._db.UpdateInvalidFlag(EwsId, SeqNo-1);
                    if (r != 0)
                    {
                        MessageBox.Show("UPDATE DONE");
                    }
                }
            }
            #endregion

            #region　登録後画面設定処理
            if (cmbEwsName.SelectedIndex == 0)
            {
                InitEwsName();
                //新規登録後すぐに更新出来るようにデータセット
                EWSID.Text = txtCreateEWSID.Text.ToString();//あやしくなった
                txtSeqNo.Text = "1";
                cmbEwsName.SelectedIndex = cmbEwsName.FindString(txtCreateEwsName.Text);
                //登録後いったんボックスカラーをもとに戻す
                InitTxtBoxColor();
            }
            #endregion
        }
        /// <summary>
        /// DB->app スコア表示用関数 
        /// 変更　btnReadDB_Click -> ReadEwsScoreBoard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CmbEwsName_DropDownClosed(object sender, EventArgs e)
        {
            var scoreset = new SortedSet<int>();
            scoreset.Add(0);
            var stackrecords = new List<Record>();
            int[] scoreindex = new int[4];
            //表に値が入力済みかチェック
            if (Check_Completed())
            {
                ////ファイルの行番号取得           
                string strMsg = "表に値が書き込まれています、更新しますか？";

                //メッセージボックスで行番号を表示
                var resultMessageBox = MessageBox.Show(strMsg
                                , "エラー"
                                , MessageBoxButtons.OKCancel
                                , MessageBoxIcon.Information);

                if (resultMessageBox == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
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

            if( SelectedItem == "-新規追加-"){
                SelectedInitforCreate();
                cmbEwsName.SelectedIndex = selectindex;
                return;
            }

            //UPDATE時に必要のないテキストボックスを非表示に
            txtCreateEWSID.Enabled = false;
            txtCreateEwsName.Enabled = false;
            txtCreateWarningThresolds.Enabled = false;

            var stackrecord = new List<Record>();
            stackrecord = this._db.GetRecords(EwsName[SelectedItem]);//EwsName[SelectedItem]

            foreach(var record in stackrecord)
            {
                //EWSID,SeqNo保存 
                EWSID.Text = record.EWSId.ToString();
                txtSeqNo.Text = record.SeqNo.ToString();
                //setにScoreに使用されているレベル3種類を保存
                scoreset.Add(record.Score);
                if (DicVitalcodeandDisplayOrder.ContainsKey(record.VitalCode))
                {
                    if (DicVitalcodeandDisplayOrder[record.VitalCode] <= record.DisplayOrder)
                    {
                        DicVitalcodeandDisplayOrder[record.VitalCode] = record.DisplayOrder;
                    }
                }
                else
                {
                    DicVitalcodeandDisplayOrder.Add(record.VitalCode, record.DisplayOrder);
                }
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
            foreach(var record in stackrecord)
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
                            _cmb[i, j].SelectedIndex = 2;
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
        private int Check_DisplayOrder(List<ErrorRecord> list)
        {
            int ret = 0;
            for(int i=0; i<10; i++)
            {
                if (_txtDisplayOrder[i].Text == "") continue;

                if (Int32.TryParse(_txtDisplayOrder[i].Text, out int num)){
                    if( num > 10)
                    {
                        var err = new ErrorRecord(14, i, 99);//14 DisplayOrderは10より大きい値を入力出来ません。
                        list.Add(err);
                    }
                    ret = 14;
                }
                else
                {
                    var err = new ErrorRecord(15, i, 99);//15 DisplayOrderは数値以外の入力は出来ません。
                    list.Add(err);
                    ret = 15;
                }
            }
            return ret;
        }
        /// <summary>
        /// 起動時EwsNameコンボボックスにＤＢからEwsName一覧を取ってきて設定
        /// </summary>
        private void InitEwsName()
        {
            EwsName.Clear();
            cmbEwsName.Items.Clear();
            cmbEwsName.Items.Add("-新規追加-");

            _db.SetEwsName(EwsName, cmbEwsName);
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

            //登録されているvitalcodeを取ってきてスタックしておく（重複を許していない）
            var vitalcodes = new HashSet<string>();
            vitalcodes = _db.GetVitalCode();


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
                    Save_TxtA[i,j] = "";
                    Save_TxtB[i, j] = "";
                    Save_Cmb[i, j] = "";
                }
                _txtDisplayOrder[i].TextChanged += new System.EventHandler(txtDisplayOrder_TextChanged);
                _cmbDataTypeUP[i].SelectedIndexChanged += new System.EventHandler(DataTypeCmb_SelectedIndexChanged);
                _cmbDataTypeUP[i].FlatStyle = FlatStyle.Popup;
                _vitalcode[i].SelectedIndexChanged += new System.EventHandler(VitalCodeCmb_SelectedIndexChanged);
                _vitalcode[i].FlatStyle = FlatStyle.Popup;

                //save配列Null埋め
                Save_DisplayOrder[i] = "";
                Save_DataType[i] = -1;//あやしいかもしれない
                Save_VitalCode[i] = "";
                
            }
        }
        /// <summary>
        /// 表示中の表を削除
        /// </summary>
        private void AllClear()
        {
            //エラーボックス初期化
            txtOutSql.Text = "";
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

        private void txtScoreLv1R_TextChanged(object sender, EventArgs e)
        {
            txtScoreLv1L.Text = txtScoreLv1R.Text;
        }

        private void txtScoreLv2R_TextChanged(object sender, EventArgs e)
        {
            txtScoreLv2L.Text = txtScoreLv2R.Text;
        }

        private void txtScoreLv3R_TextChanged(object sender, EventArgs e)
        {
            txtScoreLv3L.Text = txtScoreLv3R.Text;
        }
        /// <summary>
        /// DataTypeコンボボックス選択時処理(UPDATEページ)
        /// 数値、文字列の二つの条件に合わせて記号コンボボックスのアイテムを設定する
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

        private void SelectedInitforCreate()
        {

            if (Check_Completed())
            {
                ////ファイルの行番号取得           
                string strMsg = "表に値が書き込まれています、更新しますか？";

                //メッセージボックスで行番号を表示
                var resultMessageBox = MessageBox.Show(strMsg
                                , "エラー"
                                , MessageBoxButtons.OKCancel
                                , MessageBoxIcon.Information);

                if (resultMessageBox == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }
            }
            //画面初期化
            AllClear();
            InitTxtBoxColor();

            //登録可能なIDをDBからとってくる。登録時に勝手に付けられるから意味ないけど画面で見れたほうがいいので取得する
            txtCreateEWSID.Text = _db.GetEwsId().ToString();

            //UPDATE時に必要のなかったテキストボックスを表示
            txtCreateEWSID.Enabled = true;
            txtCreateEwsName.Enabled = true;
            txtCreateWarningThresolds.Enabled = true;
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
            txtCreateEwsName.BackColor = SystemColors.Window;
            txtCreateWarningThresolds.BackColor = SystemColors.Window;
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
                    if( ((TextBox)sender) == _txtScore[i] )
                    {
                        if (Save_Score[i] != _txtScore[i].Text)
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

        private void btnCreateRecord_Click(object sender, EventArgs e)
        {
            txtOutSql.Text = "";
            CreatButton_Click();
        }

    }
}
