taskkill /f /im valuator.exe
taskkill /f /im rankCalculator.exe
taskkill /f /im eventsLogger.exe

cd ..\nginx
nginx -s stop