@echo off

set serviceName=TwSynchroService
set serviceFilePath=D:\git\TwSynchro\TwSynchro\TwSynchro\bin\Release\net5.0\publish\TwSynchro.exe
set serviceDescription=���ʻ�������ͬ������

sc create %serviceName%  BinPath=%serviceFilePath%
sc config %serviceName%  start=auto  
sc description %serviceName%  %serviceDescription%
sc start %serviceName%

pause
