using System;
using System.Linq;
using System.IO;

namespace TaggedDateEditor
{

    class Program
    {
        static void Main(string[] args)
        {
            string filePath = "./input.mp4";
            byte[] searchPattern = { 0x6D, 0x76, 0x68, 0x64 }; //# 'mvhd' header pattern in MP4 files

            string dateString = "2012-02-06 06:26:50"; //# example date
            byte[] replacementData = GetReplacementData(dateString);

            ReplaceBytes(filePath, searchPattern, replacementData);
        }

        static byte[] GetReplacementData(string dateString)
        {
            DateTime date = DateTime.Parse(dateString);
            long time = new DateTimeOffset(date).ToUnixTimeSeconds() + 2082844800;
            byte[] replacementData = BitConverter.GetBytes(time).Take(4).ToArray();

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(replacementData);
            }

            return replacementData;
        }

        static void ReplaceBytes(string filePath, byte[] searchPattern, byte[] replacementData)
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            bool found = false;
            bool match = false;

            for (int i = 0; i < fileData.Length - 20; i++)
            {
                if (fileData[i] == searchPattern[0]) //# it matches first search byte?
                {
                    for (int j = 0; j < searchPattern.Length; j++) //# also check rest for matching
                    {
                        if (fileData[i + j] != searchPattern[j])
                        { match = false; break; }

                        if (j == (searchPattern.Length - 1)) //# if all counted bytes are matching
                        { match = true; }
                    }

                    if (match == true)
                    {
                        found = true;
                        int offset = (i + searchPattern.Length + 8);
                        Array.Copy(replacementData, 0, fileData, offset, replacementData.Length);
                        break;
                    }
                }
            }

            if (!found)
            {
                Console.WriteLine("Search pattern not found.");
                return;
            }

            File.WriteAllBytes(filePath, fileData);
            Console.WriteLine("Bytes replaced successfully.");
        }
    }
}
