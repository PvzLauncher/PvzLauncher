using System.Text;

namespace PvzLauncherRemake.Utils
{
    public class StringHelper
    {
        public List<string> RandomStrings = new List<string>()
        {
            "q","w","e","r","t","y","u","i","o","p","a","s","d","f","g","h","j","k","l","z","x","c","v","b","n","m",
            "Q","W","E","R","T","Y","U","I","O","P","A","S","D","F","G","H","J","K","L","Z","X","C","V","B","N","M",
            "1","2","3","4","5","6","7","8","9","0","-","=","!","@","#","$","%","^","&","*","(",")","_","+","[","]",
            "\\",";","'",",",".","/","{","}","|",":","\"","<",">","?"
        };

        private Random _random = new Random();

        public string GenRandomeString(int length)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(RandomStrings[_random.Next(0, RandomStrings.Count)]);
            }
            return sb.ToString();
        }
    }
}
