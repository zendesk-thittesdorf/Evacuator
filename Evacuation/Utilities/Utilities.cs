﻿using System;
using System.Net;
using AppKit;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Utilities
{
	public static class Utilities
	{
		const int KbInGb = 1073741824;
		const long KbInTb = 1099511627776;

		public static long KbToGb(this long Kb)
		{
			return Kb / KbInGb;
		}

		public static double KbToTb(this long Kb)
		{
			return Kb * 100 / KbInTb / 100.0;
		}

		public static string Delimiter(this object[] values, string delimiter)
		{
			return string.Join(delimiter, values.Select(x => string.Format("\"{0}\"", x)).ToList());
		}

		// Check for value in DNS
		public static bool InDns(this string value)
		{
			for (int i = 0; i < 3; i++)
			{
				try
				{
					var addr = Dns.GetHostAddresses(value.ToString());
					if (addr.Length > 0) return true;
				}
				catch (Exception)
				{ }
			}
			return false;
		}

		// Increment the IntValue of a NSTextField by 1
		public static void Inc(this NSTextField lbl)
		{
			lbl.BeginInvokeOnMainThread(() =>
			{
				lbl.IntValue += 1;
			});
		}

		// Set Label for NSTextField to 0
		public static void Zero(this NSTextField lbl)
		{
			lbl.BeginInvokeOnMainThread(() =>
			{
				lbl.IntValue = 0;
			});
		}

		// Set Label for NSTextField to str
		public static void Set(this NSTextField lbl, String str)
		{
			lbl.BeginInvokeOnMainThread(() =>
			{
				lbl.StringValue = str;
			});
		}

		// Add object to list if not empty when trimmed
		public static void AddNonEmpty(this List<String> list, String item)
		{
			if (item.Trim() != "") list.Add(item);
		}

		// Save String to File on the Desktop
		public static void WriteAscii(this string Contents, string FileName)
		{
			Write(FileName, Contents, Encoding.ASCII);
		}

		public static void WriteUnicode(this string Contents, string FileName)
		{
			Write(FileName, Contents, Encoding.Unicode);
		}

		private static void Write(string FileName, string Contents, Encoding enc)
		{
			FileName = string.Format(FileName, DateTime.Now);
			using (StreamWriter outputFile = new StreamWriter(new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/" + FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite), enc))
			{
				outputFile.Write(Contents);
			}
		}

		// Set Hidden for NSButton
		public static void Hide(this NSButton obj, bool hidden = true)
		{
			obj.BeginInvokeOnMainThread(() =>
			{
                obj.Hidden = hidden;
            });
		}

		// Set Hidden for NSProgressIndicator
		public static void Hide(this NSProgressIndicator obj, bool hidden = true)
		{
			obj.BeginInvokeOnMainThread(() =>
			{
				obj.Hidden = hidden;
			});
		}

        // Init Range for NSProgressIndicator
        public static void Range(this NSProgressIndicator obj, int min, int max)
        {
			obj.BeginInvokeOnMainThread(() =>
			{
				obj.MinValue = min;
				obj.MaxValue = max;
			});
        }

		// Set value for NSProgressIndicator
		public static void Set(this NSProgressIndicator obj, double val)
		{
			obj.BeginInvokeOnMainThread(() =>
			{
                obj.DoubleValue = val;
                obj.Display();
			});
		}
	}
}