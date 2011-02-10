P4.NET
======

P4.NET is a fork of the [original P4.NET component created by Shawn Hladky](http://p4dotnet.sourceforge.net/index.php/P4.Net_Overview). It provides managed implementation of Perforce API. Main features of this fork (as opposed to Shawn's original):

- Support for CLR 2 (.NET 2.0, 3.0, 3.5)
- Support for CLR 4 (.NET 4.0)
- Single AnyCPU assembly (works in both 32 and 64 bit .NET runtimes)


Overview
--------

Based on: [http://public.perforce.com:8080/guest/shawn_hladky/P4.Net/main/](http://public.perforce.com:8080/guest/shawn_hladky/P4.Net/main/)

Current version: **2.0.0.2** (Feb 9, 2011; [sources](https://github.com/milang/P4.net/zipball/v2.0.0.2); [binaries](https://github.com/downloads/milang/P4.net/P4.NET_2.0.0.2.zip))

Build requirements: **Visual C++ 2010** (free edition should have no problems)


How to build
------------

1) Download sources from GitHub or clone the repository using Git

2) Run/double-click `(project_root)/p4.net_build.cmd`. This will produce the following binaries:

  - `bin/Debug_v2.0/P4API.dll (pdb, xml)` - signed P4.NET assembly in debug mode, compiled for .NET 2.0 (i.e. usable from .NET 2.0, 3.0 and 3.5)
  - `bin/Release_v2.0/P4API.dll (pdb, xml)` - signed P4.NET assembly in release mode, compiled for .NET 2.0 (i.e. usable from .NET 2.0, 3.0 and 3.5)
  - `bin/Debug_v4.0/P4API.dll (pdb, xml)` - signed P4.NET assembly in debug mode, compiled for .NET 4.0
  - `bin/Release_v4.0/P4API.dll (pdb, xml)` - signed P4.NET assembly in release mode, compiled for .NET 4.0
