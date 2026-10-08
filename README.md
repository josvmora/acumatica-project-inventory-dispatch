# Final Dispatch — V1 (2026-10-07)

Source files for installation and validation in Acumatica. The `../Dispacht`
folder remains the stable version. This folder is a complete alternative version
with the same class and screen names: replace the files in the customization
project; do not publish both definitions at the same time.

## Behavior

- Details supports regular POs (`POOrderType.RegularOrder`) and `TR` SOs assigned
  to the same project as the header. The Completed status requirement and the
  stable version's exclusion of orders already included in another dispatch are
  preserved.
- When an order is added, its stock inventory lines are copied to the persistent
  `JEINDispachtMaterial` table. Services, charges, and non-stock items are omitted.
- PO: warehouse from `POLine.SiteID`. TR SO: warehouse from
  `SOOrder.DestinationSiteID`, never the Mainwarehouse shipping warehouse.
- The location is resolved in `INLocation` by `SiteID` and `ProjectID`. Exactly
  one active location valid for shipping (`SalesValid`) must exist. If none or
  more than one is found, the operation fails with an error; it does not pick the
  first match or a general location. `ProjectID` and `TaskID` are read through
  the cache so DAC extensions are included.
- `Qty` remains the ordered quantity for report compatibility. `DispatchQty` is
  a separate, editable quantity that defaults to zero. Warehouse, location,
  item, UOM, and source references are read-only.
- Availability is `min(INLocationStatus.QtyAvail, QtyOnHand)` by item, subitem,
  warehouse, and location. It is displayed in the line's UOM. Validation totals
  dispatch quantities in base units and checks them when editing, saving, and
  creating Issues. This is an availability check, not a reservation.
- `TaskID` is taken from the location when present; otherwise, from the source
  line. `CostCodeID` comes from the source line. Standard graph project and
  accounting validations still apply.

## Workflow

1. Create or edit the dispatch while it is On Hold, add orders, and enter
   `Dispatch Qty`.
2. Save and select **Remove Hold**.
3. **Dispatch and Release** asks for confirmation. It creates one Inventory
   Issue per warehouse for the positive quantities, using Reason Code
   `PROJECTISSUE`, then releases all Issues in the background. Releasing moves
   inventory and affects the project's actual cost. Issue creation and its links
   are saved in a transaction; the later release cannot be automatically undone
   if one or more Issues have already been released.
4. The Issues tab shows the linked issue number, warehouse, reason, status, Hold,
   Released, date, total, and creation date.
5. If an Issue requires lot/serial assignments or other data, release fails with
   the Acumatica error. Complete the required data and run **Dispatch and
   Release** again to process Issues that have not yet been released.
6. When all Issues are released, the dispatch automatically moves to Completed.
   Completion verifies that the item, subitem, warehouse, location, base-unit
   quantities, project, and Reason Code still match the dispatch.
7. To cancel a test before release, select **Undo Dispatch**. This is allowed
   only if every linked Issue exists and is still on Hold and unreleased. It
   deletes the Issues and links in a transaction and returns the dispatch to
   Hold.

Creating Issues locks the dispatch against editing, deletion, cancellation, and
reopening. Running the action again does not create another batch. The unique
company/dispatch/warehouse key prevents duplicates if two sessions run
concurrently; if creation fails, its entire transaction is rolled back. A release
failure may leave some Issues already released; run **Dispatch and Release**
again to process the remaining ones. Issues deleted manually are not recreated
automatically. Do not change or delete their business lines: completion detects
differences. This version does not support reversals or successive partial
dispatches against the same order.

## Installation

1. Run `sql/JEDispacht.sql` (stable base, repeatable), then
   `sql/JEDispachtInventory.sql` (the two new tables, repeatable).
2. Replace the existing files with those in `code/`. Also add
   `JEDispachtEntryInventory.cs` (the second part of the same graph),
   `JEDispachtIssue.cs`, and `JEDispachtINReleaseProcessExt.cs` (automatic
   completion from `INReleaseProcess`). There are nine code files. Do not keep
   the `PXProjection` definition of `JEINDispachtMaterial` alongside the new
   persistent definition.
3. Replace `screens/JE401003.aspx`. The other screen files are included unchanged
   to provide a complete copy. The screen IDs remain JE401003 and JE401004.
4. Configure Reason Code `PROJECTISSUE` for Inventory Issues, with valid accounts
   and subaccounts. The code uses this reason code; it does not create it or
   change accounting.
5. Validate and publish in a test instance running the same Acumatica version as
   the target instance.
6. Update the JE401005 / `je401003.rpx` report schema: `Qty` remains the ordered
   quantity; use `DispatchQty` for the actual dispatched quantity. The
   `DispachtNbr` filter is still required. The RPX file is not in this repository.

Orders added in this version generate materials immediately. Older dispatches do
not receive an automatic snapshot of current data. To continue an older dispatch
in this workflow, select **Put on Hold**, review its orders, and run **Load
Missing Materials**. This adds only missing rows and preserves quantities already
edited. It does not reconstruct history: it copies current source data. Do not
run it on older dispatches that must remain historical records.

## Validation Required in the Acumatica Instance

This repository does not include Acumatica DLLs or a build project. Local syntax
and structure checks do not replace compiling and publishing against the
installed version. In particular, validate the inventory DACs, the
`ProjectID`/`TaskID` extensions on `INLocation`, permissions, and accounting
configuration.

Run the local checks with `python verification/run_checks.py` (requires .NET SDK
9.0.300 installed on this machine). The SHA256 check uses `stable-hashes.json` to
verify that the stable source files have not changed.

- PO with two warehouses; TR whose source warehouse differs from its destination:
  verify that Materials and the Issue use the warehouses described above.
- Missing, inactive, invalid-for-shipping, and duplicate project locations must
  block processing. A location from another project must never be selected.
- Save and reopen a new dispatch: materials, keys, and quantities must persist.
  Removing an order while On Hold removes only its materials.
- For the same item and location on two orders, individually valid quantities
  whose total exceeds availability must fail. Repeat with different UOMs and
  subitems.
- Test zero, negative, exactly available, and excess quantities. Reduce inventory
  between editing and selecting **Dispatch and Release** to verify the
  revalidation.
- Failure at the second warehouse must not leave a partial Issue or link. Retry,
  and test two sessions creating the same dispatch: Issues must not be duplicated.
- Create Issues and verify Hold, `PROJECTISSUE`, project, task, cost code,
  warehouse, location, and quantities. Issues must remain unreleased.
- Manually release, return to the dispatch, and verify completion. Completion
  must reject an unreleased or deleted Issue, or an Issue whose quantity or
  location was changed.
- Test lot/serial-controlled items by completing assignments on the Issue; this
  version does not select lots or serial numbers automatically.

Availability may change between Issue creation and manual release. The standard
Issue is responsible for validating the inventory movement when released. This
version uses location-level balances; it does not select project-specific
inventory layers. If Project-Specific Inventory is used with separate layers,
validate that mode in the instance before using this version.

Official reference for the different availability measures:
[Inventory Allocation Details](https://github.com/Acumatica/Acumatica-AI-Resources/blob/2026R1/Documentation/UserGuide/IN_40_20_00.md).
The field shown here is the conservative availability measure described above,
not the standard field named Available for Issue.

## Workflow Update (2026-10-07)

For an installation already running the previous version, replace
`JEDispacht.cs`, `JEDispachtEntry.cs`, and `JEDispachtEntryInventory.cs`; add
`JEDispachtINReleaseProcessExt.cs` and publish. The screen keeps Issues at the
end. Status D fits in the existing Status column and does not require a schema
change. The extension must be compiled and tested against the installed version
of `INReleaseProcess`; only syntax was checked here, and no actual release was
performed.

Dispatches created with the previous workflow that have Issues but are still
Open are not migrated automatically. The new lifecycle applies when
**Dispatch and Release** is selected on documents without Issues. Review previous
test documents separately before adopting this version.
