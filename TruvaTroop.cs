using Bannerlord.ButterLib.ObjectSystem.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace Truva
{
    public class TruvaTroop 
    {
        [SaveableField(1)]
        private CharacterObject _troopLeader;

        [SaveableField(2)]
        private TroopRoster _troopRoster;

        [SaveableField(3)]
        private List<TroopRoster> _onWayTroops = new List<TroopRoster>();

        [SaveableField(4)]
        private string _settlementId;

        [SaveableField(5)]
        private bool _isAtWar = false;

        [SaveableField(6)]
        private bool _isArrivedToSettlement = false;

        public TruvaTroop(CharacterObject troopLeader, TroopRoster truvaTroopRoster, string settlementId)
        {
            troopLeader.HeroObject.ChangeState(Hero.CharacterStates.Disabled);

            _troopLeader = troopLeader;
            _troopRoster = truvaTroopRoster;
            _settlementId = settlementId;
            _isArrivedToSettlement = false;
            SetFlags();
        }

        public TextObject SettlementName
        {
            get { return Settlement.Find(_settlementId).Name; }
        }

        public string SettlementId
        {
            get { return _settlementId; }
        }

        public int MemberCount
        {
            get { return TroopRoster.Count; }
        }

        public Hero TroopLeader
        {
            get
            {
                return _troopLeader.HeroObject;
            }
        }

        public IFaction SettlementOwner
        {
            get
            {
                return Settlement.Find(_settlementId).OwnerClan.MapFaction;
            }
        }

        public int OnWayTroopCount => _onWayTroops.Count;

        [SaveableProperty(1)]
        public bool IsAtWar
        {
            get => _isAtWar;
            set { _isAtWar = value; }
        }

        [SaveableProperty(2)]
        public bool IsArrivedToSettlement
        {
            get { return _isArrivedToSettlement; }
            set { _isArrivedToSettlement = value; }
        }

        [SaveableProperty(3)]
        public bool IsOnTheWay { get; set; }

        [SaveableProperty(4)]
        public bool IsRemoving { get; set; }


        [SaveableProperty(5)]
        public TroopRoster TroopRoster
        {
            get { return _troopRoster; }
            set
            {
                _troopRoster = value;
            }
        }


        public void AddOnWayTroop(TroopRoster troopRoster)
        {
            if(_onWayTroops == null)
                _onWayTroops = new List<TroopRoster>();

            IsOnTheWay = true;

            _onWayTroops.Add(troopRoster);
        }

        public void AddToTruvaTroop(TroopRoster troopRoster)
        {
            if(_onWayTroops.Contains(troopRoster))
                _onWayTroops.Remove(troopRoster);

            for (int i = 0; i < troopRoster.Count; i++)
                troopRoster.GetCharacterAtIndex(i).SetFlag("TruvaTroop");

            //InformationManager.DisplayMessage(new InformationMessage("AddToTruvaTroop On Way: ", Colors.Green));

            _troopRoster.Add(troopRoster);
        }


        public void OnTimeIsUp()
        {
            IsOnTheWay = false;
        }

        public void SetFlags()
        {
            for (int i = 0; i < _troopRoster.Count; i++)
            {
                _troopRoster.GetCharacterAtIndex(i).SetFlag("TruvaTroop");
            }
        }

    }

}
