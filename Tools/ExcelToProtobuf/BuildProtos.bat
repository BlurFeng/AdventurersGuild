@echo off

:: 文件路径
set ProtocPath=%cd%\protoc-3.11.2-win64\bin\protoc.exe
set ProtoSourcePath=%cd%\Protos
set CsharpOutputPath=%cd%\Csharp

:: 删除旧Csharp文件
del /f /q %CsharpOutputPath%\*.*
echo delete all outdated .cs file!!!
echo.

:: 生成新Csharp文件
for /r %%i in (*.proto) do (
echo %%~nxi export to .cs complete.
%ProtocPath% -I=%ProtoSourcePath% --csharp_out=%CsharpOutputPath% %%i
)

echo.
echo all .proto export to .cs complete!!!
echo.