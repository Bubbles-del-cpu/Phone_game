using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SaveMigrationSystem
{
    public static class SaveMigration
    {
        private static string ARRAY_FIELD_OLD = "Chapters";
        private static string ARRAY_FIELD_NEW = "LastChapter";
        public static bool NeedMigration(string savePath)
        {
            // Read only the first few KB to check format
            const int bufferSize = 4096; // 4KB should be enough to find field names

            using (FileStream fs = new FileStream(savePath, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(fs))
            {
                char[] buffer = new char[bufferSize];
                int charsRead = reader.Read(buffer, 0, bufferSize);
                string preview = new string(buffer, 0, charsRead);

                // Check if old field name exists
                return preview.Contains($"\"{ARRAY_FIELD_OLD}\"");
            }
        }

        public static void MigrateLargeSave(string savePath)
        {
            string tempPath = savePath + ".temp";

            using (StreamReader reader = new StreamReader(savePath))
            using (StreamWriter writer = new StreamWriter(tempPath))
            {
                string line;
                bool inChapterHistory = false;
                List<string> allChapters = new List<string>();
                StringBuilder currentChapter = new StringBuilder();
                int braceDepth = 0;
                bool captureElement = false;
                var checkField = $"\"{ARRAY_FIELD_OLD}\"";
                var newField = $"\"{ARRAY_FIELD_NEW}\"";

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains(checkField))
                    {
                        if (line.Contains($"{checkField}: [],"))
                        {
                            // Handle empty array case
                            line = line.Replace($"{checkField}: [],", $"{newField}: {{}},");
                            writer.WriteLine(line);
                            continue;
                        }
                        else if (line.Contains($"{checkField}: []"))
                        {
                            // Handle empty array case without trailing comma
                            line = line.Replace($"{checkField}: []", $"{newField}: {{}}");
                            writer.WriteLine(line);
                            continue;
                        }
                        else
                        {
                            // Multi-line array case
                            line = line.Replace($"{checkField}: [", $"{newField}:");
                            writer.WriteLine(line);
                            inChapterHistory = true;
                            continue;
                        }
                    }

                    if (inChapterHistory)
                    {
                        // Track array/object depth
                        foreach (char c in line)
                        {
                            if (c == '{')
                            {
                                if (braceDepth == 0) captureElement = true;
                                braceDepth++;
                            }
                            else if (c == '}')
                            {
                                braceDepth--;
                                if (braceDepth == 0)
                                {
                                    currentChapter.AppendLine(line);
                                    captureElement = false;
                                    // Store complete chapter
                                    allChapters.Add(currentChapter.ToString());
                                    currentChapter.Clear();
                                }
                            }
                            else if (c == ']' && braceDepth == 0)
                            {
                                // End of array - find last chapter with PastCoversations data
                                string lastChapterWithData = FindLastChapterWithData(allChapters);

                                if (!string.IsNullOrEmpty(lastChapterWithData))
                                {
                                    writer.Write($"{lastChapterWithData.TrimEnd(',', '\n', '\r')},");
                                }
                                else
                                {
                                    // No chapter with data, write empty object
                                    writer.Write("{},");
                                }

                                writer.WriteLine();
                                inChapterHistory = false;
                                break;
                            }
                        }

                        // Capture the current element
                        if (captureElement)
                        {
                            if (braceDepth == 1 && line.Trim().StartsWith("{"))
                            {
                                currentChapter.Clear();
                            }
                            currentChapter.AppendLine(line);
                        }
                    }
                    else
                    {
                        // Write line as-is
                        writer.WriteLine(line);
                    }
                }
            }

            //Replace old file with migrated one
            File.Delete(savePath + ".backup");
            File.Move(savePath, savePath + ".backup");
            File.Move(tempPath, savePath);
        }

        private static string FindLastChapterWithData(List<string> chapters)
        {
            // Search backwards through chapters
            for (int i = chapters.Count - 1; i >= 0; i--)
            {
                string chapter = chapters[i];

                // Check if PastCoversations array has content (not empty)
                // Look for pattern: "PastCoversations": [ ... with content ... ]
                int pastConvIndex = chapter.IndexOf("\"PastCoversations\"");

                if (pastConvIndex != -1)
                {
                    // Find the array brackets after PastCoversations
                    int openBracket = chapter.IndexOf('[', pastConvIndex);
                    int closeBracket = chapter.IndexOf(']', openBracket);

                    if (openBracket != -1 && closeBracket != -1)
                    {
                        // Get content between brackets
                        string arrayContent = chapter.Substring(openBracket + 1, closeBracket - openBracket - 1).Trim();

                        // Check if there's actual content (not just whitespace)
                        if (!string.IsNullOrWhiteSpace(arrayContent))
                        {
                            return chapter;
                        }
                    }
                }
            }

            // If no chapter with data found, return the last chapter or empty
            return chapters.Count > 0 ? chapters[chapters.Count - 1] : string.Empty;
        }
    }
}