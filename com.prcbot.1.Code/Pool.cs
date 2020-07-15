using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.pcrbot._1.Code
{
    public class Pool
    {
        public int star3c = 5000;
        public int star2c = 36000;
        public int star1c = 159000;
        public int total ;
        public string[] star3 = { "初音","真琴","姬塔",
            "咲恋","望","璃乃","妮侬",
            "伊绪","秋乃","莫妮卡",
            "静流","杏奈","纯","真步",
            "亚里沙" ,"镜华"};

        public string[] star2 = {"空花","美冬","雪",
            "茜里","珠希","美美",
            "真阳","忍","香织",
            "千歌","深月","惠理子",
            "宫子","栞","铃奈",
            "铃","绫音","美里"};

        public string[] star1 = { "怜", "尤加莉", "碧",
            "依里", "未奏希", "莉玛",
            "铃莓", "美咲", "日和莉",
            "胡桃" };
        public Pool()
        {
            total = star1c + star2c + star3c;
        }
        public Pool(int c1,int c2,int c3, string s1,string s2 ,string s3 )
        {
            star1c = c1;
            star2c = c2;
            star3c = c3;
            star1 = s1.Split(',');
            star2 = s2.Split(',');
            star3 = s3.Split(',');
            total = star1c + star2c + star3c;
        }
        public void ChangePool(int s1c, string s1, int s2c, string s2, int s3c, string s3)
        {
            star1c = s1c;
            star2c = s2c;
            star3c = s3c;
            star1 = s1.Split(',');
            star2 = s2.Split(',');
            star3 = s3.Split(',');
            total = star1c + star2c + star3c;
        }
    }
}
