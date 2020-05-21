using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp.Interface;
using com.pcrbot._1.Code;

namespace com.pcrbot._1.UI
{
    public class Menu:IMenuCall
    {
        private MainWindow _mainWindow = null;

        public void MenuCall(object sender, CQMenuCallEventArgs e)
        {
            if (this._mainWindow == null)
            {
                this._mainWindow = new MainWindow();
                this._mainWindow.Closing += MainWindow_Closing;
                this._mainWindow.Show();	
            }
            else
            {
                this._mainWindow.Activate();	
            }
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this._mainWindow = null;
        }
    }
    public class ModifyDate : IMenuCall
    {
        private ModifyDateWindows modDateW = null;

        public void MenuCall(object sender, CQMenuCallEventArgs e)
        {
            if (this.modDateW == null)
            {
                this.modDateW = new ModifyDateWindows();
                this.modDateW.Closing += MainWindow_Closing;
                this.modDateW.Show();
            }
            else
            {
                this.modDateW.Activate();
            }
            Common.ModifyWindowsOpen = true;
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.modDateW = null;
        }
    }
}

