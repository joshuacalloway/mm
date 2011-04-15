using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mm
{
    class StringEvent : RealTick.Api.Domain.ExtensibleRecord
    {
        private string msg;
        public StringEvent() { }
        public StringEvent(string msg) { this.msg = msg; }
        public string Msg
        {
            get { return msg; }
            set { msg = value; }
        }
    }
}
