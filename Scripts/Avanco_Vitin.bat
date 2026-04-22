@echo off

echo Avancimi i Vitit filloi ne %date% %time% >> C:\Scripts\Avanco_Vitin.log

sqlcmd -S localhost\SQLEXPRESS -d Projekti -Q "EXEC dbo.AvancoVitin;" >> C:\Scripts\Avanco_Vitin.log 2>&1

if %errorlevel% equ 0 (
    echo Sukses! >> C:\Scripts\Avanco_Vitin.log
) else (
    echo GABIM (errorlevel %errorlevel%) >> C:\Scripts\Avanco_Vitin.log
)

echo Skripti u mbyll ne %date% %time% >> C:\Scripts\Avanco_Vitin.log
echo. >> C:\Scripts\Avanco_Vitin.log