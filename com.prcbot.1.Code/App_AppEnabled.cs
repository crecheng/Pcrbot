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
    public class App_AppEnabled : IAppEnable
    {
        public void AppEnable(object sender, CQAppEnableEventArgs e)
        {
            Common.CQApi = e.CQApi;
            Common.CQLog = e.CQLog;
            Common.ae = e;
        }
    }
}
