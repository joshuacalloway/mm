// © Townsend Analytics, Ltd. 2009 All rights reserved.
// 
// Use of the Townsend API, including, but not limited to, all code and 
// documentation, is subject to the Townsend API Terms and Conditions which are 
// posted on www.realtick.com.  The code contained herein is deemed to be part of 
// the "Licensed Product" as referred to in Townsend Analytics end user agreements 
// between the user and Townsend Analytics.
// 
using System;
using System.Windows.Controls;
using System.Drawing;
using RealTick.Api.Application;
using RealTick.Api.ClientAdapter;
using RealTick.Api.Domain.Livequote;
using RealTick.Api.Domain.Order;
using RealTick.Api.Domain.Ticks;
using RealTick.Api.Domain;
using RealTick.Api.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows;
using System;
using RealTick.Api.Domain.Regional;
using RealTick.Api.Data;

using mm;

namespace mmgui
{
    // This is an extremely simple terminal emulator, used to let both the code and the user interaction for each
    // example look like a console application while actually running within our GUI.
    class Terminal : System.Windows.Controls.TextBox
    {
        public new event EventHandler<EventArgs> OnEnter;
	//public event EventHandler<DataEventArgs<StringEvent>> WriteLineListener;

        public void Clear()
        {
            if (Dispatcher.CheckAccess())
            {
                base.Clear();
            }
            else
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new ClearDeleg(this.Clear));

            }
        }

	
    protected void WriteNow(string msg) {
        if (Dispatcher.CheckAccess())
        {
            Text += msg;
            MessageAppEx.LogSev(Severity.Info, msg);
        }
        else
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new TextChanger(this.WriteNow), msg);
        }
    }

    private delegate void ClearDeleg();
    private delegate void TextChanger(string msg);
    int numlines = 0;

    public void Write(string st)
    {
      WriteNow( st );
    }

    public void WriteLine(string st)
    {
      WriteNow( st + "\r\n");
    }

    public void OnWriteLine(object sender, DataEventArgs<StringEvent> e)
    {
      Console.WriteLine("OnWriteLine");
      if (numlines > 21)
      {
          Clear();
          numlines = 1;
      }
      foreach (StringEvent data in e)
      {
          numlines += 1;
          WriteNow(data.Msg);
      }

    }

        public Terminal()
        {
	//  WriteLineListener += new EventHandler<DataEventArgs<StringEvent>>(OnWriteLine);
           // Multiline = true;
           // Background = Color.Black;
           // Foreground = Color.Green;
            IsReadOnly = true;
            
            VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
           
            //BackColor = Color.Black;
           // ForeColor = Color.Green;
         //Font = 
           // ScrollBars = ScrollBars.Both;
           // WordWrap = false;
           // ReadOnly = true;
        }
    }
}
