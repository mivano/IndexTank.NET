if not exist Download mkdir Download
if not exist Download\package mkdir Download\package
if not exist Download\package\lib mkdir Download\package\lib
if not exist Download\package\lib\net40 mkdir Download\package\lib\net40

copy LICENSE.txt Download

%systemroot%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe  /t:Build /p:Configuration=Release IndexTank.NET.sln

tools\ilmerge.exe /lib:IndexTank\bin\Release /internalize /ndebug /v2 /out:Download\IndexTank.dll IndexTank.dll Newtonsoft.Json.dll

copy IndexTank\bin\Release\IndexTank.dll Download\Package\lib\net40\

tools\nuget.exe pack IndexTank.Net.nuspec -basepath Download\Package -o Download -Prop Configuration=Release