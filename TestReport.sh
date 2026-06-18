#!/bin/sh
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:TestResults/report -reporttypes:Html
OPEN=$(which xdg-open || which open || which gnome-open)
if [ -z $OPEN ]; then
    echo "Open browser manually and navigage to 'TestResults/report/index.html'"
else
    $OPEN TestResults/report/index.html
fi
