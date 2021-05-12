set serviceName=ServiceName
set serviceFilePath=E:\Work\Code\WindowsServiceDemo\WorkerService\bin\Debug\netcoreapp3.0\WorkerService.exe
set serviceDescription=服务描述

sc create %serviceName%  BinPath=%serviceFilePath%
sc config %serviceName%    start=auto  
sc description %serviceName%  %serviceDescription%
sc start  %serviceName%
pause