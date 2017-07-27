using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Net;
using Renci.SshNet;

namespace Evacuation
{
	[XmlRoot(ElementName = "patch")]
	public class Patch
	{
		public static List<Patch> LoadPatches(Hypervisor hyp)
		{
            string ServerVersion = hyp.XenVersion;
			var allPatches =
				(Patchdata)(new XmlSerializer(typeof(Patchdata)))
					.Deserialize(new MemoryStream(
						Encoding.UTF8.GetBytes(
							new WebClient()
								 .DownloadString("http://updates.xensource.com/XenServer/updates.xml"))));
			List<Patch> minimalPatches = new List<Patch>();
            var hypPatches = hyp.Patches.Select(x => x.name_label.ToLower()).ToList();
			foreach (var patch in allPatches.Serverversions.Version.First(x => x.Value == ServerVersion).Minimalpatches.Patch)
			{
                var curPatch = allPatches.Patches.Patch.First(y => y.Uuid == patch.Uuid);
                if (!hypPatches.Contains(curPatch.Namelabel.ToLower())) minimalPatches.Add(curPatch);
			}
            return minimalPatches.OrderBy(y => y.Namelabel).ToList();
		}

        public static void ApplyPatch(Patch patch, Hypervisor hyp)
        {
            var connInfo = new ConnectionInfo(hyp.HostName, 22, "root", new AuthenticationMethod[] { new PasswordAuthenticationMethod("root", Models.Password.ToString()) });
            var patchPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Documents/XenPatches/";
            var fileName = patch.Patchurl.Split('&').First().Split('/').Last();
            // Check if cache path exists
            if (!Directory.Exists(patchPath)) Directory.CreateDirectory(patchPath);
            // Check if download exists
            if (!File.Exists(patchPath + fileName))
            {
                // Download
                new WebClient().DownloadFile(patch.Patchurl, patchPath + fileName);
			}
            // SCP to Hypervisor
            var sftp = new SftpClient(connInfo);
            sftp.Connect();
            sftp.ChangeDirectory("/tmp/");
            using (var upFS = File.OpenRead(patchPath + fileName)){
                sftp.UploadFile(upFS, fileName, true);
            }
            sftp.Disconnect();

			// Apply Patch
            using (var ssh = new SshClient(connInfo)){
                var nameLabel = fileName.Split('.').First();
                ssh.Connect();
                var extract = ssh.CreateCommand("cd /tmp; unzip /tmp/" + fileName);
				extract.Execute();
                var upload = ssh.CreateCommand("xe patch-upload file-name=/tmp/" + nameLabel + ".iso");
                upload.Execute();
                hyp.LoadPatchInfo();
                var patchToApplyUuid = hyp.Patches.Where(x => x.name_label == nameLabel).First().uuid;
                var apply = ssh.CreateCommand("xe patch-apply uuid=" + patchToApplyUuid + " host-uuid=" + hyp.Uuid);
                apply.Execute();
            }
		}

		[XmlAttribute(AttributeName = "after-apply-guidance")]
		public string Afterapplyguidance { get; set; }
		[XmlAttribute(AttributeName = "name-description")]
		public string Namedescription { get; set; }
		[XmlAttribute(AttributeName = "name-label")]
		public string Namelabel { get; set; }
		[XmlAttribute(AttributeName = "patch-url")]
		public string Patchurl { get; set; }
		[XmlAttribute(AttributeName = "releasenotes")]
		public string Releasenotes { get; set; }
		[XmlAttribute(AttributeName = "timestamp")]
		public string Timestamp { get; set; }
		[XmlAttribute(AttributeName = "url")]
		public string Url { get; set; }
		[XmlAttribute(AttributeName = "uuid")]
		public string Uuid { get; set; }
		[XmlAttribute(AttributeName = "version")]
		public string Version { get; set; }
		[XmlElement(ElementName = "conflictingpatches")]
		public Conflictingpatches Conflictingpatches { get; set; }
		[XmlElement(ElementName = "requiredpatches")]
		public Requiredpatches Requiredpatches { get; set; }
		[XmlAttribute(AttributeName = "installation-size")]
		public string Installationsize { get; set; }
		[XmlAttribute(AttributeName = "guidance-mandatory")]
		public string Guidancemandatory { get; set; }
		[XmlAttribute(AttributeName = "contains-livepatch")]
		public string Containslivepatch { get; set; }
		[XmlAttribute(AttributeName = "download-size")]
		public string Downloadsize { get; set; }
		[XmlAttribute(AttributeName = "livepatch-component")]
		public string Livepatchcomponent { get; set; }
		[XmlAttribute(AttributeName = "patch-download-contains-source")]
		public string Patchdownloadcontainssource { get; set; }
		[XmlAttribute(AttributeName = "update-type")]
		public string Updatetype { get; set; }
		[XmlAttribute(AttributeName = "priority")]
		public string Priority { get; set; }
		[XmlAttribute(AttributeName = "source-url")]
		public string Sourceurl { get; set; }
	}

	[XmlRoot(ElementName = "conflictingpatch")]
	public class Conflictingpatch
	{
		[XmlAttribute(AttributeName = "uuid")]
		public string Uuid { get; set; }
	}

	[XmlRoot(ElementName = "conflictingpatches")]
	public class Conflictingpatches
	{
		[XmlElement(ElementName = "conflictingpatch")]
		public List<Conflictingpatch> Conflictingpatch { get; set; }
	}

	[XmlRoot(ElementName = "requiredpatch")]
	public class Requiredpatch
	{
		[XmlAttribute(AttributeName = "uuid")]
		public string Uuid { get; set; }
	}

	[XmlRoot(ElementName = "requiredpatches")]
	public class Requiredpatches
	{
		[XmlElement(ElementName = "requiredpatch")]
		public Requiredpatch Requiredpatch { get; set; }
	}

	[XmlRoot(ElementName = "patches")]
	public class Patches
	{
		[XmlElement(ElementName = "patch")]
		public List<Patch> Patch { get; set; }
	}

	[XmlRoot(ElementName = "version")]
	public class Version
	{
		[XmlElement(ElementName = "patch")]
		public List<Patch> Patch { get; set; }
		[XmlAttribute(AttributeName = "build-number")]
		public string Buildnumber { get; set; }
		[XmlAttribute(AttributeName = "latest")]
		public string Latest { get; set; }
		[XmlAttribute(AttributeName = "name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName = "timestamp")]
		public string Timestamp { get; set; }
		[XmlAttribute(AttributeName = "url")]
		public string Url { get; set; }
		[XmlAttribute(AttributeName = "value")]
		public string Value { get; set; }
		[XmlElement(ElementName = "minimalpatches")]
		public Minimalpatches Minimalpatches { get; set; }
		[XmlAttribute(AttributeName = "latestcr")]
		public string Latestcr { get; set; }
	}

	[XmlRoot(ElementName = "minimalpatches")]
	public class Minimalpatches
	{
		[XmlElement(ElementName = "patch")]
		public List<Patch> Patch { get; set; }
	}

	[XmlRoot(ElementName = "serverversions")]
	public class Serverversions
	{
		[XmlElement(ElementName = "version")]
		public List<Version> Version { get; set; }
	}

	[XmlRoot(ElementName = "xencenterversions")]
	public class Xencenterversions
	{
		[XmlElement(ElementName = "version")]
		public List<Version> Version { get; set; }
	}

	[XmlRoot(ElementName = "patchdata")]
	public class Patchdata
	{
		[XmlElement(ElementName = "patches")]
		public Patches Patches { get; set; }
		[XmlElement(ElementName = "serverversions")]
		public Serverversions Serverversions { get; set; }
		[XmlElement(ElementName = "xencenterversions")]
		public Xencenterversions Xencenterversions { get; set; }
	}

}
