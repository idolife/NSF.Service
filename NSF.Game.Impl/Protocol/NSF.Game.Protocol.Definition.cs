//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: NSF.Game.Protocol.Definition.proto
namespace NSF.Game.Logic
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"GameReq")]
  public partial class GameReq : global::ProtoBuf.IExtensible
  {
    public GameReq() {}
    
    private string _json;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"json", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string Json
    {
      get { return _json; }
      set { _json = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"GameAck")]
  public partial class GameAck : global::ProtoBuf.IExtensible
  {
    public GameAck() {}
    
    private string _json;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"json", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string Json
    {
      get { return _json; }
      set { _json = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}