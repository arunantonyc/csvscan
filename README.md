# csvscan
Fast simple csv files filtering app.

Usage: csvscan [mode] [operation] [options]

mode:
  -h  Help (default)
  -v Verbose
  -s Silent
  
operation:
  -r Read
  
options:
  -r "Source Folder" "Filter File" "Output Folder"
  
Source Folder: One or more source files in csv format
Filters File: Text file with one column filter per line
    format: {Column Position Index}={Value-1},{Value-2},..
    e.g.: 10=1234,5678
    e.g.: 11=hello,world,qwerty
Output Folder: Filtered results folder
  
