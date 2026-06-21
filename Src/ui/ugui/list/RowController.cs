using UnityEngine;

namespace com.github.lhervier.ksp.controlfromheremod.ui.ugui.list
{
    /// <summary>
    /// Marker component of a command-module row. The row is fully static (rebuilt by the
    /// <see cref="ListController"/> whenever the vessel or its control point changes), so the controller
    /// holds no behavior; the action buttons are wired to the service directly by the builder.
    /// </summary>
    public class RowController : MonoBehaviour
    {
    }
}
