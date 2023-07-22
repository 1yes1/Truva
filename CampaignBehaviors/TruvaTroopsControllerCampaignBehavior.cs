using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

namespace Truva.CampaignBehaviors
{
    public class TruvaTroopsControllerCampaignBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.MakePeace.AddNonSerializedListener(this, OnMakePeace);
            CampaignEvents.SiegeCompletedEvent.AddNonSerializedListener(this, OnSiegeCompleted);
            CampaignEvents.TownRebelliosStateChanged.AddNonSerializedListener(this, OnTownRebelliousStateChanged);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnMakePeace(IFaction side1Faction, IFaction side2Faction, MakePeaceAction.MakePeaceDetail detail)
        {
            if (side1Faction == Hero.MainHero.MapFaction || side2Faction == Hero.MainHero.MapFaction)
            {
                IFaction otherFaction = side1Faction == Hero.MainHero.MapFaction ? side2Faction : side1Faction;

                List<TruvaTroop> truvaTroops = TruvaHelper.GetTruvaTroopsInFaction(otherFaction);
                if (truvaTroops.Count > 0)
                {
                    for (int i = 0; i < truvaTroops.Count; i++)
                    {
                        Campaign.Current.GetCampaignBehavior<TruvaCampaignBehavior>().RemoveTruvaTroop(truvaTroops[i]);
                    }
                }
            }
        }

        public void OnSiegeCompleted(Settlement settlement, MobileParty capturerParty, bool isWin, MapEvent.BattleTypes battleType)
        {
            TruvaTroop truvaTroop = TruvaHelper.FindTruvaTroop(settlement.StringId);

            //InformationManager.DisplayMessage(new InformationMessage("Siege Completed " + settlement.Name, Colors.Magenta));

            if (truvaTroop != null && isWin && battleType == MapEvent.BattleTypes.Siege)
            {
                if (!capturerParty.MapFaction.IsAtWarWith(MobileParty.MainParty.MapFaction))
                {
                    Campaign.Current.GetCampaignBehavior<TruvaCampaignBehavior>().RemoveTruvaTroop(truvaTroop);
                }
            }
        }


        private void OnTownRebelliousStateChanged(Town town, bool isRebellious)
        {
            TruvaTroop truvaTroop = TruvaHelper.FindTruvaTroop(town.StringId);
            //InformationManager.DisplayMessage(new InformationMessage("Rebellious : " + town.InRebelliousState, Colors.Magenta));
            //InformationManager.DisplayMessage(new InformationMessage("town.OwnerClan : " + town.OwnerClan.Name, Colors.Magenta));

            if (truvaTroop != null && town.InRebelliousState && !town.OwnerClan.MapFaction.IsAtWarWith(MobileParty.MainParty.MapFaction))
            {
                Campaign.Current.GetCampaignBehavior<TruvaCampaignBehavior>().RemoveTruvaTroop(truvaTroop);
            }
        }

    }
}
