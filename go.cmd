set script=.\build.msbuild
set msbuild=%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set targets=%*
if /i "%targets%" == "" set targets=BuildFull
%msbuild% %script% /target:%targets%
@pause
