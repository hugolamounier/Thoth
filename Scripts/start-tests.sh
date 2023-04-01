#!/bin/bash
dotnet test --configuration Release --logger trx --settings "./runsettings.xml"