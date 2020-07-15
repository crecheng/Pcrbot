using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.pcrbot._1.Code
{
    public static class MyString
    {
        public static Dictionary<string, string> str=new Dictionary<string, string>();
        public static void ResetStr()
        {
            if (str == null)
                str = new Dictionary<string, string>();
            str.Clear();
            str.Add("boss录入", "boss录入");
            str.Add("申请出刀", "申请出刀");
            str.Add("完成", "完成");
            str.Add("挂树", "挂树");
            str.Add("查树", "查树");
            str.Add("查看", "查看");
            str.Add("强行下树", "强行下树");
            str.Add("修正", "修正");
            str.Add("出刀数据", "出刀数据");
            str.Add("清除数据", "清除数据");
            str.Add("解除出刀", "解除出刀");
            str.Add("砍树", "砍树");
            str.Add("代打", "代打");
            str.Add("挑战", "挑战");
            str.Add("读取", "读取");
            str.Add("保存", "保存");
            str.Add("指令详情", "指令详情");
            str.Add("正在攻击boss，请稍等！", "正在攻击boss，请稍等！");
            str.Add(" 已挂树，无法出刀", " 已挂树，无法出刀");
            str.Add("  已开始挑战boss", "  已开始挑战boss");
            str.Add("正在挑战boss", "正在挑战boss");
            str.Add("同时挑战人数达到设置上限！", "同时挑战人数达到设置上限！");
            str.Add("请在“完成”后空格后输入你的伤害或输入“击杀”", "请在“完成”后空格后输入你的伤害或输入“击杀”");
            str.Add("击杀", "击杀");
            str.Add(" 已完成挑战boss", " 已完成挑战boss");
            str.Add("你已跨boss结算，当前数据不记录!", "你已跨boss结算，当前数据不记录!");
            str.Add("请正确输入伤害，如果已击杀请输入“击杀”", "请正确输入伤害，如果已击杀请输入“击杀”");
            str.Add(" 已挂树", " 已挂树");
            str.Add("挂树成员：", "挂树成员：");
            str.Add("请在“强行下树”后空格后输入你的伤害或击杀", "请在“强行下树”后空格后输入你的伤害或击杀");
            str.Add("已强行下树", "已强行下树");
            str.Add("你不是管理者账户，无法进行此操作！", "你不是管理者账户，无法进行此操作！");
            str.Add("今天", "今天");
            str.Add("暂无你的出刀数据!", "暂无你的出刀数据!");
            str.Add("解除出刀成功！", "解除出刀成功！");
            str.Add("砍树成功！", "砍树成功！");
            str.Add("请正确输入代打数据 格式为：", "请正确输入代打数据 格式为：");
            str.Add("代打 伤害(或击杀) at(@)代打账号拥有者", "代打 伤害(或击杀) at(@)代打账号拥有者");
            str.Add(" 已完成挑战boss（代打）", " 已完成挑战boss（代打）");
            str.Add("只有管理者才能输入代打数据！", "只有管理者才能输入代打数据！");
            str.Add("请正确输入数据 格式为：", "请正确输入数据 格式为：");
            str.Add("挑战 伤害 at(@)账号拥有者", "挑战 伤害 at(@)账号拥有者");
            str.Add("请正确输入伤害！", "请正确输入伤害！");
            str.Add(" 已完成挑战boss（手动输入）", " 已完成挑战boss（手动输入）");
            str.Add("只有管理者才能手动输入挑战数据！", "只有管理者才能手动输入挑战数据！");
            str.Add("全员入会", "全员入会");
            str.Add("成功加入", "成功加入");
            str.Add("名成员", "名成员");
            str.Add("你已经在公会了", "你已经在公会了");
            str.Add("成功加入公会！", "成功加入公会！");
            str.Add("该成员已经在公会了", "该成员已经在公会了");
            str.Add("入会", "入会");
            str.Add("退会", "退会");
            str.Add("你退出了公会了", "你退出了公会了");
            str.Add("你还不在公会！", "你还不在公会！");
            str.Add("该成员踢出了公会了", "该成员踢出了公会了");
            str.Add("该成员还不在公会！", "该成员还不在公会！");
            str.Add("催刀", "催刀");
            str.Add("管理催刀了！", "管理催刀了！");
            str.Add("请以上成员立即出刀!", "请以上成员立即出刀!");
            str.Add("删刀", "删刀");
            str.Add("删刀成功！", "删刀成功！");
            str.Add("恭喜骑士君获得", "恭喜骑士君获得");
            str.Add("获得母猪石×", "获得母猪石×");
            str.Add("抽一井", "抽一井");
            str.Add("次数不足，明天再来！", "次数不足，明天再来！");
            
        }
    }
}
