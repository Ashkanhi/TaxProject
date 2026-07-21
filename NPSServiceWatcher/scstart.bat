sc stop KPPUnitService
taskkill /F /IM KPPUnitService.exe
timeout 3
sc start KPPUnitService