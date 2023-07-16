using Bannerlord.ButterLib.ObjectSystem.Extensions;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Truva.Hamony
{
    [HarmonyPatch(typeof(TroopUpgradeTracker), nameof(TroopUpgradeTracker.CheckUpgradedCount))]
    public class CheckUpgradedCountHarm
    {
        static bool Prefix(ref int __result, TroopUpgradeTracker __instance, Dictionary<Tuple<PartyBase, CharacterObject>, int> ____upgradedRegulars, PartyBase party, CharacterObject character)
        {
            //Şuan burası çalışıyorrr
            //__result = 1;
            //BasicCharacterObject troop = Game.Current.ObjectManager.GetObject<BasicCharacterObject>("imperial_elite_cataphract");
            //InformationManager.DisplayMessage(new InformationMessage("aaaaaaaa: " + character.Name, Colors.Green));

            int result = 0;
            if (!character.IsHero)
            {
                int num = 0;
                TroopRoster troopRoster;

                if (character.HasFlag("TruvaTroop"))
                    troopRoster = PartyBase.MainParty.MemberRoster;
                else
                    troopRoster = party.MemberRoster;

                num = troopRoster.FindIndexOfTroop(character);
                int num4;

                if (num >= 0)
                {
                    TroopRosterElement elementCopyAtIndex = troopRoster.GetElementCopyAtIndex(num);

                    var calculateReadyToUpgradeSafe = AccessTools.Method(typeof(TroopUpgradeTracker), "CalculateReadyToUpgradeSafe");

                    int num2 = (int)calculateReadyToUpgradeSafe.Invoke(__instance, new object[] { elementCopyAtIndex, party });

                    int num3;
                    if (____upgradedRegulars.TryGetValue(new Tuple<PartyBase, CharacterObject>(party, character), out num3) && num2 > num3)
                    {
                        num3 = MathF.Min(elementCopyAtIndex.Number, num3);
                        result = num2 - num3;
                        ____upgradedRegulars[new Tuple<PartyBase, CharacterObject>(party, character)] = num2;
                    }
                }
                else if (____upgradedRegulars.TryGetValue(new Tuple<PartyBase, CharacterObject>(party, character), out num4) && num4 > 0)
                {
                    result = -num4;
                }
            }

            __result = result;

            return false;
        }
    }
}
