SetLocal EnableDelayedExpansion
set WorkingDir=%~dp0
set LogFile="!WorkingDir!\log.txt"

rem dir "!WorkingDir!\*.cs" "!WorkingDir!\*.xaml" /b/s>"!WorkingDir!\dir.txt"

Echo [!date!_!time!] > !Logfile!

FOR /f "delims=" %%f IN ('dir /b /s !WorkingDir!\*.cs !WorkingDir!\*.xaml !WorkingDir!\*.json') DO (
	Echo Listing of file "%%f">> !Logfile!
	type "%%f">> !Logfile!
	Echo End of file "%%f">> !Logfile!
)


