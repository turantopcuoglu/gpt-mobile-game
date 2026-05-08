using UnityEngine;

namespace GermSweep3D
{
    public sealed class SaveSystem
    {
        private const string VersionKey = "GS3D_SaveVersion";
        private const int Version = 1;
        public void Initialize()
        {
            if (!PlayerPrefs.HasKey(VersionKey)) { PlayerPrefs.SetInt(VersionKey, Version); PlayerPrefs.SetInt("GS3D_CurrentLevel", 1); PlayerPrefs.SetInt("GS3D_Sound", 1); PlayerPrefs.SetInt("GS3D_Haptic", 1); PlayerPrefs.Save(); }
        }
        public int Coins { get { return PlayerPrefs.GetInt("GS3D_Coins", 0); } set { PlayerPrefs.SetInt("GS3D_Coins", Mathf.Max(0, value)); PlayerPrefs.Save(); } }
        public int CurrentLevel { get { return Mathf.Max(1, PlayerPrefs.GetInt("GS3D_CurrentLevel", 1)); } set { PlayerPrefs.SetInt("GS3D_CurrentLevel", Mathf.Max(1, value)); PlayerPrefs.Save(); } }
        public bool SoundEnabled { get { return PlayerPrefs.GetInt("GS3D_Sound", 1) == 1; } set { PlayerPrefs.SetInt("GS3D_Sound", value ? 1 : 0); PlayerPrefs.Save(); } }
        public bool HapticEnabled { get { return PlayerPrefs.GetInt("GS3D_Haptic", 1) == 1; } set { PlayerPrefs.SetInt("GS3D_Haptic", value ? 1 : 0); PlayerPrefs.Save(); } }
        public int GetUpgradeLevel(UpgradeType type) { return Mathf.Max(0, PlayerPrefs.GetInt("GS3D_Upgrade_" + type, 0)); }
        public void SetUpgradeLevel(UpgradeType type, int level) { PlayerPrefs.SetInt("GS3D_Upgrade_" + type, Mathf.Max(0, level)); PlayerPrefs.Save(); }
    }

    public sealed class UpgradeSystem
    {
        private readonly SaveSystem save;
        private const float Multiplier = 1.45f;
        public UpgradeSystem(SaveSystem save) { this.save = save; }
        public int GetBaseCost(UpgradeType type)
        { switch (type) { case UpgradeType.VacuumPower: return 75; case UpgradeType.Capacity: return 50; case UpgradeType.MoveSpeed: return 60; default: return 70; } }
        public int GetCost(UpgradeType type) { return Mathf.RoundToInt(GetBaseCost(type) * Mathf.Pow(Multiplier, save.GetUpgradeLevel(type))); }
        public bool TryBuy(UpgradeType type)
        { int cost = GetCost(type); if (save.Coins < cost) return false; save.Coins -= cost; save.SetUpgradeLevel(type, save.GetUpgradeLevel(type) + 1); return true; }
        public PlayerStats BuildStats()
        {
            return new PlayerStats
            {
                moveSpeed = 5f + save.GetUpgradeLevel(UpgradeType.MoveSpeed) * .35f,
                suctionRadius = 2.2f + save.GetUpgradeLevel(UpgradeType.SuctionRadius) * .18f,
                suctionPower = 1 + save.GetUpgradeLevel(UpgradeType.VacuumPower),
                capacity = 10 + save.GetUpgradeLevel(UpgradeType.Capacity) * 3
            };
        }
    }

    public interface IAdService
    {
        bool IsRewardedReady(); bool IsInterstitialReady(); void ShowRewardedAd(System.Action<bool> onCompleted); void ShowInterstitial(System.Action onClosed);
    }
    public sealed class MockAdService : IAdService
    {
        public bool IsRewardedReady() { return true; }
        public bool IsInterstitialReady() { return true; }
        public void ShowRewardedAd(System.Action<bool> onCompleted) { Debug.Log("[MockAdService] Rewarded ad completed successfully."); if (onCompleted != null) onCompleted(true); }
        public void ShowInterstitial(System.Action onClosed) { Debug.Log("[MockAdService] Interstitial closed."); if (onClosed != null) onClosed(); }
    }

    public sealed class AudioService
    {
        private readonly SaveSystem save; public AudioService(SaveSystem save) { this.save = save; }
        public void PlayCollect() { if (save.SoundEnabled) Debug.Log("[Audio] collect"); }
        public void PlayDrop() { if (save.SoundEnabled) Debug.Log("[Audio] drop"); }
        public void PlayHeal() { if (save.SoundEnabled) Debug.Log("[Audio] heal"); }
        public void PlayWin() { if (save.SoundEnabled) Debug.Log("[Audio] win"); }
        public void PlayLose() { if (save.SoundEnabled) Debug.Log("[Audio] lose"); }
    }
    public sealed class HapticService
    {
        private readonly SaveSystem save; public HapticService(SaveSystem save) { this.save = save; }
        public void Light() { if (save.HapticEnabled) Debug.Log("[Haptic] light"); }
        public void Medium() { if (save.HapticEnabled) Debug.Log("[Haptic] medium"); }
        public void Heavy() { if (save.HapticEnabled) Debug.Log("[Haptic] heavy"); }
    }
}
