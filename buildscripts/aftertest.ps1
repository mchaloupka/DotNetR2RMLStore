$env:PATH = "C:\msys64\usr\bin;" + $env:PATH
Invoke-WebRequest -Uri "https://codecov.io/bash" -OutFile codecov.sh
bash codecov.sh -f "coverage.xml" -t $env:CODECOV_TOKEN