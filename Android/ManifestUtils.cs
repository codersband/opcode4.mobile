using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using AndroidXml;
using Ionic.Zip;

namespace opcode4.mobile.Android
{
    public class ManifestUtils
    {
        public Tuple<string, string> GetPackageAndActivityNamesFromFile(byte[] content)
        {
            string activityName = null;

            var doc = GetAppManifestFromFile(content);

            var packageAttr = doc.Element("manifest").Attribute("package");
            if (string.IsNullOrWhiteSpace(packageAttr?.Value))
                throw new Exception("[AndroidPackageHelper.GetPackageAndActivityNamesFromFile]: cannot find packageName from manifest");

            var activities = doc.Descendants().Where(p => p.Name.LocalName.Equals("activity") || p.Name.LocalName.Equals("activity-alias"));
            foreach (var activity in activities)
            {
                var categories = activity.Descendants().Where(p => p.Name.LocalName.Equals("category", StringComparison.InvariantCultureIgnoreCase));
                foreach (var category in categories)
                {
                    var launchAttr = category.Attributes().FirstOrDefault(a => a.Value.Equals("android.intent.category.LAUNCHER", StringComparison.InvariantCultureIgnoreCase));
                    if (launchAttr != null)
                    {
                        var activityAtttr = activity.Attributes().FirstOrDefault(a => a.Name.LocalName.Equals("name", StringComparison.InvariantCultureIgnoreCase));
                        if (activityAtttr != null)
                        {
                            activityName = activityAtttr.Value;
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(activityName))
                    break;
            }

            if (string.IsNullOrWhiteSpace(packageAttr.Value) || string.IsNullOrWhiteSpace(activityName))
                throw new Exception("[AndroidPackageHelper.GetPackageAndActivityNamesFromFile]: cannot find packageName or activityName from manifest");

            return new Tuple<string, string>(packageAttr.Value, activityName);

        }

        public KeyValuePair<int, Version> GetAppVersionFromFile(byte[] content)
        {
            if (content == null || content.Length == 0)
                throw new Exception("[AndroidQuery.GetAppVersionFromFile]: byte content is required");

            var doc = GetAppManifestFromFile(content);
            var attrs = doc.Element("manifest").Attributes().ToList();

            var versionCode = attrs.FirstOrDefault(a => a.Name.ToString().IndexOf("versioncode", StringComparison.InvariantCultureIgnoreCase) != -1);
            var versionName = attrs.FirstOrDefault(a => a.Name.ToString().IndexOf("versionname", StringComparison.InvariantCultureIgnoreCase) != -1);

            if (string.IsNullOrWhiteSpace(versionCode?.Value) || string.IsNullOrWhiteSpace(versionName?.Value))
                throw new Exception("[AndroidQuery.GetAppVersionFromFile]: cannot find versionCode or versionName from manifest");

            versionName.Value = Regex.Replace(versionName.Value, @"[^\.\d]+", "");
            //var delimCount = versionName.Value.Split('.').Length;

            return new KeyValuePair<int, Version>(Convert.ToInt32(versionCode.Value), new Version(versionName.Value));
        }

        private static XDocument GetAppManifestFromFile(byte[] content)
        {
            if (content == null || content.Length == 0)
                throw new Exception("[AndroidPackageHelper.GetAppManifestFromFile]: byte content is required");

            XDocument doc;
            using (var ms = new MemoryStream())
            {
                ms.Write(content, 0, content.Length);
                ms.Seek(0, SeekOrigin.Begin);

                if (!ZipFile.IsZipFile(ms, false))
                    throw new Exception("[AndroidPackageHelper.GetAppManifestFromFile]: invalid zip");

                ms.Seek(0, SeekOrigin.Begin);
                using (var zip = ZipFile.Read(ms))
                {
                    var r = zip.FirstOrDefault(e => e.FileName.EndsWith("AndroidManifest.xml", StringComparison.InvariantCultureIgnoreCase));
                    if (r == null)
                        throw new Exception("[AndroidPackageHelper.GetAppManifestFromFile]: AndroidManifest.xml was not found");

                    using (var ms2 = new MemoryStream())
                    {
                        r.Extract(ms2);
                        ms2.Seek(0, SeekOrigin.Begin);

                        var reader = new AndroidXmlReader(ms2);
                        doc = XDocument.Load(reader);
                    }
                }
            }

            if (doc == null)
                throw new Exception("[AndroidPackageHelper.GetAppManifestFromFile]: unable to load manifest from source");

            return doc;
        }
    }
}
