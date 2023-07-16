using SandBox.View.Map;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace Truva.MissionView
{
    public class TruvaSiegeVM : TaleWorlds.Library.ViewModel
    {
        private bool _type0IsHidden;
        private bool _type1IsHidden;
        private bool _type2IsHidden;
        private bool _type3IsHidden;
        private bool _type4IsHidden;
        private bool _type5IsHidden;

        private bool _callTruvaIsHidden;
        private bool _callTruvaCancelIsHidden;

        public TruvaSiegeVM()
        {
        }

        [DataSourceProperty]
        public bool IsTruvaTroopSpawned
        {
            get
            {
                return !Mission.Current.GetMissionBehavior<TruvaSiegeLogic>().IsTruvaTroopSpawned;
            }
            set { }
        }


        [DataSourceProperty]
        public bool CanShowSpawnChoose
        {
            get
            {
                return Mission.Current.GetMissionBehavior<TruvaSiegeLogic>().IsTruvaTroopSpawned;
            }
            set { }
        }


        [DataSourceProperty]
        public bool CallTruvaIsHidden
        {
            get
            {
                return this._callTruvaIsHidden;
            }
            set
            {
                if (value != this._callTruvaIsHidden)
                {
                    this._callTruvaIsHidden = value;
                    base.OnPropertyChangedWithValue(value, "CallTruvaIsHidden");
                }
            }
        }


        [DataSourceProperty]
        public bool CallTruvaCancelIsHidden
        {
            get
            {
                return this._callTruvaCancelIsHidden;
            }
            set
            {
                if (value != this._callTruvaCancelIsHidden)
                {
                    this._callTruvaCancelIsHidden = value;
                    base.OnPropertyChangedWithValue(value, "CallTruvaCancelIsHidden");
                }
            }
        }



        [DataSourceProperty]
        public bool Type0IsHidden
        {
            get
            {
                return this._type0IsHidden;
            }
            set
            {
                if (value != this._type0IsHidden)
                {
                    this._type0IsHidden = value;
                    base.OnPropertyChangedWithValue(value, "Type0IsHidden");
                }
            }
        }


        [DataSourceProperty]
        public bool Type1IsHidden
        {
            get
            {
                return this._type1IsHidden;
            }
            set
            {
                if (value != this._type1IsHidden)
                {
                    this._type1IsHidden = value;
                    base.OnPropertyChangedWithValue(value, "Type1IsHidden");
                }
            }
        }

        [DataSourceProperty]
        public bool Type2IsHidden
        {
            get
            {
                return this._type2IsHidden;
            }
            set
            {
                if (value != this._type2IsHidden)
                {
                    this._type2IsHidden = value;
                    base.OnPropertyChangedWithValue(value, "Type2IsHidden");
                }
            }
        }

        [DataSourceProperty]
        public bool Type3IsHidden
        {
            get
            {
                return this._type3IsHidden;
            }
            set
            {
                if (value != this._type3IsHidden)
                {
                    this._type3IsHidden = value;
                    base.OnPropertyChangedWithValue(value, "Type3IsHidden");
                }
            }
        }

        [DataSourceProperty]
        public bool Type4IsHidden
        {
            get
            {
                return this._type4IsHidden;
            }
            set
            {
                if (value != this._type4IsHidden)
                {
                    this._type4IsHidden = value;
                    base.OnPropertyChangedWithValue(value, "Type4IsHidden");
                }
            }
        }


        [DataSourceProperty]
        public bool Type5IsHidden
        {
            get
            {
                return this._type5IsHidden;
            }
            set
            {
                if (value != this._type5IsHidden)
                {
                    this._type5IsHidden = value;
                    base.OnPropertyChangedWithValue(value, "Type5IsHidden");
                }
            }
        }

    }
}
