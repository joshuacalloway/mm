using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RealTick.Api.Application;
using RealTick.Api.ClientAdapter;

using RealTick.Api.Exceptions;


namespace mm
{
    static class OrderManager
    {

       // [STAThread]
        static void runMain()
        {
	    //Application.EnableVisualStyles();
	    //pplication.SetCompatibleTextRenderingDefault(false);

            try {
		
                // Note careful disposal of ToolkitApp here; if this is omitted, the
                // application may hang on shutdown.
                using (ClientAdapterToolkitApp app = new ClientAdapterToolkitApp())
		{
                    //Application.Run(new Form1(app));
                }
            }
            catch (ToolkitPermsException ex)
            {
                //MessageBox.Show(ex.Message, "PERMISSION ERROR");
            }
            catch (Exception ex)
            {
                MessageAppEx.LogSev(Severity.Critical, "Detected top-level exception in RealTickAPIQuickStart");
                MessageAppEx.LogException(ex);
                throw;
            }
	    
        }

    }
}
