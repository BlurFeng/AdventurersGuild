:: @echo off

:: 文件路径
set ProtoDLLPath=%cd%\bin\Debug\Google.Protobuf.dll
set NetDLLPath=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\netstandard.dll
set CsDLLOutPath=%cd%\Csharp
set csc=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe

:: 删除旧DLL文件
del /f /q %CsDLLOutPath%\ConfigProto.dll
echo delete ConfigProto.dll file!!!
echo.

:: 生成新DLL文件
%csc% /t:library /out:%CsDLLOutPath%\ConfigProto.dll /r:%ProtoDLLPath% /r:%NetDLLPath% %CsDLLOutPath%\*.cs

echo.
echo export ConfigProto.dll complete!!!
echo.