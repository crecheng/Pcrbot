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
    public static class Common
    {
        public static CQApi CQApi { get; set; }
        public static CQLog CQLog { get; set; }
        public static CQAppEnableEventArgs ae { get; set; }
        public static CQGroupMessageEventArgs gm { get; set; }
        public static bool ModifyWindowsOpen { get; set; }
    }
}
