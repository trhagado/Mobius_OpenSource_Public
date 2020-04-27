ExternalLibraries Project
-------------------------

ExternalLibraries consists of a set of external dlls that are not in the .Net library and are not built from Mobius source. 
These dlls should be copied to the startup project bin/debug directory (i.e  In Client or NativeSession) for each solution they are needed in.
The easiest way to do this is to add the following command to the post-build event command line for the startup project for the solution.
  xcopy C:\Mobius_OpenSource\Mobius-3.0\ExternalLibraries\*.dll $(TargetDir) /d/c/r/y

Any project references to members of ExternalLibraries should have Copy Local = False to avoid unnecessary copying that slows builds.

ExternalLibraries should be configured to not build in the solution configuration.

