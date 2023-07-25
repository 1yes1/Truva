using Bannerlord.ButterLib.ObjectSystem.Extensions;
using HarmonyLib;
using SandBox.BoardGames;
using SandBox.Missions.MissionLogics;
using SandBox.View.Map;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Diamond;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;
using TaleWorlds.Localization;
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

        private WorldPosition _spawnPosition;

        public bool IsTruvaTroopSpawned { get; private set; }

        private TruvaTroop TruvaTroop { get; set; }

        private TruvaSiegeAttackLogic TruvaSiegeAttack { get; set; }

        private TruvaSiegeMissionView TruvaSiegeMV { get; set; }

        public TruvaInputManager InputManager { get; set; }

        public MatrixFrame TruvaSpawnFrame { get; private set; }

        public bool IsBattleResultReady { get; private set; }

        public bool IsTruvaDestroyed { get => _deadCount + _woundedCount >= _agents.Count - 1; }

        public bool HasTruvaTroop { get => (Mission.Current.IsSiegeBattle && Settlement.CurrentSettlement != null && TruvaHelper.FindTruvaTroop(Settlement.CurrentSettlement.StringId) != null); }

        public override void AfterStart()
        {
            if (!HasTruvaTroop)
                return;

            InformationManager.DisplayMessage(new InformationMessage("You have Truva Troop inside the settlement!", Colors.Yellow));
            InformationManager.DisplayMessage(new InformationMessage("You can call them with holding 'Q' and sliding mouse up!", Colors.Yellow));

            InputManager = Mission.GetMissionBehavior<TruvaInputManager>();

            TruvaSiegeAttack = Mission.GetMissionBehavior<TruvaSiegeAttackLogic>();

            TruvaSiegeMV = Mission.GetMissionBehavior<TruvaSiegeMissionView>();
        }

        public override void OnDeploymentFinished()
        {
            if (!HasTruvaTroop)
                return;

            TruvaSiegeMV.OnTruvaSpawnChooseEvent += OnTruvaSpawnChoose;

            TruvaTroop = TruvaHelper.FindTruvaTroop(Settlement.CurrentSettlement.StringId);

            _isDeploymentFinished = true;
        }


        public override void OnRetreatMission()
        {
            if (!HasTruvaTroop || !IsTruvaTroopSpawned)
                return;

            CalculateWoundAndDead();
        }

        public override void OnMissionResultReady(MissionResult missionResult)
        {
            if (!HasTruvaTroop || !IsTruvaTroopSpawned)
                return;

            if(missionResult.PlayerDefeated)
            {
                ShowMissionMessage("Truva Troop is retreating!");

                for (int i = 0; i < _agents.Count; i++)
                {
                    _agents[i].Retreat(_spawnPosition);
                }
            }

            IsBattleResultReady = true;
            //InformationManager.DisplayMessage(new InformationMessage("battle result", Colors.Yellow));
            CalculateWoundAndDead();
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            if (!HasTruvaTroop || !IsTruvaTroopSpawned)
                return;

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
            if (IsTruvaTroopSpawned)
            {
                //InformationManager.DisplayMessage(new InformationMessage("------------------Calculate-------------", Colors.Yellow));
                TruvaTroop.TroopRoster.KillNumberOfMenRandomly(_deadCount, false);
                TruvaTroop.TroopRoster.WoundNumberOfTroopsRandomly(_woundedCount);
            }
        }

        public override void OnMissionStateFinalized()
        {
            if (!HasTruvaTroop)
                return;

            ShowMissionMessage(TruvaTroop.SettlementName + " Truva Troop added to your party!");

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

            TruvaTroop.IsAtWar = true;

            SpawnAgents(TruvaTroop.TroopRoster);

            //Mission.Current.GetMissionBehavior<MissionAgentSpawnLogic>()

            IsTruvaTroopSpawned = true;

            ShowMissionMessage("Truva Troop came to help!");

            OnTruvaTroopsSpawnedEvent?.Invoke(_agents, _truvaFormation);
        }

        private void SpawnAgents(TroopRoster troopRoster)
        {
            if (IsTruvaTroopSpawned)
                return;

            Team team = Mission.Current.PlayerTeam;
            MatrixFrame frame = new MatrixFrame();
            Vec2 spawnDirection;

            Mission.Current.GetFormationSpawnFrame(BattleSideEnum.Defender, FormationClass.HeavyInfantry, true, out _spawnPosition, out spawnDirection);

            frame.origin = _spawnPosition.GetGroundVec3();
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

                    Mission.Current.PlayerTeam.RemoveAgentFromTeam(agent);
                    //Mission.PlayerTeam.AddAgentToTeam(agent);

                    agent.SetWatchState(Agent.WatchState.Alarmed);

                    if (agent.Character == TruvaTroop.TroopLeader.CharacterObject)
                        _truvaFormation.Captain = agent;

                    _agents.Add(agent);

                }
            }

            _truvaFormation.SetControlledByAI(true);
        }

        public void ShowMissionMessage(string text)
        {
            TextObject message = new TextObject(text, null);
            InformationManager.DisplayMessage(new InformationMessage(message.ToString(),Colors.Yellow));
            MBInformationManager.AddQuickInformation(message, 0, TruvaTroop.TroopLeader.CharacterObject, "");
        }


    }
}
