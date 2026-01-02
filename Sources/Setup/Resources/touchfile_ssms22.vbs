Dim path, fso, file, objShell, objFolder
Dim shell, exec, output

' following doesn't work as expected if elevated execution runs
'path = Session.Property("SSMS22INSTALLROOT") + "Common7\IDE\Extensions\extensions.configurationchanged"

Set shell = CreateObject("WScript.Shell")
Set exec = shell.Exec("""%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere"" -products Microsoft.VisualStudio.Product.Ssms -property installationPath -version [22.0,23.0)")
output = exec.StdOut.ReadAll
output = Trim(output)
output = Replace(output, vbCrlf, "")

path = output + "\Common7\IDE\Extensions\extensions.configurationchanged"

Set fso = CreateObject("Scripting.Filesystemobject")
Set file = fso.GetFile(path)
Set objShell = CreateObject("Shell.Application")

Set objFolder = objShell.NameSpace(file.ParentFolder.Path)
objFolder.Items.Item(file.Name).ModifyDate = Now
