using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Test
{
    public static class TextReaderExtensions
    {
        public static IEnumerable<string> EnumerateLines(this TextReader sReader)
        {
            if (sReader == null)
                throw new ArgumentNullException();
            string line;
            while ((line = sReader.ReadLine()) != null)
                yield return line;
        }
    }
}
