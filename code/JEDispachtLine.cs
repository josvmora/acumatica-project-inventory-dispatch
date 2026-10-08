using System;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.JE
{
  /// <summary>Represents a source sales or purchase order attached to a dispatch.</summary>
  /// <remarks>Autor: Jose Vivanco; GitHub: josvmora; Fecha: Octubre 2026.</remarks>
  [Serializable]
  [PXCacheName("JEDispachtLine")]
  public class JEDispachtLine : PXBqlTable, IBqlTable
  {
    #region Keys
    /// <summary>Defines the composite primary key for a dispatch line.</summary>
    /// <remarks>Autor: Jose Vivanco; GitHub: josvmora; Fecha: Octubre 2026.</remarks>
    public class PK : PrimaryKeyOf<JEDispachtLine>.By<dispachtNbr, lineNbr>
    {
      /// <summary>Finds a dispatch line by dispatch number and line number.</summary>
      /// <param name="graph">The graph used to execute the query.</param>
      /// <param name="dispachtNbr">The parent dispatch number.</param>
      /// <param name="lineNbr">The line number.</param>
      /// <returns>The matching line, or <see langword="null"/> if none exists.</returns>
      public static JEDispachtLine Find(PXGraph graph, string dispachtNbr, int lineNbr)
        => FindBy(graph, dispachtNbr, lineNbr);
    }

    /// <summary>Defines the foreign key from this line to its dispatch header.</summary>
    /// <remarks>Autor: Jose Vivanco; GitHub: josvmora; Fecha: Octubre 2026.</remarks>
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

  /// <summary>Defines the source order types available on dispatch lines.</summary>
  /// <remarks>Autor: Jose Vivanco; GitHub: josvmora; Fecha: Octubre 2026.</remarks>
  public class JEDispachtDocType
  {
    public const string SO = "SO";
    public const string PO = "PO";

    /// <summary>BQL constant representing a sales order source.</summary>
    /// <remarks>Autor: Jose Vivanco; GitHub: josvmora; Fecha: Octubre 2026.</remarks>
    public class so : PX.Data.BQL.BqlString.Constant<so>
    {
      /// <summary>Initializes the constant with the sales order code.</summary>
      public so() : base(SO) { }
    }

    /// <summary>BQL constant representing a purchase order source.</summary>
    /// <remarks>Autor: Jose Vivanco; GitHub: josvmora; Fecha: Octubre 2026.</remarks>
    public class po : PX.Data.BQL.BqlString.Constant<po>
    {
      /// <summary>Initializes the constant with the purchase order code.</summary>
      public po() : base(PO) { }
    }

    /// <summary>Provides source order types and display labels for UI fields.</summary>
    /// <remarks>Autor: Jose Vivanco; GitHub: josvmora; Fecha: Octubre 2026.</remarks>
    public class List : PXStringListAttribute
    {
      /// <summary>Initializes the selector with the supported source order types.</summary>
      public List()
        : base(
          new string[] { SO, PO },
          new string[] { "Sales Order", "Purchase Order" })
      { }
    }
  }
}
