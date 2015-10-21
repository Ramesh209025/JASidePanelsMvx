using Cirrious.MvvmCross.ViewModels;

namespace JASPMvx.Core.ViewModels
{
    public enum PanelEnum
    {
        None,
        Center,
        Left,
        Right
    }

    /// <summary>
    /// Determines which Panel the next ShowViewModel call will act upon
    /// and optionally allows you to show that panel (default) or not
    /// </summary>
    public class ActivePanelPresentationHint : MvxPresentationHint
    {
        public readonly PanelEnum ActivePanel;
        public readonly bool ShowPanel;

        public ActivePanelPresentationHint(PanelEnum activePanel, bool showPanel = true)
        {
            ActivePanel = activePanel;
            ShowPanel = showPanel;
        }
    }
}