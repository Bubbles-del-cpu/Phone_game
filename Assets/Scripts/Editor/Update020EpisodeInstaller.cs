#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using MeetAndTalk;
using MeetAndTalk.Event;
using MeetAndTalk.GlobalValue;
using MeetAndTalk.Localization;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.Video;

/// <summary>
/// Rebuildable installer for the 0.20 story update. The source-of-truth dialogue
/// remains the two supplied text files; this class translates their authored
/// ranges into Meet & Talk graphs and wires the chapters into the active game.
/// </summary>
public static class Update020EpisodeInstaller
{
    private const string Root = "Assets/Entities/Dialogue/ep 27 and ep 27.5";
    private const string Media = Root + "/media/";
    private const string Episode27Source = Root + "/scripts/NTS HM EP 27.txt";
    private const string Episode275Source = Root + "/scripts/NTS HM EP 27.5.txt";
    private const string JapaneseTranslationSource = Root + "/scripts/Update 0.20 Japanese.tsv";
    private const string Episode27Asset = "Assets/Entities/Dialogue/Episode 27.asset";
    private const string Episode275Asset = "Assets/Entities/Dialogue/Episode 27.5.asset";
    private const string GeneratedFolder = "Assets/Entities/Update 0.20";
    private const string VariablesFolder = "Assets/Entities/Variables/Update 0.20";
    private const string PostsFolder = "Assets/Entities/Social Media Posts/ep27";
    private const string GalleryConfigPath = "Assets/New Files & Scripts/GalleryUnlockConfig.asset";
    private const string Update020GalleryCode = "BUBBLE TROUBLE";

    private const string LilyPath = "Assets/Entities/Characters/Lily.asset";
    private const string LeoPath = "Assets/Entities/Characters/Leo.asset";
    private const string MathPath = "Assets/Entities/Characters/Math.asset";
    private const string MathLilyPath = "Assets/Entities/Characters/Math&Lily.asset";
    private const string MathBabesPath = "Assets/Entities/Characters/Math&Babes.asset";
    private const string SemPath = GeneratedFolder + "/Sem.asset";
    private const string LeoSocialProfilePath = GeneratedFolder + "/Leo Social Profile.asset";
    private const string LeoSpicyProfilePath = GeneratedFolder + "/Leo Spicy Social Profile.asset";
    private const string LeoProfileImagePath = "Assets/Sprites/Gallery/ep17/profile image Leo.png";
    private const string LeoMiniProfileImagePath = "Assets/Sprites/Charecter PFP's/Leo.jpg";

    private static DialogueCharacterSO Lily;
    private static DialogueCharacterSO Leo;
    private static DialogueCharacterSO Math;
    private static DialogueCharacterSO MathLily;
    private static DialogueCharacterSO MathBabes;
    private static DialogueCharacterSO Sem;

    private static readonly Dictionary<string, SocialMediaPostSO> Posts = new();
    private static readonly Dictionary<string, string> JapaneseByEnglish = new();

    [MenuItem("Tools/NTS/Install Update 0.20 Episodes")]
    public static void Install()
    {
        PlayerSettings.bundleVersion = "0.20.1.beta";
        EnsureFolder(GeneratedFolder);
        EnsureFolder(VariablesFolder);
        EnsureFolder(PostsFolder);
        EnsureGalleryCode();
        LoadJapaneseTranslations();

        LoadCharacters();
        EnsureLeoProfileLocalization();
        EnsureLeoProfiles();
        EnsureVariables();
        EnsurePosts();
        EnsureReplayQuestionLocalization();

        var episode27 = LoadOrCreate<DialogueContainerSO>(Episode27Asset);
        episode27.name = "Episode 27";
        BuildEpisode27(episode27);
        EditorUtility.SetDirty(episode27);

        var episode275 = LoadOrCreate<DialogueContainerSO>(Episode275Asset);
        episode275.name = "Episode 27.5";
        BuildEpisode275(episode275);
        EditorUtility.SetDirty(episode275);

        WireIntoGame(episode27, episode275);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        VerifyGalleryCode();
        Debug.Log("[Update 0.20] Episodes 27 and 27.5 installed, including media, variables, social posts, replay questions, and the new gallery code.");
    }

    private static void EnsureGalleryCode()
    {
        var config = RequireAsset<GalleryUnlockConfig>(GalleryConfigPath);
        var serializedConfig = new SerializedObject(config);
        serializedConfig.FindProperty("_codeHash").stringValue =
            GalleryHelper.ComputeHash(Update020GalleryCode, config.Salt);
        serializedConfig.FindProperty("_reference").intValue = Update020GalleryCode.Length;
        serializedConfig.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(config);
    }

    [MenuItem("Tools/NTS/Verify Update 0.20 Gallery Code")]
    public static void VerifyGalleryCode()
    {
        var config = RequireAsset<GalleryUnlockConfig>(GalleryConfigPath);
        var correctCodeWorks = config.Length == Update020GalleryCode.Length &&
                               GalleryHelper.MatchesCode(Update020GalleryCode, config.Salt, config.Hash);
        var incorrectCodeFails = !GalleryHelper.MatchesCode("BUBBLE TROUBLES", config.Salt, config.Hash);

        if (!correctCodeWorks || !incorrectCodeFails)
            throw new InvalidOperationException("Update 0.20 gallery code verification failed.");

        Debug.Log("[Update 0.20] Gallery code verification passed: correct code accepted and incorrect code rejected.");
    }

    private static void LoadJapaneseTranslations()
    {
        JapaneseByEnglish.Clear();
        if (!File.Exists(JapaneseTranslationSource))
        {
            Debug.LogWarning($"[Update 0.20] Japanese translation file is missing: {JapaneseTranslationSource}");
            return;
        }

        var lineNumber = 0;
        foreach (var rawLine in File.ReadLines(JapaneseTranslationSource))
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(rawLine) || rawLine.StartsWith("#", StringComparison.Ordinal))
                continue;

            var columns = rawLine.Split('\t');
            if (columns.Length != 2)
                throw new InvalidDataException($"Malformed Japanese translation row {lineNumber}.");

            string english;
            string japanese;
            try
            {
                english = Encoding.UTF8.GetString(Convert.FromBase64String(columns[0]));
                japanese = Encoding.UTF8.GetString(Convert.FromBase64String(columns[1]));
            }
            catch (FormatException exception)
            {
                throw new InvalidDataException($"Invalid base64 in Japanese translation row {lineNumber}.", exception);
            }

            if (!JapaneseByEnglish.TryAdd(english, japanese))
                throw new InvalidDataException($"Duplicate Japanese translation source text on row {lineNumber}: {english}");
        }
    }

    private static string JapaneseFor(string english) =>
        JapaneseByEnglish.TryGetValue(english ?? string.Empty, out var japanese) ? japanese : string.Empty;

    private static List<LanguageGeneric<string>> TextLanguages(string english) =>
        Languages(english, JapaneseFor(english));

    private static void LoadCharacters()
    {
        Lily = RequireAsset<DialogueCharacterSO>(LilyPath);
        Leo = RequireAsset<DialogueCharacterSO>(LeoPath);
        Math = RequireAsset<DialogueCharacterSO>(MathPath);
        MathLily = RequireAsset<DialogueCharacterSO>(MathLilyPath);
        MathBabes = RequireAsset<DialogueCharacterSO>(MathBabesPath);

        Sem = AssetDatabase.LoadAssetAtPath<DialogueCharacterSO>(SemPath);
        if (Sem == null)
        {
            Sem = ScriptableObject.CreateInstance<DialogueCharacterSO>();
            Sem.name = "Sem";
            AssetDatabase.CreateAsset(Sem, SemPath);
        }

        var semAvatar = RequireAsset<Sprite>(Media + "sem profile image.png");
        var semGallery = RequireAsset<Sprite>(Media + "sem.jpg");
        Sem.characterName = Languages("Sem", "Sem");
        Sem.CustomizedName = new GlobalValueClass();
        Sem.UseGlobalValue = false;
        Sem.Images = new List<Sprite> { semGallery };
        Sem.textColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        Sem.Avatars = new List<CharacterSprite>
        {
            new() { type = AvatarType.Normal, LeftPosition = semAvatar, RightPosition = semAvatar }
        };
        Sem.SocialMediaProfile = null;
        Sem.SpicySocialMediaProfile = null;
        EditorUtility.SetDirty(Sem);
    }

    private static void EnsureLeoProfileLocalization()
    {
        var values = new Dictionary<string, string>
        {
            ["leo_profile_name"] = "Leo",
            ["leo_profile_description"] = "Painter. Teacher. Usually covered in charcoal.",
            ["leo_spicy_profile_name"] = "Leo After Dark",
            ["leo_spicy_profile_description"] = "Studies in light, form, and bad decisions."
        };

        var collection = LocalizationEditorSettings.GetStringTableCollection("Social Media Apps");
        if (collection == null)
            throw new InvalidOperationException("Social Media Apps localization collection was not found.");

        foreach (var pair in values)
        {
            var shared = collection.SharedData.GetEntry(pair.Key) ?? collection.SharedData.AddKey(pair.Key);
            foreach (var table in collection.StringTables)
            {
                var entry = table.GetEntry(shared.Id) ?? table.AddEntry(shared.Id, pair.Value);
                entry.Value = pair.Value;
                EditorUtility.SetDirty(table);
            }
        }

        EditorUtility.SetDirty(collection.SharedData);
    }

    private static void EnsureLeoProfiles()
    {
        var profileImage = RequireAsset<Sprite>(LeoProfileImagePath);
        var miniIcon = RequireAsset<Sprite>(LeoMiniProfileImagePath);
        var regular = ConfigureLeoProfile(LeoSocialProfilePath, "Leo Social Profile", profileImage, miniIcon,
            "leo_profile_name", "leo_profile_description", 1284, 73, new Color(0.32f, 0.18f, 0.12f, 1f));
        var spicy = ConfigureLeoProfile(LeoSpicyProfilePath, "Leo Spicy Social Profile", profileImage, miniIcon,
            "leo_spicy_profile_name", "leo_spicy_profile_description", 436, 18, new Color(0.48f, 0.08f, 0.08f, 1f));

        Leo.SocialMediaProfile = regular;
        Leo.SpicySocialMediaProfile = spicy;
        EditorUtility.SetDirty(Leo);
    }

    private static SocialMediaProfileSO ConfigureLeoProfile(string path, string assetName, Sprite profileImage,
        Sprite miniIcon, string nameKey, string descriptionKey, int followers, int following, Color color)
    {
        var profile = LoadOrCreate<SocialMediaProfileSO>(path);
        profile.name = assetName;
        profile.BaseGalleryMediaItems ??= new List<SocialMediaProfileSO.BaseGalleryMediaItem>();
        SetPrivateField(profile, "_profileImage", profileImage);
        SetPrivateField(profile, "_miniProfileIcon", miniIcon);
        SetPrivateField(profile, "_profileName", Localized("Social Media Apps", nameKey));
        SetPrivateField(profile, "_profileDescription", Localized("Social Media Apps", descriptionKey));
        SetPrivateField(profile, "_followerCount", followers);
        SetPrivateField(profile, "_followingCount", following);
        SetPrivateField(profile, "_profileColor", color);
        EditorUtility.SetDirty(profile);
        return profile;
    }

    private static void SetPrivateField<T>(object target, string fieldName, T value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null)
            throw new MissingFieldException(target.GetType().Name, fieldName);
        field.SetValue(target, value);
    }

    private static void EnsureVariables()
    {
        var manager = RequireAsset<GlobalValueManager>("Assets/Meet and Talk/Resources/GlobalValue.asset");
        var names = new[]
        {
            "leo_humiliation", "painting_public", "sem_known", "sem_stays",
            "leo_raw", "leo_anal_first", "sem_threesome"
        };

        foreach (var name in names)
        {
            if (manager.BoolValues.All(value => value.ValueName != name))
            {
                manager.BoolValues.Add(new GlobalValueBool
                {
                    ValueName = name,
                    Value = false,
                    BaseValue = false,
                    PreviousValues = new List<bool>()
                });
            }

            GetBoolEvent(name, false);
            GetBoolEvent(name, true);
        }

        EditorUtility.SetDirty(manager);
    }

    private static GlobalValueEvent GetBoolEvent(string valueName, bool value)
    {
        var safeName = valueName.Replace(".", "_");
        var path = $"{VariablesFolder}/{safeName}_{(value ? "true" : "false")}.asset";
        var evt = AssetDatabase.LoadAssetAtPath<GlobalValueEvent>(path);
        if (evt == null)
        {
            evt = ScriptableObject.CreateInstance<GlobalValueEvent>();
            evt.name = Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(evt, path);
        }

        evt.Operation = new GlobalValueOperationClass
        {
            ValueName = valueName,
            Operation = GlobalValueOperations.Set,
            OperationValue = value ? "true" : "false"
        };
        EditorUtility.SetDirty(evt);
        return evt;
    }

    private static void EnsurePosts()
    {
        Posts.Clear();
        Posts["leo1"] = CreatePost("Leo 1 - Tonight", Leo,
            "Tonight. 6 for the class, 7 for everyone else. One new piece. Wine is bad, come anyway.",
            "今夜。クラスは6時、一般は7時。新作が一点。ワインはまずいけど、それでも来て。",
            Media + "ep27 1 covered canvas.png", MediaTargetPlatform.SocialMediaPost);
        Posts["leo3"] = CreatePost("Leo 3 - End of Season", Leo,
            "End of season.",
            "シーズン終了。",
            Media + "LEO AFTER PATRY SOCIAL IMAGE ep27.5.png", MediaTargetPlatform.SocialMediaPost);

        Posts["math_spicy_open"] = CreatePost("Math Arcade Bonus - Open", MathLily,
            "Player 2", "Player 2", Media + "MATH 2.5 NTR math and lily spicy social image.png",
            MediaTargetPlatform.SpicySocialMediaPost);
        Posts["math_spicy_secret"] = CreatePost("Math Arcade Bonus - Secret", MathBabes,
            "Player 2", "Player 2", Media + "MATH 2.5 NTR math and babes spicy social post.png",
            MediaTargetPlatform.SpicySocialMediaPost);
    }

    private static SocialMediaPostSO CreatePost(string assetName, DialogueCharacterSO character, string caption,
        string japaneseCaption, string imagePath, MediaTargetPlatform platform)
    {
        var path = $"{PostsFolder}/{assetName}.asset";
        var post = AssetDatabase.LoadAssetAtPath<SocialMediaPostSO>(path);
        if (post == null)
        {
            post = ScriptableObject.CreateInstance<SocialMediaPostSO>();
            post.name = assetName;
            AssetDatabase.CreateAsset(post, path);
        }

        post.Character = character;
        post.MessageTexts = Languages(caption, japaneseCaption);
        post.Message = string.Empty;
        post.MediaType = MediaType.Sprite;
        post.TargetPlatform = platform;
        post.GalleryVisibility = GalleryDisplay.Display;
        post.Image = RequireAsset<Sprite>(imagePath);
        post.Video = null;
        post.VideoThumbnail = null;
        post.NotBackgroundCapable = false;
        post.Comments = new List<SocialMediaPostSO.SocialMediaComment>();
        EditorUtility.SetDirty(post);
        return post;
    }

    private static void EnsureReplayQuestionLocalization()
    {
        var questions = new Dictionary<string, (string English, string Japanese)>
        {
            ["replay_ep27_math_path"] = ("Are you on the NTR path with Math or the NTS path with Leo?", "MathのNTRルートですか、それともLeoのNTSルートですか？"),
            ["replay_ep27_medicine"] = ("On the NTR path, did you refuse the doctor's medicine?", "NTRルートで、医師の薬を拒否しましたか？"),
            ["replay_ep27_leo_humiliation"] = ("On the NTS path, did you accept Leo's humiliating framing?", "NTSルートで、Leoの屈辱的な演出を受け入れましたか？"),
            ["replay_ep27_spicy_app"] = ("Should the Spicy Socials app be unlocked? Choose Yes for the NTR path.", "Spicy Socialsアプリをアンロックしますか？NTRルートでは「はい」を選んでください。"),
            ["replay_ep27_undress"] = ("Did Lily fully undress for Leo's second drawing class?", "LilyはLeoの2回目のデッサン教室で全裸になりましたか？"),
            ["replay_ep27_student_number"] = ("Did Lily keep Sem's number after the class?", "Lilyは教室の後、Semの番号を取っておきましたか？"),
            ["replay_ep275_sem_stays"] = ("Did you tell Leo that Sem could stay after the viewing?", "鑑賞会の後もSemが残ってよいとLeoに伝えましたか？")
        };

        var collection = LocalizationEditorSettings.GetStringTableCollection("Chapter Replay Questions");
        if (collection == null)
            throw new InvalidOperationException("Chapter Replay Questions localization collection was not found.");

        foreach (var pair in questions)
        {
            var shared = collection.SharedData.GetEntry(pair.Key) ?? collection.SharedData.AddKey(pair.Key);
            foreach (var table in collection.StringTables)
            {
                var localizedValue = table.LocaleIdentifier.Code.StartsWith("ja", StringComparison.OrdinalIgnoreCase)
                    ? pair.Value.Japanese
                    : pair.Value.English;
                var entry = table.GetEntry(shared.Id) ?? table.AddEntry(shared.Id, localizedValue);
                entry.Value = localizedValue;
                EditorUtility.SetDirty(table);
            }
        }

        EditorUtility.SetDirty(collection.SharedData);
    }

    private static void BuildEpisode27(DialogueContainerSO target)
    {
        var lines = File.ReadAllLines(Episode27Source);
        var b = new GraphBuilder(target, lines, EpisodeMediaMap27(), EpisodePostMap27());

        var ntr = b.Seq(
            b.Event("spicy_app_active", true),
            b.Linear(26, 28),
            b.If("Docter_Medicine_denied", b.Linear(38, 49), b.Linear(29, 36)),
            b.Linear(51, 62),
            b.End());

        var main = b.Seq(
            b.Linear(99, 109),
            b.If("undress_full_ep23", b.Linear(110, 112), b.Linear(114, 116)),
            b.Linear(118, 133),
            b.If("leo_humiliation", b.Linear(134, 139), b.Linear(141, 151)),
            b.Linear(153, 196),
            b.Choice("L", new[]
            {
                b.Branch(198, 199, 203),
                b.Branch(206, 207, 210)
            }),
            b.Linear(212, 226),
            b.Choice("L", new[]
            {
                b.Branch(228, 229, 230),
                b.Branch(233, 234, 236)
            }),
            b.Linear(238, 254),
            b.If("leo_humiliation", b.Linear(255, 263), b.Linear(265, 273)),
            b.Linear(275, 316),
            b.If("undress_full_ep23", b.Linear(317, 321), b.Linear(323, 327)),
            b.Linear(329, 367),
            b.If("leo_humiliation", b.Linear(368, 384), b.Linear(386, 393)),
            b.If("undress_full_ep23", b.Linear(395, 396), b.Linear(397, 398)),
            b.Linear(400, 417),
            b.If("undress_full_ep23", b.Linear(418, 422), b.Linear(424, 429)),
            b.Linear(431, 463),
            b.If("student_number",
                b.Seq(b.Linear(464, 479), b.Choice("L", new[]
                {
                    b.Branch(481, 482, 483), b.Branch(485, 486, 490)
                })),
                b.Seq(b.Linear(493, 499), b.Choice("L", new[]
                {
                    b.Branch(501, 502, 505), b.Branch(507, 508, 509)
                }))),
            b.Linear(512, 559),
            b.If("leo_humiliation", b.Linear(560, 569), b.Linear(570, 579)),
            b.Linear(581, 599),
            b.If("leo_humiliation", b.Linear(600, 610), b.Linear(611, 618)),
            b.Linear(620, 622),
            b.Choice("LEO", new[]
            {
                b.BranchWithEvent(624, "sem_stays", false, 625, 629,
                    "PATH A — Lily alone with Leo. You will not see Sem tonight."),
                b.BranchWithEvent(632, "sem_stays", true, 633, 636,
                    "PATH B — Sem stays. Leo and Sem both.")
            }),
            b.If("sem_stays", BuildEpisode27PathB(b), BuildEpisode27PathA(b))
        );

        var syncHumiliation = b.If("ep14_leo_images",
            b.Event("leo_humiliation", true), b.Event("leo_humiliation", false));
        var root = b.Seq(b.Start(), syncHumiliation, b.If("Continue_math", ntr, main));
        b.FinalizeGraph(root);
    }

    private static GraphBuilder.Flow BuildEpisode27PathA(GraphBuilder b)
    {
        return b.Seq(
            b.Linear(640, 646),
            b.Choice("L", new[]
            {
                b.Branch(648, 649, 652),
                b.Branch(654, 655, 657)
            }),
            b.Linear(658, 673),
            b.If("leo_humiliation",
                b.Choice("L", new[]
                {
                    b.Branch(676, 677, 684),
                    b.Branch(686, 687, 691)
                }),
                b.Choice("L", new[]
                {
                    b.Branch(695, 696, 699),
                    b.Branch(701, 702, 707)
                })),
            b.Linear(709, 726),
            b.Message("L", "I had to lie down for one minute before I go back out there."),
            b.Linear(727, 730),
            b.End()
        );
    }

    private static GraphBuilder.Flow BuildEpisode27PathB(GraphBuilder b)
    {
        return b.Seq(
            b.Linear(734, 753),
            b.Choice("L", new[]
            {
                b.Branch(755, 756, 760),
                b.Branch(762, 763, 765)
            }),
            b.Linear(767, 780),
            b.If("leo_humiliation", b.Linear(781, 791), b.Linear(793, 800)),
            b.Linear(802, 808),
            b.End()
        );
    }

    private static void BuildEpisode275(DialogueContainerSO target)
    {
        var lines = File.ReadAllLines(Episode275Source);
        var b = new GraphBuilder(target, lines, EpisodeMediaMap275(), EpisodePostMap275());

        var ntrOpen = b.Seq(b.Linear(35, 60), b.End());
        var ntrSecret = b.Seq(b.Linear(61, 70), b.End());

        var viewing = b.Seq(
            b.Linear(102, 117),
            b.If("undress_full_ep23",
                b.Seq(b.Linear(118, 124), b.Timelapse("L", "2 minutes later...."), b.Linear(125, 126)),
                b.Seq(b.Linear(127, 137), b.Timelapse("L", "2 minutes later...."), b.Linear(138, 139))),
            b.Linear(140, 149),
            b.If("leo_humiliation", b.Linear(150, 157), b.Linear(158, 162)),
            b.If("sem_stays",
                b.Seq(
                    b.Linear(215, 244),
                    b.If("leo_humiliation", b.Linear(245, 255), b.Linear(256, 265))),
                b.Seq(
                    b.Linear(165, 189),
                    b.If("leo_humiliation", b.Linear(190, 204), b.Linear(205, 213)))),
            b.Linear(267, 288),
            b.If("sem_stays", b.Linear(291, 292), b.Linear(289, 290)),
            b.Linear(293, 301),
            b.If("sem_stays", BuildEpisode275PathB(b), BuildEpisode275PathA(b))
        );

        var syncHumiliation = b.If("ep14_leo_images",
            b.Event("leo_humiliation", true), b.Event("leo_humiliation", false));
        var ntr = b.Seq(
            b.Event("spicy_app_active", true),
            b.If("Docter_Medicine_denied", ntrSecret, ntrOpen));
        var root = b.Seq(b.Start(), syncHumiliation, b.If("Continue_math", ntr, viewing));
        b.FinalizeGraph(root);
    }

    private static GraphBuilder.Flow BuildEpisode275PathA(GraphBuilder b)
    {
        var rawChoice = b.Choice("L", new[]
        {
            b.BranchWithEvent(352, "leo_raw", false, 353, 356, "Lily will remember"),
            b.BranchWithEvent(359, "leo_raw", true, 360, 369, "Lily will remember")
        });

        return b.Seq(
            b.Linear(304, 321),
            b.If("leo_humiliation", b.Linear(322, 329), b.Linear(331, 336)),
            b.Linear(338, 350),
            rawChoice,
            b.Linear(371, 421),
            b.If("leo_raw",
                b.Linear(422, 446),
                b.Seq(
                    b.Player("L", "And he used something"),
                    b.Linear(449, 453),
                    b.If("leo_humiliation", b.Linear(454, 455), b.Silent("L")))),
            b.Linear(459, 464),
            b.If("leo_humiliation", b.Linear(465, 484), b.Linear(486, 495)),
            b.Linear(497, 498),
            b.Timelapse("LEO", "Later..."),
            b.Linear(499, 501),
            b.If("leo_humiliation", b.Linear(502, 506), b.Linear(508, 516)),
            b.End()
        );
    }

    private static GraphBuilder.Flow BuildEpisode275PathB(GraphBuilder b)
    {
        var sendChoice = b.Choice("L", new[]
        {
            b.Branch(548, 549, 549),
            b.Branch(551, 552, 553)
        });
        var rawChoice = b.Choice("L", new[]
        {
            b.BranchWithEvent(615, "leo_raw", false, 616, 617, "Lily will remember"),
            b.BranchWithEvent(620, "leo_raw", true, 621, 628, "Lily will remember")
        });
        var humiliationClose = b.Seq(
            b.Linear(784, 790),
            b.Choice("L", new[]
            {
                b.Branch(792, 793, 794),
                b.Branch(796, 797, 798)
            }),
            b.Linear(799, 812));

        return b.Seq(
            b.Linear(522, 546),
            sendChoice,
            b.Linear(554, 562),
            b.If("student_number", b.Linear(563, 566), b.Linear(567, 569)),
            b.Linear(570, 575),
            b.If("leo_humiliation",
                b.Seq(b.Timelapse("LEO", "Later..."), b.Linear(576, 587)),
                b.Silent("LEO")),
            b.Linear(589, 613),
            rawChoice,
            b.Linear(629, 754),
            b.If("leo_raw", b.Linear(755, 766), b.Linear(767, 769)),
            b.Linear(771, 783),
            b.If("leo_humiliation", humiliationClose, b.Linear(813, 824)),
            b.Linear(825, 845),
            b.End()
        );
    }

    private static Dictionary<int, GraphBuilder.MediaSpec> EpisodeMediaMap27()
    {
        GraphBuilder.MediaSpec I(string path, bool noBackground = false) => new(Media + path, null, noBackground);
        return new Dictionary<int, GraphBuilder.MediaSpec>
        {
            [36] = I("ep27 NTR 1.png"), [44] = I("ep27 NTR 1.png"), [54] = I("ep27 NTR 2.png"),
            [104] = I("ep27 1 covered canvas.png"), [167] = I("ep27 2 lily morning selfie.png"),
            [222] = I("ep27 3 two outfits on the bed.png"), [240] = I("ep27 4 lily mirror selfie in the white set.png"),
            [244] = I("ep27 5 close low angle loly sitting back on the hotel bed.png"),
            [249] = I("ep27 6 lily in the blackcocktail dress.png"), [277] = I("ep27 7 lily in the back of the car.png"),
            [290] = I("ep27 8 leos studio set up for a viewing.png"), [298] = I("ep27 9 leo pouring 2 glasses of red wine.png"),
            [318] = I("ep27 10a lily groom behind on the painting fully naked.png"),
            [324] = I("ep27 10b lily groom behind on the painting fully naked.png"),
            [348] = I("ep27 11 lily from behind in the black dress looking at something off frame.png"),
            [365] = I("ep27 12 brass name plate NOT BACKGROUND CAPABLE.png", true),
            [410] = I("ep27 13 the private preview .jpg"), [513] = I("ep27 14 sems charcoal drawing.png"),
            [529] = I("ep27 15 sem and lily at the wine table.png"), [561] = I("ep27 16 lily taking sip over rim of her glass.png"),
            [571] = I("ep27 16 lily taking sip over rim of her glass.png"),
            [682] = I("ep27 A 17 lily in the back room, mouth slightly open.png"),
            [687] = I("ep27 A 17 lily in the back room, mouth slightly open.png"),
            [698] = I("ep27 A 17 lily in the back room, mouth slightly open.png"),
            [706] = I("ep27 A 17 lily in the back room, mouth slightly open.png"),
            [719] = I("ep27 A 19 lily in the mirror dress smoothed down prrof after plug entered you cant see the plug.png"),
            [727] = I("ep27 A 20 but she is already in his bed blushing.png"),
            [743] = I("ep27 B 17 lily in the backroom.png"), [804] = I("ep27 B 19 sem in the doorway in the backroom.png")
        };
    }

    private static Dictionary<int, string> EpisodePostMap27() => new() { [161] = "leo1" };

    private static Dictionary<int, GraphBuilder.MediaSpec> EpisodeMediaMap275()
    {
        GraphBuilder.MediaSpec I(string path, bool noBackground = false) => new(Media + path, null, noBackground);
        GraphBuilder.MediaSpec V(string path, string thumb) => new(Media + path, Media + thumb, false);
        return new Dictionary<int, GraphBuilder.MediaSpec>
        {
            [103] = I("ep27.5 21 wode shot of the full studio.png"),
            [125] = I("ep27.5 23a lily next to the paninting the painting is in the frame.png"),
            [138] = I("ep27.5 23b the painting is also in the frame.png"),
            [143] = I("ep27.5 22 lily and gerard mid conversation.png"),
            [178] = I("ep27.5 24 face slightly flushed.png"),
            [221] = I("ep27.5 24 Sem and Lily drinking wine close together BUT NOW THEY ARE STANDING OUTSIDE.png"),
            [278] = I("ep27.5 25 lilys own bathroom selfie mirror dress rumpled cheeks pink.png"),
            [294] = I("ep27.5 26 studio nearly empty.png"),
            [306] = I("ep27.5 27 sem leaving sketchbookunder his arm.png"),
            [376] = I("ep27.5 28 lily on her front in leo's bed.png"),
            [392] = I("ep27.5 29 leo and lily laying in bed video banner.jpg"),
            [396] = V("ep27.5 29.5 EXTRA VIDEO OF LILY AND LEO MAKING OUT.mp4", "ep27.5 29 leo and lily laying in bed video banner.jpg"),
            [407] = I("ep27.5 A 30 close up of face moaning.jpg"),
            [419] = V("Video/ep37.5 VIDEO 1.mp4", "Video/ep27.5 VIDEO 1 BANNER CHANGED TO A DOGGYSTYLE POSITION.png"),
            [440] = I("ep27.5 A EXTRA IMAGE LILY WITH CUM IN HER MOUTH.png"),
            [535] = I("ep27.5 26 sem across the studio glass in hand.jpg"),
            [598] = I("ep27.5 28 lilys face caught across the studio laughing.png"),
            [642] = I("ep27.5 29 lily on the count under the paints spattered dust sheet.png"),
            [651] = I("ep27.5 30 leo sitting in the chard YOU CANT SEE LILY OR SEM ONLY LEO.png"),
            [660] = I("ep27.5 31 sem and lily on the couch LILY IS ON SEMS LAP.jpg"),
            [669] = I("ep27.5 32 B lilys face eyes wide the exact second she finds out what she is accutaly dealing with NOW HAS SEMS COCK VISABLE.png"),
            [678] = I("ep27.5 33 B CHANGED ITS NOW AN IMAGE SUCKING SEMS COCK.png"),
            [715] = V("Video/ep27.5 VIDEO 2 .mp4", "Video/ep27.5 VIDEO 2 BANNER LILY GETTING FUCKED ON A BED EIFELTOWER BY SEM AND LEO.png"),
            [748] = I("ep27.5 A EXTRA IMAGE LILY WITH CUM IN HER MOUTH.png"),
            [777] = I("ep 27.5 35B sem in the kitchen mug in hand looking back.png"),
            [809] = I("ep27.5 36V lily selfie NOW IN LEO'S BED.png"),
            [819] = I("ep27.5 36V lily selfie NOW IN LEO'S BED.png")
        };
    }

    private static Dictionary<int, string> EpisodePostMap275() => new()
    {
        [38] = "math_spicy_open", [62] = "math_spicy_secret", [845] = "leo3"
    };

    private static void WireIntoGame(DialogueContainerSO episode27, DialogueContainerSO episode275)
    {
        var manager = Resources.FindObjectsOfTypeAll<DialogueChapterManager>()
            .FirstOrDefault(item => item.gameObject.scene.IsValid());
        if (manager == null)
            throw new InvalidOperationException("No active DialogueChapterManager was found. Open the main mobile scene first.");

        if (!manager.AllDialogueCharacters.Contains(Sem))
            manager.AllDialogueCharacters.Add(Sem);

        UpsertStory(manager.StoryList, episode27, 27, new List<DialogueChapterManager.ChapterData.ChapterReplaySetting>
        {
            Replay("replay_ep27_math_path", "Continue_math", "replay_answer_ntr", "true", "replay_answer_nts", "false"),
            ReplayYesNo("replay_ep27_spicy_app", "spicy_app_active"),
            ReplayYesNo("replay_ep27_medicine", "Docter_Medicine_denied"),
            ReplayYesNo("replay_ep27_leo_humiliation", "ep14_leo_images"),
            ReplayYesNo("replay_ep27_undress", "undress_full_ep23"),
            ReplayYesNo("replay_ep27_student_number", "student_number")
        });

        UpsertStory(manager.StoryList, episode275, 28, new List<DialogueChapterManager.ChapterData.ChapterReplaySetting>
        {
            Replay("replay_ep27_math_path", "Continue_math", "replay_answer_ntr", "true", "replay_answer_nts", "false"),
            ReplayYesNo("replay_ep27_spicy_app", "spicy_app_active"),
            ReplayYesNo("replay_ep27_medicine", "Docter_Medicine_denied"),
            ReplayYesNo("replay_ep27_leo_humiliation", "ep14_leo_images"),
            ReplayYesNo("replay_ep27_undress", "undress_full_ep23"),
            ReplayYesNo("replay_ep27_student_number", "student_number"),
            ReplayYesNo("replay_ep275_sem_stays", "sem_stays")
        });

        EditorUtility.SetDirty(manager);
        EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
        EditorSceneManager.SaveScene(manager.gameObject.scene);
    }

    private static void UpsertStory(List<DialogueChapterManager.ChapterData> stories, DialogueContainerSO story,
        int targetIndex, List<DialogueChapterManager.ChapterData.ChapterReplaySetting> replay)
    {
        var existingIndex = stories.FindIndex(item => item.Story == story);
        if (existingIndex >= 0 && existingIndex != targetIndex)
            stories.RemoveAt(existingIndex);
        while (stories.Count <= targetIndex)
            stories.Add(new DialogueChapterManager.ChapterData { ReplaySettings = new List<DialogueChapterManager.ChapterData.ChapterReplaySetting>() });

        stories[targetIndex] = new DialogueChapterManager.ChapterData
        {
            Story = story,
            StartID = string.Empty,
            ChapterIndex = targetIndex,
            IsStoryChapter = true,
            ReplaySettings = replay
        };
    }

    private static DialogueChapterManager.ChapterData.ChapterReplaySetting ReplayYesNo(string question, string valueName)
        => Replay(question, valueName, "replay_answer_yes", "true", "replay_answer_no", "false");

    private static DialogueChapterManager.ChapterData.ChapterReplaySetting Replay(string question, string valueName,
        string option1Key, string option1, string option2Key, string option2)
    {
        return new DialogueChapterManager.ChapterData.ChapterReplaySetting
        {
            DialogueTitleString = Localized("Chapter Replay Questions", question),
            Value = new GlobalValueSave { ValueName = valueName },
            Option1TextString = Localized("Chapter Replay Answers", option1Key),
            OptionSetting1 = option1,
            Option2TextString = Localized("Chapter Replay Answers", option2Key),
            OptionSetting2 = option2
        };
    }

    private static LocalizedString Localized(string table, string key) => new()
    {
        TableReference = table,
        TableEntryReference = key
    };

    private static List<LanguageGeneric<T>> Languages<T>(T english, T japanese) => new()
    {
        new LanguageGeneric<T> { languageEnum = LocalizationEnum.English, LanguageGenericType = english },
        new LanguageGeneric<T> { languageEnum = LocalizationEnum.Japanese, LanguageGenericType = japanese }
    };

    private static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null)
            return asset;
        asset = ScriptableObject.CreateInstance<T>();
        asset.name = Path.GetFileNameWithoutExtension(path);
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static T RequireAsset<T>(string path) where T : UnityEngine.Object
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
            throw new FileNotFoundException($"Required update asset was not found or imported as {typeof(T).Name}: {path}");
        return asset;
    }

    private static void EnsureFolder(string fullPath)
    {
        var parts = fullPath.Split('/');
        var current = parts[0];
        for (var i = 1; i < parts.Length; i++)
        {
            var next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    private sealed class GraphBuilder
    {
        internal readonly struct MediaSpec
        {
            public readonly string AssetPath;
            public readonly string ThumbnailPath;
            public readonly bool NotBackgroundCapable;

            public MediaSpec(string assetPath, string thumbnailPath, bool notBackgroundCapable)
            {
                AssetPath = assetPath;
                ThumbnailPath = thumbnailPath;
                NotBackgroundCapable = notBackgroundCapable;
            }
        }
        internal sealed class Flow
        {
            public string Entry;
            public List<string> Exits = new();
        }

        internal sealed class ChoiceBranch
        {
            public string Option;
            public string Hint;
            public Flow Content;
        }

        private readonly DialogueContainerSO _asset;
        private readonly string[] _lines;
        private readonly Dictionary<int, MediaSpec> _mediaByLine;
        private readonly Dictionary<int, string> _postByLine;
        private float _x;

        public GraphBuilder(DialogueContainerSO asset, string[] lines, Dictionary<int, MediaSpec> mediaByLine,
            Dictionary<int, string> postByLine)
        {
            _asset = asset;
            _lines = lines;
            _mediaByLine = mediaByLine;
            _postByLine = postByLine;
            _asset.AllowDialogueSave = false;
            _asset.BlockingReopeningDialogue = false;
            _asset.NodeLinkDatas = new List<NodeLinkData>();
            _asset.DialogueChoiceNodeDatas = new List<DialogueChoiceNodeData>();
            _asset.DialogueNodeDatas = new List<DialogueNodeData>();
            _asset.TimerChoiceNodeDatas = new List<TimerChoiceNodeData>();
            _asset.EndNodeDatas = new List<EndNodeData>();
            _asset.EventNodeDatas = new List<EventNodeData>();
            _asset.StartNodeDatas = new List<StartNodeData>();
            _asset.RandomNodeDatas = new List<RandomNodeData>();
            _asset.CommandNodeDatas = new List<CommandNodeData>();
            _asset.IfNodeDatas = new List<IfNodeData>();
        }

        public Flow Start()
        {
            var node = new StartNodeData { NodeGuid = Guid.NewGuid().ToString(), Position = Position(), startID = string.Empty };
            _asset.StartNodeDatas.Add(node);
            return Node(node.NodeGuid);
        }

        public Flow End()
        {
            var node = new EndNodeData
            {
                NodeGuid = Guid.NewGuid().ToString(), Position = Position(), EndNodeType = EndNodeType.End, Dialogue = null
            };
            _asset.EndNodeDatas.Add(node);
            return new Flow { Entry = node.NodeGuid, Exits = new List<string>() };
        }

        public Flow Silent(string characterCode) => Message(characterCode, string.Empty, 0f);

        public Flow Event(string valueName, bool value)
        {
            var node = new EventNodeData
            {
                NodeGuid = Guid.NewGuid().ToString(), Position = Position(),
                EventScriptableObjects = new List<EventScriptableObjectData>
                {
                    new() { DialogueEventSO = GetBoolEvent(valueName, value) }
                }
            };
            _asset.EventNodeDatas.Add(node);
            return Node(node.NodeGuid);
        }

        public Flow If(string valueName, Flow trueFlow, Flow falseFlow)
        {
            trueFlow ??= Silent("L");
            falseFlow ??= Silent("L");
            var node = new IfNodeData
            {
                NodeGuid = Guid.NewGuid().ToString(), Position = Position(), ValueName = valueName,
                Operations = GlobalValueIFOperations.Equal, OperationValue = string.Empty,
                TrueGUID = trueFlow.Entry, FalseGUID = falseFlow.Entry
            };
            _asset.IfNodeDatas.Add(node);
            Link(node.NodeGuid, trueFlow.Entry);
            Link(node.NodeGuid, falseFlow.Entry);
            return new Flow
            {
                Entry = node.NodeGuid,
                Exits = trueFlow.Exits.Concat(falseFlow.Exits).Distinct().ToList()
            };
        }

        public ChoiceBranch Branch(int optionLine, int contentStart, int contentEnd, string hint = "")
            => new() { Option = SpeakerText(optionLine), Hint = hint, Content = Linear(contentStart, contentEnd) };

        public ChoiceBranch BranchWithEvent(int optionLine, string valueName, bool value, int contentStart, int contentEnd,
            string hint = "")
            => new()
            {
                Option = SpeakerText(optionLine), Hint = hint,
                Content = Seq(Event(valueName, value), Linear(contentStart, contentEnd))
            };

        public Flow Choice(string partnerCode, IEnumerable<ChoiceBranch> branches)
        {
            var branchList = branches.ToList();
            var node = new DialogueChoiceNodeData
            {
                NodeGuid = Guid.NewGuid().ToString(), Position = Position(), DialogueNodePorts = new List<DialogueNodePort>(),
                AudioClips = AudioLanguages(), Character = Character(partnerCode), AvatarPos = AvatarPosition.None,
                AvatarType = AvatarType.Normal, TextType = TextLanguages(string.Empty), Duration = 2f,
                Delay = 0f, Timelapse = string.Empty, RequireCharacterInput = false,
                SelectedChoice = new List<LanguageGeneric<string>>()
            };

            foreach (var branch in branchList)
            {
                var portGuid = Guid.NewGuid().ToString();
                node.DialogueNodePorts.Add(new DialogueNodePort
                {
                    PortGuid = portGuid, InputGuid = branch.Content.Entry, OutputGuid = node.NodeGuid,
                    TextLanguage = TextLanguages(branch.Option),
                    HintLanguage = TextLanguages(branch.Hint ?? string.Empty)
                });
                Link(node.NodeGuid, branch.Content.Entry);
            }

            _asset.DialogueChoiceNodeDatas.Add(node);
            return new Flow
            {
                Entry = node.NodeGuid,
                Exits = branchList.SelectMany(branch => branch.Content.Exits).Distinct().ToList()
            };
        }

        public Flow Linear(int startLine, int endLine)
        {
            var flows = new List<Flow>();
            for (var lineNumber = startLine; lineNumber <= endLine && lineNumber <= _lines.Length; lineNumber++)
            {
                if (_postByLine.TryGetValue(lineNumber, out var postKey))
                {
                    flows.Add(Post(postKey));
                    continue;
                }

                var raw = _lines[lineNumber - 1].Trim();
                if (raw.Length == 0 || raw.StartsWith("if ", StringComparison.OrdinalIgnoreCase) ||
                    raw.StartsWith("else", StringComparison.OrdinalIgnoreCase) || raw.StartsWith("CHOICE") ||
                    raw.StartsWith("(Continue", StringComparison.OrdinalIgnoreCase) || raw.StartsWith("ACT ") ||
                    raw.StartsWith("PATH ") || raw.StartsWith("---") || raw.StartsWith("==="))
                    continue;

                var setMatch = Regex.Match(raw, @"^\(set\s+([A-Za-z0-9_.]+)\s*=\s*(true|false)\)", RegexOptions.IgnoreCase);
                if (setMatch.Success)
                {
                    flows.Add(Event(CanonicalVariable(setMatch.Groups[1].Value),
                        bool.Parse(setMatch.Groups[2].Value)));
                    continue;
                }

                if (raw.StartsWith("Timelaps:", StringComparison.OrdinalIgnoreCase))
                {
                    var value = raw.Substring(raw.IndexOf(':') + 1).Trim().Trim('(', ')');
                    flows.Add(Timelapse(PartnerForLine(lineNumber), value));
                    continue;
                }

                if (raw.StartsWith("(") && IsTimeMarker(raw))
                {
                    flows.Add(Timelapse(PartnerForLine(lineNumber), raw.Trim('(', ')')));
                    continue;
                }

                var speakerMatch = Regex.Match(raw, @"^(L|LEO|SEM|MA|M):\s*(.*)$");
                if (!speakerMatch.Success)
                {
                    if (_mediaByLine.TryGetValue(lineNumber, out var standaloneMedia))
                        flows.Add(Media(standaloneMedia, "L"));
                    continue;
                }

                var speaker = speakerMatch.Groups[1].Value;
                var text = speakerMatch.Groups[2].Value;
                if (_mediaByLine.TryGetValue(lineNumber, out var media))
                {
                    flows.Add(Media(media, speaker == "M" ? PartnerForLine(lineNumber) : speaker));
                    continue;
                }

                if (text.StartsWith("(Picture", StringComparison.OrdinalIgnoreCase) ||
                    text.StartsWith("(Video", StringComparison.OrdinalIgnoreCase))
                    continue;

                flows.Add(speaker == "M"
                    ? Player(PartnerForLine(lineNumber), text)
                    : Message(speaker, text));
            }

            return flows.Count == 0 ? Silent(PartnerForLine(startLine)) : Seq(flows.ToArray());
        }

        public Flow Message(string characterCode, string text, float duration = 2f)
        {
            var node = new DialogueNodeData
            {
                NodeGuid = Guid.NewGuid().ToString(), Position = Position(), DialogueNodePorts = new List<DialogueNodePort>(),
                AudioClips = AudioLanguages(), Character = Character(characterCode), AvatarPos = AvatarPosition.None,
                AvatarType = AvatarType.Normal, Texts = TextLanguages(text), Timelapses = TextLanguages(string.Empty),
                Timelapse = string.Empty, Duration = duration, Delay = 0f, MediaType = MediaType.Sprite,
                Image = null, Video = null, VideoThumbnail = null, NotBackgroundCapable = false,
                GalleryVisibility = GalleryDisplay.Display, Post = null, DelayTimer = 0f
            };
            _asset.DialogueNodeDatas.Add(node);
            return Node(node.NodeGuid);
        }

        public Flow Player(string partnerCode, string text)
        {
            var branch = new ChoiceBranch { Option = text, Hint = string.Empty, Content = Silent(partnerCode) };
            var choice = Choice(partnerCode, new[] { branch });
            // The silent continuation exists only to give the one-port choice a valid target.
            return choice;
        }

        public Flow Timelapse(string characterCode, string text)
        {
            var flow = Message(characterCode, string.Empty, 0f);
            var node = _asset.DialogueNodeDatas.First(item => item.NodeGuid == flow.Entry);
            node.Timelapses = TextLanguages(text);
            return flow;
        }

        public Flow Post(string postKey)
        {
            if (!Posts.TryGetValue(postKey, out var post))
                throw new KeyNotFoundException($"Unknown update social post key: {postKey}");
            var flow = Message(post.Character == Leo ? "LEO" : post.Character == MathLily ? "MA" : "MA", string.Empty, 0f);
            var node = _asset.DialogueNodeDatas.First(item => item.NodeGuid == flow.Entry);
            node.Character = post.Character;
            node.Post = post;
            return flow;
        }

        public Flow MediaImage(string path, string characterCode, bool noBackground = false)
            => Media(new MediaSpec(path, null, noBackground), characterCode);

        public Flow MediaVideo(string path, string thumbnail, string characterCode)
            => Media(new MediaSpec(path, thumbnail, false), characterCode);

        private Flow Media(MediaSpec spec, string characterCode)
        {
            var flow = Message(characterCode, string.Empty, 0f);
            var node = _asset.DialogueNodeDatas.First(item => item.NodeGuid == flow.Entry);
            node.NotBackgroundCapable = spec.NotBackgroundCapable;
            if (string.IsNullOrEmpty(spec.ThumbnailPath))
            {
                node.MediaType = MediaType.Sprite;
                node.Image = RequireAsset<Sprite>(spec.AssetPath);
            }
            else
            {
                node.MediaType = MediaType.Video;
                node.Video = RequireAsset<VideoClip>(spec.AssetPath);
                node.VideoThumbnail = RequireAsset<Sprite>(spec.ThumbnailPath);
            }
            return flow;
        }

        public Flow Seq(params Flow[] flows)
        {
            var valid = flows.Where(flow => flow != null && !string.IsNullOrEmpty(flow.Entry)).ToList();
            if (valid.Count == 0)
                return Silent("L");
            for (var i = 0; i < valid.Count - 1; i++)
            {
                foreach (var exit in valid[i].Exits)
                    Link(exit, valid[i + 1].Entry);
            }
            return new Flow { Entry = valid[0].Entry, Exits = valid[^1].Exits.ToList() };
        }

        public void FinalizeGraph(Flow root)
        {
            if (root == null || string.IsNullOrEmpty(root.Entry))
                throw new InvalidOperationException("Episode graph has no entry node.");

            var allGuids = new HashSet<string>(
                _asset.StartNodeDatas.Select(node => node.NodeGuid)
                    .Concat(_asset.EndNodeDatas.Select(node => node.NodeGuid))
                    .Concat(_asset.DialogueNodeDatas.Select(node => node.NodeGuid))
                    .Concat(_asset.DialogueChoiceNodeDatas.Select(node => node.NodeGuid))
                    .Concat(_asset.EventNodeDatas.Select(node => node.NodeGuid))
                    .Concat(_asset.IfNodeDatas.Select(node => node.NodeGuid)));

            if (_asset.StartNodeDatas.Count != 1 || _asset.StartNodeDatas[0].NodeGuid != root.Entry)
                throw new InvalidOperationException("Episode graph must have exactly one root start node.");

            foreach (var link in _asset.NodeLinkDatas)
            {
                if (!allGuids.Contains(link.BaseNodeGuid) || !allGuids.Contains(link.TargetNodeGuid))
                    throw new InvalidOperationException($"Episode graph contains a broken link: {link.BaseNodeGuid} -> {link.TargetNodeGuid}");
            }

            var outgoing = _asset.NodeLinkDatas
                .GroupBy(link => link.BaseNodeGuid)
                .ToDictionary(group => group.Key, group => group.Select(link => link.TargetNodeGuid).ToList());
            var visited = new HashSet<string>();
            var pending = new Stack<string>();
            pending.Push(root.Entry);
            while (pending.Count > 0)
            {
                var guid = pending.Pop();
                if (!visited.Add(guid) || !outgoing.TryGetValue(guid, out var targets))
                    continue;
                foreach (var target in targets)
                    pending.Push(target);
            }

            var unreachable = allGuids.Where(guid => !visited.Contains(guid)).ToList();
            if (unreachable.Count > 0)
                throw new InvalidOperationException($"Episode graph contains {unreachable.Count} unreachable nodes.");

            var endGuids = new HashSet<string>(_asset.EndNodeDatas.Select(node => node.NodeGuid));
            var invalidLeaves = visited.Where(guid => !outgoing.ContainsKey(guid) && !endGuids.Contains(guid)).ToList();
            if (invalidLeaves.Count > 0)
                throw new InvalidOperationException($"Episode graph contains {invalidLeaves.Count} unfinished paths.");

            EditorUtility.SetDirty(_asset);
        }

        private string SpeakerText(int lineNumber)
        {
            var raw = _lines[lineNumber - 1].Trim();
            var match = Regex.Match(raw, @"^(?:L|LEO|SEM|MA|M):\s*(.*)$");
            if (!match.Success)
                throw new InvalidDataException($"Expected a speaker line at {lineNumber}: {raw}");
            return match.Groups[1].Value;
        }

        private string PartnerForLine(int lineNumber)
        {
            for (var index = System.Math.Min(lineNumber - 2, _lines.Length - 1); index >= 0; index--)
            {
                var raw = _lines[index].Trim();
                if (raw.Contains("new number texts", StringComparison.OrdinalIgnoreCase)) return "SEM";
                if (raw.Contains("You text Leo", StringComparison.OrdinalIgnoreCase) ||
                    raw.Contains("Leo texts", StringComparison.OrdinalIgnoreCase)) return "LEO";
                if (raw.Contains("Math texts", StringComparison.OrdinalIgnoreCase)) return "MA";
                if (raw.Contains("Lily texts", StringComparison.OrdinalIgnoreCase)) return "L";
                var match = Regex.Match(raw, @"^(L|LEO|SEM|MA):");
                if (match.Success) return match.Groups[1].Value;
            }
            return "L";
        }

        private DialogueCharacterSO Character(string code) => code switch
        {
            "LEO" => Leo,
            "SEM" => Sem,
            "MA" => Math,
            _ => Lily
        };

        private static string CanonicalVariable(string source) => source switch
        {
            "math_continue" => "Continue_math",
            "docter_medicine_denied" => "Docter_Medicine_denied",
            _ => source
        };

        private static bool IsTimeMarker(string raw) =>
            Regex.IsMatch(raw, @"^\((About|A few|A little|Later|Early|That evening|The next|In the afternoon|[0-9]+\s+(minute|minutes|hour|hours))", RegexOptions.IgnoreCase);

        private Flow Node(string guid) => new() { Entry = guid, Exits = new List<string> { guid } };

        private Vector2 Position()
        {
            _x += 350f;
            return new Vector2(_x, 0f);
        }

        private void Link(string from, string to)
        {
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to)) return;
            if (_asset.NodeLinkDatas.Any(link => link.BaseNodeGuid == from && link.TargetNodeGuid == to)) return;
            _asset.NodeLinkDatas.Add(new NodeLinkData { BaseNodeGuid = from, TargetNodeGuid = to });
        }

        private static List<LanguageGeneric<AudioClip>> AudioLanguages() => new()
        {
            new LanguageGeneric<AudioClip> { languageEnum = LocalizationEnum.English, LanguageGenericType = null },
            new LanguageGeneric<AudioClip> { languageEnum = LocalizationEnum.Japanese, LanguageGenericType = null }
        };
    }
}
#endif
