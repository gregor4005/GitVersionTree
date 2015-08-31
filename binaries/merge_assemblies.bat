@echo off
del *.pdb
"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\NETFX 4.0 Tools\ilmerge" /targetplatform:v4 /out:GitVersionTree.exe GitVersionTree.exe CommandLineParser.dll
del CommandLineParser.dll
@echo on