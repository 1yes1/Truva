using Bannerlord.ButterLib.ObjectSystem.Extensions;
using SandBox.BoardGames;
using SandBox.View.Map;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;
using Truva.CampaignBehaviors;
using Truva.MissionView;
using Truva.ViewModel;

namespace Truva.MissionBehaviors
{
    public class TruvaSiegeLogic : MissionLogic
    {
        public event Action<List<Agent>, Formation> OnTruvaTroopsSpawnedEvent;

        private bool _isDeploymentFinished = false;

        private int _deadCount = 0;
        private int _woundedCount = 0;


        private List<Agent> _agents = new List<Agent>();

        private Formation _truvaFormation;

        public bool IsTruvaTroopSpawned { get; private set; }

        private TruvaTroop TruvaTroop { get; set; }

        private TruvaSiegeAttackLogic TruvaSiegeAttack { get; set; }

        private TruvaSiegeMissionView TruvaSiegeMV { get; set; }

        public TruvaInputManager InputManager { get; set; }

        public MatrixFrame TruvaSpawnFrame { get; private set; }

        public bool HasTruvaTroop { get => (Settlement.CurrentSettlement != null && TruvaHelper.FindTruvaTroop(Settlement.CurrentSettlement.StringId) != null); }

        public override void AfterStart()
        {
            base.AfterStart();

            InformationManager.DisplayMessage(new InformationMessage("Siege Started", Colors.Red));

            InputManager = Mission.GetMissionBehavior<TruvaInputManager>();

            TruvaSiegeAttack = Mission.GetMissionBehavior<TruvaSiegeAttackLogic>();

            TruvaSiegeMV = Mission.GetMissionBehavior<TruvaSiegeMissionView>();
        }

        public override void OnDeploymentFinished()
        {
            base.OnDeploymentFinished();

            if (!HasTruvaTroop)
                return;

            InputManager.OnKeyCtrlAPressed += SpawnTruvaTroops;
            TruvaSiegeMV.OnTruvaSpawnChooseEvent += OnTruvaSpawnChoose;

            _isDeploymentFinished = true;
        }

        public override void OnRetreatMission()
        {
            if (!HasTruvaTroop)
                return;

            CalculateWoundAndDead();
        }

        public override void OnMissionResultReady(MissionResult missionResult)
        {
            if (!HasTruvaTroop)
                return;

            CalculateWoundAndDead();
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            if (_agents.Contains(affectedAgent) && !affectedAgent.IsHero)
            {
                if (agentState == AgentState.Killed)
                    _deadCount++;
                else if (agentState == AgentState.Unconscious)
                    _woundedCount++;
            }
        }

        private void CalculateWoundAndDead()
        {
            InformationManager.DisplayMessage(new InformationMessage("------------------Calculate-------------", Colors.Yellow));

            InformationManager.DisplayMessage(new InformationMessage("_deadCount: " + _deadCount, Colors.Magenta));
            InformationManager.DisplayMessage(new InformationMessage("_woundedCount: " + _woundedCount, Colors.Magenta));

            TruvaTroop.TroopRoster.KillNumberOfMenRandomly(_deadCount, false);
            TruvaTroop.TroopRoster.WoundNumberOfTroopsRandomly(_woundedCount);
        }

        public override void OnMissionStateFinalized()
        {
            if (!HasTruvaTroop)
                return;

            InformationManager.DisplayMessage(new InformationMessage("Siege Finished- Troops Added To Main", Colors.Red));
            PartyBase.MainParty.MemberRoster.Add(TruvaTroop.TroopRoster);
            Campaign.Current.GetCampaignBehavior<TruvaCampaignBehavior>().RemoveTruvaTroop(TruvaTroop, true);
        }

        private void OnTruvaSpawnChoose(bool spawn)
        {
            if (spawn)
                SpawnTruvaTroops();
        }

        private void SpawnTruvaTroops()
        {
            if (!_isDeploymentFinished)
                return;

            InformationManager.DisplayMessage(new InformationMessage("Truva Troop! Attack!", Colors.Red));

            TruvaTroop = TruvaHelper.FindTruvaTroop(Settlement.CurrentSettlement.StringId);
            TruvaTroop.IsAtWar = true;

            SpawnAgents(TruvaTroop.TroopRoster);

            IsTruvaTroopSpawned = true;

            //Burda almamız lazım ki en yakın kapıyı alabilelim

            OnTruvaTroopsSpawnedEvent?.Invoke(_agents, _truvaFormation);
        }

        private void SpawnAgents(TroopRoster troopRoster)
        {
            if (IsTruvaTroopSpawned)
                return;

            Team team = Mission.AttackerTeam;
            MatrixFrame frame = new MatrixFrame();
            WorldPosition spawnPosition;
            Vec2 spawnDirection;

            Mission.GetFormationSpawnFrame(BattleSideEnum.Defender, FormationClass.HeavyInfantry, true, out spawnPosition, out spawnDirection);

            frame.origin = spawnPosition.GetGroundVec3();
            frame.rotation = Mat3.CreateMat3WithForward(new Vec3(spawnDirection));

            TruvaSpawnFrame = frame;

            _truvaFormation = new Formation(Mission.AttackerTeam, Mission.NumOfFormationsSpawnedTeamOne);
            _truvaFormation.ArrangementOrder = ArrangementOrder.ArrangementOrderCircle;

            for (int i = 0; i < troopRoster.Count; i++)
            {
                //BasicCharacterObject troop = Game.Current.ObjectManager.GetObject<BasicCharacterObject>("imperial_elite_cataphract");
                TroopRosterElement troopRosterElement = troopRoster.GetElementCopyAtIndex(i);

                BasicCharacterObject troop = troopRosterElement.Character;
                troop.SetFlag("TruvaTroop");

                for (int j = 0; j < troopRosterElement.Number; j++)
                {
                    AgentBuildData data = new AgentBuildData(new BasicBattleAgentOrigin(troop));

                    Vec2 direction = spawnDirection;

                    data.InitialPosition(in frame.origin).Team(team).InitialDirection(in direction).Controller(Agent.ControllerType.AI).Formation(_truvaFormation);
                    Agent agent = Mission.Current.SpawnAgent(data);
                    agent.SetWatchState(Agent.WatchState.Alarmed);

                    if (agent.Character == TruvaTroop.TroopLeader.CharacterObject)
                        _truvaFormation.Captain = agent;

                    _agents.Add(agent);

                }
            }

            _truvaFormation.SetControlledByAI(true);
        }

    }
}
