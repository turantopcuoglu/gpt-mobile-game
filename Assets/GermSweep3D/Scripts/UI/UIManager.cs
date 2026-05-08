using UnityEngine;
using UnityEngine.UI;

namespace GermSweep3D
{
    public sealed class UIManager : MonoBehaviour
    {
        private GameManager game;
        private Canvas canvas;
        private GameObject homePanel, hudPanel, pausePanel, winPanel, failPanel, upgradePanel, settingsPanel;
        private Text homeCoins, homeLevel, hudLevel, hudCoins, hudWaste, hudMedical, hudPatients, hudTimer, winEarned, winClean, winPatients, failReason;
        private UnityEngine.UI.Slider cleanSlider, capacitySlider;
        private readonly UpgradeCard[] cards = new UpgradeCard[4];

        public void Initialize(GameManager manager)
        {
            if (canvas != null) return;
            game = manager; BuildCanvas(); BuildHome(); BuildHud(); BuildPause(); BuildWin(); BuildFail(); BuildUpgrade(); BuildSettings(); game.Level.HudChanged += RefreshHud;
        }
        public void ShowHome() { SetOnly(homePanel); RefreshHome(); }
        public void ShowGameplay() { SetOnly(hudPanel); RefreshHud(); }
        public void ShowPause() { SetOnly(pausePanel); }
        public void ShowWin(int reward, float clean, int patients) { SetOnly(winPanel); winEarned.text = "Earned: " + reward; winClean.text = "Clean score: " + Mathf.RoundToInt(clean) + "%"; winPatients.text = "Patients healed: " + patients; }
        public void ShowFail(string reason) { SetOnly(failPanel); failReason.text = string.IsNullOrEmpty(reason) ? "Level failed" : reason; }

        private void BuildCanvas()
        {
            GameObject root = new GameObject("GermSweepCanvas"); DontDestroyOnLoad(root); canvas = root.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; root.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; root.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920); root.AddComponent<GraphicRaycaster>();
        }
        private void BuildHome()
        {
            homePanel = Panel("HomePanel"); AddTitle(homePanel, "GERM SWEEP 3D", 760); homeCoins = Label(homePanel, "Coins: 0", -620); homeLevel = Label(homePanel, "Level 1", -700);
            AddRobotPreview(homePanel); Button(homePanel, "Play", -80, () => game.Play()); Button(homePanel, "Upgrades", -230, () => { SetOnly(upgradePanel); RefreshUpgrade(); }); Button(homePanel, "Skins", -380, () => Debug.Log("Skins placeholder")); Button(homePanel, "Settings", -530, () => SetOnly(settingsPanel)); Button(homePanel, "Privacy Policy", -680, () => Application.OpenURL("https://example.com/privacy"));
        }
        private void BuildHud()
        {
            hudPanel = Panel("GameplayHUD"); hudLevel = Label(hudPanel, "Level", 820); hudCoins = Label(hudPanel, "Coins", 740); cleanSlider = Slider(hudPanel, "Clean", 630); capacitySlider = Slider(hudPanel, "Capacity", 540); hudWaste = Label(hudPanel, "Waste: 0", 440); hudMedical = Label(hudPanel, "Medical: 0", 360); hudPatients = Label(hudPanel, "Patients: 0", 280); hudTimer = Label(hudPanel, "Timer", 200); Button(hudPanel, "Pause", 800, () => game.Pause(), new Vector2(230, 90), new Vector2(360, 0));
        }
        private void BuildPause()
        {
            pausePanel = Panel("PausePanel"); AddTitle(pausePanel, "Paused", 500); Button(pausePanel, "Resume", 220, () => game.Resume()); Button(pausePanel, "Restart", 70, () => game.Restart()); Button(pausePanel, "Home", -80, () => game.Home()); Button(pausePanel, "Sound: toggle", -230, () => { game.Save.SoundEnabled = !game.Save.SoundEnabled; }); Button(pausePanel, "Haptic: toggle", -380, () => { game.Save.HapticEnabled = !game.Save.HapticEnabled; });
        }
        private void BuildWin()
        {
            winPanel = Panel("WinPanel"); AddTitle(winPanel, "Clinic Cleaned!", 560); winEarned = Label(winPanel, "Earned: 0", 330); winClean = Label(winPanel, "Clean score", 240); winPatients = Label(winPanel, "Patients", 150); Button(winPanel, "Double Coins Ad", -80, () => game.Level.DoubleRewardWithAd()); Button(winPanel, "Continue", -240, () => game.ContinueAfterWin());
        }
        private void BuildFail()
        {
            failPanel = Panel("FailPanel"); AddTitle(failPanel, "Clinic Failed", 520); failReason = Label(failPanel, "Reason", 300); Button(failPanel, "Revive Rewarded Ad", 80, () => game.Level.ReviveWithAd()); Button(failPanel, "Restart", -90, () => game.Restart()); Button(failPanel, "Home", -240, () => game.Home());
        }
        private void BuildUpgrade()
        {
            upgradePanel = Panel("UpgradePanel"); AddTitle(upgradePanel, "Upgrades", 720);
            UpgradeType[] types = { UpgradeType.VacuumPower, UpgradeType.Capacity, UpgradeType.MoveSpeed, UpgradeType.SuctionRadius };
            for (int i = 0; i < types.Length; i++) cards[i] = BuildCard(types[i], 430 - i * 230);
            Button(upgradePanel, "Back", -680, () => ShowHome());
        }
        private void BuildSettings()
        {
            settingsPanel = Panel("SettingsPanel"); AddTitle(settingsPanel, "Settings", 520); Button(settingsPanel, "Sound On/Off", 230, () => game.Save.SoundEnabled = !game.Save.SoundEnabled); Button(settingsPanel, "Haptic On/Off", 70, () => game.Save.HapticEnabled = !game.Save.HapticEnabled); Button(settingsPanel, "Back", -250, () => ShowHome());
        }
        private UpgradeCard BuildCard(UpgradeType type, float y)
        {
            GameObject row = new GameObject(type + "Card"); row.transform.SetParent(upgradePanel.transform, false); RectTransform rt = row.AddComponent<RectTransform>(); rt.sizeDelta = new Vector2(850, 180); rt.anchoredPosition = new Vector2(0, y); Image img = row.AddComponent<Image>(); img.color = new Color(1, 1, 1, .18f);
            Text name = Label(row, type.ToString(), 50, new Vector2(300, 60), new Vector2(-240, 0)); Text level = Label(row, "Lv", -15, new Vector2(250, 50), new Vector2(-230, 0)); Text cost = Label(row, "Cost", -70, new Vector2(250, 50), new Vector2(-230, 0)); Button(row, "Buy", -20, () => { if (game.Upgrades.TryBuy(type)) RefreshUpgrade(); }, new Vector2(220, 85), new Vector2(270, 0)); return new UpgradeCard { type = type, levelText = level, costText = cost };
        }
        private void RefreshHome() { homeCoins.text = "Coins: " + game.Save.Coins; homeLevel.text = "Current level: " + game.Save.CurrentLevel; }
        private void RefreshHud()
        {
            if (game == null || game.Level.CurrentLevel == null) return; LevelManager l = game.Level; PlayerRobotController p = l.Player;
            hudLevel.text = "Level " + l.CurrentLevel.levelIndex + " - " + l.CurrentLevel.displayName; hudCoins.text = "Coins: " + game.Save.Coins; cleanSlider.value = l.CleanPercent / 100f; hudPatients.text = "Patients healed: " + l.PatientsHealed + "/" + l.CurrentLevel.requiredPatientsHealed; hudTimer.text = "Time: " + Mathf.CeilToInt(l.TimeRemaining);
            if (p != null) { capacitySlider.value = p.Stats.capacity > 0 ? (float)p.CarriedAmount / p.Stats.capacity : 0; hudWaste.text = "Waste: " + p.CurrentWaste; hudMedical.text = "Medical: " + p.CurrentMedical; }
        }
        private void RefreshUpgrade() { for (int i = 0; i < cards.Length; i++) { cards[i].levelText.text = "Level: " + game.Save.GetUpgradeLevel(cards[i].type); cards[i].costText.text = "Cost: " + game.Upgrades.GetCost(cards[i].type); } RefreshHome(); }
        private void SetOnly(GameObject active) { GameObject[] panels = { homePanel, hudPanel, pausePanel, winPanel, failPanel, upgradePanel, settingsPanel }; for (int i = 0; i < panels.Length; i++) if (panels[i] != null) panels[i].SetActive(panels[i] == active); }
        private GameObject Panel(string name) { GameObject p = new GameObject(name); p.transform.SetParent(canvas.transform, false); RectTransform rt = p.AddComponent<RectTransform>(); rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero; Image bg = p.AddComponent<Image>(); bg.color = new Color(.05f, .08f, .1f, name == "GameplayHUD" ? .15f : .88f); return p; }
        private void AddTitle(GameObject parent, string text, float y) { Text t = Label(parent, text, y, new Vector2(900, 120)); t.fontSize = 58; t.fontStyle = FontStyle.Bold; }
        private Text Label(GameObject parent, string text, float y) { return Label(parent, text, y, new Vector2(760, 70)); }
        private Text Label(GameObject parent, string text, float y, Vector2 size, Vector2? xOffset = null)
        { GameObject go = new GameObject(text); go.transform.SetParent(parent.transform, false); RectTransform rt = go.AddComponent<RectTransform>(); rt.sizeDelta = size; rt.anchoredPosition = new Vector2(xOffset.HasValue ? xOffset.Value.x : 0, y); Text label = go.AddComponent<Text>(); label.text = text; label.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); label.alignment = TextAnchor.MiddleCenter; label.color = Color.white; label.fontSize = 38; return label; }
        private void Button(GameObject parent, string text, float y, UnityEngine.Events.UnityAction action, Vector2? size = null, Vector2? xOffset = null)
        { GameObject go = new GameObject(text + "Button"); go.transform.SetParent(parent.transform, false); RectTransform rt = go.AddComponent<RectTransform>(); rt.sizeDelta = size ?? new Vector2(620, 115); rt.anchoredPosition = new Vector2(xOffset.HasValue ? xOffset.Value.x : 0, y); Image img = go.AddComponent<Image>(); img.color = new Color(.1f, .55f, .85f, .95f); UnityEngine.UI.Button b = go.AddComponent<UnityEngine.UI.Button>(); b.onClick.AddListener(action); Text label = Label(go, text, 0, rt.sizeDelta); label.color = Color.white; }
        private UnityEngine.UI.Slider Slider(GameObject parent, string label, float y)
        { Label(parent, label, y + 40, new Vector2(760, 45)); GameObject go = new GameObject(label + "Slider"); go.transform.SetParent(parent.transform, false); RectTransform rt = go.AddComponent<RectTransform>(); rt.sizeDelta = new Vector2(760, 35); rt.anchoredPosition = new Vector2(0, y); UnityEngine.UI.Slider s = go.AddComponent<UnityEngine.UI.Slider>(); s.minValue = 0; s.maxValue = 1; GameObject bg = new GameObject("Background"); bg.transform.SetParent(go.transform, false); bg.AddComponent<Image>().color = Color.gray; RectTransform br = bg.GetComponent<RectTransform>(); br.anchorMin = Vector2.zero; br.anchorMax = Vector2.one; br.offsetMin = Vector2.zero; br.offsetMax = Vector2.zero; GameObject fill = new GameObject("Fill"); fill.transform.SetParent(go.transform, false); fill.AddComponent<Image>().color = Color.green; RectTransform fr = fill.GetComponent<RectTransform>(); fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one; fr.offsetMin = Vector2.zero; fr.offsetMax = Vector2.zero; s.fillRect = fr; return s; }
        private void AddRobotPreview(GameObject parent) { GameObject preview = new GameObject("Robot preview placeholder"); preview.transform.SetParent(parent.transform, false); RectTransform rt = preview.AddComponent<RectTransform>(); rt.sizeDelta = new Vector2(220, 220); rt.anchoredPosition = new Vector2(0, 360); Image img = preview.AddComponent<Image>(); img.color = new Color(.35f, .8f, 1f, .9f); }
        private struct UpgradeCard { public UpgradeType type; public Text levelText; public Text costText; }
    }
}
