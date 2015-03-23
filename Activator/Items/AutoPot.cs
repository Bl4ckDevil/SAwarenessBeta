﻿using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAssemblies.Activators
{
    internal class AutoPot
    {
        private readonly List<Pot> _pots = new List<Pot>();
        public static Menu.MenuItemSettings AutoPotActivator = new Menu.MenuItemSettings(typeof(AutoPot));

        public AutoPot()
        {
            _pots.Add(new Pot(2037, "PotionOfGiantStrengt", Pot.PotType.Health, 120, 0)); //elixirOfFortitude
            _pots.Add(new Pot(2039, "PotionOfBrilliance", Pot.PotType.Mana, 0, 0)); //elixirOfBrilliance            
            _pots.Add(new Pot(2041, "ItemCrystalFlask", Pot.PotType.Both, 120, 60)); //crystalFlask
            _pots.Add(new Pot(2009, "ItemMiniRegenPotion", Pot.PotType.Both, 80, 50)); //biscuit
            _pots.Add(new Pot(2010, "ItemMiniRegenPotion", Pot.PotType.Both, 170, 10)); //biscuit
            _pots.Add(new Pot(2003, "RegenerationPotion", Pot.PotType.Health, 150, 0)); //healthPotion
            _pots.Add(new Pot(2004, "FlaskOfCrystalWater", Pot.PotType.Mana, 0, 100)); //manaPotion
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~AutoPot()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Activator.Activators.GetActive() && AutoPotActivator.GetActive();
        }

        private static void SetupMainMenu()
        {
            var menu = new LeagueSharp.Common.Menu("SAutoPotActivator", "SAssembliesSActivatorsAutoPotActivator", true);
            SetupMenu(menu);
            menu.AddToMainMenu();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            Menu.MenuItemSettings tempSettings;
            AutoPotActivator.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("ACTIVATORS_AUTOPOT_MAIN"), "SAssembliesActivatorsAutoPot"));
            tempSettings = AutoPotActivator.AddMenuItemSettings(Language.GetString("ACTIVATORS_AUTOPOT_HEALTHPOT_MAIN"), "SAssembliesActivatorsAutoPotHealthPot");
            tempSettings.MenuItems.Add(
                tempSettings.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoPotHealthPotPercent", Language.GetString("ACTIVATORS_AUTOPOT_HEALTHPOT_PERCENT")).SetValue(new Slider(20, 99,
                        0))));
            tempSettings.MenuItems.Add(
                tempSettings.Menu.AddItem(new MenuItem("SAssembliesActivatorsAutoPotHealthPotActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            tempSettings = AutoPotActivator.AddMenuItemSettings(Language.GetString("ACTIVATORS_AUTOPOT_MANAPOT_MAIN"), "SAssembliesActivatorsAutoPotManaPot");
            tempSettings.MenuItems.Add(
                tempSettings.Menu.AddItem(
                    new MenuItem("SAssembliesActivatorsAutoPotManaPotPercent", Language.GetString("ACTIVATORS_AUTOPOT_MANAPOT_PERCENT")).SetValue(new Slider(20, 99, 0))));
            tempSettings.MenuItems.Add(
                tempSettings.Menu.AddItem(new MenuItem("SAssembliesActivatorsAutoPotManaPotActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            AutoPotActivator.MenuItems.Add(
                AutoPotActivator.Menu.AddItem(new MenuItem("SAssembliesActivatorsAutoPotOverusage", Language.GetString("ACTIVATORS_AUTOPOT_PREVENTOVERUSAGE")).SetValue(false)));
            AutoPotActivator.MenuItems.Add(
                AutoPotActivator.Menu.AddItem(new MenuItem("SAssembliesActivatorsAutoPotActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return AutoPotActivator;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || ObjectManager.Player.IsDead || ObjectManager.Player.InFountain() ||
                ObjectManager.Player.HasBuff("Recall") || ObjectManager.Player.HasBuff("SummonerTeleport") ||
                ObjectManager.Player.HasBuff("RecallImproved") ||
                ObjectManager.Player.ServerPosition.CountEnemiesInRange(1500) > 0)
                return;
            Pot myPot = null;
            if (
                AutoPotActivator.GetMenuSettings("SAssembliesActivatorsAutoPotHealthPot")
                    .GetMenuItem("SAssembliesActivatorsAutoPotHealthPotActive")
                    .GetValue<bool>())
            {
                foreach (Pot pot in _pots)
                {
                    if (pot.Type == Pot.PotType.Health || pot.Type == Pot.PotType.Both)
                    {
                        if (ObjectManager.Player.Health/ObjectManager.Player.MaxHealth*100 <=
                            AutoPotActivator.GetMenuSettings("SAssembliesActivatorsAutoPotHealthPot")
                                .GetMenuItem("SAssembliesActivatorsAutoPotHealthPotPercent")
                                .GetValue<Slider>().Value)
                        {
                            if (AutoPotActivator.GetMenuItem("SAssembliesActivatorsAutoPotOverusage").GetValue<bool>() &&
                                ObjectManager.Player.Health + pot.Health >= ObjectManager.Player.MaxHealth)
                                continue;
                            if (!Items.HasItem(pot.Id))
                                continue;
                            if (!Items.CanUseItem(pot.Id))
                                continue;
                            myPot = pot;
                            break;
                        }
                    }
                }
            }
            if (myPot != null)
                UsePot(myPot);
            if (
                AutoPotActivator.GetMenuSettings("SAssembliesActivatorsAutoPotManaPot")
                    .GetMenuItem("SAssembliesActivatorsAutoPotManaPotActive")
                    .GetValue<bool>())
            {
                foreach (Pot pot in _pots)
                {
                    if (pot.Type == Pot.PotType.Mana || pot.Type == Pot.PotType.Both)
                    {
                        if (ObjectManager.Player.Mana/ObjectManager.Player.MaxMana*100 <=
                            AutoPotActivator.GetMenuSettings("SAssembliesActivatorsAutoPotManaPot")
                                .GetMenuItem("SAssembliesActivatorsAutoPotManaPotPercent")
                                .GetValue<Slider>().Value)
                        {
                            if (AutoPotActivator.GetMenuItem("SAssembliesActivatorsAutoPotOverusage").GetValue<bool>() &&
                                ObjectManager.Player.Mana + pot.Mana >= ObjectManager.Player.MaxMana)
                                continue;
                            if (!Items.HasItem(pot.Id))
                                continue;
                            if (!Items.CanUseItem(pot.Id))
                                continue;
                            myPot = pot;
                            break;
                        }
                    }
                }
            }
            if (myPot != null)
                UsePot(myPot);
        }

        private void UsePot(Pot pot)
        {
            foreach (BuffInstance buff in ObjectManager.Player.Buffs)
            {
                Console.WriteLine(buff.Name);
                if (buff.Name.Contains(pot.Buff))
                {
                    return;
                }
            }
            if (pot.LastTime + 5 > Game.Time)
                return;
            if (!Items.HasItem(pot.Id))
                return;
            if (!Items.CanUseItem(pot.Id))
                return;
            Items.UseItem(pot.Id);
            pot.LastTime = Game.Time;
        }

        public class Pot
        {
            public enum PotType
            {
                None,
                Health,
                Mana,
                Both
            }

            public String Buff;
            public int Id;
            public float LastTime;
            public PotType Type;
            public int Health;
            public int Mana;

            public Pot()
            {
            }

            public Pot(int id, String buff, PotType type, int health, int mana)
            {
                Id = id;
                Buff = buff;
                Type = type;
                Health = health;
                Mana = mana;
            }
        }
    }
}