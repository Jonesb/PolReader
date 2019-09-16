using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace PolReader
{
    [Serializable()]
    public class SecEditFile : ISerializable
    {

        public List<SecEditSection> Sections { get; set; }

        private string FileName;
	
        public SecEditFile(SerializationInfo info, StreamingContext ctxt)
        {
            this.Sections = (List<SecEditSection>)info.GetValue("Sections", typeof(List<SecEditSection>)); ;           
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Sections", this.Sections);            
        }

        public SecEditFile()
        {
            FileName = "";

            Sections = new List<SecEditSection>();
            
        }

        public SecEditFile(string FileLocation)
        {
            FileName = FileLocation;

            Sections = new List<SecEditSection>();

            if (File.Exists(FileName))
            {
                getFileData();
            }
        }

        public void AddExtraFile(string FileLocation)
        {
            FileName = FileLocation;

            //Sections = new List<SecEditSection>();

            if (File.Exists(FileName))
            {
                getFileData();
            }
        }

        public void AddData(string sectionName,string sectionValue, string sectionData)
        {
            bool found = false;

            foreach (SecEditSection secSection in Sections)
            {
                if (secSection.Name.Equals(sectionName))
                {
                    found = true;
                    SecEditValuePair secVP = new SecEditValuePair();
                    secVP.Name = sectionValue;
                    secVP.Value = sectionData;

                    secSection.Entries.Add(secVP);
                }
            }

            if (!found)
            {
                SecEditSection secEditSecion = new SecEditSection(sectionName);
                
                SecEditValuePair secVP = new SecEditValuePair();
                secVP.Name = sectionValue;
                secVP.Value = sectionData;

                secEditSecion.Entries.Add(secVP);

                Sections.Add(secEditSecion);
            }

        }

        public void getFileData()
        {            
            string WholeLineStr = "";
            string CurrentHeader = "";

            StreamReader sr = new StreamReader(FileName, System.Text.Encoding.Default);
            
            while (!sr.EndOfStream)
            {
                WholeLineStr = sr.ReadLine().ToString();

                if (WholeLineStr.Length > 0)
                {
                    if (!(WholeLineStr.Substring(0, 1).Equals(";", StringComparison.OrdinalIgnoreCase)))
                    {                     
                        if (WholeLineStr.Substring(0, 1).Equals("[", StringComparison.OrdinalIgnoreCase) &&
                            WholeLineStr.Substring(WholeLineStr.Length - 1, 1).Equals("]", StringComparison.OrdinalIgnoreCase))
                        {
                            string HeaderName = WholeLineStr.Substring(1, WholeLineStr.Length -2);
                            Sections.Add(new SecEditSection(HeaderName));                            
                        }
                        else if (Sections.Count > 0)
                        {
                            Sections[Sections.Count - 1].Entries.Add(new SecEditValuePair(WholeLineStr));
                        }
                    }
                }

            }
            sr.Close();            

        }


    }
}
