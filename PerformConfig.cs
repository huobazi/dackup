
using System;
using System.Xml.Serialization;
using System.Collections.Generic;


public class Option
{
     [XmlAttribute(AttributeName = "name")]
    public string Name { get; set; }
         
    [XmlAttribute(AttributeName = "value")]
    public string Value { get; set; }
}

public class Archives
{
    [XmlArray(ElementName="includes")]
    [XmlArrayItem(ElementName="path")]
    public List<string> Includes { get; set; }

    [XmlArray(ElementName="excludes")]
    [XmlArrayItem(ElementName="path")]
    public List<string> Excludes { get; set; }
}

public class Database
{

    [XmlElement(ElementName = "option")]
    public List<Option> OptionList { get; set; }

     [XmlAttribute(AttributeName = "type")]
    public string Type { get; set; }
     
    [XmlAttribute(AttributeName = "name")]
    public string Name { get; set; }
}

public class Storage
{
    [XmlElement(ElementName = "option")]
    public List<Option> OptionList { get; set; }
     
    [XmlAttribute(AttributeName = "type")]
    public string Type { get; set; }
}

public class NotifyBase
{
    [XmlElement(ElementName = "option")]
    public List<Option> OptionList { get; set; }

    [XmlAttribute(AttributeName = "on_success")]
    public bool OnSuccess { get; set; }

    [XmlAttribute(AttributeName = "on_warning")]
    public bool OnWarning { get; set; }

    [XmlAttribute(AttributeName = "on_failure")]
    public bool OnFailure { get; set; }
}

public class HttpPost : NotifyBase
{
}

public class Slack : NotifyBase
{
}

public class Notifiers
{    
    [XmlElement(ElementName="http_post")]
    public HttpPost HttpPost { get; set; }
    
    [XmlElement(ElementName="slack")]
    public Slack Slack { get; set; }
}

[XmlRoot(ElementName="perform")]
public class PerformConfig
{
    [XmlElement(ElementName="archives")]
    public Archives Archives { get; set; }

    [XmlArray(ElementName="databases")]
    [XmlArrayItem(ElementName="database")]
    public List<Database> Databases { get; set; }

    [XmlArray(ElementName="storages")]
    [XmlArrayItem(ElementName="storage")]
    public List<Storage> Storages { get; set; }

    [XmlElement(ElementName="notifiers")]
    public Notifiers Notifiers { get; set; }

    [XmlAttribute(AttributeName = "compress")]
    public string Compress { get; set; }

    [XmlAttribute(AttributeName = "name")]
    public string Name { get; set; }
}

