using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using Truva.ViewModel;

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



    }
}
