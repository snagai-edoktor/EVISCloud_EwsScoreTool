using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace app2
{
    public partial class Form2 : Form
    {
        public class ErrorRecord
        {
            public int ErrorId;
            public int i;
            public int j;

            public ErrorRecord(int errorid, int ind_i, int ind_j)
            {
                ErrorId = errorid;
                i = ind_i;
                j = ind_j;
            }

        }

        public string[] ErrorMessage = new string[]
        {
                "正常",//0
                "スコアが0に向かって降順に設定されていません。",//1
                "スコアが未設定です",//2
                "ディスプレイオーダーが設定されていません。",//3
                "データタイプが設定されていません。",//4
                "バイタルコードが設定されていません。",//5
                "入力記号が設定されていません。",//6
                "入力値に対するスコアがありません。",//7 jが必要
                "データタイプが数値型の場合文字列の入力は出来ません。",//8 txtA用
                "数値が連続していません。",//9 j=99
                "レコードが作成されていません。",//10  i=99,j=99
                "データタイプが数値型の場合文字列の入力は出来ません。",//11 txtB用
                "スコアの入力値は数値である必要があります。",//12
                "DisPlayOrderが重複しています。",//13
                "DisplayOrderは10より大きい値を入力出来ません。",//14
                "DisplayOrderは数値以外の入力は出来ません。",//15
                "EwsNameが未入力です。",//16
                "WarningThresoldsが未入力です。",//17
                "",//
                "",//
                "",//
                "",//
                "",//
                "",//50 + x スコアは
                "",//100 A,Bどっちも文字列
        };

        private void Error_ColorChange(ErrorRecord err)
        {
            switch (err.ErrorId)
            {
                case 1://"スコアが0に向かって降順に設定されていません。"
                case 2://"スコアが未設定です"
                    for (int i = 0; i < 7; i++)
                    {
                        if (i != 3)
                        {
                            _txtScore[i].BackColor = Color.Red;
                        }
                    }
                    break;
                case 3://"ディスプレイオーダーが設定されていません。"
                    _txtDisplayOrder[err.i].BackColor = Color.Red;
                    break;
                case 4://"データタイプが設定されていません。"
                    _cmbDataTypeUP[err.i].BackColor = Color.Red;
                    break;
                case 5://"バイタルコードが設定されていません。"
                    _vitalcode[err.i].BackColor = Color.Red;
                    break;
                case 6://"入力記号が設定されていません。"
                    _cmb[err.i, err.j].BackColor = Color.Red;
                    break;
                case 7://"入力値に対するスコアがありません。"
                    _txtScore[err.j].BackColor = Color.Red;
                    break;
                case 8://"データタイプが数値型の場合文字列の入力は出来ません。"
                    _txtCriteiaValueA[err.i, err.j].BackColor = Color.Red;
                    break;
                case 9://"数値が連続していません。" shundbg -> 値が入ってるところだけにしてもいいかも
                    for (int j = 0; j < 7; j++)
                    {
                        _txtCriteiaValueA[err.i, j].BackColor = Color.Red;
                        _txtCriteiaValueB[err.i, j].BackColor = Color.Red;
                    }
                    break;
                case 10:
                    ////ファイルの行番号取得           
                    string strMsg = "レコードが作成されていません。";
                    //メッセージボックスで行番号を表示
                    MessageBox.Show(strMsg
                                    , "エラー"
                                    , MessageBoxButtons.OK
                                    , MessageBoxIcon.Information);
                    break;
                case 11://"データタイプが数値型の場合文字列の入力は出来ません。"
                    _txtCriteiaValueB[err.i, err.j].BackColor = Color.Red;
                    break;
                case 12://"スコアの入力値は数値である必要があります。"
                    _txtScore[3 + (err.i + 1)].BackColor = Color.Red;
                    _txtScore[3 - (err.i + 1)].BackColor = Color.Red;
                    break;
                case 13://"DisPlayOrderが重複しています。"
                    _txtDisplayOrder[err.i].BackColor = Color.Red;
                    break;
                case 14:
                    _txtDisplayOrder[err.i].BackColor = Color.Red;
                    break;
                case 15:
                    _txtDisplayOrder[err.i].BackColor = Color.Red;
                    break;
                case 16:
                    txtCreateEwsName.BackColor = Color.Red;
                    break;
                case 17:
                    txtCreateWarningThresolds.BackColor = Color.Red;
                    break;
            }
        }

    }
}
