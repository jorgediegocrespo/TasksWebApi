#!/bin/bash

docker exec -it vault vault kv put secret/ConnectionStrings DataBaseConnection="Server=localhost,1433;Database=TasksDb;User Id=sa;Password=Sql_S3rv3r;Encrypt=false"
docker exec -it vault vault kv put secret/XApiKey Value=FPwuYPF6uGKEvHMnXUFiRn4d1IWdLMKr03Dv8fMSKIpZYmCKuB
docker exec -it vault vault kv put secret/Jwt Issuer=localhost Audience=localhost Key=O83ol3xQYkpmX4gdlCs4aQtVSlEvmFfri861yToMKocl6eOJac ExpireMinutes=30 RefreshTokenExpireMinutes=120
docker exec -it vault vault kv put secret/Audit Type=AzureTableStorage ConnectionString="DefaultEndpointsProtocol=https;AccountName=tasksauditstorage;AccountKey=hLs7Cff7TJ0C9IxR5O6YYhqP2UXfqi/NEegYYi1OIV5RmUXtlzI++Y/t5WU9j1ImHOpSR7t1K6sA+AStMsrAqA==;EndpointSuffix=core.windows.net" TableName=Events
docker exec -it vault vault kv put secret/SerilogLog Type=AzureTableStorage ConnectionString="DefaultEndpointsProtocol=https;AccountName=tasksauditstorage;AccountKey=hLs7Cff7TJ0C9IxR5O6YYhqP2UXfqi/NEegYYi1OIV5RmUXtlzI++Y/t5WU9j1ImHOpSR7t1K6sA+AStMsrAqA==;EndpointSuffix=core.windows.net" TableName=Serilog