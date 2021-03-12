set script=.\build.msbuild
set msbuild="%PROGRAMFILES(x86)%\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
set targets=%*
if /i "%targets%" == "" set targets=BuildFull
%msbuild% %script% /target:%targets%
@pause
