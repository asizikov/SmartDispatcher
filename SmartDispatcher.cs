// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.ComponentModel;
#if NETFX_CORE
using Windows.UI.Xaml;
using System.ServiceModel;
using Windows.UI.Core;
using WinRTWin = Windows;
#endif

namespace System.Windows.Threading
{
    /// <summary>
    /// A smart dispatcher system for routing actions to the user interface
    /// thread.
    /// </summary>
    public static class SmartDispatcher
    {
        /// <summary>
        /// A single Dispatcher instance to marshall actions to the user
        /// interface thread.
        /// </summary>
#if WINDOWS_PHONE
                private static Dispatcher _instance;
#endif
#if NETFX_CORE
        private static CoreDispatcher _instance;
#endif


        /// <summary>
        /// Backing field for a value indicating whether this is a design-time
        /// environment.
        /// </summary>
        private static bool? _designer;

        /// <summary>
        /// Requires an instance and attempts to find a Dispatcher if one has
        /// not yet been set.
        /// </summary>
        private static void RequireInstance()
        {
            if (_designer == null)
            {
#if WINDOWS_PHONE
               _designer = DesignerProperties.IsInDesignTool;
#endif
#if NETFX_CORE
                _designer = WinRTWin.ApplicationModel.DesignMode.DesignModeEnabled;
#endif

            }

            // Design-time is more of a no-op, won't be able to resolve the
            // dispatcher if it isn't already set in these situations.
            if (_designer == true)
            {
                return;
            }

            // Attempt to use the RootVisual of the plugin to retrieve a
            // dispatcher instance. This call will only succeed if the current
            // thread is the UI thread.
            try
            {
                //_instance = Application.Current.RootVisual.Dispatcher;
#if WINDOWS_PHONE
                _instance = Deployment.Current.Dispatcher;
#endif
#if NETFX_CORE
                _instance = Window.Current.CoreWindow.Dispatcher;
#endif

            }
            catch (Exception e)
            {
                throw new InvalidOperationException("The first time SmartDispatcher is used must be from a user interface thread. Consider having the application call Initialize, with or without an instance.", e);
            }

            if (_instance == null)
            {
                throw new InvalidOperationException("Unable to find a suitable Dispatcher instance.");
            }
        }

        /// <summary>
        /// Initializes the SmartDispatcher system, attempting to use the
        /// RootVisual of the plugin to retrieve a Dispatcher instance.
        /// </summary>
        public static void Initialize()
        {
            if (_instance == null)
            {
                RequireInstance();
            }
        }

        /// <summary>
        /// Initializes the SmartDispatcher system with the dispatcher
        /// instance.
        /// </summary>
        /// <param name="dispatcher">The dispatcher instance.</param>
#if WINDOWS_PHONE
         public static void Initialize(Dispatcher dispatcher)
#endif
#if NETFX_CORE
        public static void Initialize(CoreDispatcher dispatcher)
#endif       
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException("dispatcher");
            }

            _instance = dispatcher;

            if (_designer == null)
            {
#if WINDOWS_PHONE
                _designer = DesignerProperties.IsInDesignTool;
#endif
#if NETFX_CORE
                _designer = WinRTWin.ApplicationModel.DesignMode.DesignModeEnabled;
#endif
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool CheckAccess()
        {
            if (_instance == null)
            {
                RequireInstance();
            }
#if WINDOWS_PHONE
            return _instance.CheckAccess();
#endif
#if NETFX_CORE
            return _instance.HasThreadAccess;
#endif
        }

        /// <summary>
        /// Executes the specified delegate asynchronously on the user interface
        /// thread. If the current thread is the user interface thread, the
        /// dispatcher if not used and the operation happens immediately.
        /// </summary>
        /// <param name="a">A delegate to a method that takes no arguments and 
        /// does not return a value, which is either pushed onto the Dispatcher 
        /// event queue or immediately run, depending on the current thread.</param>
        public static void BeginInvoke(Action a)
        {
            if (_instance == null)
            {
                RequireInstance();
            }

            // If the current thread is the user interface thread, skip the
            // dispatcher and directly invoke the Action.
            if (CheckAccess() || _designer == true)
            {
                a();
            }
            else
            {
#if WINDOWS_PHONE
                _instance.BeginInvoke(a);
#endif
#if NETFX_CORE
                _instance.RunAsync(CoreDispatcherPriority.Normal, () => { a(); });
#endif
            }
        }
    }
}