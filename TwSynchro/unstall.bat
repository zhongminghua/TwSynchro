set serviceName=ServiceName

sc stop   %serviceName% 
sc delete %serviceName% 

pause