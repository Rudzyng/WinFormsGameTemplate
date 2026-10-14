namespace WinFormsGameTemplate
{
    public partial class Form1 : Form
    {
        protected API PortalApi { get; }
        protected GameLaunchContext LaunchContext { get; }
        protected GameSessionResponse? PortalSession { get; private set; }

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
            return SubmitScoreCoreAsync(amount);
        }

        async Task<ScoreResponse?> SubmitScoreCoreAsync(int amount)
        {
            return await PortalApi.AddScoreAsync(LaunchContext, amount);
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (LaunchContext.SessionId is not > 0)
                return;

            try
            {
                PortalSession = await PortalApi.ResolveGameSessionAsync(LaunchContext);
                Text = $"Игра — {PortalSession.userName} · {PortalSession.totalUserPoints}";
            }
            catch (Exception exc)
            {
                MessageBox.Show(
                    exc.Message,
                    "Игровая сессия отклонена",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Close();
            }
        }
    }
}
