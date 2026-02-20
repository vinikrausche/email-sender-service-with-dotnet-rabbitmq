#!/usr/bin/env bash
set -euo pipefail

dotnet new xunit -n EmailSender.Tests --framework net10.0 --no-restore
dotnet sln EmailSender.sln add EmailSender.Tests/EmailSender.Tests.csproj
dotnet add EmailSender.Tests/EmailSender.Tests.csproj reference EmailSender.csproj
dotnet test EmailSender.sln
