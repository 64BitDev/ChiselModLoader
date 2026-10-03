namespace ChiselModLoader.Shared
{
    //for stuff like versions and stuff
    static class CMLGlobals
    {
        public static readonly Int64 CMLVersion = 0;
        public static string basedir = AppDomain.CurrentDomain.BaseDirectory;

        public static string GetFullPath(string path)
        {
            if(!Path.IsPathFullyQualified(path))
            {
                return Path.Combine(basedir,path);
            }
            return path;
        }
    }
}
