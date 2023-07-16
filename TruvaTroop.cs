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
        [SaveableField(1)] private CharacterObject _troopLeader;
        [SaveableField(2)] private TroopRoster _truvaTroop;
        [SaveableField(3)] private int _memberCount;
        [SaveableField(4)] private string _settlementId;
        [SaveableField(5)] private bool _isAtWar = false;

        public TruvaTroop(CharacterObject troopLeader, TroopRoster truvaTroop, string settlementId)
        {
            troopLeader.HeroObject.ChangeState(Hero.CharacterStates.Disabled);

            _troopLeader = troopLeader;
            _truvaTroop = truvaTroop;
            _settlementId = settlementId;
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
            get { return _memberCount; }
        }

        public TroopRoster TroopRoster
        {
            get { return _truvaTroop; }
            set
            {
                _truvaTroop = value;
            }
        }

        public Hero TroopLeader
        {
            get
            {
                return _troopLeader.HeroObject;
            }
        }


        public bool IsAtWar
        {
            get => _isAtWar;
            set { _isAtWar = value; }
        }

        public class TruvaTroopTypeDefiner : SaveableTypeDefiner
        {
            public TruvaTroopTypeDefiner() : base(1984999991) { }

            protected override void DefineClassTypes()
            {
                base.AddClassDefinition(typeof(TruvaTroop), 1);
            }

            protected override void DefineContainerDefinitions()
            {
                ConstructContainerDefinition(typeof(List<TruvaTroop>));
            }
        }


    }

}
