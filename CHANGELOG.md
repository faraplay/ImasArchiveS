## Version 0.4.0

### Added
New patch editor - can create new patches and add files to the patch

Lyrics extraction

Parameter extraction now extracts more things

### Changed
Moved all menu options to main menu bar

Commu spreadsheet reader now only reads one column of translated messages, to be consistent with the updated online spreadsheet

File import/exporting is now accessed thorugh right-clicking the file you want to import/export on
the file browser.

### Removed
Old patch creation from commu command has been removed - use new patch editor instead


## Version 0.3.2

### Added
Command line arguments are now processed, meaning that you can now double-click on a file in Windows Explorer and use this app to open it.

New command to extract all images from an arc file

New command and dialog for converting an image file into a gtf file

### Changed
Open command is now split into several commands - one for arc files, one for par files, one for gtf files, and one for opening any file with the hex viewer.

GTF reading has been optimised to read faster.

### Fixed
Excel reader can now properly extract strings from cells containing non-shared strings.

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
