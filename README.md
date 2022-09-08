# WLinEd
W's Line Editor

WLinEd is a simple line editor - a console application writen in order to learn C#.
It is intended to allow editing lines of text in a text file (It is a try to write something similar to other line editors like edlin or ed).

Usage: 
wlined.exe <filename> [< makro.file][ > PRN|/dev/lpt]

A makro file contains commands as you type them.
If commands needs additional string, type it in the next line

To see all commands, just type <h><enter> or run wlined.exe without params.

Makefile uses mcs compiler from Mono, provided with WSL-1 for Debian distribution but any newer C# compiler should be enough to compile the one-file source.

Examples directory contains makro.

  

 


 
 
  
  

