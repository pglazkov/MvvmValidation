set script=.\build.msbuild
set msbuild="%PROGRAMFILES(x86)%\MSBuild\14.0\Bin\MSBuild.exe"
set targets=%*
if /i "%targets%" == "" set targets=BuildFull
%msbuild% %script% /target:%targets%
@pause
