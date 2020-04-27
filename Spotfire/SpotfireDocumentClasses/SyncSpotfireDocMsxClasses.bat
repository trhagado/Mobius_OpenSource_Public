rem - sync .cs files between C:\Mobius\Spotfire\SpotfireDocumentClasses and C:\Mobius\Spotfire\SpotfireApiServerMx\SpotfireDocumentClasses
rem - also synchs the MobiusSpotfireApiServer.dll

xcopy C:\Mobius\Spotfire\SpotfireDocumentClasses\*.cs C:\Mobius\Spotfire\SpotfireApiServerMx\SpotfireDocumentClasses /d /Y

xcopy C:\Mobius\Spotfire\SpotfireApiServerMx\SpotfireDocumentClasses\*.cs C:\Mobius\Spotfire\SpotfireDocumentClasses /d /Y

xcopy C:\Mobius\Spotfire\SpotfireApiServerMx\bin\Debug\MobiusSpotfireApiServer.dll \\<serverFolder>\MobiusSpotfireApiServer.dll /Y

pause
