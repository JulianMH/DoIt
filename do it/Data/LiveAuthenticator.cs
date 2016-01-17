using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.Live;
using System.IO;
using System.IO.IsolatedStorage;

namespace DoIt.Data
{
    internal static class LiveAuthenticator
    {
        private static LiveConnectSession session;
        /// <summary>
        /// Versucht Login, ohne einen entsprechenden Dialog zu zu zeigen.
        /// </summary>
        /// <returns>Ein MobileServiceUser, der awaited werden kann oder null bei Misserfolg.</returns>
        internal static async Task<MobileServiceUser> AuthenticateSilent(MobileServiceClient mobileService)
        {
            LiveAuthClient liveAuthClient = new LiveAuthClient(APIKeys.LiveClientId);
            session = (await liveAuthClient.InitializeAsync()).Session;
            return await mobileService.LoginWithMicrosoftAccountAsync(session.AuthenticationToken);

        }

        /// <summary>
        /// Login mit einem entsprechenden Dialog.
        /// </summary>
        /// <returns>Ein MobileServiceUser, der awaited werden kann oder null bei Misserfolg.</returns>
        internal async static Task<MobileServiceUser> Authenticate(MobileServiceClient mobileService)
        {
            LiveAuthClient liveAuthClient = new LiveAuthClient(APIKeys.LiveClientId);
            var source = new TaskCompletionSource<MobileServiceUser>();

            liveAuthClient.Logout();

            var scopes = new string[] { "wl.basic", "wl.offline_access", "wl.signin" };
            App.Current.RootVisual.Visibility = System.Windows.Visibility.Collapsed;

            try
            {
                session = (await liveAuthClient.LoginAsync(scopes)).Session;
            }
            finally
            {
                App.Current.RootVisual.Visibility = System.Windows.Visibility.Visible;
            }
            return await mobileService.LoginWithMicrosoftAccountAsync(session.AuthenticationToken);

        }

        /// <summary>
        /// Lädt informationen wie Namen über den Windows Live User.
        /// </summary>
        /// <param name="userId">id des Users.</param>
        /// <returns>Der neu erstellte User.</returns>
        internal async static Task<Person> GetNewUserData(string userId, string notificationChannel, int score, DateTime accountCreatedDate)
        {
            LiveConnectClient client = new LiveConnectClient(session);

            var data = (await client.GetAsync("me")).Result;

            return new Person(0, userId, (string)data["id"], (string)data["last_name"], (string)data["first_name"],
                    notificationChannel, System.Globalization.CultureInfo.CurrentCulture.ToString(), score, App.VersionNumber, accountCreatedDate);

        }

        /// <summary>
        /// Loggt den aktuellen Nutzer aus.
        /// </summary>
        internal static void Logout()
        {
            LiveAuthClient liveAuthClient = new LiveAuthClient(APIKeys.LiveClientId);
            liveAuthClient.Logout();
        }

        /// <summary>
        /// Gibt einen Isolated Storage Pfad zu einem Profilbild einer Person an.
        /// Bei Bedarf wird dieses erst von Server geladen.
        /// </summary>
        /// <param name="authenticationUserId">Nutzer dessen Profilbild geladen werden soll.</param>
        /// <returns></returns>
        internal static async Task<string> GetProfilePicture(string authenticationUserId)
        {
            if (authenticationUserId == null)
                return null;
            try
            {
                string fileName = authenticationUserId + ".png";
                using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (file.FileExists(fileName))
                    {
                        //Wenn die Datei weniger als einen Monat alt ist vewende sie.
                        if ((file.GetCreationTime(fileName).DateTime - DateTime.Now) < TimeSpan.FromDays(30))
                            return fileName;
                        else //Ansonsten löschen und neu downloaden, dadurch immer schön aktuell.
                            file.DeleteFile(fileName);
                    }

                    //LiveConnectClient bereitstellen
                    LiveConnectClient client = new LiveConnectClient(session);

                    //Pfad des Profilbildes von Windows Live abrufen.
                    var path = authenticationUserId + "/picture";

                    var task = client.DownloadAsync((string)(await client.GetAsync(path)).Result["location"]);
                    using (var fileStream = (await task).Stream)

                    //Filestream auf Platte abspeichern
                    {
                        using (FileStream fileStreamSave = file.CreateFile(fileName))
                        {
                            byte[] buffer = new byte[8 * 1024];
                            int length;

                            while ((length = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                                fileStreamSave.Write(buffer, 0, length);

                            fileStreamSave.Flush();
                        }
                    }

                    return fileName;
                }
            }
            catch (Exception e)
            {
                //Wenn Debugger vorhanden ist, Fehler beachten ansonsten Silent einfach ohne Profilbild arbeiten.
                if (System.Diagnostics.Debugger.IsAttached)
                    throw new Exception("Could not load profile picture.", e);
                else
                    return null;
            }
        }
    }
}

