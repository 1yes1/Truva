using Bannerlord.ButterLib.ObjectSystem.Extensions;
using HarmonyLib;
using System;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ScreenSystem;
using Truva.CampaignBehaviors;
using Truva.Models;
using static TaleWorlds.CampaignSystem.Party.PartyScreenLogic;

namespace Truva.Menus
{
    public class TruvaTroopManageMenu
    {
        private event Action<CharacterObject, TroopRoster, Settlement, bool> OnTruvaTroopMenuDoneClickedEvent;

        private event Action<string> OnTruvaTroopCanceledOrDestroyedEvent;

        private int _requestedRecruitCount = 101;

        private int _deliveredRecruitCount = 0;

        private bool _isManaging = false;

        private CharacterObject _truvaTroopLeader;

        private TroopRoster _troopRoster;

        private TroopRoster _oldTroopRoster;

        private Settlement _targetSettlement;

        private TruvaTroop _truvaTroop;

        public TruvaTroopManageMenu(Action<CharacterObject, TroopRoster, Settlement, bool> OnTruvaTroopMenuDoneClicked, Action<string> OnTruvaTroopDestroyed, Settlement settlement)
        {
            TextObject textObject = new TextObject(settlement.Name.ToString() + " Truva Troop", null);

            OnTruvaTroopMenuDoneClickedEvent += OnTruvaTroopMenuDoneClicked;
            OnTruvaTroopCanceledOrDestroyedEvent += OnTruvaTroopDestroyed;

            ScreenManager.PopScreen();

            _targetSettlement = settlement;

            PartyScreenManager.OpenScreenWithCondition(
            new IsTroopTransferableDelegate(IsTroopTransferable),
            new PartyPresentationDoneButtonConditionDelegate(DoneButtonCondition),
            new PartyPresentationDoneButtonDelegate(DoneClicked), OnTruvaTroopCanceledOrDestroyed, TransferState.Transferable, TransferState.NotTransferable,
            textObject, _requestedRecruitCount - _deliveredRecruitCount, true, false, PartyScreenMode.Normal);
        }

        /*
         Zaten var olan truva troop u düzenlemek için açıyoruz.
         */
        public TruvaTroopManageMenu(TruvaTroop truvaTroop, Action<CharacterObject, TroopRoster, Settlement, bool> OnTruvaTroopMenuDoneClicked, Action<string> OnTruvaTroopDestroyed)
        {
            _truvaTroop = truvaTroop;

            _oldTroopRoster = truvaTroop.TroopRoster.CloneRosterData();

            _isManaging = true;

            _targetSettlement = Settlement.Find(_truvaTroop.SettlementId);

            OnTruvaTroopMenuDoneClickedEvent += OnTruvaTroopMenuDoneClicked;
            OnTruvaTroopCanceledOrDestroyedEvent += OnTruvaTroopDestroyed;

            TextObject textObject = new TextObject(truvaTroop.SettlementName.ToString() + " Truva Troop", null);

            //PartyScreenManager.OpenScreenForManagingAlley(truvaTroop.TroopRoster,
            //new IsTroopTransferableDelegate(this.IsTroopTransferable),
            //new PartyPresentationDoneButtonConditionDelegate(this.DoneButtonCondition),
            //new PartyPresentationDoneButtonDelegate(this.DoneClicked), textObject, OnTruvaTroopCanceledOrDestroyed);

            PartyScreenLogic partyScreenLogic = new PartyScreenLogic();
            PartyScreenLogicInitializationData initializationData = PartyScreenLogicInitializationData.CreateBasicInitDataWithMainParty(truvaTroop.TroopRoster, TroopRoster.CreateDummyTroopRoster(), TransferState.Transferable, TransferState.NotTransferable, TransferState.NotTransferable, IsTroopTransferable, null, textObject, null, truvaTroop.TroopLeader, _requestedRecruitCount, 0, DoneClicked, DoneButtonCondition, OnTruvaTroopCanceledOrDestroyed, null, null, false, false, false, true, 0);
            partyScreenLogic.Initialize(initializationData);
            PartyState partyState = Game.Current.GameStateManager.CreateState<PartyState>();
            partyState.InitializeLogic(partyScreenLogic);
            Game.Current.GameStateManager.PushState(partyState, 0);

            FieldInfo _partyScreenLogic = AccessTools.Field(typeof(PartyScreenManager), "_partyScreenLogic");
            _partyScreenLogic.SetValue(PartyScreenManager.Instance, partyScreenLogic);
        }

        private Tuple<bool, TextObject> DoneButtonCondition(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, int leftLimitNum, int rightLimitNum)
        {
            if (leftMemberRoster.TotalHeroes > 1)
            {
                TextObject textObject = new TextObject("You can select only 1 companion to be leader!", null);
                return new Tuple<bool, TextObject>(false, textObject);
            }

            if (leftMemberRoster.TotalHeroes == 0 && _truvaTroop != null && TruvaHelper.FindTruvaTroop(_truvaTroop.SettlementId) == null ||
                leftMemberRoster.TotalHeroes == 0 && TruvaHelper.FindTruvaTroop(_targetSettlement.StringId) == null ||
                !_isManaging && leftMemberRoster.TotalManCount <= 0)
            {
                TextObject textObject = new TextObject("You need to choose a leader companion!", null);
                return new Tuple<bool, TextObject>(false, textObject);
            }

            if (leftMemberRoster.TotalManCount > _requestedRecruitCount - _deliveredRecruitCount)
            {
                TextObject textObject = new TextObject("You can choose max " + _requestedRecruitCount + " soldier!", null);
                return new Tuple<bool, TextObject>(false, textObject);
            }

            return new Tuple<bool, TextObject>(true, null);
        }

        private bool IsTroopTransferable(CharacterObject character, PartyScreenLogic.TroopType type, PartyRosterSide side, PartyBase leftOwnerParty)
        {
            bool leftToRight = false;

            if (_truvaTroop != null && side == PartyRosterSide.Left)
                leftToRight = false;

            if (_truvaTroop == null)
                leftToRight = true;

            if (side == PartyRosterSide.Left && !character.HasFlag("TruvaTroop"))
            {
                leftToRight = true;

            }

            return _requestedRecruitCount - _deliveredRecruitCount >= 0 &&
                (side == PartyRosterSide.Right && (character.Tier > 1 || character.IsHero) || leftToRight);
        }

        private bool DoneClicked(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, FlattenedTroopRoster takenPrisonerRoster, FlattenedTroopRoster releasedPrisonerRoster, bool isForced, PartyBase leftParty, PartyBase rightParty)
        {
            _troopRoster = leftMemberRoster;

            if (_troopRoster.Count == 0 && _isManaging)
            {
                InquiryResultRemove();
                return false;
            }

            InquiryResultPay(_isManaging);

            return false;
        }

        private void InquiryResultRemove()
        {
            InformationManager.ShowInquiry(new InquiryData(new TextObject("Info", null).ToString(), new TextObject(_targetSettlement + " Truva Troop will be removed. Are you sure?", null).ToString(),
            true, true, new TextObject("{=yS7PvrTD}OK", null).ToString(), new TextObject("Cancel", null).ToString(), OnOkClickedToRemove, null, "", 0f, null, null, null), false, true);

        }

        private void OnOkClickedToRemove()
        {
            TruvaCampaignBehavior.TruvaTroops.Remove(TruvaHelper.FindTruvaTroop(_truvaTroop.SettlementId));
            OnTruvaTroopCanceledOrDestroyedEvent?.Invoke(_truvaTroop.SettlementId);

            PartyScreenManager.CloseScreen(true, true);
        }

        private void InquiryResultPay(bool isManaging)
        {
            TextObject textObject;
            int playerGold = Clan.PlayerClan.Gold;
            int totalWage = 0;

            if (!isManaging)
            {
                totalWage = TruvaHelper.GetTroopWage(_troopRoster) * TruvaModel.WageMultiplier;
                textObject = new TextObject(_targetSettlement + " Truva Troop will cost you " + CampaignUIHelper.GetAbbreviatedValueTextFromValue(totalWage) + "{GOLD_ICON}. Do you want to create it?", null);
            }
            else
            {
                TroopRoster newlyAddedTroopRoster = TruvaHelper.GetNewlyAddedTroopRoster(_oldTroopRoster, _troopRoster);
                totalWage = TruvaHelper.GetTroopWage(newlyAddedTroopRoster) * TruvaModel.WageMultiplier;
                textObject = new TextObject("Newly added Troop will cost you " + CampaignUIHelper.GetAbbreviatedValueTextFromValue(totalWage) + "{GOLD_ICON}. Do you want to continue?", null);
            }

            if (totalWage > playerGold)
            {
                string totalWageStr = CampaignUIHelper.GetAbbreviatedValueTextFromValue(totalWage);
                string playerGoldStr = CampaignUIHelper.GetAbbreviatedValueTextFromValue(playerGold);
                string differenceStr = CampaignUIHelper.GetAbbreviatedValueTextFromValue(totalWage - playerGold);

                InformationManager.ShowInquiry(new InquiryData(new TextObject("Info", null).ToString(), new TextObject("Insufficient gold! The cost of the troop is " + totalWageStr + "{GOLD_ICON} but you have only " + playerGoldStr + "{GOLD_ICON}. You need " + differenceStr + "{GOLD_ICON} more!", null).ToString(),
                true, false, new TextObject("{=yS7PvrTD}OK", null).ToString(), null, null, null, "", 0f, null, null, null), false, true);
                return;
            }

            textObject.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"8\">");

            InformationManager.ShowInquiry(new InquiryData(new TextObject("Cost", null).ToString(), textObject.ToString(),
true, true, new TextObject("Yes", null).ToString(), new TextObject("Cancel", null).ToString(), OnOkClickedToPay, null, "", 0f, null, null, null), false, false);

        }



        private void OnOkClickedToPay()
        {
            for (int i = 0; i < _troopRoster.Count; i++)
            {
                if (_troopRoster.GetCharacterAtIndex(i).IsHero && _troopRoster.GetCharacterAtIndex(i) != Hero.MainHero.CharacterObject)
                {
                    _truvaTroopLeader = _troopRoster.GetCharacterAtIndex(i);
                    break;
                }
            }

            OnTruvaTroopMenuDoneClickedEvent?.Invoke(_truvaTroopLeader, _troopRoster, _targetSettlement, !_isManaging);
            PartyScreenManager.CloseScreen(true, true);
        }

        private void OnTruvaTroopCanceledOrDestroyed()
        {
            OnTruvaTroopCanceledOrDestroyedEvent?.Invoke("");
        }
    }
}
