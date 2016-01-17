using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DoIt.Controls
{
    //Hilfsklasse die folgendes Problem verhindert:
    //Zu schnelles auf App Bar Hämmern, Mehr als eine MessageBox offen.
    //http://porcupineprogrammer.blogspot.de/2012/02/messagebox-crash-on-windows-phone-7.html
    internal static class SafeMessageBox
    {
        private static bool isMessageBoxOpen = false;
        internal static MessageBoxResult Show(string message, string title)
        {
            if (!isMessageBoxOpen)
            {
                isMessageBoxOpen = true;
                var result = MessageBox.Show(message, title, MessageBoxButton.OK);
                isMessageBoxOpen = false;
                return result;
            }
            else 
                return MessageBoxResult.Cancel;
        }
    }
}
