using Security;
using System.Text;

namespace Evacuation.Models
{
    public static class DracPassword
    {
		static string value = "";

		static void Load()
		{
			byte[] pass = null;
			value = (SecKeyChain.FindGenericPassword("Drac", "root", out pass) == SecStatusCode.Success) ? value = Encoding.UTF8.GetString(pass) : "";
		}

		public static void Save(this string Pass)
		{
			if (Pass == "")
			{
				SecKeyChain.Remove(
					new SecRecord(SecKind.GenericPassword)
					{
						Service = "Drac",
						Account = "root"
					});
			}
			else
			{
				SecKeyChain.AddGenericPassword("Drac", "root", Encoding.UTF8.GetBytes(Pass));
			}
			Load();
		}

		public new static string ToString()
		{
			Load();
			return value;
		}
	}
}
