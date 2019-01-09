using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace labelImage
{
    class XmlTools
    {
        public List<RemarkRectangleNode> rectangleNodes;

        public void SaveXMLFilesAsLabelImageFormat(string path)
        {
            try
            {
                //xml文件存储路径
                path = "E:\\MyComputers.xml";
                //生成xml文件
                GenerateXMLFile(path);
                //遍历xml文件的信息
                //GetXMLInformation(path);
                ////修改xml文件的信息
                //ModifyXmlInformation(path);
                ////向xml文件添加节点信息
                //AddXmlInformation(path);
                ////删除指定节点信息
                //DeleteXmlInformation(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void GenerateXMLFile(string xmlFilePath)
        {
            try
            {
                //初始化一个xml实例
                XmlDocument myXmlDoc = new XmlDocument();
                //创建xml的根节点
                XmlElement rootElement = myXmlDoc.CreateElement("annotation");
                //将根节点加入到xml文件中（AppendChild）
                myXmlDoc.AppendChild(rootElement);

                //初始化第一层的第一个子节点
                XmlElement folderElement = myXmlDoc.CreateElement("folder");
                folderElement.InnerText = "Image";
                rootElement.AppendChild(folderElement);
                XmlElement filenameElement = myXmlDoc.CreateElement("filename");
                filenameElement.InnerText = "3n011.jpg";
                rootElement.AppendChild(filenameElement);
                XmlElement pathElement = myXmlDoc.CreateElement("path");
                pathElement.InnerText = xmlFilePath;
                rootElement.AppendChild(pathElement);

                XmlElement sourceElement = myXmlDoc.CreateElement("source");
                rootElement.AppendChild(sourceElement);
                XmlElement databaseElement = myXmlDoc.CreateElement("database");
                databaseElement.InnerText = "Unknown";
                sourceElement.AppendChild(databaseElement);

                XmlElement sizeElement = myXmlDoc.CreateElement("size");
                XmlElement widthElement = myXmlDoc.CreateElement("width");
                widthElement.InnerText = "0";
                XmlElement heightElement = myXmlDoc.CreateElement("height");
                heightElement.InnerText = "0";
                XmlElement depthElement = myXmlDoc.CreateElement("depth");
                depthElement.InnerText = "3";
                rootElement.AppendChild(sizeElement);
                sizeElement.AppendChild(widthElement);
                sizeElement.AppendChild(heightElement);
                sizeElement.AppendChild(depthElement);

                XmlElement segmentedElement = myXmlDoc.CreateElement("segmented");
                segmentedElement.InnerText = "0";
                rootElement.AppendChild(sizeElement);

                foreach (RemarkRectangleNode node in rectangleNodes)
                {
                    XmlElement objectElement = myXmlDoc.CreateElement("object");
                    rootElement.AppendChild(objectElement);
                    XmlElement nameElement = myXmlDoc.CreateElement("name");
                    nameElement.InnerText = "2";
                    objectElement.AppendChild(nameElement);
                    XmlElement poseElement = myXmlDoc.CreateElement("pose");
                    poseElement.InnerText = "Unspecified";
                    objectElement.AppendChild(poseElement);
                    XmlElement truncatedElement = myXmlDoc.CreateElement("truncated");
                    truncatedElement.InnerText = "0";
                    objectElement.AppendChild(truncatedElement);
                    XmlElement difficultElement = myXmlDoc.CreateElement("difficult");
                    difficultElement.InnerText = "0";
                    objectElement.AppendChild(difficultElement);
                    XmlElement bndboxElement = myXmlDoc.CreateElement("bndbox");
                    objectElement.AppendChild(bndboxElement);
                    XmlElement xminElement = myXmlDoc.CreateElement("xmin");
                    xminElement.InnerText = node.xmin.ToString();
                    bndboxElement.AppendChild(xminElement);
                    XmlElement xmaxElement = myXmlDoc.CreateElement("xmax");
                    xmaxElement.InnerText = node.xmax.ToString();
                    bndboxElement.AppendChild(xmaxElement);
                    XmlElement yminElement = myXmlDoc.CreateElement("ymin");
                    yminElement.InnerText = node.ymin.ToString();
                    bndboxElement.AppendChild(yminElement);
                    XmlElement ymaxElement = myXmlDoc.CreateElement("ymax");
                    ymaxElement.InnerText = node.ymax.ToString();
                    bndboxElement.AppendChild(ymaxElement);
                }

                //将xml文件保存到指定的路径下
                myXmlDoc.Save(xmlFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
