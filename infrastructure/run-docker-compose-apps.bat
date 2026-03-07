@echo off

REM Load .env file and set variables
for /f "usebackq tokens=1,2 delims==" %%A in (".env") do (
    set "line=%%A"
    if not "!line!"=="" if not "!line:~0,1!"=="#" set "%%A=%%B"
)
REM login to local docker registry
echo %DOCKER_REGISTRY_TOKEN% | docker login localhost:5050 -u %DOCKER_REGISTRY_USER% --password-stdin

docker-compose -p expense-management-apps -f docker-compose-apps.yml up -d
