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
    
    Filter format: {Column Index}={Value-1},{Value-2},..
    e.g. Text Filter: 11=12345,hello,world
    e.g. Starts-With: 11=1234*,hell*
    e.g. Ends-With: 11=*2345,*ello
    e.g. Contains: 11=*234*,*ell*
    
Output Folder: Filtered results folder
