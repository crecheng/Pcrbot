using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using com.pcrbot._1.Code;
using Native.Sdk.Cqp;

namespace com.pcrbot._1.UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (File.Exists(setPathData))
                Loading();
        }
        string[] str;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            str = new string[5];
            str[0] = rootMemberId.Text;
            str[1] = GroupId.Text;
            str[2] = pcrbotKey.Text;
            str[3] = maxMember.Text;
            str[4] = (SaveDatPre.IsChecked==true) ? "1" : "0";
            var rootMemberList = rootMemberId.Text.Split(',');
            var GroupIdList = GroupId.Text.Split(',');
            Dictionary<long, Pcrbot> pcrbot = null;
            try
            {
                 pcrbot= Event_GroupMsg.GetPrcbot();

            }catch(Exception ex)
            {
                if (Common.CQLog != null)
                {
                    Common.CQLog.Warning("获取pcrbot失败" + ex.Message);
                }
            }
            foreach (var p in pcrbot)
            {
                p.Value.key = str[2];
                p.Value.rootMember.Clear();
                int.TryParse(str[3], out int max);
                if (max >= 1)
                    p.Value.MaxAttackCount = max;
                else
                    maxMember.Text = "1";
                p.Value.SaveDatPer = str[4].Equals("1");
                foreach (var id in rootMemberList)
                {
                    if (long.TryParse(id, out long temp))
                    {
                        p.Value.rootMember.Add(temp);
                    }
                }

                p.Value.UseGroupId.Clear();
                foreach (var id in GroupIdList)
                {
                    if (long.TryParse(id, out long temp))
                    {
                        p.Value.UseGroupId.Add(temp);
                    }
                }
            }
            Save();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        readonly string setPath = @"data\app\com.prcbot";
        readonly string setPathData = @"data\app\com.prcbot\set.data";
        readonly string strPathData = @"data\app\com.prcbot\str.data";
        private void Save()
        {
            if (!Directory.Exists(setPath))
            {
                Directory.CreateDirectory(setPath);
            }
            File.WriteAllLines(setPathData, str);
            File.WriteAllLines(strPathData, GetMyStr());
        }

        private void Loading()
        {
            if(File.Exists(setPathData))
                str = File.ReadAllLines(setPathData);
            if (str != null)
            {
                if (str.Length >= 1)
                {
                    rootMemberId.Text = str[0];
                }
                if (str.Length >= 2)
                {
                    GroupId.Text = str[1];
                }
                if (str.Length >= 3)
                {
                    pcrbotKey.Text = str[2];
                }
                if (str.Length >= 4)
                {
                    maxMember.Text = str[3];
                }
                if (str.Length >= 5)
                {
                    SaveDatPre.IsChecked = str[4].Equals("1");
                }
            }
            if (File.Exists(strPathData))
                SetMyStr(File.ReadAllLines(strPathData));
            else
                SetMyStr(GetMyStr());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string path = Directory.GetCurrentDirectory()+@"\"+setPath;
            System.Diagnostics.Process.Start("explorer.exe", path);
        }

        private void StrList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(strList.SelectedItem!=null)
                modifyText.Text = ((ModfiyMyS)strList.SelectedItem).value;
        }

        private string[] GetMyStr()
        {
            if (MyString.str.Count <= 0)
                MyString.ResetStr();
            string[] s = new string[MyString.str.Count*2];
            int i = 0;
            foreach(var mystr in MyString.str)
            {
                s[i++] = mystr.Key;
                s[i++] = mystr.Value;
            }
            return s;
        }
        private void SetMyStr(string[] s)
        {
            if (s != null)
            {
                MyString.ResetStr();
                for (int i = 1; i < s.Length; i += 2)
                {
                    if (MyString.str.ContainsKey(s[i - 1]))
                        MyString.str[s[i - 1]] = s[i];
                    else
                        MyString.str.Add(s[i - 1], s[i]);
                }
                foreach(var ms in MyString.str)
                {
                    ModfiyMyS myS = new ModfiyMyS(ms.Key, ms.Value);
                    strList.Items.Add(myS);
                }
            }
        }

        private void IsModify_Click(object sender, RoutedEventArgs e)
        {
            string key = ((ModfiyMyS)(strList.SelectedItem)).key;
            if (key != null)
            {
                if (MyString.str.ContainsKey(key))
                {
                    MyString.str[key] = modifyText.Text;
                    ((ModfiyMyS)strList.SelectedItem).value = modifyText.Text;
                    strList.Items.Clear();
                    SetMyStr(GetMyStr());
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MyString.ResetStr();
            strList.Items.Clear();
            modifyText.Text = String.Empty;
            SetMyStr(GetMyStr());
        }
    }
    public class ModfiyMyS
    {
        public string key;
        public string value;
        public ModfiyMyS(string k,string v)
        {
            key = k;
            value = v;
        }
        public override string ToString()
        {
            return value;
        }
    }
}
