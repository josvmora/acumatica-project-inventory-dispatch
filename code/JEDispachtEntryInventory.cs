// DISPACHT FINAL - VERSION V2 - 2026-10-07 - DESPACHAR CREA ISSUES SIN LIBERAR Y CAMBIA A DISPATCHING; CIERRE AL LIBERAR TODOS.
using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.SO;

namespace PX.Objects.JE
{
    public partial class JEDispachtEntry
    {
        private const string IssueReason = "PROJECTISSUE";
        private bool buildingMaterials;
        private bool creatingIssues;
        private bool undoingDispatch;
        private bool changingStatus;

        private bool HasIssues()
        {
            if (string.IsNullOrEmpty(Document.Current?.DispachtNbr)) return false;
            return PXSelectReadonly<JEDispachtIssue,
                Where<JEDispachtIssue.dispachtNbr, Equal<Required<JEDispachtIssue.dispachtNbr>>>>
                .SelectWindowed(this, 0, 1, Document.Current.DispachtNbr).Count != 0;
        }

        private bool CanUndoDispatch(JEDispacht dispatch)
        {
            if (!IsSavedDispatch(dispatch) || dispatch.Status != JEDispachtStatus.Dispatching)
                return false;

            bool foundIssue = false;
            foreach (JEDispachtIssue link in PXSelectReadonly<JEDispachtIssue,
                Where<JEDispachtIssue.dispachtNbr,
                    Equal<Required<JEDispachtIssue.dispachtNbr>>>>
                .Select(this, dispatch.DispachtNbr))
            {
                foundIssue = true;
                if (link.DocType != INDocType.Issue) return false;

                INRegister issue = PXSelectReadonly<INRegister,
                    Where<INRegister.docType, Equal<Required<INRegister.docType>>,
                        And<INRegister.refNbr, Equal<Required<INRegister.refNbr>>>>>
                    .Select(this, link.DocType, link.RefNbr);
                if (issue?.RefNbr == null || issue.Released == true || issue.Hold != true)
                    return false;
            }

            return foundIssue;
        }

        // Project/task fields can be supplied by the project inventory DAC extensions.
        // Resolve through the cache, which includes extension fields; never guess a location.
        private int? LocationProject(INLocation location)
        {
            PXCache cache = Caches[typeof(INLocation)];
            if (!cache.Fields.Contains("ProjectID"))
                throw new PXException("INLocation.ProjectID is unavailable. Enable/configure project locations before using this version.");
            return cache.GetValue(location, "ProjectID") as int?;
        }

        private INLocation ResolveLocation(int? siteID)
        {
            if (siteID == null || Document.Current?.ProjectID == null)
                throw new PXException("Warehouse and project are required to resolve the material location.");
            INLocation found = null;
            foreach (INLocation location in PXSelectReadonly<INLocation,
                Where<INLocation.siteID, Equal<Required<INLocation.siteID>>>>.Select(this, siteID))
            {
                if (location.Active != true || location.SalesValid != true
                    || LocationProject(location) != Document.Current.ProjectID) continue;
                if (found != null)
                    throw new PXException("Warehouse {0} has multiple active issue locations for this project ({1}, {2}). Configure a unique project location.", siteID, found.LocationCD, location.LocationCD);
                found = location;
            }
            if (found == null)
                throw new PXException("Warehouse {0} has no active issue location assigned to project {1}.", siteID, Document.Current.ProjectID);
            return found;
        }

        private JEINDispachtMaterial BuildMaterial(JEDispachtLine parent, int? sourceLine,
            int? inventoryID, int? subItemID, string descr, decimal? qty, string uom,
            int? siteID, int? taskID, int? costCodeID)
        {
            InventoryItem item = InventoryItem.PK.Find(this, inventoryID);
            // Charges, services and non-stock lines do not create an inventory issue.
            if (item?.StkItem != true) return null;
            INLocation location = ResolveLocation(siteID);
            int? locationTask = Caches[typeof(INLocation)].GetValue(location, "TaskID") as int?;
            return new JEINDispachtMaterial
            {
                DispachtNbr = parent.DispachtNbr, DispachtLineNbr = parent.LineNbr,
                SourceLineNbr = sourceLine, DocType = parent.DocType,
                OrderType = parent.OrderType, OrderNbr = parent.OrderNbr,
                InventoryID = inventoryID, SubItemID = subItemID, Descr = descr,
                Qty = qty, UOM = uom, SiteID = siteID, LocationID = location.LocationID,
                TaskID = locationTask ?? taskID, CostCodeID = costCodeID, DispatchQty = 0m
            };
        }

        private List<JEINDispachtMaterial> ReadOrderMaterials(JEDispachtLine line)
        {
            var rows = new List<JEINDispachtMaterial>();
            if (line.DocType == JEDispachtDocType.PO)
            {
                foreach (POLine source in PXSelectReadonly<POLine,
                    Where<POLine.orderType, Equal<Required<POLine.orderType>>,
                        And<POLine.orderNbr, Equal<Required<POLine.orderNbr>>>>>
                    .Select(this, line.OrderType, line.OrderNbr))
                {
                    var material = BuildMaterial(line, source.LineNbr, source.InventoryID,
                        source.SubItemID, source.TranDesc, source.OrderQty, source.UOM,
                        source.SiteID, source.TaskID, source.CostCodeID);
                    if (material != null) rows.Add(material);
                }
            }
            else if (line.DocType == JEDispachtDocType.SO)
            {
                SOOrder order = PXSelectReadonly<SOOrder,
                    Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
                        And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>
                    .Select(this, line.OrderType, line.OrderNbr);
                foreach (SOLine source in PXSelectReadonly<SOLine,
                    Where<SOLine.orderType, Equal<Required<SOLine.orderType>>,
                        And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>>>>
                    .Select(this, line.OrderType, line.OrderNbr))
                {
                    var material = BuildMaterial(line, source.LineNbr, source.InventoryID,
                        source.SubItemID, source.TranDesc, source.OrderQty, source.UOM,
                        order?.DestinationSiteID, source.TaskID, source.CostCodeID);
                    if (material != null) rows.Add(material);
                }
            }
            return rows;
        }

        private void LoadOrderMaterials(JEDispachtLine parent)
        {
            // Validate all source rows before inserting any snapshot for this order.
            var rows = ReadOrderMaterials(parent);
            buildingMaterials = true;
            try
            {
                foreach (var row in rows)
                {
                    bool exists = false;
                    foreach (JEINDispachtMaterial old in Materials.Select())
                        if (old.DispachtLineNbr == row.DispachtLineNbr && old.SourceLineNbr == row.SourceLineNbr)
                        { exists = true; break; }
                    if (!exists) Materials.Insert(row);
                }
            }
            finally { buildingMaterials = false; }
        }

        public PXAction<JEDispacht> LoadMaterials;
        [PXButton(CommitChanges = true, Category = "Processing")]
        [PXUIField(DisplayName = "Load Missing Materials", MapEnableRights = PXCacheRights.Update)]
        protected virtual IEnumerable loadMaterials(PXAdapter adapter)
        {
            EnsureDispatchEditable();
            foreach (JEDispachtLine line in Lines.Select())
            {
                string error = GetOrderSelectionError(new JEDispachtAddRow
                { DocType = line.DocType, OrderType = line.OrderType, OrderNbr = line.OrderNbr });
                if (error != null) throw new PXException(error);
                ReadOrderMaterials(line);
            }
            foreach (JEDispachtLine line in Lines.Select()) LoadOrderMaterials(line);
            Materials.View.RequestRefresh();
            return adapter.Get();
        }

        private decimal BaseAvailable(JEINDispachtMaterial row)
        {
            decimal available = 0m;
            foreach (INLocationStatus status in PXSelectReadonly<INLocationStatus,
                Where<INLocationStatus.inventoryID, Equal<Required<INLocationStatus.inventoryID>>,
                    And<INLocationStatus.siteID, Equal<Required<INLocationStatus.siteID>>,
                    And<INLocationStatus.locationID, Equal<Required<INLocationStatus.locationID>>>>>>
                .Select(this, row.InventoryID, row.SiteID, row.LocationID))
            {
                if (status.SubItemID != row.SubItemID) continue;
                // Never permit more than physically on hand even when availability is configured differently.
                available += Math.Min(status.QtyAvail ?? 0m, status.QtyOnHand ?? 0m);
            }
            return Math.Max(0m, available);
        }

        protected virtual void JEINDispachtMaterial_AvailableQty_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            var row = e.Row as JEINDispachtMaterial;
            if (row?.InventoryID == null || string.IsNullOrEmpty(row.UOM)) return;
            e.ReturnValue = INUnitAttribute.ConvertFromBase<JEINDispachtMaterial.inventoryID>(
                sender, row, row.UOM, BaseAvailable(row), INPrecision.QUANTITY);
        }

        private static string StockKey(JEINDispachtMaterial row)
        { return string.Format("{0}|{1}|{2}|{3}", row.InventoryID, row.SubItemID, row.SiteID, row.LocationID); }

        private List<JEINDispachtMaterial> ValidateQuantities(JEINDispachtMaterial replacement = null)
        {
            var totals = new Dictionary<string, decimal>();
            var representatives = new Dictionary<string, JEINDispachtMaterial>();
            var positive = new List<JEINDispachtMaterial>();
            foreach (JEINDispachtMaterial cached in Materials.Select())
            {
                var row = replacement != null && cached.DispachtLineNbr == replacement.DispachtLineNbr
                    && cached.SourceLineNbr == replacement.SourceLineNbr ? replacement : cached;
                if ((row.DispatchQty ?? 0m) < 0m) throw new PXException("Dispatch quantity cannot be negative.");
                if ((row.DispatchQty ?? 0m) == 0m) continue;
                INLocation location = ResolveLocation(row.SiteID);
                if (location.LocationID != row.LocationID)
                    throw new PXException("The project location changed for warehouse {0}. Reload the order before creating Issues.", row.SiteID);
                string key = StockKey(row);
                decimal baseQty = INUnitAttribute.ConvertToBase<JEINDispachtMaterial.inventoryID>(
                    Materials.Cache, row, row.UOM, row.DispatchQty.Value, INPrecision.QUANTITY);
                totals[key] = (totals.ContainsKey(key) ? totals[key] : 0m) + baseQty;
                representatives[key] = row;
                positive.Add(row);
            }
            foreach (var total in totals)
            {
                var row = representatives[total.Key];
                decimal available = BaseAvailable(row);
                if (total.Value > available)
                    throw new PXException("Item {0}, warehouse {1}, location {2}: total dispatch quantity {3} exceeds available {4} (base units).",
                        row.InventoryID, row.SiteID, row.LocationID, total.Value, available);
            }
            return positive;
        }

        protected virtual void JEINDispachtMaterial_DispatchQty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            EnsureDispatchEditable();
            if (e.Row == null) return;
            var copy = (JEINDispachtMaterial)sender.CreateCopy(e.Row);
            copy.DispatchQty = (decimal?)e.NewValue;
            ValidateQuantities(copy);
        }

        protected virtual void JEINDispachtMaterial_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            EnsureDispatchEditable();
            if (!buildingMaterials) throw new PXException("Add materials through Add Documents.");
        }

        protected virtual void JEINDispachtMaterial_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
        {
            EnsureDispatchEditable();
            var old = (JEINDispachtMaterial)e.Row;
            var row = (JEINDispachtMaterial)e.NewRow;
            // API/import callers have the same restrictions as the Materials grid.
            foreach (string field in new[] { "DispachtLineNbr", "SourceLineNbr", "DocType", "OrderType", "OrderNbr",
                "InventoryID", "SubItemID", "Descr", "Qty", "UOM", "SiteID", "LocationID", "TaskID", "CostCodeID" })
                if (!object.Equals(sender.GetValue(old, field), sender.GetValue(row, field)))
                    throw new PXException("Only Dispatch Qty. can be edited on a material line.");
            ValidateQuantities(row);
        }

        protected virtual void JEINDispachtMaterial_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        { EnsureDispatchEditable(); }

        protected virtual void JEDispachtLine_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            EnsureDispatchEditable();
            var line = (JEDispachtLine)e.Row;
            string error = GetOrderSelectionError(new JEDispachtAddRow
                { DocType = line.DocType, OrderType = line.OrderType, OrderNbr = line.OrderNbr });
            if (error != null) throw new PXException(error);
            ReadOrderMaterials(line);
        }

        protected virtual void JEDispachtLine_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        { EnsureDispatchEditable(); }

        protected virtual void JEDispachtLine_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
        {
            EnsureDispatchEditable();
            foreach (string field in new[] { "LineNbr", "DocType", "OrderType", "OrderNbr" })
                if (!object.Equals(sender.GetValue(e.Row, field), sender.GetValue(e.NewRow, field)))
                    throw new PXException("Remove and add the source document instead of changing its reference.");
        }

        protected virtual void JEDispacht_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
        {
            var old = (JEDispacht)e.Row;
            var row = (JEDispacht)e.NewRow;
            if (old.Status != row.Status && !changingStatus)
                throw new PXException("Use the dispatch actions to change its status.");
            if (old.Status == JEDispachtStatus.Hold && !HasIssues()) return;
            foreach (string field in new[] { "ProjectID", "BranchID", "DispachtDate", "ShipDate", "CarrierID",
                "Descr", "PickupDate", "DropoffDate", "ShippingTo" })
                if (!object.Equals(sender.GetValue(old, field), sender.GetValue(row, field)))
                    throw new PXException("This dispatch is locked. Its header cannot be changed.");
        }

        protected virtual void JEDispacht_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        { EnsureDispatchEditable(); }

        public override void Persist()
        {
            if (!creatingIssues && IsSavedDispatch(Document.Current)
                && (Materials.Cache.IsDirty || Lines.Cache.IsDirty))
            {
                JEDispacht stored = PXSelectReadonly<JEDispacht,
                    Where<JEDispacht.dispachtNbr, Equal<Required<JEDispacht.dispachtNbr>>>>
                    .Select(this, Document.Current.DispachtNbr);
                if (stored?.Status != JEDispachtStatus.Hold || HasIssues())
                    throw new PXException("The saved dispatch is locked. Reload it before editing materials or documents.");
            }
            if (!creatingIssues && !undoingDispatch && !HasIssues()) ValidateQuantities();
            base.Persist();
        }

        public PXAction<JEDispacht> DispatchDocuments;
        [PXButton(CommitChanges = true, DisplayOnMainToolbar = true, Category = "Processing",
            Connotation = PX.Data.WorkflowAPI.ActionConnotation.Success)]
        [PXUIField(DisplayName = "Dispatch and Release", MapEnableRights = PXCacheRights.Update)]
        protected virtual IEnumerable dispatchDocuments(PXAdapter adapter)
        {
            var current = Document.Current;
            if (!IsSavedDispatch(current)
                || (current.Status != JEDispachtStatus.Open
                    && current.Status != JEDispachtStatus.Dispatching))
                throw new PXException("Save the dispatch and Remove Hold before creating and releasing Issues.");

            if (Document.Ask("Confirm Inventory Release",
                "This will create and release the Inventory Issues. Releasing them will move inventory and affect the project's actual cost. Are you sure you want to continue?",
                MessageButtons.YesNo) != WebDialogResult.Yes)
                return adapter.Get();

            if (current.Status == JEDispachtStatus.Dispatching)
            {
                if (!HasIssues())
                    throw new PXException("No linked Issues were found to release.");
                QueueIssueRelease(current.DispachtNbr);
                return adapter.Get();
            }

            if (HasIssues()) throw new PXException("Issues already exist for this dispatch. Use the Issues tab.");
            Save.Press();
            var rows = ValidateQuantities();
            if (rows.Count == 0) throw new PXException("Enter at least one positive Dispatch Qty. before creating Issues.");
            // One transaction for every warehouse and link: partial batches cannot be committed.
            creatingIssues = true;
            try
            {
                using (var scope = new PXTransactionScope())
                {
                    var sites = new Dictionary<int, List<JEINDispachtMaterial>>();
                    foreach (var row in rows)
                    {
                        if (!sites.ContainsKey(row.SiteID.Value)) sites[row.SiteID.Value] = new List<JEINDispachtMaterial>();
                        sites[row.SiteID.Value].Add(row);
                    }
                    foreach (var site in sites)
                    {
                        var issue = PXGraph.CreateInstance<INIssueEntry>();
                        var header = issue.issue.Insert(new INRegister
                        {
                            DocType = INDocType.Issue, BranchID = Document.Current.BranchID,
                            SiteID = site.Key, TranDate = Document.Current.DispachtDate,
                            TranDesc = "Dispatch " + Document.Current.DispachtNbr, Hold = true
                        });
                        foreach (var material in site.Value)
                        {
                            var tran = issue.transactions.Insert(new INTran { TranType = INTranType.Issue });
                            var cache = issue.transactions.Cache;
                            cache.SetValueExt<INTran.inventoryID>(tran, material.InventoryID);
                            cache.SetValueExt<INTran.subItemID>(tran, material.SubItemID);
                            cache.SetValueExt<INTran.siteID>(tran, material.SiteID);
                            cache.SetValueExt<INTran.locationID>(tran, material.LocationID);
                            cache.SetValueExt<INTran.uOM>(tran, material.UOM);
                            cache.SetValueExt<INTran.projectID>(tran, Document.Current.ProjectID);
                            cache.SetValueExt<INTran.taskID>(tran, material.TaskID);
                            cache.SetValueExt<INTran.costCodeID>(tran, material.CostCodeID);
                            cache.SetValueExt<INTran.reasonCode>(tran, IssueReason);
                            cache.SetValueExt<INTran.qty>(tran, material.DispatchQty);
                            tran.TranDesc = material.Descr;
                            issue.transactions.Update(tran);
                        }
                        issue.Save.Press();
                        // The unique (company, dispatch, warehouse) key also guards concurrent clicks.
                        Issues.Cache.AllowInsert = true;
                        Issues.Insert(new JEDispachtIssue
                        {
                            DispachtNbr = Document.Current.DispachtNbr, SiteID = site.Key,
                            DocType = issue.issue.Current.DocType, RefNbr = issue.issue.Current.RefNbr,
                            ReasonCode = IssueReason
                        });
                    }
                    changingStatus = true;
                    var dispatch = (JEDispacht)Document.Cache.CreateCopy(Document.Current);
                    dispatch.Status = JEDispachtStatus.Dispatching;
                    Document.Update(dispatch);
                    Save.Press();
                    scope.Complete();
                }
            }
            catch
            {
                // Discard rolled-back link cache state before allowing a retry.
                Clear();
                throw;
            }
            finally { creatingIssues = false; changingStatus = false; Issues.Cache.AllowInsert = false; }
            QueueIssueRelease(Document.Current.DispachtNbr);
            Issues.View.RequestRefresh();
            Document.View.RequestRefresh();
            return adapter.Get();
        }

        private void QueueIssueRelease(string dispachtNbr)
        {
            PXLongOperation.StartOperation(this, delegate
            {
                ReleaseLinkedIssues(dispachtNbr);
            });
        }

        private static void ReleaseLinkedIssues(string dispachtNbr)
        {
            var graph = PXGraph.CreateInstance<JEDispachtEntry>();
            var documents = new List<INRegister>();
            foreach (JEDispachtIssue link in PXSelectReadonly<JEDispachtIssue,
                Where<JEDispachtIssue.dispachtNbr,
                    Equal<Required<JEDispachtIssue.dispachtNbr>>>>
                .Select(graph, dispachtNbr))
            {
                INRegister issue = PXSelect<INRegister,
                    Where<INRegister.docType, Equal<Required<INRegister.docType>>,
                        And<INRegister.refNbr, Equal<Required<INRegister.refNbr>>>>>
                    .Select(graph, link.DocType, link.RefNbr);
                if (issue?.RefNbr == null || link.DocType != INDocType.Issue)
                    throw new PXException("Linked Issue {0} no longer exists; release was stopped.", link.RefNbr);
                if (issue.Released == true) continue;

                var issueGraph = PXGraph.CreateInstance<INIssueEntry>();
                issueGraph.issue.Current = PXSelect<INRegister,
                    Where<INRegister.docType, Equal<Required<INRegister.docType>>,
                        And<INRegister.refNbr, Equal<Required<INRegister.refNbr>>>>>
                    .Select(issueGraph, link.DocType, link.RefNbr);
                if (issueGraph.issue.Current == null)
                    throw new PXException("Linked Issue {0} no longer exists; release was stopped.", link.RefNbr);

                if (issueGraph.issue.Current.Hold == true)
                {
                    issueGraph.issue.Cache.SetValueExt<INRegister.hold>(issueGraph.issue.Current, false);
                    issueGraph.issue.Update(issueGraph.issue.Current);
                    issueGraph.Save.Press();
                }
                documents.Add(issueGraph.issue.Current);
            }

            if (documents.Count > 0)
                INDocumentRelease.ReleaseDoc(documents, false);
        }

        private IEnumerable UndoDispatchIssues(PXAdapter adapter)
        {
            var row = Document.Current;
            if (!IsSavedDispatch(row) || row.Status != JEDispachtStatus.Dispatching)
                throw new PXException("Only a saved dispatch in Dispatching status can be undone.");
            if (!HasIssues())
                throw new PXException("No linked Issues were found for this dispatch.");

            Save.Press();
            string number = Document.Current.DispachtNbr;
            try
            {
                undoingDispatch = true;
                using (var scope = new PXTransactionScope())
                {
                    bool locked = PXDatabase.Update<JEDispacht>(
                        new PXDataFieldAssign<JEDispacht.status>(JEDispachtStatus.Dispatching),
                        new PXDataFieldRestrict<JEDispacht.dispachtNbr>(number),
                        new PXDataFieldRestrict<JEDispacht.status>(JEDispachtStatus.Dispatching));
                    if (!locked)
                        throw new PXException("The dispatch status changed. Refresh it and try again.");

                    var links = new List<JEDispachtIssue>();
                    foreach (JEDispachtIssue link in PXSelectReadonly<JEDispachtIssue,
                        Where<JEDispachtIssue.dispachtNbr,
                            Equal<Required<JEDispachtIssue.dispachtNbr>>>>
                        .Select(this, number))
                    {
                        INRegister issue = PXSelectReadonly<INRegister,
                            Where<INRegister.docType, Equal<Required<INRegister.docType>>,
                                And<INRegister.refNbr, Equal<Required<INRegister.refNbr>>>>>
                            .Select(this, link.DocType, link.RefNbr);
                        if (issue?.RefNbr == null)
                            throw new PXException("Linked Issue {0} no longer exists; the dispatch was not changed.", link.RefNbr);
                        if (link.DocType != INDocType.Issue || issue.Released == true || issue.Hold != true)
                            throw new PXException(
                                "Issue {0} must still be an unreleased Issue on Hold. The dispatch was not changed.",
                                link.RefNbr);
                        links.Add(link);
                    }
                    if (links.Count == 0)
                        throw new PXException("No linked Issues were found for this dispatch.");

                    Issues.Cache.AllowDelete = true;
                    foreach (JEDispachtIssue link in links)
                    {
                        var issueGraph = PXGraph.CreateInstance<INIssueEntry>();
                        INRegister issue = PXSelect<INRegister,
                            Where<INRegister.docType, Equal<Required<INRegister.docType>>,
                                And<INRegister.refNbr, Equal<Required<INRegister.refNbr>>>>>
                            .Select(issueGraph, link.DocType, link.RefNbr);
                        if (issue == null || issue.Released == true || issue.Hold != true)
                            throw new PXException(
                                "Issue {0} changed while undoing. The dispatch was not changed.",
                                link.RefNbr);
                        issueGraph.issue.Current = issue;
                        issueGraph.issue.Delete(issue);
                        issueGraph.Save.Press();
                        Issues.Cache.Delete(link);
                    }
                    Issues.Cache.AllowDelete = false;

                    bool returnedToHold = PXDatabase.Update<JEDispacht>(
                        new PXDataFieldAssign<JEDispacht.status>(JEDispachtStatus.Hold),
                        new PXDataFieldAssign<JEDispacht.lastModifiedByID>(Accessinfo.UserID),
                        new PXDataFieldAssign<JEDispacht.lastModifiedDateTime>(DateTime.UtcNow),
                        new PXDataFieldRestrict<JEDispacht.dispachtNbr>(number),
                        new PXDataFieldRestrict<JEDispacht.status>(JEDispachtStatus.Dispatching));
                    if (!returnedToHold)
                        throw new PXException("The dispatch status changed while undoing. No changes were committed.");

                    Save.Press();
                    scope.Complete();
                }
            }
            catch
            {
                Clear();
                throw;
            }
            finally
            {
                undoingDispatch = false;
                changingStatus = false;
                Issues.Cache.AllowDelete = false;
            }

            Issues.View.RequestRefresh();
            Document.Cache.Clear();
            Document.Current = PXSelect<JEDispacht,
                Where<JEDispacht.dispachtNbr, Equal<Required<JEDispacht.dispachtNbr>>>>
                .Select(this, number);
            Document.View.RequestRefresh();
            CurrentDocument.View.RequestRefresh();
            return adapter.Get();
        }

        // Called by the inventory release graph while its SQL transaction is open.
        // The status update serializes releases for the same dispatch and is rolled
        // back along with inventory when any validation or persistence fails.
        public void CompleteFromReleasedIssues()
        {
            if (Document.Current?.Status != JEDispachtStatus.Dispatching) return;
            string number = Document.Current.DispachtNbr;
            bool locked = PXDatabase.Update<JEDispacht>(
                new PXDataFieldAssign<JEDispacht.status>(JEDispachtStatus.Dispatching),
                new PXDataFieldRestrict<JEDispacht.dispachtNbr>(number),
                new PXDataFieldRestrict<JEDispacht.status>(JEDispachtStatus.Dispatching));
            if (!locked) return;
            bool hasLinks = false;
            foreach (JEDispachtIssue link in PXSelectReadonly<JEDispachtIssue,
                Where<JEDispachtIssue.dispachtNbr, Equal<Required<JEDispachtIssue.dispachtNbr>>>>
                .Select(this, number))
            {
                hasLinks = true;
                INRegister issue = PXSelectReadonly<INRegister,
                    Where<INRegister.docType, Equal<Required<INRegister.docType>>,
                        And<INRegister.refNbr, Equal<Required<INRegister.refNbr>>>>>
                    .Select(this, link.DocType, link.RefNbr);
                if (issue?.Released != true) return;
            }
            if (!hasLinks) return;
            EnsureIssuesReleased();
            PXDatabase.Update<JEDispacht>(
                new PXDataFieldAssign<JEDispacht.status>(JEDispachtStatus.Completed),
                new PXDataFieldAssign<JEDispacht.lastModifiedByID>(Accessinfo.UserID),
                new PXDataFieldAssign<JEDispacht.lastModifiedDateTime>(DateTime.UtcNow),
                new PXDataFieldRestrict<JEDispacht.dispachtNbr>(number),
                new PXDataFieldRestrict<JEDispacht.status>(JEDispachtStatus.Dispatching));
        }

        private void EnsureIssuesReleased()
        {
            if (!HasIssues()) throw new PXException("Create and manually release the Issues before completing the dispatch.");
            var expected = new Dictionary<string, decimal>();
            foreach (JEINDispachtMaterial material in Materials.Select())
            {
                if ((material.DispatchQty ?? 0m) == 0m) continue;
                string key = StockKey(material);
                decimal qty = INUnitAttribute.ConvertToBase<JEINDispachtMaterial.inventoryID>(
                    Materials.Cache, material, material.UOM, material.DispatchQty.Value, INPrecision.QUANTITY);
                expected[key] = (expected.ContainsKey(key) ? expected[key] : 0m) + qty;
            }
            var actual = new Dictionary<string, decimal>();
            foreach (JEDispachtIssue link in PXSelectReadonly<JEDispachtIssue,
                Where<JEDispachtIssue.dispachtNbr, Equal<Required<JEDispachtIssue.dispachtNbr>>>>
                .Select(this, Document.Current.DispachtNbr))
            {
                INRegister issue = PXSelectReadonly<INRegister,
                    Where<INRegister.docType, Equal<Required<INRegister.docType>>,
                        And<INRegister.refNbr, Equal<Required<INRegister.refNbr>>>>>
                    .Select(this, link.DocType, link.RefNbr);
                if (issue?.RefNbr == null || issue.Released != true)
                    throw new PXException("Every linked Issue must exist and be released before completing the dispatch.");
                foreach (INTran tran in PXSelectReadonly<INTran,
                    Where<INTran.docType, Equal<Required<INTran.docType>>,
                        And<INTran.refNbr, Equal<Required<INTran.refNbr>>>>>
                    .Select(this, link.DocType, link.RefNbr))
                {
                    if (tran.ReasonCode != IssueReason || tran.ProjectID != Document.Current.ProjectID
                        || tran.TranType != INTranType.Issue)
                        throw new PXException("Issue {0} no longer matches the dispatch project/reason/type.", link.RefNbr);
                    string key = string.Format("{0}|{1}|{2}|{3}", tran.InventoryID, tran.SubItemID, tran.SiteID, tran.LocationID);
                    actual[key] = (actual.ContainsKey(key) ? actual[key] : 0m) + (tran.BaseQty ?? 0m);
                }
            }
            if (actual.Count != expected.Count)
                throw new PXException("The released Issue lines no longer match Materials.");
            foreach (var pair in expected)
                if (!actual.ContainsKey(pair.Key) || actual[pair.Key] != pair.Value)
                    throw new PXException("The released Issue quantities/locations no longer match Materials.");
        }

        public PXAction<JEDispacht> ViewIssue;
        [PXButton]
        [PXUIField(DisplayName = "View Issue", MapEnableRights = PXCacheRights.Select)]
        protected virtual IEnumerable viewIssue(PXAdapter adapter)
        {
            JEDispachtIssue link = Issues.Current;
            if (link == null) return adapter.Get();
            var graph = PXGraph.CreateInstance<INIssueEntry>();
            graph.issue.Current = PXSelect<INRegister,
                Where<INRegister.docType, Equal<Required<INRegister.docType>>,
                    And<INRegister.refNbr, Equal<Required<INRegister.refNbr>>>>>
                .Select(graph, link.DocType, link.RefNbr);
            if (graph.issue.Current == null) throw new PXException("The linked Issue no longer exists.");
            throw new PXRedirectRequiredException(graph, "Issue") { Mode = PXBaseRedirectException.WindowMode.New };
        }
    }
}
