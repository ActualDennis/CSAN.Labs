using System;
using System.Collections.Generic;
using System.Text;

namespace HttpProxy.Firewalls {
    public abstract class Firewall {
        public abstract bool CheckIfBlocked(string hostname);
        public abstract void NewRule(string host);
    }
}
