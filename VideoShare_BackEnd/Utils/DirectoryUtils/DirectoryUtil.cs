using VideoShare_BackEnd.Utils.NullUtils;

namespace VideoShare_BackEnd.Utils.DirectoryUtils
{
    public class DirectoryUtil
    {
        // 确保日志路径存在
        public static void EnsurePathExist(string path)
        {
            if (path.IsNullOrEmpty())
            {
                return;
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}