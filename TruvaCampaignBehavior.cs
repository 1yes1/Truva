using System;
using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Truva
{
    public class TruvaCampaignBehavior : CampaignBehaviorBase
    {
        private const float _truvaTroopSpeed = 5;
        
        private const float _speedMultiplier = 20;

        private float _startMissionTime = 0;
        
        private float _timeDelay = 1;
        
        private bool _charactersSpawned = false;

        private List<TruvaTroop> _truvaTroops = new List<TruvaTroop>();
        
        private List<TruvaTimeChecker> _truvaTimeCheckers = new List<TruvaTimeChecker>();

        private bool _isTruvaTroopAlreadyExist = false;

        public override void RegisterEvents()
        {
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this,OnGameLoaded);

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

        public bool IsTruvaTroopAlreadyExist
        {
            get
            {
                return _isTruvaTroopAlreadyExist;
            }
            set
            {
                _isTruvaTroopAlreadyExist = value;
            }
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
            TruvaTroopMenu truvaTroopMenuManager;

            if((truvaTroop = TruvaHelper.FindTruvaTroop(Hero.MainHero.CurrentSettlement.StringId)) == null)
            {
                truvaTroopMenuManager = new TruvaTroopMenu(OnTruvaTroopMenuDoneClicked,null);
                this._isTruvaTroopAlreadyExist = false;
            }
            else
            {
                truvaTroopMenuManager = new TruvaTroopMenu(truvaTroop, OnTruvaTroopMenuDoneClicked, null);
                this._isTruvaTroopAlreadyExist = true;
            }
        }

        private void OnTruvaTroopMenuDoneClicked(CharacterObject troopLeader, TroopRoster troopRoster)
        {

            if (_isTruvaTroopAlreadyExist)
            {
                TruvaTroop truvaTroop = TruvaHelper.FindTruvaTroop(troopLeader);
                if(truvaTroop != null)
                    truvaTroop.TroopRoster = troopRoster;
            }
            else
            {
                TruvaTroop truvaTroop = new TruvaTroop(troopLeader, troopRoster, Hero.MainHero.CurrentSettlement.StringId);
                _truvaTroops.Add(truvaTroop);
                InformationManager.DisplayMessage(new InformationMessage("Truva Troop Created: " + _truvaTroops[_truvaTroops.Count - 1].SettlementName, Colors.Green));
            }
        }


        private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
        {
            InformationManager.DisplayMessage(new InformationMessage("Truva GameLoaded", Colors.Green));
        }


        private void OnMissionStarted(IMission mission)
        {
            //Eğer hero savaştaysa, town daysa vb kontroller yap

        }


        public void RemoveTruvaTroop(TruvaTroop truvaTroop)
        {
            TruvaTroops.Remove(truvaTroop);

            Settlement settlement = Settlement.Find(truvaTroop.SettlementId);
            float distance = settlement.GetPosition().Distance(Hero.MainHero.GetPosition());

            int day = (int) Math.Max(1, Math.Round(distance / (_truvaTroopSpeed * _speedMultiplier)));

            CampaignTime campaignTime = CampaignTime.DaysFromNow(day);

            InformationManager.DisplayMessage(new InformationMessage(truvaTroop.SettlementName+" Truva Troops will join your party in " + day + " day.", Colors.Cyan));

            TruvaTimeChecker timeChecker = new TruvaTimeChecker(campaignTime, truvaTroop, AddTroopToMainParty);
            _truvaTimeCheckers.Add(timeChecker);
        }


        private void AddTroopToMainParty(TruvaTroop truvaTroop,TruvaTimeChecker truvaTimeChecker)
        {
            //InformationManager.DisplayMessage(new InformationMessage("Troop Eklendi : " + truvaTroop.TroopRoster.TotalManCount, Colors.Green));
            TextObject message = new TextObject(truvaTroop.SettlementName+" Truva Troops rejoined your party", null);

            MBInformationManager.AddQuickInformation(message, 0, truvaTroop.TroopLeader.CharacterObject, "");
            PartyBase.MainParty.MemberRoster.Add(truvaTroop.TroopRoster);

            _truvaTimeCheckers.Remove(truvaTimeChecker);
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
