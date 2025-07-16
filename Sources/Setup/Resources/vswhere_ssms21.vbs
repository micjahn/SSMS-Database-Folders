Dim shell, exec, output
Set shell = CreateObject("WScript.Shell")
Set exec = shell.Exec("""%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere"" -products Microsoft.VisualStudio.Product.Ssms -property installationPath -version [21.0,22.0)")
output = exec.StdOut.ReadAll
output = Trim(output)
output = Replace(output, vbCrlf, "")

Session.Property("SSMS21INSTALLROOT") = output
