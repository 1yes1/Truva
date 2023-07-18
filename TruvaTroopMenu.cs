using Bannerlord.ButterLib.ObjectSystem.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ScreenSystem;
using Truva.ViewModel;
using static TaleWorlds.CampaignSystem.Party.PartyScreenLogic;

namespace Truva
{
    public class TruvaTroopMenu
    {
        private event Action<CharacterObject, TroopRoster, Settlement, bool> OnTruvaTroopMenuDoneClickedEvent;

        private event Action<string> OnTruvaTroopCanceledOrDestroyedEvent;

        private int _requestedRecruitCount = 101;

        private int _deliveredRecruitCount = 0;

        private bool _isManaging = false;

        private CharacterObject _truvaTroopLeader;

        private TroopRoster _troopRoster;

        private Settlement _targetSettlement;

        private TruvaTroop _truvaTroop;


        public TruvaTroopMenu(Action<CharacterObject, TroopRoster, Settlement,bool> OnTruvaTroopMenuDoneClicked, Action<string> OnTruvaTroopDestroyed, Settlement settlement)
        {
            TextObject textObject = new TextObject(settlement.Name.ToString()+ " Truva Troop", null);

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
        public TruvaTroopMenu(TruvaTroop truvaTroop,Action<CharacterObject,TroopRoster,Settlement, bool> OnTruvaTroopMenuDoneClicked, Action<string> OnTruvaTroopDestroyed)
        {
            _truvaTroop = truvaTroop;
            
            _isManaging = true;

            _targetSettlement = Settlement.Find(_truvaTroop.SettlementId);

            OnTruvaTroopMenuDoneClickedEvent += OnTruvaTroopMenuDoneClicked;
            OnTruvaTroopCanceledOrDestroyedEvent += OnTruvaTroopDestroyed;

            TextObject textObject = new TextObject(truvaTroop.SettlementName.ToString() + " Truva Troop", null);

            PartyScreenManager.OpenScreenForManagingAlley(truvaTroop.TroopRoster,
            new IsTroopTransferableDelegate(this.IsTroopTransferable),
            new PartyPresentationDoneButtonConditionDelegate(this.DoneButtonCondition),
            new PartyPresentationDoneButtonDelegate(this.DoneClicked), textObject, OnTruvaTroopCanceledOrDestroyed);

        }

        public TroopRoster GetTroopRoster()
        {
            return _troopRoster;
        }

        private bool DoneClicked(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, FlattenedTroopRoster takenPrisonerRoster, FlattenedTroopRoster releasedPrisonerRoster, bool isForced, PartyBase leftParty, PartyBase rightParty)
        {
            _troopRoster = leftMemberRoster;

            if (_troopRoster.Count == 0 && _isManaging)
            {
                InquiryResult();
                return false;
            }

            for (int i = 0; i < _troopRoster.Count; i++)
            {
                if (_troopRoster.GetCharacterAtIndex(i).IsHero && _troopRoster.GetCharacterAtIndex(i) != Hero.MainHero.CharacterObject)
                {
                    _truvaTroopLeader = _troopRoster.GetCharacterAtIndex(i);
                    break;
                }
            }

            OnTruvaTroopMenuDoneClickedEvent?.Invoke(_truvaTroopLeader, leftMemberRoster, _targetSettlement, !_isManaging);

            return true;
        }

        private Tuple<bool, TextObject> DoneButtonCondition(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, int leftLimitNum, int rightLimitNum)
        {
            if (leftMemberRoster.TotalHeroes > 1)
            {
                TextObject textObject = new TextObject("You can select only 1 companion to be leader!", null);
                return new Tuple<bool, TextObject>(false, textObject);
            }

            if((leftMemberRoster.TotalHeroes == 0 && (_truvaTroop != null && TruvaHelper.FindTruvaTroop(_truvaTroop.SettlementId) == null)) || 
                (leftMemberRoster.TotalHeroes == 0 && TruvaHelper.FindTruvaTroop(_targetSettlement.StringId) == null) || 
                (!_isManaging && leftMemberRoster.TotalManCount <= 0))
            {
                TextObject textObject = new TextObject("You need to choose a leader companion!", null);
                return new Tuple<bool, TextObject>(false, textObject);
            }

            return new Tuple<bool, TextObject>(true, null);
        }

        private bool IsTroopTransferable(CharacterObject character, PartyScreenLogic.TroopType type, PartyScreenLogic.PartyRosterSide side, PartyBase leftOwnerParty)
        {
            bool leftToRight = false;

            if(_truvaTroop != null && side == PartyRosterSide.Left)
                leftToRight = false;

            if (_truvaTroop == null)
                leftToRight = true;

            if (side == PartyRosterSide.Left && !character.HasFlag("TruvaTroop"))
                leftToRight = true;

            return this._requestedRecruitCount - this._deliveredRecruitCount >= 0 &&
                ((side == PartyRosterSide.Right && (character.Tier > 1 || character.IsHero)) || (leftToRight)) ;
        }

        /*
         *Tüm trooplar alınmış truva troop silinsin mi popup ı
         */
        private bool InquiryResult()
        {
            InformationManager.ShowInquiry(new InquiryData(new TextObject("Info", null).ToString(), new TextObject(_targetSettlement + " Truva Troop will be deleted. Are you sure?", null).ToString(),
            true, true, new TextObject("{=yS7PvrTD}OK", null).ToString(), new TextObject("Cancel", null).ToString(), OnOkClicked, null, "", 0f, null, null, null), false, false);

            return true;
        }

        private void OnOkClicked()
        {
            TruvaCampaignBehavior.TruvaTroops.Remove(TruvaHelper.FindTruvaTroop(_truvaTroop.SettlementId));
            OnTruvaTroopCanceledOrDestroyedEvent?.Invoke(_truvaTroop.SettlementId);

            PartyScreenManager.CloseScreen(true,true);
        }

        private void OnTruvaTroopCanceledOrDestroyed()
        {
            OnTruvaTroopCanceledOrDestroyedEvent?.Invoke("");
        }
    }
}
