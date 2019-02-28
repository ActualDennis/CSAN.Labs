using System.Linq;

namespace traceroute_pcap {
    public class ArgsResolver {
        public static ArgsInfo Resolve(string[] args)
        {
            try
            {
                return new ArgsInfo()
                {
                    Destination = NamesResolver.Resolve(args[0]),
                    IsReversedLookupEnabled = args.FirstOrDefault(x => x.ToUpper() == "-ER") != null,
                    HopsCount = int.Parse(args[1]) 
                };

            }
            catch { return null; }
        }
    }
}
