using System;
using System.ComponentModel;
//using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using WixSharp;
using WixSharp.Forms;

namespace WixSharp_SysInternals
{
    class Program
    {
        static void Main()
        {
            // TLS 1.2 at least required by sysinternals.com in 2021
            // Not sure if the .NET Framework we are running on encompasses it, so...
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            // Get Zip File from official site
            string remoteURL = @"https://download.sysinternals.com/files/SysinternalsSuite.zip";
            string zipFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());

            System.Net.WebClient webDownloader = new System.Net.WebClient();
            Console.WriteLine("Downloading File \"{0}\" to \"{1}\" .......\n\n", remoteURL, zipFile);
            webDownloader.DownloadFile(remoteURL, zipFile);
            // Unzip
            string zipDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
            Console.WriteLine("Extracting File \"{0}\" to \"{1}\" .......\n\n", zipFile, zipDir);
              ZipFile.ExtractToDirectory(zipFile, zipDir,true);
            
            // Newest file to create a version number from
            System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(zipDir);
            System.IO.FileInfo newestFile = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
            DateTime dtFile = newestFile.LastWriteTimeUtc;
            String versionStr = dtFile.ToString("yy.MM.dd"); // 0-256 for each version component
            Console.WriteLine($"Version extracted from newest file:{versionStr}");
            Version sysIntVersion = new Version(versionStr);

            // LicenceFile should be .rtf
            string LicFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                                                    System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetRandomFileName())+".rtf");
            string licRtf = @"{\rtf1\ansi\deff0 {\fonttbl {\f0 Times New Roman;}}\f0\fs30 This is not a Microsoft software and it comes with no warranty}";
            System.IO.File.WriteAllText(LicFile, licRtf);

            // and now Repack files as msi
            var project = new Project()
            {
                Name = "SysInternalsSuite",
                Version = sysIntVersion,
                LicenceFile = LicFile,
                // Let's display a licence
                // UI = WUI.WixUI_ProgressOnly,
                OutFileName = $"SysInternals_{sysIntVersion}",
                SourceBaseDir = zipDir,
                ControlPanelInfo = new ProductInfo()
                {
                    Manufacturer = "NotMicrosoft",
                    UrlInfoAbout = "https://github.com/1Dimitri/WixSharpSysInternals"
                },
                Dirs = new[]
                {
                    new Dir(@"%ProgramFiles%\Microsoft Repack\SysInternals",
                     new Files("*.*"))
                },
               
            };

            project.GUID = new Guid("6fe30b47-2577-43ad-9095-1861ba25889b");

            project.BuildMsi();

            // clean-up temporary files
            System.IO.File.Delete(zipFile);
            System.IO.Directory.Delete(zipDir, true);
            System.IO.File.Delete(LicFile);

        }


    }
}