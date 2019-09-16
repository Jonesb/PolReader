using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml;
using PolReader.DiffingClasses;

namespace PolReader
{
    [Serializable()]
    public class PolReaderXMLNode : ISerializable
    {
        //public string Type { get; set; }
        public string Name { get; set; }

        public int DBID { get; set; }
        public int XMLNodeID { get; set; }

        public List<PolReaderXMLNode> Children { get; set; }
        public List<PolReaderXMLProperty> Properties { get; set; }

        public PolReaderXMLNode()
        {
            //Type = "";
            Name = "";

            Children = new List<PolReaderXMLNode>();
            Properties = new List<PolReaderXMLProperty>();
        }

        public PolReaderXMLNode(SerializationInfo info, StreamingContext ctxt)
        {
            //this.Type = (string)info.GetValue("Type", typeof(string));
            this.Name = (string)info.GetValue("Name", typeof(string));

            this.Children = (List<PolReaderXMLNode>)info.GetValue("Children", typeof(List<PolReaderXMLNode>));
            this.Properties = (List<PolReaderXMLProperty>)info.GetValue("Properties", typeof(List<PolReaderXMLProperty>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            //info.AddValue("Type", this.Type);
            info.AddValue("Name", this.Name);
            info.AddValue("Children", this.Children);
            info.AddValue("Properties", this.Properties);
        }

        public string Displayname
        {
            get
            {
                string PropName = getPropertyValue("Name");
                if (PropName.Length > 0)
                {
                    return PropName;
                }
                else
                {
                    return Name;
                }
            }
        }

        public PolReaderXMLNode(string FileName)
        {
            //Type = "";
            Name = "";

            Children = new List<PolReaderXMLNode>();
            Properties = new List<PolReaderXMLProperty>();

            if (File.Exists(FileName))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(FileName);
                XmlElement child = doc.DocumentElement;

                if (child != null)
                {
                    LoadNodeInfo(child);
                }
            }
                        

        }

        public void LoadNodeInfo(XmlElement NodeItem)
        {
            //Type = NodeItem.Name;
            Name = NodeItem.Name;
            foreach (XmlAttribute item in NodeItem.Attributes)
            {
                PolReaderXMLProperty prop = new PolReaderXMLProperty();

                prop.Name = item.Name;
                prop.Value = item.Value;
                Properties.Add(prop);

            }

            foreach (XmlNode child in NodeItem.ChildNodes)
            {
                if (child is XmlElement)
                {
                    PolReaderXMLNode pxmlitem = new PolReaderXMLNode();
                    pxmlitem.LoadNodeInfo((XmlElement)child);
                    Children.Add(pxmlitem);
                }
            }

        }

        public bool PropertyExists(string propertyName)
        {
            bool ReturnValue = false;

            foreach (PolReaderXMLProperty prop in Properties)
            {
                if (prop.Name.Equals(propertyName,StringComparison.CurrentCultureIgnoreCase))
                {
                    ReturnValue = true;
                    break;
                }
            }

            return ReturnValue;
        }

        public string getPropertyValue(string propertyName)
        {
            string ReturnValue = "";

            foreach (PolReaderXMLProperty prop in Properties)
            {
                if (prop.Name.Equals(propertyName, StringComparison.CurrentCultureIgnoreCase))
                {
                    ReturnValue = prop.Value;
                    break;
                }
            }

            return ReturnValue;
        }

        public string UniqueName()
        {
            string ReturnValue = "";

            if (PropertyExists("UID"))
            {
                ReturnValue = getPropertyValue("UID");
            }
            else if (PropertyExists("NameUID"))
            {
                ReturnValue = getPropertyValue("NameUID");
            }
            else if (PropertyExists("Name"))
            {
                ReturnValue = getPropertyValue("Name");
            }
            else
            {
                
                foreach (PolReaderXMLProperty prop in Properties)
                {
                    ReturnValue += prop.Name + "." + prop.Value + "..";
                }
            }

            return ReturnValue;
        }

        public bool EqualsItem(PolReaderXMLNode CompareItem)
        {
            bool ReturnValue = false;

            if (Name.Equals(CompareItem.Name))
            {
                
                if (PropertyExists("UID"))
                {
                    ReturnValue = getPropertyValue("UID").Equals(CompareItem.getPropertyValue("UID"));
                }
                else if (PropertyExists("NameUID"))
                {
                    ReturnValue = getPropertyValue("NameUID").Equals(CompareItem.getPropertyValue("NameUID"));
                }
                else if (PropertyExists("Name"))
                {
                    ReturnValue = getPropertyValue("Name").Equals(CompareItem.getPropertyValue("Name"));
                }
                else
                {
                    int FoundCount = 0;

                    if (Name.Equals("FilterCollection", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (Children.Count > 0 && CompareItem.Children.Count > 0 &&
                            Children[0].Name.Equals(CompareItem.Children[0].Name))
                        {
                            foreach (PolReaderXMLProperty prop in Properties)
                            {
                                bool found = false;

                                foreach (PolReaderXMLProperty compProp in CompareItem.Properties)
                                {
                                    if (prop.Name.Equals(compProp.Name) && prop.Value.Equals(compProp.Value))
                                    {
                                        found = true;
                                        break;
                                    }
                                }

                                if (!found)
                                {
                                    ReturnValue = false;
                                    break;
                                }
                                else
                                {
                                    FoundCount++;
                                }
                            }
                        }
                        else
                        {
                            FoundCount = -1;
                        }
                    }
                    else
                    {                        

                        foreach (PolReaderXMLProperty prop in Properties)
                        {
                            bool found = false;

                            foreach (PolReaderXMLProperty compProp in CompareItem.Properties)
                            {
                                if (prop.Name.Equals(compProp.Name) && prop.Value.Equals(compProp.Value))
                                {
                                    found = true;
                                    break;
                                }
                            }

                            if (!found)
                            {
                                ReturnValue = false;
                                break;
                            }
                            else
                            {
                                FoundCount++;
                            }
                        }
                    }

                    ReturnValue = (FoundCount == Properties.Count);
                }
            }

            return ReturnValue;
        }

        public XmlNodeDiffInfo Compare(PolReaderXMLNode oldXMLNodeInfo)
        {
            XmlNodeDiffInfo returnValue = new XmlNodeDiffInfo();
            
            if (PropertyExists("Name"))
            {
                returnValue.Name = Name + " : " + getPropertyValue("Name");
            }
            else
            {
                returnValue.Name = Name;
            }

            foreach (PolReaderXMLProperty prop in Properties)
            {
                bool found = false;

                foreach (PolReaderXMLProperty oldProp in oldXMLNodeInfo.Properties)
                {
                    if (prop.Name.Equals(oldProp.Name))
                    {
                        if (!prop.Value.Equals(oldProp.Value))
                        {
                            XmlPropertyDiffInfo DiffItem = new XmlPropertyDiffInfo();
                            DiffItem.Type = XmlPropertyDiffInfo.UPDATED_POLICY_ITEM;
                            DiffItem.OldItem = oldProp;
                            DiffItem.NewItem = prop;
                            returnValue.propItems.Add(DiffItem);
                        }

                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    XmlPropertyDiffInfo DiffItem = new XmlPropertyDiffInfo();
                    DiffItem.Type = XmlPropertyDiffInfo.NEW_POLICY_ITEM;
                    DiffItem.NewItem = prop;
                    returnValue.propItems.Add(DiffItem);
                }
            }

            foreach (PolReaderXMLProperty oldProp in oldXMLNodeInfo.Properties)
            {
                bool found = false;

                foreach (PolReaderXMLProperty prop in Properties)
                {
                    if (prop.Name.Equals(oldProp.Name))
                    {
                        found = true;
                    }
                }

                if (!found)
                {
                    XmlPropertyDiffInfo DiffItem = new XmlPropertyDiffInfo();
                    DiffItem.Type = XmlPropertyDiffInfo.DELETED_POLICY_ITEM;
                    DiffItem.OldItem = oldProp;
                    DiffItem.DBID = oldProp.DBID;
                    returnValue.propItems.Add(DiffItem);
                }
            }

            foreach (PolReaderXMLNode node in Children)
            {
                bool found = false;

                foreach (PolReaderXMLNode oldNode in oldXMLNodeInfo.Children)
                {                    
                    if (node.EqualsItem(oldNode))
                    {
                        XmlNodeDiffInfo diffItem = node.Compare(oldNode);

                        if (diffItem.propItems.Count > 0 || diffItem.nodeItems.Count > 0)
                        {
                            diffItem.Type = XmlNodeDiffInfo.UPDATED_POLICY;
                            returnValue.nodeItems.Add(diffItem);
                        }

                        found = true;
                        break;
                    }

                }

                if (!found)
                {
                    XmlNodeDiffInfo DiffItem = node.Compare(new PolReaderXMLNode());
                    DiffItem.Type = XmlNodeDiffInfo.NEW_POLICY;                    
                    returnValue.nodeItems.Add(DiffItem);
                }
            }

            foreach (PolReaderXMLNode oldNode in oldXMLNodeInfo.Children) 
            {
                bool found = false;

                foreach (PolReaderXMLNode node in Children)
                {
                    if (node.EqualsItem(oldNode))
                    {                        
                        found = true;
                        break;
                    }
                   
                }

                if (!found)
                {
                    XmlNodeDiffInfo DiffItem = new PolReaderXMLNode().Compare(oldNode);
                    DiffItem.Name = oldNode.Name;
                    DiffItem.Type = XmlNodeDiffInfo.DELETED_POLICY;
                    DiffItem.DBID = oldNode.DBID;
                    returnValue.nodeItems.Add(DiffItem);
                }
            }

            return returnValue;

        }

    }
}
