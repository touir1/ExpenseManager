@echo off
REM List of container names to start
SET CONTAINERS=nginx expenses_postgres rabbitmq redis

REM Loop over each container
FOR %%C IN (%CONTAINERS%) DO (
    echo Starting container %%C...
    docker start %%C
)

echo All containers started.
pause