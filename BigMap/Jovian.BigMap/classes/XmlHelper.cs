using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Jovian.BigMap.classes
{
    /// <summary>
    /// LPY 2016-3-11 添加
    /// 静态类
    /// XML文件读取写入帮助类
    /// </summary>
    public class XmlHelper
    {
        public XmlHelper()
        {
        }

        /// <summary>
        /// 从xml文件中读取数据
        /// </summary>
        /// <param name="xmlFileName">文件名</param>
        /// <param name="xPath">节点路径，例如：   /Root/XXXX</param>
        /// <returns>节点值</returns>
        public static string GetValueByXPath(string xmlFileName, string xPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                string result = string.Empty;
                xmlDoc.Load(xmlFileName);
                XmlNode xmlNode = xmlDoc.SelectSingleNode(xPath);
                if (xmlNode != null)
                    result = xmlNode.InnerText;
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 更新xml文件中节点值
        /// </summary>
        /// <param name="xmlFileName">文件名</param>
        /// <param name="xPath">节点路径，例如：   /Root/XXXX</param>
        /// <param name="value">待更新的值</param>
        public static void UpdateValueByXPath(string xmlFileName, string xPath, string value)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(xmlFileName);
                XmlNode xmlNode = xmlDoc.SelectSingleNode(xPath);
                if (xmlNode != null)
                    xmlNode.InnerText = value;
                xmlDoc.Save(xmlFileName);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
            }
        }

        /// <summary>
        /// 针对业务，更新模块信息到xml文件
        /// </summary>
        /// <param name="xmlFileName">xml文件名</param>
        /// <param name="xPath">路径</param>
        /// <param name="rw">模块类</param>
        public static void UpdateModularsByName(string xmlFileName, string xPath, RemoteWin[] rws)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(xmlFileName);
                XmlNode xmlNode = xmlDoc.SelectSingleNode(xPath);
                //XmlElement xmlElement = null;
                if (xmlNode != null)
                {
                    xmlNode.RemoveAll();
                    foreach (RemoteWin rw in rws)
                    {
                        XmlElement modular = xmlDoc.CreateElement("Modular");

                        XmlElement name = xmlDoc.CreateElement("Name"); name.InnerText = rw.Name; modular.AppendChild(name);
                        XmlElement width = xmlDoc.CreateElement("Width"); width.InnerText = rw.Width.ToString(); modular.AppendChild(width);
                        XmlElement height = xmlDoc.CreateElement("Height"); height.InnerText = rw.Height.ToString(); modular.AppendChild(height);
                        XmlElement x = xmlDoc.CreateElement("X"); x.InnerText = rw.X.ToString(); modular.AppendChild(x);
                        XmlElement y = xmlDoc.CreateElement("Y"); y.InnerText = rw.Y.ToString(); modular.AppendChild(y);
                        XmlElement zindex = xmlDoc.CreateElement("Zindex"); zindex.InnerText = rw.Zindex.ToString(); modular.AppendChild(zindex);
                        XmlElement fontsize = xmlDoc.CreateElement("FontSize"); fontsize.InnerText = rw.FontSize.ToString(); modular.AppendChild(fontsize);
                        XmlElement img = xmlDoc.CreateElement("Img"); img.InnerText = rw.Img.Replace("\\","\\\\"); modular.AppendChild(img);

                        xmlNode.AppendChild(modular);
                    }
                }
                xmlDoc.Save(xmlFileName);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
            }
        }

        /// <summary>
        /// 针对业务，从xml文件读取模块信息
        /// </summary>
        /// <param name="xmlFileName"></param>
        /// <param name="xPath"></param>
        /// <returns></returns>
        public static RemoteWin[] GetModularsByXPath(string xmlFileName, string xPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            ArrayList alModulars = new ArrayList();
            RemoteWin[] rws = null;
            try
            {
                xmlDoc.Load(xmlFileName);
                XmlNode xmlNode = xmlDoc.SelectSingleNode(xPath);
                if (xmlNode != null)
                {
                    foreach (XmlElement xe in xmlNode)
                    {
                        alModulars.Add(new RemoteWin(Convert.ToDouble(xe["Width"].InnerText), Convert.ToDouble(xe["Height"].InnerText), Convert.ToDouble(xe["X"].InnerText), Convert.ToDouble(xe["Y"].InnerText), Convert.ToInt32(xe["Zindex"].InnerText), xe["Name"].InnerText, Convert.ToInt32(xe["FontSize"].InnerText), xe["Img"].InnerText));
                    }
                    rws = (RemoteWin[])alModulars.ToArray(typeof(RemoteWin));
                }
                return rws;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 业务相关，根据旧模块名，模块并替换为新模块名
        /// </summary>
        /// <param name="xmlFileName"></param>
        /// <param name="xPath"></param>
        /// <param name="oldName"></param>
        /// <param name="rw"></param>
        public static void UpdateModularByName(string xmlFileName, string xPath, string oldName, RemoteWin rw)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(xmlFileName);
                XmlNode xmlNode = xmlDoc.SelectSingleNode(xPath);
                if (xmlNode != null)
                {
                    foreach (XmlElement xe in xmlNode)
                    {
                        if (xe["Name"].InnerText == oldName)
                        {
                            XmlElement modular = xmlDoc.CreateElement("Modular");

                            XmlElement name = xmlDoc.CreateElement("Name"); name.InnerText = rw.Name; modular.AppendChild(name);
                            XmlElement width = xmlDoc.CreateElement("Width"); width.InnerText = rw.Width.ToString(); modular.AppendChild(width);
                            XmlElement height = xmlDoc.CreateElement("Height"); height.InnerText = rw.Height.ToString(); modular.AppendChild(height);
                            XmlElement x = xmlDoc.CreateElement("X"); x.InnerText = rw.X.ToString(); modular.AppendChild(x);
                            XmlElement y = xmlDoc.CreateElement("Y"); y.InnerText = rw.Y.ToString(); modular.AppendChild(y);
                            XmlElement zindex = xmlDoc.CreateElement("Zindex"); zindex.InnerText = rw.Zindex.ToString(); modular.AppendChild(zindex);
                            XmlElement fontsize = xmlDoc.CreateElement("FontSize"); fontsize.InnerText = rw.FontSize.ToString(); modular.AppendChild(fontsize);
                            XmlElement img = xmlDoc.CreateElement("Img"); img.InnerText = xe["Img"].InnerText; modular.AppendChild(img);

                            xmlNode.ReplaceChild(modular, xe);
                        }
                    }
                    xmlDoc.Save(xmlFileName);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
                return;
            }
        }

        public static bool CreateXMLFile()
        {
            bool isSuccess = false;
            try
            {
                XmlDocument dom = new XmlDocument();
                XmlDeclaration xmlDec = dom.CreateXmlDeclaration("1.0", "gb2312", null);
                XmlElement xe = dom.CreateElement("Root");
                dom.AppendChild(xmlDec);
                dom.AppendChild(xe);
                dom.Save("params.xml");
                isSuccess = true;
            }
            catch (Exception)
            {
            }
            return isSuccess;
        }
    }
}
