set script=.\build.msbuild
set msbuild="%PROGRAMFILES(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
set targets=%*
if /i "%targets%" == "" set targets=BuildFull
%msbuild% %script% /target:%targets%
@pause
