using System.Reflection;
using System.Runtime.InteropServices;
using MediaPortal.Common.Utils;

// Version Compatibility
// http://wiki.team-mediaportal.com/1_MEDIAPORTAL_1/18_Contribute/6_Plugins/Plugin_Related_Changes/1.6.0_to_1.7.0
[assembly: CompatibleVersion("1.7.0.0", "1.7.0.0")]

[assembly: UsesSubsystem("MP.SkinEngine")]
[assembly: UsesSubsystem("MP.Players.Music")]
[assembly: UsesSubsystem("MP.DB.Music")]
[assembly: UsesSubsystem("MP.Config")]

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("My Lyrics")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("My Lyrics")]
[assembly: AssemblyCopyright("Copyright © 2009-2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("42554aa9-3a57-4326-b4bc-a7d1069c4864")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("2.2.0.$WCREV$")]
[assembly: AssemblyFileVersion("2.2.0.$WCREV$")]