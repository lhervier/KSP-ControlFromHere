using UnityEngine;

namespace com.github.lhervier.ksp.controlfromheremod.ui.ugui
{
    /// <summary>
    /// Marker controller for the window content container: a vertical stack of the always-visible circuit
    /// breaker banner (fixed height) over the scrollable command-module list (fills the rest). It holds no
    /// behavior of its own — the banner and the list own theirs — it only exists so the shared PopupBuilder
    /// can mount the container as the window's content.
    /// </summary>
    public class ContentController : MonoBehaviour
    {
    }
}
