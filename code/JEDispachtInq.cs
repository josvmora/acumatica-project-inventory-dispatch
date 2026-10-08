using PX.Data;
using PX.Data.BQL.Fluent;

namespace PX.Objects.JE
{
    public class JEDispachtInq : PXGraph<JEDispachtInq>
    {
        public PXCancel<JEDispacht> Cancel;

        [PXFilterable]
        public SelectFrom<JEDispacht>
            .OrderBy<JEDispacht.dispachtDate.Desc, JEDispacht.dispachtNbr.Desc>
            .View.ReadOnly Results;
    }
}