using System;
using DoIt.Data;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using System.IO.IsolatedStorage;

namespace DoIt.ViewModels
{
    /// <summary>
    /// ViewModel, stellt eine Person dar.
    /// </summary>
    public class PersonViewModel : NotifyPropertyChangedObject
    {
        private Person friend;

        #region Eigenschaften
        /// <summary>
        /// Vorname der Person.
        /// </summary>
        public string FirstName { get { return friend.FirstName; } }

        /// <summary>
        /// Name der Person.
        /// </summary>
        public string Name { get { return friend.Name; } }

        /// <summary>
        /// Name der Person.
        /// </summary>
        public string FullName { get { return friend.GetFullName(); } }

        /// <summary>
        /// Punktestand der Person.
        /// </summary>
        public int Score { get { return friend.Score; } }

        /// <summary>
        /// Punktestand der Person im Tagesdurchschnitt.
        /// </summary>
        public int AverageScore
        {
            get
            {
                int days = Math.Max(1, (int)((DateTime.Today - friend.AccountCreatedDate).TotalDays + 0.5));
                return friend.Score / days;
            }
        }

        /// <summary>
        /// Name des Landes aus dem die Person stammt.
        /// </summary>
        public string CountryName
        {
            get
            {
                try
                {
                    return new System.Globalization.RegionInfo(new System.Globalization.CultureInfo(friend.Culture).Name).NativeName;
                }
                catch
                {
                    return Localization.Strings.FriendDetailPageUnknownCountry;
                }
            }
        }

        /// <summary>
        /// Datum, an dem der Account erstellt wurde.
        /// </summary>
        public string AccountCreatedDate { get { return friend.AccountCreatedDate.ToShortDateString(); } }

        /// <summary>
        /// Source zu einem Profilbild der Person.
        /// </summary>
        public BitmapImage Picture { get; private set; }
        #endregion

        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="friend">Dar darzustellende Person.</param>
        internal PersonViewModel(Person friend)
        {
            this.friend = friend;
            this.friend.PropertyChanged += friend_PropertyChanged;

            //Standartprofilbild setzen, dann asynchron das richtige Profilbild laden.
            this.Picture = (BitmapImage)App.Current.Resources["ContactDefaultPicture"];
            if(this.friend.AuthenticationUserId != "")
                LoadProfilePicture();
        }

        void friend_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Score")
            {
                NotifyPropertyChanged("Score");
                NotifyPropertyChanged("AverageScore");
            }
        }

        /// <summary>
        /// Versucht asynchron ein Profilbild zu laden.
        /// </summary>
        private async void LoadProfilePicture()
        {
            try
            {
                string pictureFileName = await LiveAuthenticator.GetProfilePicture(friend.AuthenticationUserId);

                if (pictureFileName != null)
                {
                    BitmapImage image = null;
                    using (var imageStream = await LoadImageAsync(pictureFileName))
                    {
                        if (imageStream != null)
                        {
                            image = new BitmapImage();
                            image.SetSource(imageStream);
                        }
                    }
                    if (image != null)
                    {
                        this.Picture = image;
                        NotifyPropertyChanged("Picture");
                    }
                }
            }
            catch (Exception e)
            {
                //Nichts tun, ist ja nur ein Profilbild und den Ärger nicht wert.
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
            }
        }

        /// <summary>
        /// Lädt einen Stream für ein Bild aus dem Isolated Storage.
        /// </summary>
        /// <param name="filename">Dateiname des Bilds im Isolated Storage.</param>
        /// <returns>Einen Awaitbaren Task der einen Stream liefert.</returns>
        private Task<Stream> LoadImageAsync(string filename)
        {
            return System.Threading.Tasks.Task.Factory.StartNew<Stream>(() =>
            {
                Stream stream = null;

                using (var isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (isoStore.FileExists(filename))
                    {
                        stream = isoStore.OpenFile(filename, System.IO.FileMode.Open, FileAccess.Read);
                    }
                }
                return stream;
            });
        }

        public override bool Equals(object obj)
        {
            //Wofür wird das genutzt? Bei der Suche nach Leuten anscheindend.
            if (obj is PersonViewModel)
                return (this.friend.UserId == ((PersonViewModel)obj).friend.UserId);
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return friend.UserId.GetHashCode();
        }

        /// <summary>
        /// Liefert die UserId des dargestellten Nutzers.
        /// </summary>
        /// <returns>Die UserId des dargestellten Nutzers.</returns>
        internal string GetUserId()
        {
            return friend.UserId;
        }
    }
}
