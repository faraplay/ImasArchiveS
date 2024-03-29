## Version 0.7.0

### Added
New image/UI component renderer now supports zooming in/out and dragging, and saving the rendered image

UI editor new features:
- Display, edit and export animations
- Add/delete sprites and controls
- Export the PAU part of a component (UI component data)

Accelerator keys (Alt+ shortcuts) have been added for menu options

DLC catalog info and work info is now also extracted to spreadsheet and added from speradsheet to patches

### Changed

UI editor now uses the specified default visibility for UI controls instead of always rendering them as visible

Border rectangles of sprites in a spritesheet can now be toggled on/off

File browser no longer displays the files and folders as buttons

File browser uses greyed-out icons when navigation buttons are disabled

Refactored parameter extraction and UI component parsing

### Fixed
Rewrote color quantizer to fix bug with transparent pixels

## Version 0.6.0

### Added
New UI component editor that can display UI components

Program now extracts stage info

### Changed
Less progress message spam when adding images to a patch.zip

### Fixed
Fix stride value in 4x4 block GTF creation

Fixed bug where GTF creation would produce black pixels on a very slight gradient

Console app will now actually add lyrics files from the lyrics folder

## Version 0.5.1

### Added
Program now adds "all"-commus to a patch

DLC costume data is now extracted and added to patches

### Changed
Program no longer spams as many progress messages when extracting commus or patching an arc file

### Fixed
Fixed parameter pastbl extraction and adding to patch

Extracting parameter process no longer reports extracting skill board if the spreadsheet already contains skill board data

## Version 0.5.0

### Added
Command-line app with various features such as patching, extracting, patch creation and spreadsheet data copying.

Parameter extraction now also extracts mail (both system and idol mail)

Spreadsheet data copy function (on command-line app only) that can copy columns of data from one spreadsheet to another. It will only copy from one column to
another if the two columns are in sheets with the same name and have the same heading.

### Fixed
App will now no longer add multiple files of the same name to a zip file - instead, any such files will be skipped.

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
