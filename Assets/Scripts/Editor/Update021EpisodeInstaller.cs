#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
/// Rebuildable importer for the 0.21 story update. The supplied Episode 28 and
/// 28.5 scripts remain the source of truth; this importer builds both branching
/// Meet & Talk graphs, variables, replay setup, localization and story order.
/// </summary>
public static class Update021EpisodeInstaller
{
    private const string Root = "Assets/Entities/Dialogue/ep28 and ep28.5";
    private const string Images = Root + "/Images/";
    private const string Videos = Root + "/Videos/";
    private const string Episode28Source = Root + "/NTS HM EP 28.txt";
    private const string Episode285Source = Root + "/NTS HM EP 28.5.txt";
    private const string JapaneseSource = Root + "/Update 0.21 Japanese.tsv";
    private const string Episode28Asset = "Assets/Entities/Dialogue/Episode 28.asset";
    private const string Episode285Asset = "Assets/Entities/Dialogue/Episode 28.5.asset";
    private const string VariablesFolder = "Assets/Entities/Variables/Update 0.21";

    private static DialogueCharacterSO Lily;
    private static DialogueCharacterSO Lisa;
    private static DialogueCharacterSO Dave;
    private static readonly Dictionary<string, string> JapaneseByEnglish = new();

    [MenuItem("Tools/NTS/Install Update 0.21 Episodes")]
    public static void Install()
    {
        EnsureFolder(VariablesFolder);
        LoadCharacters();
        LoadJapaneseTranslations();
        EnsureVariables();
        EnsureReplayQuestionLocalization();

        var episode28 = LoadOrCreate<DialogueContainerSO>(Episode28Asset);
        episode28.name = "Episode 28";
        BuildEpisode28(episode28);

        var episode285 = LoadOrCreate<DialogueContainerSO>(Episode285Asset);
        episode285.name = "Episode 28.5";
        BuildEpisode285(episode285);

        WireIntoGame(episode28, episode285);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        ValidateEpisode(episode28, Episode28Source, Episode28MediaMap().Count);
        ValidateEpisode(episode285, Episode285Source, Episode285MediaMap().Count);
        Debug.Log("[Update 0.21] Episodes 28 and 28.5 installed and validated with all branches, media, variables, replay questions and Japanese localization.");
    }

    private static void LoadCharacters()
    {
        Lily = RequireAsset<DialogueCharacterSO>("Assets/Entities/Characters/Lily.asset");
        Lisa = RequireAsset<DialogueCharacterSO>("Assets/Entities/Characters/Lisa.asset");
        Dave = RequireAsset<DialogueCharacterSO>("Assets/Entities/Characters/Dave(bro).asset");
    }

    private static void LoadJapaneseTranslations()
    {
        JapaneseByEnglish.Clear();
        if (!File.Exists(JapaneseSource))
            throw new FileNotFoundException("Update 0.21 Japanese translation file is missing.", JapaneseSource);

        var lineNumber = 0;
        foreach (var rawLine in File.ReadLines(JapaneseSource, Encoding.UTF8))
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(rawLine) || rawLine.StartsWith("#", StringComparison.Ordinal))
                continue;

            var columns = rawLine.Split('\t');
            if (columns.Length != 2)
                throw new InvalidDataException($"Malformed Japanese translation row {lineNumber}.");

            try
            {
                var english = Encoding.UTF8.GetString(Convert.FromBase64String(columns[0]));
                var japanese = Encoding.UTF8.GetString(Convert.FromBase64String(columns[1]));
                JapaneseByEnglish[english] = japanese;
            }
            catch (FormatException exception)
            {
                throw new InvalidDataException($"Invalid base64 in Japanese translation row {lineNumber}.", exception);
            }
        }
    }

    private static void EnsureVariables()
    {
        var manager = RequireAsset<GlobalValueManager>("Assets/Meet and Talk/Resources/GlobalValue.asset");
        foreach (var name in new[] { "ep28_pack_lingerie", "ep28_dave_raw", "ep28_join_shower" })
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

    private static void EnsureReplayQuestionLocalization()
    {
        var questions = new Dictionary<string, (string English, string Japanese)>
        {
            ["replay_ep28_date"] = ("Did Lily accept the date with Dave?", "LilyはDaveとのデートを受け入れましたか？"),
            ["replay_ep28_math_path"] = ("Are you on the NTR path with Math or the NTS path with Leo?", "MathのNTRルートですか、それともLeoのNTSルートですか？"),
            ["replay_ep28_medicine"] = ("On the NTR path, did you refuse the doctor's medicine?", "NTRルートで、医師の薬を拒否しましたか？"),
            ["replay_ep28_boat_dave"] = ("Did Lily have the encounter with Dave on the boat?", "船でLilyとDaveの出来事はありましたか？"),
            ["replay_ep28_boat_lisa"] = ("Did Lily join Lisa below deck?", "Lilyは船内でLisaと一緒になりましたか？"),
            ["replay_ep28_leo_raw"] = ("Did Leo go without a condom in Episode 27.5?", "エピソード27.5でLeoはコンドームを使いませんでしたか？"),
            ["replay_ep28_lingerie"] = ("Did you tell Lily to pack the lingerie?", "Lilyにランジェリーを持っていくよう伝えましたか？")
        };

        var collection = LocalizationEditorSettings.GetStringTableCollection("Chapter Replay Questions");
        if (collection == null)
            throw new InvalidOperationException("Chapter Replay Questions localization collection was not found.");

        foreach (var pair in questions)
        {
            var shared = collection.SharedData.GetEntry(pair.Key) ?? collection.SharedData.AddKey(pair.Key);
            foreach (var table in collection.StringTables)
            {
                var value = table.LocaleIdentifier.Code.StartsWith("ja", StringComparison.OrdinalIgnoreCase)
                    ? pair.Value.Japanese
                    : pair.Value.English;
                var entry = table.GetEntry(shared.Id) ?? table.AddEntry(shared.Id, value);
                entry.Value = value;
                EditorUtility.SetDirty(table);
            }
        }

        EditorUtility.SetDirty(collection.SharedData);
    }

    private static void BuildEpisode28(DialogueContainerSO target)
    {
        var lines = File.ReadAllLines(Episode28Source, Encoding.UTF8);
        var b = new GraphBuilder(target, lines, Episode28MediaMap());

        var lunchRoute = b.If("Continue_math",
            b.If("Docter_Medicine_denied", b.Linear(557, 559), b.Linear(551, 553)),
            b.Linear(545, 547));

        var dateRoute = b.Seq(
            b.Linear(14, 20),
            b.If("Continue_math", b.Silent("L"), b.Linear(23, 24)),
            b.Linear(28, 37),
            b.If("ep24.5_lily_dave_blowjob", b.Linear(41, 43), b.Linear(47, 48)),
            b.Linear(52, 143),
            b.If("ep24.5_lily_dave_blowjob", b.Linear(147, 149), b.Linear(153, 155)),
            b.Linear(159, 226),
            b.Choice("L", new[]
            {
                b.BranchWithEvent(230, "ep28_pack_lingerie", true, 231, 231),
                b.BranchWithEvent(235, "ep28_pack_lingerie", false, 236, 236)
            }),
            b.Linear(239, 477),
            b.If("ep24.5_lily_dave_blowjob", b.Linear(480, 481), b.Silent("L")),
            b.Linear(487, 541),
            lunchRoute,
            b.Linear(563, 771),
            b.End());

        var noDateRoute = b.Seq(b.Linear(784, 853), b.End());
        var root = b.Seq(b.Start(), b.If("dave_lily_pending", dateRoute, noDateRoute));
        b.FinalizeGraph(root);
        EditorUtility.SetDirty(target);
    }

    private static void BuildEpisode285(DialogueContainerSO target)
    {
        var lines = File.ReadAllLines(Episode285Source, Encoding.UTF8);
        var b = new GraphBuilder(target, lines, Episode285MediaMap());

        var miaMemory = b.If("ep24.5_lily_dave_blowjob", b.Linear(41, 41),
            b.If("ep24.5_lily_join_lisa", b.Linear(45, 46), b.Silent("LI")));

        var knownBathroomMemory = b.Linear(79, 81);
        var bathroomMemory = b.If("Continue_math",
            b.If("Docter_Medicine_denied", b.Linear(70, 75), knownBathroomMemory),
            knownBathroomMemory);

        var ntsProtection = b.Seq(
            b.Linear(173, 180),
            b.If("leo_raw", b.Linear(184, 184), b.Linear(188, 188)),
            b.Linear(192, 192),
            b.Choice("L", new[]
            {
                b.BranchWithEvent(197, "ep28_dave_raw", false, 198, 198, "Lily will remember"),
                b.BranchWithEvent(203, "ep28_dave_raw", true, 204, 204, "Lily will remember")
            }));

        var ntrProtection = b.If("Docter_Medicine_denied",
            b.Seq(b.Event("ep28_dave_raw", false), b.Linear(210, 212)),
            b.Seq(b.Event("ep28_dave_raw", true), b.Linear(218, 220)));

        var joinShower = b.Seq(
            b.Linear(595, 604),
            b.If("ep28_dave_raw", b.Linear(607, 638), b.Linear(642, 671)));

        var dateRoute = b.Seq(
            b.Linear(14, 37),
            miaMemory,
            b.Linear(50, 67),
            bathroomMemory,
            b.Linear(85, 169),
            b.If("Continue_math", ntrProtection, ntsProtection),
            b.Linear(224, 226),
            b.If("ep28_pack_lingerie", b.Linear(229, 241), b.Linear(245, 257)),
            b.Linear(261, 268),
            b.If("ep28_pack_lingerie", b.Linear(271, 273), b.Linear(277, 279)),
            b.Linear(283, 309),
            b.Choice("LI", new[]
            {
                b.Branch(314, 315, 322),
                b.Branch(327, 328, 329)
            }),
            b.Linear(333, 343),
            b.Choice("LI", new[]
            {
                b.Branch(347, 348, 349),
                b.Branch(354, 355, 356)
            }),
            b.Linear(360, 366),
            b.If("ep28_pack_lingerie", b.Linear(369, 371), b.Linear(375, 376)),
            b.Linear(380, 387),
            b.Choice("LI", new[]
            {
                b.Branch(392, 393, 394),
                b.Branch(399, 400, 401)
            }),
            b.Linear(405, 425),
            b.If("ep28_dave_raw", b.Linear(428, 428), b.Linear(432, 432)),
            b.Linear(436, 474),
            b.If("ep28_dave_raw", b.Linear(477, 477), b.Linear(481, 481)),
            b.Linear(485, 510),
            b.If("ep28_dave_raw", b.Linear(513, 513), b.Linear(517, 517)),
            b.Linear(521, 550),
            b.If("ep28_dave_raw", b.Linear(553, 562), b.Linear(566, 567)),
            b.Choice("L", new[]
            {
                b.BranchWithEvent(574, "ep28_join_shower", true, 575, 576, "Lily will remember"),
                b.BranchWithEvent(581, "ep28_join_shower", false, 582, 588, "Lily will remember")
            }),
            b.If("ep28_join_shower", joinShower, b.Silent("L")),
            b.Linear(680, 701),
            b.End());

        var root = b.Seq(b.Start(), b.If("dave_lily_pending", dateRoute, b.End()));
        b.FinalizeGraph(root);
        EditorUtility.SetDirty(target);
    }

    private static Dictionary<int, GraphBuilder.MediaSpec> Episode28MediaMap()
    {
        GraphBuilder.MediaSpec I(string file) => new(Images + file, null, false);
        GraphBuilder.MediaSpec V(string file) => new(Videos + file, Videos + Path.GetFileNameWithoutExtension(file) + " banner.png", false);
        return new Dictionary<int, GraphBuilder.MediaSpec>
        {
            [115] = I("ep28 image 01 lisa teasing selfie.png"),
            [208] = I("ep28 image 02 packing outfits.png"),
            [225] = I("ep28 image 03 pink lingerie set.png"),
            [267] = I("ep28 image 05 izzy pillow throw.png"),
            [279] = I("ep28 image 04 sunrise breakfast.png"),
            [307] = I("ep28 image 06 breakfast rear view.png"),
            [334] = I("ep28 image 07A lily topless changing selfie.png"),
            [341] = I("ep28 image 07 lily bikini mirror selfie.png"),
            [365] = I("ep28 image 08 lily underwater blue fish.png"),
            [388] = I("ep28 image 09 lily on dave shoulders.png"),
            [428] = I("ep28 image 10 scooter before hike.png"),
            [465] = I("ep28 image 11 waterfall couple selfie.png"),
            [497] = I("ep28 image 12 exhausted lisa trail selfie.png"),
            [511] = I("ep28 image 13 waterfall kiss hand on ass.png"),
            [533] = I("ep28 image 14 lunch group waiter photo.png"),
            [580] = I("ep28 image 15 lily pulls lisa waterfall.png"),
            [589] = I("ep28 image 16 topless under waterfall.png"),
            [615] = I("ep28 image 17 post waterfall lily lisa selfie.png"),
            [655] = I("ep28 image 18 private sunset dance lesson.png"),
            [663] = V("ep28 video 19 sunset dance.mp4"),
            [673] = I("ep28 image 20 close dance face hold.png"),
            [722] = I("ep28 image 21 lily angry fist closeup.png"),
            [746] = I("ep28 image 22 underwater dome arrival.png"),
            [795] = I("ep28 image 46 poolside lily izzy selfie.png"),
            [802] = I("ep28 image 47 izzy splashes lily.png"),
            [819] = I("ep28 image 48 pool table group drinks.png"),
            [840] = I("ep28 image 49 relaxed group dinner.png")
        };
    }

    private static Dictionary<int, GraphBuilder.MediaSpec> Episode285MediaMap()
    {
        GraphBuilder.MediaSpec I(string file) => new(Images + file, null, false);
        GraphBuilder.MediaSpec V(string file) => new(Videos + file, Videos + Path.GetFileNameWithoutExtension(file) + " banner.png", false);
        return new Dictionary<int, GraphBuilder.MediaSpec>
        {
            [20] = I("ep28.5 image 23 lisa solo table view.png"),
            [25] = I("ep28.5 image 24 lily feeds dave.png"),
            [37] = I("ep28.5 image 25 mia and husband enter dome.png"),
            [64] = I("ep28.5 image 26 lily scared bathroom selfie.png"),
            [164] = I("ep28.5 image 27 hotel suite arrival.png"),
            [236] = I("ep28.5 image 28 pink lingerie bed selfie.png"),
            [252] = I("ep28.5 image 29 robe bed selfie.png"),
            [271] = I("ep28.5 image 30 fixed camera lingerie reveal.png"),
            [277] = I("ep28.5 image 31 fixed camera robe reveal.png"),
            [318] = I("ep28.5 image 32 begging reward blocked view.png"),
            [363] = V("ep28.5 video 33 fixed camera oral.mp4"),
            [428] = I("ep28.5 image 34 and 35 shared rear entry still.png"),
            [432] = I("ep28.5 image 34 and 35 shared rear entry still.png"),
            [448] = I("ep28.5 image 36 lisa touching herself selfie.png"),
            [477] = V("ep28.5 video 37 raw rear entry.mp4"),
            [481] = V("ep28.5 video 38 condom rear entry.mp4"),
            [513] = I("ep28.5 image 39 raw finish pov.png"),
            [517] = I("ep28.5 image 39 condom finish pov.png"),
            [617] = I("ep28.5 image 40 shower wall sex still.png"),
            [632] = I("ep28.5 image 41 raw aftermath closeup.png"),
            [665] = V("ep28.5 video 43 lily lisa facial.mp4"),
            [668] = I("ep28.5 image 44 lily lisa bathroom aftermath selfie.png"),
            [690] = I("ep28.5 image 45 lisa photo sleeping together.png")
        };
    }

    private static void WireIntoGame(DialogueContainerSO episode28, DialogueContainerSO episode285)
    {
        var manager = Resources.FindObjectsOfTypeAll<DialogueChapterManager>()
            .FirstOrDefault(item => item.gameObject.scene.IsValid());
        if (manager == null)
            throw new InvalidOperationException("No active DialogueChapterManager was found. Open the main mobile scene first.");

        foreach (var character in new[] { Lily, Lisa, Dave })
            if (!manager.AllDialogueCharacters.Contains(character))
                manager.AllDialogueCharacters.Add(character);

        UpsertStory(manager.StoryList, episode28, 29, new List<DialogueChapterManager.ChapterData.ChapterReplaySetting>
        {
            ReplayYesNo("replay_ep28_date", "dave_lily_pending"),
            Replay("replay_ep28_math_path", "Continue_math", "replay_answer_ntr", "true", "replay_answer_nts", "false"),
            ReplayYesNo("replay_ep28_medicine", "Docter_Medicine_denied"),
            ReplayYesNo("replay_ep28_boat_dave", "ep24.5_lily_dave_blowjob")
        });

        UpsertStory(manager.StoryList, episode285, 30, new List<DialogueChapterManager.ChapterData.ChapterReplaySetting>
        {
            ReplayYesNo("replay_ep28_date", "dave_lily_pending"),
            Replay("replay_ep28_math_path", "Continue_math", "replay_answer_ntr", "true", "replay_answer_nts", "false"),
            ReplayYesNo("replay_ep28_medicine", "Docter_Medicine_denied"),
            ReplayYesNo("replay_ep28_boat_dave", "ep24.5_lily_dave_blowjob"),
            ReplayYesNo("replay_ep28_boat_lisa", "ep24.5_lily_join_lisa"),
            ReplayYesNo("replay_ep28_leo_raw", "leo_raw"),
            ReplayYesNo("replay_ep28_lingerie", "ep28_pack_lingerie")
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

    private static void ValidateEpisode(DialogueContainerSO chapter, string sourcePath, int expectedMedia)
    {
        if (chapter.StartNodeDatas.Count != 1 || chapter.EndNodeDatas.Count < 1)
            throw new InvalidOperationException($"{chapter.name} start/end validation failed.");

        var actualMedia = chapter.DialogueNodeDatas.Count(node => node.Image != null || node.Video != null);
        if (actualMedia != expectedMedia)
            throw new InvalidOperationException($"{chapter.name} media validation failed: expected {expectedMedia}, found {actualMedia}.");

        var sourceTexts = File.ReadAllLines(sourcePath, Encoding.UTF8)
            .Select(line => Regex.Match(line.Trim(), @"^(?:D|L|LI|M):\s*(.*)$"))
            .Where(match => match.Success && !IsMediaText(match.Groups[1].Value))
            .Select(match => match.Groups[1].Value)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToList();

        var graphTexts = chapter.DialogueNodeDatas
            .SelectMany(node => node.Texts ?? new List<LanguageGeneric<string>>())
            .Where(value => value.languageEnum == LocalizationEnum.English && !string.IsNullOrEmpty(value.LanguageGenericType))
            .Select(value => value.LanguageGenericType)
            .Concat(chapter.DialogueChoiceNodeDatas
                .SelectMany(node => node.DialogueNodePorts ?? new List<DialogueNodePort>())
                .SelectMany(port => port.TextLanguage ?? new List<LanguageGeneric<string>>())
                .Where(value => value.languageEnum == LocalizationEnum.English && !string.IsNullOrEmpty(value.LanguageGenericType))
                .Select(value => value.LanguageGenericType))
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToList();

        if (!sourceTexts.SequenceEqual(graphTexts))
            throw new InvalidOperationException($"{chapter.name} text validation failed: source={sourceTexts.Count}, graph={graphTexts.Count}.");

        var sourceTimeMarkers = File.ReadAllLines(sourcePath, Encoding.UTF8)
            .Select(line => line.Trim())
            .Where(line => Regex.IsMatch(line, @"^\(.+\.\.\.\)$"))
            .Select(line => line.Substring(1, line.Length - 2))
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToList();
        var graphTimeMarkers = chapter.DialogueNodeDatas
            .SelectMany(node => node.Timelapses ?? new List<LanguageGeneric<string>>())
            .Where(value => value.languageEnum == LocalizationEnum.English && !string.IsNullOrEmpty(value.LanguageGenericType))
            .Select(value => value.LanguageGenericType)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToList();
        if (!sourceTimeMarkers.SequenceEqual(graphTimeMarkers))
            throw new InvalidOperationException($"{chapter.name} timestamp validation failed: source={sourceTimeMarkers.Count}, graph={graphTimeMarkers.Count}.");

        var localizedEntries = chapter.DialogueNodeDatas
            .SelectMany(node => node.Texts.Concat(node.Timelapses))
            .Concat(chapter.DialogueChoiceNodeDatas.SelectMany(node => node.DialogueNodePorts)
                .SelectMany(port => port.TextLanguage.Concat(port.HintLanguage)))
            .Where(value => value.languageEnum == LocalizationEnum.Japanese)
            .Select(value => value.LanguageGenericType)
            .Where(value => !string.IsNullOrEmpty(value))
            .ToList();
        if (localizedEntries.Any(value => Regex.IsMatch(value, @"[A-Za-z]") && !Regex.IsMatch(value, @"[\u3040-\u30ff\u3400-\u9fff]")))
            throw new InvalidOperationException($"{chapter.name} contains an untranslated Japanese entry.");
    }

    private static bool IsMediaText(string value)
    {
        var text = value.TrimStart();
        return text.StartsWith("(Sends ", StringComparison.OrdinalIgnoreCase) ||
               text.StartsWith("(Forwards ", StringComparison.OrdinalIgnoreCase);
    }

    private static List<LanguageGeneric<string>> TextLanguages(string english)
    {
        var japanese = string.Empty;
        if (!string.IsNullOrEmpty(english))
        {
            if (english == "Lily will remember")
                japanese = "Lilyは覚えているでしょう";
            else if (!JapaneseByEnglish.TryGetValue(english, out japanese) || string.IsNullOrEmpty(japanese))
                throw new KeyNotFoundException($"Missing Japanese translation for: {english}");
        }

        return new List<LanguageGeneric<string>>
        {
            new() { languageEnum = LocalizationEnum.English, LanguageGenericType = english },
            new() { languageEnum = LocalizationEnum.Japanese, LanguageGenericType = japanese }
        };
    }

    private static List<LanguageGeneric<AudioClip>> AudioLanguages() => new()
    {
        new() { languageEnum = LocalizationEnum.English, LanguageGenericType = null },
        new() { languageEnum = LocalizationEnum.Japanese, LanguageGenericType = null }
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

        private static readonly HashSet<string> TimeMarkers = new(StringComparer.OrdinalIgnoreCase)
        {
            "In the afternoon...", "A few minutes later...", "Shortly after...", "Later that afternoon...",
            "Late that afternoon...",
            "That evening...", "Early the next morning...", "A little later...", "Mid-morning...",
            "Later in the morning...", "Late in the morning...", "Near midday...", "Around lunchtime...",
            "After lunch...", "Late in the afternoon...", "Later that evening...", "Near the end of dinner...",
            "Ten minutes later...", "Five minutes later...", "Two minutes later...", "A minute later...",
            "About forty minutes later..."
        };

        private readonly DialogueContainerSO _asset;
        private readonly string[] _lines;
        private readonly Dictionary<int, MediaSpec> _mediaByLine;
        private float _x;

        public GraphBuilder(DialogueContainerSO asset, string[] lines, Dictionary<int, MediaSpec> mediaByLine)
        {
            _asset = asset;
            _lines = lines;
            _mediaByLine = mediaByLine;
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
                    TextLanguage = TextLanguages(branch.Option), HintLanguage = TextLanguages(branch.Hint ?? string.Empty)
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
                var raw = _lines[lineNumber - 1].Trim();
                if (raw.Length == 0)
                    continue;

                if (raw.StartsWith("(") && raw.EndsWith(")") && TimeMarkers.Contains(raw.Substring(1, raw.Length - 2)))
                {
                    flows.Add(Timelapse(PartnerForLine(lineNumber), raw.Substring(1, raw.Length - 2)));
                    continue;
                }

                var speakerMatch = Regex.Match(raw, @"^(D|L|LI|M):\s*(.*)$");
                if (!speakerMatch.Success)
                    continue;

                var speaker = speakerMatch.Groups[1].Value;
                var text = speakerMatch.Groups[2].Value;
                if (_mediaByLine.TryGetValue(lineNumber, out var media))
                {
                    flows.Add(Media(media, speaker == "M" ? PartnerForLine(lineNumber) : speaker));
                    continue;
                }

                if (IsMediaText(text))
                    throw new InvalidDataException($"No media mapping exists for source line {lineNumber}: {text}");

                flows.Add(speaker == "M" ? Player(PartnerForLine(lineNumber), text) : Message(speaker, text));
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
            return Choice(partnerCode, new[] { branch });
        }

        public Flow Timelapse(string characterCode, string text)
        {
            var flow = Message(characterCode, string.Empty, 0f);
            var node = _asset.DialogueNodeDatas.First(item => item.NodeGuid == flow.Entry);
            node.Timelapses = TextLanguages(text);
            return flow;
        }

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
            for (var index = 0; index < valid.Count - 1; index++)
                foreach (var exit in valid[index].Exits)
                    Link(exit, valid[index + 1].Entry);
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
                if (!allGuids.Contains(link.BaseNodeGuid) || !allGuids.Contains(link.TargetNodeGuid))
                    throw new InvalidOperationException($"Episode graph contains a broken link: {link.BaseNodeGuid} -> {link.TargetNodeGuid}");

            var outgoing = _asset.NodeLinkDatas.GroupBy(link => link.BaseNodeGuid)
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
        }

        private string SpeakerText(int lineNumber)
        {
            var raw = _lines[lineNumber - 1].Trim();
            var match = Regex.Match(raw, @"^(?:D|L|LI|M):\s*(.*)$");
            if (!match.Success)
                throw new InvalidDataException($"Expected a speaker line at {lineNumber}: {raw}");
            return match.Groups[1].Value;
        }

        private string PartnerForLine(int lineNumber)
        {
            for (var index = lineNumber - 1; index < Math.Min(_lines.Length, lineNumber + 5); index++)
            {
                var forward = PartnerFromContext(_lines[index]);
                if (forward != null)
                    return forward;
            }

            for (var index = Math.Min(lineNumber - 2, _lines.Length - 1); index >= 0; index--)
            {
                var raw = _lines[index].Trim();
                var partner = PartnerFromContext(raw);
                if (partner != null)
                    return partner;
                var match = Regex.Match(raw, @"^(D|L|LI):");
                if (match.Success)
                    return match.Groups[1].Value;
            }
            return "L";
        }

        private static string PartnerFromContext(string source)
        {
            var raw = source.Trim();
            if (raw.Contains("Lisa texts", StringComparison.OrdinalIgnoreCase)) return "LI";
            if (raw.Contains("Dave texts", StringComparison.OrdinalIgnoreCase)) return "D";
            if (raw.Contains("Lily texts", StringComparison.OrdinalIgnoreCase) ||
                raw.Contains("MC texts Lily", StringComparison.OrdinalIgnoreCase)) return "L";
            return null;
        }

        private static DialogueCharacterSO Character(string code) => code switch
        {
            "LI" => Lisa,
            "D" => Dave,
            _ => Lily
        };

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
    }
}
#endif
