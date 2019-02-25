using HttpProxy.Firewalls;
using System;
using System.Collections.Generic;
using System.Text;

namespace HttpProxy {
    public class HttpFirewall : Firewall {

        public HttpFirewall()
        {
            BlockedHosts = XmlSettingsParser.GetBlockedWebsites();
        }
        public List<string> BlockedHosts { get; private set; }

        public override bool CheckIfBlocked(string hostname)
        {
            foreach(var item in BlockedHosts)
            {
                if (item.GetNormalizedWebsitePath().ToUpper() == hostname.GetNormalizedWebsitePath().ToUpper())
                    return true;
            }

            return false;
        }

        public override void NewRule(string host)
        {
            BlockedHosts.Add(host);
        }
    }
}
