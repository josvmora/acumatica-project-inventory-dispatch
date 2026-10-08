using System;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.IN;

namespace PX.Objects.JE
{
    [Serializable]
    [PXCacheName("Dispacht Issue")]
    public class JEDispachtIssue : PXBqlTable, IBqlTable
    {
        public abstract class dispachtNbr : BqlString.Field<dispachtNbr> { }
        [PXDBString(15, IsKey = true, IsUnicode = true)]
        [PXDBDefault(typeof(JEDispacht.dispachtNbr))]
        [PXParent(typeof(Select<JEDispacht, Where<JEDispacht.dispachtNbr, Equal<Current<dispachtNbr>>>>))]
        public virtual string DispachtNbr { get; set; }

        public abstract class siteID : BqlInt.Field<siteID> { }
        [PXDBInt(IsKey = true)]
        [PXDefault]
        [PXSelector(typeof(Search<INSite.siteID>), SubstituteKey = typeof(INSite.siteCD))]
        [PXUIField(DisplayName = "Warehouse", Enabled = false)]
        public virtual int? SiteID { get; set; }

        public abstract class docType : BqlString.Field<docType> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Document Type", Enabled = false)]
        public virtual string DocType { get; set; }

        public abstract class refNbr : BqlString.Field<refNbr> { }
        [PXDBString(15, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Issue Nbr.", Enabled = false)]
        public virtual string RefNbr { get; set; }

        public abstract class reasonCode : BqlString.Field<reasonCode> { }
        [PXDBString(20, IsUnicode = true)]
        [PXUIField(DisplayName = "Reason Code", Enabled = false)]
        public virtual string ReasonCode { get; set; }

        public abstract class createdByID : BqlGuid.Field<createdByID> { }
        [PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }

        public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }
        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = "Created", Enabled = false)]
        public virtual DateTime? CreatedDateTime { get; set; }

        public abstract class tstamp : BqlByteArray.Field<tstamp> { }
        [PXDBTimestamp]
        public virtual byte[] Tstamp { get; set; }

    }
}
