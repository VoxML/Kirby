using System.Collections.Generic;
using System.Xml.Serialization;

// stores Ros worlds loaded from XML

public class sdf
{
    [XmlAttribute]
    public string version { get; set; }

    [XmlElement]
    public WorldModel model { get; set; }
}

public class WorldModel
{
    [XmlAttribute]
    public string name { get; set; }

    [XmlElement]
    public PoseFrame pose { get; set; }

    [XmlElement("link")]
    public List<WorldModelLink> link { get; set; }

    [XmlElement("static")]
    public string s { get; set; }
}

public class WorldModelLink
{
    [XmlAttribute]
    public string name { get; set; }

    [XmlElement]
    public WorldModelLinkColl collision { get; set; }

    [XmlElement]
    public WorldModelLinkVis visual { get; set; }

    [XmlElement]
    public PoseFrame pose { get; set; }
}

public class WorldModelLinkColl
{
    [XmlAttribute]
    public string name { get; set; }

    [XmlElement]
    public Geometry geometry { get; set; }

    [XmlElement]
    public PoseFrame pose { get; set; }
}

public class WorldModelLinkVis
{
    [XmlAttribute]
    public string name { get; set; }

    [XmlElement]
    public PoseFrame pose { get; set; }

    [XmlElement]
    public Geometry geometry { get; set; }

    [XmlElement]
    public VisMaterial material { get; set; }
}

public class PoseFrame
{
    [XmlAttribute]
    public string frame { get; set; }

    [XmlText]
    public string value { get; set; }
}

public class Geometry
{
    [XmlElement]
    public GeomBox box { get; set; }
}

public class GeomBox
{
    public string size { get; set; }
}

public class VisMaterial
{
    [XmlElement]
    public VisMaterialScript script { get; set; }
    public string ambient { get; set; }
}

public class VisMaterialScript
{
    public string uri { get; set; }
    public string name { get; set; }
}

public class WallAdjusterGroundTruth
{

}
