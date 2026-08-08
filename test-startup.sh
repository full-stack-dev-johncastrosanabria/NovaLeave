#!/bin/bash
cd "/Users/johnbenjamincastrosanabria/Desktop/repos/NovaLeave"
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS="https://localhost:5443/"
dotnet run --project src/NovaLeave.Web/NovaLeave.Web.csproj --no-build 2>&1 | head -100
