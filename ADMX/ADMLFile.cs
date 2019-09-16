using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace ADMX
{
    public class ADMLFile
    {
        public List<ADMLItem> Items = new List<ADMLItem>();
        
        public ADMLFile(string Location)
        {
            if (File.Exists(Location))
            {
                XmlDocument XmlDoc = new XmlDocument();
                XmlNodeList SettingsList;
                XmlNodeList PresentationList;

                XmlDoc.Load(Location);

                if (XmlDoc.DocumentElement.Attributes["xmlns"] !=null)
                {
                    string xmlns = XmlDoc.DocumentElement.Attributes["xmlns"].Value;
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(XmlDoc.NameTable);
                    nsmgr.AddNamespace("policyDefinitionResources", xmlns);

                    SettingsList = XmlDoc.SelectNodes("//policyDefinitionResources:policyDefinitionResources//policyDefinitionResources:resources//policyDefinitionResources:stringTable//policyDefinitionResources:string", nsmgr);
                    PresentationList = XmlDoc.SelectNodes("//policyDefinitionResources:policyDefinitionResources//policyDefinitionResources:resources//policyDefinitionResources:presentationTable//policyDefinitionResources:presentation", nsmgr);
                }
                else
                {
                    SettingsList = XmlDoc.SelectNodes("//policyDefinitionResources//resources//stringTable//string");
                    PresentationList = XmlDoc.SelectNodes("//policyDefinitionResources//resources//presentationTable//presentation");
                }


                foreach (XmlNode Setting in SettingsList)
                {
                    ADMLItem Item = new ADMLItem();

                    try
                    {
                        Item.ID = Setting.Attributes["id"].Value;
                        Item.Text = Setting.InnerText;
                        Items.Add(Item);
                    }
                    catch (Exception e)
                    {

                    }

                }

                foreach (XmlNode Presentation in PresentationList)
                {
                    
                    foreach (XmlNode PresentationItem in Presentation.ChildNodes)
                    {
                        ADMLItem Item = new ADMLItem();

                        if (PresentationItem.Name.Equals("checkBox") || 
                            PresentationItem.Name.Equals("dropdownList") ||
                            (PresentationItem.Name.Equals("decimalTextBox")))
                        {
                            try
                            {
                                Item.ID = PresentationItem.Attributes["refId"].Value;
                                Item.Text = PresentationItem.InnerText;
                                Items.Add(Item);

                            }
                            catch (Exception e)
                            {

                            }

                        }
                        else if (PresentationItem.Name.Equals("listBox") ||                        
                        (PresentationItem.Name.Equals("comboBox")))
                        {
                            try
                            {
                                Item.ID = PresentationItem.Attributes["refId"].Value;
                                Item.Text = PresentationItem.InnerText;
                                Items.Add(Item);

                            }
                            catch (Exception e)
                            {

                            }

                        }
                        else if (PresentationItem.Name.Equals("textBox"))
                        {
                            try
                            {
                                foreach (XmlNode PresentationItemChild in PresentationItem.ChildNodes)
                                {
                                    if (PresentationItemChild.Name.Equals("label"))
                                    {
                                        Item.ID = PresentationItem.Attributes["refId"].Value;
                                        Item.Text = PresentationItemChild.InnerText;
                                        Items.Add(Item);
                                        break;
                                    }
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

        public String GetText(string Id)
        {
            string returnValue = Id;

            if (Id.ToUpper().StartsWith("$(STRING."))
            {
                returnValue = Id.ToUpper().Replace("$(STRING.", "").Replace(")", "");
            }

            foreach (ADMLItem Item in Items)
            {
                if (Item.ID.Equals(returnValue, StringComparison.CurrentCultureIgnoreCase))
                {
                    returnValue = Item.Text;
                    break;
                }
            }

            return returnValue;
        }

    }
}
