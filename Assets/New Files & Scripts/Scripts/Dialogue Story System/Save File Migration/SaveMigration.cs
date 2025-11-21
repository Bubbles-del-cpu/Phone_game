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
                StringBuilder lastChapter = new StringBuilder();
                int braceDepth = 0;
                bool captureElement = false;

                while ((line = reader.ReadLine()) != null)
                {
                    // Detect start of chapterHistory array
                    if (line.Contains($"\"{ARRAY_FIELD_OLD}\""))
                    {
                        // Replace field name
                        line = line.Replace($"\"{ARRAY_FIELD_OLD}\": [", $"\"{ARRAY_FIELD_NEW}\":");
                        writer.WriteLine(line);
                        inChapterHistory = true;
                        continue;
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
                                    lastChapter.AppendLine(line);
                                    captureElement = false;
                                }
                            }
                            else if (c == ']' && braceDepth == 0)
                            {
                                // End of array - write last element without array brackets
                                writer.Write($"{lastChapter.ToString().TrimEnd(',', '\n', '\r')},");
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
                                lastChapter.Clear();
                            }
                            lastChapter.AppendLine(line);
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
    }
}