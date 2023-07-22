using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.SaveSystem;

namespace Truva
{
    public class CustomTruvaSaveableTypeDefiner: SaveableTypeDefiner
    {
        public CustomTruvaSaveableTypeDefiner() : base(1984999991) { }

        protected override void DefineClassTypes()
        {
            base.AddClassDefinition(typeof(TruvaTroop), 1);
            base.AddClassDefinition(typeof(TruvaTimeChecker), 2);
        }

        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(List<TruvaTroop>));
            ConstructContainerDefinition(typeof(List<TroopRoster>));
            ConstructContainerDefinition(typeof(List<TruvaTimeChecker>));
            ConstructContainerDefinition(typeof(MbEvent<TruvaTroop, TroopRoster>));
            //ConstructContainerDefinition(typeof(Action<TruvaTroop, TroopRoster>));
        }


    }
}
