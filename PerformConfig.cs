
using System;
using System.Xml.Serialization;
using System.Collections.Generic;

[XmlRoot(ElementName = "includes")]
public class Includes
{
    [XmlElement(ElementName = "option")]
    public List<Option> PathList { get; set; }
}

[XmlRoot(ElementName = "excludes")]
public class Excludes
{
    [XmlElement(ElementName = "option")]
    public List<Option> PathList { get; set; }
}

[XmlRoot(ElementName = "archives")]
public class Archives
{
    [XmlElement(ElementName = "includes")]
    public Includes Includes { get; set; }

    [XmlElement(ElementName = "excludes")]
    public Excludes Excludes { get; set; }
}

[XmlRoot(ElementName = "option")]
public class Option
{
    [XmlAttribute(AttributeName = "name")]
    public string Name { get; set; }
    
    [XmlAttribute(AttributeName = "value")]
    public string Value { get; set; }
}

[XmlRoot(ElementName = "database")]
public class Database
{
    [XmlElement(ElementName = "option")]
    public List<Option> OptionList { get; set; }

    [XmlAttribute(AttributeName = "type")]
    public string Type { get; set; }

    [XmlAttribute(AttributeName = "name")]
    public string Name { get; set; }
}

[XmlRoot(ElementName = "databases")]
public class Databases
{
    [XmlElement(ElementName = "database")]
    public List<Database> DatabaseList { get; set; }
}

[XmlRoot(ElementName = "store")]
public class Storage
{
    [XmlElement(ElementName = "option")]
    public List<Option> OptionList { get; set; }

    [XmlAttribute(AttributeName = "type")]
    public string Type { get; set; }
}

[XmlRoot(ElementName = "storages")]
public class Storages
{
    [XmlElement(ElementName = "storage")]
    public List<Storage> StorageList { get; set; }
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

[XmlRoot(ElementName = "http_post")]
public class HttpPost : NotifyBase
{
}

[XmlRoot(ElementName = "slack")]
public class Slack : NotifyBase
{
}

[XmlRoot(ElementName = "notifiers")]
public class Notifiers
{
    [XmlElement(ElementName = "http_post")]
    public HttpPost HttpPost { get; set; }

    [XmlElement(ElementName = "slack")]
    public Slack Slack { get; set; }
}

[XmlRoot(ElementName = "perform")]
public class PerformConfig
{
    [XmlElement(ElementName = "archives")]
    public Archives Archives { get; set; }

    [XmlElement(ElementName = "databases")]
    public Databases Databases { get; set; }

    [XmlElement(ElementName = "storages")]
    public Storages Storages { get; set; }

    [XmlElement(ElementName = "notifiers")]
    public Notifiers Notifiers { get; set; }

    [XmlAttribute(AttributeName = "compress")]
    public string Compress { get; set; }

    [XmlAttribute(AttributeName = "name")]
    public string Name { get; set; }
}

