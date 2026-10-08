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
  [Serializable]
  [PXCacheName("JEDispacht")]
  [PXPrimaryGraph(typeof(JEDispachtEntry))]
  public class JEDispacht : PXBqlTable, IBqlTable
  {
    #region Keys
    public class PK : PrimaryKeyOf<JEDispacht>.By<dispachtNbr>
    {
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

  public class JEDispachtStatus
  {
    public const string Hold = "H";
    public const string Open = "O";
    public const string Dispatching = "D";
    public const string Completed = "C";
    public const string Cancelled = "X";

    public class hold : PX.Data.BQL.BqlString.Constant<hold>
    {
      public hold() : base(Hold) { }
    }

    public class open : PX.Data.BQL.BqlString.Constant<open>
    {
      public open() : base(Open) { }
    }

    public class dispatching : PX.Data.BQL.BqlString.Constant<dispatching>
    {
      public dispatching() : base(Dispatching) { }
    }

    public class completed : PX.Data.BQL.BqlString.Constant<completed>
    {
      public completed() : base(Completed) { }
    }

    public class cancelled : PX.Data.BQL.BqlString.Constant<cancelled>
    {
      public cancelled() : base(Cancelled) { }
    }

    public class ListAttribute : PXStringListAttribute
    {
      public ListAttribute()
        : base(
          new string[] { Hold, Open, Dispatching, Completed, Cancelled },
          new string[] { "On Hold", "Open", "Dispatching", "Completed", "Cancelled" })
      { }
    }
  }
}
