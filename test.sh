#!/usr/bin/env bash
set -euo pipefail

if [ ! -d "EmailSender.Tests" ]; then
  dotnet new xunit -n EmailSender.Tests --framework net10.0 --no-restore
  dotnet sln EmailSender.sln add EmailSender.Tests/EmailSender.Tests.csproj
  dotnet add EmailSender.Tests/EmailSender.Tests.csproj reference EmailSender.csproj
fi

dotnet test EmailSender.sln
