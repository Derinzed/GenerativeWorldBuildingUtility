using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly
[assembly: AssemblyTitle("WorldBuildingGenerator")]
[assembly: AssemblyDescription("An example application")]
[assembly: AssemblyCompany("Roguish Cartographer")]
[assembly: AssemblyProduct("World Building Generator")]

#if RELEASE || DEBUG || LOCALSERVERDEBUG || ADMINOVERRIDE
// Define the assembly version
[assembly: AssemblyVersion("1.1.0.1")]  // Major.Minor.Build.Revision
[assembly: AssemblyFileVersion("1.1.0.1")]
#endif

#if RELEASEFULLSUBSCRIPTION
[assembly: AssemblyVersion("1.1.0.2")]  // Major.Minor.Build.Revision
[assembly: AssemblyFileVersion("1.1.0.2")]
#endif

#if RELEASELIMITEDSUBSCRIPTION
[assembly: AssemblyVersion("1.1.0.3")]  // Major.Minor.Build.Revision
[assembly: AssemblyFileVersion("1.1.0.3")]
#endif