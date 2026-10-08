// DISPACHT FINAL - VERSION V2 - 2026-10-07 - BOTON DESPACHAR EN OPEN; ELIMINA COMPLETE MANUAL.
using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.SO;
using PX.Objects.PO;
using PX.Objects.PM;
using PX.Objects.IN;

namespace PX.Objects.JE
{
    public partial class JEDispachtEntry : PXGraph<JEDispachtEntry, JEDispacht>
    {
        public PXSelect<JEDispacht> Document;

        // Bind the tab separately from the primary form so it follows the
        // selected header instead of participating in primary-key navigation.
        public PXSelect<JEDispacht,
            Where<JEDispacht.dispachtNbr,
                Equal<Current<JEDispacht.dispachtNbr>>>> CurrentDocument;

        [PXFilterable]
        public SelectFrom<JEDispachtLine> 
            .Where<JEDispachtLine.dispachtNbr.IsEqual<JEDispacht.dispachtNbr.FromCurrent>>
            .OrderBy<JEDispachtLine.lineNbr.Asc>
            .View Lines;

        public PXFilter<JEDispachtAddFilter> AddFilter;

        [PXFilterable]
        public PXSelect<JEINDispachtMaterial,
            Where<JEINDispachtMaterial.dispachtNbr,
                Equal<Current<JEDispacht.dispachtNbr>>>> Materials;

        [PXFilterable]
        public PXSelectJoin<JEDispachtIssue,
            LeftJoin<INRegister, On<INRegister.docType, Equal<JEDispachtIssue.docType>,
                And<INRegister.refNbr, Equal<JEDispachtIssue.refNbr>>>>,
            Where<JEDispachtIssue.dispachtNbr, Equal<Current<JEDispacht.dispachtNbr>>>> Issues;

        public PXSelect<JEDispachtAddRow,
            Where<JEDispachtAddRow.userID, Equal<Current<AccessInfo.userID>>>> AddRows;

        protected virtual void JEDispacht_ShippingTo_FieldDefaulting(
            PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var row = e.Row as JEDispacht;
            e.NewValue = GetProjectShippingAddress(row?.ProjectID);
        }

        protected virtual void JEDispacht_ProjectID_FieldVerifying(
            PXCache sender, PXFieldVerifyingEventArgs e)
        {
            var row = e.Row as JEDispacht;
            if (row == null || object.Equals(row.ProjectID, e.NewValue))
                return;

            // The view merges saved lines with pending additions and deletions.
            foreach (JEDispachtLine line in Lines.Select())
            {
                PXEntryStatus status = Lines.Cache.GetStatus(line);
                if (status == PXEntryStatus.Deleted || status == PXEntryStatus.InsertedDeleted)
                    continue;

                // On validation failure the selector displays NewValue. Return
                // the original substitute key, not the rejected internal ID.
                PMProject previousProject = PXSelectReadonly<PMProject,
                    Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>
                    .Select(this, row.ProjectID);
                e.NewValue = previousProject?.ContractCD;
                throw new PXSetPropertyException(
                    "Remove all documents from Details before changing the project.",
                    PXErrorLevel.Error);
            }
        }

        protected virtual void JEDispacht_ProjectID_FieldUpdated(
            PXCache sender, PXFieldUpdatedEventArgs e)
        {
            // Copy a snapshot into this document; never edit the project's address.
            sender.SetDefaultExt<JEDispacht.shippingTo>(e.Row);
        }

        private string GetProjectShippingAddress(int? projectID)
        {
            if (projectID == null)
                return null;

            PMProject project = PXSelect<PMProject,
                Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>
                .Select(this, projectID);
            if (project?.SiteAddressID == null)
                return null;

            PMSiteAddress address = PXSelect<PMSiteAddress,
                Where<PMSiteAddress.addressID, Equal<Required<PMSiteAddress.addressID>>>>
                .Select(this, project.SiteAddressID);
            if (address == null)
                return null;

            var parts = new List<string>();
            foreach (string part in new[] { address.AddressLine1, address.AddressLine2,
                address.City, address.State, address.PostalCode, address.CountryID })
            {
                if (!string.IsNullOrWhiteSpace(part))
                    parts.Add(part.Trim());
            }
            return parts.Count == 0 ? null : string.Join(", ", parts);
        }

        protected virtual void JEDispacht_ProjectName_FieldSelecting(
            PXCache sender, PXFieldSelectingEventArgs e)
        {
            var row = e.Row as JEDispacht;
            e.ReturnValue = null;
            if (row?.ProjectID == null)
                return;

            PMProject project = PXSelect<PMProject,
                Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>
                .Select(this, row.ProjectID);
            e.ReturnValue = project?.Description;
        }

        protected virtual void JEDispachtLine_DisplayLineNbr_FieldSelecting(
            PXCache sender, PXFieldSelectingEventArgs e)
        {
            var row = e.Row as JEDispachtLine;
            if (row?.LineNbr == null)
                return;

            // Select merges saved rows with pending inserts/deletes. Use a separate
            // view so calculating the display field does not invoke the grid view.
            int position = 1;
            foreach (JEDispachtLine line in PXSelect<JEDispachtLine,
                Where<JEDispachtLine.dispachtNbr,
                    Equal<Current<JEDispacht.dispachtNbr>>>>.Select(this))
            {
                var status = sender.GetStatus(line);
                if (status != PXEntryStatus.Deleted && status != PXEntryStatus.InsertedDeleted
                    && line.LineNbr < row.LineNbr)
                    position++;
            }

            e.ReturnValue = position;
        }

        protected virtual void JEDispachtLine_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            LoadOrderMaterials((JEDispachtLine)e.Row);
            Lines.View.RequestRefresh();
            Materials.View.RequestRefresh();
        }

        protected virtual void JEDispachtLine_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            var deleted = (JEDispachtLine)e.Row;
            foreach (JEINDispachtMaterial material in Materials.Select())
                if (material.DispachtLineNbr == deleted.LineNbr)
                    Materials.Delete(material);
            Lines.View.RequestRefresh();
            Materials.View.RequestRefresh();
        }

        protected virtual void JEDispacht_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            JEDispacht document = (JEDispacht)e.Row;
            if (document == null || e.Operation == PXDBOperation.Delete || !string.IsNullOrEmpty(document.DispachtNbr))
                return;

            document.DispachtNbr = GetNextDispachtNbr();
            foreach (JEDispachtLine line in Lines.Cache.Cached)
            {
                if (line.DispachtNbr == null)
                {
                    line.DispachtNbr = document.DispachtNbr;
                    Lines.Cache.Update(line);
                }
            }
        }

        private string GetNextDispachtNbr()
        {
            JEDispacht maxRec = PXSelectGroupBy<JEDispacht, Aggregate<Max<JEDispacht.dispachtNbr>>>.Select(this);
            int nextNbr = 1;
            if (maxRec?.DispachtNbr != null && int.TryParse(maxRec.DispachtNbr.Replace("DSP", ""), out int lastNbr))
                nextNbr = lastNbr + 1;

            return "DSP" + nextNbr.ToString("D6");
        }

        protected virtual void LoadAddRows()
        {
            JEDispachtAddFilter filter = AddFilter.Current;
            int? headerProjectID = Document.Current?.ProjectID;
            string orderNbrFilter = filter?.OrderNbr;
            var existingLines = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (JEDispachtLine line in PXSelect<JEDispachtLine>.Select(this))
            {
                existingLines.Add($"{line.DocType}|{line.OrderType}|{line.OrderNbr}");
            }

            foreach (JEDispachtLine line in Lines.Select())
                existingLines.Add($"{line.DocType}|{line.OrderType}|{line.OrderNbr}");

            foreach (JEDispachtAddRow oldRow in AddRows.Select())
                AddRows.Delete(oldRow);

            if (filter?.DocType == JEDispachtDocType.SO)
            {
                foreach (SOOrder order in PXSelect<SOOrder>.Select(this))
                {
                    if (order.Cancelled == true || order.OrderType != "TR")
                        continue;

                    if (headerProjectID == null || order.ProjectID != headerProjectID)
                        continue;

                    if (!string.IsNullOrEmpty(orderNbrFilter) && (order.OrderNbr?.IndexOf(orderNbrFilter, StringComparison.OrdinalIgnoreCase) ?? -1) < 0)
                        continue;

                    if (existingLines.Contains($"{JEDispachtDocType.SO}|{order.OrderType}|{order.OrderNbr}"))
                        continue;

                    AddRows.Insert(new JEDispachtAddRow
                    {
                        UserID = Accessinfo.UserID,
                        Selected = false,
                        DocType = JEDispachtDocType.SO,
                        OrderType = order.OrderType,
                        OrderNbr = order.OrderNbr,
                        ProjectID = order.ProjectID,
                        Descr = order.OrderDesc
                    });
                }
            }
            else
            {
                foreach (POOrder order in PXSelect<POOrder>.Select(this))
                {
                    if (order.Hold == true || order.OrderType != POOrderType.RegularOrder)
                        continue;

                    if (headerProjectID == null || order.ProjectID != headerProjectID)
                        continue;

                    if (!string.IsNullOrEmpty(orderNbrFilter) && (order.OrderNbr?.IndexOf(orderNbrFilter, StringComparison.OrdinalIgnoreCase) ?? -1) < 0)
                        continue;

                    if (existingLines.Contains($"{JEDispachtDocType.PO}|{order.OrderType}|{order.OrderNbr}"))
                        continue;

                    AddRows.Insert(new JEDispachtAddRow
                    {
                        UserID = Accessinfo.UserID,
                        Selected = false,
                        DocType = JEDispachtDocType.PO,
                        OrderType = order.OrderType,
                        OrderNbr = order.OrderNbr,
                        ProjectID = order.ProjectID,
                        Descr = order.OrderDesc
                    });
                }
            }

        }

        private string GetOrderSelectionError(JEDispachtAddRow row)
        {
            bool completed = false;
            if (row.DocType == JEDispachtDocType.PO)
            {
                POOrder order = PXSelectReadonly<POOrder,
                    Where<POOrder.orderType, Equal<Required<POOrder.orderType>>,
                        And<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>>>>
                    .Select(this, row.OrderType, row.OrderNbr);
                completed = order != null && order.Status == POOrderStatus.Completed
                    && order.OrderType == POOrderType.RegularOrder && order.ProjectID == Document.Current?.ProjectID;
            }
            else if (row.DocType == JEDispachtDocType.SO)
            {
                SOOrder order = PXSelectReadonly<SOOrder,
                    Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
                        And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>
                    .Select(this, row.OrderType, row.OrderNbr);
                completed = order != null && order.Status == SOOrderStatus.Completed
                    && order.OrderType == "TR" && order.ProjectID == Document.Current?.ProjectID;
            }

            return completed ? null : string.Format(
                "Order {0} {1} must be Completed, belong to this project, and be Normal PO or TR SO.",
                row.DocType, row.OrderNbr);
        }

        protected virtual void JEDispachtAddRow_Selected_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = e.Row as JEDispachtAddRow;
            if (row == null || row.Selected != true)
                return;

            string error = GetOrderSelectionError(row);
            if (error != null)
            {
                // Reset the accepted value so the callback returns an unchecked box.
                sender.SetValue<JEDispachtAddRow.selected>(row, false);
                sender.RaiseExceptionHandling<JEDispachtAddRow.selected>(row, false,
                    new PXSetPropertyException(error, PXErrorLevel.Error));
                AddRows.View.RequestRefresh();
                return;
            }

            sender.RaiseExceptionHandling<JEDispachtAddRow.selected>(row, true, null);
        }

        protected virtual void JEDispachtAddFilter_DocType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            AddFilter.Current.OrderNbr = null;
            LoadAddRows();
            AddRows.View.RequestRefresh();
        }

        protected virtual void JEDispachtAddFilter_OrderNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            LoadAddRows();
            AddRows.View.RequestRefresh();
        }

        #region Actions

        private bool IsSavedDispatch(JEDispacht row)
        {
            if (row == null || string.IsNullOrWhiteSpace(row.DispachtNbr))
                return false;

            PXEntryStatus status = Document.Cache.GetStatus(row);
            return status != PXEntryStatus.Inserted
                && status != PXEntryStatus.InsertedDeleted
                && status != PXEntryStatus.Deleted;
        }

        protected virtual void JEDispacht_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = e.Row as JEDispacht;
            bool onHold = row?.Status == JEDispachtStatus.Hold;
            bool open = row?.Status == JEDispachtStatus.Open;
            bool hasIssues = HasIssues();
            bool editable = onHold && !hasIssues;
            PXUIFieldAttribute.SetEnabled<JEDispacht.dispachtDate>(sender, row, editable);
            PXUIFieldAttribute.SetEnabled<JEDispacht.shipDate>(sender, row, editable);
            PXUIFieldAttribute.SetEnabled<JEDispacht.branchID>(sender, row, editable);
            PXUIFieldAttribute.SetEnabled<JEDispacht.projectID>(sender, row, editable);
            PXUIFieldAttribute.SetEnabled<JEDispacht.carrierID>(sender, row, editable);
            PXUIFieldAttribute.SetEnabled<JEDispacht.descr>(sender, row, editable);
            PXUIFieldAttribute.SetEnabled<JEDispacht.pickupDate>(sender, row, editable);
            PXUIFieldAttribute.SetEnabled<JEDispacht.dropoffDate>(sender, row, editable);
            PXUIFieldAttribute.SetEnabled<JEDispacht.shippingTo>(sender, row, editable);
            // Keep header updates available for the status actions and navigation.
            Document.Cache.AllowDelete = editable;
            Lines.Cache.AllowInsert = editable;
            Lines.Cache.AllowUpdate = editable;
            Lines.Cache.AllowDelete = editable;
            AddDocument.SetEnabled(row != null && editable);
            AddSelectedDocuments.SetEnabled(row != null && editable);
            bool canHold = open || row?.Status == JEDispachtStatus.Completed
                || row?.Status == JEDispachtStatus.Cancelled;
            bool saved = IsSavedDispatch(row);
            // Keep menu entries visible; only enabled toolbar actions appear there.
            RemoveHold.SetEnabled(saved && onHold);
            RemoveHold.SetVisible(true);
            PutOnHold.SetEnabled(saved && canHold && !hasIssues);
            PutOnHold.SetVisible(true);
            UndoDispatch.SetEnabled(CanUndoDispatch(row));
            UndoDispatch.SetVisible(true);
            CancelDispatch.SetEnabled(saved && open && !hasIssues);
            CancelDispatch.SetVisible(true);
            PrintDispatch.SetEnabled(saved);
            PrintDispatch.SetVisible(true);
            Materials.Cache.AllowInsert = editable;
            Materials.Cache.AllowDelete = editable;
            Materials.Cache.AllowUpdate = editable;
            PXUIFieldAttribute.SetEnabled<JEINDispachtMaterial.dispatchQty>(Materials.Cache, null, editable);
            DispatchDocuments.SetEnabled(saved && (open || row?.Status == JEDispachtStatus.Dispatching));
            LoadMaterials.SetEnabled(saved && editable);
            Issues.Cache.AllowInsert = false;
            Issues.Cache.AllowUpdate = false;
            Issues.Cache.AllowDelete = false;
        }

        public PXAction<JEDispacht> RemoveHold;
        [PXButton(CommitChanges = true, DisplayOnMainToolbar = true, Category = "Processing")]
        [PXUIField(DisplayName = "Remove Hold", MapEnableRights = PXCacheRights.Update,
            MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable removeHold(PXAdapter adapter)
        {
            return ChangeDispatchStatus(adapter, JEDispachtStatus.Open, JEDispachtStatus.Hold);
        }

        public PXAction<JEDispacht> PutOnHold;
        [PXButton(CommitChanges = true, DisplayOnMainToolbar = false, Category = "Processing")]
        [PXUIField(DisplayName = "Put on Hold", MapEnableRights = PXCacheRights.Update,
            MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable putOnHold(PXAdapter adapter)
        {
            return ChangeDispatchStatus(adapter, JEDispachtStatus.Hold,
                JEDispachtStatus.Open, JEDispachtStatus.Completed, JEDispachtStatus.Cancelled);
        }

        public PXAction<JEDispacht> UndoDispatch;
        [PXButton(CommitChanges = true, DisplayOnMainToolbar = false, Category = "Processing",
            Connotation = PX.Data.WorkflowAPI.ActionConnotation.Danger)]
        [PXUIField(DisplayName = "Undo Dispatch", MapEnableRights = PXCacheRights.Update,
            MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable undoDispatch(PXAdapter adapter)
        {
            return UndoDispatchIssues(adapter);
        }

        public PXAction<JEDispacht> CancelDispatch;
        [PXButton(CommitChanges = true, DisplayOnMainToolbar = true, Category = "Processing",
            Connotation = PX.Data.WorkflowAPI.ActionConnotation.Danger)]
        [PXUIField(DisplayName = "Cancel Dispatch", MapEnableRights = PXCacheRights.Update,
            MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable cancelDispatch(PXAdapter adapter)
        {
            return ChangeDispatchStatus(adapter, JEDispachtStatus.Cancelled,
                JEDispachtStatus.Open);
        }

        private IEnumerable ChangeDispatchStatus(PXAdapter adapter, string target, params string[] allowed)
        {
            var row = Document.Current;
            if (row == null)
                return adapter.Get();
            if (Array.IndexOf(allowed, row.Status) < 0)
                throw new PXException("This action is not available in the current dispatch status.");
            if (!IsSavedDispatch(row))
                throw new PXException("Save the dispatch before changing its status.");

            if (HasIssues() && target != JEDispachtStatus.Completed)
                throw new PXException("Issues already exist. This dispatch cannot be reopened or cancelled.");
            // Save pending edits to the existing dispatch before changing state.
            Save.Press();
            row = Document.Current;
            string previousStatus = row.Status;
            try
            {
                changingStatus = true;
                var updated = (JEDispacht)Document.Cache.CreateCopy(row);
                updated.Status = target;
                Document.Update(updated);
                Save.Press();
            }
            catch
            {
                Document.Cache.SetValue<JEDispacht.status>(Document.Current, previousStatus);
                throw;
            }
            finally { changingStatus = false; }
            Document.View.RequestRefresh();
            CurrentDocument.View.RequestRefresh();
            return new[] { Document.Current };
        }

        public PXAction<JEDispacht> PrintDispatch;
        [PXButton(CommitChanges = true, DisplayOnMainToolbar = false, Category = "Printing and Emailing")]
        [PXUIField(DisplayName = "Print Dispatch",
            MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable printDispatch(PXAdapter adapter)
        {
            if (Document.Current == null)
                return adapter.Get();

            if (!IsSavedDispatch(Document.Current))
                throw new PXException("Save the dispatch before printing.");

            // Persist pending edits first: reports read saved data.
            Save.Press();
            string dispachtNbr = Document.Current?.DispachtNbr;
            if (string.IsNullOrWhiteSpace(dispachtNbr))
                throw new PXException("Save the dispatch before printing.");

            var parameters = new Dictionary<string, string>
            {
                ["DispatchNbr"] = dispachtNbr
            };
            // Use the report screen ID; JE401003 is the dispatch entry screen.
            // Report screen JE401005 is mapped to je401003.rpx in Acumatica.
            throw new PXReportRequiredException(parameters, "JE401005", "Dispatch")
            {
                Mode = PXBaseRedirectException.WindowMode.New
            };
        }

        private void EnsureDispatchEditable()
        {
            if (Document.Current?.Status != JEDispachtStatus.Hold || HasIssues())
                throw new PXException("Only an On Hold dispatch without Issues can be edited.");
        }

        public PXAction<JEDispacht> AddDocument;
        [PXButton(CommitChanges = true, DisplayOnMainToolbar = false)]
        [PXUIField(DisplayName = "Add Documents")]
        protected virtual IEnumerable addDocument(PXAdapter adapter)
        {
            EnsureDispatchEditable();
            if (AddFilter.AskExt((graph, view) =>
            {
                LoadAddRows();
                AddRows.View.RequestRefresh();
            }) == WebDialogResult.OK)
            {
                AddSelectedRows();
            }
            return adapter.Get();
        }

        public PXAction<JEDispacht> AddSelectedDocuments;
        [PXButton(CommitChanges = true, DisplayOnMainToolbar = false)]
        [PXUIField(DisplayName = "Add")]
        protected virtual IEnumerable addSelectedDocuments(PXAdapter adapter)
        {
            AddSelectedRows();
            return adapter.Get();
        }

        protected virtual void AddSelectedRows()
        {
            EnsureDispatchEditable();
            if (Document.Current == null)
                return;

            int nextLine = 1;
            var existingLines = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (JEDispachtLine line in Lines.Select())
            {
                existingLines.Add($"{line.DocType}|{line.OrderType}|{line.OrderNbr}");
                if (line.LineNbr >= nextLine)
                    nextLine = line.LineNbr.Value + 1;
            }

            var selectedRows = new List<JEDispachtAddRow>();
            foreach (JEDispachtAddRow row in AddRows.Select())
                if (row.Selected == true)
                    selectedRows.Add(row);

            // Recheck before inserting any lines, including selections made with Check All.
            bool invalidSelection = false;
            foreach (JEDispachtAddRow row in selectedRows)
            {
                string error = GetOrderSelectionError(row);
                if (error == null)
                    continue;

                row.Selected = false;
                AddRows.Update(row);
                AddRows.Cache.RaiseExceptionHandling<JEDispachtAddRow.selected>(row, false,
                    new PXSetPropertyException(error, PXErrorLevel.Error));
                invalidSelection = true;
            }

            if (invalidSelection)
            {
                AddRows.View.RequestRefresh();
                throw new PXException("Some orders are not finalized. Only orders with Completed status can be added.");
            }

            foreach (JEDispachtAddRow row in selectedRows)
            {
                if (!existingLines.Add($"{row.DocType}|{row.OrderType}|{row.OrderNbr}"))
                {
                    AddRows.Delete(row);
                    continue;
                }

                Lines.Insert(new JEDispachtLine
                {
                    DispachtNbr = Document.Current.DispachtNbr,
                    LineNbr = nextLine,
                    DocType = row.DocType,
                    OrderType = row.OrderType,
                    OrderNbr = row.OrderNbr,
                    Descr = row.Descr
                });

                nextLine++;
                AddRows.Delete(row);
            }

            AddRows.View.RequestRefresh();
            Lines.View.RequestRefresh();
        }

        #endregion
    }
}
