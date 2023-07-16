﻿using Bannerlord.UIExtenderEx;
using HarmonyLib;
using SandBox.GauntletUI.AutoGenerated0;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using Truva.MissionView;

namespace Truva
{
    public class SubModule : MBSubModuleBase
    {

        private TruvaSiegeLogic _siegeLogic;

        private TruvaSiegeAttackLogic _siegeAttackLogic;

        private TruvaInputManager _inputManager;

        private TruvaSiegeMissionView _truvaSiegeMissionView;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            Harmony harmony = new Harmony("truva_harmony");
            harmony.PatchAll();


            UIExtender _extender = new UIExtender("Truva");
            _extender.Register(typeof(SubModule).Assembly);
            _extender.Enable();
        }


        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            if (game.GameType is Campaign)
            {
                CampaignGameStarter campaignStarter = (CampaignGameStarter)gameStarterObject;

                campaignStarter.AddBehavior(new TruvaCampaignBehavior());
            }


            InformationManager.DisplayMessage(new InformationMessage("REAL Truva Mod ENABLED", Colors.Green));
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            mission.AddMissionBehavior(_siegeLogic = new TruvaSiegeLogic());
            mission.AddMissionBehavior(_inputManager = new TruvaInputManager());
            mission.AddMissionBehavior(_truvaSiegeMissionView = new TruvaSiegeMissionView(_inputManager, _siegeLogic));
            mission.AddMissionBehavior(_siegeAttackLogic = new TruvaSiegeAttackLogic(_truvaSiegeMissionView));
        }

    }
}