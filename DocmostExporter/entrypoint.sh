#!/bin/bash
dotnet DocmostExporter.dll
cd generated/
mkdocs serve -a 0.0.0.0:8000