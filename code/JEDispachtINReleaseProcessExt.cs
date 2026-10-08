// DISPACHT FINAL - VERSION V2 - 2026-10-07 - NUEVO: CIERRA EL DESPACHO AUTOMATICAMENTE AL LIBERAR EL ULTIMO ISSUE.
using PX.Data;
using PX.Objects.IN;
using PX.Objects.IN.InventoryRelease;

namespace PX.Objects.JE
{
    // Runs for standard inventory releases, including release from processing screens.
    /// <summary>Completes linked dispatches when their inventory issues are released.</summary>
    /// <remarks>Autor: Jose Vivanco; GitHub: josvmora; Fecha: Octubre 2026.</remarks>
    public class JEDispachtINReleaseProcessExt : PXGraphExtension<INReleaseProcess>
    {
        /// <summary>Indicates that this graph extension is active.</summary>
        /// <returns><see langword="true"/> to enable dispatch completion on issue release.</returns>
        public static bool IsActive() { return true; }

        /// <summary>Checks released inventory documents and advances associated dispatches.</summary>
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
