using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Library;

namespace Truva.ViewModel
{
    internal class TruvaTroopItemVM : TaleWorlds.Library.ViewModel
    {
        private MBBindingList<TruvaTroopItemContentVM> _truvaTroopContents;

        private TruvaTroop _truvaTroop;

        public TruvaTroopItemVM(TruvaTroopVM truvaTroopVM,TruvaTroop truvaTroop)
        {
            _truvaTroop = truvaTroop;
            TruvaTroopContents = new MBBindingList<TruvaTroopItemContentVM>();
            TruvaTroopContents.Add(new TruvaTroopItemContentVM(truvaTroopVM, truvaTroop));
        }

        [DataSourceProperty]
        public string SettlementName
        {
            get
            {
                return this._truvaTroop.SettlementName.ToString();
            }
        }


        [DataSourceProperty]
        public MBBindingList<TruvaTroopItemContentVM> TruvaTroopContents
        {
            get
            {
                return this._truvaTroopContents;
            }
            set
            {
                if (value != this._truvaTroopContents)
                {
                    this._truvaTroopContents = value;
                    base.OnPropertyChangedWithValue<MBBindingList<TruvaTroopItemContentVM>>(value, "TruvaTroopContents");
                }
            }
        }

    }
}