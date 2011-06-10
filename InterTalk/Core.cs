using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InterTalk
{
    public sealed class Core
    { 
        public delegate void CoreEventHandler(object sender, CoreEventArgs e);
        private Dictionary<String, CoreEventHandler>[] Conditions;
    }
}
