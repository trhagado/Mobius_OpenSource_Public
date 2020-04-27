Readme.txt for the SpotfireDocumentClasses project.

The files in the SpotfireDocumentClasses project are integrated into both the Mobius Client and 
into the SpotfireApiServerMx dll (MobiusSpotfireApiServer.dll). 

For dev/debug purposes SpotfireDocumentClasses can remain as a separate project;
however, when deploying SpotfireApiServerMx for use in the Webplayer a single .dll must be built. 
This is done by synching the .cs project files with the files in the separate SpotfireApiServerMx 
SpotfireDocumentClasses folder. The SyncSpotfireDocMsxClasses.bat script uses XCopy to synch the two folders.

The SpotfireApiServerMx solution must be modified to include either the SpotfireDocumentClasses 
project or folder as is appropriate.
