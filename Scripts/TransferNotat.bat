@echo off
echo Transferimi filloi ne %date% %time% >> C:\Scripts\transfer_log.txt
sqlcmd -S localhost\SQLEXPRESS -d Projekti -Q "EXEC dbo.TransferoNotatNeFinale;" >> C:\Scripts\transfer_log.txt 2>&1
if %errorlevel% equ 0 (
    echo Sukses! >> C:\Scripts\transfer_log.txt
) else (
    echo GABIM (errorlevel %errorlevel%) >> C:\Scripts\transfer_log.txt
)