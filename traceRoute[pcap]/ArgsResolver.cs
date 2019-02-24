using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using traceroute_pcap;

namespace traceroute_pcap_ {
    public class ArgsResolver {
        public static ArgsInfo Resolve(string[] args)
        {
            try
            {
                return new ArgsInfo()
                {
                    Destination = NamesResolver.Resolve(args[1]),
                    IsReversedLookupEnabled = args.FirstOrDefault(x => x.ToUpper() == "-ER") != null,
                    RouterIP = NamesResolver.Resolve(args[0]).ToString()
                };

            }
            catch { return null; }
        }
    }
}
