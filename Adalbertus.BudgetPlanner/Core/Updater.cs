using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Adalbertus.BudgetPlanner.Extensions;
using System.Diagnostics;
using System.IO;

namespace Adalbertus.BudgetPlanner.Core
{
    public static class Updater
    {
        public static string CurrentVersion { get { return GetCurrentVersion(); } }

        public static string GetCurrentVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
        }

        public static int GetNumericVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                return 0;
            }
            int numericVersion = 0;
            if (!Int32.TryParse(version.Replace(".", "").Replace(" beta", ""), out numericVersion))
            {
                return 0;
            }
            return numericVersion;
        }

        private static UpdateInfo _currentUpdateInfo;
        public static UpdateInfo UpdateInfo { get { return _currentUpdateInfo; } }

        public static bool CheckForNewVersion(string url)
        {
            if (url.IsNullOrWhiteSpace())
            {
                return false;
            }
            try
            {
                using (var xmlReader = XmlTextReader.Create(url))
                {
                    var xml = XDocument.Load(xmlReader);
                    _currentUpdateInfo = Parse(xml);
                    if (_currentUpdateInfo == null)
                    {
                        return false;
                    }
                    return GetNumericVersion(_currentUpdateInfo.Version) > GetNumericVersion(CurrentVersion);
                }
            }
            catch
            {
                return false;
            }
        }

        private static UpdateInfo Parse(XDocument xml)
        {
            if (xml == null)
            {
                return null;
            }

            var itemElement = xml.Descendants("Item").FirstOrDefault();
            if (itemElement == null)
            {
                return null;
            }

            var versionElement = itemElement.Element("Version");
            var dateElement = itemElement.Element("Date");
            var changesElement = itemElement.Element("Changes");
            var urlElement = itemElement.Element("Url");
            if (versionElement == null || dateElement == null || changesElement == null || urlElement == null)
            {
                return null;
            }
            if (versionElement.Value.IsNullOrWhiteSpace() || urlElement.Value.IsNullOrWhiteSpace())
            {
                return null;
            }

            DateTime date;
            if (!DateTime.TryParse(dateElement.Value, out date))
            {
                return null;
            }

            return new UpdateInfo
            {
                Version = versionElement.Value,
                Changes = changesElement.Value,
                Date = date,
                Url = urlElement.Value,
                ExeFile = string.Empty,
            };
        }

        public static void RunUpdateAndExit()
        {
            if (_currentUpdateInfo == null)
            {
                return;
            }
            if (_currentUpdateInfo.ExeFile.IsNullOrWhiteSpace())
            {
                return;
            }
            if (GetNumericVersion(_currentUpdateInfo.Version) > GetNumericVersion(CurrentVersion))
            {
                if (File.Exists(_currentUpdateInfo.ExeFile))
                {
                    Process.Start(_currentUpdateInfo.ExeFile);
                    Environment.Exit(0);
                }
            }
        }
    }
}
