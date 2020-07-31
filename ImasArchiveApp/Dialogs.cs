using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImasArchiveApp
{
    class Dialogs : IGetFileName
    {

        public string OpenGetFileName(string title, string filter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter
            };
            return (openFileDialog.ShowDialog() == true) ? openFileDialog.FileName : null;
        }
        public string SaveGetFileName(string title, string defaultPath, string filter)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = title,
                Filter = filter
            };
            if (defaultPath != null && defaultPath.Contains('\\'))
            {
                saveFileDialog.FileName = defaultPath.Substring(defaultPath.LastIndexOf('\\') + 1);
                saveFileDialog.InitialDirectory = defaultPath.Substring(0, defaultPath.LastIndexOf('\\'));
            }
            return (saveFileDialog.ShowDialog() == true) ? saveFileDialog.FileName : null;
        }
        public string SaveGetFileName(string title, string defaultDir, string defaultName, string filter)
        {
            return SaveGetFileName(title, defaultDir + '\\' + defaultName, filter);
        }
        public string OpenGetFolderName(string title)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                Title = title,
                IsFolderPicker = true
            };
            return dialog.ShowDialog() == CommonFileDialogResult.Ok ? dialog.FileName : null;
        }

        public static (string, string, int)? GetConvertToGTFData()
        {
            ConvertToGtfDialog dialog = new ConvertToGtfDialog();
            if (dialog.ShowDialog() == true)
            {
                return (dialog.ImagePath, dialog.GtfPath, dialog.Type);
            }
            else
            {
                return null;
            }
        }
    }
}
