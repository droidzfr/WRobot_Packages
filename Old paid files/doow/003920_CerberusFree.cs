using System;
using System.ComponentModel;
using System.Collections.Generic;
//using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using robotManager;
//using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using robotManager.Products;
using wManager;
using wManager.Plugin;
using wManager.Wow;
using wManager.Wow.Bot.States;
//using wManager.Wow.Class;
using wManager.Wow.Enums;
using wManager.Wow.Forms;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

public class Main : IPlugin
{
    private bool _isLaunched;
    private int _defaultMinSlots;
    private int _ammoBagId;

    private static readonly Dictionary<string, int> ItemsValue = new Dictionary<string, int>
    {
        //Trash
        { "Broken Antenna", 4 }, // Item and value of item     
        { "Broken Fang", 6 },
        { "Broken Vine", 4 },
        { "Broken Antler",  105},
        { "Cracked Egg Shells", 4 },
        { "Cracked Bill", 28},
        { "Small Egg", 4},
        { "Small Leather Collar", 33 },
        { "Discolored Fang", 6 },
        { "Chipped Claw", 4 },
        { "Enriched Lasher Root", 5 },
        { "Moongraze Stag Tenderloin", 5 },
        { "Plucked Feather", 5 },
        { "Ruffled Feather", 41 },
        { "Torn Moth Wing", 4 },
        { "Withered Lasher Root", 3 },
        //Armor
        { "Burnt Leather Belt", 26 },
        { "Burnt Leather Boots", 41 },
        { "Burnt Buckler", 53 },
        { "Burnt Cloak", 22 },
        { "Burnt Leather Bracers", 19 },
        { "Burnt Leather Gloves", 36 },
        { "Cadet Bracers", 73 },
        { "Flimsy Chain Belt", 1 },
        { "Flimsy Chain Boots", 3 },
        { "Flimsy Chain Cloak", 4 },
        { "Flimsy Chain Pants", 2 },
        { "Flimsy Chain Gloves", 2 },
        { "Flimsy Chain Bracers", 3 },
        { "Flimsy Chain Vest", 9 },
        { "Frayed Robe", 4 },
        { "Frayed Shoes", 3 },
        { "Frayed Gloves",  1 },
        { "Frayed Pants", 1 },
        { "Frayed Belt", 1 },
        { "Frayed Bracers", 3 },
        { "Frayed Cloak", 4 },
        { "Gypsy Sash", 57},
        { "Journeyman´s Belt", 23 },
        { "Journeyman´s Bracers", 15 },
        { "Loose Chain Cloak", 7 },
        { "Loose Chain Boots", 29 },
        { "Loose Chain Belt", 16 },
        { "Loose Chain Gloves", 13 },
        { "Loose Chain Pants", 37 },
        { "Loose Chain Vest", 58 },
        { "Loose Chain Bracers", 28 },
        { "Patchwork Armor", 14 },
        { "Patchwork Belt", 10 },
        { "Patchwork Bracers", 25 },
        { "Patchwork Shoes", 25 },
        { "Pathwork Pants", 20 },
        { "Pathwork Gloves", 7},
        { "Patchwork Cloak", 7 },
        { "Ragged Cloak", 2 },
        { "Ragged Leather Bracers", 2 },
        { "Ragged Leather Belt", 4 },
        { "Ragged Leather Boots", 4 },
        { "Ragged Leather Vest", 11 },
        { "Ragged Leather Pants", 3 },
        { "Recruit's Pants", 1 },
        { "Recruit's Boots", 1 },
        { "Recruit's Shirt", 1 },
        { "Worn Leather Belt", 9 },
        { "Worn Leather Bracers", 18 },
        { "Worn Leather Boots", 27 },
        { "Worn Leather Gloves", 6 },
        { "Worn Leather Pants", 19 },
        { "Worn Leather Vest", 52 },
        //Cloaks
        { "Beaded Cloak", 24 },
        { "Charger´s Cloak",  24},
        { "Warrior´s Cloak",  23},
        { "Primal Cape", 24 },
        //Bows
        { "Cracked Shortbow", 39 },
        { "Cadet's Bow", 28 },
        //Rifles
        { "Hunting Rifle", 79 },
        //Maces
        { "Beatstick", 99 },
        { "Carpenter´s Mallet", 72 },
        //Daggers
        { "Sharpened Letter Opener", 38 },
        //Swords 
        { "Crude Bastard Sword", 49 },
        { "Practice Sword",  71},
        { "Battleworn Claymore", 9 },
        //Staffs
        { "Withered Staff", 68 },
        //Shields
        { "Cracked Buckler", 16 },
        { "Bent Large Shield", 7 },
        { "Charger´s Shield", 52 },
        //Axes
        { "Beaten Battle Axe", 65 },
        { "Wood Chopper", 99 },
        { "Rusty Hatchet", 75 },
    };

    private static readonly Dictionary<string, string> Racials = new Dictionary<string, string>
    {
        {"Blood Elf","Arcane Torrent"}, // Race and Racial
        {"Troll","Berserking"},
        {"Orc","Blood Fury"},
        {"Tauren","War Stomp"},
        {"Undead","Will of the Forsaken"},
        {"Gnome","Escape Artist"},
        {"Human","Every Man for Himself"},
        {"Draenei","Gift of the Naaru"},
        {"Night Elf","Shadowmeld"},
        {"Dwarf","Stoneform"},
    };

    public void Initialize()
    {
        _isLaunched = true;
        CerberusSettings.Load();
        wManagerSetting.Load();
        Lua.LuaDoString("DEFAULT_CHAT_FRAME: AddMessage('|cFFFFCE2E -----------------------------------------------------------------------')");
        Lua.LuaDoString("DEFAULT_CHAT_FRAME: AddMessage('|cFFFFCE2E FreeVersion:|cff00FF7F    Cerberus  |cFFFFCE2E   Initializing |cffff0000 V1.0.0')");
        Lua.LuaDoString("DEFAULT_CHAT_FRAME: AddMessage('|cff00FF7F     The Guardian of Precious Loot |cFFFFCE2E by  |cff00FF7F Arrigato')");
        Lua.LuaDoString("DEFAULT_CHAT_FRAME: AddMessage('|cFFFFCE2E ----------------------------------------------------------------------')");
        Logging.Write("[Cerburus v1.0.0] Free Version by Arrigato, is Online--");
        LootGuardian();
        BotSettings();
        
        if (ObjectManager.Me.WowClass == WoWClass.Hunter)
        {
            _ammoBagId = GetAmmoContainerSlotId();
            _defaultMinSlots = wManagerSetting.CurrentSetting.MinFreeBagSlotsToGoToTown;
        }

        TownWatcher();
        WatchForLuaEvents();
        //  robotManager.Events.FiniteStateMachineEvents.OnRunState += OnRunState;       
    }

    public void Dispose()
    {
        wManagerSetting.CurrentSetting.MinFreeBagSlotsToGoToTown = _defaultMinSlots;
        _isLaunched = false;
        Logging.Write("[Cerburus v1.0.0] Free Version by Arrigato, has been stopped since wrobot is not running");
    }

    public void Settings()
    {
        CerberusSettings.Load();
        CerberusSettings.CurrentSetting.ToForm();
        CerberusSettings.CurrentSetting.Save();
    }

    private void TownWatcher()
    {
        robotManager.Events.FiniteStateMachineEvents.OnRunState += (engine, state, cancelable) =>
        {
            if (CerberusSettings.CurrentSetting.RacialUser)
            {
                RacialInitiator();
            }
            if (CerberusSettings.CurrentSetting.StartAreaHelper)
            {
                StartAreaHelper();
            }        
        };

        robotManager.Events.FiniteStateMachineEvents.OnAfterRunState += (engine, state) =>
        {

        };
    }

    private void WatchForLuaEvents()
    {
        var QuestItems = new List<string>()
        {
            //Vanilla
            "Tainted Arcane Sliver",
            "Flawless Draenethyst Sphere",
            "Imperfect Draenethyst Fragment",
            "Moss-Twined Heart",
            "Gold Pickup Schedule",
            "Westfall Deed",
            "Dargol's Skull",
            "Captain Sanders' Treasure Map",
            "Ursangous' Paw",
            "Shadumbra's Head",
            "Sharptalon's Claw",
            "Befouled Water Globe",
            "Grime-Encrusted Ring",
            "OOX-09/HL Distress Beacon",
            "OOX-22/FE Distress Beacon",
            "OOX-17/TN Distress Beacon",
            "Deadwood Ritual Totem",
            "Crudely-Written Log",
            "Brann Bronzebeard's Lost Letter",

            //TBC
            "Captain Kelisendra's Lost Rutters",
            "The Lady's Necklace",
            "Old Whitebark's Pendant",
            "Amani Invasion Plans",
            "Avruu's Orb",
            "Diabolical Plans",
            "Weathered Treasure Map",
            "Tzerak's Armor Plate",
            "Blood Elf Plans",
            "Withered Basidium",
            "'Count' Ungula's Mandible",
            "Dathric's Blade",
            "Belmara's Tome",
            "Luminrath's Mantle",
            "Cohlien's Cap",
            "Crimson Crystal Shard",
            "Burning Legion Missive",
            "Thunderlord Clan Artifact",
            "Illidari-Bane Shard",
            "Meeting Note",
            "Primed Key Mold",
            "Gorgrom's Favor",
            "Damaged Mask",
            "Orb of the Grishna",
            "Ishaal's Almanac",
            "Partially Digested Hand",
            "Murkblood Escape Plans",
            "Shards of Ahune",
            "'Brew of the Month' Club Membership Form",
            "Direbrew's Dire Brew",
            "Incriminating Documents",
            "Faintly Glowing Crystal",
            "Rune Covered Tablet",
            "Blood Elf Communication",
            "Gurf's Dignity",
            "Red Crystal Pendant",
            "A Letter from the Admiral",
            "Eroded Leather Case",
            "A Mysterious Tome",
            "Howling Wind",
            "Cabal Orders",
            "Murkblood Invasion Plans",
            "Vial of Void Horror Ooze",
            "Strange Engine Part",
            "The Journal of Val'zareq",

            //WotLK
            "Gjalerbron Attack Plans",
            "Vrykul Scroll of Ascension",
            "Scourge Device",
            "Mezhen's Writings",
            "Vial of Fresh Blood",
            "The Ultrasonic Screwdriver",
            "Goramosh's Strange Device",
            "Captain Malin's Letter",
            "Lieutenant Ta'zinni's Letter",
            "Emblazoned Battle Horn",
            "Mikhail's Journal",
            "The Favor of Zangus",
            "Torturer's Rod",
            "Ruby Brooch",
            "Strange Mojo",
            "Note from the Grand Admiral",
            "SCRAP-E Access Card",
            "Slag Covered Metal",
            "Dark Armor Plate",
            "Dr. Terrible's 'Building a Better Flesh Giant'",
            "Jagged Shard",
            "Faded Lovely Greeting Card",
            "Kvaldir Attack Plans",
            "Ith'rix's Hardened Carapace",
            "Scintillating Fragment",
            "Writhing Choker",
            "Flesh-Bound Tome",
            "Everfrost Chip",
            "Unliving Choker",
            "Damaged Necklace",
            "Sealed Vial of Poison",
            "Waterlogged Recipe",
        };

        List<string> Profiles = new List<string>()
        {
        "BambosShadowfangFarmer",
        "BambosAuchenaiCryptsFarmer"
        };

        EventsLuaWithArgs.OnEventsLuaWithArgs += (LuaEventsId id, List<string> args) =>
        {
            if (id ==  LuaEventsId.LOOT_OPENED)
            {
                if (ObjectManager.Me.WowClass == WoWClass.Hunter)
                {
                    ManageAmmo();
                }
            }
            if (id == LuaEventsId.LOOT_SLOT_CLEARED)
            {
                if (CerberusSettings.CurrentSetting.ItemOpener)
                {
                    ItemOpener();
                }
                if (CerberusSettings.CurrentSetting.DeleteQuestItems)
                {
                    foreach (var QuestItem in QuestItems)
                    {
                        DeleteItems(QuestItem, 0);
                    }
                }        
            }
            if (id == LuaEventsId.LOOT_CLOSED)
            {
                if (CerberusSettings.CurrentSetting.StartAreaHelper)
                {
                    Logging.Write(String.Format("{0: [Cerberus Bag Watcher] Your Bag Value is #### Gold, ## Silver, and ## Copper}", BagValue()));
                    Logging.Write(String.Format("{0: [Cerberus Bag Watcher] Your Money Value is #### Gold, ## Silver, and ## Copper}", CurrentMoney()));
                    Logging.Write(String.Format("{0: [Cerberus Bag Watcher] Your Total Value is #### Gold, ## Silver, and ## Copper}", TotalMoney()));
                }
            }
            if (id == LuaEventsId.BAG_UPDATE)
            {
                ScrollUser();
            }
            if (!Profiles.Any(x => Equals(Products.ProductSettings.Name) && CerberusSettings.CurrentSetting.BotSettings))
            {
                if (id == LuaEventsId.PLAYER_LEVEL_UP)
                {
                    CheckLevel();
                }
            }
        };
    }

    private void LootGuardian()
    {
        var aknife = "Gnomish Army Knife";
        var a1 = "Mining Pick";
        var a2 = "Hammer Pick";
        var a3 = "Skinning Knife";
        var a4 = "Gyromatic Micro-Adjustor";
        var a5 = "Arclight Spanner";
        var a6 = "Jeweler's Kit";
        var a7 = "Blacksmithing Hammer";
        var a8 = "Back Scratcher";
        var a9 = "Whirly Thing";
        var Mining = new List<string>()
            {
                "Mining Pick",
                "Hammer Pick",
                "Rough Stone",
                "Rough Grinding Stone",
                "Coarse Stone",
                "Coarse Grinding Stone",
                "Heavy Stone",
                "Heavy Grinding Stone",
                "Solid Stone",
                "Solid Grinding Stone",
                "Dense Stone",
                "Dense Grinding Stone",
                "Guardian Stone"
            };
        var Herbing = new List<string>()
            {
                "Herbalist's Spade",
                "Herbalist's Gloves",
                "Fel Blossom",
            };
        var Alchemy = new List<string>()
            {
                "Philosopher's Stone",
                "Empty Vial",
                "Leaded Vial",
                "Crystal Vial",
                "Imbued Vial",
                "Enchanted Vial"
            };
        var Enchanting1 = new List<string>()
            {
                "Runed Copper Rod",
                "Runed Silver Rod",
                "Runed Golden Rod",
                "Runed Truesilver Rod",
                "Runed Arcanite Rod",
                "Runed Fel Iron",
                "Runed Eternium Rod",
                "Runed Adamantite Rod",
                "Runed Titanium Rod"
            };
        var Enchanting2 = new List<string>()
            {
                "Strange Dust",
                "Lesser Magic Essence",
                "Greater Magic Essence",
                "Lesser Astral Essence",
                "Small Glimmering Shard",
                "Soul Dust",
                "Greater Astral Essence",
                "Large Glimmering Shard",
                "Lesser Mystic Essence",
                "Small Glowing Shard"
            };
        var Enchanting3 = new List<string>()
            {
                "Vision Dust",
                "Greater Mystic Essence",
                "Large Glowing Shard",
                "Lesser Nether Essence",
                "Small Radiant Shard",
                "Dream Dust",
                "Greater Nether Essence",
                "Large Radiant Shard",
                "Light Illusion Dust",
                "Lesser Eternal Essence",
                "Small Brilliant Shard",
                "Rich Illusion Dust",
                "Greater Eternal Essence",
                "Large Brilliant Shard"
            };
        var Enchanting4 = new List<string>()
            {
                "Arcane Dust",
                "Lesser Planar Essence",
                "Nexus Crystal",
                "Greater Planar Essence",
                "Small Prismatic Shard",
                "Infinite Dust",
                "Lesser Cosmic Essence",
                "Large Prismatic Shard",
                "Void Crystal",
                "Greater Cosmic Essence",
                "Titanium Powder",
                "Small Dream Shard",
                "Dream Shard",
                "Abyss Crystal"
            };
        var Skinning1 = new List<string>()
            {
                "Skinning Knife",
                "Zulian Slicer",
                "Finkle's Skinner"
            };
        var Skinning2 = new List<string>()
            {
                "Light Leather",
                "Light Hide,",
                "Cured Light Hide",
                "Slimy Murloc Scale",
                "Medium Leather",
                "Medium Hide",
                "Cured Medium Hide",
                "Raptor Hide",
                "Heavy Leather",
                "Heavy Hide",
                "Cured Heavy Hide",
                "Turtle Scale",
                "Thick Murloc Scale"
            };
        var Skinning3 = new List<string>()
            {
                "Thick Leather",
                "Thick Hide,",
                "Cured Thick Hide",
                "Worn Dragonscale",
                "Scorpid Scale",
                "Warbear Leather",
                "Rugged Leather",
                "Rugged Hide",
                "Green Dragonscale",
                "Devilsaur Leather",
                "Cured Rugged Hide",
                "Blue Dragonscale",
                "Heavy Scorpid Scale",
                "Enchanted Leather",
                "Black Dragonscale",
                "Scale of Onyxia",
                "Dreamscale",
                "Core Leather",
                "Primal Tiger Leather",
                "Primal Bat Leather"
            };
        var Skinning4 = new List<string>()
            {
                "Heavy Knothide Leather",
                "Wind Scales",
                "Wind Scale Fragment",
                "Thick Clefthoof Leather",
                "Red Dragonscale",
                "Patch of Thick Clefthoof Leather",
                "Patch of Crystal Infused Leather",
                "Nether Dragonscales",
                "Nether Dragonscale Fragment",
                "Knothide Leather Scraps",
                "Knothide Leather",
                "Fel Scales",
                "Fel Scale Fragment",
                "Crystal Infused Leather",
                "Cobra Scales",
                "Cobra Scale Fragment",
                "Cobra Scale Fragment",
                "Borean Leather Scraps",
                "Borean Leather"
            };
        var Blacksmithing = new List<string>()
            {
                "Blacksmith Hammer",
                "Hammer Pick"
            };
        var Inscription = new List<string>()
            {
                "Virtuoso Inking Set",
                "Moonglow Ink",
                "Alabaster Pigment",
                "Midnight Ink",
                "Dusky Pigment",
                "Hunter's Ink",
                "Lion's Ink",
                "Golden Pigment",
                "Verdant Pigment",
                "Dawnstar Ink",
                "Jadefire Ink",
                "Emerald Pigment",
                "Royal Ink",
                "Burnt Pigment",
                "Violet Pigment",
                "Celestial Ink",
                "Indigo Pigment",
                "Fiery Ink",
                "Silvery Pigment",
                "Shimmering Ink",
                "Ruby Pigment",
                "Ink of the Sky",
                "Nether Pigment",
                "Ethereal Ink",
                "Sapphire Pigment",
                "Darkflame Ink",
                "Ink of the Sea",
                "Azure Pigment",
                "Snowfall Ink",
                "Ebon Pigment",
                "Icy Pigment"
            };
        var Jewelcrafting = new List<string>()
            {
                "Malachite",
                "Tigerseye",
                "Small Lustrous Pearl",
                "Cut Tigerseye",
                "Delicate Copper Wire",
                "Shadowgem",
                "Bronze Setting",
                "Moss Agate",
                "Iridescent Pearl",
                "Lesser Moonstone",
                "Lesser Moonstone",
                "Jade",
                "Mithril Filigree",
                "Thick Citrine",
                "Shadow Pearl",
                "Jaggal Pearl",
                "Golden Pearl",
                "Cut Citrine",
                "Citrine",
                "Black Pearl",
                "Thorium Setting",
                "Star Ruby",
                "Blood of the Mountain",
                "Large Opal",
                "Blue Sapphire",
                "Prismatic Black Diamond",
                "Huge Emerald",
                "Azerothian Diamond",
                "Arcane Crystal",
                "Mercurial Adamantite",
                "Shadow Draenite",
                "Golden Draenite",
                "Flame Spessarite",
                "Deep Peridot",
                "Blood Garnet",
                "Azure Moonstone",
                "Adamantite Powder",
                "Talasite",
                "Star of Elune",
                "Skyfire Diamond",
                "Noble Topaz",
                "Nightseye",
                "Living Ruby",
                "Earthstorm Diamond",
                "Dawnstone",
                "Shadowsong Amethyst",
                "Seaspray Emerald",
                "Pyrestone",
                "Empyrean Sapphire",
                "Crimson Spinel",
                "Sun Crystal",
                "Shadow Crystal",
                "Northsea Pearl",
                "Huge Citrine",
                "Dark Jade",
                "Chalcedony",
                "Bloodstone",
                "Siren's Tear",
                "Twilight Opal",
                "Skyflare Diamond",
                "Sky Sapphire",
                "Scarlet Ruby",
                "Monarch Topaz",
                "Forest Emerald",
                "Earthsiege Diamond",
                "Dragon's Eye",
                "Autumn's Glow",
                "Majestic Zircon",
                "King's Amber",
                "Eye of Zul",
                "Dreadstone",
                "Cardinal Ruby",
                "Ametrine",
                "Aquamarine",
                "Lionseye"
            };
        var Fishing = new List<string>()
            {
                "Fishing Pole",
                "Strong Fishing Pole",
                "Blump Family Fishing Pole",
                "Ephemeral Fishing Pole",
                "Bone Fishing Pole",
                "Jeweled Fishing Pole",
                "Darkwood Fishing Pole",
                "Nat's Lucky Fishing Pole",
                "Arcanite Fishing Pole",
                "Big Iron Fishing Pole",
                "Nat Pagle's Extreme Angler FC-5000",
                "Seth's Graphite Fishing Pole",
                "Mastercraft Kalu'ak Fishing Pole",
                "Spun Truesilver Fishing Line",
                "High Test Eternium Fishing Line",
                "Shiny Bauble",
                "Nightcrawlers",
                "Aquadynamic Fish Lens",
                "Bright Baubles",
                "Flesh Eating Worm",
                "Sharpened Fish Hook",
                "Aquadynamic Fish Attractor",
                "Glow Worm",
                "Lucky Fishing Hat",
                "Weather-Beaten Fishing Hat",
                "Fishing Chair",
                "Nat Pagle's Extreme Anglin' Boots",
                "Boots of the Bay",
                "Captain Rumsey's Lager",
                "Underbelly Elixir"
            };
        var ElemWater = new List<string>()
            {
                "Elemental Water",
                "Essence of Water",
                "Globe of Water",
                "Mote of Water",
                "Primal Water",
                "Crystallized Water",
                "Eternal Water"
            };
        var ElemEarth = new List<string>()
            {
                "Elemental Earth",
                "Essence of Earth",
                "Core of Earth",
                "Mote of Earth",
                "Primal Earth",
                "Crystallized Earth",
                "Eternal Earth"
            };
        var ElemFire = new List<string>()
            {
                "Elemental Fire",
                "Essence of Fire",
                "Heart of Fire",
                "Mote of Fire",
                "Primal Fire",
                "Crystallized Fire",
                "Eternal Fire"
            };
        var ElemAir = new List<string>()
            {
                "Elemental Air",
                "Essence of Air",
                "Breath of Wind",
                "Mote of Air",
                "Primal Air",
                "Crystallized Air",
                "Eternal Air"
            };
        var ElemUndeath = new List<string>()
            {
                "Ichor of Undeath",
                "Essence of Undeath"
            };
        var ElemLife = new List<string>()
            {
                "Heart of the Wild",
                "Living Essence",
                "Mote of Life",
                "Primal Life",
                "Crystallized Life",
                "Eternal Life"
            };
        var ElemShadow = new List<string>()
            {
                "Mote of Shadow",
                "Primal Shadow",
                "Crystallized Shadow",
                "Eternal Shadow"
            };
        var ElemMana = new List<string>()
            {
               "Mote of Mana",
               "Primal Mana",
               "Eternal Mana"
            };
        var ElemNether = new List<string>()
            {
                "Primal Nether",
                "Nether Vortex"
            };
        var ElemMight = new List<string>()
            {
                "Primal Might"
            };
        var Cloth1 = new List<string>()
            {
                "Linen Cloth",
                "Wool Cloth"
            };
        var Cloth2 = new List<string>()
            {
                "Silk Cloth",
                "Mageweave Cloth"
            };
        var Cloth3 = new List<string>()
            {
                "Runecloth",
                "Netherweave Cloth"
            };
        var Cloth4 = new List<string>()
            {
                "Frostweave Cloth"
            };
        var Cloth5 = new List<string>()
            {
               "Felcloth",
               "Mooncloth",
               "Spellcloth",
               "Shadowcloth",
               "Primal Mooncloth",
               "Spellweave",
               "Moonshroud",
               "Ebonweave"
            };
        var OreandBars = new List<string>()
            {
                "Copper Ore",
                "Copper Bar",
                "Silver Ore",
                "Silver Bar",
                "Tin Ore",
                "Tin Bar",
                "Gold Ore",
                "Gold Bar",
                "Iron Ore",
                "Iron Bar",
                "Steel Bar"
            };
        var OreandBars2 = new List<string>()
            {
                "Thorium Ore",
                "Thorium Bar",
                "Mithril Ore",
                "Mithril Bar",
                "Truesilver Ore",
                "Truesilver Bar",
                "Dark Iron Ore",
                "Dark Iron Bar",
                "Enchanted Thorium Bar",
                "Arcanite Bar"
            };
        var OreandBars3 = new List<string>()
            {
                "Small Obsidian Shard",
                "Large Obsidian Shard",
                "Fel Iron Ore",
                "Fel Iron Bar",
                "Felsteel Bar",
                "Sulfuron Ingot",
                "Elementium Ingot",
                "Enchanted Elementium Bar",
                "Adamantite Ore",
                "Adamantite Bar",
                "Hardened Adamantite Bar",
                "Khorium Ore",
                "Khorium Bar",
                "Hardened Khorium",
                "Eternium Ore",
                "Eternium Bar",
                "Cobalt Ore",
                "Cobalt Bar",
                "Saronite Ore",
                "Saronite Bar",
                "Titansteel Bar",
                "Titanium Ore",
                "Titanium Bar"
            };
        var Bags1 = new List<string>()
            {
                "Mooncloth Bag",
                "Pumpkin Bag",
                "Bottomless Bag",
                "Traveler's Backpack",
                "Core Felcloth Bag",
                "Sun Touched Satchel",
                "Primal Mooncloth Bag",
                "Abyssal Bag",
                "Glacial Bag"
            };
        var Bags2 = new List<string>()
            {
                "Kodo Hide Bag",
                "Linen Bag",
                "Red Linen Bag",
                "Small Black Pouch",
                "Small Red Pouch",
                "Small Blue Pouch",
                "Small Green Pouch",
                "Small Brown Pouch",
                "Merchant's Satchel",
                "Wizbang's Gunnysack",
                "Wizbang's Gunnysack",
                "Old Moneybag",
                "Jewelry Box",
                "Courier's Bag",
                "Green Woolen Bag",
                "Red Woolen Bag",
                "Woolen Bag",
                "Blue Leather Bag",
                "Gnoll Hide Sack",
                "Green Leather Bag",
                "Red Leather Bag",
                "White Leather Bag",
                "Captain Sanders' Booty Bag",
                "Nolkai's Bag",
                "Old Blanchy's Feed Pouch",
                "Master's Haversack",
                "Brown Leather Satchel"
            };
        var Bags3 = new List<string>()
            {
                "Black Silk Pack",
                "Green Silk Pack",
                "Small Silk Pack",
                "Fel Steed Saddlebags",
                "Large Blue Sack",
                "Large Brown Sack",
                "Large Green Sack",
                "Large Red Sack",
                "Large Rucksack",
                "Murloc Skin Bag",
                "Snakeskin Bag",
                "Deviate Hide Pack",
                "Gunnysack of the Night Watch",
                "Heavy Brown Bag",
                "Mageweave Bag",
                "Red Mageweave Bag",
                "Large Knapsack",
                "Sturdy Lunchbox",
                "Huge Brown Sack"
            };
        var Bags4 = new List<string>()
            {
                "Runecloth Bag",
                "Draenic Leather Pack",
                "Journeyman's Backpack",
                "Toll-hide Bag",
                "Explorer's Knapsack",
                "Thawpelt Sack",
                "Traveler's Backpack",
                "Mooncloth Bag",
                "Netherweave Bag",
                "Pumpkin Bag",
                "Wayfarer's Knapsack",
                "Demon Hide Sack",
                "Darkmoon Storage Box"
        };
        var Bags5 = new List<string>()
            {
                "Onyxia Hide Backpack",
                "Core Felcloth Bag",
                "Bottomless Bag",
                "Imbued Netherweave Bag",
                "Jack-o'-Lantern",
                "Panther Hide Sack",
                "Supply Bag",
                "Halaani Bag",
                "Frostweave Bag",
                "Primal Mooncloth Bag",
                "Ebon Shadowbag",
                "Sun Touched Satchel",
                "Pit Lord's Satchel",
                "Tattered Hexcloth Sack",
                "Papa's New Bag"
            };
        var Bags6 = new List<string>()
            {
                "Quiver of a Thousand Feathers",
                "Dragon Hide Bag",
                "Abyssal Bag",
                "Dragon Hide Bag",
                "Enlarged Onyxia Hide Backpack",
                "Glacial Bag",
                "Papa's Brand New Bag",
                "Tattered Hexcloth Bag",
                "Portable Hole"
            };
        var RaidStuffs1 = new List<string>()
            {
                "Flask of Fortification",
                "Flask of Relentless Assault",
                "Flask of the Titans",
                "Flask of Distilled Wisdom",
                "Flask of Supreme Power",
                "Flask of Chromatic Resistance",
                "Flask of Endless Rage",
                "Flask of Pure Mojo",
                "Flask of Stoneblood",
                "Flask of the Frost Wyrm",
                "Shattrath Flask of Pure Death",
                "Shattrath Flask of Blinding Light",
                "Shattrath Flask of Supreme Power",
                "Shattrath Flask of Mighty Restoration",
                "Shattrath Flask of Fortification",
                "Shattrath Flask of Relentless Assault",
                "Unstable Flask of the Bandit",
                "Dense Weightstone",
                "Elemental Sharpening Stone",
                "Adamantite Sharpening Stone",
                "Instant PoisonI",
                "Instant PoisonII",
                "Instant PoisonIII",
                "Instant PoisonIV",
                "Instant PoisonV",
                "Instant PoisonVI",
                "Instant PoisonVII",
                "Deadly PoisonI",
                "Deadly PoisonII",
                "Deadly PoisonIII",
                "Deadly PoisonIV",
                "Deadly PoisonV",
                "Deadly PoisonVI",
                "Deadly PoisonVII",
                "Wound PoisonI",
                "Wound PoisonII",
                "Wound PoisonIII",
                "Wound PoisonIV",
                "Wound PoisonV",
                "Wound PoisonVI",
                "Wound PoisonVII",
                "Superior Mana Oil",
                "Brilliant Mana Oil",
                "Sacred Candle",
                "Devout Candle",
                "Rune of Teleportation",
                "Rune of Portals",
                "Arcane Powder",
                "Light Feather",
                "Infernal Stone",
                "Demonic Figurine",
                "Ironwood Seed",
                "Flintweed Seed",
                "Starleaf Seed",
                "Wild Thornroot",
                "Wild Quillvine",
                "Wild Spineleaf",
                "Ankh",
                "Shiny Fish Scales",
                "Fish Oil",
                "Symbol of Divinity",
                "Symbol of Kings",
                "Corpse Dust"
            };
        var RaidStuffs2 = new List<string>()
            {
                "Guru's Elixir",
                "Elixir of Mastery",
                "Elixir of the Mongoose",
                "Elixir of Mighty Agility",
                "Elixir of Major Agility",
                "Ground Scorpok Assay",
                "Elixir of Giants",
                "Elixir of Mighty Strength",
                "R.O.I.D.S.",
                "Wrath Elixir",
                "Fel Strength Elixir",
                "Onslaught Elixir",
                "Elixir of Lightning Speed",
                "Elixir of Accuracy",
                "Elixir of Armor Piercing",
                "Elixir of Deadly Strikes",
                "Elixir of Expertise",
                "Spellpower Elixir",
                "Elixir of Healing Power",
                "Greater Arcane Elixir",
                "Arcane Elixir",
                "Cerebral Cortex Compound",
                "Elixir of the Sages",
                "Elixir of Greater Intellect",
                "Elixir of Spirit",
                "Elixir of Mastery",
                "Elixir of Draenic Wisdom",
                "Spirit of Zanza",
                "Crystal Force",
                "Gizzard Gum",
                "Elixir of Mighty Mageblood",
                "Elixir of Major Mageblood",
                "Elixir of Protection",
                "Lung Juice Cocktail",
                "Elixir of Mighty Fortitude",
                "Elixir of Mighty Defense",
                "Juju Escape",
                "Magic Resistance Potion",
                "Mageblood Potion",
                "Elixir of Major Mageblood",
                "Major Troll's Blood Potion",
                "Gift of Arthas",
                "Noggenfogger Elixir",
                "Elixir of Greater Water Breathing",
                "Elixir of Water Breathing",
                "Restorative Potion",
                "Free Action Potion",
                "Elixir of Brute Force",
                "Elixir of Superior Defense",
                "Juju Ember",
                "Juju Chill",
                "Juju Guile",
                "Juju Flurry",
                "Juju Might",
                "Juju Power",
                "Greater Stoneshield Potion",
                "Major Mana Potion",
                "Super Mana Potion",
                "Mana Potion Injector",
                "Endless Mana Potion",
                "Runic Mana Potion",
                "Runic Mana Injector",
                "Major Healing Potion",
                "Super Healing Potion",
                "Healing Potion Injector",
                "Endless Healing Potion",
                "Runic Healing Potion",
                "Runic Healing Injector",
                "Auchenai Healing Potion",
                "Winterfall Firewater",
                "Greater Fire Protection Potion",
                "Greater Shadow Protection Potion",
                "Crystal Yield",
                "Haste Potion"
            };
        var RaidStuffs3 = new List<string>()
            {
                "Deviate Fish",
                "Smoked Desert Dumplings",
                "Buzzard Bites",
                "Clam Bar",
                "Ravager Dog",
                "Warp Burger",
                "Blackened Sporefish",
                "Blackened Basilisk",
                "Broiled Bloodfin",
                "Grilled Mudfish",
                "Crunchy Serpent",
                "Essence Mango",
                "Golden Fish Sticks",
                "Mok'Nathal Shortribs",
                "Poached Bluefish",
                "Roasted Clefthoof",
                "Spicy Smoked Sausage",
                "Stormchops",
                "Sporeling Snack",
                "Talbuk Steak",
                "Warp Burger",
                "Dirge's Kickin' Chimaerok Chops",
                "The Golden Link",
                "Spicy Hot Talbuk",
                "Spicy Crawdad",
                "Skullfish Soup",
                "Oronok's Tuber of Strength",
                "Oronok's Tuber of Spell Power",
                "Oronok's Tuber of Healing",
                "Oronok's Tuber of Agility",
                "Kibler's Bits",
                "Fisherman's Feast",
                "Worm Delight",
                "Worg Tartare",
                "Very Burnt Worg",
                "Tracker Snacks",
                "Tender Shoveltusk Steak",
                "Spicy Fried Herring",
                "Spicy Blue Nettlefish",
                "Spiced Worm Burger",
                "Spiced Mammoth Treats",
                "Snapper Extreme",
                "Shoveltusk Soup",
                "Rhinolicious Wormsteak",
                "Poached Northern Sculpin",
                "Mighty Rhino Dogs",
                "Mega Mammoth Meal",
                "Imperial Manta Steak",
                "Hearty Rhino",
                "Great Feast",
                "Fish Feast",
                "Firecracker Salmon",
                "Dragonfin Filet",
                "Dalaran Clam Chowder",
                "Cuttlesteak",
                "Blackened Dragonfin",
                "Scroll of Protection VII",
                "Scroll of Protection VI",
                "Scroll of Agility VIII",
                "Scroll of Agility VII",
                "Scroll of Agility VI",
                "Scroll of Intellect VIII",
                "Scroll of Intellect VII",
                "Scroll of Intellect VI",
                "Scroll of Spirit VIII",
                "Scroll of Spirit VII",
                "Scroll of Spirit VI",
                "Runescroll of Fortitude",
                "Scroll of Stamina VIII",
                "Scroll of Stamina VII",
                "Scroll of Stamina VI",
                "Scroll of Strength VIII",
                "Scroll of Strength VII",
                "Scroll of Strength VI",
                "Scroll of Recall III",
                "Heavy Runecloth Bandage",
                "Heavy Netherweave Bandage",
                "Heavy Frostweave Bandage"
            };
        var RaidStuffs4 = new List<string>()
            {
                "Whipper Root Tuber",
                "Drums of Battle",
                "Drums of War",
                "Thistle Tea",
                "Hot Apple Cider",
                "Rumsey Rum Dark",
                "Field Repair Bot 74A",
                "Field Repair Bot 110G",
                "Scrapbot Construction Kit",
                "Jeeves",
                "Oil of Immolation",
                "Goblin Sapper Charge",
                "Gnomish Battle Chicken",
                "Argent Tournament Invitation",
                "Lava Core",
                "Fiery Core"
            };
        var RepItems1 = new List<string>()
            {
                "Encrypted Twilight Text",
                "Bloodscalp Coin",
                "Gurubashi Coin",
                "Hakkari Coin",
                "Razzashi Coin",
                "Sandfury Coin",
                "Skullsplitter Coin",
                "Vilebranch Coin",
                "Witherbark Coin",
                "Zulian Coin",
                "Minion's Scourgestone",
                "Invader's Scourgestone",
                "Corruptor's Scourgestone",
                "Battered Junkbox",
                "Worn Junkbox",
                "Sturdy Junkbox",
                "Heavy Junkbox",
                "Strong Junkbox",
                "Reinforced Junkbox",
                "Deadwood Headdress Feather",
                "Winterfall Spirit Beads",
                "Troll Tribal Necklace"
            };
        var RepItems2 = new List<string>()
            {
                "Mark of Honor Hold",
                "Mark of Thrallmar",
                "Obsidian Warbeads",
                "Unidentified Plant Parts",
                "Coilfang Armaments",
                "Oshu'gun Crystal Fragment",
                "Pair of Ivory Tusks",
                "Zaxxis Insignia",
                "Nethercite Ore",
                "Nethermine Flayer Hide",
                "Netherdust Pollen",
                "Netherwing Egg",
                "Apexis Shard",
                "Apexis Crystal",
                "Sanguine Hibiscus",
                "Mark of Sargeras",
                "Mark of Kil'jaeden",
                "Fel Armament",
                "Holy Dust",
                "Firewing Signet",
                "Sunfury Signet",
                "Arcane Tome",
                "Arakkoa Feather"
            };
        var Repitems3 = new List<string>()
        {

        };
        //Profession Trade Goods and Tools
        if (CerberusSettings.CurrentSetting.MiningTools)
        {
            foreach (var item in Mining)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.HerbingTools)
        {
            foreach (var item in Herbing)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.AlchemyTools)
        {
            foreach (var item in Alchemy)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.EnchantingTools)
        {
            foreach (var item in Enchanting1)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }

        }
        if (CerberusSettings.CurrentSetting.EnchantingTools2)
        {
            foreach (var item in Enchanting2)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.EnchantingTools3)
        {
            foreach (var item in Enchanting3)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.EnchantingTools4)
        {
            foreach (var item in Enchanting4)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.SkinningTools)
        {
            foreach (var item in Skinning1)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.SkinningTools2)
        {
            foreach (var item in Skinning2)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.SkinningTools3)
        {
            foreach (var item in Skinning3)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.SkinningTools4)
        {
            foreach (var item in Skinning4)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.BlackSmithingTools)
        {

            foreach (var item in Blacksmithing)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.InscriptionTools)
        {
            foreach (var item in Inscription)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.JewelCraftingTools)
        {
            foreach (var item in Jewelcrafting)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.FishingTools)
        {
            foreach (var item in Fishing)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        //Elemental Trade Goods
        if (CerberusSettings.CurrentSetting.Water)
        {
            foreach (var item in ElemWater)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.Earth)
        {
            foreach (var item in ElemEarth)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.Fire)
        {
            foreach (var item in ElemFire)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.Air)
        {
            foreach (var item in ElemAir)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.Undeath)
        {
            foreach (var item in ElemUndeath)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.Life)
        {
            foreach (var item in ElemLife)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.Shadow)
        {
            foreach (var item in ElemShadow)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.Mana)
        {
            foreach (var item in ElemMana)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.Nether)
        {
            foreach (var item in ElemNether)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.Might)
        {
            foreach (var item in ElemMight)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        //Cloth
        if (CerberusSettings.CurrentSetting.LinenWool)
        {
            foreach (var item in Cloth1)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.SilkMage)
        {
            foreach (var item in Cloth2)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.NetherRune)
        {
            foreach (var item in Cloth3)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.FrostWeave)
        {
            foreach (var item in Cloth4)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.SpecialCloth)
        {
            foreach (var item in Cloth5)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        //Ore and Bar´s
        if (CerberusSettings.CurrentSetting.OreAndBar)
        {
            foreach (var item in OreandBars)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.OreAndBar2)
        {
            foreach (var item in OreandBars2)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.OreAndBar3)
        {
            foreach (var item in OreandBars3)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        //Bags
        if (CerberusSettings.CurrentSetting.Bag1)
        {
            foreach (var item in Bags1)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.Bag2)
        {
            foreach (var item in Bags2)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.Bag3)
        {
            foreach (var item in Bags3)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.Bag4)
        {
            foreach (var item in Bags4)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.Bag5)
        {
            foreach (var item in Bags5)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.Bag6)
        {
            foreach (var item in Bags6)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        //RaidStuff
        if (CerberusSettings.CurrentSetting.RaidStuff)
        {
            foreach (var item in RaidStuffs1)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.RaidStuff2)
        {
            foreach (var item in RaidStuffs2)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.RaidStuff3)
        {
            foreach (var item in RaidStuffs3)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.RaidStuff4)
        {
            foreach (var item in RaidStuffs4)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        //Rep Stuff
        if (CerberusSettings.CurrentSetting.Repclassic)
        {
            foreach (var item in RepItems1)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.Reptbc)
        {
            foreach (var item in RepItems2)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.Repwotlk)
        {
            foreach (var item in Repitems3)
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item))
                {
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item);
                }
            }
        }
        if (CerberusSettings.CurrentSetting.ArmyKnife)
        {
            if (wManagerSetting.CurrentSetting.DoNotSellList.Contains(aknife))
            {
                List<string> tools = new List<string>();
                tools.Add(aknife);
                tools.Remove(a1);
                tools.Remove(a2);
                tools.Remove(a3);
                tools.Remove(a4);
                tools.Remove(a5);
                tools.Remove(a6);
                tools.Remove(a7);
                tools.Remove(a8);
                tools.Remove(a9);
            }
        }
    }

    public static void PluginChecker()
    {
        try
        {
            var Pluginlist = new List<string>()
                {
                "VanillaClamOpener",
                "SmoothMove",
                "Ammo",
                "DoNotSellMyLoot"
                };
            var ActivePlugins = wManagerSetting.CurrentSetting.Plugin﻿sSettings;
            var Plugin = Pluginlist.Any(i => ActivePlugins.Any(x => x.Actif & x.FileName.Equals(i)));
            if (Plugin)
            {
                foreach (var plugin in ActivePlugins)
                {
                    plugin.Actif = false;
                }
            }
            PluginsManager.DisposeAllPlugins();
            PluginsManager.LoadAllPlugins();
        }

        catch { }
    }

    public static void BotSettings()
    {
        var Class = ObjectManager.Me.WowClass;
        var Settings = wManagerSetting.CurrentSetting;
        try
        {
            Logging.Write("------------Checking Settings-------------");
            //Class/Fight Class    
            Settings.FightInteractTargetMinDistance = 45;
            Settings.IgnoreFightWhenInMove = false;
            Settings.AssignTalents = true;
            Settings.CanAttackUnitsAlreadyInFight = false;
            Settings.DetectEvadingMob = true;
            Settings.TrainNewSkills = true;
            Settings.DontStartFighting = false;
            Settings.AttackElite = false;
            Settings.AttackBeforeBeingAttacked = true;
            Settings.HelpingGroupMembers = true;
            Settings.SpellRotationSpeed = false;
            Settings.LootInCombat = false;
            Settings.BlackListTrainingDummy = true;
            Settings.CalcuCombatRange = false;
            Settings.IgnoreCombatWithPet = true;
            Settings.UseLuaToMove = true;
            Settings.RandomJumping = false;
            Settings.IgnoreFightWithPlayer = false;
            Settings.UnlockMaxFps = false;
            //Mount Options
            Settings.UseMount = true;
            Settings.MountDistance = 80;
            Settings.IgnoreFightGoundMount = true;
            Settings.UseFlyingMount = true;
            //Food/Drink
            Settings.FoodIsSpell = false;

            if (Settings.Plugin﻿sSettings.Any﻿(plugin => plugin.Actif﻿ && string.Equals(plugin.FileName, "HumanMasterPlugin.dll") && ObjectManager.Me.Level <= 5))
            {
                if (ObjectManager.Me.PlayerRaceString == "Draenei")
                {
                    Settings.FoodName = "";
                }
                if (ObjectManager.Me.PlayerRaceString == "Human")
                {
                    Settings.FoodName = "";
                }
            }
            else if (Settings.Plugin﻿sSettings.Any﻿(plugin => plugin.Actif﻿ && !string.Equals(plugin.FileName, "HumanMasterPlugin.dll") && ObjectManager.Me.Level <= 5))
            {
                if (ObjectManager.Me.PlayerRaceString == "Draenei")
                {
                    Settings.FoodName = "Tough Hunk of Bread";
                }
                if (ObjectManager.Me.PlayerRaceString == "Human")
                {
                    Settings.FoodName = "";
                }
            }
            
            if (ObjectManager.Me.Level < 12 || Class != WoWClass.Warrior || Class != WoWClass.Rogue || Class != WoWClass.DeathKnight)
            {
                Settings.FoodPercent = 60;
                Settings.FoodMaxPercent = 98;
                Settings.DrinkPercent = 1;
                Settings.DrinkMaxPercent = 98;
            }
            else if (ObjectManager.Me.Level >= 12 && Class == WoWClass.Warlock || Class == WoWClass.Hunter)
            {
                Settings.FoodPercent = 25;
                Settings.FoodMaxPercent = 98;
                Settings.DrinkPercent = 20;
                Settings.DrinkMaxPercent = 98;
            }
            else if (Class == WoWClass.Warrior || Class == WoWClass.Rogue || Class == WoWClass.DeathKnight)
            {
                Settings.FoodPercent = 60;
                Settings.FoodMaxPercent = 98;
                Settings.DrinkPercent = 1;
                Settings.DrinkMaxPercent = 98;
            }
            else
            {
                Settings.FoodPercent = 40;
                Settings.FoodMaxPercent = 98;
                Settings.DrinkPercent = 40;
                Settings.DrinkMaxPercent = 98;
            }

            Settings.DrinkIsSpell = false;
            Settings.DrinkName = "";

            if (Settings.Plugin﻿sSettings.Any﻿(plugin => plugin.Actif﻿ && string.Equals(plugin.FileName, "HumanMasterPlugin.dll") && ObjectManager.Me.Level < 5))
            {
                Settings.TryToUseBestBagFoodDrink = true;
            }
            else if (Settings.Plugin﻿sSettings.Any﻿(plugin => plugin.Actif﻿ && string.Equals(plugin.FileName, "HumanMasterPlugin.dll") && ObjectManager.Me.Level >= 5))
            {
                Settings.TryToUseBestBagFoodDrink = false;
            }
            else
            {
                Settings.TryToUseBestBagFoodDrink = true;
            }

            //Looting and Farming Options
            Settings.LootMobs = true;
            Settings.LootChests = true;
            Settings.SkinMobs = true;
            Settings.HarvestMinerals = false;
            Settings.HarvestHerbs = true;
            Settings.HarvestTimber = false;
            Settings.HarvestAvoidPlayersRadius = 7;

            if (ObjectManager.Me.Level <= 6)
            {
                Settings.MaxUnitsNearMobs = 1;
                Settings.MaxUnitsNearObjects = 1;
            }
            else if (ObjectManager.Me.Level <= 15)
            {
                Settings.MaxUnitsNearMobs = 1;
                Settings.MaxUnitsNearObjects = 1;
            }
            else if (ObjectManager.Me.Level >= 20 && (Class == WoWClass.Warlock || Class == WoWClass.Hunter))
            {
                Settings.MaxUnitsNearMobs = 3;
                Settings.MaxUnitsNearObjects = 3;
            }
            else if (ObjectManager.Me.Level >= 30)
            {
                Settings.MaxUnitsNearMobs = 2;
                Settings.MaxUnitsNearObjects = 2;
            }

            Settings.SearchRadiusMobs = 88;
            Settings.SearchRadiusObjects = 300;
            Settings.HarvestDuringLongMove = false;
            Settings.BlackListZoneWhereDead = false;
            Settings.Prospecting = false;
            Settings.ProspectingTime = 15;
            Settings.ProspectingInTown = false;
            Settings.Milling = false;
            Settings.MillingTime = 15;
            Settings.MillingInTown = false;
            Settings.SkipInOutDoors = false;
            Settings.LootAndHarvestRangeQuickly = true;
            Settings.MaxTryPerNode = 6;
            Settings.DetectNodesStuck = true;
            Settings.SkipNodesInWater = false;
            Settings.IgnoreFightDuringFarmIfDruidForm = false;
            Settings.SkinNinja = false;
            Settings.Smelting = false;
            //Relogger
            Settings.Relogger = false;
            //Vendor(Selling or Buying)
            Settings.MinFreeBagSlotsToGoToTown = 2;

            if (Bag.GetItemContainerBagIdAndSlot("Reins of the Traveler's Tundra Mammoth").Count() > 0)
            {
                Settings.UseMammoth = true;
                ObjectManager.GetObjectWoWItem().FirstOrDefaul﻿t(i => i.Entry == 1205);
            }
            else
            {
                Settings.UseMammoth = false;
            }

            Settings.Selling = true;
            Settings.Repair = true;
            Settings.SellGray = true;
            Settings.SellWhite = true;
            Settings.SellGreen = true;
            Settings.SellBlue = false;
            Settings.SellPurple = false;
            Settings.ToTownTimer = 90;
            Settings.GoToTownHerbBags = false;
            Settings.GoToTownMiningBags = false;
            //Other options
            Settings.LatencyMin = 500;
            Settings.LatencyMax = 1000;
            Settings.NpcMailboxSearchRadius = 1000;
            Settings.AddToNpcDb = false;
            Settings.NpcScanRepair = true;
            Settings.NpcScanVendor = true;
            Settings.NpcScanAuctioneer = true;
            Settings.NpcScanMailboxes = true;
            Settings.UseSpiritHealer = false;
            Settings.AcceptOnlyProfileNpc = false;
            Settings.WaitResurrectionSickness = true;
            //Path-finding
            Settings.UsePathsFinder = true;
            Settings.SmoothPath = true;
            Settings.WallDistancePathFinder = 0.6f;
            Settings.AvoidWallWithRays = true;
            Settings.AvoidBlacklistedZonesPathFinder = true;
            Settings.BlackListIfNotCompletePath = true;
            Settings.DismountWhenStuck = true;
            Settings.TryToAvoidEnemyGroupsRatio = 1.20;
            //Taxi
            Settings.FlightMasterDiscoverRange = 550;

            if (Settings.PluginsSettings.Any﻿(flight => flight.FileName.Contains("VanillaFlightMaster")))
            {
                Settings.FlightMasterTaxiUse = false;
            }
            else
                Settings.FlightMasterTaxiUse = true;

            Settings.FlightMasterTaxiUseOnlyIfNear = false;
            //Stop game/bot /Security
            Settings.SecurityShutdownComputer = false;
            Settings.CloseIfFullBag = false;
            Settings.CloseIfReached4000HonorPoints = false;
            Settings.CloseIfPlayerTeleported = false;
            Settings.CloseAfterXLevel = 100;
            Settings.CloseIfWhisperBiggerOrEgalAt = 20;
            Settings.CloseAfterXBlockagesActive = false;
            Settings.CloseAfterXBlockagesLatest10Minutes = 80;
            Settings.CloseAfterXMinActive = false;
            Settings.CloseAfterXMin = 6000;
            Settings.CloseAfterXDeathsActive = false;
            Settings.CloseAfterXDeathsLatest10Minutes = 5;
            Settings.SecurityPauseBotIfNerbyPlayer = false;
            Settings.DisableNearPlayerToTown = true;
            Settings.SecurityPauseBotIfNerbyPlayerRadius = 20;
            Settings.SecurityPauseBotIfNerbyPlayerTime = 10;
            Settings.HearthstoneAfterXBlockagesLatest10Minutes = 50;
            Settings.UseSpiritHealerAfterXDeathsLatest10Minutes = 50;
            Settings.SecuritySongIfNewWhisper = false;
            Settings.RecordChatInLog = true;
            //Mail 
            Settings.UseMail = false;

            //Resting Mana and Drink/Food amount
            if (ObjectManager.Me.Level >= 6 && Class == WoWClass.Mage && Settings.Plugin﻿sSettings.Any﻿(plugin => plugin.Actif﻿ && string.Equals(plugin.FileName, "HumanMasterPlugin.dll") && SpellManager.KnowSpell(587 & 5504)))
            {
                Settings.RestingMana = false;
                Settings.FoodAmount = 0;
                Settings.DrinkAmount = 0;
            }
            else if (ObjectManager.Me.Level < 5 && (Class == WoWClass.Rogue || Class == WoWClass.DeathKnight || Class == WoWClass.Warrior) && Settings.Plugin﻿sSettings.Any﻿(plugin => plugin.Actif﻿ && !string.Equals(plugin.FileName, "HumanMasterPlugin.dll")))
            {
                Settings.RestingMana = true;
                Settings.FoodAmount = 10;
                Settings.DrinkAmount = 0;
            }
            else if (ObjectManager.Me.Level < 5 && (Class != WoWClass.Rogue || Class != WoWClass.DeathKnight || Class != WoWClass.Warrior) && Settings.Plugin﻿sSettings.Any﻿(plugin => plugin.Actif﻿ && !string.Equals(plugin.FileName, "HumanMasterPlugin.dll")))
            {
                Settings.RestingMana = true;
                Settings.FoodAmount = 10;
                Settings.DrinkAmount = 10;
            }
            else if (ObjectManager.Me.Level < 5 && (Class == WoWClass.Rogue || Class == WoWClass.DeathKnight || Class == WoWClass.Warrior) && Settings.Plugin﻿sSettings.Any﻿(plugin => plugin.Actif﻿ && string.Equals(plugin.FileName, "HumanMasterPlugin.dll")))
            {
                Settings.RestingMana = false;
                Settings.FoodAmount = 20;
                Settings.DrinkAmount = 0;
            }
            else if (ObjectManager.Me.Level < 5 && (Class != WoWClass.Rogue || Class != WoWClass.DeathKnight || Class != WoWClass.Warrior) && Settings.Plugin﻿sSettings.Any﻿(plugin => plugin.Actif﻿ && string.Equals(plugin.FileName, "HumanMasterPlugin.dll")))
            {
                Settings.RestingMana = false;
                Settings.FoodAmount = 20;
                Settings.DrinkAmount = 20;
            }
            else if (ObjectManager.Me.Level >= 5 && ObjectManager.Me.Level < 16 && (Class == WoWClass.Rogue || Class == WoWClass.DeathKnight || Class == WoWClass.Warrior))
            {
                Settings.RestingMana = false;
                Settings.FoodAmount = 30;
                Settings.DrinkAmount = 0;
            }
            else if (ObjectManager.Me.Level >= 5 && ObjectManager.Me.Level < 16 && (Class != WoWClass.Rogue || Class != WoWClass.DeathKnight || Class != WoWClass.Warrior))
            {
                Settings.RestingMana = false;
                Settings.FoodAmount = 20;
                Settings.DrinkAmount = 20;
            }
            else if (ObjectManager.Me.Level >= 16 && (Class == WoWClass.Warlock || Class == WoWClass.Rogue || Class == WoWClass.DeathKnight || Class == WoWClass.Warrior))
            {
                Settings.RestingMana = false;
                Settings.FoodAmount = 80;
                Settings.DrinkAmount = 0;
            }
            else if (ObjectManager.Me.Level >= 16 && Class == WoWClass.Priest || Class == WoWClass.Hunter)
            {
                Settings.RestingMana = true;
                Settings.FoodAmount = 30;
                Settings.DrinkAmount = 100;
            }
            else
            {
                Settings.RestingMana = true;
                Settings.FoodAmount = 60;
                Settings.DrinkAmount = 60;
            }
            //TBC unique settings
            if (Information.ForWowBuild >= 8606)
            {
                Settings.ForceAutoLoot = true;
            }
            Settings.Save();
        }
        catch { }
    }

    public static void ScrollUser()
    {
        var Scrolls = new List<uint>()
            {
                3012,
                1477,
                4425,
                10309,
                27498,
                33457,
                43463,
                43464,
                955,
                2290,
                4419,
                10308,
                27499,
                33458,
                37091,
                37092,
                3013,
                1478,
                4421,
                10305,
                27500,
                33459,
                43467,
                1181,
                1712,
                4424,
                10306,
                27501,
                33460,
                37097,
                37098,
                1180,
                1711,
                4422,
                10307,
                27502,
                33461,
                37093,
                37093,
                954,
                2289,
                4426,
                10310,
                27503,
                33462,
                43465
            };

        foreach (uint Scroll in Scrolls)
        {
            if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(Scroll) && !ObjectManager.Me.IsDeadMe)
            {
                ItemsManager.UseItem(Scroll);
                Thread.Sleep(Usefuls.Latency + 1000);
            }
        }
    }

    public static void ItemOpener()
    {
        if (Information.ForWowBuild >= 8700)
        {
            foreach (WoWItem item in Bag.GetBagItem())
            {
                if (item.GetItemInfo.ItemName == "Big-mouth Clam")
                {
                    Keyboard.DownKey(Memory.WowMemory.Memory.WindowHandle, Keys.ShiftKey);
                    Thread.Sleep(Others.Random(50, 150));
                    ItemsManager.UseItem(7973);
                    Thread.Sleep(Others.Random(50, 150));
                    Keyboard.UpKey(Memory.WowMemory.Memory.WindowHandle, Keys.ShiftKey);
                    Thread.Sleep(Others.Random(50, 150));
                }
                if (item.GetItemInfo.ItemName == "Small Barnacled Clam")
                {
                    Keyboard.DownKey(Memory.WowMemory.Memory.WindowHandle, Keys.ShiftKey);
                    Thread.Sleep(Others.Random(50, 150));
                    ItemsManager.UseItem(5523);
                    Thread.Sleep(Others.Random(50, 150));
                    Keyboard.UpKey(Memory.WowMemory.Memory.WindowHandle, Keys.ShiftKey);
                    Thread.Sleep(Others.Random(50, 150));
                }
                if (item.GetItemInfo.ItemName == "Thick-shelled Clam")
                {
                    Keyboard.DownKey(Memory.WowMemory.Memory.WindowHandle, Keys.ShiftKey);
                    Thread.Sleep(Others.Random(50, 150));
                    ItemsManager.UseItem(5524);
                    Thread.Sleep(Others.Random(50, 150));
                    Keyboard.UpKey(Memory.WowMemory.Memory.WindowHandle, Keys.ShiftKey);
                    Thread.Sleep(Others.Random(50, 150));
                }
                if (item.GetItemInfo.ItemName == "Oozing Bag")
                {
                    Keyboard.DownKey(Memory.WowMemory.Memory.WindowHandle, Keys.ShiftKey);
                    Thread.Sleep(Others.Random(50, 150));
                    ItemsManager.UseItem(20768);
                    Thread.Sleep(Others.Random(50, 150));
                    Keyboard.UpKey(Memory.WowMemory.Memory.WindowHandle, Keys.ShiftKey);
                    Thread.Sleep(Others.Random(50, 150));
                }
                if (item.GetItemInfo.ItemName == "Pirate's Footlocker")
                {
                    Keyboard.DownKey(Memory.WowMemory.Memory.WindowHandle, Keys.ShiftKey);
                    Thread.Sleep(Others.Random(50, 150));
                    ItemsManager.UseItem(9276);
                    Thread.Sleep(Others.Random(50, 150));
                    Keyboard.UpKey(Memory.WowMemory.Memory.WindowHandle, Keys.ShiftKey);
                    Thread.Sleep(Others.Random(50, 150));
                }
                if (item.GetItemInfo.ItemName == "Scum Covered Bag")
                {
                    Keyboard.DownKey(Memory.WowMemory.Memory.WindowHandle, Keys.ShiftKey);
                    Thread.Sleep(Others.Random(50, 150));
                    ItemsManager.UseItem(20767);
                    Thread.Sleep(Others.Random(50, 150));
                    Keyboard.UpKey(Memory.WowMemory.Memory.WindowHandle, Keys.ShiftKey);
                    Thread.Sleep(Others.Random(50, 150));
                }
                if (item.GetItemInfo.ItemName == "Battered Chest")
                {
                    Keyboard.DownKey(Memory.WowMemory.Memory.WindowHandle, Keys.ShiftKey);
                    Thread.Sleep(Others.Random(50, 150));
                    ItemsManager.UseItem(6356);
                    Thread.Sleep(Others.Random(50, 150));
                    Keyboard.UpKey(Memory.WowMemory.Memory.WindowHandle, Keys.ShiftKey);
                    Thread.Sleep(Others.Random(50, 150));
                }

            }
        }
        else
        {
            foreach (WoWItem item in Bag.GetBagItem())
            {
                if (item.GetItemInfo.ItemName == "Big-mouth Clam")
                {
                    ItemsManager.UseItem(7973);
                    Thread.Sleep(Others.Random(50, 150));
                }
                if (item.GetItemInfo.ItemName == "Small Barnacled Clam")
                {
                    ItemsManager.UseItem(5523);
                    Thread.Sleep(Others.Random(50, 150));
                }
                if (item.GetItemInfo.ItemName == "Thick-shelled Clam")
                {
                    ItemsManager.UseItem(5524);
                }
                if (item.GetItemInfo.ItemName == "Oozing Bag")
                {
                    ItemsManager.UseItem(20768);
                    Thread.Sleep(Others.Random(50, 150));
                }
                if (item.GetItemInfo.ItemName == "Pirate's Footlocker")
                {
                    ItemsManager.UseItem(9276);
                    Thread.Sleep(Others.Random(50, 150));
                }
                if (item.GetItemInfo.ItemName == "Scum Covered Bag")
                {
                    ItemsManager.UseItem(20767);
                    Thread.Sleep(Others.Random(50, 150));
                }
                if (item.GetItemInfo.ItemName == "Battered Chest")
                {                  
                    ItemsManager.UseItem(6356);
                    Thread.Sleep(Others.Random(50, 150));
                }
            }
        }
    }

    public void StartAreaHelper()
    {
        if (ToTown.ForceToTown)
        {
            return;
        }

        if (ObjectManager.Me.Level <= 4)
        {
            if (BagValue() >= 200)
            {
                Logging.Write("[Cerberus Bag Watcher]:" + " Your Bag Value is " + BagValue() + " We are now selling to have money for Class Trainer");
                ToTown.ForceToTown = true;
            }
        }
        else if (ObjectManager.Me.Level <= 6)
        {
            if (CurrentMoney() < 200 && (TotalMoney() > 200))
            {
                Logging.Write("[Cerberus Bag Watcher]:" + " Your Bag Value is " + BagValue() + " We are now selling to have money for First Aid and Food");
                ToTown.ForceToTown = true;
            }
        }
       else if (ObjectManager.Me.Level < 8)
       {
            if (CurrentMoney() < 300 && (TotalMoney() > 300))
            {
                Logging.Write("[Cerberus Bag Watcher]:" + " Your Bag Value is " + BagValue() + " We are now selling to have money for Class Trainer");
                ToTown.ForceToTown = true;
            }
       }
    }

    public void RacialInitiator()
    {
        if (RacialCD() > 0 || !RacialSettings())
        {
            return;
        }
        if (RacialCD() == 0 && RacialSettings())
        {
            RacialSkillManager();
        }
    }

    public void RacialSkillManager()
    {
        var Race = ObjectManager.Me.PlayerRaceString;
        var Racial = Racials.Where(k => k.Key.Equals(Race)).OrderBy(k => k.Key).LastOrDefault().Value;
        if (RacialCD() == 0 && RacialSettings())
        {
            SpellManager.CastSpellByNameLUA(Racial);
            Thread.Sleep(Others.Random(150 * 10, 200 * 10));
        }
    }

    public bool RacialSettings()
    {
        var Hostile = ObjectManager.GetWoWUnitHostile();
        var Race = ObjectManager.Me.PlayerRaceString;
        var Distance = ObjectManager.Target.GetDistance;

        if (Race == "Draenei" )
        {
            if (ObjectManager.Me.HealthPercent <= 55 && ObjectManager.Me.InCombat)
                return true;     
        }
        else if(Race == "Gnome")
        {
            if (ObjectManager.Me.InCombat && (ObjectManager.Me.Rooted || ObjectManager.Me.SpeedMoving < 1))
                return true;
        }
        else if (Race == "Dwarf")
        {
            if (ObjectManager.Me.HaveBuff("Rabies") || ObjectManager.Me.HaveBuff("Tetanus"))
                return true;
        }        
        else if (Race == "Night Elf")
        {
            if (!ObjectManager.Me.InCombat && ObjectManager.Me.HealthPercent < 35)
                return true;
        }
        else if (Race == "Human")
        {
            if (ObjectManager.Me.InCombat && (ObjectManager.Me.Possessed || !ObjectManager.Me.CanMove))
                return true;
        }
        else if (Race =="Orc")
        {            
            if (ObjectManager.Me.InCombat && ObjectManager.Target.HealthPercent >= 80 || Hostile.Count > 1)
                return true;
        }
        else if (Race =="Troll")
        {
            if (ObjectManager.Me.InCombat && ObjectManager.Target.HealthPercent >= 80 || Hostile.Count > 1)
                return true;
        }
        else if (Race == "Undead")
        {
            if (ObjectManager.Me.InCombat && (ObjectManager.Me.Possessed || !ObjectManager.Me.CanMove))
                return true;
        }
        else if (Race == "Blood Elf")
        {
            if ((ObjectManager.Target.IsCast && ObjectManager.Me.InCombat && Distance <= 8) || (ObjectManager.Me.InCombat && ObjectManager.Me.RunicPower < 10 || ObjectManager.Me.Energy < 10 || ObjectManager.Me.ManaPercentage < 6))
                return true;
        }
        else if (Race == "Tauren")
        {
            if ((ObjectManager.Target.IsCast && ObjectManager.Me.InCombat && Distance <= 8) || (Hostile.Count >= 3))
                return true;
        }
        return false;
    }
    
    public int RacialCD()
    {
        var Race = ObjectManager.Me.PlayerRaceString;
        var Racial = Racials.Where(k => k.Key.Equals(Race)).OrderBy(k => k.Key).LastOrDefault().Value;
        var RacialCD = SpellManager.GetSpellCooldownTimeLeftBySpellName(Racial);
        return RacialCD;
    }

    public int BagValue()
    {
        List<int> ValueList = new List<int>();
        foreach (var Item in Bag.GetBagItem())
        {
            var GetStackCount = ItemsManager.GetItemCountByNameLUA(Item.Name);
            var CurrentBagValue = ItemsValue.Where(i => i.Key == Item.Name).OrderBy(i => i.Key).LastOrDefault().Value;
            ValueList.Add(CurrentBagValue * GetStackCount);
        }
        return ValueList.Sum();
    }

    public long CurrentMoney()
    {
        var Money = ObjectManager.Me.GetMoneyCopper;
        return Money;
    }

    public double TotalMoney()
    {
        var TotalMoney = CurrentMoney() + BagValue();
        return TotalMoney;
    }

    public static void CheckLevel()
    {
        uint Level = ObjectManager.Me.Level;
        if (Level <= 10 || Level == 15 || Level == 20 || Level == 30 || Level == 40 || Level == 50 || Level == 60)
        {
            BotSettings();
        }
    }

    private void ManageAmmo()
    {
        wManagerSetting.CurrentSetting.MinFreeBagSlotsToGoToTown = _defaultMinSlots + Bag.GetContainerNumFreeSlotsByBagID(_ammoBagId);
    }

    public int GetAmmoContainerSlotId()
    {
        return Lua.LuaDoString<int>(@"
                for slot = 20,23 do
                    local link = GetInventoryItemLink('player', slot);
                    if link then
                        local _, _, _, _, _, itemType = GetItemInfo(link);
						if itemType == 'Quiver' then
                            return slot - 19; -- gets back slot
                        end
                    end
                end    
            ");
    }

    public static  void DeleteItems(string itemName, int leaveAmount)
    {
        var GetItemQuantity = ItemsManager.GetItemCountByNameLUA(itemName);
        var itemQuantity = GetItemQuantity - leaveAmount;
        if (string.IsNullOrWhiteSpace(itemName) || itemQuantity <= 0)
            return;
        var execute =
            "local itemCount = " + itemQuantity + "; " +
            "local deleted = 0; " +
            "for b=0,4 do " +
                "if GetBagName(b) then " +
                    "for s=1, GetContainerNumSlots(b) do " +
                        "local itemLink = GetContainerItemLink(b, s) " +
                        "if itemLink then " +
                            "local _, stackCount = GetContainerItemInfo(b, s)\t " +
                            "local leftItems = itemCount - deleted; " +
                            "if string.find(itemLink, \"" + itemName + "\") and leftItems > 0 then " +
                                "if stackCount <= 1 then " +
                                    "PickupContainerItem(b, s); " +
                                    "DeleteCursorItem(); " +
                                    "deleted = deleted + 1; " +
                                "else " +
                                    "if (leftItems > stackCount) then " +
                                        "SplitContainerItem(b, s, stackCount); " +
                                        "DeleteCursorItem(); " +
                                        "deleted = deleted + stackCount; " +
                                    "else " +
                                        "SplitContainerItem(b, s, leftItems); " +
                                        "DeleteCursorItem(); " +
                                        "deleted = deleted + leftItems; " +
                                    "end " +
                                "end " +
                            "end " +
                        "end " +
                    "end " +
                "end " +
            "end; ";
        Lua.LuaDoString(execute);
    }

    public class CerberusSettings : Settings
    {
        public CerberusSettings()
        {
            BotSettings = true;
            ItemOpener = true;
            DeleteQuestItems = true;
            StartAreaHelper = true;
            RacialUser = true;
            ItemEquip = true;
            ArmyKnife = false;
            MiningTools = true;
            HerbingTools = true;
            AlchemyTools = true;
            EnchantingTools = true;
            EnchantingTools2 = true;
            EnchantingTools3 = true;
            EnchantingTools4 = true;
            SkinningTools = true;
            SkinningTools2 = true;
            SkinningTools3 = true;
            SkinningTools4 = true;
            BlackSmithingTools = true;
            InscriptionTools = true;
            JewelCraftingTools = true;
            FishingTools = true;
            Water = true;
            Earth = true;
            Fire = true;
            Air = true;
            Undeath = true;
            Life = true;
            Shadow = true;
            Mana = true;
            Nether = true;
            Might = true;
            LinenWool = true;
            SilkMage = true;
            NetherRune = true;
            FrostWeave = true;
            SpecialCloth = true;
            OreAndBar = true;
            OreAndBar2 = true;
            OreAndBar3 = true;
            Bag1 = true;
            Bag2 = false;
            Bag3 = false;
            Bag4 = false;
            Bag5 = false;
            Bag6 = false;
            RaidStuff = true;
            RaidStuff2 = true;
            RaidStuff3 = true;
            RaidStuff4 = true;
            Repclassic = true;
            Reptbc = true;
            Repwotlk = true;
            ConfigWinForm(new Point(600, 800), Translate.Get("Settings"), false);
        }

        public static CerberusSettings CurrentSetting { get; set; }


        public bool Save()
        {
            try
            {
                return Save(AdviserFilePathAndName("Cerberus", ObjectManager.Me.Name + "." + Usefuls.RealmName));
            }
            catch (Exception e)
            {
                Logging.WriteDebug("CerberusSettings => Save(): " + e);
                return false;
            }
        }

        public static bool Load()
        {
            try
            {
                if (File.Exists(AdviserFilePathAndName("Cerberus", ObjectManager.Me.Name + "." + Usefuls.RealmName)))
                {
                    CurrentSetting =
                        Load<CerberusSettings>(AdviserFilePathAndName("Cerberus",
                                                                        ObjectManager.Me.Name + "." + Usefuls.RealmName));
                    return true;
                }
                CurrentSetting = new CerberusSettings();
            }
            catch (Exception e)
            {
                Logging.WriteDebug("Cerberus => Load(): " + e);
            }
            return false;
        }

        [Category("00) ----Settings----")]
        [DisplayName("01) Automatic setup of WRobot Settings")]
        [Description("Automaticly sets up your bot settings for optimal botting ")]
        public bool BotSettings { get; set; }

        [Category("00) ----Settings----")]
        [DisplayName("02) Start Area Helper")]
        [Description("Make sure u have money for training skills etc")]
        public bool StartAreaHelper { get; set; }

        [Category("00) ----Settings----")]
        [DisplayName("03) Racial Skill Manager")]
        [Description("Uses your character´s racial skill")]
        public bool RacialUser { get; set; }

        [Category("00) ----Settings----")]
        [DisplayName("04) Quest Item Deleter")]
        [Description("Automaticly delete quest items u pick up from mobs   **Disable this if u use Quester Profile**")]
        public bool DeleteQuestItems { get; set; }

        [Category("00) ----Settings----")]
        [DisplayName("05) Clam and Box Opener")]
        [Description("Automaticly opens up and loot Clam´s, Bag´s and Footlocker´s ")]
        public bool ItemOpener { get; set; }

        [Category("00) ----Settings----")]
        [DisplayName("06) Item and Bag Equip")]
        [Description("Automaticly equip´s better items and bags ")]
        public bool ItemEquip { get; set; }

        [Category("00) ----Settings----")]
        [DisplayName("08) Do you have Gnomish Army Knife ?")]
        [Description("Sell all other profession tools except Gnomish Army Knife and Philosopher's Stone")]
        public bool ArmyKnife { get; set; }

        //Profession Trade Goods and Tools Window
        [Category("01) ----Profession Trade Goods and Tools----")]
        [DisplayName("02) Keep mining tools and stones ?")]
        [Description("Mining Pick, Rough Stone, Rough Grinding Stone, Coarse Stone, Coarse Grinding Stone, Heavy Stone, Heavy Grinding Stone, Solid Stone, Solid Grinding Stone, Dense Stone, Dense Grinding Stone, Guardian Stone")]
        public bool MiningTools { get; set; }

        [Category("01) ----Profession Trade Goods and Tools----")]
        [DisplayName("03) Keep herbalism tools ?")]
        [Description("Herbalist's Spade,  Herbalist's Gloves, Fel Blossom")]
        public bool HerbingTools { get; set; }

        [Category("01) ----Profession Trade Goods and Tools----")]
        [DisplayName("04) Keep alchemy tools and vials ?")]
        [Description("Philosopher's Stone, Empty Vial, Leaded Vial, Crystal Vial, Imbued Vial, Enchanted Vial")]
        public bool AlchemyTools { get; set; }

        [Category("01) ----Profession Trade Goods and Tools----")]
        [DisplayName("05) Keep enchanting tools ?")]
        [Description("Runed Copper Rod, Runed Silver Rod, Runed Golden Rod, Runed Truesilver Rod, Runed Arcanite Rod, Runed Fel Iron Rod, Runed Eternium Rod, Runed Adamantite Rod, Runed Titanium Rod")]
        public bool EnchantingTools { get; set; }

        [Category("01) ----Profession Trade Goods and Tools----")]
        [DisplayName("06) Keep enchanting materials 1-200 ?")]
        [Description("Strange Dust, Lesser Magic Essence, Greater Magic Essence, Lesser Astral Essence, Small Glimmering Shard, Soul Dust, Greater Astral Essence, Large Glimmering Shard, Lesser Mystic Essence, Small Glowing Shard")]
        public bool EnchantingTools2 { get; set; }

        [Category("01) ----Profession Trade Goods and Tools----")]
        [DisplayName("07) Keep enchanting materials 200-300 ?")]
        [Description("Vision Dust, Greater Mystic Essence, Large Glowing Shard, Lesser Nether Essence, Small Radiant Shard, Dream Dust, Greater Nether Essence, Large Radiant Shard, Light Illusion Dust, Lesser Eternal Essence, Small Brilliant Shard, Rich Illusion Dust, Greater Eternal Essence, Large Brilliant Shard")]
        public bool EnchantingTools3 { get; set; }

        [Category("01) ----Profession Trade Goods and Tools----")]
        [DisplayName("08) Keep enchanting materials 300-450 ?")]
        [Description("Arcane Dust, Lesser Planar Essence, Nexus Crystal, Greater Planar Essence, Small Prismatic Shard, Infinite Dust, Lesser Cosmic Essence, Large Prismatic Shard, Void Crystal, Greater Cosmic Essence, Titanium Powder, Small Dream Shard, Dream Shard, Abyss Crystal")]
        public bool EnchantingTools4 { get; set; }

        [Category("01) ----Profession Trade Goods and Tools----")]
        [DisplayName("09) Keep skinning tools ?")]
        [Description("Skinning Knife, Zulian Slicer, Finkle's Skinner")]
        public bool SkinningTools { get; set; }

        [Category("01) ----Profession Trade Goods and Tools----")]
        [DisplayName("10) Keep leatherworking materials 1-200 ?")]
        [Description("Light Leather, Light Hide, Cured Light Hide, Slimy Murloc Scale, Medium Leather, Medium Hide, Cured Medium Hide, Raptor Hide, Heavy Leather, Heavy Hide, Cured Heavy Hide, Turtle Scale, Thick Murloc Scale")]
        public bool SkinningTools2 { get; set; }

        [Category("01) ----Profession Trade Goods and Tools----")]
        [DisplayName("11) Keep leatherworking materials 200-300 ?")]
        [Description("Thick Leather, Thick Hide, Cured Thick Hide, Worn Dragonscale, 	Scorpid Scale, Warbear Leather, Rugged Leather, Rugged Hide, Green Dragonscale, Devilsaur Leather, Cured Rugged Hide, Blue Dragonscale, Heavy Scorpid Scale, Enchanted Leather, Black Dragonscale, Scale of Onyxia, Dreamscale, Core Leather, Primal Tiger Leather, Primal Bat Leather")]
        public bool SkinningTools3 { get; set; }

        [Category("01) ----Profession Trade Goods and Tools----")]
        [DisplayName("12) Keep leatherworking materials 300-450 ?")]
        [Description("Heavy Knothide Leather, Wind Scales, Wind Scale Fragment, Thick Clefthoof Leather, Red Dragonscale,  Patch of Thick Clefthoof Leather, Patch of Crystal Infused Leather, Nether Dragonscales, Nether Dragonscale Fragment, Knothide Leather Scraps, Knothide Leather, Fel Scales, Fel Scale Fragment, Crystal Infused Leather, , Cobra Scales, Cobra Scale Fragment, Borean Leather Scraps, Borean Leather")]
        public bool SkinningTools4 { get; set; }

        [Category("01) ----Profession Trade Goods and Tools----")]
        [DisplayName("13) Keep Blacksmithing Tools ?")]
        [Description("All Blacksmithing Tools")]
        public bool BlackSmithingTools { get; set; }

        [Category("01) ----Profession Trade Goods and Tools----")]
        [DisplayName("15) Keep Inscription Tools and Tradegoods ?")]
        [Description("All Tools, Ink´s and Pigments")]
        public bool InscriptionTools { get; set; }

        [Category("01) ----Profession Trade Goods and Tools----")]
        [DisplayName("16) Keep Jewel Crafting, Tools and Tradegoods ?")]
        [Description("All Tools and Gems")]
        public bool JewelCraftingTools { get; set; }

        [Category("01) ----Profession Trade Goods and Tools----")]
        [DisplayName("17) Keep Fishing Tools and Tradegoods ?")]
        [Description("All Poles, Bait, and Gear")]
        public bool FishingTools { get; set; }

        //Elemental Trade Goods
        [Category("02) ----Elemental Trade Goods----")]
        [DisplayName("01) Keep Water Trade Goods ?")]
        [Description("Elemental Water, Essence of Water, Globe of Water, Mote of Water, Primal Water, Crystallized Water, Eternal Water")]
        public bool Water { get; set; }

        [Category("02) ----Elemental Trade Goods----")]
        [DisplayName("02) Keep Earth Trade Goods ?")]
        [Description("Elemental Earth, Essence of Earth, Core of Earth, Mote of Earth, Primal Earth, Crystallized Earth, Eternal Earth")]
        public bool Earth { get; set; }

        [Category("02) ----Elemental Trade Goods----")]
        [DisplayName("03) Keep Fire Trade Goods ?")]
        [Description("Elemental Fire, Essence of Fire, Heart of Fire, Mote of Fire, Primal Fire, Crystallized Fire, Eternal Fire")]
        public bool Fire { get; set; }

        [Category("02) ----Elemental Trade Goods----")]
        [DisplayName("04) Keep Air Trade Goods ?")]
        [Description("Elemental Air, Essence of Air, Breath of Wind, Mote of Air, Primal Air, Crystallized Air, Eternal Air")]
        public bool Air { get; set; }

        [Category("02) ----Elemental Trade Goods----")]
        [DisplayName("05) Keep Undeath Trade Goods ?")]
        [Description("Ichor of Undeath, Essence of Undeath")]
        public bool Undeath { get; set; }

        [Category("02) ----Elemental Trade Goods----")]
        [DisplayName("06) Keep Life Trade Goods ?")]
        [Description("Heart of the Wild, Living Essence, Mote of Life, Primal Life, Crystallized Life, Eternal Life")]
        public bool Life { get; set; }

        [Category("02) ----Elemental Trade Goods----")]
        [DisplayName("07) Keep Shadow Trade Goods")]
        [Description("Mote of Shadow, Primal Shadow, Crystallized Shadow, Eternal Shadow")]
        public bool Shadow { get; set; }

        [Category("02) ----Elemental Trade Goods----")]
        [DisplayName("08) Keep Mana Trade Goods")]
        [Description("Mote of Mana, Primal Mana, Eternal Mana")]
        public bool Mana { get; set; }

        [Category("02) ----Elemental Trade Goods----")]
        [DisplayName("09) Keep Nether Trade Goods ?")]
        [Description("Primal Nether, Nether Vortex")]
        public bool Nether { get; set; }

        [Category("02) ----Elemental Trade Goods----")]
        [DisplayName("10) Keep Might Trade Goods ?")]
        [Description("Primal Might")]
        public bool Might { get; set; }

        //Cloth
        [Category("03) ----Cloth----")]
        [DisplayName("01) Keep Linen Cloth and Wool Cloth ?")]
        [Description("Linen Cloth and Wool Cloth")]
        public bool LinenWool { get; set; }

        [Category("03) ----Cloth----")]
        [DisplayName("02) Keep Silk Cloth and Mageweave Cloth ?")]
        [Description("Silk Cloth and Mageweave Cloth")]
        public bool SilkMage { get; set; }

        [Category("03) ----Cloth----")]
        [DisplayName("03) Keep Runecloth and Mageweave Cloth ?")]
        [Description("Runecloth and Mageweave Cloth")]
        public bool NetherRune { get; set; }

        [Category("03) ----Cloth----")]
        [DisplayName("04) Keep Frostweave Cloth ?")]
        [Description("Frostweave Cloth")]
        public bool FrostWeave { get; set; }

        [Category("03) ----Cloth----")]
        [DisplayName("05) Keep Special Clot ?")]
        [Description("Felcloth, Mooncloth, Spellcloth, Shadowcloth, Primal Mooncloth, Spellweave, Moonshroud, Ebonweave")]
        public bool SpecialCloth { get; set; }

        //Ore and Bar´s
        [Category("04) ----Ore and Bar´s----")]
        [DisplayName("01) Keep 1-200 ?")]
        [Description("Copper Ore, Copper Bar, Silver Ore, Silver Bar, Tin Ore, Tin Bar, Gold Ore, Gold Bar, Iron Ore, Iron Bar, Steel Bar")]
        public bool OreAndBar { get; set; }

        [Category("04) ----Ore and Bar´s----")]
        [DisplayName("02) Keep 200-300 ?")]
        [Description("Thorium Ore, Thorium Bar, Mithril Ore, Mithril Bar, Truesilver Ore, Truesilver Bar, Dark Iron Ore, Dark Iron Bar, Enchanted Thorium Bar, Arcanite Bar")]
        public bool OreAndBar2 { get; set; }

        [Category("04) ----Ore and Bar´s----")]
        [DisplayName("03) Keep 300-450 ?")]
        [Description("Small Obsidian Shard, Large Obsidian Shard, Fel Iron Ore, Fel Iron Bar, Felsteel Bar, Sulfuron Ingot, Elementium Ingot, Enchanted Elementium Bar, Adamantite Ore, Adamantite Bar, Hardened Adamantite Bar, Khorium Ore, Khorium Bar, Hardened Khorium, Eternium Ore,  Eternium Bar, Cobalt Ore, Cobalt Bar, Saronite Ore, Saronite Bar, Titansteel Bar, Titanium Ore, Titanium Bar")]
        public bool OreAndBar3 { get; set; }

        // BAGS
        [Category("05) ----Bags----")]
        [DisplayName("01) Keep Best Tradeable Bags in all Expansions ?")]
        [Description("Traveler's Backpack, Mooncloth Bag, Pumpkin Bag, Bottomless Bag, Core Felcloth Bag, Sun Touched Satchel, Primal Mooncloth Bag, Abyssal Bag, Glacial Bag")]
        public bool Bag1 { get; set; }

        [Category("05) ----Bags----")]
        [DisplayName("02) Keep 6 and 8 Slot ?")]
        [Description("All 6 and 8 Slot Bags")]
        public bool Bag2 { get; set; }

        [Category("05) ----Bags----")]
        [DisplayName("03) Keep 10 and 12 Slot ?")]
        [Description("All 10 and 12 Slot Bags")]
        public bool Bag3 { get; set; }

        [Category("05) ----Bags----")]
        [DisplayName("04) Keep 14 and 16 Slot ?")]
        [Description("All 14 and 16 Slot Bags")]
        public bool Bag4 { get; set; }

        [Category("05) ----Bags----")]
        [DisplayName("05) Keep 18 and 20 Slot ?")]
        [Description("All 18 and 20 Slot Bags")]
        public bool Bag5 { get; set; }

        [Category("05) ----Bags----")]
        [DisplayName("06) Keep 22 and 24 Slot ?")]
        [Description("All 18 and 20 Slot Bags")]
        public bool Bag6 { get; set; }

        [Category("06) ----Raidstuff----")]
        [DisplayName("01) Keep Flasks, Poisons, Weapon Oils/Stone and Reagents ?")]
        [Description("All Flasks, Poisons, Weapon Oils/Stone and Reagents needed for Raiding in Vanilla, TBC and WotLK")]
        public bool RaidStuff { get; set; }

        [Category("06) ----Raidstuff----")]
        [DisplayName("02) Keep Elixirs and Potions  ? ")]
        [Description("All Elixir´s and Potions´s needed for Raiding in Vanilla, TBC and WotLK")]
        public bool RaidStuff2 { get; set; }

        [Category("06) ----Raidstuff----")]
        [DisplayName("03) Keep Food, Scrolls and Bandages ?")]
        [Description("All Buff Foods, Bandages and Scrolls needed for Raiding in Vanilla, TBC and WotLK")]
        public bool RaidStuff3 { get; set; }

        [Category("06) ----Raidstuff----")]
        [DisplayName("04) Keep Miscellanous Raid Stuff")]
        [Description("Whipper Root Tuber, Drums of Battle, Drums of War, Thistle Tea, Hot Apple Cider, Rumsey Rum Dark, Captain Rumsey's Lager, Field Repair Bot 74A, Field Repair Bot 110G, Scrapbot Construction Kit, Jeeves, Oil of Immolation, Goblin Sapper Charge, Gnomish Battle Chicken, Argent Tournament Invitation")]
        public bool RaidStuff4 { get; set; }

        [Category("07) ----Reputation Items----")]
        [DisplayName("01) Keep Classic items u can deliver for reputation gain")]
        [Description("")]
        public bool Repclassic { get; set; }

        [Category("07) ----Reputation Items----")]
        [DisplayName("02) Keep TBC items u can deliver for reputation gain")]
        [Description("")]
        public bool Reptbc { get; set; }

        [Category("07) ----Reputation Items----")]
        [DisplayName("03) Keep WotLK items u can deliver for reputation gain")]
        [Description("")]
        public bool Repwotlk { get; set; }
    }
}