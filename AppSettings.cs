using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections;
using System.IO;

namespace PolReader
{
    class AppSettings:ApplicationSettingsBase
    {

        public AppSettings()
        {
            
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("")]
        public String LastGPFolderLoc
        {
            get {

                return (String)this["LastGPFolderLoc"]; 
            }
            set {

                if (Directory.Exists(value))
                {
                    this["LastGPFolderLoc"] = value;

                    if (this["GPFolderLocs"] == null)
                    {
                        this["GPFolderLocs"] = new List<String>();
                    }

                    List<String> str = (List<String>)this["GPFolderLocs"];

                    if (!str.Exists(delegate(string s)
                                                {
                                                    bool returnValue = false;

                                                    if (value.Equals(s, StringComparison.CurrentCultureIgnoreCase))
                                                    {
                                                        returnValue = true;
                                                    }

                                                    return returnValue;
                                                }))
                    {
                        str.Insert(0, value);
                    }

                    if (str.Count > 5)
                    {
                        str.RemoveRange(4, str.Count - 5);
                    }

                    this.Save();
                }
            }
        }

        [UserScopedSettingAttribute()]
        public List<String> GPFolderLocs
        {
            get
            {
                if (this["GPFolderLocs"] == null)
                {
                    this["GPFolderLocs"] = new List<String>();                    
                }

                return (List<String>)this["GPFolderLocs"];
            }
        }

    }
}
