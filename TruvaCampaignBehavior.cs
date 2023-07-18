using System;
using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;
using Truva.ViewModel;

namespace Truva
{
    public class TruvaCampaignBehavior : CampaignBehaviorBase
    {
        private float _startMissionTime = 0;
        
        private float _timeDelay = 1;
        
        private bool _charactersSpawned = false;

        private List<TruvaTroop> _truvaTroops = new List<TruvaTroop>();
        
        private List<TruvaTimeChecker> _truvaTimeCheckers = new List<TruvaTimeChecker>();

        private TroopRoster _managingOldTruvaTroopRoster;

        private TroopRoster _managingOldPlayerTroopRoster;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this,OnSessionLaunched);

            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, HourlyTick);

            CampaignEvents.OnMissionStartedEvent.AddNonSerializedListener(this, OnMissionStarted);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData<List<TruvaTroop>>("_truvaTroops", ref _truvaTroops);
        }

        public static List<TruvaTroop> TruvaTroops
        {
            get
            {
                return Campaign.Current.GetCampaignBehavior<TruvaCampaignBehavior>()._truvaTroops;
            }
            set
            {
                Campaign.Current.GetCampaignBehavior<TruvaCampaignBehavior>()._truvaTroops = value;
            }
        }

        public void OpenTruvaTroopMenu(bool _isManaging, TruvaTroop truvaTroop = null, Settlement settlement = null)
        {
            TruvaTroopMenu truvaTroopMenuManager;

            if (_isManaging)
            {
                _managingOldTruvaTroopRoster = truvaTroop.TroopRoster.CloneRosterData();
                _managingOldPlayerTroopRoster = PartyBase.MainParty.MemberRoster.CloneRosterData();
                truvaTroopMenuManager = new TruvaTroopMenu(truvaTroop, OnTruvaTroopMenuDoneClicked, null);
            }
            else
                truvaTroopMenuManager = new TruvaTroopMenu(OnTruvaTroopMenuDoneClicked, null, settlement);
        }

        public void OpenChooseSettlementMenu()
        {
            ScreenManager.PopScreen();
            TruvaChooseSettlementMenu truvaChooseSettlementMenu = new TruvaChooseSettlementMenu(OnSettlementChoosed, OnSettlementChooseCanceled);
        }


        public void RemoveFromTimeCheckers(TruvaTimeChecker truvaTimeChecker)
        {
            _truvaTimeCheckers.Remove(truvaTimeChecker);
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddGameMenus(campaignGameStarter);

        }

        private void AddGameMenus(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("town_outside", "town_manage_truva_troops", "Manage Truva Troop", 
                new GameMenuOption.OnConditionDelegate(TruvaTroopMenuCondition), 
                new GameMenuOption.OnConsequenceDelegate(TruvaTroopMenuConsequence), false, 5, false, null);
        }

        private bool TruvaTroopMenuCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.OrderTroopsToAttack;
            return true;
        }

        private void TruvaTroopMenuConsequence(MenuCallbackArgs args)
        {
            TruvaTroop truvaTroop;

            if((truvaTroop = TruvaHelper.FindTruvaTroop(Hero.MainHero.CurrentSettlement.StringId)) == null)
                OpenTruvaTroopMenu(false,null, Hero.MainHero.CurrentSettlement);
            else
                OpenTruvaTroopMenu(true,truvaTroop);
        }

        private void OnTruvaTroopMenuDoneClicked(CharacterObject troopLeader, TroopRoster troopRoster, Settlement settlement, bool _isNewlyCreated)
        {
            TruvaTroop truvaTroop;

            Tuple<CampaignTime, string, float> tuple;

            if (!_isNewlyCreated)//Managing
            {
                 truvaTroop = TruvaHelper.FindTruvaTroop(troopLeader);

                truvaTroop.TroopRoster = _managingOldTruvaTroopRoster;

                TroopRoster newlyAddedTroopRoster = TruvaHelper.GetNewlyAddedTroopRoster(_managingOldTruvaTroopRoster,troopRoster);

                if (newlyAddedTroopRoster.TotalManCount <= 0)
                    return;

                truvaTroop.AddOnWayTroop(newlyAddedTroopRoster);

                tuple = TruvaHelper.GetReachTimeToSettlement(Hero.MainHero, Settlement.Find(truvaTroop.SettlementId));
                
                CampaignTime campaignTime = tuple.Item1;

                TruvaTimeChecker timeChecker = new TruvaTimeChecker(campaignTime, truvaTroop, newlyAddedTroopRoster, AddToTruvaTroop);

                _truvaTimeCheckers.Add(timeChecker);
            }
            else 
            {
                truvaTroop = new TruvaTroop(troopLeader, troopRoster, settlement.StringId);

                _truvaTroops.Add(truvaTroop);

                tuple = TruvaHelper.GetReachTimeToSettlement(Hero.MainHero, Settlement.Find(truvaTroop.SettlementId));

                CampaignTime campaignTime = tuple.Item1;

                TruvaTimeChecker timeChecker = new TruvaTimeChecker(campaignTime, truvaTroop, null, AddTroopToSettlement);

                _truvaTimeCheckers.Add(timeChecker);
            }


            if (_isNewlyCreated)
                InformationManager.DisplayMessage(new InformationMessage("Created Truva Troop will arrive " + truvaTroop.SettlementName + " in " + tuple.Item2, Colors.Green));
            else
                InformationManager.DisplayMessage(new InformationMessage("Added troops will arrive " + truvaTroop.SettlementName + " in " + tuple.Item2, Colors.Green));
        }

        private void OnSettlementChoosed(List<InquiryElement> settlements)
        {
            if (settlements == null || settlements.Count <= 0)
                return;

            Settlement settlement = (Settlement)settlements[0].Identifier;

            ScreenManager.PushScreen(ViewCreatorManager.CreateScreenView<TruvaTroopScreen>());
            TruvaTroopMenu truvaTroopMenu = new TruvaTroopMenu(OnTruvaTroopMenuDoneClicked, null, settlement);

        }

        private void OnSettlementChooseCanceled(List<InquiryElement> settlements)
        {
            ScreenManager.PushScreen(ViewCreatorManager.CreateScreenView<TruvaTroopScreen>());
        }

        private void OnMissionStarted(IMission mission)
        {
            //Eğer hero savaştaysa, town daysa vb kontroller yap

        }

        public void RemoveTruvaTroop(TruvaTroop truvaTroop)
        {
            TruvaTroops.Remove(truvaTroop);

            Tuple<CampaignTime,string,float> tuple = TruvaHelper.GetReachTimeToSettlement(Hero.MainHero, Settlement.Find(truvaTroop.SettlementId));

            CampaignTime campaignTime = tuple.Item1;

            TruvaTimeChecker timeChecker = new TruvaTimeChecker(campaignTime, truvaTroop,null, AddTroopToMainParty);

            _truvaTimeCheckers.Add(timeChecker);

            InformationManager.DisplayMessage(new InformationMessage("Removed Truva Troop will arrive your party in " + tuple.Item2, Colors.Green));
        }


        private void AddTroopToMainParty(TruvaTroop truvaTroop,TroopRoster troopRoster)
        {
            //InformationManager.DisplayMessage(new InformationMessage("Troop Eklendi : " + truvaTroop.TroopRoster.TotalManCount, Colors.Green));
            TextObject message = new TextObject(truvaTroop.SettlementName+" Truva Troops rejoined your party", null);

            MBInformationManager.AddQuickInformation(message, 0, truvaTroop.TroopLeader.CharacterObject, "");
            PartyBase.MainParty.MemberRoster.Add(truvaTroop.TroopRoster);

        }

        private void AddTroopToSettlement(TruvaTroop truvaTroop, TroopRoster troopRoster)
        {
            //InformationManager.DisplayMessage(new InformationMessage("Troop Eklendi : " + truvaTroop.TroopRoster.TotalManCount, Colors.Green));
            TextObject message = new TextObject(truvaTroop.SettlementName + " Truva Troops entered "+truvaTroop.SettlementName, null);

            MBInformationManager.AddQuickInformation(message, 0, truvaTroop.TroopLeader.CharacterObject, "");

            truvaTroop.IsArrivedToSettlement = true;

        }

        private void AddToTruvaTroop(TruvaTroop targetTruvaTroop,TroopRoster troopRoster)
        {
            targetTruvaTroop.AddToTruvaTroop(troopRoster);
        }

        private void HourlyTick()
        {
            for (int i = 0; i < _truvaTimeCheckers.Count; i++)
            {
                _truvaTimeCheckers[i].CheckTime();
            }
        }

    }
}
