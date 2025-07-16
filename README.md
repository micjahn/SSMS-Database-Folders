
# [SSMS Database Folders](https://github.com/micjahn/SSMS-Database-Folders)

This an extension for SQL Server Management Studio 18 and above. Version 17 and lower are only supported until version 1.0.4.
It groups databases in the Object Explorer tree into folders.

Source code, documentation and issues can be found at <https://github.com/micjahn/SSMS-Database-Folders>

This work is based upon [SSMS Schema Folders](https://github.com/nicholas-ross/SSMS-Schema-Folders).

## Install

[Download the latest release.](https://github.com/micjahn/SSMS-Database-Folders/releases)

## Options

There are a few user options which change the style and behaviour of the databases folders.
`Tools > Options > SQL Server Object Explorer > Database Folders`

* Group databases in folders by name - Activates grouping of databases in folders by using that part of the database names before the first underscore
* Separate folders for readonly databases - Use a separate folder for readonly databases
* Custom folder configurations - set a list of configurations for custom folders
	* Custom folder name - user defined name of the folder
	* Add databases to folder by regular expressions - a list of regular expression to define which databases should be added to the folder
	* Use other grouping mechanisms inside the custom folder - the main options are used to make sublevels of folders and build a structure
* Build folder name by regular expressions
* Enabled - enables or disables the extension. can also be toggled by a button in the toolbar

## Known Issues

### Compatibility with other extensions
This extension moves nodes in the Object Explorer tree view. This could cause problems with other extensions that are not expecting it. At this point in time, I am not aware of any extensions where this is an issue. If you do have problems then let me know.

Please report any issues to <https://github.com/micjahn/SSMS-Database-Folders/issues>.

## SSMS 17 and below
The last version of this extension which works with SSMS 17 and lower is version 1.0.4.

## Change Log

### v1.0.5 (2025-07-16)
* added support for SSMS 21
* removed support for SSMS 17 and older

### v1.0.4 (2024-08-29)
* added support for custom folders

### v1.0.3 (2024-08-20)
* added support for SSMS 20

### v1.0.2 (2023-12-19)
* added support for SSMS 19
* added button to the SSMS toolbar for temporary activation/deactivation of the addin
* added option for showing debug output

### v1.0.1 (2020-09-05)
* added option for regulare expressions
* sort folders

### v1.0 (2020-xx-xx)
* first release.
