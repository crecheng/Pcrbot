using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using Native.Sdk.Cqp.Model;
using Native.Sdk.Cqp;

namespace com.pcrbot._1.Code
{
    public class EggPool
    {
        int star3a = 5000;  //25000/5;
        int star2a = 36000; //180000/5;
        int star1a = 159000;//795000/5;
        int star3b = 5000;  //25000/5;
        int star2b = 195000;//975000/5;
        Pool pool = new Pool();
        Dictionary<int, Dictionary<string ,int>> GetResult = new Dictionary<int, Dictionary<string, int>>();
        List<string> star3List = new List<string>();
        Image ImageNull;
        public void ChangePool(int s1c, string s1, int s2c, string s2, int s3c, string s3)
        {
            pool.ChangePool(s1c, s1, s2c, s2, s3c, s3);
        }
        public string GetOneWell(bool sendImg=false)
        {
            GetResult.Clear();
            star3List.Clear();
            GetResult.Add(3, new Dictionary<string, int>());
            GetResult.Add(2, new Dictionary<string, int>());
            GetResult.Add(1, new Dictionary<string, int>());

            string getName;
            int star=1;
            string rus;
            for(int i = 1; i <= 300; i++)
            {
                if (i % 10 == 0)
                {
                    getName = TimesB(out star);
                }
                else
                    getName = TimesA(out star);
                if (star == 3)
                {
                    star3List.Add(getName);
                }
                if (GetResult[star].ContainsKey(getName))
                {
                    GetResult[star][getName]++;
                }
                else
                {
                    GetResult[star].Add(getName, 1);
                }
            }
            rus = GetResultStr(sendImg);
            return rus;
        }

        private string GetResultImg(bool flag=true)
        {

            string s=null;
            if (flag)
            {
                if (star3List.Count == 0)
                    return null;
                else
                {
                    try
                    {
                        lock(star3List)
                        {

                            Image img = new Bitmap(257, 64 + (star3List.Count - 1) / 4 * 70);
                            Graphics g = Graphics.FromImage(img);
                            for (int i = 0; i < star3List.Count; i++)
                            {
                                g.DrawImage(GetImage(star3List[i]), i % 4 * 64, i / 4 * 70, 64, 64);
                            }
                            string tempImgPath = "data/image/pcrbot/temp.png";
                            if (!Directory.Exists("data/image/pcrbot"))
                            {
                                Directory.CreateDirectory("data/image/pcrbot");
                            }
                            if (!File.Exists(tempImgPath))
                            {
                                File.Create(tempImgPath).Close();
                            }
                            if (File.Exists(tempImgPath))
                            {
                                img.Save(tempImgPath);
                                return "pcrbot/temp.png";
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Common.SendLog("绘制扭蛋图片失败！" + ex.Message);
                    }

                }
            }
            else
            {

            }
            return s;
        }
        readonly string imagePath = "data/app/com.prcbot/pic";

        private Image GetImage(string name)
        {
            try
            {
                if (Directory.Exists(imagePath))
                {
                    StringBuilder tempImgPath = new StringBuilder(imagePath + "/" + name);
                    tempImgPath.Append(".png");
                    if (File.Exists(tempImgPath.ToString()))
                    {
                        return Image.FromFile(tempImgPath.ToString());
                    }
                    else
                    {
                        tempImgPath.Clear();
                        tempImgPath.Append(tempImgPath + "/");
                        tempImgPath.Append(name);
                        tempImgPath.Append(".jpg");
                        if (File.Exists(tempImgPath.ToString()))
                        {
                            return Image.FromFile(tempImgPath.ToString());
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                if (Common.CQLog != null)
                {
                    Common.CQLog.Info("获取扭蛋图片失败" + ex.Message);
                }
            }
            return GetNullImg();

        }

        public Image GetNullImg()
        {
            if (ImageNull == null)
            {
                ImageNull = new Bitmap(64, 64);
                Graphics g = Graphics.FromImage(ImageNull);
                Brush brush = new SolidBrush(Color.Pink);
                g.FillRectangle(brush, 0, 0, 64, 64);
                Font f = new Font("楷体", 15f, FontStyle.Bold);
                brush = new SolidBrush(Color.Gray);
                g.DrawString("暂无", f, brush, 5, 5);
                g.DrawString("图片", f, brush, 5, 35); 
            }
            return ImageNull;

        }


        private string GetResultStr(bool sendImg = false)
        {
            string rs;
            int star3Count = 0;
            int star2Count = 0;
            int star1Count = 0;
            int pigStone = 0;
            string star3Name = string.Empty;
            if (GetResult.ContainsKey(1) && GetResult[1] != null)
            {
                foreach (var temp in GetResult[1])
                {
                    star1Count += temp.Value;
                    pigStone+= temp.Value;
                }
            }
            if (GetResult.ContainsKey(2) && GetResult[1] != null)
            {
                foreach (var temp in GetResult[2])
                {
                    star2Count += temp.Value;
                    pigStone += temp.Value*10;
                }
            }
            if (GetResult.ContainsKey(3) && GetResult[3] != null)
            {
                int i = 1;
                foreach (var temp in GetResult[3])
                {
                    star3Count += temp.Value;
                    pigStone += temp.Value*50;
                    star3Name += temp.Key + "×" + temp.Value + (i++%3==0?"\n":"  ");
                }
            }
            string ImgP = string.Empty;
            CQCode imgC = null;
            if (sendImg)
            {
                ImgP = GetResultImg();
                if (ImgP != null)
                {
                    imgC = CQApi.CQCode_Image(ImgP);
                    ImgP = imgC.ToSendString();
                }
                else
                {
                    ImgP = string.Empty;
                }
            }
            return rs = "素敵な仲間が増えますよ！\n" + ImgP+
                MyString.str["恭喜骑士君获得"] +"\n"+
                star3Name+ "\n"+
                "★★★×" + star3Count +"★★×" + star2Count + "★×" + star1Count + "\n" + 
                MyString.str["获得母猪石×"] + pigStone;
        }



        private string TimesA(out int thisTimesStar)
        {
            string s = string.Empty;
            int chance = MyRandom.NextTo(pool.total);
            if (chance < pool.star3c)
            {
                thisTimesStar = 3;
                s = pool.star3[MyRandom.NextTo(pool.star3.Length)];
            }
            else if (chance < pool.star2c+pool.star3c)
            {
                thisTimesStar = 2;
                s = pool.star2[MyRandom.NextTo(pool.star2.Length)];
            }
            else
            {
                thisTimesStar = 1;
                s = pool.star1[MyRandom.NextTo(pool.star1.Length)];
            }
            return s;
        }

        private string TimesB(out int thisTimesStar)
        {
            string s = string.Empty;
            int chance = MyRandom.NextTo(pool.star3c+pool.star2c);
            if (chance < pool.star3c)
            {
                thisTimesStar = 3;
                s = pool.star3[MyRandom.NextTo(pool.star3.Length)];
            }
            else 
            {
                thisTimesStar = 2;
                s = pool.star2[MyRandom.NextTo(pool.star2.Length)];
            }
            return s;
        }

    }
    
}
