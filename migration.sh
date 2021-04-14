#!/bin/bash

dotnet ef migrations add $1 && dotnet ef database update

echo ''

echo 'Finish script :)'
