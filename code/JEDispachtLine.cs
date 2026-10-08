using System;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.JE
{
  [Serializable]
  [PXCacheName("JEDispachtLine")]
  public class JEDispachtLine : PXBqlTable, IBqlTable
  {
    #region Keys
    public class PK : PrimaryKeyOf<JEDispachtLine>.By<dispachtNbr, lineNbr>
    {
      public static JEDispachtLine Find(PXGraph graph, string dispachtNbr, int lineNbr)
        => FindBy(graph, dispachtNbr, lineNbr);
    }

    public class JEDispachtFK : JEDispacht.PK.ForeignKeyOf<JEDispacht>.By<dispachtNbr> { }
    #endregion

    #region DispachtNbr
    [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = "")]
    [PXParent(typeof(Select<JEDispacht, Where<JEDispacht.dispachtNbr, Equal<Current<dispachtNbr>>>>))]
    public virtual string DispachtNbr { get; set; }
    public abstract class dispachtNbr : PX.Data.BQL.BqlString.Field<dispachtNbr> { }
    #endregion

    #region LineNbr
    [PXDBInt(IsKey = true)]
    [PXUIField(DisplayName = "Line ID", Visible = false, Enabled = false)]
    public virtual int? LineNbr { get; set; }
    public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
    #endregion

    #region DisplayLineNbr
    // Unbound display position; never changes the persisted line key.
    [PXInt]
    [PXUIField(DisplayName = "Line Nbr.", Enabled = false)]
    public virtual int? DisplayLineNbr { get; set; }
    public abstract class displayLineNbr : PX.Data.BQL.BqlInt.Field<displayLineNbr> { }
    #endregion

    #region DocType
    // "SO" or "PO" - identifies which existing document this collapsed line refers to.
    [PXDBString(2, IsFixed = true)]
    [PXUIField(DisplayName = "Doc. Type", Enabled = false)]
    [JEDispachtDocType.List()]
    public virtual string DocType { get; set; }
    public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
    #endregion

    #region OrderType
    [PXDBString(2, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Order Type", Enabled = false)]
    public virtual string OrderType { get; set; }
    public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
    #endregion

    #region OrderNbr
    [PXDBString(15, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Order Nbr.", Enabled = false)]
    public virtual string OrderNbr { get; set; }
    public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
    #endregion

    #region Descr
    [PXDBString(256, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Description", Enabled = false)]
    public virtual string Descr { get; set; }
    public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
    #endregion

    #region CreatedByID
    [PXDBCreatedByID()]
    public virtual Guid? CreatedByID { get; set; }
    public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
    #endregion

    #region CreatedByScreenID
    [PXDBCreatedByScreenID()]
    public virtual string CreatedByScreenID { get; set; }
    public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
    #endregion

    #region CreatedDateTime
    [PXDBCreatedDateTime(DisplayMask = "g", InputMask = "g")]
    [PXUIField(DisplayName = "Created Datetime", Enabled = false, IsReadOnly = true)]
    public virtual DateTime? CreatedDateTime { get; set; }
    public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
    #endregion

    #region LastModifiedByID
    [PXDBLastModifiedByID()]
    public virtual Guid? LastModifiedByID { get; set; }
    public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
    #endregion

    #region LastModifiedByScreenID
    [PXDBLastModifiedByScreenID()]
    public virtual string LastModifiedByScreenID { get; set; }
    public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
    #endregion

    #region LastModifiedDateTime
    [PXDBLastModifiedDateTime]
    [PXUIField(DisplayName = "Last Modified Datetime")]
    public virtual DateTime? LastModifiedDateTime { get; set; }
    public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
    #endregion

    #region Tstamp
    [PXDBTimestamp()]
    public virtual byte[] Tstamp { get; set; }
    public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
    #endregion

    #region NoteID
    [PXNote()]
    public virtual Guid? NoteID { get; set; }
    public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
    #endregion
  }

  public class JEDispachtDocType
  {
    public const string SO = "SO";
    public const string PO = "PO";

    public class so : PX.Data.BQL.BqlString.Constant<so>
    {
      public so() : base(SO) { }
    }

    public class po : PX.Data.BQL.BqlString.Constant<po>
    {
      public po() : base(PO) { }
    }

    public class List : PXStringListAttribute
    {
      public List()
        : base(
          new string[] { SO, PO },
          new string[] { "Sales Order", "Purchase Order" })
      { }
    }
  }
}
