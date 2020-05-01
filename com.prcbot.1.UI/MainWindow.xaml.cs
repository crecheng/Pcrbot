﻿using System;
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
            str = new string[2];
            str[0] = rootMemberId.Text;
            str[1] = GroupId.Text;
            var rootMemberList = rootMemberId.Text.Split(',');
            var GroupIdList = GroupId.Text.Split(',');
            Dictionary<long, Pcrbot> pcrbot = Event_GroupMsg.GetPrcbot();
            foreach (var p in pcrbot)
            {
                p.Value.rootMember.Clear();
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
        private void Save()
        {
            if (!Directory.Exists(setPath))
            {
                Directory.CreateDirectory(setPath);
            }
            File.WriteAllLines(setPathData, str);
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
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string path = Directory.GetCurrentDirectory()+@"\"+setPath;
            System.Diagnostics.Process.Start("explorer.exe", path);
        }
    }
}