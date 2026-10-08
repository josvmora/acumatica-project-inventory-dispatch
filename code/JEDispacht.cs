// DISPACHT FINAL - VERSION V2 - 2026-10-07 - AGREGA ESTADO DISPATCHING (D); CONSERVA COMPLETED (C).aaaaaa
using System;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.PM;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.GL;

namespace PX.Objects.JE
{
  /// <summary>
  /// Stores the dispatch header and its shipping, project, and audit information.
  /// </summary>
  /// <remarks>Autor: Jose Vivanco; GitHub: josvmora; Fecha: Octubre 2026.</remarks>
  [Serializable]
  [PXCacheName("JEDispacht")]
  [PXPrimaryGraph(typeof(JEDispachtEntry))]
  public class JEDispacht : PXBqlTable, IBqlTable
  {
    #region Keys
    /// <summary>Defines the primary key for a dispatch.</summary>
    /// <remarks>Autor: Jose Vivanco; GitHub: josvmora; Fecha: Octubre 2026.</remarks>
    public class PK : PrimaryKeyOf<JEDispacht>.By<dispachtNbr>
    {
      /// <summary>Finds a dispatch by its dispatch number.</summary>
      /// <param name="graph">The graph used to execute the query.</param>
      /// <param name="dispachtNbr">The dispatch number to find.</param>
      /// <returns>The matching dispatch, or <see langword="null"/> if none exists.</returns>
      public static JEDispacht Find(PXGraph graph, string dispachtNbr)
        => FindBy(graph, dispachtNbr);
    }
    #endregion

    #region DispachtNbr
    [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = "")]
    [PXDefault]
    [PXSelector(typeof(Search<JEDispacht.dispachtNbr>))]
    [PXUIField(DisplayName = "Dispacht Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
    public virtual string DispachtNbr { get; set; }
    public abstract class dispachtNbr : PX.Data.BQL.BqlString.Field<dispachtNbr> { }
    #endregion

    #region BranchID
    [Branch(typeof(AccessInfo.branchID), IsDetail = false,
        PersistingCheck = PXPersistingCheck.Null)]
    public virtual int? BranchID { get; set; }
    public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
    #endregion

    #region Descr
    [PXDBString(256, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Description")]
    public virtual string Descr { get; set; }
    public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
    #endregion

    #region Status
    [PXDBString(1, IsFixed = true)]
    [PXDefault(JEDispachtStatus.Hold)]
    [PXUIField(DisplayName = "Status", Enabled = false)]
    [JEDispachtStatus.List()]
    public virtual string Status { get; set; }
    public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
    #endregion

    #region DispachtDate
    [PXDBDate()]
    [PXDefault(typeof(AccessInfo.businessDate))]
    [PXUIField(DisplayName = "Dispacht Date")]
    public virtual DateTime? DispachtDate { get; set; }
    public abstract class dispachtDate : PX.Data.BQL.BqlDateTime.Field<dispachtDate> { }
    #endregion

    #region ShipDate
    [PXDBDate]
    [PXUIField(DisplayName = "Ship Date")]
    public virtual DateTime? ShipDate { get; set; }
    public abstract class shipDate : PX.Data.BQL.BqlDateTime.Field<shipDate> { }
    #endregion

    #region CarrierID
    [Vendor(DisplayName = "Carrier", DescriptionField = typeof(Vendor.acctName))]
    [PXDefault]
    [PXRestrictor(typeof(Where<Vendor.type, Equal<BAccountType.vendorType>,
        Or<Vendor.type, Equal<BAccountType.combinedType>>>),
        "Carrier must be a vendor.")]
    public virtual int? CarrierID { get; set; }
    public abstract class carrierID : PX.Data.BQL.BqlInt.Field<carrierID> { }
    #endregion

    #region PickupDate
    [PXDBDate]
    [PXUIField(DisplayName = "Pickup Date")]
    public virtual DateTime? PickupDate { get; set; }
    public abstract class pickupDate : PX.Data.BQL.BqlDateTime.Field<pickupDate> { }
    #endregion

    #region DropoffDate
    [PXDBDate]
    [PXUIField(DisplayName = "Dropoff Date")]
    public virtual DateTime? DropoffDate { get; set; }
    public abstract class dropoffDate : PX.Data.BQL.BqlDateTime.Field<dropoffDate> { }
    #endregion

    #region ShippingTo
    [PXDBString(2000, IsUnicode = true)]
    [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
    [PXUIField(DisplayName = "Shipping To")]
    public virtual string ShippingTo { get; set; }
    public abstract class shippingTo : PX.Data.BQL.BqlString.Field<shippingTo> { }
    #endregion

    #region ProjectID
    // Header project: the "Add Documents" picker only shows SO/PO from this project, or with no project at all.
    [PXDBInt()]
    [PXDefault]
    [PXSelector(
      typeof(Search<PMProject.contractID>),
      typeof(PMProject.contractCD),
      typeof(PMProject.description),
      DescriptionField = typeof(PMProject.description),
      SubstituteKey = typeof(PMProject.contractCD))]
    [PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), "Project is cancelled.")]
    [PXUIField(DisplayName = "Project")]
    public virtual int? ProjectID { get; set; }
    public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
    #endregion

    #region ProjectName
    // Unbound description used by the screen's record title.
    [PXString(256, IsUnicode = true)]
    [PXUIField(DisplayName = "Project Name", Enabled = false)]
    public virtual string ProjectName { get; set; }
    public abstract class projectName : PX.Data.BQL.BqlString.Field<projectName> { }
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

  /// <summary>Defines dispatch status values and their selector labels.</summary>
  /// <remarks>Autor: Jose Vivanco; GitHub: josvmora; Fecha: Octubre 2026.</remarks>
  public class JEDispachtStatus
  {
    public const string Hold = "H";
    public const string Open = "O";
    public const string Dispatching = "D";
    public const string Completed = "C";
    public const string Cancelled = "X";

    /// <summary>BQL constant for the on-hold status.</summary>
    /// <remarks>Autor: Jose Vivanco; GitHub: josvmora; Fecha: Octubre 2026.</remarks>
    public class hold : PX.Data.BQL.BqlString.Constant<hold>
    {
      /// <summary>Initializes the constant with the on-hold status value.</summary>
      public hold() : base(Hold) { }
    }

    /// <summary>BQL constant for the open status.</summary>
    /// <remarks>Autor: Jose Vivanco; GitHub: josvmora; Fecha: Octubre 2026.</remarks>
    public class open : PX.Data.BQL.BqlString.Constant<open>
    {
      /// <summary>Initializes the constant with the open status value.</summary>
      public open() : base(Open) { }
    }

    /// <summary>BQL constant for the dispatching status.</summary>
    /// <remarks>Autor: Jose Vivanco; GitHub: josvmora; Fecha: Octubre 2026.</remarks>
    public class dispatching : PX.Data.BQL.BqlString.Constant<dispatching>
    {
      /// <summary>Initializes the constant with the dispatching status value.</summary>
      public dispatching() : base(Dispatching) { }
    }

    /// <summary>BQL constant for the completed status.</summary>
    /// <remarks>Autor: Jose Vivanco; GitHub: josvmora; Fecha: Octubre 2026.</remarks>
    public class completed : PX.Data.BQL.BqlString.Constant<completed>
    {
      /// <summary>Initializes the constant with the completed status value.</summary>
      public completed() : base(Completed) { }
    }

    /// <summary>BQL constant for the cancelled status.</summary>
    /// <remarks>Autor: Jose Vivanco; GitHub: josvmora; Fecha: Octubre 2026.</remarks>
    public class cancelled : PX.Data.BQL.BqlString.Constant<cancelled>
    {
      /// <summary>Initializes the constant with the cancelled status value.</summary>
      public cancelled() : base(Cancelled) { }
    }

    /// <summary>Provides the status values and display labels for UI fields.</summary>
    /// <remarks>Autor: Jose Vivanco; GitHub: josvmora; Fecha: Octubre 2026.</remarks>
    public class ListAttribute : PXStringListAttribute
    {
      /// <summary>Initializes the selector with all dispatch statuses.</summary>
      public ListAttribute()
        : base(
          new string[] { Hold, Open, Dispatching, Completed, Cancelled },
          new string[] { "On Hold", "Open", "Dispatching", "Completed", "Cancelled" })
      { }
    }
  }
}
