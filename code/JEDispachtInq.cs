using PX.Data;
using PX.Data.BQL.Fluent;

namespace PX.Objects.JE
{
    /// <summary>Provides a read-only list of dispatch records for inquiry.</summary>
    /// <remarks>Autor: Jose Vivanco; GitHub: josvmora; Fecha: Octubre 2026.</remarks>
    public class JEDispachtInq : PXGraph<JEDispachtInq>
    {
        public PXCancel<JEDispacht> Cancel;

        [PXFilterable]
        public SelectFrom<JEDispacht>
            .OrderBy<JEDispacht.dispachtDate.Desc, JEDispacht.dispachtNbr.Desc>
            .View.ReadOnly Results;
    }
}
