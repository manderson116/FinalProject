using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;

/*
 *          ┌───────────────────────────────────┐
 *          │    PUBLIC SAFETY ANNOUNCEMENT:    │
 *          │     INFORMATION HAZARDS AHEAD     │
 *          │       PROCEED WITH CAUTION        │
 *          └───────────────────────────────────┘
*/

namespace FinalProject
{
    public partial class MainPage : ContentPage
    {
        public class Media
        {
            public string Name { get; set; }
        }
        ObservableCollection<Media> mediaList = new ObservableCollection<Media>();
        public ObservableCollection<Media> MediaList { get { return mediaList; } }

        public MainPage()
        {
            InitializeComponent();

            if (App.MediaRepo.Count() > 0 && App.MediaRepo.GetMaxOrder() > 0)
            {
                for (int i = 1; i <= App.MediaRepo.GetMaxOrder(); i++)
                {
                    // add to local playlist
                    mediaPlaylist.Add(App.MediaRepo.GetFilepath(App.MediaRepo.GetByOrder(i)));

                    // add to ListView list
                    mediaList.Add(new Media() { Name = Path.GetFileName(App.MediaRepo.GetFilepath(App.MediaRepo.GetByOrder(i))) });
                }
                PlaylistView.ItemsSource = mediaList;
                MediaSwitch(0, false);
            }
        }

        FolderPickerResult? folderSelection;
        List<String> mediaPlaylist = new List<String>();
        int playlistIndex = 0;
        private async void SelectFolderButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // Request permission to read files in storage
                var status = PermissionStatus.Unknown;
                status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();

                if (status != PermissionStatus.Granted)
                {
                    if (Permissions.ShouldShowRationale<Permissions.StorageRead>())
                    {
                        await Shell.Current.DisplayAlert("Permissions Required", "This app needs permission to read files in your storage in order to play them.", "OK");
                    }
                    status = await Permissions.RequestAsync<Permissions.StorageRead>();
                }
                if (status != PermissionStatus.Granted)
                {
                    StatusLabel.Text = "File access permissions denied!";
                    return;
                }
            }
            catch (Exception ex)
            {
                return;
            }

            // Folder selection
            folderSelection = await FolderPicker.PickAsync(default);

            if (folderSelection.Folder == null)
            {
                StatusLabel.Text = "No folder was selected!";
                return;
            }

            string folderPath = folderSelection.Folder.Path;
            SelectFolderButton.Text = folderPath;

            // Get files in directory
            if (Directory.Exists(folderPath))
            {
                mediaPlaylist.Clear();
                mediaList.Clear();
                App.MediaRepo.DeleteAll();
                string[] files = Directory.GetFiles(folderPath);
                Array.Sort(files);
                int order = 0;
                foreach (string file in files)
                {
                    if (Path.GetExtension(file) == ".wav" || Path.GetExtension(file) == ".mp3")
                    {
                        order++;
                        // adding the same thing to 3 difference sources because i'm a bad coder
                        // add to local playlist
                        mediaPlaylist.Add(file);

                        // add to ListView list
                        mediaList.Add(new Media() { Name = Path.GetFileName(file) });

                        // add to database
                        App.MediaRepo.Add(Path.GetFileName(file), Path.GetFullPath(file), order);
                    }
                }
            }
            PlaylistView.ItemsSource = mediaList;
            MediaSwitch(0, false);
        }

        private void ShuffleButtonClicked(object sender, EventArgs e)
        {
            if (mediaPlaylist.Count() <= 0)
                return;

            // Save current track's name
            var currentMedia = mediaPlaylist[playlistIndex];
            mediaList.Clear();

            // Shuffle playlist
            mediaPlaylist = mediaPlaylist.OrderBy(_ => Guid.NewGuid()).ToList();
            for (int f = 0; f < mediaPlaylist.Count(); f++)
            {
                mediaList.Add(new Media() { Name = Path.GetFileName(mediaPlaylist[f])});
                App.MediaRepo.UpdateOrder(App.MediaRepo.GetByFilepath(Path.GetFullPath(mediaPlaylist[f])), f + 1);
            }

            // Find new index of current track
            playlistIndex = mediaPlaylist.FindIndex(i => i.Equals(currentMedia));
            CurrentMediaLabel.Text = "[ " + (playlistIndex + 1) + " / " + mediaPlaylist.Count() + " ] " + Path.GetFileName(mediaPlaylist[playlistIndex]);
            PlaylistView.SelectedItem = mediaList.ElementAt(playlistIndex);
        }

        private void PreviousButtonClicked(object sender, EventArgs e)
        {
            PreviousMedia();
        }

        private void PlayPauseButtonClicked(object sender, EventArgs e)
        {
            if (mediaPlaylist.Count() <= 0)
                return;

            if (MediaPlayerElement.CurrentState == MediaElementState.Stopped || MediaPlayerElement.CurrentState == MediaElementState.Paused)
            {
                PlayMedia();
            }
            else if (MediaPlayerElement.CurrentState == MediaElementState.Playing)
            {
                PlayPauseButton.Text = "▶️";
                MediaPlayerElement.Pause();
            }
            else
            {
                PlayPauseButton.Text = "⏯️";
            }
        }

        private void NextButtonClicked(object sender, EventArgs e)
        {
            if (mediaPlaylist.Count() <= 0)
            {
                StatusLabel.Text = "No valid media found!";
                return;
            }
            NextMedia();
        }

        int repeatState = 1;
        private void RepeatButtonClicked(object sender, EventArgs e)
        {
            repeatState = (repeatState + 1) % 3;
            switch (repeatState)
            {
                case 0:
                    RepeatButton.Text = "🔚"; // Stop at the end of the playlist
                    MediaPlayerElement.ShouldLoopPlayback = false;
                    break;
                case 1:
                    RepeatButton.Text = "🔁"; // Loop at the end of the playlist
                    MediaPlayerElement.ShouldLoopPlayback = false;
                    break;
                case 2:
                    RepeatButton.Text = "🔂"; // Loop the current track
                    MediaPlayerElement.ShouldLoopPlayback = true;
                    break;
                default:
                    RepeatButton.Text = "♾"; // Uh oh.
                    break;
            }

            //StatusLabel.Text = "";
            //for (int i = 1; i <= App.MediaRepo.Count(); i++)
            //    StatusLabel.Text += (App.MediaRepo.String(App.MediaRepo.GetByOrder(i)) + "\n");
            //for (int i = 0; i < mediaPlaylist.Count(); i++)
            //    StatusLabel.Text += (mediaPlaylist[i] + "\n");
        }

        private void SeekSliderChanged(object sender, EventArgs e)
        {
            MediaPlayerElement.SeekTo(new TimeSpan(0, 0, 0, 0, (int)SeekSlider.Value));
        }

        private void PlaylistViewItemTapped(object sender, ItemTappedEventArgs e)
        {
            MediaSwitch(e.ItemIndex, true);

            //Media item = args.SelectedItem as Media;
            //if (item == null)
            //    return;

            //playlistIndex = mediaPlaylist.FindIndex(file => Path.GetFileName(file).Equals(item.Name));
            //MediaSwitch(playlistIndex);
        }

        private void MediaPlayerOpened(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(PlayMedia);
        }

        private void MediaPlayerEnded(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(AutoPlay);
        }

        private void AutoPlay()
        {
            MediaPlayerElement.Stop();

            // If playlist ends and looping is off, stop playing
            if (playlistIndex + 1 >= mediaPlaylist.Count() && repeatState == 0)
            {
                PlayPauseButton.Text = "▶️";
                return;
            }

            // If track looping is on, don't go to next track
            if (repeatState != 2)
            {
                NextMedia();
                PlayMedia();
            }
        }

        private void PlayMedia()
        {
            MediaPlayerElement.Play();
            PlayPauseButton.Text = "⏸️";
        }

        private void PreviousMedia()
        {
            var nextIndex = mediaPlaylist.Count() - 1;
            if (playlistIndex > 0)
                nextIndex = (playlistIndex - 1);
            
            MediaSwitch(nextIndex, false);
        }

        private void NextMedia()
        {
            MediaSwitch((playlistIndex + 1) % mediaPlaylist.Count(), false);
        }

        private void MediaSwitch(int mediaIndex, bool skipIfSame)
        {
            StatusLabel.Text = "";
            if (mediaPlaylist.Count() <= 0)
            {
                StatusLabel.Text = "No valid media found!";
                return;
            }
            if (mediaIndex == playlistIndex && skipIfSame)
                return;

            if (!File.Exists(Path.GetFullPath(mediaPlaylist[mediaIndex])))
            {
                int temp = mediaIndex;
                while (!File.Exists(Path.GetFullPath(mediaPlaylist[mediaIndex])))
                {
                    StatusLabel.Text = Path.GetFileName(mediaPlaylist[mediaIndex]).ToString() + " not found! Skipping...";
                    mediaIndex = (mediaIndex + 1) % mediaPlaylist.Count();
                    if (mediaIndex == temp)
                    {
                        StatusLabel.Text = "Where did the songs go?";
                        return;
                    }
                }
            }
            
            CurrentMediaLabel.Text = "[ " + (mediaIndex + 1) + " / " + mediaPlaylist.Count() + " ] " + Path.GetFileName(mediaPlaylist[mediaIndex]);
            PlaylistView.SelectedItem = mediaList.ElementAt(mediaIndex);
            playlistIndex = mediaIndex;
            MediaPlayerElement.Source = MediaSource.FromFile(mediaPlaylist[mediaIndex]);
        }
    }
}
