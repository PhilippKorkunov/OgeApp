using System;
using System.Security.Cryptography;
using System.Text;

namespace OgeApp.Windows.WindowsClassesStructsAndEnums
{
    public readonly struct HashConvertor
    {
        private readonly byte[]? tmpHash;
        public HashConvertor(string taskName, string? answer) 
        {
            var tmpSource = ASCIIEncoding.ASCII.GetBytes($"{taskName} {answer}");
            tmpHash = MD5.Create().ComputeHash(tmpSource);
        }

        public override string ToString()
        {
            if (tmpHash is null) { return String.Empty; }
            int i;
            StringBuilder sOutput = new(tmpHash.Length);
            for (i = 0; i < tmpHash.Length - 1; i++)
            {
                sOutput.Append(tmpHash[i].ToString("X2"));
            }
            return sOutput.ToString();
        }
    }
}
