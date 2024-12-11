namespace FinalProject
{
    public partial class App : Application
    {
        public static MediaRepository MediaRepo { get; private set; }

        public App(MediaRepository mediaRepo)
        {
            InitializeComponent();

            MainPage = new AppShell();

            MediaRepo = mediaRepo;
        }
    }
}
