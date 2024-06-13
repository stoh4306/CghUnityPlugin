using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

public class HolXmlWriter : XmlTextWriter
{

    private XmlWriter writer;
    protected int level;

    public override XmlWriterSettings Settings => writer.Settings;

    public override WriteState WriteState => writer.WriteState;

    public override string XmlLang => writer.XmlLang;

    public override XmlSpace XmlSpace => writer.XmlSpace;

    public HolXmlWriter(TextWriter w) : base(w)
    {
        level = 0;
    }

    public HolXmlWriter(Stream w, Encoding encoding) : base(w, encoding)
    {
        level = 0;
    }

    public HolXmlWriter(string filename, Encoding encoding) : base(filename, encoding)
    {
        level = 0;
    }
    public HolXmlWriter(string fileName,XmlWriterSettings settings):base(TextWriter.Null)
    {
        writer = Create(fileName, settings);
    }

    public override void WriteEndDocument()
    {
        level--;
        writer.WriteEndDocument();
    }

    public override void WriteEndElement()
    {
        level--;
        writer.WriteEndElement();
    }

    public override Task WriteEndElementAsync()
    {
        level--;
        return writer.WriteEndElementAsync();
    }

    public override void WriteFullEndElement()
    {
        level--;
        writer.WriteFullEndElement();
    }

    public override Task WriteFullEndElementAsync()
    {
        level--;
        return writer.WriteFullEndElementAsync();
    }

    public override void WriteNode(XmlReader reader, bool defattr)
    {
        level++;
        writer.WriteNode(reader, defattr);
    }

    public override void WriteNode(XPathNavigator navigator, bool defattr)
    {
        level++;
        writer.WriteNode(navigator, defattr);
    }

    public override Task WriteNodeAsync(XmlReader reader, bool defattr)
    {
        level++;
        return writer.WriteNodeAsync(reader, defattr);
    }

    public override Task WriteNodeAsync(XPathNavigator navigator, bool defattr)
    {
        level++;
        return writer.WriteNodeAsync(navigator, defattr);
    }

    public override void WriteStartDocument()
    {
        level++;
        writer.WriteStartDocument();
    }

    public override void WriteStartDocument(bool standalone)
    {
        level++;
        writer.WriteStartDocument(standalone);
    }

    public override Task WriteStartDocumentAsync()
    {
        level++;
        return writer.WriteStartDocumentAsync();
    }

    public override Task WriteStartDocumentAsync(bool standalone)
    {
        level++;
        return writer.WriteStartDocumentAsync(standalone);
    }

    public override void WriteStartElement(string prefix, string localName, string ns)
    {
        level++;
        writer.WriteStartElement(prefix, localName, ns);
    }

    public override Task WriteStartElementAsync(string prefix, string localName, string ns)
    {
        level++;
        return writer.WriteStartElementAsync(prefix, localName, ns);
    }
    public void WriteNewLine(bool isIndent = true)
    {
        writer.WriteString(writer.Settings.NewLineChars);
        for (int i = 1; i < level; ++i)
        {
            writer.WriteString(writer.Settings.IndentChars);
        }
    }

    public override bool Equals(object obj)
    {
        return writer.Equals(obj);
    }

    public override int GetHashCode()
    {
        return writer.GetHashCode();
    }

    public override string ToString()
    {
        return writer.ToString();
    }

    public override Task FlushAsync()
    {
        return writer.FlushAsync();
    }

    public override void WriteAttributes(XmlReader reader, bool defattr)
    {
        writer.WriteAttributes(reader, defattr);
    }

    public override Task WriteAttributesAsync(XmlReader reader, bool defattr)
    {
        return writer.WriteAttributesAsync(reader, defattr);
    }

    public override Task WriteBase64Async(byte[] buffer, int index, int count)
    {
        return writer.WriteBase64Async(buffer, index, count);
    }

    public override Task WriteBinHexAsync(byte[] buffer, int index, int count)
    {
        return writer.WriteBinHexAsync(buffer, index, count);
    }

    public override Task WriteCDataAsync(string text)
    {
        return writer.WriteCDataAsync(text);
    }

    public override Task WriteCharEntityAsync(char ch)
    {
        return writer.WriteCharEntityAsync(ch);
    }

    public override Task WriteCharsAsync(char[] buffer, int index, int count)
    {
        return writer.WriteCharsAsync(buffer, index, count);
    }

    public override Task WriteCommentAsync(string text)
    {
        return writer.WriteCommentAsync(text);
    }

    public override Task WriteDocTypeAsync(string name, string pubid, string sysid, string subset)
    {
        return writer.WriteDocTypeAsync(name, pubid, sysid, subset);
    }
    
    public override Task WriteEndDocumentAsync()
    {
        return writer.WriteEndDocumentAsync();
    }

    public override Task WriteEntityRefAsync(string name)
    {
        return writer.WriteEntityRefAsync(name);
    }

    public override Task WriteNameAsync(string name)
    {
        return writer.WriteNameAsync(name);
    }

    public override Task WriteNmTokenAsync(string name)
    {
        return writer.WriteNmTokenAsync(name);
    }

    public override Task WriteProcessingInstructionAsync(string name, string text)
    {
        return writer.WriteProcessingInstructionAsync(name, text);
    }

    public override Task WriteQualifiedNameAsync(string localName, string ns)
    {
        return writer.WriteQualifiedNameAsync(localName, ns);
    }

    public override Task WriteRawAsync(char[] buffer, int index, int count)
    {
        return writer.WriteRawAsync(buffer, index, count);
    }

    public override Task WriteRawAsync(string data)
    {
        return writer.WriteRawAsync(data);
    }
    

    public override Task WriteStringAsync(string text)
    {
        return writer.WriteStringAsync(text);
    }

    public override Task WriteSurrogateCharEntityAsync(char lowChar, char highChar)
    {
        return writer.WriteSurrogateCharEntityAsync(lowChar, highChar);
    }

    public override void WriteValue(bool value)
    {
        writer.WriteValue(value);
    }

    public override void WriteValue(DateTime value)
    {
        writer.WriteValue(value);
    }

    public override void WriteValue(DateTimeOffset value)
    {
        writer.WriteValue(value);
    }

    public override void WriteValue(decimal value)
    {
        writer.WriteValue(value);
    }

    public override void WriteValue(double value)
    {
        writer.WriteValue(value);
    }

    public override void WriteValue(int value)
    {
        writer.WriteValue(value);
    }

    public override void WriteValue(long value)
    {
        writer.WriteValue(value);
    }

    public override void WriteValue(object value)
    {
        writer.WriteValue(value);
    }

    public override void WriteValue(float value)
    {
        writer.WriteValue(value);
    }

    public override void WriteValue(string value)
    {
        writer.WriteValue(value);
    }

    public override Task WriteWhitespaceAsync(string ws)
    {
        return writer.WriteWhitespaceAsync(ws);
    }

    public override void Close()
    {
        writer.Close();
    }

    public override void Flush()
    {
        writer.Flush();
    }

    public override string LookupPrefix(string ns)
    {
        return writer.LookupPrefix(ns);
    }

    public override void WriteBase64(byte[] buffer, int index, int count)
    {
        writer.WriteBase64(buffer, index, count);
    }

    public override void WriteBinHex(byte[] buffer, int index, int count)
    {
        writer.WriteBinHex(buffer, index, count);
    }

    public override void WriteCData(string text)
    {
        writer.WriteCData(text);
    }

    public override void WriteCharEntity(char ch)
    {
        writer.WriteCharEntity(ch);
    }

    public override void WriteChars(char[] buffer, int index, int count)
    {
        writer.WriteChars(buffer, index, count);
    }

    public override void WriteComment(string text)
    {
        writer.WriteComment(text);
    }

    public override void WriteDocType(string name, string pubid, string sysid, string subset)
    {
        writer.WriteDocType(name, pubid, sysid, subset);
    }

    public override void WriteEndAttribute()
    {
        writer.WriteEndAttribute();
    }

    public override void WriteEntityRef(string name)
    {
        writer.WriteEntityRef(name);
    }

    public override void WriteName(string name)
    {
        writer.WriteName(name);
    }

    public override void WriteNmToken(string name)
    {
        writer.WriteNmToken(name);
    }

    public override void WriteProcessingInstruction(string name, string text)
    {
        writer.WriteProcessingInstruction(name, text);
    }

    public override void WriteQualifiedName(string localName, string ns)
    {
        writer.WriteQualifiedName(localName, ns);
    }

    public override void WriteRaw(char[] buffer, int index, int count)
    {
        writer.WriteRaw(buffer, index, count);
    }

    public override void WriteRaw(string data)
    {
        writer.WriteRaw(data);
    }

    public override void WriteStartAttribute(string prefix, string localName, string ns)
    {
        writer.WriteStartAttribute(prefix, localName, ns);
    }

    public override void WriteString(string text)
    {
        writer.WriteString(text);
    }

    public override void WriteSurrogateCharEntity(char lowChar, char highChar)
    {
        writer.WriteSurrogateCharEntity(lowChar, highChar);
    }

    public override void WriteWhitespace(string ws)
    {
        writer.WriteWhitespace(ws);
    }
}


public class HolStringParser
{
    static char[] token = { '(', ')', '\t', ' ', '\n', '\r' };
    public static Vector2 GetVector2FromString(string str)
    {
        var strings = str.Split(token, StringSplitOptions.RemoveEmptyEntries);
        Vector2 output = Vector2.zero;
        for (var i = 0; i < 2; i++)
        {
            output[i] = float.Parse(strings[i]);
        }
        return output;
    }

    public static Vector3Int GetVectorIntFromString(string str)
    {
        var strings = str.Split(token, StringSplitOptions.RemoveEmptyEntries);
        Vector3Int output = Vector3Int.zero;
        for (var i = 0; i < 2; i++)
        {
            output[i] = int.Parse(strings[i]);
        }
        return output;
    }

    public static Vector2Int GetVector2intFromString(string str)
    {
        var strings = str.Split(token, StringSplitOptions.RemoveEmptyEntries);
        Vector2Int output = Vector2Int.zero;
        for (var i = 0; i < 2; i++)
        {
            output[i] = int.Parse(strings[i]);
        }
        return output;
    }


    public static Color GetColorFromString(string str)
    {
        var strings = str.Split(token, StringSplitOptions.RemoveEmptyEntries);
        Color output = Color.black;
        for (var i = 0; i < 3; i++)
        {
            output[i] = float.Parse(strings[i]);
        }
        return output;
    }
    public static Vector3 GetVectorFromString(string str)
    {
        var strings = str.Split(token, StringSplitOptions.RemoveEmptyEntries);
        Vector3 output = Vector3.zero;
        for (var i = 0; i < 3; i++)
        {
            output[i] = float.Parse(strings[i]);
        }
        return output;
    }
    public static Quaternion GetQuaterFromString(string str)
    {
        var strings = str.Split(token, StringSplitOptions.RemoveEmptyEntries);
        Quaternion output = Quaternion.identity;
        output.w = float.Parse(strings[0]);
        output.x = float.Parse(strings[1]);
        output.y = float.Parse(strings[2]);
        output.z = float.Parse(strings[3]);
        return output;
    }

    public static Matrix4x4 ReadMat3x3FromFile(string fileName)
    {
        Matrix4x4 mat = new Matrix4x4();
        StreamReader sr = new StreamReader(fileName);
        string fileTxt = sr.ReadToEnd();
        
        string[] splitedTxt = fileTxt.Split(token, StringSplitOptions.RemoveEmptyEntries);
        mat.m00 = float.Parse(splitedTxt[0]);
        mat.m01 = float.Parse(splitedTxt[1]);
        mat.m02 = float.Parse(splitedTxt[2]);
        mat.m10 = float.Parse(splitedTxt[3]);
        mat.m11 = float.Parse(splitedTxt[4]);
        mat.m12 = float.Parse(splitedTxt[5]);
        mat.m20 = float.Parse(splitedTxt[6]);
        mat.m21 = float.Parse(splitedTxt[7]);
        mat.m22 = float.Parse(splitedTxt[8]);
        
        sr.Close();
        return mat;
    }

    public static Matrix4x4 ReadMat4x4FromFile(string fileName)
    {
        
        StreamReader sr = new StreamReader(fileName);
        string fileTxt = sr.ReadToEnd();

        Matrix4x4 mat = ReadMat4x4FromString(fileTxt);
        
        sr.Close();
        return mat;
    }
    public static Matrix4x4 ReadMat4x4FromString(string data)
    {
        Matrix4x4 mat = new Matrix4x4();
        string[] splitedTxt = data.Split(token, StringSplitOptions.RemoveEmptyEntries);
        mat.m00 = float.Parse(splitedTxt[0]);
        mat.m01 = float.Parse(splitedTxt[1]);
        mat.m02 = float.Parse(splitedTxt[2]);
        mat.m03 = float.Parse(splitedTxt[3]);
        mat.m10 = float.Parse(splitedTxt[4]);
        mat.m11 = float.Parse(splitedTxt[5]);
        mat.m12 = float.Parse(splitedTxt[6]);
        mat.m13 = float.Parse(splitedTxt[7]);
        mat.m20 = float.Parse(splitedTxt[8]);
        mat.m21 = float.Parse(splitedTxt[9]);
        mat.m22 = float.Parse(splitedTxt[10]);
        mat.m23 = float.Parse(splitedTxt[11]);
        mat.m30 = float.Parse(splitedTxt[12]);
        mat.m31 = float.Parse(splitedTxt[13]);
        mat.m32 = float.Parse(splitedTxt[14]);
        mat.m33 = float.Parse(splitedTxt[15]);
        return mat;
    }

    public static void WriteTransformToString(HolXmlWriter xmlWriter, Transform transform)
    {
        xmlWriter.WriteStartElement("Transform");

        Matrix4x4 mat = transform.localToWorldMatrix;
        xmlWriter.WriteNewLine();
        xmlWriter.WriteValue(mat.m00);
        xmlWriter.WriteValue(" ");
        xmlWriter.WriteValue(mat.m01);
        xmlWriter.WriteValue(" ");
        xmlWriter.WriteValue(mat.m02);
        xmlWriter.WriteValue(" ");
        xmlWriter.WriteValue(mat.m03);
        xmlWriter.WriteNewLine();
        xmlWriter.WriteValue(mat.m10);
        xmlWriter.WriteValue(" ");
        xmlWriter.WriteValue(mat.m11);
        xmlWriter.WriteValue(" ");
        xmlWriter.WriteValue(mat.m12);
        xmlWriter.WriteValue(" ");
        xmlWriter.WriteValue(mat.m13);
        xmlWriter.WriteNewLine();
        xmlWriter.WriteValue(mat.m20);
        xmlWriter.WriteValue(" ");
        xmlWriter.WriteValue(mat.m21);
        xmlWriter.WriteValue(" ");
        xmlWriter.WriteValue(mat.m22);
        xmlWriter.WriteValue(" ");
        xmlWriter.WriteValue(mat.m23);
        xmlWriter.WriteNewLine();
        xmlWriter.WriteValue(mat.m30);
        xmlWriter.WriteValue(" ");
        xmlWriter.WriteValue(mat.m31);
        xmlWriter.WriteValue(" ");
        xmlWriter.WriteValue(mat.m32);
        xmlWriter.WriteValue(" ");
        xmlWriter.WriteValue(mat.m33);
        xmlWriter.WriteEndElement();

    }
    public static Matrix4x4 UnityMatToMayaMat(Matrix4x4 mat)
    {
        mat.m02 = -mat.m02;
        mat.m12 = -mat.m12;
        mat.m20 = -mat.m20;
        mat.m21 = -mat.m21;
        mat.m23 = -mat.m23;
        Matrix4x4 mat2 = Matrix4x4.identity;
        mat2.m22 = -1;
        mat2 = mat * mat2;

        return mat;
    }

    public static void WriteMatrixToString(HolXmlWriter xmlWriter, Matrix4x4 mat)
    {
        xmlWriter.WriteStartElement("Transform");
        xmlWriter.WriteNewLine();
        xmlWriter.WriteValue(mat.m00);
        xmlWriter.WriteValue(" ");
        xmlWriter.WriteValue(mat.m01);
        xmlWriter.WriteValue(" ");
        xmlWriter.WriteValue(mat.m02);
        xmlWriter.WriteValue(" ");
        xmlWriter.WriteValue(mat.m03);
        xmlWriter.WriteNewLine();
        xmlWriter.WriteValue(mat.m10);
        xmlWriter.WriteValue(" ");
        xmlWriter.WriteValue(mat.m11);
        xmlWriter.WriteValue(" ");
        xmlWriter.WriteValue(mat.m12);
        xmlWriter.WriteValue(" ");
        xmlWriter.WriteValue(mat.m13);
        xmlWriter.WriteNewLine();
        xmlWriter.WriteValue(mat.m20);
        xmlWriter.WriteValue(" ");
        xmlWriter.WriteValue(mat.m21);
        xmlWriter.WriteValue(" ");
        xmlWriter.WriteValue(mat.m22);
        xmlWriter.WriteValue(" ");
        xmlWriter.WriteValue(mat.m23);
        xmlWriter.WriteNewLine();
        xmlWriter.WriteValue(mat.m30);
        xmlWriter.WriteValue(" ");
        xmlWriter.WriteValue(mat.m31);
        xmlWriter.WriteValue(" ");
        xmlWriter.WriteValue(mat.m32);
        xmlWriter.WriteValue(" ");
        xmlWriter.WriteValue(mat.m33);
        xmlWriter.WriteEndElement();
    }
        public static string VectorToString(Vector2 input)
    {
        string result = "";
        result = input.x + " " + input.y;
        return result;
    }
    public static string VectorToString(Vector2Int input)
    {
        string result = "";
        result = input.x + " " + input.y;
        return result;
    }
    public static string VectorToString(Vector3 input)
    {
        string result = "";
        result = input.x + " " + input.y + " "+ input.z;
        return result;
    }
    public static string VectorToString(Vector3Int input)
    {
        string result = "";
        result = input.x + " " + input.y + " " + input.z;
        return result;
    }
    public static string QuaternionToString(Quaternion input)
    {
        
        string result = "";
        result = input.w+" "+input.x + " " + input.y + " " + input.z;
        return result;
    }
    public static void MatToTransform(ref Transform outTransform,Matrix4x4 mat)
    {
        if (mat.ValidTRS())
        {
            outTransform.rotation = mat.rotation;
            outTransform.position = new Vector3(mat.m03, mat.m13, mat.m23);
            outTransform.localScale = mat.lossyScale;
        }
    }


    public static void ReadTransformFromString(ref GameObject obj,XmlReader xmlReader)
    {
        if(xmlReader.Name == "Transform")
        {
            xmlReader.Read();
            Matrix4x4 mat = ReadMat4x4FromString(xmlReader.Value);
            Transform trans = obj.transform;
            MatToTransform(ref trans, mat);
            return;
        }
        while (xmlReader.Read())
        {
            if (xmlReader.IsStartElement())
            {
                string name = xmlReader.Name;
                switch (xmlReader.Name)
                {
                    case "Position":
                        xmlReader.Read();
                        obj.transform.position = GetVectorFromString(xmlReader.Value);

                        break;
                    case "Rotation":
                        xmlReader.Read();
                        obj.transform.rotation = GetQuaterFromString(xmlReader.Value);
                        break;
                    case "Scale":
                        xmlReader.Read();
                        obj.transform.localScale = GetVectorFromString(xmlReader.Value);
                        break;
                    case "Transform":
                        xmlReader.Read();
                        Matrix4x4 mat =  ReadMat4x4FromString(xmlReader.Value);
                        Transform trans = obj.transform;
                        MatToTransform(ref trans,mat);
                        break;


                }
            }
            else if (xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == "Transform") { 
                    return;
            }
        }
    }
    public static void WriteMatrix3x3ToFile(string fileName,Matrix4x4 mtx)
    {
        StreamWriter sw = new StreamWriter(fileName);
        sw.Write(mtx.m00);
        sw.Write("\t");
        sw.Write(mtx.m01);
        sw.Write("\t");
        sw.Write(mtx.m02);
        sw.WriteLine();
        sw.Write(mtx.m10);
        sw.Write("\t");
        sw.Write(mtx.m11);
        sw.Write("\t");
        sw.Write(mtx.m12);
        sw.WriteLine();
        sw.Write(mtx.m20);
        sw.Write("\t");
        sw.Write(mtx.m21);
        sw.Write("\t");
        sw.Write(mtx.m22);
        sw.WriteLine();
        sw.Close();

    }
    public static void WriteMatrix4x4ToFile(string fileName, Matrix4x4 mtx)
    {
        StreamWriter sw = new StreamWriter(fileName);   
        sw.Write(mtx.m00);
        sw.Write("\t");
        sw.Write(mtx.m01);
        sw.Write("\t");
        sw.Write(mtx.m02);
        sw.Write("\t");
        sw.Write(mtx.m03);
        sw.WriteLine();
        sw.Write(mtx.m10);
        sw.Write("\t");
        sw.Write(mtx.m11);
        sw.Write("\t");
        sw.Write(mtx.m12);
        sw.Write("\t");
        sw.Write(mtx.m13);
        sw.WriteLine();
        sw.Write(mtx.m20);
        sw.Write("\t");
        sw.Write(mtx.m21);
        sw.Write("\t");
        sw.Write(mtx.m22);
        sw.Write("\t");
        sw.Write(mtx.m23);
        sw.WriteLine();
        sw.Write(mtx.m30);
        sw.Write("\t");
        sw.Write(mtx.m31);
        sw.Write("\t");
        sw.Write(mtx.m32);
        sw.Write("\t");
        sw.Write(mtx.m33);
        sw.WriteLine();
        sw.Close();
    }
}
