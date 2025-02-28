#!/bin/bash
dotnet DocmostExporter.dll
cd generated/
mkdocs serve