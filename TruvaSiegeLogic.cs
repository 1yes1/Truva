using Bannerlord.ButterLib.ObjectSystem.Extensions;
using SandBox.BoardGames;
using SandBox.View.Map;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.GameState;
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
using Truva.MissionView;
using Truva.ViewModel;

namespace Truva
{
    public class TruvaSiegeLogic : MissionLogic
    {
        public event Action<List<Agent>,Formation> OnTruvaTroopsSpawnedEvent;

        private bool _isDeploymentFinished = false;

        private List<Agent> _agents = new List<Agent>();

        private Formation _truvaFormation;

        public bool IsTruvaTroopSpawned { get; private set; }

        private TruvaTroop TruvaTroop { get; set; }

        private TruvaSiegeAttackLogic TruvaSiegeAttack { get ; set; }

        private TruvaSiegeMissionView TruvaSiegeMV { get; set; }

        public TruvaInputManager InputManager { get; set; }

        public MatrixFrame TruvaSpawnFrame { get ; private set; }


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

            InputManager.OnKeyCtrlAPressed += SpawnTruvaTroops;
            TruvaSiegeMV.OnTruvaSpawnChooseEvent += OnTruvaSpawnChoose;

            _isDeploymentFinished = true;
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

            OnTruvaTroopsSpawnedEvent?.Invoke(_agents,_truvaFormation);
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
