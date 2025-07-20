using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyProfilUIAutomation
{
    internal class BoyProfilCircleJsonMapper
    {
        public static List<BoyProfilCircle> retrieveBoyProfilCircles()
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string[] flowCadDirs = Directory.GetDirectories(desktopPath, "*FlowCAD*", SearchOption.TopDirectoryOnly);

                if (flowCadDirs.Length == 0)
                {
                    return null;
                }

                // Pick the first matching FlowCAD directory
                string flowCadDir = flowCadDirs[0];

                // Build path for hat.json inside that directory
                string fullPath = Path.Combine(flowCadDir, "hat.json");

                if (!File.Exists(fullPath))
                    throw new FileNotFoundException($"File not found: {fullPath}");

                string jsonContent = File.ReadAllText(fullPath);
                var circles = JsonConvert.DeserializeObject<List<BoyProfilCircle>>(jsonContent);
                File.Delete(fullPath);
                return circles ?? new List<BoyProfilCircle>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError reading JSON file: {ex.Message}");
                return new List<BoyProfilCircle>();
            }
        }
    }
}
