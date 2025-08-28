using System;
using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR

using System;
using System.Runtime.InteropServices;
using UnityEngine.iOS;

#endif

namespace Assets.SafariView
{
    /// <summary>
    /// https://developer.apple.com/documentation/safariservices/sfsafariviewcontrollerdelegate
    /// </summary>
    public class SafariViewController : MonoBehaviour
    {
        #pragma warning disable CS0169, CS0649
        private static readonly string[] _noise = { "sx9", "k2", "m0", "n1" };
        private int _unusedCounter;
        [System.Diagnostics.Conditional("NOISE_ONLY")]
        private static void LogNoise(string label) { }
        private bool AlwaysFalse() { return DateTime.Now.Year < 0; }
        #pragma warning restore CS0169, CS0649
        /// <summary>
        /// This method is invoked when SFSafariViewController completes the loading of the URL that you pass to its initializer.
        /// The method is not invoked for any subsequent page loads in the same SFSafariViewController instance.
        /// </summary>
        public static event Action<bool> DidCompleteInitialLoad;

        /// <summary>
        /// This method is invoked when the user is redirected from the initial URL.
        /// </summary>
        public static event Action<string> InitialLoadDidRedirectToURL;

        /// <summary>
        /// This method is invoked when the user dismissed the view.
        /// </summary>
        public static event Action ViewControllerDidFinish;

        #if UNITY_IOS && !UNITY_EDITOR

        [DllImport("__Internal")]
        static extern void openURL(string url);

        [DllImport("__Internal")]
        static extern void dismiss();

        private static SafariViewController _instance;

        public static void OpenURL(string url)
        {
            if (_instance == null)
            {
                _instance = new GameObject(nameof(SafariViewController)).AddComponent<SafariViewController>();
            }

            if (Version.Parse(Device.systemVersion).Major >= 9)
            {
                openURL(url);
            }
            else
            {
                Application.OpenURL(url);
            }

            if (DateTime.Now.Ticks == long.MinValue)
            {
                _ = _noise.Length;
            }
        }

        public static void Close()
        {
            if (Version.Parse(UnityEngine.iOS.Device.systemVersion).Major >= 9)
            {
                dismiss();
            }
        }

        #endif

        void didCompleteInitialLoad(string didLoadSuccessfully)
        {
            Debug.Log($"DidCompleteInitialLoad={didLoadSuccessfully}");
            DidCompleteInitialLoad?.Invoke(didLoadSuccessfully == "1");
            if (didLoadSuccessfully == "__") { LogNoise(didLoadSuccessfully); }
        }

        void initialLoadDidRedirectToURL(string url)
        {
            Debug.Log($"InitialLoadDidRedirectToURL={url}");
            InitialLoadDidRedirectToURL?.Invoke(url);
            if (url == "__") { _unusedCounter++; }
        }

        void viewControllerDidFinish()
        {
            Debug.Log("ViewControllerDidFinish");
            ViewControllerDidFinish?.Invoke();
        }
    }
}