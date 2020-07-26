## Version 0.3.1

### Added
New command to extract parameter data from an arc file

Hex viewer now has different encoding options

GTF viewer now has option to save image as a .png

### Fixed
Patching a currently open arc is now possible.

The current file is no longer closed if you choose to open a file and select "Cancel".

The hex viewer now properly displays data past offset 0x10000.

## Version 0.3.0

### Added
New patch command is added - arcs can now be patched with a zip file or folder

New command "Create Commu Patch" to create a zip file containing all translated commus from an Excel spreadsheet

### Changed
Extracting commus now outputs the commus into a single Excel spreadsheet

### Removed
Replace Commu command has been removed - use new patch method instead

## Version 0.2.0

### Added
Special viewers for gtf/dds (images) and par/pta files

New panel system for viewing par files

The name of the current file is displayed to the left of the window

### Changed
Menus have been rearranged again

The list of files to the left is now collapsible


## Version 0.1.1

### Added
Command to extract all commus

Default folder name appears in dialog when extracting files

### Changed
File names in the file browser no longer have the .gz extension

Order of menu commands has been rearranged

### Fixed
Bug where any changes to a file in an arc would be disposed after extracting files
