using System;
using System.Collections.Generic;
using System.Text;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using TaleWorlds.Library;

namespace Truva.Extensions
{
    [PrefabExtension("ClanScreen", "descendant::Widget[@Id='TopPanel']/Children/Widget/Children/ListPanel/Children")]
    internal class ClanScreenExtension1: PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Replace;

        [PrefabExtensionFileName]
        public string PatchFileName 
        {
            get 
            {
                return "ClanScreenExtension1.xml";
            }
        }
    }

    [PrefabExtension("ClanScreen", "descendant::Widget[@Id='TopPanel']/Children/Widget/Children/ListPanel[@HorizontalAlignment='Right']/Children")]
    internal class ClanScreenExtension2 : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Replace;

        [PrefabExtensionFileName]
        public string PatchFileName
        {
            get
            {
                return "ClanScreenExtension2.xml";
            }
        }

    }

}
