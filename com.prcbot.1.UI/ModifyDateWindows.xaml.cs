using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using com.pcrbot._1.Code;
using com.pcrbot._1.UI;

namespace com.pcrbot._1.UI
{
    /// <summary>
    /// ModifyDateWindows.xaml 的交互逻辑
    /// </summary>
    public partial class ModifyDateWindows : Window
    {
        private Dictionary<long, Pcrbot> pcrbot;
        private long GroupId;
        private long QQId;
        private int Day;
        private DayDmg dayDmg;
        public ModifyDateWindows()
        {
            InitializeComponent();
            MyRefreah();
        }

        private void MyRefreah()
        {
            GroupIdList.Items.Clear();
            QQIdList.Items.Clear();
            DayList.Items.Clear();
            DmgList.Items.Clear();
            pcrbot = Event_GroupMsg.GetPrcbot();
            if (pcrbot != null)
            {
                foreach (var p in pcrbot)
                {
                    GroupIdList.Items.Add("" + p.Key);
                }
            }
        }

        private void DmgIs_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(NewDmg.Text, out int dmg))
            {
                if (GroupId != 0)
                {
                    if (pcrbot != null && pcrbot.ContainsKey(GroupId))
                    {
                        if (QQId != 0 && pcrbot[GroupId].GetMemberDateDic().ContainsKey(QQId))
                        {
                            if(Day!=0 && pcrbot[GroupId].GetMemberDateDic()[QQId].memberDateDay.ContainsKey(Day))
                            {
                                if (dayDmg != null && pcrbot[GroupId].GetMemberDateDic()[QQId].memberDateDay[Day].damage.Count > dayDmg.i)
                                {

                                    DayDmg tempDayDmg = new DayDmg(dayDmg.i,dayDmg.dmg);
                                    DmgList.Items.Remove(dayDmg);
                                    dayDmg = tempDayDmg;
                                    pcrbot[GroupId].GetMemberDateDic()[QQId].memberDateDay[Day].damage[dayDmg.i] = dmg;
                                    dayDmg.dmg = dmg;
                                    DmgList.Items.Add(dayDmg);
                                }
                            }
                        }
                    }
                }
            }
        }
    
        private void GroupIdList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GroupId = 0;
            QQIdList.Items.Clear();
            QQId = 0;
            DayList.Items.Clear();
            Day = 0;
            DmgList.Items.Clear();
            var tempSelectGroup = (GroupIdList.SelectedItem != null) ? GroupIdList.SelectedItem.ToString() : "";
            if (long.TryParse(tempSelectGroup, out long s))
            {
                if (pcrbot != null && pcrbot.ContainsKey(s))
                {
                    GroupId = s;
                    foreach (var qq in pcrbot[s].GetMemberDateDic())
                    {
                        QQIdList.Items.Add("" + qq.Key);
                    }
                }
            }
        }

        private void QQIdList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            QQId = 0;
            DayList.Items.Clear();
            Day = 0;
            DmgList.Items.Clear();
            var tempSelect = (QQIdList.SelectedItem != null) ? QQIdList.SelectedItem.ToString() : "";
            if (GroupId != 0)
            {
                if (pcrbot != null && pcrbot.ContainsKey(GroupId))
                {
                    if (long.TryParse(tempSelect, out long qq))
                    {
                        var member = pcrbot[GroupId].GetMemberDateDic();
                        if (member.ContainsKey(qq))
                        {
                            QQId = qq;
                            foreach (var day in member[qq].memberDateDay)
                            {
                                DayList.Items.Add("" + day.Key);
                            }
                        }

                    }
                }
            }
        }

        private void DayList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Day = 0;
            DmgList.Items.Clear();
            var tempSelect = (DayList.SelectedItem != null) ? DayList.SelectedItem.ToString() : "";
            if (GroupId != 0)
            {
                if (pcrbot != null && pcrbot.ContainsKey(GroupId))
                {
                    if (QQId != 0 && pcrbot[GroupId].GetMemberDateDic().ContainsKey(QQId))
                    {
                        if (int.TryParse(tempSelect, out int d))
                        {
                            var day = pcrbot[GroupId].GetMemberDateDic()[QQId].memberDateDay;
                            if (day.ContainsKey(d))
                            {
                                Day = d;
                                for (int i = 0; i < day[d].damage.Count; i++)
                                {
                                    DayDmg dayDmg = new DayDmg(i, day[d].damage[i]);
                                    DmgList.Items.Add(dayDmg);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DmgList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dayDmg = (DayDmg)DmgList.SelectedItem;
            if (dayDmg != null)
                NewDmg.Text = ""+dayDmg.dmg;
        }

        private void QQIdDel_Click(object sender, RoutedEventArgs e)
        {
            var s = QQIdList.SelectedItem;
            var tempSelect = (s != null) ? s.ToString() : "";
            if (s != null && GroupId != 0)
            {
                if (pcrbot != null && pcrbot.ContainsKey(GroupId))
                {
                    if (long.TryParse(tempSelect, out long qq))
                    {
                        if (pcrbot[GroupId].GetMemberDateDic().ContainsKey(qq))
                        {
                            pcrbot[GroupId].GetMemberDateDic().Remove(qq);
                            QQIdList.Items.Remove(s);
                            QQId = 0;
                            DayList.Items.Clear();
                            Day = 0;
                            DmgList.Items.Clear();
                        }
                    }
                }
            }
        }

        private void DayDel_Click(object sender, RoutedEventArgs e)
        {
            var s = DayList.SelectedItem;
            var tempSelect = (s != null) ? s.ToString() : "";
            if (s!=null&&GroupId != 0)
            {
                if (pcrbot != null && pcrbot.ContainsKey(GroupId))
                {
                    if (QQId != 0 && pcrbot[GroupId].GetMemberDateDic().ContainsKey(QQId))
                    {
                        if (int.TryParse(tempSelect, out int d))
                        {
                            if (pcrbot[GroupId].GetMemberDateDic()[QQId].memberDateDay.ContainsKey(d))
                            {
                                pcrbot[GroupId].GetMemberDateDic()[QQId].memberDateDay.Remove(d);
                                DayList.Items.Remove(s);
                                Day = 0;
                                DmgList.Items.Clear();
                            }
                        }
                    }
                }
            }
        }

        private void DmgDel_Click(object sender, RoutedEventArgs e)
        {
            var s = DmgList.SelectedItem;
            if (s != null && GroupId != 0)
            {
                if (pcrbot != null && pcrbot.ContainsKey(GroupId))
                {
                    if (QQId != 0 && pcrbot[GroupId].GetMemberDateDic().ContainsKey(QQId))
                    {
                        if (Day != 0 && pcrbot[GroupId].GetMemberDateDic()[QQId].memberDateDay.ContainsKey(Day))
                        {
                            if (dayDmg != null && pcrbot[GroupId].GetMemberDateDic()[QQId].memberDateDay[Day].damage.Count > dayDmg.i)
                            {
                                var temp = pcrbot[GroupId].GetMemberDateDic()[QQId].memberDateDay[Day];
                                temp.damage.RemoveAt(dayDmg.i);
                                temp.attackTimes = temp.damage.Count();
                                NewDmg.Text = "";
                                DmgList.Items.Remove(dayDmg);
                                dayDmg = null;
                            }
                        }
                    }
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Common.ModifyWindowsOpen = false;
        }
    }
    class DayDmg
    {
        public int i;
        public int dmg;
        public DayDmg(int i,int damage)
        {
            this.i = i;
            dmg = damage;
        }
        public override string ToString()
        {
            return ""+(i+1)+" ,"+dmg;
        }
    }
}
