using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace HttpProxy {
    public static class XmlSettingsParser {
        public static List<string> GetBlockedWebsites()
        {
            var settingsFile = new XmlDocument();
            settingsFile.Load(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "Settings.xml");

            XmlNodeList blockedWebsites = settingsFile.SelectNodes("/Settings/BlockedWebsites/Website");
            
            var returnList = new List<string>();

            for (int i = 0; i < blockedWebsites.Count; ++i)
            {
                returnList.Add(blockedWebsites[i].FirstChild.Value);
            }

            return returnList;
        }
    }
}
