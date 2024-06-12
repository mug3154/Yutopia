using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;


[Serializable]
public class Story_Data
{
    public Queue<Story_Data_Base> List = new Queue<Story_Data_Base>();
}

public class Story_Data_Converter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(Story_Data_Base).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType,
        object existingValue, JsonSerializer serializer)
    {
        JObject jObject = JObject.Load(reader);

        string className = (string)jObject["ClassName"];
        var type = Type.GetType(className);
        var target = Activator.CreateInstance(type);
        serializer.Populate(jObject.CreateReader(), target);
        return target;
    }

    public override void WriteJson(JsonWriter writer, object value,
        JsonSerializer serializer)
    {
        JToken t = JToken.FromObject(value);

        if (t.Type != JTokenType.Object)
        {
            t.WriteTo(writer);
        }
        else
        {
            JObject o = (JObject)t;
            IList<string> propertyNames = o.Properties().Select(p => p.Name).ToList();

            o.AddFirst(new JProperty("Keys", new JArray(propertyNames)));
            o.AddFirst(new JProperty("ClassName", value.GetType().ToString()));

            o.WriteTo(writer);
        }
    }
}


[Serializable]
public class Story_Data_Base
{
}

//[Serializable]
//public class Story_Data_Resource_List : Story_Data_Base
//{
//    public List<Tuple<string, string>> _AtlasNImgs;
//}

[Serializable]
public class Story_Data_Object : Story_Data_Base
{
    public string Name;
    public string AtlasName {  get; set; }
    public string ResourceName {  get; set; }
    public float X;
    public float Y;
    public int OrderInLayer;
    public ushort[] Color;
    public float ScaleX;
    public float ScaleY;
}

[Serializable]
public class Story_Data_Delay : Story_Data_Base
{
    public int MilliSeconds;
}

[Serializable]
public class Story_Data_Script : Story_Data_Base
{
    public bool Skippable;
    public string SpeecherName;
    public string Script; //Skippable=true인 경우 스킵 시 size나 color를 제외한 모든 요소가 무시되고 바로 출력된다.
}

[Serializable]
public class Story_Data_ScriptBox_Visible : Story_Data_Base
{
    public bool Visible;
}

[Serializable]
public class Story_Data_FadeInOut : Story_Data_Base
{
    public string TargetName;
    public ushort[] StartColor;
    public ushort[] EndColor;
    public float Duration;
    public string Ease {  get; set; }
}

[Serializable]
public class Story_Data_Object_Visible : Story_Data_Base
{
    public string TargetName;
    public bool Visible;
}

[Serializable]
public class Story_Data_Move : Story_Data_Base
{
    public string TargetName;
    public float[] StartXY;
    public float[] EndXY;
    public float Duration;
    public string Ease;
}

[Serializable]
public class Story_Data_Camera_Shaking : Story_Data_Base
{
    public float Duration;
    public float Strength = 1;
    public int Vibrato = 10;
    public bool FadeOut = true;
}

//[Serializable]
//public class Story_Data_Vignette