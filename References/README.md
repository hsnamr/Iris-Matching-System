# Required reference assemblies

Place the following DLLs in this folder to build the project:

- **AForge.dll**
- **AForge.Imaging.dll**
- **AForge.Math.dll**
- **DockingToolbar.dll**
- **SourceGrid2.dll**
- **SourceLibrary.dll**
- **WeifenLuo.WinFormsUI.dll**

If your solution includes custom controls **ColorSlider** and **FilterPreview**, add them under a `Controls` folder (e.g. `Controls\ColorSlider.cs`, `Controls\FilterPreview.cs`) so the Canny detector form can reference them.

This project targets **.NET 10** and uses **Microsoft.Data.SqlClient** (NuGet) for SQL Server; the rest of the dependencies are these local references.

If you have the original .NET Framework build, copy the DLLs from its `References` or `bin` folder. For .NET 10, use versions that support .NET Standard 2.0 or rebuild the libraries for .NET 10 if needed.
