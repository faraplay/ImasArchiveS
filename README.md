# ImasArchiveS
App to read, edit and extract files from the game Idolmaster: One For All (PS3).

## Instructions for usage
### Opening
Click on "File >> Open" to open a new arc file. 
Make sure that the corresponding .bin file of the same name is in the same directory as the .arc file when you do this.

You can also change the drop-down option for file type in the file selection dialog to "All files"
to select other types of files.

Alternatively, you can click on "File >> New Arc" to select a folder from which a new .arc file (and corresponding .bin)
will be built.

To close the currently open file, click on "File >> Close". Note that no changes will be saved.

### Arc file options
#### Extracting & Replacing files
You can browse through the folders within the .arc in the sidebar on the left. Double-click to open a folder or file.

Opening a file will display its contents in the hex viewer on the right.

Once a file is opened, you can extract that particular file by selecting "File >> Export" from the smaller menu.
You can replace the file with the contents of another file by selecting "File >> Import" from the smaller menu.

Note that renaming the file is not yet supported.

To save your changes to the arc, select "Arc >> Save Arc". Then input a new name for the arc. Note that overwriting the currently 
open arc is not allowed.

#### Commu Extracting
To extract all commus from the arc, choose "Arc >> Extract Commus", then input a name for a new folder.

To replace commus in the arc with custom commus, choose "Arc >> Replace Commus", 
then choose a folder containing all the commus you wish to insert.

#### Font patching
To patch the font (stored in disc.arc) for the custom commus, choose "Arc >> Patch Font"
while disc.arc is opened.