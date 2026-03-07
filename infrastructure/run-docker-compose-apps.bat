
@echo off
setlocal enabledelayedexpansion

REM Load .env file and set variables
for /f "usebackq eol=# tokens=1,* delims==" %%A in (".env") do (
    set "%%A=%%B"
)
REM login to local docker registry
<nul set /p "=!DOCKER_REGISTRY_TOKEN!" | docker login localhost:5050 -u "!DOCKER_REGISTRY_USER!" --password-stdin

REM Start the applications
docker-compose -p expense-management-apps -f docker-compose-apps.yml up -d
