using System.Collections.Generic;
using UnityEngine;

namespace GermSweep3D
{
    public sealed class LevelManager : MonoBehaviour
    {
        public int PatientsHealed { get; private set; }
        public float CleanPercent { get { return currentLevel == null || runtimeCleanUnits <= 0 ? 100f : Mathf.Clamp01((float)deliveredCleanUnits / runtimeCleanUnits) * 100f; } }
        public int LastReward { get; private set; }
        public string FailReason { get; private set; }
        public PlayerRobotController Player { get; private set; }
        public event System.Action HudChanged;

        private GameManager game;
        private LevelDatabase database;
        private PrefabFactory factory;
        private ObjectPool pool;
        private Transform levelRoot;
        private LevelDefinition currentLevel;
        private readonly List<Patient> patients = new List<Patient>();
        private readonly List<PooledEntry> spawnedObjects = new List<PooledEntry>();
        private TreatmentStation station;
        private GiantVirusBoss boss;
        private int deliveredCleanUnits;
        private int runtimeCleanUnits;
        private int collectedCoins;
        private int crystalCount;
        private float timeRemaining;
        private bool gameplayEnabled;
        private bool rewardGranted;
        private bool doubleClaimed;

        public void Initialize(GameManager manager)
        {
            game = manager; database = new LevelDatabase(); factory = new PrefabFactory();
            if (levelRoot == null) levelRoot = new GameObject("ActiveLevelRoot").transform;
            pool = new ObjectPool(levelRoot); SetupCameraAndFloor();
        }
        public void LoadLevel(int index)
        {
            ClearLevel(); currentLevel = database.GetLevel(index); timeRemaining = currentLevel.timeLimit; deliveredCleanUnits = 0; runtimeCleanUnits = currentLevel.totalCleanUnits; collectedCoins = 0; PatientsHealed = 0; crystalCount = 0; rewardGranted = false; doubleClaimed = false; FailReason = string.Empty;
            SpawnStatic("BioWasteBin", new Vector3(-3, 0, 4)).GetComponent<BioWasteBin>().Initialize(this);
            station = SpawnStatic("TreatmentStation", new Vector3(3, 0, 4)).GetComponent<TreatmentStation>(); station.Initialize(this);
            Player = Instantiate(factory.GetPrefab("PlayerRobot"), currentLevel.playerStartPosition, Quaternion.identity, levelRoot).GetComponent<PlayerRobotController>(); Player.gameObject.SetActive(true); Player.Initialize(game.Upgrades.BuildStats(), currentLevel.playerStartPosition); Player.InventoryChanged += NotifyHud; Player.GetComponent<VacuumCollector>().Initialize(Player);
            Camera.main.GetComponent<CameraFollow>().target = Player.transform;
            SpawnCollectibles(currentLevel.wasteSpawns, CollectibleType.Waste); SpawnCollectibles(currentLevel.medicalSpawns, CollectibleType.Medical); SpawnCollectibles(currentLevel.coinSpawns, CollectibleType.Coin);
            for (int i = 0; i < currentLevel.patientSpawns.Count; i++) { Patient p = SpawnStatic("Patient", currentLevel.patientSpawns[i].position).GetComponent<Patient>(); p.Initialize(this, currentLevel.patientSpawns[i]); patients.Add(p); }
            for (int i = 0; i < currentLevel.hazardSpawns.Count; i++) SpawnHazard(currentLevel.hazardSpawns[i]);
            if (currentLevel.hasBoss) { boss = SpawnStatic(currentLevel.bossConfig.bossKey, currentLevel.bossConfig.position).GetComponent<GiantVirusBoss>(); boss.Initialize(this, currentLevel.bossConfig); }
            NotifyHud();
        }
        public void ClearLevel()
        {
            gameplayEnabled = false; patients.Clear(); station = null; boss = null; Player = null;
            for (int i = 0; i < spawnedObjects.Count; i++) pool.Release(spawnedObjects[i].key, spawnedObjects[i].gameObject);
            spawnedObjects.Clear();
            if (levelRoot != null) for (int i = levelRoot.childCount - 1; i >= 0; i--) if (levelRoot.GetChild(i).gameObject.activeSelf) Destroy(levelRoot.GetChild(i).gameObject);
        }
        public void SetGameplayEnabled(bool enabled) { gameplayEnabled = enabled; if (Player != null) Player.SetInputEnabled(enabled); }
        private void Update()
        {
            if (!gameplayEnabled || currentLevel == null) return;
            timeRemaining -= Time.deltaTime; if (timeRemaining <= 0) { FailLevel("Timer expired"); return; }
            if (GoalsComplete()) CompleteLevel(); NotifyHud();
        }
        public float TimeRemaining { get { return Mathf.Max(0, timeRemaining); } }
        public int CollectedCoins { get { return collectedCoins; } }
        public LevelDefinition CurrentLevel { get { return currentLevel; } }
        public void CollectItem(CollectibleItem item, PlayerRobotController player)
        {
            switch (item.type)
            {
                case CollectibleType.Waste: player.AddWaste(item.amountValue); break;
                case CollectibleType.Medical: player.AddMedical(item.amountValue); break;
                case CollectibleType.Coin: collectedCoins += item.amountValue; game.Save.Coins += item.amountValue; break;
                case CollectibleType.SanitizerCrystal: crystalCount += item.amountValue; if (boss != null && crystalCount >= currentLevel.bossConfig.crystalsPerDamage) { crystalCount = 0; boss.Damage(1); } break;
            }
            game.Audio.PlayCollect(); game.Haptics.Light(); NotifyHud();
        }
        public void DeliverWaste(int amount) { deliveredCleanUnits += amount; game.Audio.PlayDrop(); NotifyHud(); }
        public void TryHealPatients(TreatmentStation source) { for (int i = 0; i < patients.Count; i++) if (patients[i].TryHeal(source)) break; NotifyHud(); }
        public void PatientHealed(Patient patient) { PatientsHealed++; game.Audio.PlayHeal(); game.Haptics.Medium(); NotifyHud(); }
        public void BossDefeated() { collectedCoins += 25; game.Save.Coins += 25; NotifyHud(); }
        public void SpawnBossGerm(Vector3 position) { GameObject go = SpawnStatic("GreenGerm", new Vector3(position.x, 0, position.z)); go.GetComponent<CollectibleItem>().Initialize(this, "GreenGerm", CollectibleType.Waste, 1, 1); runtimeCleanUnits += 1; }
        public void FailLevel(string reason) { if (game.State == GameState.Lose || game.State == GameState.Win) return; FailReason = reason; game.ChangeState(GameState.Lose); }
        public void ReviveWithAd()
        { if (!game.Ads.IsRewardedReady()) return; game.Ads.ShowRewardedAd(success => { if (success) { timeRemaining += 30; game.ChangeState(GameState.Gameplay); } }); }
        public void DoubleRewardWithAd()
        { if (doubleClaimed || !game.Ads.IsRewardedReady()) return; game.Ads.ShowRewardedAd(success => { if (success) { game.Save.Coins += LastReward; LastReward *= 2; doubleClaimed = true; game.UI.ShowWin(LastReward, CleanPercent, PatientsHealed); } }); }
        private bool GoalsComplete() { return CleanPercent >= currentLevel.requiredCleanPercent && PatientsHealed >= currentLevel.requiredPatientsHealed && (!currentLevel.hasBoss || boss == null || boss.Health <= 0); }
        private void CompleteLevel()
        { if (rewardGranted) return; rewardGranted = true; LastReward = currentLevel.baseCoinReward + collectedCoins + Mathf.RoundToInt(CleanPercent) + PatientsHealed * 50; game.Save.Coins += LastReward; game.ChangeState(GameState.Win); }
        private void SpawnCollectibles(List<SpawnData> spawns, CollectibleType fallbackType)
        { for (int i = 0; i < spawns.Count; i++) { SpawnData s = spawns[i]; if (s.prefabKey == "Stain") { Stain stain = SpawnStatic("Stain", s.position).GetComponent<Stain>(); stain.Initialize(this, s.amountValue); } else { CollectibleType type = s.prefabKey == "SanitizerCrystal" ? CollectibleType.SanitizerCrystal : fallbackType; SpawnStatic(s.prefabKey, s.position).GetComponent<CollectibleItem>().Initialize(this, s.prefabKey, type, s.amountValue, s.requiredVacuumPower); } } }
        private GameObject SpawnStatic(string key, Vector3 position) { GameObject prefab = factory.GetPrefab(key); GameObject go = pool.Get(key, prefab, position + Vector3.up * .5f, Quaternion.identity); go.name = key; spawnedObjects.Add(new PooledEntry { key = key, gameObject = go }); return go; }
        private void SpawnHazard(HazardSpawnData spawn) { SpawnStatic(spawn.hazardKey, spawn.position); }
        private void NotifyHud() { if (HudChanged != null) HudChanged(); }
        private struct PooledEntry { public string key; public GameObject gameObject; }
        private void SetupCameraAndFloor()
        {
            if (Camera.main == null) { GameObject cam = new GameObject("Main Camera"); cam.tag = "MainCamera"; cam.AddComponent<Camera>(); cam.AddComponent<AudioListener>(); }
            if (Camera.main.GetComponent<CameraFollow>() == null) Camera.main.gameObject.AddComponent<CameraFollow>(); Camera.main.transform.position = new Vector3(0, 9, -8); Camera.main.transform.rotation = Quaternion.Euler(55, 0, 0);
            if (GameObject.Find("ClinicFloor") == null) { GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube); floor.name = "ClinicFloor"; floor.transform.position = new Vector3(0, -.08f, 0); floor.transform.localScale = new Vector3(18, .1f, 20); floor.GetComponent<Renderer>().material.color = new Color(.82f, .9f, .92f); }
        }
    }
}
