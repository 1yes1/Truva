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
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;
using Truva.Menus;
using Truva.Models;
using Truva.ViewModel;

namespace Truva.CampaignBehaviors
{
    public class TruvaCampaignBehavior : CampaignBehaviorBase
    {
        private List<TruvaTroop> _truvaTroops = new List<TruvaTroop>();
        
        private List<TruvaTimeChecker> _truvaTimeCheckers = new List<TruvaTimeChecker>();

        private TroopRoster _managingOldTruvaTroopRoster;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, HourlyTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
            //dataStore.SyncData("_truvaSaveClass", ref _truvaSaveClass);
            dataStore.SyncData("_truvaTroops", ref _truvaTroops);
            dataStore.SyncData("_truvaTimeCheckers", ref _truvaTimeCheckers);

            if(dataStore.IsLoading)
            {
                for (int i = 0; i < _truvaTroops.Count; i++)
                    _truvaTroops[i].SetFlags();
            }
            
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

        public void OpenTruvaTroopMenu(bool isManaging, TruvaTroop truvaTroop = null, Settlement settlement = null)
        {
            TruvaTroopManageMenu truvaTroopMenuManager;

            if (isManaging)
            {
                if (Settlement.Find(truvaTroop.SettlementId).IsUnderSiege)
                {
                    InquiryUnderSiege(Settlement.Find(truvaTroop.SettlementId));
                    return;
                }
                //Truva ekranını kaldırıp manage ekranına gidiyoruz
                ScreenManager.PopScreen();

                _managingOldTruvaTroopRoster = truvaTroop.TroopRoster.CloneRosterData();
                truvaTroopMenuManager = new TruvaTroopManageMenu(truvaTroop, OnTruvaTroopMenuDoneClicked, null);
            }
            else
                truvaTroopMenuManager = new TruvaTroopManageMenu(OnTruvaTroopMenuDoneClicked, null, settlement);
        }

        public void RemoveTruvaTroop(TruvaTroop truvaTroop, bool noMessage = false,bool fromMenu = false)
        {
            if (Settlement.Find(truvaTroop.SettlementId).IsUnderSiege && fromMenu)
            {
                InquiryUnderSiege(Settlement.Find(truvaTroop.SettlementId));
                return;
            }

            TruvaTroops.Remove(truvaTroop);

            truvaTroop.IsRemoving = true;

            if (noMessage)
                return;

            Tuple<CampaignTime, string, float> tuple = TruvaHelper.GetReachTimeToSettlement(Hero.MainHero, Settlement.Find(truvaTroop.SettlementId));
            CampaignTime campaignTime = tuple.Item1;

            TruvaTimeChecker timeChecker = new TruvaTimeChecker(campaignTime, truvaTroop, null, OnTimeIsUp);
            _truvaTimeCheckers.Add(timeChecker);

            InformationManager.DisplayMessage(new InformationMessage("Removed Truva Troop will arrive your party in " + tuple.Item2, Colors.Yellow));

            if(fromMenu)
            {
                ScreenManager.PopScreen();
                ScreenManager.PushScreen(ViewCreatorManager.CreateScreenView<TruvaTroopScreen>());
            }
        }

        private void InquiryUnderSiege(Settlement settlement)
        {
            InformationManager.ShowInquiry(new InquiryData(new TextObject("Info", null).ToString(), new TextObject(settlement.Name + " is under siege. You can't send or remove troop!", null).ToString(),
false, true,null, new TextObject("{=yS7PvrTD}OK", null).ToString(), null, null, "", 0, null, null, null), false, false);
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
            Campaign.Current.SpeedUpMultiplier = 25;

            for (int i = 0; i < _truvaTimeCheckers.Count; i++)
            {
                _truvaTimeCheckers[i].OnTimeIsUpEvent -= OnTimeIsUp;
                _truvaTimeCheckers[i].OnTimeIsUpEvent += OnTimeIsUp;
            }

        }

        private void OnTruvaTroopMenuDoneClicked(CharacterObject troopLeader, TroopRoster troopRoster, Settlement settlement, bool _isNewlyCreated)
        {
            TruvaTroop truvaTroop;

            Tuple<CampaignTime, string, float> tuple;

            TroopRoster troopRosterForWage;

            if (!_isNewlyCreated)//Managing
            {
                truvaTroop = TruvaHelper.FindTruvaTroop(troopLeader);

                truvaTroop.TroopRoster = _managingOldTruvaTroopRoster;

                TroopRoster newlyAddedTroopRoster = TruvaHelper.GetNewlyAddedTroopRoster(_managingOldTruvaTroopRoster, troopRoster);

                troopRosterForWage = newlyAddedTroopRoster;

                if (newlyAddedTroopRoster.TotalManCount <= 0)
                    return;

                truvaTroop.AddOnWayTroop(newlyAddedTroopRoster);

                tuple = TruvaHelper.GetReachTimeToSettlement(Hero.MainHero, Settlement.Find(truvaTroop.SettlementId));

                CampaignTime campaignTime = tuple.Item1;

                TruvaTimeChecker timeChecker = new TruvaTimeChecker(campaignTime, truvaTroop, newlyAddedTroopRoster, OnTimeIsUp);

                _truvaTimeCheckers.Add(timeChecker);
            }
            else
            {
                truvaTroop = new TruvaTroop(troopLeader, troopRoster, settlement.StringId);

                troopRosterForWage = troopRoster;

                _truvaTroops.Add(truvaTroop);

                tuple = TruvaHelper.GetReachTimeToSettlement(Hero.MainHero, Settlement.Find(truvaTroop.SettlementId));

                CampaignTime campaignTime = tuple.Item1;

                TruvaTimeChecker timeChecker = new TruvaTimeChecker(campaignTime, truvaTroop, null, OnTimeIsUp);

                _truvaTimeCheckers.Add(timeChecker);
            }

            int truvaTroopCost = (int)TruvaHelper.GetTroopWage(troopRosterForWage) * TruvaModel.WageMultiplier;
            Hero.MainHero.Gold -= truvaTroopCost;

            string soundEventPath = "event:/ui/notification/coins_negative";
            TextObject textObject = new TextObject(truvaTroop.SettlementName + " Truva Troops cost: -" + truvaTroopCost + "{GOLD_ICON}", null);
            textObject.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"8\">");
            InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), soundEventPath));

            if (_isNewlyCreated)
                InformationManager.DisplayMessage(new InformationMessage("Created Truva Troop will arrive " + truvaTroop.SettlementName + " in " + tuple.Item2,Colors.Yellow));
            else
                InformationManager.DisplayMessage(new InformationMessage("Added troops will arrive " + truvaTroop.SettlementName + " in " + tuple.Item2, Colors.Yellow));
        }

        private void OnSettlementChoosed(List<InquiryElement> settlements)
        {
            if (settlements == null || settlements.Count <= 0)
                return;

            Settlement settlement = (Settlement)settlements[0].Identifier;

            ScreenManager.PushScreen(ViewCreatorManager.CreateScreenView<TruvaTroopScreen>());
            TruvaTroopManageMenu truvaTroopMenu = new TruvaTroopManageMenu(OnTruvaTroopMenuDoneClicked, null, settlement);
        }

        private void OnSettlementChooseCanceled(List<InquiryElement> settlements)
        {
            ScreenManager.PushScreen(ViewCreatorManager.CreateScreenView<TruvaTroopScreen>());
        }

        private void AddTroopToMainParty(TruvaTroop truvaTroop, TroopRoster troopRoster)
        {
            TruvaHelper.ShowTruvaCampaignMessage(truvaTroop.SettlementName + " Truva Troops rejoined your party", truvaTroop.TroopLeader.CharacterObject);
            PartyBase.MainParty.MemberRoster.Add(truvaTroop.TroopRoster);
        }

        private void AddTroopToSettlement(TruvaTroop truvaTroop, TroopRoster troopRoster)
        {
            int killCount = truvaTroop.TroopRoster.TotalManCount * TruvaModel.GetPercentageOfCaught(Settlement.Find(truvaTroop.SettlementId)) / 100;
            truvaTroop.TroopRoster.KillNumberOfMenRandomly(killCount, false);


            TruvaHelper.ShowTruvaCampaignMessage(truvaTroop.SettlementName + " Truva Troops entered the settlement", truvaTroop.TroopLeader.CharacterObject);
            TruvaHelper.ShowTruvaCampaignMessage("Your " + killCount + " soldier had caught while entering the settlement! Now you have " + truvaTroop.TroopRoster.TotalManCount + " soldier on Truva Troop.", truvaTroop.TroopLeader.CharacterObject);

            truvaTroop.IsArrivedToSettlement = true;
        }

        private void AddToTruvaTroop(TruvaTroop targetTruvaTroop, TroopRoster troopRoster)
        {
            int killCount = troopRoster.TotalManCount * TruvaModel.GetPercentageOfCaught(Settlement.Find(targetTruvaTroop.SettlementId)) / 100;
            troopRoster.KillNumberOfMenRandomly(killCount, false);

            TruvaHelper.ShowTruvaCampaignMessage(targetTruvaTroop.SettlementName + " newly added troop entered the settlement.", targetTruvaTroop.TroopLeader.CharacterObject);
            TruvaHelper.ShowTruvaCampaignMessage("Your " + killCount + " soldier had caught while entering the settlement! Now you have " + (targetTruvaTroop.TroopRoster.TotalManCount + troopRoster.TotalManCount) + " soldier on Truva Troop.", targetTruvaTroop.TroopLeader.CharacterObject);

            targetTruvaTroop.AddToTruvaTroop(troopRoster);
        }

        private void HourlyTick()
        {
            for (int i = 0; i < _truvaTimeCheckers.Count; i++)
            {
                _truvaTimeCheckers[i].CheckTime();
            }
        }

        private void OnTimeIsUp(TruvaTroop targetTruvaTroop, TroopRoster troopRoster)
        {
            if (targetTruvaTroop.OnWayTroopCount > 0)
            {
                AddToTruvaTroop(targetTruvaTroop, troopRoster);
            }
            else if (!targetTruvaTroop.IsArrivedToSettlement)
            {
                AddTroopToSettlement(targetTruvaTroop, troopRoster);
            }
            else if (targetTruvaTroop.IsRemoving)
            {
                AddTroopToMainParty(targetTruvaTroop, troopRoster);
            }
        }

    }
}
