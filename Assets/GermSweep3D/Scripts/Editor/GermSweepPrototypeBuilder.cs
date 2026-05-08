#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GermSweep3D.EditorTools
{
    public static class GermSweepPrototypeBuilder
    {
        [MenuItem("Germ Sweep 3D/Rebuild Main Scene And Placeholder Prefabs")]
        public static void Build()
        {
            Directory.CreateDirectory("Assets/GermSweep3D/Prefabs");
            Directory.CreateDirectory("Assets/GermSweep3D/Materials");
            string[] keys = { "PlayerRobot", "GreenGerm", "YellowGerm", "RedGerm", "MedicalSupply", "Coin", "BioWasteBin", "TreatmentStation", "Patient", "WetFloorHazard", "MovingGermCloud", "SanitizerBoost", "SanitizerCrystal", "GiantVirusBoss", "ExitZone" };
            for (int i = 0; i < keys.Length; i++) CreatePrefab(keys[i]);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            new GameObject("GameManager").AddComponent<GameManager>();
            GameObject level = new GameObject("LevelManager (runtime component lives on GameManager)"); level.transform.position = Vector3.zero;
            GameObject ui = new GameObject("UIManager (runtime Canvas is generated)"); ui.transform.position = Vector3.zero;
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube); floor.name = "ClinicFloor"; floor.transform.position = new Vector3(0, -.08f, 0); floor.transform.localScale = new Vector3(18, .1f, 20); floor.GetComponent<Renderer>().sharedMaterial = MaterialFor("ClinicFloor", new Color(.82f, .9f, .92f));
            Camera.main.transform.position = new Vector3(0, 9, -8); Camera.main.transform.rotation = Quaternion.Euler(55, 0, 0); Camera.main.gameObject.AddComponent<CameraFollow>();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Main.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Main.unity", true) };
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        }

        private static void CreatePrefab(string key)
        {
            GameObject go = GameObject.CreatePrimitive(key == "GiantVirusBoss" ? PrimitiveType.Sphere : key == "PlayerRobot" ? PrimitiveType.Capsule : PrimitiveType.Cube);
            go.name = key; go.GetComponent<Renderer>().sharedMaterial = MaterialFor(key, ColorFor(key));
            Collider collider = go.GetComponent<Collider>(); if (collider != null) collider.isTrigger = key != "PlayerRobot";
            if (key == "PlayerRobot") { Object.DestroyImmediate(collider); CharacterController cc = go.AddComponent<CharacterController>(); cc.radius = .45f; cc.height = 1f; cc.center = new Vector3(0, .5f, 0); go.AddComponent<PlayerRobotController>(); go.AddComponent<PlayerInputController>(); go.AddComponent<VacuumCollector>(); }
            else if ((key == "GreenGerm" || key == "YellowGerm" || key == "RedGerm") || key == "MedicalSupply" || key == "Coin" || key == "SanitizerCrystal") go.AddComponent<CollectibleItem>();
            else if (key == "BioWasteBin") go.AddComponent<BioWasteBin>(); else if (key == "TreatmentStation") go.AddComponent<TreatmentStation>(); else if (key == "Patient") go.AddComponent<Patient>();
            else if (key == "WetFloorHazard" || key == "MovingGermCloud") go.AddComponent<HazardZone>(); else if (key == "SanitizerBoost") go.AddComponent<PowerUp>(); else if (key == "GiantVirusBoss") go.AddComponent<GiantVirusBoss>();
            PrefabUtility.SaveAsPrefabAsset(go, "Assets/GermSweep3D/Prefabs/" + key + ".prefab"); Object.DestroyImmediate(go);
        }
        private static Material MaterialFor(string name, Color color)
        {
            string path = "Assets/GermSweep3D/Materials/" + name + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) { mat = new Material(Shader.Find("Standard")); mat.color = color; AssetDatabase.CreateAsset(mat, path); }
            return mat;
        }
        private static Color ColorFor(string key)
        {
            switch (key) { case "GreenGerm": return Color.green; case "YellowGerm": return Color.yellow; case "RedGerm": return Color.red; case "MedicalSupply": return new Color(.3f, .7f, 1f); case "Coin": return new Color(1f, .75f, .05f); case "BioWasteBin": return new Color(0, .25f, .08f); case "TreatmentStation": return Color.white; case "WetFloorHazard": return new Color(1f, .45f, .05f); case "MovingGermCloud": return new Color(1f, .15f, .05f); case "SanitizerBoost": return Color.cyan; case "SanitizerCrystal": return new Color(.6f, 1f, 1f); case "GiantVirusBoss": return new Color(.55f, .05f, .8f); default: return new Color(.35f, .8f, 1f); }
        }
    }
}
#endif
