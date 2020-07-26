# ImasArchiveS
App to read, edit, extract and patch files from the game Idolmaster: One For All (PS3).

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
When patching the game, make sure to patch each of the arc files. This includes the DLC (though you'll have to decrypt those first, 
using another program).

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

#### Extracting Specific Data
To extract all commus from the arc, choose "Arc >> Extract Commus", then input a name for the new spreadsheet.
All the text and data for the commu messages will be outputted into a new spreadsheet.

Data in the parameter folder can also be extracted. Choose "Arc >> Extract Parameter Data" to extract to a 
spreadsheet. Note that research into the types of data is still ongoing and not all data in the parameter folder 
will be extracted.

#### Font patching
To patch the font (stored in disc.arc) for the patched commus, choose "Arc >> Patch Font"
while disc.arc is opened.

### Other file options

#### GTF (image) viewer
Clicking the "Save" menu option allows you to export the GTF file as a PNG file.

#### Hex viewer
You can select the encoding for the text view on the the right by choosing from the Encoding menu 
header. There are three options:

* ASCII: Uses the basic ASCII character set. This charset has only 128 characters, therefore 
bytes that differ by a value of 128 will be displayed as the same character. The translation patch 
uses character codes with ASCII offset by 128, so use this to read patched text.

* Latin-1: Uses the ISO-8859-1 charset (which coincides with the first 256 chars of Unicode). 
This is probably the charset used in most hex editors.

* UTF16-BE: This groups bytes into 16-bit big-endian words and displays the Unicode character with that 
number. This is the encoding used in-game for most text, so use this to read any text in the files.