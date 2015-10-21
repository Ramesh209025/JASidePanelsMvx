﻿using Cirrious.CrossCore.Exceptions;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Touch.Views;
using Cirrious.MvvmCross.Touch.Views.Presenters;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using JASidePanels;
using JASPMvx.Core.ViewModels;
using UIKit;

namespace JASPMvx.Ios
{
    /// <summary>
    /// This class combines code from MvxTouchViewPresenter and MvxModalSupportTouchViewPresenter
    /// along with custom code to produce a IOS Presenter to smoothly integrate the JASidePanels component
    /// </summary>
    public class JaSidePanelsMvxPresenter : MvxTouchViewPresenter
    {
        private UIViewController _currentModalViewController;

        private readonly JASidePanelController _jaSidePanelController;
        private PanelEnum _activePanel;

        private UINavigationController CentrePanelUiNavigationController()
        {
            return ((UINavigationController)_jaSidePanelController.CenterPanel);
        }

        private UINavigationController RightPanelUiNavigationController()
        {
            return ((UINavigationController)_jaSidePanelController.RightPanel);
        }

        private UINavigationController LeftPanelUiNavigationController()
        {
            return ((UINavigationController)_jaSidePanelController.LeftPanel);
        }

        private UINavigationController GetActivePanelUiNavigationController
        {
            get
            {
                switch (_activePanel)
                {
                    case PanelEnum.Center:
                        return CentrePanelUiNavigationController();
                    case PanelEnum.Left:
                        return LeftPanelUiNavigationController();
                    case PanelEnum.Right:
                        return RightPanelUiNavigationController();
                }

                return CentrePanelUiNavigationController();
            }
        }

        public override void Show(IMvxTouchView view)
        {
            Trc.Mn("IMvxTouchView");

            // Handle modal first
            // This will use our TopLevel UINavigation Controller, to present over the top of the Panels UX
            if (view is IMvxModalTouchView)
            {
                if (_currentModalViewController != null)
                    throw new MvxException("Only one modal view controller at a time supported");
                _currentModalViewController = view as UIViewController;
                PresentModalViewController(view as UIViewController, true);
                return;
            }

            // Then handle panels 
            UIViewController viewController = view as UIViewController;
            if (viewController == null)
                throw new MvxException("Passed in IMvxTouchView is not a UIViewController");

            if (MasterNavigationController == null)
            {
                ShowFirstView(viewController);
            }
            else
            {
                if (GetActivePanelUiNavigationController == null)
                {
                    // If we have cleared down our panel completely, then we will be setting a new root view
                    // this is perfect for Menu items 
                    switch (_activePanel)
                    {
                        case PanelEnum.Center:
                            _jaSidePanelController.CenterPanel = new UINavigationController(viewController);
                            break;
                        case PanelEnum.Left:
                            _jaSidePanelController.LeftPanel = new UINavigationController(viewController);
                            break;
                        case PanelEnum.Right:
                            _jaSidePanelController.RightPanel = new UINavigationController(viewController);
                            break;
                    }
                }
                else
                {
                    // Otherwise we just want to push to the designated panel 
                    GetActivePanelUiNavigationController.PushViewController(viewController, true);
                }
            }
        }

        public override void NativeModalViewControllerDisappearedOnItsOwn()
        {
            if (_currentModalViewController != null)
                MvxTrace.Error("How did a modal disappear when we didn't have one showing?");
            else
                _currentModalViewController = null;
        }

        public override void CloseModalViewController()
        {
            if (_currentModalViewController != null)
            {        
                _currentModalViewController.DismissModalViewController(true);
                _currentModalViewController = null;
            }
            else
                base.CloseModalViewController();
        }

        public JaSidePanelsMvxPresenter(UIApplicationDelegate applicationDelegate, UIWindow window) :
            base(applicationDelegate, window)
        {
            _jaSidePanelController = new JASidePanelController();
            _activePanel = PanelEnum.Center;
        }

        protected override void ShowFirstView(UIViewController viewController)
        {
            Trc.Mn();

            // Creates our top level UINavigationController as standard
            base.ShowFirstView(viewController);

            // So lets push our JaSidePanels viewController and then our first viewController in the centre panel to start things off
            // We will let our initial viewmodel load up the panels as required
            MasterNavigationController.NavigationBarHidden = true;
            MasterNavigationController.PushViewController(_jaSidePanelController, false);
            _jaSidePanelController.CenterPanel = new UINavigationController(viewController);
        }

        public override void ChangePresentation(MvxPresentationHint hint)
        {
            ProcessActivePanelPresentation(hint);
            ProcessResetRootPresentation(hint);
            ProcessPopToRootPresentation(hint);

            base.ChangePresentation(hint);
        }

        private void ProcessActivePanelPresentation(MvxPresentationHint hint)
        {
            var activePresentationHint = hint as ActivePanelPresentationHint;
            if (activePresentationHint != null)
            {
                var panelHint = activePresentationHint;

                _activePanel = panelHint.ActivePanel;

                if (panelHint.ShowPanel)
                {
                    ShowPanel(panelHint.ActivePanel);
                }
            }
        }

        private void ProcessPopToRootPresentation(MvxPresentationHint hint)
        {
            var popHint = hint as PanelPopToRootPresentationHint;
            if (popHint != null)
            {
                var panelHint = popHint;

                switch (panelHint.Panel)
                {
                    case PanelEnum.Center:
                        if (CentrePanelUiNavigationController() != null)
                            CentrePanelUiNavigationController().PopToRootViewController(false);
                        break;
                    case PanelEnum.Left:
                        if (LeftPanelUiNavigationController() != null)
                            LeftPanelUiNavigationController().PopToRootViewController(false);
                        break;
                    case PanelEnum.Right:
                        if (RightPanelUiNavigationController() != null)
                            RightPanelUiNavigationController().PopToRootViewController(false);
                        break;
                }
            }
        }

        private void ProcessResetRootPresentation(MvxPresentationHint hint)
        {
            var popHint = hint as PanelResetRootPresentationHint;
            if (popHint != null)
            {
                var panelHint = popHint;

                switch (panelHint.Panel)
                {
                    case PanelEnum.Center:
                        _jaSidePanelController.CenterPanel = null;
                        break;
                    case PanelEnum.Left:
                        _jaSidePanelController.LeftPanel = null;
                        break;
                    case PanelEnum.Right:
                        _jaSidePanelController.RightPanel = null;
                        break;
                }
            }
        }

        private void ShowPanel(PanelEnum panel)
        {
            switch (panel)
            {
                case PanelEnum.Center:
                    _jaSidePanelController.ShowCenterPanelAnimated(true);
                    break;
                case PanelEnum.Left:
                    _jaSidePanelController.ShowLeftPanelAnimated(true);
                    break;
                case PanelEnum.Right:
                    _jaSidePanelController.ShowRightPanelAnimated(true);
                    break;
            }
        }

        public override void Close(IMvxViewModel toClose)
        {
            if (_currentModalViewController != null)
            {
                IMvxTouchView mvxTouchView = _currentModalViewController as IMvxTouchView;
                if (mvxTouchView == null)
                    MvxTrace.Error("Unable to close view - modal is showing but not an IMvxTouchView");
                else if (mvxTouchView.ReflectionGetViewModel() != toClose)
                {
                    MvxTrace.Error("Unable to close view - modal is showing but is not the requested viewmodel");
                }
                else
                {
                    // ISSUE: reference to a compiler-generated method
                    _currentModalViewController.DismissViewController(true, () => { });
                    _currentModalViewController = null;
                }

                return;
            }

            // We will look across all active navigation stacks to see if we can
            // pop our MvxView associated with this MvxViewModel (saves explicitly having to specify)            
            bool modelClosed = CloseTopView(toClose, CentrePanelUiNavigationController());
            if (!modelClosed) modelClosed = CloseTopView(toClose, LeftPanelUiNavigationController());
            if (!modelClosed) modelClosed = CloseTopView(toClose, RightPanelUiNavigationController());

            if (!modelClosed)
            {
                MvxTrace.Warning("Don't know how to close this viewmodel - none of topmost views represent this viewmodel");
            }
        }

        /// <summary>
        /// See if the supplied ViewModel matches up with the MvxView at the top of the supplied UINavigationController
        /// and if so, pop that View from the stack
        /// </summary>
        private bool CloseTopView(IMvxViewModel toClose, UINavigationController uiNavigationController)
        {
            if (uiNavigationController == null)
                return false;

            IMvxTouchView mvxTouchView = uiNavigationController.TopViewController as IMvxTouchView;

            if (mvxTouchView == null)
                return false;

            if (mvxTouchView.ReflectionGetViewModel() != toClose)
            {
                return false;
            }

            uiNavigationController.PopViewController(true);

            return true;
        }
    }
}