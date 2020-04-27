rem - sync .cs files between C:\Mobius_OpenSource\Spotfire\SpotfireDocumentClasses and C:\Mobius_OpenSource\Spotfire\SpotfireApiServerMx\SpotfireDocumentClasses
rem - also synchs the MobiusSpotfireApiServer.dll

xcopy C:\Mobius_OpenSource\Spotfire\SpotfireDocumentClasses\*.cs C:\Mobius_OpenSource\Spotfire\SpotfireApiServerMx\SpotfireDocumentClasses /d /Y

xcopy C:\Mobius_OpenSource\Spotfire\SpotfireApiServerMx\SpotfireDocumentClasses\*.cs C:\Mobius_OpenSource\Spotfire\SpotfireDocumentClasses /d /Y

xcopy C:\Mobius_OpenSource\Spotfire\SpotfireApiServerMx\bin\Debug\MobiusSpotfireApiServer.dll \\<serverFolder>\MobiusSpotfireApiServer.dll /Y

pause
