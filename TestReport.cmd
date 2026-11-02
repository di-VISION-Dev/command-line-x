@echo off
set REPORTTOOL=
dotnet tool list -g dotnet-reportgenerator-globaltool
if %errorlevel% equ 0 (
	SET REPORTTOOL=reportgenerator
)
if "" == "%REPORTTOOL%" (
	dotnet tool restore
	SET REPORTTOOL=dotnet reportgenerator
)
dotnet test --collect:"XPlat Code Coverage"
%REPORTTOOL% -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:TestResults/report -reporttypes:Html
start TestResults\report\index.html
