using UnityEngine;

namespace GermSweep3D
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public GameState State { get; private set; }
        public SaveSystem Save { get; private set; }
        public UpgradeSystem Upgrades { get; private set; }
        public IAdService Ads { get; private set; }
        public AudioService Audio { get; private set; }
        public HapticService Haptics { get; private set; }
        public LevelManager Level { get; private set; }
        public UIManager UI { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureBootstrap()
        {
            if (Instance != null) return;
            GameObject go = new GameObject("GameManager"); DontDestroyOnLoad(go); go.AddComponent<GameManager>();
        }
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 60; Screen.orientation = ScreenOrientation.Portrait;
            Save = new SaveSystem(); Save.Initialize(); Upgrades = new UpgradeSystem(Save); Ads = new MockAdService(); Audio = new AudioService(Save); Haptics = new HapticService(Save);
            Level = gameObject.AddComponent<LevelManager>(); UI = gameObject.AddComponent<UIManager>();
        }
        private void Start() { ChangeState(GameState.Boot); }
        public void ChangeState(GameState next)
        {
            State = next;
            switch (next)
            {
                case GameState.Boot: Time.timeScale = 1; UI.Initialize(this); Level.Initialize(this); ChangeState(GameState.Home); break;
                case GameState.Home: Time.timeScale = 1; Level.ClearLevel(); UI.ShowHome(); break;
                case GameState.Gameplay: Time.timeScale = 1; Level.SetGameplayEnabled(true); UI.ShowGameplay(); break;
                case GameState.Pause: Time.timeScale = 0; Level.SetGameplayEnabled(false); UI.ShowPause(); break;
                case GameState.Win: Time.timeScale = 1; Level.SetGameplayEnabled(false); Audio.PlayWin(); UI.ShowWin(Level.LastReward, Level.CleanPercent, Level.PatientsHealed); break;
                case GameState.Lose: Time.timeScale = 1; Level.SetGameplayEnabled(false); Audio.PlayLose(); UI.ShowFail(Level.FailReason); break;
            }
        }
        public void Play() { Level.LoadLevel(Save.CurrentLevel); ChangeState(GameState.Gameplay); }
        public void Restart() { Level.LoadLevel(Save.CurrentLevel); ChangeState(GameState.Gameplay); }
        public void Resume() { ChangeState(GameState.Gameplay); }
        public void Pause() { if (State == GameState.Gameplay) ChangeState(GameState.Pause); }
        public void Home() { ChangeState(GameState.Home); }
        public void ContinueAfterWin()
        {
            int completed = Mathf.Max(1, Save.CurrentLevel);
            Save.CurrentLevel = Mathf.Min(completed + 1, 5);
            if (completed >= 2 && completed % 2 == 0 && Ads.IsInterstitialReady()) Ads.ShowInterstitial(() => ChangeState(GameState.Home)); else ChangeState(GameState.Home);
        }
    }
}
