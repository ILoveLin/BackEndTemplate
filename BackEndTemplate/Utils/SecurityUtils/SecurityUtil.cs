namespace BackEndTemplate.Utils.SecurityUtil
{
    public class SecurityUtil
    {
        public static string Decrypt(string str)
        {
            var data = new char[str.Length];
            int lindex = 0;
            int rindex = data.Length - 1;
            for (int i = 0; i < data.Length; i++)
            {
                if (i % 2 == 0)
                {
                    data[lindex++] = str[i];
                }
                else
                {
                    data[rindex--] = str[i];
                }
            }
            return new string(data.Reverse().ToArray());
        }
    }
}
