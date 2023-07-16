using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Encyclopedia.Pages;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using Truva.ViewModel;
using static TaleWorlds.CampaignSystem.Party.PartyScreenLogic;

namespace Truva
{
    public class TruvaTroopMenu
    {
        private event Action<CharacterObject, TroopRoster> OnTruvaTroopMenuDoneClickedEvent;
        private event Action<string> OnTruvaTroopDestroyedEvent;

        private int _requestedRecruitCount = 101;
        private int _deliveredRecruitCount = 0;
        private CharacterObject _truvaTroopLeader;
        private TroopRoster _troopRoster;

        private TruvaTroop _truvaTroop;

        public TruvaTroopMenu(Action<CharacterObject, TroopRoster> OnTruvaTroopMenuDoneClicked, Action<string> OnTruvaTroopDestroyed)
        {
            TextObject textObject = new TextObject(Hero.MainHero.CurrentSettlement.Name.ToString()+ " Truva Troop", null);

            OnTruvaTroopMenuDoneClickedEvent += OnTruvaTroopMenuDoneClicked;
            OnTruvaTroopDestroyedEvent += OnTruvaTroopDestroyed;

            PartyScreenManager.OpenScreenWithCondition(
            new IsTroopTransferableDelegate(this.IsTroopTransferable),
            new PartyPresentationDoneButtonConditionDelegate(this.DoneButtonCondition),
            new PartyPresentationDoneButtonDelegate(this.DoneClicked), null, TransferState.Transferable, TransferState.NotTransferable,
            textObject, this._requestedRecruitCount - this._deliveredRecruitCount, true, false, PartyScreenMode.Normal);
        }

        /*
         Zaten var olan truva troop u düzenlemek için açıyoruz.
         */
        public TruvaTroopMenu(TruvaTroop truvaTroop,Action<CharacterObject,TroopRoster> OnTruvaTroopMenuDoneClicked, Action<string> OnTruvaTroopDestroyed)
        {
            _truvaTroop = truvaTroop;

            OnTruvaTroopMenuDoneClickedEvent += OnTruvaTroopMenuDoneClicked;
            OnTruvaTroopDestroyedEvent += OnTruvaTroopDestroyed;

            TextObject textObject = new TextObject(truvaTroop.SettlementName.ToString() + " Truva Troop", null);

            PartyScreenManager.OpenScreenForManagingAlley(truvaTroop.TroopRoster,
            new IsTroopTransferableDelegate(this.IsTroopTransferable),
            new PartyPresentationDoneButtonConditionDelegate(this.DoneButtonCondition),
            new PartyPresentationDoneButtonDelegate(this.DoneClicked), textObject,null);

        }

        public TroopRoster GetTroopRoster()
        {
            return _troopRoster;
        }

        private bool DoneClicked(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, FlattenedTroopRoster takenPrisonerRoster, FlattenedTroopRoster releasedPrisonerRoster, bool isForced, PartyBase leftParty, PartyBase rightParty)
        {
            _troopRoster = leftMemberRoster;

            if (_troopRoster.Count == 0)
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

            OnTruvaTroopMenuDoneClickedEvent?.Invoke(_truvaTroopLeader, leftMemberRoster);

            return true;
        }

        private Tuple<bool, TextObject> DoneButtonCondition(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, int leftLimitNum, int rightLimitNum)
        {
            if (leftMemberRoster.TotalHeroes > 1)
            {
                TextObject textObject = new TextObject("You can select only 1 companion to be leader!", null);
                return new Tuple<bool, TextObject>(false, textObject);
            }

            if(leftMemberRoster.TotalHeroes == 0 && (_truvaTroop != null && TruvaHelper.FindTruvaTroop(_truvaTroop.SettlementId) == null))
            {
                TextObject textObject = new TextObject("You need to choose a leader companion!", null);
                return new Tuple<bool, TextObject>(false, textObject);
            }
            else if (leftMemberRoster.TotalHeroes == 0 && Hero.MainHero.CurrentSettlement != null && TruvaHelper.FindTruvaTroop(Hero.MainHero.CurrentSettlement.StringId) == null)
            {
                TextObject textObject = new TextObject("You need to choose a leader companion!", null);
                return new Tuple<bool, TextObject>(false, textObject);
            }

            return new Tuple<bool, TextObject>(true, null);

        }

        private bool IsTroopTransferable(CharacterObject character, PartyScreenLogic.TroopType type, PartyScreenLogic.PartyRosterSide side, PartyBase leftOwnerParty)
        {
            bool leftToRight = true;

            if(_truvaTroop != null && side == PartyRosterSide.Left)
                leftToRight = false;

            if (Hero.MainHero.CurrentSettlement != null && _truvaTroop != null && Hero.MainHero.CurrentSettlement.StringId == _truvaTroop.SettlementId)
                leftToRight = true;

            return this._requestedRecruitCount - this._deliveredRecruitCount >= 0 &&
                (side == PartyScreenLogic.PartyRosterSide.Left || (MobileParty.MainParty.MemberRoster.Contains(character) && (character.Tier > 1 || character.IsHero))) && leftToRight;
        }

        /*
         *Tüm trooplar alınmış truva troop silinsin mi popup ı
         */
        private bool InquiryResult()
        {
            InformationManager.ShowInquiry(new InquiryData(new TextObject("Info", null).ToString(), new TextObject(_truvaTroop.SettlementName + " Truva Troop will be deleted. Are you sure?", null).ToString(),
true, true, new TextObject("{=yS7PvrTD}OK", null).ToString(), new TextObject("Cancel", null).ToString(), OnOkClicked, null, "", 0f, null, null, null), false, false);

            return true;
        }

        private void OnOkClicked()
        {
            TruvaCampaignBehavior.TruvaTroops.Remove(TruvaHelper.FindTruvaTroop(_truvaTroop.SettlementId));
            OnTruvaTroopDestroyedEvent?.Invoke(_truvaTroop.SettlementId);

            PartyScreenManager.CloseScreen(true,true);
        }
    }
}
