using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Native.Sdk.Cqp.Interface;
using Native.Sdk.Cqp.Model;
using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp;
using System.IO;

namespace com.pcrbot._1.Code
{
    public class Pcrbot
    {
        private int[] boss;
        private int[] bossDate;
        private int targetBoss;
        private string[] str;
        private List<long> tree = new List<long>();
        private long AttackMember;//正在挑战boss的人
        private bool Attack;//是否有人正在攻击
        private int week;
        public List<long> rootMember = new List<long>();
        public List<long> UseGroupId = new List<long>();
        public List<string> log = new List<string>();
        public long GroupId { get; set; }
        public string GroupName { get; set; }
        public string logPath;
        public int logp;
        public Pcrbot()
        {
            boss = new int[5] { 6000000, 8000000, 10000000, 12000000, 20000000 };
            bossDate = new int[5] { 6000000, 8000000, 10000000, 12000000, 20000000 };
            targetBoss = 0;
            AttackMember = 0;
            Attack = false;
            week = 1;
            Loading();
        }
        public Pcrbot(long GroupId,string GroupName)
        {
            boss = new int[5] { 6000000, 8000000, 10000000, 12000000, 20000000 };
            bossDate = new int[5] { 6000000, 8000000, 10000000, 12000000, 20000000 };
            targetBoss = 0;
            AttackMember = 0;
            Attack = false;
            week = 1;
            Loading();
            this.GroupId = GroupId;
            this.GroupName = GroupName;
            logPath = setPath + "/"+GroupName + " " + GroupId+".txt";
        }
        public bool isRun = true;

        public void PrcbotMsg(object sender, CQGroupMessageEventArgs e)
        {
            if (UseGroupId.Count == 0 || UseGroupId.Contains(e.FromGroup.Id))
            {
                if (isRun)
                {
                    ClubFight(e);
                }
                if (rootMember.Contains(e.FromQQ.Id))
                {
                    if (e.Message.Text.Equals("开启bot"))
                    {
                        isRun = true;
                        e.FromGroup.SendGroupMessage("pcrbot开启！");
                    }
                    else if (e.Message.Text.Equals("关闭bot"))
                    {
                        isRun = false;
                        e.FromGroup.SendGroupMessage("pcrbot关闭！");
                    }
                }
            }
        }

        private void ClubFight(CQGroupMessageEventArgs e)
        {
            String msg = e.Message.Text;
            String[] msgA = msg.Split(' ');
            if (msgA[0].Equals("boss录入"))
            {
                if (rootMember.Contains(e.FromQQ.Id))
                {
                    if (msgA.Length > 1)
                    {
                        e.CQLog.Debug("" + msgA.Length);
                        bool tempb = true;
                        targetBoss = 0;
                        AttackMember = 0;
                        Attack = false;
                        week = 1;
                        bossDate = new int[msgA.Length - 1];
                        boss = new int[msgA.Length - 1];
                        for (int i = 1; i < msgA.Length; i++)
                        {
                            int temp;
                            if (int.TryParse(msgA[i], out temp))
                            {
                                boss[i - 1] = temp * 10000;
                                bossDate[i - 1] = temp * 10000;
                            }
                            else
                            {
                                tempb = false;
                                e.FromGroup.SendGroupMessage("正确录入" + i + "个\n请正确输入，后续应为整数，且单位为万\n例如\n——\nboss录入 200\n——\n表示录入1个200w血boss");
                                break;
                            }
                        }
                        if (tempb)
                        {
                            string temp = "成功录入" + boss.Length + "个数据";
                            e.FromGroup.SendGroupMessage(temp);
                            SaveLog(MemberName(e) + "  " + temp);
                        }
                    }
                    else
                    {
                        e.FromGroup.SendGroupMessage("请正确输入，后续应为整数，且单位为万\n例如\n——\nboss录入 200\n——\n表示录入1个200w血boss");
                    }
                }
                else
                {
                    e.FromGroup.SendGroupMessage("你不是管理者账户，无法进行此操作！");
                }
            }
            else if (msgA[0].Equals("申请出刀"))
            {
                if (bossDate != null)
                {
                    if (Attack)
                    {
                        e.FromGroup.SendGroupMessage(MemberName(e,AttackMember) + "正在攻击boss，请稍等！");
                    }
                    else
                    {
                        if (tree.Contains(e.FromQQ.Id))
                        {
                            e.FromGroup.SendGroupMessage(e.FromQQ.GetGroupMemberInfo(e.FromGroup.Id).Card + " 已挂树，" + "无法出刀");
                        }
                        else
                        {
                            Attack = true;
                            AttackMember = e.FromQQ.Id;
                            string temp = MemberName(e) + "  已开始挑战boss";
                            e.FromGroup.SendGroupMessage(temp+"\n现在" + week +"周目，" + (targetBoss + 1) + "号boss生命值" + bossDate[targetBoss]);
                            SaveLog(temp);
                        }
                    }
                }
                else
                {
                    e.FromGroup.SendGroupMessage("暂无boss数据，请录入！");
                }
            }
            else if (msgA[0].Equals("完成"))
            {
                if (e.FromQQ == AttackMember)
                {
                    int damage;
                    if (msgA.Length < 2)
                    {
                        e.FromGroup.SendGroupMessage("请在“完成”后空格后输入你的伤害或输入“击杀”");

                    }
                    else
                    {
                        if (msgA[1].Equals("击杀"))
                        {
                            Attack = false;
                            AttackMember = 0;
                            bossDate[targetBoss] = 0;
                            targetBoss++;
                            if (targetBoss == boss.Length)
                            {
                                week++;
                                for (int i = 0; i < boss.Length - 1; i++)
                                {
                                    bossDate[i] = boss[i];
                                }
                            }
                            targetBoss %= boss.Length;
                            string temp = MemberName(e) + " 已完成挑战boss";
                            e.FromGroup.SendGroupMessage(temp+"\n现在" + week +
                                    "周目，" + (targetBoss + 1) + "号boss生命值" + bossDate[targetBoss]);
                            SaveLog(temp +" 击杀 ");
                            tree.Clear();
                        }
                        else if (int.TryParse(msgA[1], out damage))
                        {
                            if (damage > bossDate[targetBoss]||damage<0)
                            {
                                e.FromGroup.SendGroupMessage("请正确输入伤害，如果已击杀请输入“击杀”");
                            }
                            else
                            {
                                bossDate[targetBoss] -= damage;
                                Check();
                                Attack = false;
                                AttackMember = 0;
                                string temp = MemberName(e) + " 已完成挑战boss";
                                e.FromGroup.SendGroupMessage(temp+"\n现在" + week +
                                    "周目，" + (targetBoss + 1) + "号boss生命值" + bossDate[targetBoss]);
                                SaveLog(temp+" "+damage);
                            }
                        }
                    }
                }
                else
                {
                    e.FromGroup.SendGroupMessage("无人正在挑战boss");
                }

            }
            else if (msgA[0].Equals("挂树"))
            {
                if (e.FromQQ == AttackMember)
                {
                    var member = e.FromQQ.Id;
                    tree.Add(member);
                    string temp = MemberName(e) + " 已挂树";
                    e.FromGroup.SendGroupMessage(temp);
                    Attack = false;
                    AttackMember = 0;
                    SaveLog(temp);
                }
            }
            else if (msgA[0].Equals("查树"))
            {
                String sendtext = "挂树成员：\n";
                for (int i = 0; i < tree.Count; i++)
                {
                    if (tree[i] != 0)
                    {
                        sendtext += MemberName(e,tree[i]) + "\n";
                    }
                }
                e.FromGroup.SendGroupMessage(sendtext);
            }
            else if (msgA[0].Equals("查看"))
            {
                if (bossDate != null)
                {
                    e.FromGroup.SendGroupMessage("当前" + week + "周目，" + (targetBoss + 1) + "号boss生命值" + bossDate[targetBoss]);
                }
                else
                {
                    e.FromGroup.SendGroupMessage("暂无boss数据，请录入！");
                }
            }
            else if (msgA[0].Equals("强行下树"))
            {
                if (tree.Contains(e.FromQQ.Id))
                {
                    if (msgA.Length >= 2)
                    {
                        int damage;
                        if (msgA.Length < 2)
                        {
                            e.FromGroup.SendGroupMessage("请在“强行下树”后空格后输入你的伤害或击杀");

                        }
                        else
                        {
                            if (msgA[1].Equals("击杀"))
                            {
                                Attack = false;
                                AttackMember = 0;
                                bossDate[targetBoss] = 0;
                                targetBoss++;
                                if (targetBoss == boss.Length)
                                {
                                    week++;
                                    for (int i = 0; i < boss.Length - 1; i++)
                                    {
                                        bossDate[i] = boss[i];
                                    }
                                }
                                targetBoss %= boss.Length;
                                string temp = MemberName(e) + "已强行下树";
                                e.FromGroup.SendGroupMessage(temp+"\n现在" + week +
                                        "周目，" + (targetBoss + 1) + "号boss生命值" + bossDate[targetBoss]);
                                tree.Remove(e.FromQQ.Id);
                                tree.Clear();
                                SaveLog(temp+ " 击杀");
                            }
                            else if (int.TryParse(msgA[1], out damage))
                            {
                                if (damage > bossDate[targetBoss]||damage<0)
                                {
                                    e.FromGroup.SendGroupMessage("请正确输入伤害，如果已击杀请输入“击杀”");
                                }
                                else
                                {
                                    bossDate[targetBoss] -= damage;
                                    Check();
                                    string temp = MemberName(e) + "已强行下树";
                                    e.FromGroup.SendGroupMessage(temp+"\n现在" + week +
                                        "周目，" + (targetBoss + 1) + "号boss生命值" + bossDate[targetBoss]);
                                    tree.Remove(e.FromQQ.Id);
                                    SaveLog(temp+ " "+damage);
                                }
                            }
                        }
                    }
                }
            }
            else if (msgA[0].Equals("修正"))
            {
                if (rootMember.Contains(e.FromQQ.Id))
                {
                    if (bossDate != null)
                    {
                        int temptar = 0;
                        int tempweek = 0;
                        int tempboss = 0;
                        if (msgA.Length >= 4)
                        {
                            if (int.TryParse(msgA[1], out tempweek) && int.TryParse(msgA[2], out temptar) && int.TryParse(msgA[3], out tempboss))
                            {
                                week = tempweek;
                                for (int i = 0; i < boss.Length - 1; i++)
                                {
                                    bossDate[i] = boss[i];
                                }
                                if (temptar > boss.Length + 1||temptar<0)
                                {
                                    e.FromGroup.SendGroupMessage("请正确输入当前第几个boss！");
                                }
                                else
                                {
                                    targetBoss = temptar - 1;
                                }
                                if (tempboss > boss[targetBoss]||tempboss<=0)
                                {
                                    e.FromGroup.SendGroupMessage("请正确输入当前boss血量!");
                                }
                                else
                                {
                                    bossDate[targetBoss] = tempboss;
                                }
                                
                                e.FromGroup.SendGroupMessage("当前" + week + "周目，" + (targetBoss + 1) + "号boss生命值" + bossDate[targetBoss]);
                                SaveLog(MemberName(e)+" 修正了boss数据");

                            }
                            else
                            {
                                e.FromGroup.SendGroupMessage("请正确输入！\n修正 [周目] [当前第几个boss] [当前boss血量]");
                            }
                        }
                        else
                        {
                            e.FromGroup.SendGroupMessage("请正确输入！\n修正 [周目] [当前第几个boss] [当前boss血量]");
                        }
                    }
                    else
                    {
                        e.FromGroup.SendGroupMessage("暂无boss数据，请录入！");
                    }
                }
                else
                {
                    e.FromGroup.SendGroupMessage("你不是管理者账户，无法进行此操作！");
                }
            }
            else if (msgA[0].Equals("指令详情"))
            {
                e.FromGroup.SendGroupMessage("boss录入\n", "申请出刀\n", "完成\n", "挂树\n", "强行下树\n", "查树\n", "查看\n", "修正\n");
                
            }
        }
        private void Check()
        {
            if (bossDate[targetBoss] == 0)
            {
                targetBoss++;
                if (targetBoss == boss.Length)
                {
                    week++;
                    for (int i = 0; i < boss.Length - 1; i++)
                    {
                        bossDate[i] = boss[i];
                    }
                }
                targetBoss %= boss.Length;
            }
        }
        readonly string setPath = @"data/app/com.prcbot";
        readonly string setPathData = @"data/app/com.prcbot/set.data";
        private void Save()
        {
            StreamWriter sw=null;
            if (!Directory.Exists(setPath))
            {
                Directory.CreateDirectory(setPath);
            }
            if (!File.Exists(logPath))
            {
                sw=File.CreateText(logPath);
                
            }
            if(sw==null)
                sw = new StreamWriter(logPath, true, Encoding.UTF8);
            for(;logp<log.Count;logp++)
            {
                sw.WriteLine(log[logp]);
            }
            sw.Close();
        }
        private void SaveLog(string s)
        {
            string temp =DateTime.Now+" , "+ s+"\n"+ week +" , " + (targetBoss + 1) + " , " + bossDate[targetBoss];
            log.Add(temp);
            Save();
            if (log.Count >= 30)
            {
                log.Clear();
                logp = 0;
            }
        }
        private void Loading()
        {
            if (File.Exists(setPathData))
            {
                str = File.ReadAllLines(setPathData);
                if (str != null)
                {
                    if (str.Length >= 1)
                    {
                        var rootMemberList = str[0].Split(',');
                        rootMember.Clear();
                        foreach (var id in rootMemberList)
                        {
                            if (long.TryParse(id, out long temp))
                            {
                                rootMember.Add(temp);
                            }
                        }
                    }
                    if (str.Length >= 2)
                    {
                        var GroupIdList = str[1].Split(',');
                        UseGroupId.Clear();
                        foreach (var id in GroupIdList)
                        {
                            if (long.TryParse(id, out long temp))
                            {
                                UseGroupId.Add(temp);
                            }
                        }
                    }
                }
            }
        }
        private string MemberName(CQGroupMessageEventArgs e)
        {
            var m = e.FromQQ.GetGroupMemberInfo(e.FromGroup.Id);
            return m.Card.Length <= 0 ? m.Nick: m.Card;
        }
        private string MemberName(CQGroupMessageEventArgs e,long qq)
        {
            var m = e.CQApi.GetGroupMemberInfo(e.FromGroup.Id, qq);
            return m.Card.Length <= 0 ? m.Nick : m.Card;
        }
    }
}
