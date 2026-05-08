using System.Collections.Generic;
using UnityEngine;

namespace GermSweep3D
{
    public sealed class PrefabFactory
    {
        private readonly Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        public GameObject GetPrefab(string key)
        {
            GameObject prefab; if (prefabs.TryGetValue(key, out prefab)) return prefab;
            prefab = BuildPrefab(key); prefabs.Add(key, prefab); return prefab;
        }
        private GameObject BuildPrefab(string key)
        {
            if (key == "PlayerRobot") return BuildPlayer();
            PrimitiveType primitive = key == "GiantVirusBoss" ? PrimitiveType.Sphere : PrimitiveType.Cube;
            GameObject go = GameObject.CreatePrimitive(primitive); go.name = key + "Prefab"; Object.DontDestroyOnLoad(go); go.SetActive(false);
            Renderer r = go.GetComponent<Renderer>(); r.sharedMaterial = Mat(ColorFor(key));
            Collider c = go.GetComponent<Collider>(); c.isTrigger = true;
            if ((key == "GreenGerm" || key == "YellowGerm" || key == "RedGerm") || key == "MedicalSupply" || key == "Coin" || key == "SanitizerCrystal") go.AddComponent<CollectibleItem>();
            else if (key == "Stain") { go.transform.localScale = new Vector3(1.5f, .08f, 1.5f); go.AddComponent<Stain>(); }
            else if (key == "BioWasteBin") { go.transform.localScale = new Vector3(1.4f, 1.2f, 1.4f); go.AddComponent<BioWasteBin>(); }
            else if (key == "TreatmentStation") { go.transform.localScale = new Vector3(1.4f, 1.2f, 1.4f); go.AddComponent<TreatmentStation>(); }
            else if (key == "Patient") { go.transform.localScale = new Vector3(.9f, 1.4f, .9f); go.AddComponent<Patient>(); }
            else if (key == "WetFloorHazard" || key == "MovingGermCloud") { go.transform.localScale = new Vector3(2f, .1f, 2f); HazardZone h = go.AddComponent<HazardZone>(); h.hazardKey = key; h.moveAmplitude = key == "MovingGermCloud" ? 2f : 0f; }
            else if (key == "SanitizerBoost") { go.transform.localScale = Vector3.one * .8f; go.AddComponent<PowerUp>(); }
            else if (key == "GiantVirusBoss") { go.transform.localScale = Vector3.one * 2f; go.AddComponent<GiantVirusBoss>(); }
            else if (key == "ExitZone") go.transform.localScale = new Vector3(2f, .1f, 2f);
            return go;
        }
        private GameObject BuildPlayer()
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule); go.name = "PlayerRobotPrefab"; Object.DontDestroyOnLoad(go); go.SetActive(false);
            go.GetComponent<Renderer>().sharedMaterial = Mat(new Color(.35f, .8f, 1f)); Object.Destroy(go.GetComponent<Collider>());
            CharacterController cc = go.AddComponent<CharacterController>(); cc.radius = .45f; cc.height = 1f; cc.center = new Vector3(0, .5f, 0);
            go.AddComponent<PlayerRobotController>(); go.AddComponent<PlayerInputController>(); go.AddComponent<VacuumCollector>();
            return go;
        }
        private Material Mat(Color color)
        {
            string key = color.ToString(); Material m; if (materials.TryGetValue(key, out m)) return m;
            m = new Material(Shader.Find("Standard")); m.color = color; materials.Add(key, m); return m;
        }
        private static Color ColorFor(string key)
        {
            switch (key) { case "GreenGerm": return Color.green; case "YellowGerm": return Color.yellow; case "RedGerm": return Color.red; case "MedicalSupply": return new Color(.3f, .7f, 1f); case "Coin": return new Color(1f, .75f, .05f); case "BioWasteBin": return new Color(0, .25f, .08f); case "TreatmentStation": return Color.white; case "WetFloorHazard": return new Color(1f, .45f, .05f); case "MovingGermCloud": return new Color(1f, .15f, .05f); case "SanitizerBoost": return Color.cyan; case "SanitizerCrystal": return new Color(.6f, 1f, 1f); case "GiantVirusBoss": return new Color(.55f, .05f, .8f); case "Stain": return new Color(.35f, .2f, .05f); default: return Color.gray; }
        }
    }
}
