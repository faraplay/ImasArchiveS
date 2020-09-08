using System.Windows.Input;

namespace ImasArchiveApp
{
    public static class CustomCommands
    {
        public static readonly RoutedUICommand Export = new RoutedUICommand
            (
                "Export",
                "Export",
                typeof(CustomCommands)
            );

        public static readonly RoutedUICommand Import = new RoutedUICommand
            (
                "Import",
                "Import",
                typeof(CustomCommands)
            );

        public static readonly RoutedUICommand ExtractAll = new RoutedUICommand
            (
                "Extract All",
                "ExtractAll",
                typeof(CustomCommands)
            );

        public static readonly RoutedUICommand NewFromFolder = new RoutedUICommand
            (
                "New From Folder",
                "NewFromFolder",
                typeof(CustomCommands)
            );

        public static readonly RoutedUICommand ExtractCommus = new RoutedUICommand
            (
                "Extract Commus",
                "ExtractCommus",
                typeof(CustomCommands)
            );

        public static readonly RoutedUICommand ReplaceCommus = new RoutedUICommand
            (
                "Replace Commus",
                "ReplaceCommus",
                typeof(CustomCommands)
            );
    }
}