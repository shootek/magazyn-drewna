using System;
using Microsoft.Win32;

namespace MagazynDrewna
{
    internal static class ExportDialog
    {
        public static bool TryPickOpenPath(out string filePath)
        {
            filePath = null;
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Pliki CSV (*.csv)|*.csv",
                DefaultExt = ".csv"
            };

            if (dialog.ShowDialog() != true)
            {
                return false;
            }

            filePath = dialog.FileName;
            return true;
        }

        public static bool TryPickSavePath(string defaultFileName, out string filePath)
        {
            filePath = null;
            var dialog = new SaveFileDialog
            {
                Filter = "Pliki CSV (*.csv)|*.csv",
                DefaultExt = ".csv",
                FileName = defaultFileName
            };

            if (dialog.ShowDialog() != true)
            {
                return false;
            }

            filePath = dialog.FileName;
            return true;
        }
    }
}
