using Security;
using System.Text;

namespace Evacuation.Models
{
    public static class Password
    {
        public static string value = "";

        public static void Load()
        {
			byte[] pass = null;
			if (SecKeyChain.FindGenericPassword("XenReporter", "root", out pass) == SecStatusCode.Success)
			{
				value = Encoding.UTF8.GetString(pass);
			}
			else
			{
                value = "";
            }
        }

        public static void Save(this string Pass)
        {
			if (Pass == "")
			{
				SecKeyChain.Remove(
					new SecRecord(SecKind.GenericPassword)
					{
						Service = "XenReporter",
						Account = "root"
					});
			}
			else
			{
				SecKeyChain.AddGenericPassword("XenReporter", "root", Encoding.UTF8.GetBytes(Pass));
			}
			Load();
        }
    }
}
