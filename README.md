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

### Patching
To patch an arc file, click on "Patch >> Patch Arc >> From Zip". Then select the arc file to patch
and the .zip file containing the new files.
When patching the game, make sure to patch each of the arc files.

### Patch creation
You can create the patch file containing all the translated commus yourself. First, download the spreadsheet 
containing all the translations, and then open it in Excel and delete all sheets without translations.
Then, in this app, select "Patch >> Create Commu Patch" and select the spreadsheet you just edited.
(Make sure that the spreadsheet is saved as an .xlsx file.)
Then type in a name for the new zip file and the parch file will be created.

### Arc file options
#### Extracting & Replacing files
You can browse through the folders within the .arc in the sidebar on the left. Double-click to open a folder or file.

Opening a file will display its contents in the hex viewer on the right.

Once a file is opened, you can extract that particular file by selecting "File >> Export" from the smaller menu.
You can also extract all files in the arc by selecting "Arc >> Extract All" and typing in a new folder name.

You can replace the file with the contents of another file by selecting "File >> Import" from the smaller menu.

Note that renaming the file is not supported.

To save your changes to the arc, select "Arc >> Save Arc". Then input a new name for the arc. Note that overwriting the currently 
open arc is not allowed.

#### Commu Extracting
To extract all commus from the arc, choose "Arc >> Extract Commus", then input a name for the new spreadsheet.
All the text and data for the commu messages will be outputted into a new spreadsheet.

#### Font patching
To patch the font (stored in disc.arc) for the patched commus, choose "Arc >> Patch Font"
while disc.arc is opened.