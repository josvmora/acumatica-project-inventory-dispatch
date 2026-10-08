// DISPACHT FINAL - VERSION V2 - 2026-10-07 - NUEVO: CIERRA EL DESPACHO AUTOMATICAMENTE AL LIBERAR EL ULTIMO ISSUE.
using PX.Data;
using PX.Objects.IN;
using PX.Objects.IN.InventoryRelease;

namespace PX.Objects.JE
{
    // Runs for standard inventory releases, including release from processing screens.
    public class JEDispachtINReleaseProcessExt : PXGraphExtension<INReleaseProcess>
    {
        public static bool IsActive() { return true; }

        protected virtual void INRegister_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            var issue = e.Row as INRegister;
            if (e.TranStatus != PXTranStatus.Open || issue?.Released != true
                || issue.DocType != INDocType.Issue
                || (e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;

            foreach (JEDispachtIssue link in PXSelectReadonly<JEDispachtIssue,
                Where<JEDispachtIssue.docType, Equal<Required<JEDispachtIssue.docType>>,
                    And<JEDispachtIssue.refNbr, Equal<Required<JEDispachtIssue.refNbr>>>>>
                .Select(Base, issue.DocType, issue.RefNbr))
            {
                var dispatch = PXGraph.CreateInstance<JEDispachtEntry>();
                dispatch.Document.Current = PXSelect<JEDispacht,
                    Where<JEDispacht.dispachtNbr, Equal<Required<JEDispacht.dispachtNbr>>>>
                    .Select(dispatch, link.DispachtNbr);
                dispatch.CompleteFromReleasedIssues();
            }
        }
    }
}