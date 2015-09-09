using System;
using System.Text;
using System.Runtime.InteropServices;


namespace AmazonTest.src
{
    class IniReader
    {
        public string Path;

        public IniReader(string path)
        {
            this.Path = path;
        }

        #region
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defVal, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        #endregion

        public string ReadValue(string section, string key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(section, key, "", temp, 255, this.Path);
            return temp.ToString();
        }

        public void WriteValue(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, this.Path);
        }
    }
}
