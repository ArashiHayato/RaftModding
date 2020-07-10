using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using FMOD.Studio;
using Harmony;



using I2.Loc;
using Steamworks;
using System;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using TMPro;

using UnityEngine.UI;


[ModTitle("Arashi's Raft Fishing Expansion")] // The mod name.
[ModDescription("This mod currently adds new rare loot for fishing.")] // Short description for the mod.
[ModAuthor("Arashi")] // The author name of the mod.
[ModIconUrl("https://imgur.com/MXK28Z5.jpg")] // An icon for your mod. Its recommended to be 128x128px and in .jpg format.
[ModWallpaperUrl("https://imgur.com/Bb5M1Jh.jpg")] // A banner for your mod. Its recommended to be 330x100px and in .jpg format.
[ModVersionCheckUrl("")] // This is for update checking. Needs to be a .txt file with the latest mod version.
[ModVersion("1.0")] // This is the mod version.
[RaftVersion("Update 11")] // This is the recommended raft version.
[ModIsPermanent(true)] // If your mod add new blocks, new items or just content you should set that to true. It loads the mod on start and prevents unloading.
public class Arashi_RaftFishingExpansion : Mod
{



    RBlockQuadType[] StandardQuadType = new RBlockQuadType[] { RBlockQuadType.quad_floor, RBlockQuadType.quad_foundation, RBlockQuadType.quad_table };
    //SO_BlockQuadType tablequadtype;
    HarmonyInstance harmonyInstance;
    string harmonyID = "com.arashi.fishingexpansion";

    //Asset Files
    public static AssetBundle ScrapMechanicTrophies;

    //Audio Bank
    public static List<AudioClip> sound_FX = new List<AudioClip>();
    public static List<AudioClip> music = new List<AudioClip>();

    //Graphics Storage
    public static List<GameObject> particles = new List<GameObject>();

    //Game Objects
    public static List<GameObject> object_3D = new List<GameObject>();
    public static List<Sprite> object_2D = new List<Sprite>();
    

    //Loot Tables
    public static SO_RandomDropper TestDropper, FishingRod_Dropper;



    //Game Managers
    private static Network_Player playerNetwork;
    public static ItemManager itemManager;
    public static ResourceManager resourceManager;
    public static List<Item_Base> usableItems;
    public static List<Item_Base> allItems;

    public async void Start()
    {
        harmonyInstance = HarmonyInstance.Create(harmonyID);
        harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());


        ComponentManager<Arashi_RaftFishingExpansion>.Value = this;





        var bundleLoadRequest = AssetBundle.LoadFromFileAsync("mods\\ModData\\Arashi_RaftFishingExpansion\\scrapmechanic_trophies.assets");
        while (!bundleLoadRequest.isDone) { await Task.Delay(1); } //Prevents assigning ScrapMechanicTrohpies before asset bundle is loaded
        ScrapMechanicTrophies = bundleLoadRequest.assetBundle;


        //Register new items

        RegisterPlaceableItem((Item_Base)await GetAssetFromTargetBundle(ScrapMechanicTrophies, "Placeable_ScrapMechanicTrophy_CraftBot"), StandardQuadType);
        RegisterPlaceableItem((Item_Base)await GetAssetFromTargetBundle(ScrapMechanicTrophies, "Placeable_ScrapMechanicTrophy_BananaCrate"), StandardQuadType);
   
        RegisterPlaceableItem((Item_Base)await GetAssetFromTargetBundle(ScrapMechanicTrophies, "Placeable_ScrapMechanicTrophy_BlueBerryCrate"), StandardQuadType);
             RegisterPlaceableItem((Item_Base)await GetAssetFromTargetBundle(ScrapMechanicTrophies, "Placeable_ScrapMechanicTrophy_BroccoliCrate"), StandardQuadType);
           RegisterPlaceableItem((Item_Base)await GetAssetFromTargetBundle(ScrapMechanicTrophies, "Placeable_ScrapMechanicTrophy_Bucket"), StandardQuadType);
           RegisterPlaceableItem((Item_Base)await GetAssetFromTargetBundle(ScrapMechanicTrophies, "Placeable_ScrapMechanicTrophy_CarrotCrate"), StandardQuadType);
           RegisterPlaceableItem((Item_Base)await GetAssetFromTargetBundle(ScrapMechanicTrophies, "Placeable_ScrapMechanicTrophy_CookBot"), StandardQuadType);
           RegisterPlaceableItem((Item_Base)await GetAssetFromTargetBundle(ScrapMechanicTrophies, "Placeable_ScrapMechanicTrophy_CowCrate"), StandardQuadType);
      
           RegisterPlaceableItem((Item_Base)await GetAssetFromTargetBundle(ScrapMechanicTrophies, "Placeable_ScrapMechanicTrophy_GlowGorp"), StandardQuadType);
           RegisterPlaceableItem((Item_Base)await GetAssetFromTargetBundle(ScrapMechanicTrophies, "Placeable_ScrapMechanicTrophy_OrangeCrate"), StandardQuadType);
           RegisterPlaceableItem((Item_Base)await GetAssetFromTargetBundle(ScrapMechanicTrophies, "Placeable_ScrapMechanicTrophy_PineappleCrate"), StandardQuadType);
           RegisterPlaceableItem((Item_Base)await GetAssetFromTargetBundle(ScrapMechanicTrophies, "Placeable_ScrapMechanicTrophy_RedBeetCrate"), StandardQuadType);
           RegisterPlaceableItem((Item_Base)await GetAssetFromTargetBundle(ScrapMechanicTrophies, "Placeable_ScrapMechanicTrophy_TomatoCrate"), StandardQuadType);
   

        //Set Loot Tables
        TestDropper = (SO_RandomDropper)await GetAssetFromTargetBundle(ScrapMechanicTrophies, "Dropper_Arashi_ScrapTrophies");
       FishingRod_Dropper = (SO_RandomDropper)await GetAssetFromTargetBundle(ScrapMechanicTrophies, "Dropper_Arashi_ScrapTrophies");




        // SetBobberLoot();


    }


    public override void WorldEvent_WorldLoaded()
    {
        if (playerNetwork == null) { playerNetwork = RAPI.GetLocalPlayer(); }
        //AddLandmarkToSpawner();
        //Debug.Log("The world is loaded!");
    }



    // The Update() method is being called every frame. Have fun!
    public void Update()
    {


        //   if (Semih_Network.InLobbyScene) { return; } //Prevents code below being run if in lobby scene





    }


    public static async Task<object> GetAssetFromTargetBundle(AssetBundle sourceBundle, string name)
    {
        AssetBundleRequest requestedAsset = sourceBundle.LoadAssetAsync(name);
        while (!requestedAsset.isDone) { await Task.Delay(1); } //ensures asset is not returned until it is ready




        return requestedAsset.asset;
    }


    int currentMenuOffset = 1;
    int line = 1;
    public void RegisterPlaceableItem(Item_Base item, RBlockQuadType[] quadtypes)
    {
        Traverse.Create(item.settings_recipe).Field("subCategory").SetValue(line.ToString());
        if (currentMenuOffset == 4)
        {
            currentMenuOffset = 0;
            line++;
        }
        currentMenuOffset++;
        RAPI.RegisterItem(item);
        foreach (RBlockQuadType rb in quadtypes)
        {
            RAPI.AddItemToBlockQuadType(item, rb);
        }
    }

     
    



    [HarmonyPatch(typeof(Bobber)), HarmonyPatch("GetRandomFishingItem")]
    public class FishingRodDropperPatch
    {
        private static void Postfix(ref Item_Base __result )
        {
            if (__result != null)
            {
                Network_Player player = RAPI.GetLocalPlayer();
                Item_Base currentItemInHand = player.PlayerItemManager.useItemController.GetCurrentItemInHand();

                System.Random rand = new System.Random();
                string oldItem = __result.UniqueName;
                if (oldItem == "Placeable_ScrapMechanic" || oldItem == "Placeable_LuckyCat" || oldItem == "Placeable_Cropplot_Shoe" || oldItem == "Placeable_GlassCandle")
                {
                    Debug.Log("Item Randomizer Activated!");
                    if (currentItemInHand.UniqueName == "FishingRod" && rand.Next(0, 10) > 7) { __result = FishingRod_Dropper.GetRandomItem(); }
                    else if (currentItemInHand.UniqueName == "FishingRod_Metal" && rand.Next(0, 10) < 8) { __result = FishingRod_Dropper.GetRandomItem(); }
                }
            }
        }
    }




    /*
    public void OnConsumeItem(Item_Base item) 
        {


        }


        [HarmonyPatch(typeof(PlayerStats)), HarmonyPatch("Consume")] //This patch appends OnConsumeItem to the edibleItem event
        internal class ConsumePatch
        {
            private static void Postfix(PlayerStats __instance, Item_Base edibleItem)
            {
                if (__instance.GetComponent(typeof(Network_Player)) == RAPI.GetLocalPlayer())//check if local player?
                {
                    ComponentManager<Arashi_RaftFishingExpansion>.Value.OnConsumeItem(edibleItem);
                }
            }
        }
       */

  






    // The OnModUnload() method is being called when your mod gets unloaded.
    public void OnModUnload()
    {
        RConsole.Log("Arashi_RaftFishingExpansion has been unloaded!");
        Destroy(gameObject); // Please do not remove that line!
    }










}
