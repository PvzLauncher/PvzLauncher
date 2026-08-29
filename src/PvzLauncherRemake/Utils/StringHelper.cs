using System.Text;

namespace PvzLauncherRemake.Utils
{
    public class StringHelper
    {
        public static class RandomeStrings
        {
            public static readonly List<string> All = new List<string>()
            {
                "q","w","e","r","t","y","u","i","o","p","a","s","d","f","g","h","j","k","l","z","x","c","v","b","n","m",
                "Q","W","E","R","T","Y","U","I","O","P","A","S","D","F","G","H","J","K","L","Z","X","C","V","B","N","M",
                "1","2","3","4","5","6","7","8","9","0","-","=","!","@","#","$","%","^","&","*","(",")","_","+","[","]",
                "\\",";","'",",",".","/","{","}","|",":","\"","<",">","?"
            };
            public static readonly List<string> Lowercases = new List<string>()
            {
                "q","w","e","r","t","y","u","i","o","p","a","s","d","f","g","h","j","k","l","z","x","c","v","b","n","m"
            };
            public static readonly List<string> Capitals = new List<string>()
            {
                "Q","W","E","R","T","Y","U","I","O","P","A","S","D","F","G","H","J","K","L","Z","X","C","V","B","N","M"
            };
            public static readonly List<string> Letters = new List<string>()
            {
                "q","w","e","r","t","y","u","i","o","p","a","s","d","f","g","h","j","k","l","z","x","c","v","b","n","m",
                "Q","W","E","R","T","Y","U","I","O","P","A","S","D","F","G","H","J","K","L","Z","X","C","V","B","N","M"
            };
            public static readonly List<string> Numbers = new List<string>()
            {
                "1","2","3","4","5","6","7","8","9","0"
            };
            public static readonly List<string> Symbols = new List<string>()
            {
                "-","=","!","@","#","$","%","^","&","*","(",")","_","+","[","]","\\",";","'",",",".","/","{","}","|",":","\"","<",">","?"
            };
        }


        private Random _random = new Random();

        public string GenRandomeString(int length, List<string> dict)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(dict[_random.Next(0, dict.Count)]);
            }
            return sb.ToString();
        }
    }
}
