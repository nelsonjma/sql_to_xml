using System;
using System.Collections;
using System.Xml;


namespace GenericMethods
{

    /// <summary>
    /// Methods that can be used in all pages.
    /// </summary>
    public class Generic
    {
        /// <summary>
        /// Creates hash from string
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string GetHash(string message)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider x =
                new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
            data = x.ComputeHash(data);
            string ret = string.Empty;
            for (int i = 0; i < data.Length; i++)
                ret += data[i].ToString("x2").ToLower();
            // -------------------------------------------------
            return ret;
        }

        /// <summary>
        /// Remove spaces and tabs from string.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string RemoveUnwantedChars(string data)
        {
            data = data.Replace("\r\n", " ");
            data = data.Replace("\r\n", " ");
            data = data.Replace("\t", " ");
            data = data.Replace("\n", " ");

            return data.Trim();
        }

        public static string RemoveStartEndSquareBrackets(string data)
        {
            try
            {
                data = data.Trim();

                if (data.StartsWith("["))
                    data = data.Substring(1);

                if (data.EndsWith("]"))
                    data = data.Substring(0, data.Length - 1);

            }
            catch
            {
            }

            return data.Trim();
        }

        /// <summary>
        /// Get just one xml tag
        /// </summary>
        /// <param name="path"></param>
        /// <param name="colum"></param>
        /// <returns></returns>
        public static ArrayList ReadXml(string path, string colum)
        {
            ArrayList listOfElements = new ArrayList();

            XmlTextReader textReader = new XmlTextReader(path);

            int foundName = 0;

            while (textReader.Read())
            {
                if (foundName > 0 && textReader.Name != "" && textReader.Name.ToLower() != colum.ToLower())
                    foundName = 0;

                if (foundName == 2 && textReader.Name.ToLower() == colum.ToLower())
                {
                    foundName = 0;
                    goto ToEnd;
                }

                if (foundName == 0 && textReader.Name.ToLower() == colum.ToLower())
                {
                    foundName = 1;
                    goto ToEnd;
                }

                if (foundName == 1 && textReader.Name == "")
                {
                    listOfElements.Add(textReader.Value);
                    foundName = 2;
                }

                ToEnd:

                byte do_not_remove_please;
            }

            textReader.Close();
            return listOfElements;
        }

        /// <summary>
        /// Equal to method above but put object in a reader that can be 
        /// used more than once without reading file again
        /// </summary>
        /// <param name="textReader"></param>
        /// <param name="colum"></param>
        /// <returns></returns>
        public static ArrayList ReadXml(XmlTextReader textReader, string colum)
        {
            ArrayList listOfElements = new ArrayList();

            int foundName = 0;

            while (textReader.Read())
            {
                if (foundName > 0 && textReader.Name != "" && textReader.Name.ToLower() != colum.ToLower())
                    foundName = 0;

                if (foundName == 2 && textReader.Name.ToLower() == colum.ToLower())
                {
                    foundName = 0;
                    goto ToEnd;
                }

                if (foundName == 0 && textReader.Name.ToLower() == colum.ToLower())
                {
                    foundName = 1;
                    goto ToEnd;
                }

                if (foundName == 1 && textReader.Name == "")
                {
                    listOfElements.Add(textReader.Value);
                    foundName = 2;
                }

                ToEnd:

                byte do_not_remove_please;
            }

            textReader.Close();
            return listOfElements;
        }

        /// <summary>
        /// Permits replace the same char multiple times ' => ''
        /// </summary>
        public static string MultiCharReplace(string data, string inChar, string outChar)
        {
            if (!data.Contains(inChar)) return data;

            string[] listDataChars = data.Split(new string[] {inChar}, StringSplitOptions.None);

            string newdata = string.Empty;

            for (int i = 0; i < listDataChars.Length; i++)
            {
                newdata += listDataChars[i];

                if (i != listDataChars.Length - 1) newdata += outChar;
            }


            return newdata;
        }
    }
}