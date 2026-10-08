using System;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.IN;

namespace PX.Objects.JE
{
    // Persistent snapshot. Replaces the stable version projection.
    [Serializable]
    [PXCacheName("Dispacht Material")]
    public class JEINDispachtMaterial : PXBqlTable, IBqlTable
    {
        public abstract class dispachtNbr : BqlString.Field<dispachtNbr> { }
        [PXDBString(15, IsKey = true, IsUnicode = true)]
        [PXDBDefault(typeof(JEDispacht.dispachtNbr))]
        [PXParent(typeof(Select<JEDispacht, Where<JEDispacht.dispachtNbr, Equal<Current<dispachtNbr>>>>))]
        public virtual string DispachtNbr { get; set; }

        public abstract class dispachtLineNbr : BqlInt.Field<dispachtLineNbr> { }
        [PXDBInt(IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Document Line", Enabled = false)]
        public virtual int? DispachtLineNbr { get; set; }

        public abstract class sourceLineNbr : BqlInt.Field<sourceLineNbr> { }
        [PXDBInt(IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Source Line", Enabled = false)]
        public virtual int? SourceLineNbr { get; set; }

        public abstract class docType : BqlString.Field<docType> { }
        [PXDBString(2, IsFixed = true)]
        [JEDispachtDocType.List]
        [PXUIField(DisplayName = "Document Type", Enabled = false)]
        public virtual string DocType { get; set; }

        public abstract class orderType : BqlString.Field<orderType> { }
        [PXDBString(2, IsUnicode = true)]
        [PXUIField(DisplayName = "Order Type", Enabled = false)]
        public virtual string OrderType { get; set; }

        public abstract class orderNbr : BqlString.Field<orderNbr> { }
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Order Nbr.", Enabled = false)]
        public virtual string OrderNbr { get; set; }

        public abstract class inventoryID : BqlInt.Field<inventoryID> { }
        [PXDBInt]
        [PXDefault]
        [PXSelector(typeof(Search<InventoryItem.inventoryID>), SubstituteKey = typeof(InventoryItem.inventoryCD), DescriptionField = typeof(InventoryItem.descr))]
        [PXUIField(DisplayName = "Inventory ID", Enabled = false)]
        public virtual int? InventoryID { get; set; }

        public abstract class subItemID : BqlInt.Field<subItemID> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Subitem", Enabled = false)]
        public virtual int? SubItemID { get; set; }

        public abstract class descr : BqlString.Field<descr> { }
        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Enabled = false)]
        public virtual string Descr { get; set; }

        public abstract class qty : BqlDecimal.Field<qty> { }
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Order Qty.", Enabled = false)]
        public virtual decimal? Qty { get; set; }

        public abstract class uOM : BqlString.Field<uOM> { }
        [PXDBString(6, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "UOM", Enabled = false)]
        public virtual string UOM { get; set; }

        public abstract class siteID : BqlInt.Field<siteID> { }
        [PXDBInt]
        [PXDefault]
        [PXSelector(typeof(Search<INSite.siteID>), SubstituteKey = typeof(INSite.siteCD))]
        [PXUIField(DisplayName = "Warehouse", Enabled = false)]
        public virtual int? SiteID { get; set; }

        public abstract class locationID : BqlInt.Field<locationID> { }
        [PXDBInt]
        [PXDefault]
        [PXSelector(typeof(Search<INLocation.locationID, Where<INLocation.siteID, Equal<Current<siteID>>>>), SubstituteKey = typeof(INLocation.locationCD))]
        [PXUIField(DisplayName = "Project Location", Enabled = false)]
        public virtual int? LocationID { get; set; }

        public abstract class taskID : BqlInt.Field<taskID> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Project Task ID", Enabled = false)]
        public virtual int? TaskID { get; set; }

        public abstract class costCodeID : BqlInt.Field<costCodeID> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Cost Code ID", Enabled = false)]
        public virtual int? CostCodeID { get; set; }

        public abstract class dispatchQty : BqlDecimal.Field<dispatchQty> { }
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Dispatch Qty.")]
        public virtual decimal? DispatchQty { get; set; }

        public abstract class availableQty : BqlDecimal.Field<availableQty> { }
        [PXDecimal(6)]
        [PXUIField(DisplayName = "Available Qty. (UOM)", Enabled = false)]
        public virtual decimal? AvailableQty { get; set; }

        public abstract class createdByID : BqlGuid.Field<createdByID> { }
        [PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }

        public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }
        [PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime { get; set; }

        public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }
        [PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID { get; set; }

        public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }
        [PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime { get; set; }

        public abstract class tstamp : BqlByteArray.Field<tstamp> { }
        [PXDBTimestamp]
        public virtual byte[] Tstamp { get; set; }

    }
}
