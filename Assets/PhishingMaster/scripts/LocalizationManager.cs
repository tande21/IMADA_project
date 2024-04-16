using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class LocalizationManager : MonoBehaviour
{
    private static StringTable strings_base { get; set; }
    private static StringTable strings_challenge { get; set; }
    private static StringTable strings_game_scene { get; set; }
    private static StringTable strings_results_highscore { get; set; }
    private static StringTable strings_tutorial { get; set; }
    private static AssetTable assets_base { get; set; }

    private static int updatesRunning = 0;
    private static string userLanguage;
    private static LocalizationManager _localizationManagerInstance;
    public static LocalizationManager Instance
    {
        get { return _localizationManagerInstance; }
        private set { _localizationManagerInstance = value; }
    }

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Instance = this;
        UpdateLocalizationTables();
    }

    void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
    }

    void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
    }

    public StringTableEntry GetStringTableEntry(string table, string key)
    {
        if (table.Equals("strings_base"))
        {
            return strings_base[key];
        }
        else if (table.Equals("strings_challenge"))
        {
            return strings_challenge[key];
        }
        else if (table.Equals("strings_game_scene"))
        {
            return strings_game_scene[key];
        }
        else if (table.Equals("strings_results_highscore"))
        {
            return strings_results_highscore[key];
        }
        else if (table.Equals("strings_tutorial"))
        {
            return strings_tutorial[key];
        }
        else
        {
            throw new ArgumentException("Unknown table name: " + table);
        }
    }

    public AssetTableEntry GetAssetTableEntry(string table, string key)
    {
        if (table.Equals("assets_base"))
        {
            return assets_base[key];
        }
        else
        {
            throw new ArgumentException("Unknown table name: " + table);
        }
    }

    public void UpdateLocalizationTables()
    {
        Debug.Log("Update localization tables...");
        updatesRunning++;
        StartCoroutine(LoadLocalizationTables());
    }

    private IEnumerator LoadLocalizationTables()
    {
        yield return LocalizationSettings.InitializationOperation;
        var string_table = LocalizationSettings.StringDatabase.GetTableAsync("strings_base");
        yield return string_table;
        strings_base = string_table.Result;

        string_table = LocalizationSettings.StringDatabase.GetTableAsync("strings_challenge");
        yield return string_table;
        strings_challenge = string_table.Result;

        string_table = LocalizationSettings.StringDatabase.GetTableAsync("strings_game_scene");
        yield return string_table;
        strings_game_scene = string_table.Result;

        string_table = LocalizationSettings.StringDatabase.GetTableAsync("strings_results_highscore");
        yield return string_table;
        strings_results_highscore = string_table.Result;

        string_table = LocalizationSettings.StringDatabase.GetTableAsync("strings_tutorial");
        yield return string_table;
        strings_tutorial = string_table.Result;

        var asset_table = LocalizationSettings.AssetDatabase.GetTableAsync("assets_base");
        yield return asset_table;
        assets_base = asset_table.Result;

        Debug.Log("Localization tables loaded.");
        updatesRunning--;
    }

    /// <summary>
    /// Sets the user language to the given language code.
    /// Sets the LocalizationSettings to the best matching locale which has a translation.
    /// This also updates the localization tables.
    /// </summary>
    /// <param name="userLanguageCode"></param>
    public void SetUserLanguage(string userLanguageCode)
    {
        StartCoroutine(SetUserLanguageAsync(userLanguageCode));
    }

    private IEnumerator SetUserLanguageAsync(string userLanguageCode)
    {
        yield return LocalizationSettings.InitializationOperation;
        userLanguage = userLanguageCode;
        if (userLanguage != null)
        {
            string languageCode = userLanguage;
            string languageCodeShort = languageCode.Split('-')[0];
            Locale bestMatch = null;
            foreach (Locale locale in LocalizationSettings.AvailableLocales.Locales)
            {
                if (locale.Identifier.Code == languageCode)
                {
                    bestMatch = locale;
                }
                else if (bestMatch == null && locale.Identifier.Code == languageCodeShort)
                {
                    bestMatch = locale;
                }
            }
            if (bestMatch != null)
            {
                Debug.Log("Setting user prefered language to " + bestMatch.Identifier.Code);
                LocalizationSettings.SelectedLocale = bestMatch;
            }
        }
        UpdateLocalizationTables();
    }

    public bool IsUserLanguageSet() => userLanguage != null;

    public bool IsTablesReady()
    {
        if (strings_base != null && strings_challenge != null && strings_game_scene != null && strings_results_highscore != null && strings_tutorial != null && assets_base != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsReady()
    {
        if (updatesRunning == 0 && strings_base != null && strings_challenge != null && strings_game_scene != null && strings_results_highscore != null && strings_tutorial != null && assets_base != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnLanguageChanged(Locale locale)
    {
        UpdateLocalizationTables();
    }
}
