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
using Native.Sdk.Cqp.Enum;

namespace com.pcrbot._1.Code
{
    public class Event_GroupMsg:IGroupMessage
    {
        static Dictionary<long,Pcrbot> pcrbot = new Dictionary<long, Pcrbot>();

        public void GroupMessage(object sender,CQGroupMessageEventArgs e)
        {
            var group = e.FromGroup;
            var groupid = group.Id;
            if (pcrbot.ContainsKey(groupid))
            {
                lock (pcrbot[groupid])
                {
                    pcrbot[groupid].PrcbotMsg(sender, e);
                }
            }
            else
            {
                Pcrbot tempP = null;
                try
                {
                    tempP = new Pcrbot(groupid,group.GetGroupInfo().Name);
                }
                catch (Exception ex)
                {
                    e.CQLog.Warning("创建Pcrbot对象失败"+ex.Message);
                }
                if (tempP != null)
                {
                    pcrbot.Add(groupid, tempP);
                    pcrbot[groupid].PrcbotMsg(sender, e);
                }
            }

        }

        public static Dictionary<long, Pcrbot> GetPrcbot()
        {
            return pcrbot;
        }
    }
}
