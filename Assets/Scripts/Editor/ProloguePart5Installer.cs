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
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Rebuilds Prologue Part 5 from the supplied script and media, then places it
/// after Prologue Part 4 in the standalone chapter list.
/// </summary>
public static class ProloguePart5Installer
{
    private const string Root = "Assets/Entities/Dialogue/prologue part 5";
    private const string SourcePath = Root + "/NTS HM PROLOGUE PART 5 - THE NIGHT.txt";
    private const string JapanesePath = Root + "/Prologue Part 5 Japanese.tsv";
    private const string MediaFolder = Root + "/Prologue part 5/";
    private const string AssetPath = "Assets/Entities/Dialogue/Prologue part 5.asset";
    private const string VariablesFolder = "Assets/Entities/Variables/Prologue Part 5";
    private const string MissingVideoPath = MediaFolder + "pro5 video 01 private recording.mp4";

    private const string LilyPath = "Assets/Entities/Characters/Lily.asset";
    private const string IzzyPath = "Assets/Entities/Characters/Izzy.asset";
    private const string DevPath = "Assets/Entities/Characters/Dev.asset";

    private static DialogueCharacterSO Lily;
    private static DialogueCharacterSO Izzy;
    private static DialogueCharacterSO Dev;
    private static readonly Dictionary<string, string> JapaneseByEnglish = new();

    [MenuItem("Tools/NTS/Install Prologue Part 5")]
    public static void Install()
    {
        EnsureFolder(VariablesFolder);
        Lily = RequireAsset<DialogueCharacterSO>(LilyPath);
        Izzy = RequireAsset<DialogueCharacterSO>(IzzyPath);
        Dev = RequireAsset<DialogueCharacterSO>(DevPath);
        LoadJapaneseTranslations();
        EnsureVariable();

        var chapter = LoadOrCreate<DialogueContainerSO>(AssetPath);
        chapter.name = "Prologue part 5";
        BuildChapter(chapter);
        EditorUtility.SetDirty(chapter);

        WireIntoGame(chapter);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Validate(chapter);

        if (AssetDatabase.LoadAssetAtPath<UnityEngine.Video.VideoClip>(MissingVideoPath) == null)
        {
            Debug.LogWarning("[Prologue Part 5] The referenced private-recording video was not supplied. " +
                             "The story remains fully playable and continues without a broken media node.");
        }

        Debug.Log("[Prologue Part 5] Installed and validated: full script, 17 supplied images, " +
                  "three choices, drunk_confession variable, gallery media, and standalone chapter flow.");
    }

    private static void BuildChapter(DialogueContainerSO target)
    {
        var lines = File.ReadAllLines(SourcePath);
        var builder = new GraphBuilder(target, lines, MediaMap());

        var root = builder.Seq(
            builder.Start(),
            builder.Linear(5, 43),
            builder.Choice("L", new[]
            {
                builder.Branch(46, 47, 50),
                builder.Branch(53, 54, 60)
            }),
            builder.Linear(64, 299),
            builder.Choice("L", new[]
            {
                builder.Branch(302, 303, 311),
                builder.Branch(314, 315, 319)
            }),
            builder.Linear(323, 513),
            builder.Choice("I", new[]
            {
                builder.Branch(516, 517, 519),
                builder.Branch(522, 523, 527)
            }),
            builder.Linear(531, 628),
            builder.Event("drunk_confession", true),
            builder.End());

        builder.FinalizeGraph(root);
    }

    private static Dictionary<int, string> MediaMap()
    {
        string Image(string fileName) => MediaFolder + fileName;
        return new Dictionary<int, string>
        {
            [39] = Image("pro5 picture 01 wedding date ring v2.png"),
            [75] = Image("pro5 picture 02 happy receptionist selfie.png"),
            [101] = Image("pro5 picture 03 staff bathroom lace selfie.png"),
            [127] = Image("pro5 picture 04 crushed celebration cake.png"),
            [149] = Image("pro5 picture 05 bedroom doorway white lace.png"),
            [158] = Image("pro5 picture 06 unzipped skirt lace.png"),
            [170] = Image("pro5 picture 07 user supplied bedroom selfie.png"),
            [202] = Image("pro5 picture 08 user supplied bed portrait.png"),
            [220] = Image("pro5 picture 09 green dress front selfie v3.png"),
            [231] = Image("pro5 picture 10 user supplied green dress rear mirror v2.png"),
            [252] = Image("pro5 picture 11 corner table group selfie v2.png"),
            [282] = Image("pro5 picture 12 unknown man at bar.png"),
            [293] = Image("pro5 picture 13 ring and water.png"),
            [356] = Image("pro5 picture 14 coat and cheek kiss v4.png"),
            [376] = Image("pro5 picture 15 covered grey shirt bedtime.png"),
            [438] = Image("pro5 picture 12 unknown man at bar.png"),
            [582] = Image("pro5 picture 16 tired reception coffee.png"),
            [605] = Image("pro5 picture 17 confession screenshot.png")
        };
    }

    private static void EnsureVariable()
    {
        var manager = RequireAsset<GlobalValueManager>("Assets/Meet and Talk/Resources/GlobalValue.asset");
        if (manager.BoolValues.All(value => value.ValueName != "drunk_confession"))
        {
            manager.BoolValues.Add(new GlobalValueBool
            {
                ValueName = "drunk_confession",
                Value = false,
                BaseValue = false,
                PreviousValues = new List<bool>()
            });
        }

        GetBoolEvent("drunk_confession", false);
        GetBoolEvent("drunk_confession", true);
        EditorUtility.SetDirty(manager);
    }

    private static void LoadJapaneseTranslations()
    {
        JapaneseByEnglish.Clear();
        if (!File.Exists(JapanesePath))
            return;

        foreach (var line in File.ReadAllLines(JapanesePath))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
                continue;
            var fields = line.Split('\t');
            if (fields.Length != 2)
                throw new InvalidDataException($"Invalid Prologue Part 5 Japanese row: {line}");
            var english = Encoding.UTF8.GetString(Convert.FromBase64String(fields[0]));
            var japanese = Encoding.UTF8.GetString(Convert.FromBase64String(fields[1]));
            JapaneseByEnglish[english] = japanese;
        }
    }

    private static string JapaneseFor(string english) =>
        JapaneseByEnglish.TryGetValue(english ?? string.Empty, out var japanese) ? japanese : english;

    private static GlobalValueEvent GetBoolEvent(string valueName, bool value)
    {
        var path = $"{VariablesFolder}/{valueName}_{(value ? "true" : "false")}.asset";
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

    private static void WireIntoGame(DialogueContainerSO chapter)
    {
        var manager = Resources.FindObjectsOfTypeAll<DialogueChapterManager>()
            .FirstOrDefault(item => item.gameObject.scene.IsValid());
        if (manager == null)
            throw new InvalidOperationException("No active DialogueChapterManager was found. Open the main mobile scene first.");

        AddCharacterIfMissing(manager, Lily);
        AddCharacterIfMissing(manager, Izzy);
        AddCharacterIfMissing(manager, Dev);

        manager.StandaloneChapters.RemoveAll(item =>
            item != null && (item.Story == chapter || (item.Story != null && item.Story.name == chapter.name)));

        // Append rather than insert: saves refer to standalone chapters by list position,
        // so existing entries (like the Christmas special) must keep their index.
        var insertIndex = manager.StandaloneChapters.Count;
        manager.StandaloneChapters.Add(new DialogueChapterManager.ChapterData
        {
            Story = chapter,
            StartID = string.Empty,
            ChapterIndex = insertIndex,
            IsStoryChapter = false,
            ReplaySettings = new List<DialogueChapterManager.ChapterData.ChapterReplaySetting>()
        });

        EditorUtility.SetDirty(manager);
        EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
        EditorSceneManager.SaveScene(manager.gameObject.scene);
    }

    private static void AddCharacterIfMissing(DialogueChapterManager manager, DialogueCharacterSO character)
    {
        if (!manager.AllDialogueCharacters.Contains(character))
            manager.AllDialogueCharacters.Add(character);
    }

    private static void Validate(DialogueContainerSO chapter)
    {
        if (chapter.StartNodeDatas.Count != 1 || chapter.EndNodeDatas.Count != 1)
            throw new InvalidOperationException("Prologue Part 5 must have exactly one start and one end node.");
        if (chapter.DialogueChoiceNodeDatas.Count(node => node.DialogueNodePorts.Count == 2) != 3)
            throw new InvalidOperationException("Prologue Part 5 must contain exactly three authored two-option choices.");
        if (chapter.DialogueNodeDatas.Count(node => node.Image != null) != 18)
            throw new InvalidOperationException("Prologue Part 5 must contain 18 image sends (17 unique files plus Picture 12 repeated).");
        if (chapter.DialogueNodeDatas.Any(node => node.Texts == null || node.Texts.Count != 2 || node.Timelapses == null || node.Timelapses.Count != 2))
            throw new InvalidOperationException("Every Prologue Part 5 dialogue node must contain English and Japanese-safe text entries.");

        var confessionEvents = chapter.EventNodeDatas.SelectMany(node => node.EventScriptableObjects)
            .Select(item => item.DialogueEventSO as GlobalValueEvent)
            .Where(item => item != null && item.Operation.ValueName == "drunk_confession" &&
                           item.Operation.OperationValue.Equals("true", StringComparison.OrdinalIgnoreCase))
            .Count();
        if (confessionEvents != 1)
            throw new InvalidOperationException("Prologue Part 5 must set drunk_confession=true exactly once.");

        var manager = Resources.FindObjectsOfTypeAll<DialogueChapterManager>()
            .FirstOrDefault(item => item.gameObject.scene.IsValid());
        if (manager == null || manager.StandaloneChapters.Count == 0 || manager.StandaloneChapters[manager.StandaloneChapters.Count - 1].Story != chapter)
            throw new InvalidOperationException("Prologue Part 5 was not installed at the end of the standalone chapters.");
    }

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
            throw new FileNotFoundException($"Required Prologue Part 5 asset was not found or imported as {typeof(T).Name}: {path}");
        return asset;
    }

    private static void EnsureFolder(string fullPath)
    {
        var parts = fullPath.Split('/');
        var current = parts[0];
        for (var index = 1; index < parts.Length; index++)
        {
            var next = current + "/" + parts[index];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[index]);
            current = next;
        }
    }

    private sealed class GraphBuilder
    {
        internal sealed class Flow
        {
            public string Entry;
            public List<string> Exits = new();
        }

        internal sealed class ChoiceBranch
        {
            public string Option;
            public Flow Content;
        }

        private readonly DialogueContainerSO _asset;
        private readonly string[] _lines;
        private readonly Dictionary<int, string> _mediaByLine;
        private float _x;

        public GraphBuilder(DialogueContainerSO asset, string[] lines, Dictionary<int, string> mediaByLine)
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
            var node = new StartNodeData
            {
                NodeGuid = Guid.NewGuid().ToString(),
                Position = Position(),
                startID = string.Empty
            };
            _asset.StartNodeDatas.Add(node);
            return Node(node.NodeGuid);
        }

        public Flow End()
        {
            var node = new EndNodeData
            {
                NodeGuid = Guid.NewGuid().ToString(),
                Position = Position(),
                EndNodeType = EndNodeType.End,
                Dialogue = null
            };
            _asset.EndNodeDatas.Add(node);
            return new Flow { Entry = node.NodeGuid, Exits = new List<string>() };
        }

        public Flow Event(string valueName, bool value)
        {
            var node = new EventNodeData
            {
                NodeGuid = Guid.NewGuid().ToString(),
                Position = Position(),
                EventScriptableObjects = new List<EventScriptableObjectData>
                {
                    new() { DialogueEventSO = GetBoolEvent(valueName, value) }
                }
            };
            _asset.EventNodeDatas.Add(node);
            return Node(node.NodeGuid);
        }

        public ChoiceBranch Branch(int optionLine, int contentStart, int contentEnd) => new()
        {
            Option = SpeakerText(optionLine),
            Content = Linear(contentStart, contentEnd)
        };

        public Flow Choice(string partnerCode, IEnumerable<ChoiceBranch> branches)
        {
            var branchList = branches.ToList();
            var node = new DialogueChoiceNodeData
            {
                NodeGuid = Guid.NewGuid().ToString(),
                Position = Position(),
                DialogueNodePorts = new List<DialogueNodePort>(),
                AudioClips = AudioLanguages(),
                Character = Character(partnerCode),
                AvatarPos = AvatarPosition.None,
                AvatarType = AvatarType.Normal,
                TextType = TextLanguages(string.Empty),
                Duration = 2f,
                Delay = 0f,
                Timelapse = string.Empty,
                RequireCharacterInput = false,
                SelectedChoice = new List<LanguageGeneric<string>>()
            };

            foreach (var branch in branchList)
            {
                node.DialogueNodePorts.Add(new DialogueNodePort
                {
                    PortGuid = Guid.NewGuid().ToString(),
                    InputGuid = branch.Content.Entry,
                    OutputGuid = node.NodeGuid,
                    TextLanguage = TextLanguages(branch.Option),
                    HintLanguage = TextLanguages(string.Empty)
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
                if (raw.Length == 0 || raw.StartsWith("CHOICE", StringComparison.OrdinalIgnoreCase) ||
                    raw.StartsWith("(The conversation continues", StringComparison.OrdinalIgnoreCase) ||
                    raw.StartsWith("---", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (raw.StartsWith("(") && IsTimeMarker(raw))
                {
                    flows.Add(Timelapse(PartnerForLine(lineNumber), raw.Trim('(', ')')));
                    continue;
                }

                var speakerMatch = Regex.Match(raw, @"^(D|L|I|M):\s*(.*)$");
                if (!speakerMatch.Success)
                    continue;

                var speaker = speakerMatch.Groups[1].Value;
                var text = speakerMatch.Groups[2].Value;
                if (_mediaByLine.TryGetValue(lineNumber, out var mediaPath))
                {
                    flows.Add(MediaImage(mediaPath, speaker == "M" ? PartnerForLine(lineNumber) : speaker));
                    continue;
                }

                if (text.StartsWith("(Sends Picture", StringComparison.OrdinalIgnoreCase) ||
                    text.StartsWith("(Sends Video", StringComparison.OrdinalIgnoreCase))
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
                NodeGuid = Guid.NewGuid().ToString(),
                Position = Position(),
                DialogueNodePorts = new List<DialogueNodePort>(),
                AudioClips = AudioLanguages(),
                Character = Character(characterCode),
                AvatarPos = AvatarPosition.None,
                AvatarType = AvatarType.Normal,
                Texts = TextLanguages(text),
                Timelapses = TextLanguages(string.Empty),
                Timelapse = string.Empty,
                Duration = duration,
                Delay = 0f,
                MediaType = MediaType.Sprite,
                Image = null,
                Video = null,
                VideoThumbnail = null,
                NotBackgroundCapable = false,
                GalleryVisibility = GalleryDisplay.Display,
                Post = null,
                DelayTimer = 0f
            };
            _asset.DialogueNodeDatas.Add(node);
            return Node(node.NodeGuid);
        }

        public Flow Player(string partnerCode, string text)
        {
            return Choice(partnerCode, new[]
            {
                new ChoiceBranch
                {
                    Option = text,
                    Content = Silent(partnerCode)
                }
            });
        }

        public Flow Silent(string characterCode) => Message(characterCode, string.Empty, 0f);

        public Flow Timelapse(string characterCode, string text)
        {
            var flow = Message(characterCode, string.Empty, 0f);
            var node = _asset.DialogueNodeDatas.First(item => item.NodeGuid == flow.Entry);
            node.Timelapses = TextLanguages(text);
            return flow;
        }

        public Flow MediaImage(string path, string characterCode)
        {
            var flow = Message(characterCode, string.Empty, 0f);
            var node = _asset.DialogueNodeDatas.First(item => item.NodeGuid == flow.Entry);
            node.MediaType = MediaType.Sprite;
            node.Image = RequireAsset<Sprite>(path);
            return flow;
        }

        public Flow Seq(params Flow[] flows)
        {
            var valid = flows.Where(flow => flow != null && !string.IsNullOrEmpty(flow.Entry)).ToList();
            if (valid.Count == 0)
                return Silent("L");
            for (var index = 0; index < valid.Count - 1; index++)
            {
                foreach (var exit in valid[index].Exits)
                    Link(exit, valid[index + 1].Entry);
            }

            return new Flow { Entry = valid[0].Entry, Exits = valid[^1].Exits.ToList() };
        }

        public void FinalizeGraph(Flow root)
        {
            var allGuids = new HashSet<string>(
                _asset.StartNodeDatas.Select(node => node.NodeGuid)
                    .Concat(_asset.EndNodeDatas.Select(node => node.NodeGuid))
                    .Concat(_asset.DialogueNodeDatas.Select(node => node.NodeGuid))
                    .Concat(_asset.DialogueChoiceNodeDatas.Select(node => node.NodeGuid))
                    .Concat(_asset.EventNodeDatas.Select(node => node.NodeGuid)));

            if (_asset.StartNodeDatas.Count != 1 || _asset.StartNodeDatas[0].NodeGuid != root.Entry)
                throw new InvalidOperationException("Prologue Part 5 must have exactly one root start node.");
            foreach (var link in _asset.NodeLinkDatas)
            {
                if (!allGuids.Contains(link.BaseNodeGuid) || !allGuids.Contains(link.TargetNodeGuid))
                    throw new InvalidOperationException($"Prologue Part 5 contains a broken link: {link.BaseNodeGuid} -> {link.TargetNodeGuid}");
            }

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
                throw new InvalidOperationException($"Prologue Part 5 contains {unreachable.Count} unreachable nodes.");

            var endGuids = new HashSet<string>(_asset.EndNodeDatas.Select(node => node.NodeGuid));
            var unfinished = visited.Where(guid => !outgoing.ContainsKey(guid) && !endGuids.Contains(guid)).ToList();
            if (unfinished.Count > 0)
                throw new InvalidOperationException($"Prologue Part 5 contains {unfinished.Count} unfinished paths.");
        }

        private string SpeakerText(int lineNumber)
        {
            var raw = _lines[lineNumber - 1].Trim();
            var match = Regex.Match(raw, @"^(?:D|L|I|M):\s*(.*)$");
            if (!match.Success)
                throw new InvalidDataException($"Expected a speaker line at {lineNumber}: {raw}");
            return match.Groups[1].Value;
        }

        private string PartnerForLine(int lineNumber)
        {
            for (var index = Math.Min(lineNumber - 2, _lines.Length - 1); index >= 0; index--)
            {
                var raw = _lines[index].Trim();
                if (raw.Contains("dev texts", StringComparison.OrdinalIgnoreCase) ||
                    raw.Contains("messages from the dev", StringComparison.OrdinalIgnoreCase)) return "D";
                if (raw.Contains("Izzy texts", StringComparison.OrdinalIgnoreCase)) return "I";
                if (raw.Contains("Lily texts", StringComparison.OrdinalIgnoreCase)) return "L";
                var match = Regex.Match(raw, @"^(D|L|I):");
                if (match.Success) return match.Groups[1].Value;
            }
            return "L";
        }

        private static DialogueCharacterSO Character(string code) => code switch
        {
            "D" => Dev,
            "I" => Izzy,
            _ => Lily
        };

        private static bool IsTimeMarker(string raw) => Regex.IsMatch(raw,
            @"^\((Late afternoon|Two minutes later|Just after six|A few minutes later|Forty minutes later|That evening|A little later|Several minutes later|Half past ten|Eleven o'clock|12:42 AM|1:14 AM|1:27 AM|Early the next morning)\.\.\.\)$",
            RegexOptions.IgnoreCase);

        private Flow Node(string guid) => new() { Entry = guid, Exits = new List<string> { guid } };

        private Vector2 Position()
        {
            _x += 350f;
            return new Vector2(_x, 0f);
        }

        private void Link(string from, string to)
        {
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
                return;
            if (_asset.NodeLinkDatas.Any(link => link.BaseNodeGuid == from && link.TargetNodeGuid == to))
                return;
            _asset.NodeLinkDatas.Add(new NodeLinkData { BaseNodeGuid = from, TargetNodeGuid = to });
        }

        private static List<LanguageGeneric<string>> TextLanguages(string english) => new()
        {
            new LanguageGeneric<string> { languageEnum = LocalizationEnum.English, LanguageGenericType = english },
            new LanguageGeneric<string> { languageEnum = LocalizationEnum.Japanese, LanguageGenericType = JapaneseFor(english) }
        };

        private static List<LanguageGeneric<AudioClip>> AudioLanguages() => new()
        {
            new LanguageGeneric<AudioClip> { languageEnum = LocalizationEnum.English, LanguageGenericType = null },
            new LanguageGeneric<AudioClip> { languageEnum = LocalizationEnum.Japanese, LanguageGenericType = null }
        };
    }
}
#endif
