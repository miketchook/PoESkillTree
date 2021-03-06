using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Data;
using POESKillTree.Localization;
using POESKillTree.ViewModels;

namespace POESKillTree.Utils.Converter
{
    [ValueConversion(typeof (string), typeof (string))]
    //list view sorter here
    public class GroupStringConverter : IValueConverter, IComparer
    {
        public Dictionary<string, AttributeGroup> AttributeGroups = new Dictionary<string, AttributeGroup>();
        private List<string[]> CustomGroups = new List<string[]>();
        private static readonly string Keystone = L10n.Message("Keystone");
        private static readonly string Weapon = L10n.Message("Weapon");
        private static readonly string Charges = L10n.Message("Charges");
        private static readonly string Minion = L10n.Message("Minion");
        private static readonly string Trap = L10n.Message("Trap");
        private static readonly string Totem = L10n.Message("Totem");
        private static readonly string Curse = L10n.Message("Curse");
        private static readonly string Aura = L10n.Message("Aura");
        private static readonly string CriticalStrike = L10n.Message("Critical Strike");
        private static readonly string Shield = L10n.Message("Shield");
        private static readonly string Block = L10n.Message("Block");
        private static readonly string General = L10n.Message("General");
        private static readonly string Defense = L10n.Message("Defense");
        private static readonly string Spell = L10n.Message("Spell");
        private static readonly string CoreAttributes = L10n.Message("Core Attributes");
        private static readonly string MiscLabel = "Everything Else";
        private static readonly string DecimalRegex = "\\d+(\\.\\d*)?";
        private static readonly List<string[]> DefaultGroups = new List<string[]>
        {
            new[] {"Share Endurance, Frenzy and Power Charges with nearby party members", Keystone},
            new[] {"and Endurance Charges on Hit with Claws", Weapon},
            new[] {"Endurance Charge", Charges},
            new[] {"Frenzy Charge", Charges},
            new[] {"Power Charge", Charges},
            new[] {"Chance to Dodge", Keystone},
            new[] {"Spell Damage when on Low Life", Keystone},
            new[] {"Cold Damage Converted to Fire Damage", Keystone},
            new[] {"Lightning Damage Converted to Fire Damage", Keystone},
            new[] {"Physical Damage Converted to Fire Damage", Keystone},
            new[] {"Deal no Non-Fire Damage", Keystone},
            new[] {"All bonuses from an equipped Shield apply to your Minions instead of you", Keystone},
            new[] {"additional totem", Keystone},
            new[] {"Cannot be Stunned", Keystone},
            new[] {"Cannot Evade enemy Attacks", Keystone},
            new[] {"Spend Energy Shield before Mana for Skill Costs", Keystone},
            new[] {"Energy Shield protects Mana instead of Life", Keystone},
            new[] {"Converts all Evasion Rating to Armour. Dexterity provides no bonus to Evasion Rating", Keystone},
            new[] {"Doubles chance to Evade Projectile Attacks", Keystone},
            new[] {"Enemies you hit with Elemental Damage temporarily get", Keystone},
            new[] {"Life Leech applies instantly", Keystone},
            new[] {"Life Leech applies to Energy Shield instead of Life", Keystone},
            new[] {"Life Regeneration applies to Energy Shield instead of Life", Keystone},
            new[] {"Immune to Chaos Damage", Keystone},
            new[] {"Minions explode when reduced to low life", Keystone},
            new[] {"Never deal Critical Strikes", Keystone},
            new[] {"Projectile Attacks deal up to", Keystone},
            new[] {"Removes all mana", Keystone},
            new[] {"Share Endurance", Keystone},
            new[] {"The increase to Physical Damage from Strength applies to Projectile Attacks as well as Melee Attacks", Keystone},
            new[] {"Damage is taken from Mana before Life", Keystone},
            new[] {"You can't deal Damage with your Skills yourself", Keystone},
            new[] {"Your hits can't be Evaded", Keystone},
            new[] {"Maximum number of Spectres", Minion},
            new[] {"Maximum number of Zombies", Minion},
            new[] {"Maximum number of Skeletons", Minion},
            new[] {"Minions deal", Minion},
            new[] {"Minions have", Minion},
            new[] {"Minions Leech", Minion},
            new[] {"Minions Regenerate", Minion},
            new[] {"Mine Damage", Trap},
            new[] {"Trap Damage", Trap},
            new[] {"Trap Duration", Trap},
            new[] {"Trap Trigger Radius", Trap},
            new[] {"Mine Duration", Trap},
            new[] {"Mine Laying Speed", Trap},
            new[] {"Trap Throwing Speed", Trap},
            new[] {"Can set up to", Trap},
            new[] {"Detonating Mines is Instant", Trap},
            new[] {"Mine Damage Penetrates", Trap},
            new[] {"Mines cannot be Damaged", Trap},
            new[] {"Trap Damage Penetrates", Trap},
            new[] {"Traps cannot be Damaged", Trap},
            new[] {"Totem Duration", Totem},
            new[] {"Casting Speed for Summoning Totems", Totem},
            new[] {"Totem Life", Totem},
            new[] {"Totem Damage", Totem},
            new[] {"Attacks used by Totems", Totem},
            new[] {"Spells Cast by Totems", Totem},
            new[] {"Totems gain", Totem},
            new[] {"Totems have", Totem},
            new[] {"Curse Duration", Curse},
            new[] {"Effect of your Curses", Curse},
            new[] {"Radius of Curses", Curse},
            new[] {"Cast Speed for Curses", Curse},
            new[] {"Enemies can have 1 additional Curse", Curse},
            new[] {"Mana Reserved", Aura},
            new[] {"effect of Auras", Aura},
            new[] {"Radius of Auras", Aura},
            new[] {"Effect of Buffs on You", Aura},
            new[] {"Weapon Critical Strike Chance", CriticalStrike},
            new[] {"increased Critical Strike Chance", CriticalStrike},
            new[] {"to Critical Strike Multiplier", CriticalStrike},
            new[] {"Global Critical Strike", CriticalStrike},
            new[] {"Critical Strikes with Daggers Poison the enemy", CriticalStrike},
            new[] {"Knocks Back enemies if you get a Critical Strike", CriticalStrike},
            new[] {"increased Melee Critical Strike Multiplier", CriticalStrike},
            new[] {"increased Melee Critical Strike Chance", CriticalStrike},
            new[] {"Elemental Resistances while holding a Shield", Shield},
            new[] {"Chance to Block Spells with Shields", Block},
            new[] {"Armour from equipped Shield", Shield},
            new[] {"additional Block Chance while Dual Wielding or Holding a shield", Block},
            new[] {"Chance to Block with Shields", Block},
            new[] {"Block and Stun Recovery", Block},
            new[] {"Energy Shield from equipped Shield", Shield},
            new[] {"Block Recovery", Block},
            new[] {"Defences from equipped Shield", Shield},
            new[] {"Damage Penetrates", General}, //needs to be here to pull into the correct tab.
            new[] {"reduced Extra Damage from Critical Strikes", Defense},
            new[] {"Armour", Defense},
            new[] {"all Elemental Resistances", Defense},
            new[] {"Chaos Resistance", Defense},
            new[] {"Evasion Rating", Defense},
            new[] {"Cold Resistance", Defense},
            new[] {"Lightning Resistance", Defense},
            new[] {"maximum Mana", Defense},
            new[] {"maximum Energy Shield", Defense},
            new[] {"Fire Resistance", Defense},
            new[] {"maximum Life", Defense},
            new[] {"Light Radius", Defense},
            new[] {"Evasion Rating and Armour", Defense},
            new[] {"Energy Shield Recharge", Defense},
            new[] {"Life Regenerated", Defense},
            new[] {"Melee Physical Damage taken reflected to Attacker", Defense},
            new[] {"Flask Recovery Speed", Defense},
            new[] {"Avoid Elemental Status Ailments", Defense},
            new[] {"Damage taken Gained as Mana when Hit", Defense},
            new[] {"Avoid being Chilled", Defense},
            new[] {"Avoid being Frozen", Defense},
            new[] {"Avoid being Ignited", Defense},
            new[] {"Avoid being Shocked", Defense},
            new[] {"Avoid being Stunned", Defense},
            new[] {"increased Stun Recovery", Defense},
            new[] {"Flasks", Defense},
            new[] {"Flask effect duration", Defense},
            new[] {"Mana Regeneration Rate", Defense},
            new[] {"maximum Mana", Defense},
            new[] {"Armour", Defense},
            new[] {"Avoid interruption from Stuns while Casting", Defense},
            new[] {"Movement Speed", Defense},
            new[] {"Mana Recovery from Flasks", Defense},
            new[] {"Life Recovery from Flasks", Defense},
            new[] {"Enemies Cannot Leech Life From You", Defense},
            new[] {"Enemies Cannot Leech Mana From You", Defense},
            new[] {"Ignore all Movement Penalties", Defense},
            new[] {"Physical Damage Reduction", Defense},
            new[] {"Hits that Stun Enemies have Culling Strike", General},
            new[] {"increased Damage against Frozen, Shocked or Ignited Enemies", General},
            new[] {"Shock Duration on enemies", General},
            new[] {"Radius of Area Skills", General},
            new[] {"chance to Ignite", General},
            new[] {"chance to Shock", General},
            new[] {"Mana Gained on Kill", General},
            new[] {"Life gained on General", General},
            new[] {"Burning Damage", General},
            new[] {"Projectile Damage", General},
            new[] {"Knock enemies Back on hit", General},
            new[] {"chance to Freeze", General},
            new[] {"Projectile Speed", General},
            new[] {"Projectiles Piercing", General},
            new[] {"Ignite Duration on enemies", General},
            new[] {"Knockback Distance", General},
            new[] {"Mana Cost of Skills", General},
            new[] {"Chill Duration on enemies", General},
            new[] {"Freeze Duration on enemies", General},
            new[] {"Damage over Time", General},
            new[] {"Chaos Damage", General},
            new[] {"Enemies Become Chilled as they Unfreeze", General},
            new[] {"Skill Effect Duration", General},
            new[] {"Life Gained on Kill", General},
            new[] {"Area Damage", General},
            new[] {"Enemy Stun Threshold", General},
            new[] {"Stun Duration", General},
            new[] {"increased Damage against Enemies on Low Life", General},
            new[] {"chance to gain Onslaught", General},
            new[] {"Spell Damage", Spell},
            new[] {"Elemental Damage with Spells", Spell},
            new[] {"Accuracy Rating", Weapon},
            new[] {"Mana gained for each enemy hit by your Attacks", Weapon},
            new[] {"Melee Weapon and Unarmed range", Weapon},
            new[] {"Life gained for each enemy hit by your Attacks", Weapon},
            new[] {"chance to cause Bleeding", Weapon},
            new[] {"Wand Physical Damage", Weapon},
            new[] {"Attack Speed", Weapon},
            new[] {"Melee Attack Speed", Weapon},
            new[] {"Melee Damage", Weapon},
            new[] {"Physical Damage with Claws", Weapon},
            new[] {"Block Chance With Staves", Block},
            new[] {"Physical Damage with Daggers", Weapon},
            new[] {"Physical Attack Damage Leeched as Mana", Weapon},
            new[] {"Physical Damage Dealt with Claws Leeched as Mana", Weapon},
            new[] {"Arrow Speed", Weapon},
            new[] {"Cast Speed while Dual Wielding", Weapon},
            new[] {"Physical Damage with Staves", Weapon},
            new[] {"Attack Damage with Main Hand", Weapon},
            new[] {"Attack Damage against Bleeding Enemies", Weapon},
            new[] {"Physical Damage with Axes", Weapon},
            new[] {"Physical Weapon Damage while Dual Wielding", Weapon},
            new[] {"Physical Damage with One Handed Melee Weapons", Weapon},
            new[] {"Physical Damage with Two Handed Melee Weapons", Weapon},
            new[] {"Physical Damage with Maces", Weapon},
            new[] {"Physical Damage with Bows", Weapon},
            new[] {"enemy chance to Block Sword Attacks", Block},
            new[] {"additional Block Chance while Dual Wielding", Block},
            new[] {"mana gained when you Block", Block},
            new[] {"Melee Physical Damage", Weapon},
            new[] {"Physical Damage with Swords", Weapon},
            new[] {"Elemental Damage with Wands", Weapon},
            new[] {"Elemental Damage with Maces", Weapon},
            new[] {"Physical Attack Damage Leeched as Life", Weapon},
            new[] {"Cold Damage with Weapons", Weapon},
            new[] {"Fire Damage with Weapons", Weapon},
            new[] {"Lightning Damage with Weapons", Weapon},
            new[] {"Physical Damage Dealt with Claws Leeched as Life", Weapon},
            new[] {"Elemental Damage with Weapons", Weapon},
            new[] {"Physical Damage with Wands", Weapon},
            new[] {"Damage with Wands", Weapon},
            new[] {"increased Physical Damage", General},
            new[] {"Elemental Damage", General},
            new[] {"Cast Speed", Spell},
            new[] {"Cold Damage", General},
            new[] {"Fire Damage", General},
            new[] {"Lightning Damage", General},
            new[] {"Strength", CoreAttributes},
            new[] {"Intelligence", CoreAttributes},
            new[] {"Dexterity", CoreAttributes},
        };

        public GroupStringConverter()
        {
            CustomGroups = new List<string[]>();
            foreach (var group in DefaultGroups)
            {
                if (!AttributeGroups.ContainsKey(group[1]))
                {
                    AttributeGroups.Add(group[1], new AttributeGroup(group[1]));
                }
            }
            foreach (var group in CustomGroups)
            {
                if (!AttributeGroups.ContainsKey(group[1]))
                {
                    AttributeGroups.Add(group[1], new AttributeGroup("Custom: "+group[1]));
                }
            }
            AttributeGroups.Add(MiscLabel, new AttributeGroup(MiscLabel));
        }

        public List<string[]> CopyCustomGroups()
        {
            List<string[]> deepcopy = new List<string[]>();
            foreach (var gp in CustomGroups)
            {
                deepcopy.Add((string[])gp.Clone());
            }
            return deepcopy;
        }

        public void ResetGroups(List<string[]> newgroups)
        {
            CustomGroups = new List<string[]>();
            foreach (var gp in newgroups)
            {
                CustomGroups.Add((string[])gp.Clone());
            }

            AttributeGroups = new Dictionary<string, AttributeGroup>();
            foreach (var group in DefaultGroups)
            {
                if (!AttributeGroups.ContainsKey(group[1]))
                {
                    AttributeGroups.Add(group[1], new AttributeGroup(group[1]));
                }
            }
            foreach (var group in CustomGroups)
            {
                if (!AttributeGroups.ContainsKey(group[1]))
                {
                    AttributeGroups.Add(group[1], new AttributeGroup("Custom: " + group[1]));
                }
            }
            AttributeGroups.Add(MiscLabel, new AttributeGroup(MiscLabel));
        }

        public void AddGroup(string groupname, string[] attributes)
        {
            if (!AttributeGroups.ContainsKey(groupname))
            {
                AttributeGroups.Add(groupname, new AttributeGroup("Custom: "+groupname));
            }
            foreach (string attr in attributes)
            {
                AddAttributeToGroup(attr, groupname);
            }
        }

        private void AddAttributeToGroup(string attribute, string groupname)
        {
            //Remove it from any existing custom groups first
            RemoveFromGroup(new string[] { attribute });
            CustomGroups.Insert(0, new string[] { attribute, groupname });
        }

        public void RemoveFromGroup(string[] attributes)
        {
            List<string[]> linesToRemove = new List<string[]>();
            foreach (string attr in attributes)
            {
                foreach (var gp in CustomGroups)
                {
                    if (new NumberLessStringComparer().Compare(attr.ToLower(), gp[0].ToLower()) == 0)
                    {
                        linesToRemove.Add(gp);
                    }
                }
            }
            foreach (string[] line in linesToRemove)
            {
                CustomGroups.Remove(line);
            }
        }

        public void DeleteGroup(string groupname)
        {
            List<string[]> linesToRemove = new List<string[]>();
            foreach (var gp in CustomGroups)
            {
                if (groupname.ToLower().Equals(gp[1].ToLower()))
                {
                    linesToRemove.Add(gp);
                }
            }
            foreach (string[] line in linesToRemove)
            {
                CustomGroups.Remove(line);
            }
            AttributeGroups.Remove(groupname);
        }

        public void UpdateGroupNames(List<POESKillTree.ViewModels.Attribute> attrlist)
        {
            Dictionary<string, float> groupTotals = new Dictionary<string, float>();
            Dictionary<string, float> groupDeltas = new Dictionary<string, float>();
            foreach (var gp in CustomGroups)
            {
                //only sum for the groups that need it
                if (!gp[1].Contains("#"))
                    continue;
                foreach (POESKillTree.ViewModels.Attribute attr in attrlist)
                {
                    if (new NumberLessStringComparer().Compare(attr.Text.ToLower(), gp[0].ToLower()) == 0)
                    {
                        Match matchResult = Regex.Match(attr.Text, DecimalRegex);
                        if (matchResult.Success)
                        {
                            if (!groupTotals.ContainsKey(gp[1]))
                            {
                                groupTotals.Add(gp[1], 0);
                                groupDeltas.Add(gp[1], 0);
                            }
                            groupTotals[gp[1]] += (float)Decimal.Parse(matchResult.Value);
                            if (attr.Deltas.Length > 0)
                                groupDeltas[gp[1]] += attr.Deltas[0];
                        }

                    }
                }
            }

            string deltaString;
            foreach (string key in groupTotals.Keys)
            {
                if (AttributeGroups.ContainsKey(key))
                {
                    if (groupDeltas[key] == 0)
                        deltaString = "";
                    else if (groupDeltas[key] > 0)
                        deltaString = " +" + groupDeltas[key].ToString();
                    else
                        deltaString = " " + groupDeltas[key].ToString();
                    AttributeGroups[key].GroupName = "Custom: "+key.Replace("#", groupTotals[key].ToString())+deltaString;
                }
            }
            
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value.ToString());
        }

        public int Compare(object a, object b)
        {
            string attr1 = ((POESKillTree.ViewModels.Attribute)a).ToString();
            string attr2 = ((POESKillTree.ViewModels.Attribute)b).ToString();
            //find the group names and types that the attributes belong in
            //2 = misc group, 1 = default group, 0 = custom group
            int group1 = 2;
            int group2 = 2;
            string attrgroup1 = MiscLabel;
            string attrgroup2 = MiscLabel;
            foreach (var gp in CustomGroups)
            {
                if (new NumberLessStringComparer().Compare(attr1.ToLower(), gp[0].ToLower()) == 0)
                {
                    attrgroup1 = gp[1];
                    group1 = 0;
                    break;
                }
            }
            if (group1 == 2) { 
                foreach (var gp in DefaultGroups)
                {
                    if (attr1.ToLower().Contains(gp[0].ToLower()))
                    {
                        attrgroup1 = gp[1];
                        group1 = 1;
                        break;
                    }
                }
            }
            foreach (var gp in CustomGroups)
            {
                if (new NumberLessStringComparer().Compare(attr2.ToLower(), gp[0].ToLower()) == 0)
                {
                    attrgroup2 = gp[1];
                    group2 = 0;
                    break;
                }
            }
            if (group2 == 2)
            {
                foreach (var gp in DefaultGroups)
                {
                    if (attr2.ToLower().Contains(gp[0].ToLower()))
                    {
                        attrgroup2 = gp[1];
                        group2 = 1;
                        break;
                    }
                }
            }

            //primary: if group types are different, sort by group type - custom first, then defaults, then misc
            if (group1 != group2)
            {
                return group1 - group2;
            }
            //secondary: if groups are different, sort by group names, alphabetically, excluding numbers
            if (!attrgroup1.Equals(attrgroup2))
            {
                attrgroup1 = Regex.Replace(attrgroup1, DecimalRegex, "#");
                attrgroup2 = Regex.Replace(attrgroup2, DecimalRegex, "#");
                return attrgroup1.CompareTo(attrgroup2);
            }
            //tertiary: if in the same group, sort by attribute string, alphabetically, excluding numbers
            attr1 = Regex.Replace(attr1, DecimalRegex, "#");
            attr2 = Regex.Replace(attr2, DecimalRegex, "#");
            return attr1.CompareTo(attr2);
        }

        public AttributeGroup Convert(string s)
        {
            foreach (var gp in CustomGroups)
            {
                if (new NumberLessStringComparer().Compare(s.ToLower(), gp[0].ToLower()) == 0)
                {
                    return AttributeGroups[gp[1]];
                }
            }
            foreach (var gp in DefaultGroups)
            {
                if (s.ToLower().Contains(gp[0].ToLower()))
                {
                    return AttributeGroups[gp[1]];
                }
            }
            return AttributeGroups[MiscLabel];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}