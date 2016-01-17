using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using DoIt.ViewModels;

namespace DoIt.Controls
{
    /// <summary>
    /// Ein einfaches Label das den vollen Namen einer Person und ihr Profilbild anzeigt.
    /// </summary>
    public partial class FriendLabel : UserControl
    {
        /// <summary>
        /// Die Person die vom Label angezeigt wird.
        /// Friend DepencyProperty Implementation.
        /// </summary>
        public static readonly DependencyProperty FriendProperty =
             DependencyProperty.Register("Friend", typeof(PersonViewModel),
             typeof(FriendLabel), new PropertyMetadata(null));

        /// <summary>
        /// Die Person die vom Label angezeigt wird.
        /// </summary>
        public PersonViewModel Friend
        {
            get { return (PersonViewModel)GetValue(FriendProperty); }
            set { SetValue(FriendProperty, value); }
        }

        /// <summary>
        /// Konstruktor.
        /// </summary>
        public FriendLabel()
        {
            InitializeComponent();
        }
    }
}
