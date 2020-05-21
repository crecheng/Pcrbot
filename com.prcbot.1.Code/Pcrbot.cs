using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Native.Sdk.Cqp.Interface;
using Native.Sdk.Cqp.Model;
using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp.Enum;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace com.pcrbot._1.Code
{
    public class Pcrbot
    {
        private int[] boss;
        private int[] bossDate;
        private int targetBoss;
        private string[] str;
        private Dictionary<long,int> tree = new Dictionary<long, int>();
        private Dictionary<long,int> AttackMembers=new Dictionary<long, int>(1);//正在挑战boss的人
        private long AttackMember = 0;
        private bool Attack;//是否有人正在攻击
        private int week;
        public List<long> rootMember = new List<long>();
        public List<long> UseGroupId = new List<long>();
        public List<string> log = new List<string>();
        public long GroupId { get; set; }
        public string GroupName { get; set; }
        public string logPath;
        public string DatePath;
        public string DatPath;
        public int Logp;
        public string key=string.Empty;
        private bool isReadKey = false;
        private Dictionary<long, MemberDate> memberDate = new Dictionary<long, MemberDate>();
        public Dictionary<long, MemberDate> GetMemberDateDic()
        {
            return memberDate;
        }
        private bool delDate = false;
        public int MaxAttackCount = 1;
        public bool SaveDatPer = false;
        private CQGroupMessageEventArgs e;

        public Pcrbot()
        {
            boss = new int[5] { 6000000, 8000000, 10000000, 12000000, 20000000 };
            bossDate = new int[5] { 6000000, 8000000, 10000000, 12000000, 20000000 };
            targetBoss = 0;
            Attack = false;
            week = 1;
            Loading();
        }
        public Pcrbot(long GroupId,string GroupName)
        {
            boss = new int[5] { 6000000, 8000000, 10000000, 12000000, 20000000 };
            bossDate = new int[5] { 6000000, 8000000, 10000000, 12000000, 20000000 };
            targetBoss = 0;
            Attack = false;
            week = 1;
            MaxAttackCount = 1;
            AttackMember = 0;
            Loading();
            this.GroupId = GroupId;
            this.GroupName = GroupName;
            GroupName = GroupName.Replace("/",string.Empty).Replace("\\", string.Empty).Replace("?", string.Empty).Replace("*", string.Empty).Replace("<", string.Empty).Replace(">", string.Empty).Replace(":", string.Empty).Replace("\"", string.Empty).Replace("|", string.Empty);
            logPath = setPath + "/"+GroupName + " " + GroupId+".txt";
            DatePath= setPath + "/" + GroupName + " " + GroupId + " 成员数据.txt";
            DatPath= setPath + "/" + GroupName + " " + GroupId + " 成员数据.dat";
            LoadDate();
        }


        public bool isRun = true;

        /// <summary>
        /// 提供用于描述酷Q群聊消息事件处理的函数
        /// <para/>
        /// Type: 2
        /// </summary>
        public void PrcbotMsg(object sender, CQGroupMessageEventArgs e)
        {
            /* if (e.Message.Text.IndexOf("CQ:hb")>=0)
             {
                 var temp = e.Message.Text;
             }*/
            this.e = e;
            if (!MsgHaveKey(e.Message.Text))
                return;
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
            //hb(e);
        }

        private bool MsgHaveKey(String s)
        {
            if (isReadKey)
            {
                ReadKey();
                isReadKey = true;
            }
            if (key != null)
            {
                if (key.Length > 0)
                    return s.IndexOf(key) == 0 ? true : false;
                else
                    return true;
            }
            else
                return true;
        }

        private void ClubFight(CQGroupMessageEventArgs e)
        {

            String msg = e.Message.Text;
            msg = msg.Substring(key.Length);
            String[] msgA = msg.Split(' ');
            if (Common.ModifyWindowsOpen && NeedChangeDate(msgA))
            {
                e.FromGroup.SendGroupMessage("管理者正在操作数据，请稍等！");
                return;
            }
            if (delDate)
            {
                if (isRoot(e.FromQQ.Id))
                {
                    delDate = false;
                    if (e.Message.Text.Equals("是"))
                    {
                        memberDate.Clear();
                        e.FromGroup.SendGroupMessage("成员出刀数据删除！");
                    }
                    else
                    {
                        e.FromGroup.SendGroupMessage("取消操作");
                    }
                }
            }
            if (msgA[0].Equals("boss录入"))
            {
                if (isRoot(e.FromQQ.Id))
                {
                    if (msgA.Length > 1)
                    {
                        e.CQLog.Debug("" + msgA.Length);
                        bool tempb = true;
                        targetBoss = 0;
                        AttackMembers = new Dictionary<long, int>();
                        Attack = false;
                        week = 1;
                        AttackMember = 0;
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
                    if (MaxAttackCount == 1)
                    {
                        if (Attack)
                        {
                            e.FromGroup.SendGroupMessage(MemberName(e, AttackMember) + "正在攻击boss，请稍等！");
                        }
                        else
                        {
                            if (tree.ContainsKey(e.FromQQ.Id))
                            {
                                e.FromGroup.SendGroupMessage(MemberName(e) + " 已挂树，" + "无法出刀");
                            }
                            else
                            {
                                Attack = true;
                                AttackMember = e.FromQQ.Id;
                                string temp = MemberName(e) + "  已开始挑战boss";
                                e.FromGroup.SendGroupMessage(temp + "\n现在" + week + "周目，" + (targetBoss + 1) + "号boss生命值" + bossDate[targetBoss]);
                                SaveLog(temp);
                            }
                        }
                    }
                    else
                    {
                        if (MaxAttackCount > AttackMembers.Count)
                        {
                            if (!AttackMembers.ContainsKey(e.FromQQ.Id))
                            {
                                if (tree.ContainsKey(e.FromQQ.Id))
                                {
                                    e.FromGroup.SendGroupMessage(MemberName(e) + " 已挂树，" + "无法出刀");
                                }
                                else
                                {
                                    AttackMembers.Add(e.FromQQ.Id, week * 10 + targetBoss);
                                    string temp = MemberName(e) + "  已开始挑战boss\n";
                                    e.FromGroup.SendGroupMessage(temp + AllAttackMember(e) + "正在挑战boss\n现在" + week + "周目，" + (targetBoss + 1) + "号boss生命值" + bossDate[targetBoss]);
                                    SaveLog(temp);
                                }
                            }
                        }
                        else
                        {
                            e.FromGroup.SendGroupMessage(AllAttackMember(e) + "正在挑战boss\n同时挑战人数达到设置上限！");
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
                if (MaxAttackCount == 1)
                {
                    if (e.FromQQ.Id == AttackMember)
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
                                int dmg = bossDate[targetBoss];
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
                                e.FromGroup.SendGroupMessage(temp + "\n现在" + week +
                                        "周目，" + (targetBoss + 1) + "号boss生命值" + bossDate[targetBoss]);
                                SaveLog(temp + " 击杀 ");
                                
                                Record(e, e.FromQQ.Id, dmg);
                                SaveDate(e, SaveDatPer);
                                tree.Clear();
                            }
                            else if (int.TryParse(msgA[1], out damage))
                            {
                                if (damage > bossDate[targetBoss] || damage < 0)
                                {
                                    e.FromGroup.SendGroupMessage("请正确输入伤害，如果已击杀请输入“击杀”");
                                }
                                else
                                {
                                    bossDate[targetBoss] -= damage;
                                    CheackBossBlood();
                                    Attack = false;
                                    AttackMember = 0;
                                    CheackBossBlood();
                                    string temp = MemberName(e) + " 已完成挑战boss";
                                    e.FromGroup.SendGroupMessage(temp + "\n现在" + week +
                                        "周目，" + (targetBoss + 1) + "号boss生命值" + bossDate[targetBoss]);
                                    SaveLog(temp + " " + damage);
                                    
                                    Record(e, e.FromQQ.Id, damage);
                                    SaveDate(e, SaveDatPer);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (AttackMembers.ContainsKey(e.FromQQ.Id))
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
                                if (AttackMembers[e.FromQQ.Id] == week * 10 + targetBoss)
                                {
                                    AttackMembers.Remove(e.FromQQ.Id);
                                    int dmg = bossDate[targetBoss];
                                    bossDate[targetBoss] = 0;
                                    targetBoss++;
                                    if (targetBoss == boss.Length)
                                    {
                                        week++;
                                        for (int i = 0; i < boss.Length; i++)
                                        {
                                            bossDate[i] = boss[i];
                                        }
                                    }
                                    targetBoss %= boss.Length;
                                    string temp = MemberName(e) + " 已完成挑战boss";
                                    e.FromGroup.SendGroupMessage(temp + "\n现在" + week +
                                            "周目，" + (targetBoss + 1) + "号boss生命值" + bossDate[targetBoss]);
                                    SaveLog(temp + " 击杀 ");
                                    
                                    Record(e, e.FromQQ.Id, dmg);
                                    SaveDate(e, SaveDatPer);
                                    tree.Clear();
                                }
                                else
                                {
                                    AttackMembers.Remove(e.FromQQ.Id);
                                    e.FromGroup.SendGroupMessage("你已跨boss结算，当前数据不记录!");
                                }
                            }
                            else if (int.TryParse(msgA[1], out damage))
                            {
                                if (damage > bossDate[targetBoss] || damage < 0)
                                {
                                    e.FromGroup.SendGroupMessage("请正确输入伤害，如果已击杀请输入“击杀”");
                                }
                                else
                                {
                                    if (AttackMembers[e.FromQQ.Id] == week * 10 + targetBoss)
                                    {


                                        bossDate[targetBoss] -= damage;
                                        CheackBossBlood();
                                        Attack = false;
                                        AttackMembers.Remove(e.FromQQ.Id);
                                        string temp = MemberName(e) + " 已完成挑战boss";
                                        e.FromGroup.SendGroupMessage(temp + "\n现在" + week +
                                            "周目，" + (targetBoss + 1) + "号boss生命值" + bossDate[targetBoss]);
                                        SaveLog(temp + " " + damage);
                                        
                                        Record(e, e.FromQQ.Id, damage);
                                        SaveDate(e, SaveDatPer);
                                    }
                                    else
                                    {
                                        AttackMembers.Remove(e.FromQQ.Id);
                                        e.FromGroup.SendGroupMessage("你已跨boss结算，当前数据不记录!");
                                    }
                                }

                            }
                        }

                    }
                }
            }
            else if (msgA[0].Equals("挂树"))
            {
                if (MaxAttackCount == 1)
                {
                    if (e.FromQQ == AttackMember)
                    {
                        var member = e.FromQQ.Id;
                        tree.Add(member, week * 10 + targetBoss);
                        string temp = MemberName(e) + " 已挂树";
                        e.FromGroup.SendGroupMessage(temp);
                        Attack = false;
                        AttackMember = 0;
                        SaveLog(temp);
                    }
                }
                else
                {
                    if (AttackMembers.ContainsKey(e.FromQQ.Id))
                    {
                        if (AttackMembers[e.FromQQ.Id] == week * 10 + targetBoss)
                        {
                            var member = e.FromQQ.Id;
                            tree.Add(member, week * 10 + targetBoss);
                            string temp = MemberName(e) + " 已挂树";
                            e.FromGroup.SendGroupMessage(temp);
                            AttackMembers.Remove(e.FromQQ.Id);
                            SaveLog(temp);
                        }
                        else
                        {
                            AttackMembers.Remove(e.FromQQ.Id);
                            e.FromGroup.SendGroupMessage("你已跨boss结算，当前数据不记录!");
                        }
                    }
                }
            }
            else if (msgA[0].Equals("查树"))
            {
                String sendtext = "挂树成员：\n";
                foreach (var t in tree)
                {
                    sendtext += MemberName(e, t.Key) + "\n";
                }
                e.FromGroup.SendGroupMessage(sendtext);
            }
            else if (msgA[0].Equals("查看"))
            {
                if (bossDate != null)
                {
                    if (MaxAttackCount == 1)
                    {
                        e.FromGroup.SendGroupMessage((Attack ? MemberName(e, AttackMember) + "正在挑战boss\n" : "")
                            + "当前" + week + "周目，" + (targetBoss + 1) + "号boss生命值" + bossDate[targetBoss]);
                    }
                    else
                        e.FromGroup.SendGroupMessage(AllAttackMember(e) + (AttackMembers.Count > 0 ? "正在挑战boss\n" : "")
                            + "当前" + week + "周目，" + (targetBoss + 1) + "号boss生命值" + bossDate[targetBoss]);
                }
                else
                {
                    e.FromGroup.SendGroupMessage("暂无boss数据，请录入！");
                }
            }
            else if (msgA[0].Equals("强行下树"))
            {
                if (tree.ContainsKey(e.FromQQ.Id))
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
                                AttackMembers[0] = 0;
                                int dmg = bossDate[targetBoss];
                                bossDate[targetBoss] = 0;
                                targetBoss++;
                                if (targetBoss == boss.Length)
                                {
                                    week++;
                                    for (int i = 0; i < boss.Length; i++)
                                    {
                                        bossDate[i] = boss[i];
                                    }
                                }
                                targetBoss %= boss.Length;
                                string temp = MemberName(e) + "已强行下树";
                                e.FromGroup.SendGroupMessage(temp + "\n现在" + week +
                                        "周目，" + (targetBoss + 1) + "号boss生命值" + bossDate[targetBoss]);
                                tree.Remove(e.FromQQ.Id);
                                tree.Clear();
                                SaveLog(temp + " 击杀");
                                
                                Record(e, e.FromQQ.Id, dmg);
                                SaveDate(e, SaveDatPer);
                            }
                            else if (int.TryParse(msgA[1], out damage))
                            {
                                if (damage > bossDate[targetBoss] || damage < 0)
                                {
                                    e.FromGroup.SendGroupMessage("请正确输入伤害，如果已击杀请输入“击杀”");
                                }
                                else
                                {
                                    bossDate[targetBoss] -= damage;
                                    CheackBossBlood();
                                    string temp = MemberName(e) + "已强行下树";
                                    e.FromGroup.SendGroupMessage(temp + "\n现在" + week +
                                        "周目，" + (targetBoss + 1) + "号boss生命值" + bossDate[targetBoss]);
                                    tree.Remove(e.FromQQ.Id);
                                    SaveLog(temp + " " + damage);
                                   
                                    Record(e, e.FromQQ.Id, damage);
                                    SaveDate(e, SaveDatPer);
                                }
                            }
                        }
                    }
                }
            }
            else if (msgA[0].Equals("修正"))
            {
                if (isRoot(e.FromQQ.Id))
                {
                    if (bossDate != null)
                    {
                        int temptar = 0;
                        int tempweek = 1;
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
                                if (temptar > boss.Length || temptar < 0)
                                {
                                    e.FromGroup.SendGroupMessage("请正确输入当前第几个boss！");
                                }
                                else
                                {
                                    targetBoss = temptar - 1;
                                }
                                if (tempboss > boss[targetBoss] || tempboss <= 0)
                                {
                                    e.FromGroup.SendGroupMessage("请正确输入当前boss血量!");
                                }
                                else
                                {
                                    bossDate[targetBoss] = tempboss;
                                }

                                e.FromGroup.SendGroupMessage("当前" + week + "周目，" + (targetBoss + 1) + "号boss生命值" + bossDate[targetBoss]);
                                SaveLog(MemberName(e) + " 修正了boss数据");

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
            else if (msgA[0].Equals("出刀数据"))
            {
                if (isRoot(e.FromQQ.Id))
                {
                    string s = string.Empty;
                    if (msgA.Length >= 2 && msgA[1].Equals("今天"))
                    {
                        int attackTimesAll = 0;
                        foreach (var d in memberDate)
                        {
                            s += d.Value.LookDateRoot(Day());
                            attackTimesAll += d.Value.DayAttack(Day());
                        }
                        s += "今日总刀数 " + attackTimesAll + "刀\n";
                    }
                    else
                    {
                        foreach (var d in memberDate)
                        {
                            s += d.Value.LookDateRoot();
                        }
                    }
                    e.FromGroup.SendGroupMessage(s + "更详细的数据请看日志文件");
                }
                else
                {
                    if (memberDate.ContainsKey(e.FromQQ.Id))
                    {
                        e.FromGroup.SendGroupMessage(memberDate[e.FromQQ.Id].LookDate(msgA.Length >= 2));
                    }
                    else
                    {
                        e.FromGroup.SendGroupMessage("暂无你的出刀数据!");
                    }
                }
            }
            else if (msgA[0].Equals("清除数据"))
            {
                if (isRoot(e.FromQQ.Id))
                {
                    e.FromGroup.SendGroupMessage("即将清除成员的出刀，确认？\n[是/否]");
                    delDate = true;
                }
                else
                {
                    e.FromGroup.SendGroupMessage("你不是管理者!");

                }
            }
            else if (msgA[0].Equals("解除出刀"))
            {
                if (isRoot(e.FromQQ.Id))
                {
                    Attack = false;
                    AttackMember = 0;
                    AttackMembers.Clear();
                    e.FromGroup.SendGroupMessage("解除出刀成功！");
                }
            }
            else if (msgA[0].Equals("砍树"))
            {
                if (isRoot(e.FromQQ.Id))
                {
                    tree.Clear();
                    e.FromGroup.SendGroupMessage("砍树成功！");
                }
            }
            else if (msgA[0].Equals("代打"))
            {
                if (isRoot(e.FromQQ.Id))
                {
                    if (msgA.Length < 3||e.Message.CQCodes.Count<=0)
                    {
                        e.FromGroup.SendGroupMessage("请正确输入代打数据 格式为：\n代打 伤害(或击杀) at(@)代打账号拥有者");
                    }
                    else
                    {
                        var qq = e.Message.CQCodes.First();
                        long qqid = 0;
                        if (qq.Function == CQFunction.At)
                        {
                            if (qq.Items.ContainsKey("qq"))
                            {
                                long.TryParse(qq.Items["qq"],out qqid);
                            }
                        }
                        if (qqid == 0)
                        {
                            e.FromGroup.SendGroupMessage("请正确输入代打数据 格式为：\n代打 伤害(或击杀) at(@)代打账号拥有者");
                            
                        }
                        else if (msgA[1].Equals("击杀"))
                        {
                            int dmg = bossDate[targetBoss];
                            bossDate[targetBoss] = 0;
                            targetBoss++;
                            if (targetBoss == boss.Length)
                            {
                                week++;
                                for (int i = 0; i < boss.Length; i++)
                                {
                                    bossDate[i] = boss[i];
                                }
                            }
                            targetBoss %= boss.Length;
                            string temp = MemberName(e,qqid) + " 已完成挑战boss（代打）";
                            e.FromGroup.SendGroupMessage(temp + "\n现在" + week +
                                    "周目，" + (targetBoss + 1) + "号boss生命值" + bossDate[targetBoss]);
                            SaveLog(temp + " 击杀 ");

                            Record(e, qqid, dmg);
                            SaveDate(e, SaveDatPer);
                            tree.Clear();
                        }
                        else if (int.TryParse(msgA[1], out int damage))
                        {
                            if (damage > bossDate[targetBoss] || damage < 0)
                            {
                                e.FromGroup.SendGroupMessage("请正确输入伤害，如果已击杀请输入“击杀”");
                            }
                            else
                            {
                                bossDate[targetBoss] -= damage;
                                CheackBossBlood();
                                string temp = MemberName(e,qqid) + " 已完成挑战boss（代打）";
                                e.FromGroup.SendGroupMessage(temp + "\n现在" + week +
                                    "周目，" + (targetBoss + 1) + "号boss生命值" + bossDate[targetBoss]);
                                SaveLog(temp + " " + damage);
                                Record(e, qqid, damage);
                                SaveDate(e, SaveDatPer);
                            }
                        }
                    }
                }
                else
                {
                    e.FromGroup.SendGroupMessage("只有管理者才能输入代打数据！");
                }
            }
            else if (msgA[0].Equals("挑战"))
            {
                if (isRoot(e.FromQQ.Id))
                {
                    if (msgA.Length < 3 || e.Message.CQCodes.Count <= 0)
                    {
                        e.FromGroup.SendGroupMessage("请正确输入数据 格式为：\n挑战 伤害 at(@)账号拥有者");
                    }
                    else
                    {
                        var qq = e.Message.CQCodes.First();
                        long qqid = 0;
                        if (qq.Function == CQFunction.At)
                        {
                            if (qq.Items.ContainsKey("qq"))
                            {
                                long.TryParse(qq.Items["qq"], out qqid);
                            }
                        }
                        if (qqid == 0)
                        {
                            e.FromGroup.SendGroupMessage("请正确输入代打数据 格式为：\n挑战 伤害 at(@)账号拥有者");

                        }
                        else if (int.TryParse(msgA[1], out int damage))
                        {
                            if (damage > bossDate[targetBoss] || damage < 0)
                            {
                                e.FromGroup.SendGroupMessage("请正确输入伤害！");
                            }
                            else
                            {
                                bossDate[targetBoss] -= damage;
                                CheackBossBlood();
                                string temp = MemberName(e, qqid) + " 已完成挑战boss（手动输入）";
                                e.FromGroup.SendGroupMessage(temp + "\n现在" + week +
                                    "周目，" + (targetBoss + 1) + "号boss生命值" + bossDate[targetBoss]);
                                SaveLog(temp + " " + damage);
                                Record(e, qqid, damage);
                                SaveDate(e, SaveDatPer);
                            }
                        }
                    }
                }
                else
                {
                    e.FromGroup.SendGroupMessage("只有管理者才能手动输入挑战数据！");
                }
            }
            else if (msgA[0].Equals("读取"))
            {
                if (isRoot(e.FromQQ.Id))
                {
                    LoadDate(e);
                }
            }
            else if (msgA[0].Equals("保存"))
            {
                if (isRoot(e.FromQQ.Id))
                {
                    if (memberDate.Count > 0)
                    {
                        SaveDate(e, true,true);
                        SaveMemberDate();
                    }
                }
            }
            else if (msgA[0].Equals("指令详情"))
            {
                e.FromGroup.SendGroupMessage("boss录入\n", "申请出刀\n", "完成\n", "挂树\n", "强行下树\n", "查树\n", "查看\n", "修正\n", "出刀数据\n");

            }
        }

        private bool NeedChangeDate(string[] msgA)
        {
            return (msgA[0].Equals("boss录入") || msgA[0].Equals("申请出刀") || msgA[0].Equals("完成") || msgA[0].Equals("强行下树") ||
                msgA[0].Equals("清除数据") || msgA[0].Equals("代打") || msgA[0].Equals("挑战") || msgA[0].Equals("读取") || msgA[0].Equals("保存"));
        }

        private bool isRoot(long qq)
        {
            return rootMember.Contains(qq);
        }

        private void hb(CQGroupMessageEventArgs e)
        {
            if (e.Message.Text.IndexOf("hb") >= 0)
            {
                var code = e.Message.CQCodes;
                if (code.Count >= 1)
                {
                    if (code[0].Function == CQFunction.Hb)
                    {
                        e.FromGroup.SendGroupMessage(MemberName(e) + "氪金成功，获得1500宝石");
                    }
                }
            }
        }

        private void CheackBossBlood()
        {
            if (bossDate[targetBoss] == 0)
            {
                targetBoss++;
                if (targetBoss == boss.Length)
                {
                    week++;
                    for (int i = 0; i < boss.Length ; i++)
                    {
                        bossDate[i] = boss[i];
                    }
                }
                targetBoss %= boss.Length;
                tree.Clear();
            }
        }

        private void Record(CQGroupMessageEventArgs e, long qq,int damage)
        {
            try
            {
                int day = Day();
                if (memberDate.ContainsKey(qq))
                {
                    if (memberDate[qq].memberDateDay.ContainsKey(day))
                    {
                        memberDate[qq].memberDateDay[day].attackTimes++;
                        memberDate[qq].memberDateDay[day].damage.Add(damage);
                    }
                    else
                    {
                        memberDate[qq].memberDateDay.Add(day, new MemberDateDay());
                        memberDate[qq].memberDateDay[day].attackTimes++;
                        memberDate[qq].memberDateDay[day].damage.Add(damage);
                    }
                }
                else
                {
                    memberDate.Add(qq, new MemberDate(qq, MemberName(e,qq)));
                    memberDate[qq].memberDateDay.Add(day, new MemberDateDay());
                    memberDate[qq].memberDateDay[day].attackTimes++;
                    memberDate[qq].memberDateDay[day].damage.Add(damage);
                }
            }catch(Exception ex)
            {
                e.CQLog.Warning("记录数据失败" + ex.Message);
            }
            try
            {
                SaveMemberDate();
            }
            catch (Exception ex)
            {
                e.CQLog.Warning("储存数据失败" + ex.Message);
            }
        }

        readonly string setPath = @"data/app/com.prcbot";
        readonly string setPathData = @"data/app/com.prcbot/set.data";

        private void Save()
        {
            StreamWriter sw=null;
            if (!Directory.Exists(setPath))
            {
                try
                {
                    Directory.CreateDirectory(setPath);
                }catch(Exception ex)
                {
                    if (e != null)
                    {
                        e.CQLog.Info("存档文件夹创建失败，请查看读写权限是否拥有！" + ex.Message);
                    }
                }
            }
            if (!File.Exists(logPath))
            {
                try
                {
                    sw = File.CreateText(logPath);
                }
                catch (Exception ex)
                {
                    if (e != null)
                    {
                        e.CQLog.Info("存档文件创建失败，请查看读写权限是否拥有！" + ex.Message);
                    }
                }

            }
            if(sw==null)
                sw = new StreamWriter(logPath, true, Encoding.UTF8);
            if (sw != null)
            {
                for (; Logp < log.Count; Logp++)
                {
                    sw.WriteLine(log[Logp]);
                }
                sw.Close();
            }
        }

        private string AllAttackMember(CQGroupMessageEventArgs e)
        {
            string s = string.Empty;
            if (MaxAttackCount==1)
            {
                return MemberName(e, AttackMember);
            }
            else
            {
                if (AttackMembers.Count == 0)
                {
                    return s;
                }
                try
                {
                    foreach (var qq in AttackMembers)
                    {
                        s += MemberName(e, qq.Key) + "\n";
                    }
                }
                catch (Exception ex)
                {
                    e.CQLog.Warning("遍历挑战成员异常！" + ex.Message);
                }
                s += "" + AttackMembers.Count + "人";
            }
            return s;
        }

        private void SaveLog(string s)
        {
            string temp =DateTime.Now+" , "+ s+"\n"+ week +" , " + (targetBoss + 1) + " , " + bossDate[targetBoss];
            log.Add(temp);
            Save();
            if (log.Count >= 30)
            {
                log.Clear();
                Logp = 0;
            }
        }

        /// <summary>
        /// 获取已保存的指令前缀，如果没有set文件则不读取
        /// </summary>
        public void ReadKey()
        {
            try
            {
                string[] str;
                if (File.Exists(setPathData))
                {
                    str = File.ReadAllLines(setPathData);
                    if (str != null)
                    {
                        if (str.Length >= 3)
                        {
                            key = str[2];

                        }

                    }
                }
            }
            catch
            {
            }
        }

        public void SaveMemberDate(bool flag=false)
        {
            StreamWriter sw = null;
            if (!Directory.Exists(setPath))
            {
                try { 
                Directory.CreateDirectory(setPath);
                }
                catch (Exception ex)
                {
                    if (e != null)
                    {
                        e.CQLog.Info("存档文件创建失败，请查看读写权限是否拥有！" + ex.Message);
                    }
                }
            }
            if (!File.Exists(DatePath))
            {
                sw = File.CreateText(DatePath);

            }
            if (sw == null)
                sw = new StreamWriter(DatePath, false, Encoding.UTF8);
            foreach (var date in memberDate)
            {
                sw.WriteLine(date.Value);
            }
            sw.Close();
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
                    if (str.Length >= 4)
                    {
                        int.TryParse(str[3], out MaxAttackCount);
                        
                    }
                    if (str.Length >= 5)
                    {
                        SaveDatPer= str[4].Equals("1");
                    }
                }
            }
        }

        private string MemberName(CQGroupMessageEventArgs e)
        {
            if (memberDate.ContainsKey(e.FromQQ.Id))
            {
                return memberDate[e.FromQQ.Id].name;
            }
            else
            {
                try
                {
                    var m = e.FromQQ.GetGroupMemberInfo(e.FromGroup.Id);
                    return m.Card.Length <= 0 ? m.Nick : m.Card;
                }
                catch (Exception ex)
                {
                    e.CQLog.Warning("获取成员信息失败\n" + ex.Message);
                    return "" + e.FromQQ.Id;
                }
            }
        }

        private string MemberName(CQGroupMessageEventArgs e,long qq)
        {
            if (qq == 0)
            {
                return "";
            }
            if (memberDate.ContainsKey(qq))
            {
                return memberDate[qq].name;
            }
            else
            {
                try
                {
                    var m = e.CQApi.GetGroupMemberInfo(e.FromGroup.Id, qq);
                    return m.Card.Length <= 0 ? m.Nick : m.Card;
                }
                catch (Exception ex)
                {
                    e.CQLog.Warning("获取成员信息失败\n" + ex.Message);
                    return "" + qq;
                }
            }
        }

        private int Day()
        {
            var now = DateTime.Now;
            return now.Day - (now.Hour < 5 ? 1 : 0);
        }

        private void SaveDate(CQGroupMessageEventArgs e=null,bool flag=true,bool flag1=false)
        {
            if (flag)
            {
                string path = DatePath + ".dat";
                FileStream sw = null;
                try
                {
                    if (!Directory.Exists(setPath))
                    {
                        try { 
                        Directory.CreateDirectory(setPath);
                        }
                        catch (Exception ex)
                        {
                            if (e != null)
                            {
                                e.CQLog.Info("dat文件夹创建失败，请查看读写权限是否拥有！" + ex.Message);
                            }
                        }

                    }
                    if (!File.Exists(path))
                    {
                        try { 
                        sw = File.Create(path);
                        }
                        catch (Exception ex)
                        {
                            if (e != null)
                            {
                                e.CQLog.Info("dat文件创建失败，请查看读写权限是否拥有！" + ex.Message);
                            }
                        }

                    }
                    if (sw == null)
                        sw = new FileStream(path, FileMode.OpenOrCreate);
                    if (sw != null)
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(sw, memberDate);
                        sw.Close();
                    }
                    if (e != null)
                    {
                        if (flag1)
                        {
                            e.FromGroup.SendGroupMessage("保存成功!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (e != null)
                    {
                        e.CQLog.Warning("保存dat数据失败!" + ex.Message);
                        e.FromGroup.SendGroupMessage("保存失败，请查看酷q日志");
                    }
                    else if (Common.CQLog != null)
                    {
                        Common.CQLog.Warning("保存dat数据失败!" + ex.Message);
                    }
                }
            }
        }

        private void LoadDate(CQGroupMessageEventArgs e = null)
        {

            string path = DatePath + ".dat";
            if (File.Exists(path))
            {
                FileStream fs = new FileStream(DatePath + ".dat", FileMode.Open);
                BinaryFormatter bf = new BinaryFormatter();
                memberDate = bf.Deserialize(fs) as Dictionary<long, MemberDate>;
                fs.Close();
                if (File.Exists(logPath))
                {
                    string []s = File.ReadAllLines(logPath);
                    string []temps = s[s.Length - 1].Split(' ');
                    if (temps.Length >= 5)
                    {
                        int.TryParse(temps[0], out week);
                        int.TryParse(temps[2], out int temptarget);
                        targetBoss = temptarget - 1;
                        int.TryParse(temps[4], out bossDate[targetBoss]);
                    }
                }

                if (e != null)
                {
                    e.FromGroup.SendGroupMessage("读取数据成功");
                }
            }
        }

    }

    [Serializable]
    public class MemberDateDay
    {
        public int attackTimes { get; set; }
        public List<int> damage { get; set; }
        public MemberDateDay()
        {
            attackTimes = 0;
            damage = new List<int>();
        }
        public int DayTotal()
        {
            int total = 0;
            foreach(var d in damage)
            {
                total += d;
            }
            return total;
        }
        public override string ToString()
        {
            attackTimes = damage.Count();
            string s = "[" + attackTimes+"]";
            foreach(var d in damage)
            {
                s += ", " + d;
            }
            return s + ", (" + DayTotal() + ")";
        }
    }

    [Serializable]
    public class MemberDate
    {
        public string name { get; set; }
        public long id { get; set; }
        public Dictionary<int,MemberDateDay> memberDateDay { get; set; }
        public MemberDate()
        {
            memberDateDay = new Dictionary<int, MemberDateDay>();
        }
        public MemberDate(long qq,string name)
        {
            memberDateDay = new Dictionary<int, MemberDateDay>();
            id = qq;
            this.name = name;
        }
        public long TotalDmg()
        {
            long total = 0;
            foreach(var d in memberDateDay)
            {
                total += d.Value.DayTotal();
            }
            return total;
        }
        public string LookDateRoot(int Day=0)
        {
            if (Day == 0)
            {
                string s = "" + name;
                int times = 0;
                foreach (var d in memberDateDay)
                {
                    times += d.Value.attackTimes;
                }
                return s + ", " + times + ", " + TotalDmg() + "\n";
            }
            else
            {
                if (memberDateDay.ContainsKey(Day))
                {
                    string s = "" + name;
                    return s + ", " + memberDateDay[Day].attackTimes + ", " + memberDateDay[Day].DayTotal() + "\n";
                }
                else
                    return "";
            }
        }
        public string LookDate(bool flag=false)
        {
            string s = "" + name+"\n";
            int times = 0;
            foreach (var d in memberDateDay)
            {
                times += d.Value.attackTimes;
            }
            s += "总完成刀数：[[" + times + "]]\n";
            s += "总伤害:((" + TotalDmg() + "))\n";
            s += "————\n";
            foreach (var day in memberDateDay)
            {
                s += "日期:{" + day.Key + "}\n";
                s += "完成刀数：[" + day.Value.attackTimes + "]\n";
                s += "当日总伤害: (" + day.Value.DayTotal() + ")\n";
                if (flag)
                {
                    foreach (var d in day.Value.damage)
                    {
                        s += "" + d + "\n";
                    }
                }
                s += "————\n";
            }
            return s;
        }
        public override string ToString()
        {
            string s = ""+name+", "+id;
            string ss = string.Empty;
            int times=0;
            foreach(var day in memberDateDay)
            {
                times += day.Value.attackTimes;
                ss += ", {" + day.Key + "}, " + day.Value;
            }
            return s + ", [["+times+"]], ((" + TotalDmg() + ")) "+ss+"\n";

        }
        public int DayAttack(int day)
        {
            if (memberDateDay.ContainsKey(day))
            {
                return memberDateDay[day].attackTimes;
            }
            else
                return 0;
        }
    }
}
