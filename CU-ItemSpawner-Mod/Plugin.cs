using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using BepInEx;
using HarmonyLib;


namespace CU_ItemSpawner_Mod
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class CU_ItemSpawner_Mod : BaseUnityPlugin
    {
        
        public bool showGUI = false;
        public Rect windowRect = new Rect(200, 100, 500, 500);

        public PlayerCamera playerCamera;

        private string searchQuery = "";
        private Vector2 scrollPos;
        private float durability = 1.0f;
        private int quantity = 1;
        private bool spawnAtMouse = false;

        private float playerSpeed = 25f;
        private float jumpForce = 26f;
        private float maxForce = 4000f;

        private List<string> allItems;
        private List<string> filteredItems;

        private void Start()
        {
            var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            if (!playerCamera)
            {
                playerCamera = UnityEngine.Component.FindObjectOfType<PlayerCamera>();
            }

            allItems = Resources.LoadAll<GameObject>("")
                .Select(o => o.name)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            filteredItems = new List<string>(allItems);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
                showGUI = !showGUI;

            if (!playerCamera)
            {
                playerCamera = UnityEngine.Component.FindObjectOfType<PlayerCamera>();
            }
        }


        private void OnGUI()
        {
            if (!showGUI)
                return;

            GUI.backgroundColor = new Color(.45f, .45f, .45f, 0.5f);
            GUI.color = Color.red;
            windowRect = GUI.Window(12345, windowRect, DrawWindow, "Spawn Menu");
        }


        private void DrawWindow(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            GUILayout.Label("Search:");
            string newSearch = GUILayout.TextField(searchQuery);
            if (newSearch != searchQuery)
            {
                searchQuery = newSearch;
                filteredItems = allItems
                    .Where(i => i.ToLower().Contains(searchQuery.ToLower()))
                    .ToList();
            }

            GUILayout.Label("Durability:");
            durability = GUILayout.HorizontalSlider(durability, 0f, 1f);
            GUILayout.Label($"{durability:F2}");

            GUILayout.Label("Amount:");
            int.TryParse(GUILayout.TextField(quantity.ToString()), out quantity);
            if (quantity < 1) quantity = 1;

            spawnAtMouse = GUILayout.Toggle(spawnAtMouse, "Spawn under mouse");

            GUILayout.Space(5);
            GUILayout.Label("Items:");
            scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.Height(250));

            foreach (var item in filteredItems.Take(200))
            {
                if (GUILayout.Button(item))
                {
                    for (int i = 0; i < quantity; i++)
                    {
                        SpawnItem(item);
                    }
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(100));

            GUILayout.Label("<b>Player settings</b>");
            GUILayout.Space(10);

            GUILayout.Label("Walk/Run speed");
            playerSpeed = GUILayout.HorizontalSlider(playerSpeed, 5f, 300f);
            maxForce = playerSpeed * 160f;
            if (playerCamera && playerCamera.body != null)
            {
                playerCamera.body.maxSpeed = playerSpeed;
                playerCamera.body.moveForce = maxForce;
            }
             GUILayout.Label($"{playerSpeed:F1}");

            GUILayout.Label("Jump Force");
            jumpForce = GUILayout.HorizontalSlider(jumpForce, 5f, 100f);
            playerCamera.body.jumpSpeed = jumpForce;

            GUILayout.Label($"{jumpForce:F1}");

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUI.DragWindow();

            if (Event.current.isMouse && windowRect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
            }

        }

        private void SpawnItem(string resourceName)
        {
            GameObject prefab = Resources.Load<GameObject>(resourceName);
            if (!prefab)
            {
                Logger.LogWarning($"No results found for '{resourceName}'");
                return;
            }

            Vector3 pos;
            if (spawnAtMouse)
            {
                pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            else
            {
                pos = UnityEngine.GameObject.Find("Body").transform.position + new Vector3(0, 2, 0);
            }

            pos.z = 0;
            GameObject obj = Instantiate(prefab, pos, Quaternion.identity);

            var item = obj.GetComponent<Item>();
            if (item)
                item.condition = durability;

            var ammo = obj.GetComponent<AmmoScript>();
            if (ammo && ammo.itemType == AmmoScript.AmmoItemType.Magazine)
                ammo.rounds = ammo.maxRounds;
        }
    }

    [HarmonyPatch(typeof(PlayerCamera))]
    public class BlockInput
    {
        [HarmonyPatch("HandleAttacks")]
        [HarmonyPrefix]
        public static bool Prefix()
        {
            var plugin = BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent<CU_ItemSpawner_Mod>();
            if (plugin == null)
                return true;

            if (plugin.showGUI && IsMouseOverWindow(plugin))
            {
                return false;
            }

            return true;
        }

        private static bool IsMouseOverWindow(CU_ItemSpawner_Mod plugin)
        {
            Vector2 mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y;
            return plugin.windowRect.Contains(mousePos);
        }
    }
}
