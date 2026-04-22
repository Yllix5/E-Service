@echo off

echo Avancimi i Semestrit filloi ne %date% %time% >> C:\Scripts\Avanco_Semestrin.log

sqlcmd -S localhost\SQLEXPRESS -d Projekti -Q "EXEC dbo.AvancoSemestrin;" >> C:\Scripts\Avanco_Semestrin.log 2>&1

if %errorlevel% equ 0 (
    echo Sukses! >> C:\Scripts\Avanco_Semestrin.log
) else (
    echo GABIM (errorlevel %errorlevel%) >> C:\Scripts\Avanco_Semestrin.log
)

echo Skripti u mbyll ne %date% %time% >> C:\Scripts\Avanco_Semestrin.log
echo. >> C:\Scripts\Avanco_Semestrin.log