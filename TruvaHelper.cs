using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using Truva.CampaignBehaviors;

namespace Truva
{
    public class TruvaHelper : MBObjectBase
    {

        public static TruvaTroop FindTruvaTroop(string settlementId)
        {
            TruvaTroop truvaTroop = null;

            for (int i = 0; i < TruvaCampaignBehavior.TruvaTroops.Count; i++)
            {
                if (TruvaCampaignBehavior.TruvaTroops[i].SettlementId == settlementId)
                {
                    truvaTroop = TruvaCampaignBehavior.TruvaTroops[i];
                    return truvaTroop;
                }
            }

            return null;
        }

        public static TruvaTroop FindTruvaTroop(CharacterObject leaderHero)
        {
            TruvaTroop truvaTroop = null;

            for (int i = 0; i < TruvaCampaignBehavior.TruvaTroops.Count; i++)
            {
                for (int j = 0; j < TruvaCampaignBehavior.TruvaTroops[i].MemberCount; j++)
                {
                    if (TruvaCampaignBehavior.TruvaTroops[i].TroopRoster.GetCharacterAtIndex(j) == leaderHero)
                    {
                        truvaTroop = TruvaCampaignBehavior.TruvaTroops[i];
                        return truvaTroop;
                    }
                }
            }

            return null;
        }

        public static List<TruvaTroop> GetTruvaTroopsInFaction(IFaction faction)
        {
            List<TruvaTroop> truvaTroops = new List<TruvaTroop>();

            for (int i = 0; i < TruvaCampaignBehavior.TruvaTroops.Count; i++)
            {
                if (TruvaCampaignBehavior.TruvaTroops[i].SettlementOwner == faction)
                    truvaTroops.Add(TruvaCampaignBehavior.TruvaTroops[i]);
            }

            return truvaTroops;
        }

        public static bool IsHeroInTruvaTroop(CharacterObject character)
        {
            bool result = false;

            for (int i = 0; i < TruvaCampaignBehavior.TruvaTroops.Count; i++)
            {
                if (TruvaCampaignBehavior.TruvaTroops[i].TroopLeader == character.HeroObject)
                {
                    result = true;
                    return result;
                }
            }

            return result;

        }

        public static Tuple<CampaignTime,string,float> GetReachTimeToSettlement(Hero hero,Settlement settlement,float truvaTroopSpeed = 5,float speedMultiplier = 20)
        {
            float distance = Campaign.Current.Models.MapDistanceModel.GetDistance(MobileParty.MainParty,settlement);

            int num = MathF.Ceiling(distance / truvaTroopSpeed);

            CampaignTime total = CampaignTime.HoursFromNow(num);

            string timeText = CampaignUIHelper.GetPartyDistanceByTimeText(distance, 5);
            timeText = (timeText == "") ? timeText = "1 hour" : timeText;

            return new Tuple<CampaignTime, string, float>(total, timeText, distance);
        }


        public static TroopRoster GetNewlyAddedTroopRoster(TroopRoster oldTroopRoster,TroopRoster fullRoster)
        {
            TroopRoster newlyAddedTroops = fullRoster.CloneRosterData();
            for (int i = 0; i < oldTroopRoster.Count; i++)
            {
                if(newlyAddedTroops.Contains(oldTroopRoster.GetElementCopyAtIndex(i).Character))
                {
                    int newNumberOfCharacter = newlyAddedTroops.GetElementCopyAtIndex(newlyAddedTroops.FindIndexOfTroop(oldTroopRoster.GetElementCopyAtIndex(i).Character)).Number;
                    int oldNumberOfCharacter = oldTroopRoster.GetElementCopyAtIndex(i).Number;

                    int numberToRemove = (newNumberOfCharacter > oldNumberOfCharacter) ? oldNumberOfCharacter : newNumberOfCharacter;
                    newlyAddedTroops.RemoveTroop(oldTroopRoster.GetElementCopyAtIndex(i).Character, numberToRemove);
                }
            }

            return newlyAddedTroops;
        }

        public static int GetTroopWage(TroopRoster troopRoster)
        {
            int totalWage = 0;
            for (int i = 0; i < troopRoster.Count; i++)
                totalWage += troopRoster.GetElementCopyAtIndex(i).Character.TroopWage * troopRoster.GetElementCopyAtIndex(i).Number;

            return totalWage;
        }

        public static void ShowTruvaCampaignMessage(string text,CharacterObject leader = null)
        {
            TextObject message = new TextObject(text, null);
            InformationManager.DisplayMessage(new InformationMessage(message.ToString(), Colors.Yellow));
            MBInformationManager.AddQuickInformation(message, 0, leader, "");
        }

    }
}
