@ECHO OFF

"%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" /nologo /t:GeneratePackages /p:Configuration=Debug

PAUSE