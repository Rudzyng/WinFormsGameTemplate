namespace WinFormsGameTemplate
{
    public partial class Form1 : Form
    {
        protected API PortalApi { get; }
        protected GameLaunchContext LaunchContext { get; }

        public Form1() : this(new API(), GameLaunchContext.Empty)
        {
        }

        public Form1(API portalApi, GameLaunchContext launchContext)
        {
            PortalApi = portalApi;
            LaunchContext = launchContext;
            InitializeComponent();
        }

        /// <summary>
        /// Отправляет итог игры в портал. Вызывайте этот метод после завершения партии.
        /// </summary>
        protected Task<ScoreResponse?> SubmitScoreAsync(int amount)
        {
            return PortalApi.AddScoreAsync(LaunchContext, amount);
        }
    }
}
