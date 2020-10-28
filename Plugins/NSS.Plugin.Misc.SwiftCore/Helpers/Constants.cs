using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.Helpers
{
    public static class Constants
    {
        /// <summary>
        /// Gets NSS Wintrix key name
        /// </summary>
        public static string WintrixKeyAttribute => "ErpId";

        #region NSSCustomerAttributes           
        public static string IsExistingCustomerAttribute => "Existing Customer";
        public static string PreferredLocationIdAttribute => "Preferred Location";
        public static string HearAboutUsAttribute => "Hear About Us";
        public static string OtherAttribute => "Other";
        public static string ItemsForNextProjectAttribute => "Items For Next Project";
        public static string NSSApprovedAttribute => "NSSApproved";
        #endregion

        #region ProductAttributes
        public static string CutOptionsAttribute => "Cut Options";
        public static string WorkOrderInstructionsAttribute => "Work Order Instructions";
        public static string LengthToleranceCutAttribute => "Length Tolerance Cut";
        public static string PurchaseUnitAttribute => "Purchase Unit";
        public static string CustomerPartNoAttribute => "Customer Part No";
        #endregion

        #region ProductFields
        public static string MTRFieldAttribute => "mtr";
        public static string ItemIdFieldAttribute => "itemId";
        public static string MetalFieldAttribute => "metal";
        public static string GradeFieldAttribute => "grade";
        public static string CoatingFieldAttribute => "coating";
        public static string DisplayThicknessFieldAttribute => "displayThickness";
        public static string ConditionFieldAttribute => "condition";
        public static string CountryOfOriginFieldAttribute => "countryOfOrigin";
        public static string DisplayWidthFieldAttribute => "displayWidth";
        #endregion

        #region UnitOfPurchase
        public static string UnitPerPieceField => "EA";
        public static string UnitPerFtField => "FT";
        public static string UnitPerWeightField => "CWT";
        #endregion


        public static string MTRBlobName => "Product-MTR.{0}.pdf";

        public static string ThemeName => "SwiftPortal";
    }
}
