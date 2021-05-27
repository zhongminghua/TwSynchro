@echo off
set serviceName=TwSynchroService

sc stop %serviceName% 
sc delete %serviceName% 

pause