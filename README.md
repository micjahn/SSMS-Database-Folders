
# [SSMS Database Folders](https://github.com/micjahn/SSMS-Database-Folders)

This an extension for SQL Server Management Studio 2012 and above.
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

## Known Issues

### Compatibility with other extensions
This extension moves nodes in the Object Explorer tree view. This could cause problems with other extensions that are not expecting it. At this point in time, I am not aware of any extensions where this is an issue. If you do have problems then let me know.

Please report any issues to <https://github.com/micjahn/SSMS-Database-Folders/issues>.

## Change Log

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
