#!/bin/bash

docker exec -it vault vault kv put secret/tasksDbConnectionString server=localhost port=1433 database=TasksDb userId=sa password=Sql_S3rv3r encrypt=false