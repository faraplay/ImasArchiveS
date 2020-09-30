# ImasArchiveS

# Imas-Archive.exe
App to read, edit, extract and patch files from the game Idolmaster: One For All (PS3).

## Instructions for usage
### Opening
Click on "File >> Open >> Open Arc" to open a new arc file. 
Make sure that the corresponding .bin file of the same name is in the same directory as the .arc file when you do this.

Click on "File >> Open >> Open Par" to open a par file, and "File >> Open >> Open GTF" to open a gtf (image) file.

To view a file in the hex viewer, click "File >> Open >> Open in Hex Viewer".

Alternatively, you can click on "File >> New Arc" to select a folder from which a new .arc file (and corresponding .bin)
will be built.

To close the currently open file, click on "File >> Close". Note that no changes will be saved.

### Patching
To patch an arc file, click on "Patch >> Patch Arc From Zip". Then select the arc file to patch
and the .zip file containing the new files.
The program will then prompt you to input a name for the patched .arc file. Make sure to input a new name - do not 
attempt to overwrite the .arc file that is currently being patched.
When patching the game, make sure to patch each of the arc files. This includes the DLC (though you'll have to decrypt those first, 
using another program).

### Patch creation
You can create the patch file containing all the translated commus yourself. 
First, download the spreadsheet 
containing all the translations, and then open it in Excel and save it again (as a .xlsx file).
You'll also need to extract all parameter files from all the .arc files of the original game, copy-paste 
the translated text into the extracted parameter spreadsheets, and save it again.

To add images and lyrics files to the patch, you'll need the filenames.xlsx files you get from extracting all 
images/lyrics (see below), as those contain the details of where in the .arc files the images/lyrics are stored.

Then, in this app, select "File >> New Patch" and enter a name for the new patch.
You can now use the "Patch Zip >> Add..." options to add new files.
* Add File: You can add any file to the patch zip. Select the file to be added and input a file path for the file
to be saved in the patch.zip file.
* Add Commus: You can select the spreadsheet containing all translations that you saved in Excel. The program will
then generate and add all commu files that have translated content to the patch.zip.
* Add Parameter Files: To use this, select the parameter spreadsheets that you edited by pasting the translated text into.
Again this will generate and add all the parameter files.
* Add Images: Select a folder containing the images you want to put in the patch and the filenames.xlsx from image extraction.
You do not need to include the original extracted images in the folder.
Make sure that the new images have the same name as the original image it is replacing. The program will then 
read the filenames.xlsx to figure out what filepath is needed for the images being inserted. If this process is taking too long
you can delete some rows from the filenames.xlsx file - those images will not be replaced.
* Add Lyrics: Similar to adding images, you need to select a folder containing the filenames.xlsx from extracting the lyrics
as well as the translated lyrics you want to insert. Once again, make sure that the new lyrics file has the same name as the original
file you want to replace.

Do not attempt to add the same file more than once.

Any changes you make will not be saved until you close the patch.zip file through "File >> Close". Your changes will 
not be saved if you close the whole program without doing this!

### Arc file options
#### Extracting & Replacing files
You can browse through the folders within the .arc in the sidebar on the left. Double-click to open a folder or file.

Opening a file will display its contents in the hex viewer on the right.

You can extract a particular file by right-clicking it in the file browser pane and selecting "Export" from the menu that pops up.
You can also extract all files in the arc by selecting "Arc >> Extract All" and typing in a new folder name.

You can replace a file with the contents of another file by right-clicking the file to be replaced and selecting "Import"
from the menu that pops up.

Note that renaming the file is not supported.

To save your changes to the arc, select "Arc >> Save Arc". Then input a new name for the arc. Note that overwriting the currently 
open arc is not allowed.

#### Extracting Specific Data
To extract all commus from the arc, choose "Arc >> Extract... >> Extract Commus", then input a name for the new spreadsheet.
All the text and data for the commu messages will be outputted into a new spreadsheet.

Data in the parameter folder can also be extracted. Choose "Arc >> Extract... >> Extract Parameter Data" to extract to a 
spreadsheet. Note that research into the types of data is still ongoing, so the data that is extracted may vary from version to version
of this program.

Images and lyrics can also be extracted. Select "Arc >> Extract... >> Extract Images" and input a new folder name to extract all images.
To extract lyrics, select "Arc >> Extract... >> Extract Lyrics" and input a new folder name.
This process may take a very long time.

#### Font patching
To patch the font (stored in disc.arc) for the patched commus, choose "Arc >> Patch Font"
while disc.arc is opened. Note that the default font included with the game is quite wonky, and
it's better to use a custom-edited font when patching.

### Other file options

#### Converting images to GTF 
Click on "File >> Convert Image to GTF" to open the GTF conversion dialog.

Select an image to convert by clicking on the "Select Image" button, and select a file name for the new GTF by 
clicking "Select Save Location".

You also need to select a GTF type by choosing from the drop-down list. It's best to match the type of the GTF file 
you are trying to replace. Note that some types do not support transparency.

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

# imas.exe
Command-line app to patch and extract game files as well as create patches.

It has the following operations:

#### help
Type "imas help" to get this.

This explains how to use the app and what each option does.

#### patch
Patches an arc file with the specified patch and outputs to the new filename.
Make sure the input and output filenames are different as the program will not allow you to
overwrite the input file.

#### make-patch
Creates or adds to a patch file. You can add commus, parameter files, images, lyrics files, or any other type of file with this.
The default behaviour is to add to the existing patch file, or create a new patch file if none exists. You can override this
and force it to overwrite the old patch by using the overwrite option.

#### extract
Extracts files or data from an arc file. You can extract commu or parameter data in the form of a spreadsheet,
extract all images or lyrics to a new directory, or extract all files to a new directory.
Note that you can only extract one thing at a time.

#### copy
This will copy columns from a source spreadsheet to a destination spreadsheet. This will only copy 
from one column to another if the sheets they are in share the same name, the columns have the 
same heading, and the sheet name is one of a few specified names.
This is useful for copying translated text from the Google sheet to a parameter data spreadsheet.

Note that with all of these options you can use the -v or --verbose option to make the program output reports 
of its progress to the console.
