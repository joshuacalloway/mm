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

namespace mm
{
    // This is an extremely simple terminal emulator, used to let both the code and the user interaction for each
    // example look like a console application while actually running within our GUI.
    class Terminal : TextBox
    {
        public new event EventHandler<EventArgs> OnEnter;

        public Terminal()
        {
           // Multiline = true;
           // Background = Color.Black;
           // Foreground = Color.Green;
            IsReadOnly = true;
           
            //BackColor = Color.Black;
           // ForeColor = Color.Green;
            //Font = new Font(FontFamily.GenericMonospace, 12);
           // ScrollBars = ScrollBars.Both;
           // WordWrap = false;
           // ReadOnly = true;
        }

    /*
     * protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                EventHandler<EventArgs> hnd = OnEnter;
                if (hnd != null)
                    hnd(this, EventArgs.Empty);
            }
            else
            {
//                base.OnKeyDown(e);
            }
        }
        */
        public void WriteLine(string st)
        {
            Text += st + "\r\n";
            SelectionStart = Text.Length;
            //ScrollToCaret();
            
        }

    }
}
