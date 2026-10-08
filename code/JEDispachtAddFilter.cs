using System;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.PM;

namespace PX.Objects.JE
{
    /// <summary>Stores the document type and order-number criteria for the Add Documents dialog.</summary>
    // Filter + selectable row used by the "Add Documents" dialog on JEDispachtEntry.
    // Only used to browse existing SOOrder/POOrder headers; nothing here is persisted to SO/PO.

    [Serializable]
    [PXHidden]
    public class JEDispachtAddFilter : PXBqlTable, IBqlTable
    {
        #region DocType
        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

        [PXDBString(2, IsFixed = true)]
        [PXDefault(JEDispachtDocType.PO)]
        [PXUIField(DisplayName = "Doc. Type", Required = true)]
        [JEDispachtDocType.List()]
        public virtual string DocType { get; set; }
        #endregion

        #region OrderNbr
        public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }

        [PXString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Order Nbr.")]
        public virtual string OrderNbr { get; set; }
        #endregion
    }

    /// <summary>Represents a selectable source order row in the Add Documents dialog.</summary>
    /// <remarks>Autor: Jose Vivanco; GitHub: josvmora; Fecha: Octubre 2026.</remarks>
    [Serializable]
    [PXHidden]
    public class JEDispachtAddRow : PXBqlTable, IBqlTable
    {
        #region UserID
        public abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }

        [PXDBGuid(IsKey = true)]
        [PXUIField(Visible = false)]
        public virtual Guid? UserID { get; set; }
        #endregion

        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        [PXDBBool]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected { get; set; }
        #endregion

        #region DocType
        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Doc. Type", Enabled = false)]
        public virtual string DocType { get; set; }
        #endregion

        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        [PXDBString(2, IsUnicode = true)]
        [PXUIField(DisplayName = "Order Type", Enabled = false)]
        public virtual string OrderType { get; set; }
        #endregion

        #region OrderNbr
        public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }

        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXUIField(DisplayName = "Order Nbr.", Enabled = false)]
        public virtual string OrderNbr { get; set; }
        #endregion

        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        [PXDBInt]
        [PXSelector(
            typeof(Search<PMProject.contractID>),
            typeof(PMProject.contractCD),
            typeof(PMProject.description),
            DescriptionField = typeof(PMProject.description),
            SubstituteKey = typeof(PMProject.contractCD))]
        [PXUIField(DisplayName = "Project", Enabled = false)]
        public virtual int? ProjectID { get; set; }
        #endregion

        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Enabled = false)]
        public virtual string Descr { get; set; }
        #endregion
    }
}
