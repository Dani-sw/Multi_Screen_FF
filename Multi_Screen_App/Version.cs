using System.Reflection;

[assembly: AssemblyTitle("Multi_Screen_App")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Mach1ne C0de")]
[assembly: AssemblyProduct("Mach1ne C0de")]
[assembly: AssemblyCopyright("Copyright Mach1ne C0de")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("1.0.3.0")]
[assembly: AssemblyFileVersion("1.0.3.0")]

namespace Multi_Screen_App
{
    public static class Version
    {
        static private string _sw_version = "v1.0.3.0";  //Number of version Here!
        static private string _sw_title = "Multi Screen APP ";
        

        static public string sw_version()
        {

            return _sw_version;

        }

        static public string sw_title()
        {
            return _sw_title;

        }
    }
}
