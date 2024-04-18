#!/bin/bash

docker-compose up -d

sleep 30 # to give time to docker to finish the setup
echo setup vault configuration
sh ../vault/init/config.sh
echo removing sql server configuration
docker rm sqlserver-configurator
echo completed
