#!/bin/bash

# osx x64
echo "Publishing OSX x64..."
dotnet publish -c release --no-self-contained /p:PublishSingleFile=true -o published -r osx-x64 src/faas-run.csproj
zip -r -j published/faas-run-osx-x64.zip published/faas-run

# windows x64
echo "Publishing Windows x64..."
dotnet publish -c release --no-self-contained /p:PublishSingleFile=true -o published -r win10-x64 src/faas-run.csproj
zip -r -j published/faas-run-win10-x64.zip published/faas-run.exe

# linux x64
echo "Publishing Linux x64..."
dotnet publish -c release --no-self-contained /p:PublishSingleFile=true -o published -r linux-x64 src/faas-run.csproj
zip -r -j published/faas-run-linux-x64.zip published/faas-run
