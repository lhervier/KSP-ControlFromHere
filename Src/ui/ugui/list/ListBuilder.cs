using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.shared.ugui;

namespace com.github.lhervier.ksp.controlfromheremod.ui.ugui.list
{
    /// <summary>
    /// Vertical container of the command-module rows. The rows themselves are (re)built by the
    /// <see cref="ListController"/> from the active vessel.
    /// </summary>
    public class ListBuilder : IUGUIBuilder<ListController>
    {
        public ListController Build()
        {
            var go = new GameObject("ControlFromHere.List", typeof(RectTransform));

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = 0f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            return go.AddComponent<ListController>();
        }
    }
}
