#!/bin/bash
cd "C:\Users\AbrahamVillalobosUga\Desktop\AI-NOVA\NovaLeave"
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS="http://localhost:51293"
dotnet run --project src/NovaLeave.Web/NovaLeave.Web.csproj --no-build 2>&1 | head -100
