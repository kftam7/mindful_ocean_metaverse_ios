using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

public class GameSave
{
    #region Server

    public static string AppleIdToken
    {
        get => PlayerPrefs.GetString("APPLE_ID_TOKEN", "");
        set => PlayerPrefs.SetString("APPLE_ID_TOKEN", value);
    }

    public static string AppleAuthCode
    {
        get => PlayerPrefs.GetString("APPLE_AUTH_CODE", "");
        set => PlayerPrefs.SetString("APPLE_AUTH_CODE", value);
    }

    public static string ServerUserInfo
    {
        get => PlayerPrefs.GetString("SERVER_USERINFO");
        set => PlayerPrefs.SetString("SERVER_USERINFO", value);
    }

    public static string ServerEvents
    {
        get => PlayerPrefs.GetString("SERVER_EVENTS");
        set => PlayerPrefs.SetString("SERVER_EVENTS", value);
    }

    public static string ServerNfts
    {
        get => PlayerPrefs.GetString("SERVER_NFTS");
        set => PlayerPrefs.SetString("SERVER_NFTS", value);
    }

    public static string ServerArtworks
    {
        get => PlayerPrefs.GetString("SERVER_ARTWORKS");
        set => PlayerPrefs.SetString("SERVER_ARTWORKS", value);
    }

    public static string ServerArtworkFiles
    {
        get => PlayerPrefs.GetString("SERVER_ARTWORK_FILES");
        set => PlayerPrefs.SetString("SERVER_ARTWORK_FILES", value);
    }

    public static string SubmitArtworkTime
    {
        get => PlayerPrefs.GetString("SUBMIT_ARTWORK_TIME", "1999-01-01T00:00:00.000Z");
        set => PlayerPrefs.SetString("SUBMIT_ARTWORK_TIME", value);
    }

    #endregion

    #region System

    public static bool NewVersion
    {
        get => PlayerPrefs.GetInt("NEW_VERSION", 0) == 1;
        set => PlayerPrefs.SetInt("NEW_VERSION", value ? 1 : 0);
    }

    public static bool TermAccepted
    {
        get => PlayerPrefs.GetInt("TERM_ACCEPTED", 0) == 1;
        set => PlayerPrefs.SetInt("TERM_ACCEPTED", value ? 1 : 0);
    }

    public static string GameLanguage
    {
        get => PlayerPrefs.GetString("GAME_LANGUAGE");
        set => PlayerPrefs.SetString("GAME_LANGUAGE", value);
    }

    //Global
    public static bool SfxOn
    {
        get { return PlayerPrefs.GetInt("SFX_ON", 1) == 1; }
        set { PlayerPrefs.SetInt("SFX_ON", value ? 1 : 0); }
    }

    //Global
    public static bool MusicOn
    {
        get { return PlayerPrefs.GetInt("MUSIC_ON", 1) == 1; }
        set { PlayerPrefs.SetInt("MUSIC_ON", value ? 1 : 0); }
    }

    //Global
    public static bool HasShownCameraPermission
    {
        get
        {
#if EVENT_DEMO
			return true;
#endif
            return PlayerPrefs.GetInt("HAS_SHOWN_CAMERA_PERMISSION", 0) == 1;
        }
        set { PlayerPrefs.SetInt("HAS_SHOWN_CAMERA_PERMISSION", value ? 1 : 0); }
    }

    //Global
    public static bool HasShownPhotoAlbumPermission
    {
        get
        {
#if EVENT_DEMO
			return true;
#endif
            return PlayerPrefs.GetInt("HAS_SHOWN_PHOTO_ALBUM_PERMISSION", 0) == 1;
        }
        set { PlayerPrefs.SetInt("HAS_SHOWN_PHOTO_ALBUM_PERMISSION", value ? 1 : 0); }
    }

    #endregion

    #region Tutorial

    public static bool ShowTutorialMainFlow
    {
        get => PlayerPrefs.GetInt("SHOW_TUTORIAL_MAIN_FLOW", 1) == 1;
        set => PlayerPrefs.SetInt("SHOW_TUTORIAL_MAIN_FLOW", value ? 1 : 0);
    }

    public static bool ShowTutorialHatchery
    {
        get => PlayerPrefs.GetInt("SHOW_TUTORIAL_HATCHERY", 1) == 1;
        set => PlayerPrefs.SetInt("SHOW_TUTORIAL_HATCHERY", value ? 1 : 0);
    }

    public static bool ShowTutorialColoring
    {
        get => PlayerPrefs.GetInt("SHOW_TUTORIAL_COLORING", 1) == 1;
        set => PlayerPrefs.SetInt("SHOW_TUTORIAL_COLORING", value ? 1 : 0);
    }

    public static bool ShowTutorialPreview
    {
        get => PlayerPrefs.GetInt("SHOW_TUTORIAL_PREVIEW", 1) == 1;
        set => PlayerPrefs.SetInt("SHOW_TUTORIAL_PREVIEW", value ? 1 : 0);
    }

    public static bool ShowTutorialMetaverse
    {
        get => PlayerPrefs.GetInt("SHOW_TUTORIAL_METAVERSE", 1) == 1;
        set => PlayerPrefs.SetInt("SHOW_TUTORIAL_METAVERSE", value ? 1 : 0);
    }

    public static bool ShowTutorialMetaverseAr
    {
        get => PlayerPrefs.GetInt("SHOW_TUTORIAL_METAVERSE_AR", 1) == 1;
        set => PlayerPrefs.SetInt("SHOW_TUTORIAL_METAVERSE_AR", value ? 1 : 0);
    }

    public static bool ShowTutorialDomainAdventureAr
    {
        get => PlayerPrefs.GetInt("SHOW_TUTORIAL_DOMAIN_ADVENTURE_AR", 1) == 1;
        set => PlayerPrefs.SetInt("SHOW_TUTORIAL_DOMAIN_ADVENTURE_AR", value ? 1 : 0);
    }

    //Global
    public static bool DefaultFishAdded
    {
        get => PlayerPrefs.GetInt("DEFAULT_FISH_ADDED", 0) == 1;
        set => PlayerPrefs.SetInt("DEFAULT_FISH_ADDED", value ? 1 : 0);
    }

    public static bool DefaultFishMessageShown
    {
        get => PlayerPrefs.GetInt("DEFAULT_FISH_MESSAGE_SHOWN", 0) == 1;
        set => PlayerPrefs.SetInt("DEFAULT_FISH_MESSAGE_SHOWN", value ? 1 : 0);
    }

    public static bool IsTutorialFinished
    {
        get
        {
#if FORCE_PRINTING_TUTORIAL
            return true;
#endif
            // return PlayerPrefs.GetInt("TUTORIAL_PROGRESS", 0) == 1;
            return true; // disable tutorial temporarily
        }

        set { PlayerPrefs.SetInt("TUTORIAL_PROGRESS", value ? 1 : 0); }
    }

    public static string LastTutorialFinishTime
    {
        get
        {
#if FORCE_PRINTING_TUTORIAL
            return System.DateTime.UtcNow.ToString();
#endif
            return PlayerPrefs.GetString("LAST_TUTORAIL_FINISH_TIME", "");
        }

        set { PlayerPrefs.SetString("LAST_TUTORAIL_FINISH_TIME", value); }
    }

    public static bool IsPrinterTutorialFinished
    {
        get
        {
#if FORCE_PRINTING_TUTORIAL
            return false;
#endif
            return PlayerPrefs.GetInt("TUTORIAL_PRINTER_PROGRESS", 0) == 1;
        }

        set { PlayerPrefs.SetInt("TUTORIAL_PRINTER_PROGRESS", value ? 1 : 0); }
    }

    #endregion

    #region Aquarium

    public static int AquariumIndex
    {
        get => PlayerPrefs.GetInt("AQUARIUM_INDEX", 1);
        set => PlayerPrefs.SetInt("AQUARIUM_INDEX", value);
    }

    public static int LastSelectedAquariumCellIndex
    {
        get => PlayerPrefs.GetInt("LAST_SELECTED_AQUARIUM_CELL_INDEX", 0);
        set => PlayerPrefs.SetInt("LAST_SELECTED_AQUARIUM_CELL_INDEX", value);
    }

    public static string SelectedAquarium
    {
        get => PlayerPrefs.GetString("SELECTED_AQUARIUM", GameSetting.DefaultAquariumSaveKey);
        set => PlayerPrefs.SetString("SELECTED_AQUARIUM", value);
    }


    public static string AquariumSave
    {
        get => PlayerPrefs.GetString("AQUARIUM_SAVE", "");
        set => PlayerPrefs.SetString("AQUARIUM_SAVE", value);
    }

    public static bool NewFishAddedFlag
    {
        get => PlayerPrefs.GetInt("NEW_FISH_ADDED_FLAG", 0) == 1;
        set => PlayerPrefs.SetInt("NEW_FISH_ADDED_FLAG", value ? 1 : 0);
    }

    #endregion

    #region Purchase

    public static string PurchaseRecord
    {
        get => PlayerPrefs.GetString("PURCHASE_RECORD", "");
        set => PlayerPrefs.SetString("PURCHASE_RECORD", value);
    }

    #endregion

    #region Coloring

    public static Color SavedPickerColor
    {
        get =>
            new Color(PlayerPrefs.GetFloat("SAVED_PICKER_COLOR_R", 0),
                PlayerPrefs.GetFloat("SAVED_PICKER_COLOR_G", 0),
                PlayerPrefs.GetFloat("SAVED_PICKER_COLOR_B", 0),
                PlayerPrefs.GetFloat("SAVED_PICKER_COLOR_A", 0));
        set
        {
            PlayerPrefs.SetFloat("SAVED_PICKER_COLOR_R", value.r);
            PlayerPrefs.SetFloat("SAVED_PICKER_COLOR_G", value.g);
            PlayerPrefs.SetFloat("SAVED_PICKER_COLOR_B", value.b);
            PlayerPrefs.SetFloat("SAVED_PICKER_COLOR_A", value.a);
        }
    }

    public static int SavedPickerColorSetIndex
    {
        get => PlayerPrefs.GetInt("SAVED_PICKER_COLORSET_INDEX", 0);
        set => PlayerPrefs.SetInt("SAVED_PICKER_COLORSET_INDEX", value);
    }

    public static int SavedPickerColorIndex
    {
        get => PlayerPrefs.GetInt("SAVED_PICKER_COLOR_INDEX", 0);
        set => PlayerPrefs.SetInt("SAVED_PICKER_COLOR_INDEX", value);
    }

    public static float LastBrushSize
    {
        get => PlayerPrefs.GetFloat("LAST_BRUSH_SIZE", 0.6f);
        set => PlayerPrefs.SetFloat("LAST_BRUSH_SIZE", value);
    }

    public static float LastColorBrightness
    {
        get => PlayerPrefs.GetFloat("LAST_COLOR_BRIGHTNESS", 0.5f);
        set => PlayerPrefs.SetFloat("LAST_COLOR_BRIGHTNESS", value);
    }

    public static string LastSavedImageId
    {
        get => PlayerPrefs.GetString("LAST_SAVED_IMAGE");
        set => PlayerPrefs.SetString("LAST_SAVED_IMAGE", value);
    }

    #endregion

    #region Others

    public static bool IsCollectedPrintingProblemReason
    {
        get => PlayerPrefs.GetInt("IS_COLLECTED_PRINTING_PROBLEM_REASON", 0) == 1;
        set => PlayerPrefs.SetInt("IS_COLLECTED_PRINTING_PROBLEM_REASON", value ? 1 : 0);
    }

    public static string FirstPrintFailTime
    {
        get => PlayerPrefs.GetString("FIRST_PRINT_FAIL_TIME", "");
        set => PlayerPrefs.SetString("FIRST_PRINT_FAIL_TIME", value);
    }

    public static bool HasSuccessfullyPrintedColoringPaper
    {
        get => PlayerPrefs.GetInt("HAS_SUCCESSFULLY_PRINTED_COLORING_PAPER", 0) == 1;
        set => PlayerPrefs.SetInt("HAS_SUCCESSFULLY_PRINTED_COLORING_PAPER", value ? 1 : 0);
    }

    public static bool HasSuccessfullyScannedColoringPaper
    {
        get => PlayerPrefs.GetInt("HAS_SUCCESSFULLY_SCANNED_COLORING_PAPER", 0) == 1;
        set => PlayerPrefs.SetInt("HAS_SUCCESSFULLY_SCANNED_COLORING_PAPER", value ? 1 : 0);
    }

    public static string SelectedModel
    {
        get => PlayerPrefs.GetString("SELECTED_MODEL", GameSetting.DefaultModel);
        set => PlayerPrefs.SetString("SELECTED_MODEL", value);
    }

    public static string LastEnvironmentSkin
    {
        get => PlayerPrefs.GetString("LAST_ENVIRONMENT_SKIN", "");
        set => PlayerPrefs.SetString("LAST_ENVIRONMENT_SKIN", value);
    }

    public static string SearchCode
    {
        get => PlayerPrefs.GetString("SEARCH_CODE");
        set => PlayerPrefs.SetString("SEARCH_CODE", value);
    }

    public static int Coin
    {
        get
        {
#if UNLIMITED_COIN
			return int.MaxValue / 2;
#endif
            return PlayerPrefs.GetInt("COIN", 0);
        }

        set
        {
#if UNLIMITED_COIN
			value = int.MaxValue / 2;
#endif
            PlayerPrefs.SetInt("COIN", value);
        }
    }

    public static string LastShownPackage
    {
        get => PlayerPrefs.GetString("LAST_SHOWN_PACKAGE");
        set => PlayerPrefs.SetString("LAST_SHOWN_PACKAGE", value);
    }

    public static string TaskProgress
    {
        get => PlayerPrefs.GetString("TASK_PROGRESS");
        set => PlayerPrefs.SetString("TASK_PROGRESS", value);
    }

    public static string AvailabilityOfModel
    {
        get => PlayerPrefs.GetString("AVAILABILITY_OF_MODEL", "");
        set => PlayerPrefs.SetString("AVAILABILITY_OF_MODEL", value);
    }

    public static string LeaderboardList
    {
        get => PlayerPrefs.GetString("LEADERBOARD_LIST", "");
        set => PlayerPrefs.SetString("LEADERBOARD_LIST", value);
    }

    public static string CharacterUpgrade
    {
        get => PlayerPrefs.GetString("CHARACTER_UPGRADE", "");
        set => PlayerPrefs.SetString("CHARACTER_UPGRADE", value);
    }

    public static string GetEditedName(string modelName)
    {
        return PlayerPrefs.GetString(modelName + "_Name", "");
    }

    public static void SetEditedName(string modelName, string name)
    {
        PlayerPrefs.SetString(modelName + "_Name", name);
    }

    public static string GetModelSkin(string modelName)
    {
        return PlayerPrefs.GetString(modelName + "_Skin", "");
    }

    public static void SetModelSkin(string modelName, string skinName)
    {
        PlayerPrefs.SetString(modelName + "_Skin", skinName);
    }

    public static int MenuSelectedIndex
    {
        get => PlayerPrefs.GetInt("MENU_SELECTED_INDEX", 0);
        set => PlayerPrefs.SetInt("MENU_SELECTED_INDEX", value);
    }

    public static string EbookUnlockStatus
    {
        get => PlayerPrefs.GetString("EBOOK_UNLOCK_STATUS", "");
        set => PlayerPrefs.SetString("EBOOK_UNLOCK_STATUS", value);
    }

    public static string NewEbookPages
    {
        get => PlayerPrefs.GetString("NEW_EBOOK_PAGES", "");
        set => PlayerPrefs.SetString("NEW_EBOOK_PAGES", value);
    }

    public static string QuestionnaireResult
    {
        get => PlayerPrefs.GetString("QUESTIONNAIRE_RESULT");
        set => PlayerPrefs.SetString("QUESTIONNAIRE_RESULT", value);
    }

    public static string CachedQuestionnaireResultList
    {
        get => PlayerPrefs.GetString("CACHED_QUESTIONNAIRE_RESULT_LIST");
        set => PlayerPrefs.SetString("CACHED_QUESTIONNAIRE_RESULT_LIST", value);
    }

    //Global
    public static bool IsFinishedQuestionnaire
    {
        get => PlayerPrefs.GetInt("IS_FINISHED_QUESTIONNAIRE", 0) == 1;
        set => PlayerPrefs.SetInt("IS_FINISHED_QUESTIONNAIRE", value ? 1 : 0);
    }

    public static bool IsNewInfoUnlocked
    {
        get => PlayerPrefs.GetInt("IS_NEW_INFO_UNLOCKED", 0) == 1;
        set => PlayerPrefs.SetInt("IS_NEW_INFO_UNLOCKED", value ? 1 : 0);
    }

    public static string SelectedPackageId
    {
        get => PlayerPrefs.GetString("SELECTED_PACKAGE_ID");
        set => PlayerPrefs.SetString("SELECTED_PACKAGE_ID", value);
    }

    public static int LastSelectedPackageCellIndex
    {
        get => PlayerPrefs.GetInt("LAST_SELECTED_PACKAGE_CELL_INDEX", 0);
        set => PlayerPrefs.SetInt("LAST_SELECTED_PACKAGE_CELL_INDEX", value);
    }

    public static int LastSelectedColoringPageCellIndex
    {
        get => PlayerPrefs.GetInt("LAST_SELECTED_COLORINGPAGE_CELL_INDEX", 0);
        set => PlayerPrefs.SetInt("LAST_SELECTED_COLORINGPAGE_CELL_INDEX", value);
    }

    public static int NumEnterARMode
    {
        get => PlayerPrefs.GetInt("NUM_ENTER_ARMODE", 0);
        set => PlayerPrefs.SetInt("NUM_ENTER_ARMODE", value);
    }

    public static int NumDrawColorPage
    {
        get => PlayerPrefs.GetInt("NUM_DRAW_COLOR_PAGE", 0);
        set => PlayerPrefs.SetInt("NUM_DRAW_COLOR_PAGE", value);
    }

    public static bool DidShowColorPageReview
    {
        get => PlayerPrefs.GetInt("DID_SHOW_COLOR_PAGE_REVIEW", 0) == 1;
        set => PlayerPrefs.SetInt("DID_SHOW_COLOR_PAGE_REVIEW", value ? 1 : 0);
    }

    public static bool DidShowARModeReview
    {
        get => PlayerPrefs.GetInt("DID_SHOW_ARMODE_REVIEW", 0) == 1;
        set => PlayerPrefs.SetInt("DID_SHOW_ARMODE_REVIEW", value ? 1 : 0);
    }

    public static int ColoringMode
    {
        get => PlayerPrefs.GetInt("COLORING_MODE", 0); // 0 in-app-coloring, 1 coloring with physical paper
        set => PlayerPrefs.SetInt("COLORING_MODE", value);
    }

    #endregion

    #region Game
    public static bool SinglePlayerModeLimitReached
    {
        get => PlayerPrefs.GetInt("SINGLE_PLAYER_LIMIT", 0) == 1;
        set
        {
            PlayerPrefs.SetInt("SINGLE_PLAYER_LIMIT", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static bool MultiPlayerModeLimitReached
    {
        get => PlayerPrefs.GetInt("MULTI_PLAYER_LIMIT", 0) == 1;
        set
        {
            PlayerPrefs.SetInt("MULTI_PLAYER_LIMIT", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static int MultiPlayerModeMatchCount
    {
        get => PlayerPrefs.GetInt("MULTI_PLAYER_DAILY_MATCH", 0);
        set
        {
            PlayerPrefs.SetInt("MULTI_PLAYER_DAILY_MATCH", value);
            PlayerPrefs.Save();
        }
    }

    public static DateTime LastSessionDateTime
    {
        get => DateTime.TryParse(PlayerPrefs.GetString("LAST_SESSION_DATE_TIME"), out var result) ? result : DateTime.UnixEpoch;
        set
        {
            PlayerPrefs.SetString("LAST_SESSION_DATE_TIME", value.ToString());
            PlayerPrefs.Save();
        }
    }

    public static int SinglePlayerBottlesCollectedToday
    {
        get => PlayerPrefs.GetInt("SINGLE_PLAYER_BOTTLES_COLLECTED_TODAY", 0);
        set
        {
            PlayerPrefs.SetInt("SINGLE_PLAYER_BOTTLES_COLLECTED_TODAY", value);
            PlayerPrefs.Save();
        }
    }
    #endregion
}