using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace ADMX
{
    public class GPOTemplates
    {
        public List<GPOTemplateItem> templateItems = new List<GPOTemplateItem>();
        public List<CategoryItem> categoryItems = new List<CategoryItem>();


        public delegate void LogUpdate(int Total, int Current, string Details);
        public event LogUpdate LogUpdateMessage;

        private void LogProgress(int Count, int Current, string Details)
        {
            if (LogUpdateMessage != null)
            {
                LogUpdateMessage(Count, Current, Details);
            }
        }
        
        public GPOTemplates()
        {
            
        }

        private void ResetCategories()
        {
            categoryItems = new List<CategoryItem>();

            CategoryItem gpoBaseItem = new CategoryItem();
            gpoBaseItem.Name = "windows:WindowsComponents";
            gpoBaseItem.DisplayName = "Windows Components";
            categoryItems.Add(gpoBaseItem);

            gpoBaseItem = new CategoryItem();
            gpoBaseItem.Name = "windows:Network";
            gpoBaseItem.DisplayName = "Network";
            categoryItems.Add(gpoBaseItem);

            gpoBaseItem = new CategoryItem();
            gpoBaseItem.Name = "windows:ControlPanel";
            gpoBaseItem.DisplayName = "Control Panel";
            categoryItems.Add(gpoBaseItem);

            gpoBaseItem = new CategoryItem();
            gpoBaseItem.Name = "windows:System";
            gpoBaseItem.DisplayName = "System";
            categoryItems.Add(gpoBaseItem);

            gpoBaseItem = new CategoryItem();
            gpoBaseItem.Name = "windows:Desktop";
            gpoBaseItem.DisplayName = "Desktop";
            categoryItems.Add(gpoBaseItem);

            gpoBaseItem = new CategoryItem();
            gpoBaseItem.Name = "windows:WindowsExplorer";
            gpoBaseItem.DisplayName = "Windows Components\\Windows Explorer";
            categoryItems.Add(gpoBaseItem);

            gpoBaseItem = new CategoryItem();
            gpoBaseItem.Name = "windows:InternetManagement_Settings";
            gpoBaseItem.DisplayName = "Windows Components\\Internet Communication Management\\InternetCommunication settings";
            categoryItems.Add(gpoBaseItem);

            gpoBaseItem = new CategoryItem();
            gpoBaseItem.Name = "windows:InternetManagement";
            gpoBaseItem.DisplayName = "Windows Components\\Internet Communication Management";
            categoryItems.Add(gpoBaseItem);

            gpoBaseItem = new CategoryItem();
            gpoBaseItem.Name = "windows:TabletPC";
            gpoBaseItem.DisplayName = "Windows Components\\Tablet PC";
            categoryItems.Add(gpoBaseItem);

            gpoBaseItem = new CategoryItem();
            gpoBaseItem.Name = "windows:Printers";
            gpoBaseItem.DisplayName = "Printers";
            categoryItems.Add(gpoBaseItem);

            gpoBaseItem = new CategoryItem();
            gpoBaseItem.Name = "windows:SharedFolders";
            gpoBaseItem.DisplayName = "Shared Folders";
            categoryItems.Add(gpoBaseItem);

            gpoBaseItem = new CategoryItem();
            gpoBaseItem.Name = "windows:StartMenu";
            gpoBaseItem.DisplayName = "Start Menu and Taskbar";
            categoryItems.Add(gpoBaseItem);
        }
        
        public void LoadTemplates(string TemplateLocation, string Language)
        {
            string Location = TemplateLocation;

            if (!TemplateLocation.EndsWith("\\"))
            {
                Location += "\\";
            }

            if (Directory.Exists(Location))
            {
                int Count = Directory.GetFiles(Location, "*.ADMX").Length;
                int Current = 0;

                foreach (String AdmxFile in Directory.GetFiles(Location, "*.ADMX"))
                {
                    ResetCategories();
                    Current++;
                    LogProgress(Count, Current, AdmxFile);

                    XmlDocument XmlDoc = new XmlDocument();
                    XmlNodeList SettingsList;
                    XmlNodeList CategoriesList;

                    XmlDoc.Load(AdmxFile);


                    if (XmlDoc.DocumentElement.Attributes["xmlns"] !=null)
                    {
                        string xmlns = XmlDoc.DocumentElement.Attributes["xmlns"].Value;
                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(XmlDoc.NameTable);
                        nsmgr.AddNamespace("policyDefinitions", xmlns);

                        SettingsList = XmlDoc.SelectNodes("//policyDefinitions:policyDefinitions//policyDefinitions:policies//policyDefinitions:policy", nsmgr);
                        CategoriesList = XmlDoc.SelectNodes("//policyDefinitions:policyDefinitions//policyDefinitions:categories//policyDefinitions:category", nsmgr);
                    }
                    else
                    {
                        SettingsList = XmlDoc.SelectNodes("//policyDefinitions//policies//policy");
                        CategoriesList = XmlDoc.SelectNodes("//policyDefinitions//categories//category");
                    }

                    ADMLFile adml = new ADMLFile(Location + Language + "\\" + AdmxFile.Replace(Location, "").ToUpper().Replace(".ADMX", ".ADML"));
                   

                    foreach (XmlNode Category in CategoriesList)
                    {
                        CategoryItem gpoItem = new CategoryItem();

                        try
                        {
                            gpoItem.Name = Category.Attributes["name"].Value;
                            gpoItem.DisplayName = adml.GetText(Category.Attributes["displayName"].Value);

                            categoryItems.Add(gpoItem);

                            XmlNode ParentInfo = Category["parentCategory"];
                            gpoItem.Parent = ParentInfo.Attributes["ref"].Value;
                            
                            foreach (CategoryItem sitem in categoryItems)
                            {
                                if (gpoItem.Parent.Equals(sitem.Name,StringComparison.CurrentCultureIgnoreCase))
                                {
                                    if (sitem.Path.Length > 0)
                                    {
                                        gpoItem.Path = sitem.Path + "\\" + sitem.DisplayName;
                                    }
                                    else
                                    {
                                        gpoItem.Path = sitem.DisplayName;
                                    }

                                    break;
                                }
                            }
                            
                                                                                   
                        }
                        catch 
                        {
                            
                        }
                    }

                    

                    foreach (XmlNode Setting in SettingsList)
                    {
                        GPOTemplateItem gpoItem =  new GPOTemplateItem();
                        

                        try
                        {
                            gpoItem.DisplayName = adml.GetText(Setting.Attributes["displayName"].Value);
                        }
                        catch
                        {

                        }

                        try
                        {                            
                            gpoItem.Key = Setting.Attributes["key"].Value;                            
                        }
                        catch (Exception e)
                        {

                        }
                        try
                        {
                            gpoItem.Explaination = adml.GetText(Setting.Attributes["explainText"].Value);
                        }
                        catch (Exception e)
                        {

                        }

                        try
                        {
                            gpoItem.Type = Setting.Attributes["class"].Value.Equals("User")?GPOTemplateItem.User:GPOTemplateItem.Machine;
                        }
                        catch (Exception e)
                        {

                        }

                        try
                        {
                            XmlNode ParentInfo = Setting["parentCategory"];
                            String Parent = ParentInfo.Attributes["ref"].Value;

                            foreach (CategoryItem sitem in categoryItems)
                            {
                                if (Parent.Equals(sitem.Name,StringComparison.CurrentCultureIgnoreCase))
                                {
                                    if (sitem.Path.Length > 0)
                                    {
                                        gpoItem.CategoryName = sitem.Path + "\\" + sitem.DisplayName;
                                    }
                                    else
                                    {
                                        gpoItem.CategoryName = sitem.DisplayName;
                                    }
                                    break;
                                }
                            }                           
                            
                        }
                        catch (Exception e)
                        {

                        }

                        try
                        {
                            gpoItem.ValueName = Setting.Attributes["valueName"].Value;
                            templateItems.Add(gpoItem);
                        }
                        catch(Exception e)
                        {
                            
                        }
                        
                        XmlNode elements = Setting["elements"];
                        XmlNode enabledList = Setting["enabledList"];

                        if (enabledList != null)
                        {
                            foreach (XmlNode childEnabledList in enabledList)
                            {
                                if (childEnabledList.Name.Equals("item"))
                                {
                                    GPOTemplateItem gpochildItem = new GPOTemplateItem();

                                    gpochildItem.Type = gpoItem.Type;                                    
                                    gpochildItem.Explaination = adml.GetText(Setting.Attributes["explainText"].Value);
                                    gpochildItem.CategoryName = gpoItem.CategoryName;
                                    gpochildItem.DisplayName = gpoItem.DisplayName;

                                    if (childEnabledList.Attributes != null &&
                                        childEnabledList.Attributes["key"] != null)
                                    {
                                        gpochildItem.Key = adml.GetText(childEnabledList.Attributes["key"].Value);
                                        gpochildItem.ValueName = adml.GetText(childEnabledList.Attributes["valueName"].Value);
                                        templateItems.Add(gpochildItem);
                                    }

                                    
                                }
                            }
                        }
                        if (elements != null)
                        {
                            foreach (XmlNode childElement in elements)
                            {
                                GPOTemplateItem gpochildItem = new GPOTemplateItem();

                                gpochildItem.Type = gpoItem.Type;
                                gpochildItem.Key = Setting.Attributes["key"].Value;
                                gpochildItem.Explaination = adml.GetText(Setting.Attributes["explainText"].Value);
                                gpochildItem.CategoryName = gpoItem.CategoryName;

                                if (childElement.Attributes !=null &&
                                    childElement.Attributes["displayName"] != null)
                                {
                                    gpochildItem.DisplayName = adml.GetText(childElement.Attributes["displayName"].Value);
                                }
                                else
                                {
                                    gpochildItem.DisplayName = adml.GetText(Setting.Attributes["displayName"].Value);
                                }
                                

                                try
                                {
                                    if (childElement.Name.Equals("boolean"))
                                    {
                                        gpochildItem.ElementType = GPOTemplateItem.BooleanType;
                                        gpochildItem.ElementString = adml.GetText(childElement.Attributes["id"].Value);
                                        gpochildItem.ValueName = childElement.Attributes["valueName"].Value;
                                        templateItems.Add(gpochildItem);
                                    }
                                    else if (childElement.Name.Equals("enum"))
                                    {
                                        gpochildItem.ElementType = GPOTemplateItem.EnumType;
                                        gpochildItem.ElementString = adml.GetText(childElement.Attributes["id"].Value);
                                        gpochildItem.ValueName = childElement.Attributes["valueName"].Value;

                                        foreach (XmlNode ValueChild in childElement.ChildNodes)
                                        {
                                            if (ValueChild.Name.Equals("item"))
                                            {
                                                XmlNode valueElements = ValueChild["value"];

                                                foreach (XmlNode ValueChildItem in valueElements.ChildNodes)
                                                {
                                                    EnumValue enumValue = new EnumValue();
                                                    enumValue.DisplayName = adml.GetText(ValueChild.Attributes["displayName"].Value);

                                                    if (ValueChildItem.Name.Equals("string"))
                                                    {
                                                        enumValue.Value = ValueChildItem.InnerText;
                                                    }
                                                    else
                                                    {
                                                        enumValue.Value = ValueChildItem.Attributes["value"].Value;
                                                    }
                                                    gpochildItem.enumValues.Add(enumValue);

                                                }
                                            }
                                        }

                                        templateItems.Add(gpochildItem);
                                    }
                                    else if (childElement.Name.Equals("text"))
                                    {
                                        gpochildItem.ElementType = GPOTemplateItem.TextType;
                                        gpochildItem.ElementString = adml.GetText(childElement.Attributes["id"].Value);
                                        gpochildItem.ValueName = childElement.Attributes["valueName"].Value;
                                        templateItems.Add(gpochildItem);
                                    }
                                    else if (childElement.Name.Equals("decimal"))
                                    {
                                        gpochildItem.ElementType = GPOTemplateItem.DecimalType;
                                        gpochildItem.ElementString = adml.GetText(childElement.Attributes["id"].Value);
                                        gpochildItem.ValueName = childElement.Attributes["valueName"].Value;
                                        templateItems.Add(gpochildItem);
                                    }                                    
                                    else if (childElement.Name.Equals("list"))
                                    {
                                        gpochildItem.ElementType = GPOTemplateItem.ListType;
                                        gpochildItem.ElementString = adml.GetText(childElement.Attributes["id"].Value);

                                        object vpobj = childElement.Attributes["valuePrefix"];                                        
                                        object evobj = childElement.Attributes["explicitValue"];
                                        object aobj = childElement.Attributes["additive"];
                                        object kobj = childElement.Attributes["key"];
                                        object eobj = childElement.Attributes["expandable"];
                                        object ceobj = childElement.Attributes["clientExtension"];

                                        if (vpobj != null)                                        
                                        {
                                            gpochildItem.ValueName = childElement.Attributes["valuePrefix"].Value;
                                        }

                                        if (kobj != null)
                                        {
                                            gpochildItem.Key = childElement.Attributes["key"].Value;

                                        }
                                        if (ceobj != null)
                                        {
                                            gpochildItem.ListGUID = childElement.Attributes["clientExtension"].Value;

                                        }                                        

                                        templateItems.Add(gpochildItem);
                                    }
                                    else
                                    {
                                        gpochildItem.ValueName = childElement.Attributes["valueName"].Value;
                                        templateItems.Add(gpochildItem);
                                    }
                                    
                                }
                                catch (Exception e)
                                {

                                }
                            }
                        }

                        
                    }                
                }
            }
        }

        public GPOTemplateItem GetItemInfo(int Context,string Key, string Value)
        {
            GPOTemplateItem GetItemInfo = null;
            
            foreach (GPOTemplateItem item in templateItems)
            {

                if (item.Key.Equals(Key, StringComparison.CurrentCultureIgnoreCase) && 
                    item.Type == Context)
                {
                    if (item.ValueName.Equals(Value.Replace("**del.", ""), StringComparison.CurrentCultureIgnoreCase))
                    {
                        GetItemInfo = item;
                        break;
                    }
                }
            }

            return GetItemInfo;
        }

    }
}
