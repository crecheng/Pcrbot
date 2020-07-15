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
using Native.Sdk.Cqp;

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
        private List<long> UnRemind = new List<long>();
        private CQGroupMessageEventArgs e;
        public int reMsgMaxLine=-1;
        public int lotteryTimes;
        Lottery lottery = new Lottery();
        ClubFight clubFight = new ClubFight();
        public string RandomImgStr = "嘤嘤嘤";
        private string[] imgName;

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
            lotteryTimes = -1;
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
                    String msg = e.Message.Text;
                    msg = msg.Substring(key.Length);
                    String[] msgA = msg.Split(' ');
                    ClubFightMsg(e,msgA);
                    LotteryMsg(e, msgA);
                    SengImg(e, msg);
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

        private void SengImg(CQGroupMessageEventArgs e, string msg)
        {
            if (msg.Equals(RandomImgStr))
            {
                if (e.CQApi.IsAllowSendImage)
                {
                    if (Directory.Exists(@"data\image\pcrbotRandomPng")){
                        if (imgName == null)
                        {
                            imgName = Directory.GetFiles(@"data\image\pcrbotRandomPng");
                            for(int i=0;i<imgName.Length;i++)
                            {
                                string[] temps = imgName[i].Split('\\');
                                imgName[i] = "pcrbotRandomPng\\"+temps[temps.Length - 1];
                            }
                        }
                        if(imgName!=null)
                        {
                            string temp = imgName[MyRandom.NextTo(imgName.Length)];
                            if (File.Exists("data\\image\\" + temp))
                            {
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_Image(temp));
                                e.Handler = true;
                            }
                        }
                    }
                }
            }
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

        private void LotteryMsg(CQGroupMessageEventArgs e, string[] msgA)
        {
            foreach (var answer in MyString.str)
            {
                if (answer.Value.Equals(msgA[0]))
                {
                    lottery.MsgHandle(msgA, answer.Key, e, this);
                }
            }
        }

        private void ClubFightMsg(CQGroupMessageEventArgs e,string[] msgA)
        {
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

            foreach (var answer in MyString.str)
            {
                if (answer.Value.Equals(msgA[0]))
                {
                    clubFight.MsgHandle(msgA, answer.Key, e, this);
                }
            }
        }

        public void BossBloodSub(int dmg)
        {
            bossDate[targetBoss] -= dmg;
            CheackBossBlood();
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

        public void ChangePool(int s1c, string s1, int s2c, string s2, int s3c, string s3)
        {
            lottery.ChangePool(s1c, s1, s2c, s2, s3c, s3);
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
        readonly string strPathData = @"data\app\com.prcbot\str.data";

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
                    if (str.Length >= 6)
                    {
                        int.TryParse(str[5],out reMsgMaxLine);
                    }
                    if (str.Length >= 7)
                    {
                        int.TryParse(str[6], out lotteryTimes);
                    }
                    if (str.Length >= 12)
                    {
                        int.TryParse(str[7],out int star1c);
                        string star1 = str[8];

                        int.TryParse(str[9],out int star2c);
                        string star2 = str[10];

                        int.TryParse(str[11],out int star3c);
                        string star3 = str[12];
                        lottery.ChangePool(star1c, star1, star2c, star2, star3c, star3);
                    }
                }
            }

            if (File.Exists(strPathData))
                SetMyStr(File.ReadAllLines(strPathData));
            else
                MyString.ResetStr();
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
            var newnow = now.AddHours(-5);
            return newnow.Day;
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

        class ClubFight
        {
            CQGroupMessageEventArgs e;
            Pcrbot p;
            string[] msg;
            public void MsgHandle(string[] msg, string answer, CQGroupMessageEventArgs e, Pcrbot p)
            {
                this.e = e;
                this.p = p;
                this.msg = msg;
                switch (answer)
                {
                    case "boss录入":
                        BossDateInput();
                        break;
                    case "申请出刀":
                        ApplyAttack();
                        break;
                    case "完成":
                        FinishAttack();
                        break;
                    case "挂树":
                        UpTree();
                        break;
                    case "查树":
                        LookTree();
                        break;
                    case "查看":
                        LookBoss();
                        break;
                    case "强行下树":
                        DownTree();
                        break;
                    case "修正":
                        ModifyBoss();
                        break;
                    case "出刀数据":
                        AttackDate();
                        break;
                    case "清除数据":
                        DelDate();
                        break;
                    case "解除出刀":
                        DelAttackMember();
                        break;
                    case "砍树":
                        CutTree();
                        break;
                    case "代打":
                        HelpAttack1();
                        break;
                    case "挑战":
                        HelpAttack2();
                        break;
                    case "读取":
                        Load();
                        break;
                    case "保存":
                        Save();
                        break;
                    case "指令详情":
                        Help();
                        break;
                    case "全员入会":
                        JoinAll();
                        break;
                    case "入会":
                        JoinA();
                        break;
                    case "退会":
                        DelMember();
                        break;
                    case "催刀":
                        Remind();
                        break;
                    case "删刀":
                        DelAttack();
                        break;
                }
            }

            void SendMsg(string s)
            {
                if(s.Length<=100||p.reMsgMaxLine<=3)
                    e.FromGroup.SendGroupMessage(s);
                else
                {
                    var tempstr = s.Split('\n');
                    StringBuilder temps = new StringBuilder();
                    for(int i = 0,j=0 ; i < tempstr.Length; i++)
                    {
                        if (j <= p.reMsgMaxLine)
                        {
                            temps.Append(tempstr[i] + "\n");
                            j++;
                        }
                        else
                        {
                            e.FromGroup.SendGroupMessage(temps);
                            temps.Clear();
                            temps.Append(tempstr[i]+"\n");
                            j = 1;
                        }
                    }
                }
            }

            void BossDateInput()
            {
                
                if (p.isRoot(e.FromQQ.Id))
                {
                    if (msg.Length > 1)
                    {
                        e.CQLog.Debug("" + msg.Length);
                        bool tempb = true;
                        p.targetBoss = 0;
                        p.AttackMembers = new Dictionary<long, int>();
                        p.Attack = false;
                        p.week = 1;
                        p.AttackMember = 0;
                        p.bossDate = new int[msg.Length - 1];
                        p.boss = new int[msg.Length - 1];
                        for (int i = 1; i < msg.Length; i++)
                        {
                            int temp;
                            if (int.TryParse(msg[i], out temp))
                            {
                                p.boss[i - 1] = temp * 10000;
                                p.bossDate[i - 1] = temp * 10000;
                            }
                            else
                            {
                                tempb = false;
                                SendMsg("正确录入" + i + "个\n请正确输入，后续应为整数，且单位为万\n例如\n——\nboss录入 200\n——\n表示录入1个200w血boss");
                                break;
                            }
                        }
                        if (tempb)
                        {
                            string temp = "成功录入" + p.boss.Length + "个数据";
                            e.FromGroup.SendGroupMessage(temp);
                            p.SaveLog(p.MemberName(e) + "  " + temp);
                        }
                    }
                    else
                    {
                        SendMsg("请正确输入，后续应为整数，且单位为万\n例如\n——\nboss录入 200\n——\n表示录入1个200w血boss");
                    }
                }
                else
                {
                    SendMsg(MyString.str["你不是管理者账户，无法进行此操作！"]);
                }
            }

            void ApplyAttack()
            {
                if (p.bossDate != null)
                {
                    if (p.MaxAttackCount == 1)
                    {
                        if (p.Attack)
                        {
                            e.FromGroup.SendGroupMessage(p.MemberName(e, p.AttackMember) + MyString.str["正在攻击boss，请稍等！"]);
                        }
                        else
                        {
                            if (p.tree.ContainsKey(e.FromQQ.Id))
                            {
                                e.FromGroup.SendGroupMessage(p.MemberName(e) + MyString.str[" 已挂树，无法出刀"]);
                            }
                            else
                            {
                                p.Attack = true;
                                p.AttackMember = e.FromQQ.Id;
                                string temp = p.MemberName(e) + MyString.str["  已开始挑战boss"];
                                e.FromGroup.SendGroupMessage(temp + "\n" +BossNow());
                                p.SaveLog(temp);
                            }
                        }
                    }
                    else
                    {
                        if (p.MaxAttackCount > p.AttackMembers.Count)
                        {
                            if (!p.AttackMembers.ContainsKey(e.FromQQ.Id))
                            {
                                if (p.tree.ContainsKey(e.FromQQ.Id))
                                {
                                    e.FromGroup.SendGroupMessage(p.MemberName(e) + MyString.str[" 已挂树，无法出刀"]);
                                }
                                else
                                {
                                    p.AttackMembers.Add(e.FromQQ.Id, p.week * 10 + p.targetBoss);
                                    string temp = p.MemberName(e) + MyString.str["  已开始挑战boss"];
                                    e.FromGroup.SendGroupMessage(temp + "\n" + p.AllAttackMember(e) + "正在挑战boss\n"+BossNow());
                                    p.SaveLog(temp);
                                }
                            }
                        }
                        else
                        {
                            e.FromGroup.SendGroupMessage(p.AllAttackMember(e) + MyString.str["正在挑战boss"] + "\n" + MyString.str["同时挑战人数达到设置上限！"]);
                        }
                    }
                }
                else
                {
                    e.FromGroup.SendGroupMessage("暂无boss数据，请录入！");
                }
            }

            void FinishAttack()
            {
                if (p.MaxAttackCount == 1)
                {
                    if (e.FromQQ.Id == p.AttackMember)
                    {
                        int damage;
                        if (msg.Length < 2)
                        {
                            e.FromGroup.SendGroupMessage("请在“完成”后空格后输入你的伤害或输入“击杀”");

                        }
                        else
                        {
                            if (msg[1].Equals("击杀"))
                            {
                                p.Attack = false;
                                p.AttackMember = 0;
                                int dmg = p.bossDate[p.targetBoss];
                                p.BossBloodSub(dmg);
                                string temp = p.MemberName(e) + " 已完成挑战boss";
                                e.FromGroup.SendGroupMessage(temp + "\n"+BossNow());
                                p.SaveLog(temp + " 击杀 ");
                                p.Record(e, e.FromQQ.Id, dmg);
                                p.SaveDate(e, p.SaveDatPer);
                                p.tree.Clear();
                            }
                            else if (int.TryParse(msg[1], out damage))
                            {
                                if (damage > p.bossDate[p.targetBoss] || damage < 0)
                                {
                                    e.FromGroup.SendGroupMessage("请正确输入伤害，如果已击杀请输入“击杀”");
                                }
                                else
                                {

                                    p.Attack = false;
                                    p.AttackMember = 0;
                                    p.BossBloodSub(damage);
                                    string temp = p.MemberName(e) + " 已完成挑战boss";
                                    e.FromGroup.SendGroupMessage(temp + "\n"+BossNow());
                                    p.SaveLog(temp + " " + damage);
                                    p.Record(e, e.FromQQ.Id, damage);
                                    p.SaveDate(e, p.SaveDatPer);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (p.AttackMembers.ContainsKey(e.FromQQ.Id))
                    {
                        int damage;
                        if (msg.Length < 2)
                        {
                            e.FromGroup.SendGroupMessage(MyString.str["请在“完成”后空格后输入你的伤害或输入“击杀”"]);

                        }
                        else
                        {
                            if (msg[1].Equals(MyString.str["击杀"]))
                            {
                                if (p.AttackMembers[e.FromQQ.Id] == p.week * 10 + p.targetBoss)
                                {
                                    p.AttackMembers.Remove(e.FromQQ.Id);
                                    int dmg = p.bossDate[p.targetBoss];
                                    p.BossBloodSub(dmg);
                                    string temp = p.MemberName(e) + MyString.str[" 已完成挑战boss"];
                                    e.FromGroup.SendGroupMessage(temp + "\n"+BossNow());
                                    p.SaveLog(temp + " 击杀 ");
                                    p.Record(e, e.FromQQ.Id, dmg);
                                    p.SaveDate(e, p.SaveDatPer);
                                    p.tree.Clear();
                                }
                                else
                                {
                                    p.AttackMembers.Remove(e.FromQQ.Id);
                                    e.FromGroup.SendGroupMessage(MyString.str["你已跨boss结算，当前数据不记录!"]);
                                }
                            }
                            else if (int.TryParse(msg[1], out damage))
                            {
                                if (damage > p.bossDate[p.targetBoss] || damage < 0)
                                {
                                    e.FromGroup.SendGroupMessage(MyString.str["请正确输入伤害，如果已击杀请输入“击杀”"]);
                                }
                                else
                                {
                                    if (p.AttackMembers[e.FromQQ.Id] == p.week * 10 + p.targetBoss)
                                    {
                                        p.BossBloodSub(damage);
                                        p.Attack = false;
                                        p.AttackMembers.Remove(e.FromQQ.Id);
                                        string temp = p.MemberName(e) + MyString.str[" 已完成挑战boss"];
                                        e.FromGroup.SendGroupMessage(temp + "\n"+BossNow());
                                        p.SaveLog(temp + " " + damage);

                                        p.Record(e, e.FromQQ.Id, damage);
                                        p.SaveDate(e, p.SaveDatPer);
                                    }
                                    else
                                    {
                                        p.AttackMembers.Remove(e.FromQQ.Id);
                                        e.FromGroup.SendGroupMessage(MyString.str["你已跨boss结算，当前数据不记录!"]);
                                    }
                                }

                            }
                        }

                    }
                }
            }

            void UpTree()
            {
                if (p.MaxAttackCount == 1)
                {
                    if (e.FromQQ == p.AttackMember)
                    {
                        var member = e.FromQQ.Id;
                        p.tree.Add(member, p.week * 10 + p.targetBoss);
                        string temp = p.MemberName(e) + MyString.str[" 已挂树"];
                        e.FromGroup.SendGroupMessage(temp);
                        p.Attack = false;
                        p.AttackMember = 0;
                        p.SaveLog(temp);
                    }
                }
                else
                {
                    if (p.AttackMembers.ContainsKey(e.FromQQ.Id))
                    {
                        if (p.AttackMembers[e.FromQQ.Id] == p.week * 10 + p.targetBoss)
                        {
                            var member = e.FromQQ.Id;
                            p.tree.Add(member, p.week * 10 + p.targetBoss);
                            string temp = p.MemberName(e) + MyString.str[" 已挂树"];
                            e.FromGroup.SendGroupMessage(temp);
                            p.AttackMembers.Remove(e.FromQQ.Id);
                            p.SaveLog(temp);
                        }
                        else
                        {
                            p.AttackMembers.Remove(e.FromQQ.Id);
                            e.FromGroup.SendGroupMessage(MyString.str["你已跨boss结算，当前数据不记录!"]);
                        }
                    }
                }
            }

            void LookTree()
            {
                String sendtext = MyString.str["挂树成员："] + "\n";
                foreach (var t in p.tree)
                {
                    sendtext += p.MemberName(e, t.Key) + "\n";
                }
                e.FromGroup.SendGroupMessage(sendtext);
            }

            void LookBoss()
            {
                if (p.bossDate != null)
                {
                    if (p.MaxAttackCount == 1)
                    {
                        e.FromGroup.SendGroupMessage((p.Attack ? p.MemberName(e, p.AttackMember) + "正在挑战boss\n" : "")+ BossNow());
                    }
                    else
                        e.FromGroup.SendGroupMessage(p.AllAttackMember(e) + (p.AttackMembers.Count > 0 ? "正在挑战boss\n" : "")+ BossNow());
                }
                else
                {
                    e.FromGroup.SendGroupMessage("暂无boss数据，请录入！");
                }
            }

            void DownTree()
            {
                if (p.tree.ContainsKey(e.FromQQ.Id))
                {
                    if (msg.Length >= 2)
                    {
                        int damage;
                        if (msg.Length < 2)
                        {
                            e.FromGroup.SendGroupMessage(MyString.str["请在“强行下树”后空格后输入你的伤害或击杀"]);

                        }
                        else
                        {
                            if (msg[1].Equals(MyString.str["击杀"]))
                            {
                                p.Attack = false;
                                p.AttackMembers[0] = 0;
                                int dmg = p.bossDate[p.targetBoss];
                                p.BossBloodSub(dmg);
                                string temp = p.MemberName(e) + MyString.str["已强行下树"];
                                e.FromGroup.SendGroupMessage(temp + "\n" + BossNow());
                                p.tree.Remove(e.FromQQ.Id);
                                p.tree.Clear();
                                p.SaveLog(temp + " 击杀");

                                p.Record(e, e.FromQQ.Id, dmg);
                                p.SaveDate(e, p.SaveDatPer);
                            }
                            else if (int.TryParse(msg[1], out damage))
                            {
                                if (damage > p.bossDate[p.targetBoss] || damage < 0)
                                {
                                    e.FromGroup.SendGroupMessage(MyString.str["请正确输入伤害，如果已击杀请输入“击杀”"]);
                                }
                                else
                                {
                                    p.BossBloodSub(damage);
                                    string temp = p.MemberName(e) + MyString.str["已强行下树"];
                                    e.FromGroup.SendGroupMessage(temp + "\n" + BossNow());
                                    p.tree.Remove(e.FromQQ.Id);
                                    p.SaveLog(temp + " " + damage);

                                    p.Record(e, e.FromQQ.Id, damage);
                                    p.SaveDate(e, p.SaveDatPer);
                                }
                            }
                        }
                    }
                }
            }

            void ModifyBoss()
            {
                if (p.isRoot(e.FromQQ.Id))
                {
                    if (p.bossDate != null)
                    {
                        int temptar = 0;
                        int tempweek = 1;
                        int tempboss = 0;
                        if (msg.Length >= 4)
                        {
                            if (int.TryParse(msg[1], out tempweek) && int.TryParse(msg[2], out temptar) && int.TryParse(msg[3], out tempboss))
                            {
                                p.week = tempweek;
                                for (int i = 0; i < p.boss.Length - 1; i++)
                                {
                                    p.bossDate[i] =p.boss[i];
                                }
                                if (temptar > p.boss.Length || temptar < 0)
                                {
                                    e.FromGroup.SendGroupMessage("请正确输入当前第几个boss！");
                                }
                                else
                                {
                                    p.targetBoss = temptar - 1;
                                }
                                if (tempboss > p.boss[p.targetBoss] || tempboss <= 0)
                                {
                                    e.FromGroup.SendGroupMessage("请正确输入当前boss血量!");
                                }
                                else
                                {
                                    p.bossDate[p.targetBoss] = tempboss;
                                }

                                e.FromGroup.SendGroupMessage(BossNow());
                                p.SaveLog(p.MemberName(e) + " 修正了boss数据");

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
                    e.FromGroup.SendGroupMessage(MyString.str["你不是管理者账户，无法进行此操作！"]);
                }
            }

            void AttackDate()
            {
                if (p.isRoot(e.FromQQ.Id))
                {
                    long qqid = GetAtQQ();
                    string s = string.Empty;
                    if (msg.Length >= 2 && msg[1].Equals(MyString.str["今天"]))
                    {
                        int attackMemberAll = 0;
                        int attackTimesAll = 0;
                        int temp = 0;
                        foreach (var d in p.memberDate)
                        {
                            s += d.Value.LookDateRoot(p.Day());
                            attackTimesAll += d.Value.DayAttack(p.Day());
                            if (temp != attackTimesAll)
                            {
                                attackMemberAll++;
                                temp = attackTimesAll;
                            }
                        }
                        s += "今日出刀人数：" + attackMemberAll + "\n今日总刀数 " + attackTimesAll + "刀\n";
                    }
                    else if (qqid != 0)
                    {
                        if (p.memberDate.ContainsKey(qqid))
                        {
                            SendMsg(p.memberDate[qqid].LookDate(msg.Length >= 2));
                        }
                        else
                        {
                            SendMsg(MyString.str["暂无你的出刀数据!"]);
                        }
                    }
                    else
                    {
                        foreach (var d in p.memberDate)
                        {
                            s += d.Value.LookDateRoot();
                        }
                    }
                    SendMsg(s);
                }
                else
                {
                    if (p.memberDate.ContainsKey(e.FromQQ.Id))
                    {
                        SendMsg(p.memberDate[e.FromQQ.Id].LookDate(msg.Length >= 2));
                    }
                    else
                    {
                        SendMsg(MyString.str["暂无你的出刀数据!"]);
                    }
                }
            }

            void DelDate()
            {
                if (p.isRoot(e.FromQQ.Id))
                {
                    e.FromGroup.SendGroupMessage("即将清除成员的出刀，确认？\n[是/否]");
                    p.delDate = true;
                }
                else
                {
                    e.FromGroup.SendGroupMessage("你不是管理者!");

                }
            }

            void DelAttackMember()
            {
                if (p.isRoot(e.FromQQ.Id))
                {
                    p.Attack = false;
                    p.AttackMember = 0;
                    p.AttackMembers.Clear();
                    e.FromGroup.SendGroupMessage(MyString.str["解除出刀成功！"]);
                }
            }

            void CutTree()
            {
                if (p.isRoot(e.FromQQ.Id))
                {
                    p.tree.Clear();
                    e.FromGroup.SendGroupMessage(MyString.str["砍树成功！"]);
                }
            }

            void HelpAttack1()
            {
                if (p.isRoot(e.FromQQ.Id))
                {
                    if (msg.Length < 3 || e.Message.CQCodes.Count <= 0)
                    {
                        e.FromGroup.SendGroupMessage(MyString.str["请正确输入代打数据 格式为："] + "\n" + MyString.str["代打 伤害(或击杀) at(@)代打账号拥有者"]);
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
                            e.FromGroup.SendGroupMessage(MyString.str["请正确输入代打数据 格式为："] + "\n" + MyString.str["代打 伤害(或击杀) at(@)代打账号拥有者"]);

                        }
                        else if (msg[1].Equals(MyString.str["击杀"]))
                        {
                            int dmg = p.bossDate[p.targetBoss];
                            p.BossBloodSub(dmg);
                            string temp = p.MemberName(e, qqid) + MyString.str[" 已完成挑战boss（代打）"];
                            e.FromGroup.SendGroupMessage(temp + "\n" + BossNow() );
                            p.SaveLog(temp + "  " + MyString.str["击杀"]);

                            p.Record(e, qqid, dmg);
                            p.SaveDate(e, p.SaveDatPer);
                            p.tree.Clear();
                        }
                        else if (int.TryParse(msg[1], out int damage))
                        {
                            if (damage > p.bossDate[p.targetBoss] || damage < 0)
                            {
                                e.FromGroup.SendGroupMessage(MyString.str["请正确输入伤害，如果已击杀请输入“击杀”"]);
                            }
                            else
                            {
                                p.BossBloodSub(damage);
                                string temp = p.MemberName(e, qqid) + MyString.str[" 已完成挑战boss（代打）"];
                                e.FromGroup.SendGroupMessage(temp + "\n" + BossNow());
                                p.SaveLog(temp + " " + damage);
                                p.Record(e, qqid, damage);
                                p.SaveDate(e, p.SaveDatPer);
                            }
                        }
                    }
                }
                else
                {
                    e.FromGroup.SendGroupMessage(MyString.str["只有管理者才能输入代打数据！"]);
                }
            }

            void HelpAttack2()
            {
                if (p.isRoot(e.FromQQ.Id))
                {
                    if (msg.Length < 3 || e.Message.CQCodes.Count <= 0)
                    {
                        e.FromGroup.SendGroupMessage(MyString.str["请正确输入数据 格式为："] + "\n" + MyString.str["挑战 伤害 at(@)账号拥有者"]);
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
                            e.FromGroup.SendGroupMessage(MyString.str["请正确输入数据 格式为："] + "\n" + MyString.str["挑战 伤害 at(@)账号拥有者"]);

                        }
                        else if (int.TryParse(msg[1], out int damage))
                        {
                            if (damage > p.bossDate[p.targetBoss] || damage < 0)
                            {
                                e.FromGroup.SendGroupMessage(MyString.str["请正确输入伤害！"]);
                            }
                            else
                            {
                                string temp = p.MemberName(e, qqid) + MyString.str[" 已完成挑战boss（手动输入）"];
                                e.FromGroup.SendGroupMessage(temp + "\n" + BossNow() );
                                p.SaveLog(temp + " " + damage);
                                p.Record(e, qqid, damage);
                                p.SaveDate(e, p.SaveDatPer);
                            }
                        }
                    }
                }
                else
                {
                    e.FromGroup.SendGroupMessage(MyString.str["只有管理者才能手动输入挑战数据！"]);
                }
            }

            void Load()
            {
                if (p.isRoot(e.FromQQ.Id))
                {
                    p.LoadDate(e);
                    SendMsg("读取成功！");
                }
            }

            void Save()
            {
                if (p.isRoot(e.FromQQ.Id))
                {
                    if (p.memberDate.Count > 0)
                    {
                        p.SaveDate(e, true, true);
                        p.SaveMemberDate();
                    }

                }
            }

            void Help()
            {
                e.FromGroup.SendGroupMessage("boss录入\n", "申请出刀\n", "完成\n", "挂树\n", "强行下树\n", "查树\n", "查看\n", "修正\n", "出刀数据\n");
            }

            string BossNow()
            {
                return "现在" + p.week + "周目，" + (p.targetBoss + 1) + "号boss生命值" + p.bossDate[p.targetBoss];
            }

            void JoinAll()
            {
                var glist = e.FromGroup.GetGroupMemberList();
                int count = 0;
                foreach(var qq in glist)
                {
                    long id = qq.QQ.Id;
                    if (!p.memberDate.ContainsKey(id))
                    {
                        p.memberDate.Add(id, new MemberDate(id, p.MemberName(e, id)));
                        p.memberDate[id].memberDateDay.Add(p.Day(), new MemberDateDay());
                        count ++;
                    }
                }
                if (p.memberDate.ContainsKey(e.CQApi.GetLoginQQId()))
                {
                    p.memberDate.Remove(e.CQApi.GetLoginQQId());
                    count--;
                }
                SendMsg(MyString.str["成功加入"] + count + MyString.str["名成员"]);
                p.SaveLog("管理者将全员加入公会");
                p.SaveDate(e);

            }

            void JoinA()
            {
                long qq = e.FromQQ.Id;
                if (msg.Length > 1)
                {
                    if (p.isRoot(qq))
                    {
                        long qqid = GetAtQQ();
                        if (qqid != 0)
                        {
                            if (p.memberDate.ContainsKey(qqid))
                            {
                                SendMsg(MyString.str["该成员已经在公会了"]);
                            }
                            else if (qqid == e.CQApi.GetLoginQQId())
                            {
                                SendMsg("不能将机器人加入公会!");
                            }
                            else
                            {
                                p.memberDate.Add(qqid, new MemberDate(qqid, p.MemberName(e, qqid)));
                                p.memberDate[qqid].memberDateDay.Add(p.Day(), new MemberDateDay());
                                SendMsg(MyString.str["成功加入公会！"]);
                                p.SaveLog("管理者将" + p.memberDate[qqid].name + " " + qqid + " 加入公会");
                                p.SaveDate(e);
                            }
                        }
                    }
                }
                else
                {
                    if (p.memberDate.ContainsKey(qq))
                    {
                        SendMsg(MyString.str["你已经在公会了"]);
                    }
                    else
                    {
                        p.memberDate.Add(qq, new MemberDate(qq, p.MemberName(e, qq)));
                        p.memberDate[qq].memberDateDay.Add(p.Day(), new MemberDateDay());
                        SendMsg(MyString.str["成功加入公会！"]);
                        p.SaveLog(p.memberDate[qq].name + " " + qq + " 入会");
                        p.SaveDate(e);
                    }
                }
            }

            void DelMember()
            {
                long qq = e.FromQQ.Id;
                if (msg.Length > 1)
                {
                    if (p.isRoot(qq))
                    {
                        long qqid = GetAtQQ();
                        if (qqid != 0)
                        {
                            if (p.memberDate.ContainsKey(qqid))
                            {
                                p.SaveLog("管理者将" + p.memberDate[qqid].name + " " + qqid + " 踢出");
                                p.memberDate.Remove(qqid);
                                SendMsg(MyString.str["该成员踢出了公会了"]);
                                p.SaveDate(e);
                            }
                            else
                            {
                                SendMsg(MyString.str["该成员还不在公会！"]);
                            }
                        } 
                    }
                }
                else
                {
                    if (p.memberDate.ContainsKey(qq))
                    {
                        p.SaveLog(p.memberDate[qq].name + " " + qq + " 退出了公会");
                        p.memberDate.Remove(qq);
                        SendMsg(MyString.str["你退出了公会了"]);
                        p.SaveDate(e);
                    }
                    else
                    {
                        SendMsg(MyString.str["你还不在公会！"]);
                    }
                }
            }

            void Remind()
            {
                if (p.isRoot(e.FromQQ.Id))
                {
                    StringBuilder s = new StringBuilder(MyString.str["管理催刀了！"] + "\n");
                    foreach(var qq in p.memberDate)
                    {
                        if (qq.Value.memberDateDay.ContainsKey(p.Day()))
                        {
                            if (qq.Value.memberDateDay[p.Day()].attackTimes < 3)
                            {
                                s.Append(CQApi.CQCode_At(qq.Key));
                            }
                        }
                        else
                        {
                            s.Append(CQApi.CQCode_At(qq.Key));
                        }
                    }
                    s.Append("\n");
                    s.Append(MyString.str["请以上成员立即出刀!"]);
                    SendMsg(s.ToString());
                }
            }

            void DelAttack()
            {
                long qq = GetAtQQ();
                int tDay,tCount;
                if(qq!=0&&int.TryParse(msg[1],out tDay)&&int.TryParse(msg[2],out tCount))
                {
                    if (p.memberDate.ContainsKey(qq))
                    {
                        if (p.memberDate[qq].memberDateDay.ContainsKey(tDay))
                        {
                            int dateCount = p.memberDate[qq].memberDateDay[tDay].damage.Count;
                            if ( dateCount>= tCount - 1)
                            {
                                p.memberDate[qq].memberDateDay[tDay].damage.RemoveAt(tCount - 1);
                                p.memberDate[qq].memberDateDay[tDay].attackTimes--;
                                SendMsg("删刀成功！");
                                p.SaveDate(e);
                            }
                            else
                            {
                                SendMsg("该成员当天只有"+dateCount+"刀！");
                            }
                        }
                        else
                        {
                            SendMsg("该成员当天没有出刀数据！");
                        }
                    }
                    else
                    {
                        SendMsg("该成员不在公会！");
                    }
                }
                else
                {
                    SendMsg("格式错误，格式为：删刀 日期 第几刀 at人");
                }
            }

            long GetAtQQ()
            {
                if (e.Message.CQCodes.Count > 0)
                {
                    var qq = e.Message.CQCodes.First();
                    if (qq != null && qq.Function == CQFunction.At)
                    {
                        if (qq.Items.ContainsKey("qq"))
                        {
                            if (long.TryParse(qq.Items["qq"], out long qqid))
                                return qqid;
                        }
                    }
                }
                return 0L;
            }
        }

        class Lottery
        {
            CQGroupMessageEventArgs e;
            Pcrbot p;
            private EggPool eggPool = new EggPool();
            string[] msg;
            public void MsgHandle(string[] msg, string answer, CQGroupMessageEventArgs e, Pcrbot p)
            {
                this.e = e;
                this.p = p;
                this.msg = msg;
                switch (answer)
                {
                    case "抽一井":
                        GetOneWell(e.CQApi.IsAllowSendImage);
                        break;
                }
            }

            public void ChangePool(int s1c,string s1,int s2c,string s2,int s3c,string s3)
            {
                eggPool.ChangePool(s1c, s1, s2c, s2, s3c, s3);
            }

            void GetOneWell(bool sendImg)
            {
                long qq = e.FromQQ.Id;
                if (p.lotteryTimes == -1)
                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(qq), eggPool.GetOneWell(sendImg));
                else if (p.lotteryTimes == 0)
                    return;
                else
                {
                    if (p.memberDate.ContainsKey(qq))
                    {
                        if (p.memberDate[qq].lotteryDay == p.Day())
                        {
                            if (p.memberDate[qq].lotteryTimes < p.lotteryTimes)
                            {
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(qq), "\n" + eggPool.GetOneWell(sendImg));
                                p.memberDate[qq].lotteryTimes++;
                            }
                            else
                            {
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(qq), "\n" + MyString.str["次数不足，明天再来！"]);
                            }
                        }
                        else
                        {
                            p.memberDate[qq].lotteryDay = p.Day();
                            p.memberDate[qq].lotteryTimes = 1;
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(qq), "\n" + eggPool.GetOneWell(sendImg));
                        }
                    }
                    else
                    {
                        p.memberDate.Add(qq, new MemberDate(qq, p.MemberName(e, qq)));
                        p.memberDate[qq].lotteryDay = p.Day();
                        p.memberDate[qq].lotteryTimes++;
                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(qq), "\n" + eggPool.GetOneWell(sendImg));
                    }
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
        public int lotteryDay { get; set; }
        public int lotteryTimes { get; set; }
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
            lotteryTimes = 0;
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

/*if (msgA[0].Equals(MyString.str["boss录入"]))
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
                    e.FromGroup.SendGroupMessage(MyString.str["你不是管理者账户，无法进行此操作！"]);
                }
            }
            else if (msgA[0].Equals(MyString.str["申请出刀"]))
            {
                if (bossDate != null)
                {
                    if (MaxAttackCount == 1)
                    {
                        if (Attack)
                        {
                            e.FromGroup.SendGroupMessage(MemberName(e, AttackMember) + MyString.str["正在攻击boss，请稍等！"]);
                        }
                        else
                        {
                            if (tree.ContainsKey(e.FromQQ.Id))
                            {
                                e.FromGroup.SendGroupMessage(MemberName(e) + MyString.str[" 已挂树，无法出刀"]);
                            }
                            else
                            {
                                Attack = true;
                                AttackMember = e.FromQQ.Id;
                                string temp = MemberName(e) + MyString.str["  已开始挑战boss"];
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
                                    e.FromGroup.SendGroupMessage(MemberName(e) + MyString.str[" 已挂树，无法出刀"]);
                                }
                                else
                                {
                                    AttackMembers.Add(e.FromQQ.Id, week * 10 + targetBoss);
                                    string temp = MemberName(e) + MyString.str["  已开始挑战boss"];
                                    e.FromGroup.SendGroupMessage(temp+"\n" + AllAttackMember(e) + "正在挑战boss\n现在" + week + "周目，" + (targetBoss + 1) + "号boss生命值" + bossDate[targetBoss]);
                                    SaveLog(temp);
                                }
                            }
                        }
                        else
                        {
                            e.FromGroup.SendGroupMessage(AllAttackMember(e) + MyString.str["正在挑战boss"]+"\n" +MyString.str["同时挑战人数达到设置上限！"]);
                        }
                    }
                }
                else
                {
                    e.FromGroup.SendGroupMessage("暂无boss数据，请录入！");
                }
            }
            else if (msgA[0].Equals(MyString.str["完成"]))
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
                            e.FromGroup.SendGroupMessage(MyString.str["请在“完成”后空格后输入你的伤害或输入“击杀”"]);

                        }
                        else
                        {
                            if (msgA[1].Equals(MyString.str["击杀"]))
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
                                    string temp = MemberName(e) + MyString.str[" 已完成挑战boss"];
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
                                    e.FromGroup.SendGroupMessage(MyString.str["你已跨boss结算，当前数据不记录!"]);
                                }
                            }
                            else if (int.TryParse(msgA[1], out damage))
                            {
                                if (damage > bossDate[targetBoss] || damage < 0)
                                {
                                    e.FromGroup.SendGroupMessage(MyString.str["请正确输入伤害，如果已击杀请输入“击杀”"]);
                                }
                                else
                                {
                                    if (AttackMembers[e.FromQQ.Id] == week * 10 + targetBoss)
                                    {


                                        bossDate[targetBoss] -= damage;
                                        CheackBossBlood();
                                        Attack = false;
                                        AttackMembers.Remove(e.FromQQ.Id);
                                        string temp = MemberName(e) + MyString.str[" 已完成挑战boss"];
                                        e.FromGroup.SendGroupMessage(temp + "\n现在" + week +
                                            "周目，" + (targetBoss + 1) + "号boss生命值" + bossDate[targetBoss]);
                                        SaveLog(temp + " " + damage);
                                        
                                        Record(e, e.FromQQ.Id, damage);
                                        SaveDate(e, SaveDatPer);
                                    }
                                    else
                                    {
                                        AttackMembers.Remove(e.FromQQ.Id);
                                        e.FromGroup.SendGroupMessage(MyString.str["你已跨boss结算，当前数据不记录!"]);
                                    }
                                }

                            }
                        }

                    }
                }
            }
            else if (msgA[0].Equals(MyString.str["挂树"]))
            {
                if (MaxAttackCount == 1)
                {
                    if (e.FromQQ == AttackMember)
                    {
                        var member = e.FromQQ.Id;
                        tree.Add(member, week * 10 + targetBoss);
                        string temp = MemberName(e) + MyString.str[" 已挂树"];
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
                            string temp = MemberName(e) + MyString.str[" 已挂树"];
                            e.FromGroup.SendGroupMessage(temp);
                            AttackMembers.Remove(e.FromQQ.Id);
                            SaveLog(temp);
                        }
                        else
                        {
                            AttackMembers.Remove(e.FromQQ.Id);
                            e.FromGroup.SendGroupMessage(MyString.str["你已跨boss结算，当前数据不记录!"]);
                        }
                    }
                }
            }
            else if (msgA[0].Equals(MyString.str["查树"]))
            {
                String sendtext = MyString.str["挂树成员："]+"\n";
                foreach (var t in tree)
                {
                    sendtext += MemberName(e, t.Key) + "\n";
                }
                e.FromGroup.SendGroupMessage(sendtext);
            }
            else if (msgA[0].Equals(MyString.str["查看"]))
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
            else if (msgA[0].Equals(MyString.str["强行下树"]))
            {
                if (tree.ContainsKey(e.FromQQ.Id))
                {
                    if (msgA.Length >= 2)
                    {
                        int damage;
                        if (msgA.Length < 2)
                        {
                            e.FromGroup.SendGroupMessage(MyString.str["请在“强行下树”后空格后输入你的伤害或击杀"]);

                        }
                        else
                        {
                            if (msgA[1].Equals(MyString.str["击杀"]))
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
                                string temp = MemberName(e) + MyString.str["已强行下树"];
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
                                    e.FromGroup.SendGroupMessage(MyString.str["请正确输入伤害，如果已击杀请输入“击杀”"]);
                                }
                                else
                                {
                                    bossDate[targetBoss] -= damage;
                                    CheackBossBlood();
                                    string temp = MemberName(e) + MyString.str["已强行下树"];
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
            else if (msgA[0].Equals(MyString.str["修正"]))
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
                    e.FromGroup.SendGroupMessage(MyString.str["你不是管理者账户，无法进行此操作！"]);
                }
            }
            else if (msgA[0].Equals(MyString.str["出刀数据"]))
            {
                if (isRoot(e.FromQQ.Id))
                {
                    string s = string.Empty;
                    if (msgA.Length >= 2 && msgA[1].Equals(MyString.str["今天"]))
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
                    e.FromGroup.SendGroupMessage(s);
                }
                else
                {
                    if (memberDate.ContainsKey(e.FromQQ.Id))
                    {
                        e.FromGroup.SendGroupMessage(memberDate[e.FromQQ.Id].LookDate(msgA.Length >= 2));
                    }
                    else
                    {
                        e.FromGroup.SendGroupMessage(MyString.str["暂无你的出刀数据!"]);
                    }
                }
            }
            else if (msgA[0].Equals(MyString.str["清除数据"]))
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
            else if (msgA[0].Equals(MyString.str["解除出刀"]))
            {
                if (isRoot(e.FromQQ.Id))
                {
                    Attack = false;
                    AttackMember = 0;
                    AttackMembers.Clear();
                    e.FromGroup.SendGroupMessage(MyString.str["解除出刀成功！"]);
                }
            }
            else if (msgA[0].Equals(MyString.str["砍树"]))
            {
                if (isRoot(e.FromQQ.Id))
                {
                    tree.Clear();
                    e.FromGroup.SendGroupMessage(MyString.str["砍树成功！"]);
                }
            }
            else if (msgA[0].Equals(MyString.str["代打"]))
            {
                if (isRoot(e.FromQQ.Id))
                {
                    if (msgA.Length < 3||e.Message.CQCodes.Count<=0)
                    {
                        e.FromGroup.SendGroupMessage(MyString.str["请正确输入代打数据 格式为："]+"\n"+ MyString.str["代打 伤害(或击杀) at(@)代打账号拥有者"]);
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
                            e.FromGroup.SendGroupMessage(MyString.str["请正确输入代打数据 格式为："] + "\n" + MyString.str["代打 伤害(或击杀) at(@)代打账号拥有者"]);
                            
                        }
                        else if (msgA[1].Equals(MyString.str["击杀"]))
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
                            string temp = MemberName(e,qqid) + MyString.str[" 已完成挑战boss（代打）"];
                            e.FromGroup.SendGroupMessage(temp + "\n现在" + week +
                                    "周目，" + (targetBoss + 1) + "号boss生命值" + bossDate[targetBoss]);
                            SaveLog(temp +"  " +MyString.str["击杀"]);

                            Record(e, qqid, dmg);
                            SaveDate(e, SaveDatPer);
                            tree.Clear();
                        }
                        else if (int.TryParse(msgA[1], out int damage))
                        {
                            if (damage > bossDate[targetBoss] || damage < 0)
                            {
                                e.FromGroup.SendGroupMessage(MyString.str["请正确输入伤害，如果已击杀请输入“击杀”"]);
                            }
                            else
                            {
                                bossDate[targetBoss] -= damage;
                                CheackBossBlood();
                                string temp = MemberName(e,qqid) + MyString.str[" 已完成挑战boss（代打）"];
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
                    e.FromGroup.SendGroupMessage(MyString.str["只有管理者才能输入代打数据！"]);
                }
            }
            else if (msgA[0].Equals(MyString.str["挑战"]))
            {
                if (isRoot(e.FromQQ.Id))
                {
                    if (msgA.Length < 3 || e.Message.CQCodes.Count <= 0)
                    {
                        e.FromGroup.SendGroupMessage(MyString.str["请正确输入数据 格式为："] + "\n" + MyString.str["挑战 伤害 at(@)账号拥有者"]);
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
                            e.FromGroup.SendGroupMessage(MyString.str["请正确输入数据 格式为："] + "\n" + MyString.str["挑战 伤害 at(@)账号拥有者"]);

                        }
                        else if (int.TryParse(msgA[1], out int damage))
                        {
                            if (damage > bossDate[targetBoss] || damage < 0)
                            {
                                e.FromGroup.SendGroupMessage(MyString.str["请正确输入伤害！"]);
                            }
                            else
                            {
                                bossDate[targetBoss] -= damage;
                                CheackBossBlood();
                                string temp = MemberName(e, qqid) + MyString.str[" 已完成挑战boss（手动输入）"];
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
                    e.FromGroup.SendGroupMessage(MyString.str["只有管理者才能手动输入挑战数据！"]);
                }
            }
            else if (msgA[0].Equals(MyString.str["读取"]))
            {
                if (isRoot(e.FromQQ.Id))
                {
                    LoadDate(e);
                }
            }
            else if (msgA[0].Equals(MyString.str["保存"]))
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
            else if (msgA[0].Equals(MyString.str["指令详情"]))
            {
                e.FromGroup.SendGroupMessage("boss录入\n", "申请出刀\n", "完成\n", "挂树\n", "强行下树\n", "查树\n", "查看\n", "修正\n", "出刀数据\n");

            }*/
