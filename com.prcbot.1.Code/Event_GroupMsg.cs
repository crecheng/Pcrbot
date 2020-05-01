using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Native.Sdk.Cqp.Interface;
using Native.Sdk.Cqp.Model;
using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp;

namespace com.pcrbot._1.Code
{
    public class Event_GroupMsg:IGroupMessage
    {
        static Dictionary<long,Pcrbot> pcrbot = new Dictionary<long, Pcrbot>();

        public void GroupMessage(object sender,CQGroupMessageEventArgs e)
        {
            var group = e.FromGroup;
            var groupid = group.Id;
            if(pcrbot.ContainsKey(groupid))
                pcrbot[groupid].PrcbotMsg(sender, e);
            else
            {
                try
                {
                    Pcrbot tempP = new Pcrbot(groupid,group.GetGroupInfo().Name);
                    pcrbot.Add(groupid, tempP);
                    pcrbot[groupid].PrcbotMsg(sender, e);
                }
                catch (Exception ex)
                {
                    e.CQLog.Warning(ex.Message);
                }
            }
        }

        public static Dictionary<long, Pcrbot> GetPrcbot()
        {
            return pcrbot;
        }
    }
}
