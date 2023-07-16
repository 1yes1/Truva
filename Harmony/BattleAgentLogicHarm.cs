using HarmonyLib;
using SandBox.Missions.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Truva.Hamony
{
    [HarmonyPatch(typeof(BattleAgentLogic), nameof(BattleAgentLogic.OnAgentBuild))]
    public class BattleAgentLogicHarm
    {
        static bool Prefix(Agent agent, Banner banner, ref TroopUpgradeTracker ____troopUpgradeTracker)
        {
            if (!agent.IsHero)
                return true;

            bool result = TruvaHelper.IsHeroInTruvaTroop((CharacterObject)agent.Character);

            if (agent.Character != null && agent.Origin != null && result)
            {
                PartyBase partyBase = PartyBase.MainParty;
                CharacterObject character = (CharacterObject)agent.Character;
                if (partyBase != null)
                {
                    ____troopUpgradeTracker.AddTrackedTroop(partyBase, character);
                }
            }

            if (result)
                return false;
            else
                return true;
        }
    }
}
