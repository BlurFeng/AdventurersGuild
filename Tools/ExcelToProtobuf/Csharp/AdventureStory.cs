// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: Adventure_Story.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Deploy {

  /// <summary>Holder for reflection information generated from Adventure_Story.proto</summary>
  public static partial class AdventureStoryReflection {

    #region Descriptor
    /// <summary>File descriptor for Adventure_Story.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static AdventureStoryReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChVBZHZlbnR1cmVfU3RvcnkucHJvdG8SBmRlcGxveSKCAgoPQWR2ZW50dXJl",
            "X1N0b3J5EgoKAklkGAEgASgFEgwKBE5hbWUYAiABKAkSEAoIRGVzY3JpYmUY",
            "AyABKAkSDAoEVHlwZRgEIAEoBRIRCglJbmNpZGVudHMYBSADKAkSHAoUVGly",
            "ZU1vdmVDb25kaXRpb25OdW0YBiADKAUSOQoJQ29uZGl0aW9uGAcgAygLMiYu",
            "ZGVwbG95LkFkdmVudHVyZV9TdG9yeS5Db25kaXRpb25FbnRyeRIXCg9Mb2dp",
            "Y0V4cHJlc3Npb24YCCABKAkaMAoOQ29uZGl0aW9uRW50cnkSCwoDa2V5GAEg",
            "ASgFEg0KBXZhbHVlGAIgASgJOgI4ASKTAQoTQWR2ZW50dXJlX1N0b3J5X01h",
            "cBI1CgVJdGVtcxgBIAMoCzImLmRlcGxveS5BZHZlbnR1cmVfU3RvcnlfTWFw",
            "Lkl0ZW1zRW50cnkaRQoKSXRlbXNFbnRyeRILCgNrZXkYASABKAUSJgoFdmFs",
            "dWUYAiABKAsyFy5kZXBsb3kuQWR2ZW50dXJlX1N0b3J5OgI4AWIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Deploy.Adventure_Story), global::Deploy.Adventure_Story.Parser, new[]{ "Id", "Name", "Describe", "Type", "Incidents", "TireMoveConditionNum", "Condition", "LogicExpression" }, null, null, null, new pbr::GeneratedClrTypeInfo[] { null, }),
            new pbr::GeneratedClrTypeInfo(typeof(global::Deploy.Adventure_Story_Map), global::Deploy.Adventure_Story_Map.Parser, new[]{ "Items" }, null, null, null, new pbr::GeneratedClrTypeInfo[] { null, })
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class Adventure_Story : pb::IMessage<Adventure_Story> {
    private static readonly pb::MessageParser<Adventure_Story> _parser = new pb::MessageParser<Adventure_Story>(() => new Adventure_Story());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Adventure_Story> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Deploy.AdventureStoryReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Adventure_Story() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Adventure_Story(Adventure_Story other) : this() {
      id_ = other.id_;
      name_ = other.name_;
      describe_ = other.describe_;
      type_ = other.type_;
      incidents_ = other.incidents_.Clone();
      tireMoveConditionNum_ = other.tireMoveConditionNum_.Clone();
      condition_ = other.condition_.Clone();
      logicExpression_ = other.logicExpression_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Adventure_Story Clone() {
      return new Adventure_Story(this);
    }

    /// <summary>Field number for the "Id" field.</summary>
    public const int IdFieldNumber = 1;
    private int id_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Id {
      get { return id_; }
      set {
        id_ = value;
      }
    }

    /// <summary>Field number for the "Name" field.</summary>
    public const int NameFieldNumber = 2;
    private string name_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Name {
      get { return name_; }
      set {
        name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "Describe" field.</summary>
    public const int DescribeFieldNumber = 3;
    private string describe_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Describe {
      get { return describe_; }
      set {
        describe_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "Type" field.</summary>
    public const int TypeFieldNumber = 4;
    private int type_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Type {
      get { return type_; }
      set {
        type_ = value;
      }
    }

    /// <summary>Field number for the "Incidents" field.</summary>
    public const int IncidentsFieldNumber = 5;
    private static readonly pb::FieldCodec<string> _repeated_incidents_codec
        = pb::FieldCodec.ForString(42);
    private readonly pbc::RepeatedField<string> incidents_ = new pbc::RepeatedField<string>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<string> Incidents {
      get { return incidents_; }
    }

    /// <summary>Field number for the "TireMoveConditionNum" field.</summary>
    public const int TireMoveConditionNumFieldNumber = 6;
    private static readonly pb::FieldCodec<int> _repeated_tireMoveConditionNum_codec
        = pb::FieldCodec.ForInt32(50);
    private readonly pbc::RepeatedField<int> tireMoveConditionNum_ = new pbc::RepeatedField<int>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<int> TireMoveConditionNum {
      get { return tireMoveConditionNum_; }
    }

    /// <summary>Field number for the "Condition" field.</summary>
    public const int ConditionFieldNumber = 7;
    private static readonly pbc::MapField<int, string>.Codec _map_condition_codec
        = new pbc::MapField<int, string>.Codec(pb::FieldCodec.ForInt32(8, 0), pb::FieldCodec.ForString(18, ""), 58);
    private readonly pbc::MapField<int, string> condition_ = new pbc::MapField<int, string>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::MapField<int, string> Condition {
      get { return condition_; }
    }

    /// <summary>Field number for the "LogicExpression" field.</summary>
    public const int LogicExpressionFieldNumber = 8;
    private string logicExpression_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string LogicExpression {
      get { return logicExpression_; }
      set {
        logicExpression_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Adventure_Story);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Adventure_Story other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Id != other.Id) return false;
      if (Name != other.Name) return false;
      if (Describe != other.Describe) return false;
      if (Type != other.Type) return false;
      if(!incidents_.Equals(other.incidents_)) return false;
      if(!tireMoveConditionNum_.Equals(other.tireMoveConditionNum_)) return false;
      if (!Condition.Equals(other.Condition)) return false;
      if (LogicExpression != other.LogicExpression) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Id != 0) hash ^= Id.GetHashCode();
      if (Name.Length != 0) hash ^= Name.GetHashCode();
      if (Describe.Length != 0) hash ^= Describe.GetHashCode();
      if (Type != 0) hash ^= Type.GetHashCode();
      hash ^= incidents_.GetHashCode();
      hash ^= tireMoveConditionNum_.GetHashCode();
      hash ^= Condition.GetHashCode();
      if (LogicExpression.Length != 0) hash ^= LogicExpression.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Id != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(Id);
      }
      if (Name.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Name);
      }
      if (Describe.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(Describe);
      }
      if (Type != 0) {
        output.WriteRawTag(32);
        output.WriteInt32(Type);
      }
      incidents_.WriteTo(output, _repeated_incidents_codec);
      tireMoveConditionNum_.WriteTo(output, _repeated_tireMoveConditionNum_codec);
      condition_.WriteTo(output, _map_condition_codec);
      if (LogicExpression.Length != 0) {
        output.WriteRawTag(66);
        output.WriteString(LogicExpression);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Id != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Id);
      }
      if (Name.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
      }
      if (Describe.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Describe);
      }
      if (Type != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Type);
      }
      size += incidents_.CalculateSize(_repeated_incidents_codec);
      size += tireMoveConditionNum_.CalculateSize(_repeated_tireMoveConditionNum_codec);
      size += condition_.CalculateSize(_map_condition_codec);
      if (LogicExpression.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(LogicExpression);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Adventure_Story other) {
      if (other == null) {
        return;
      }
      if (other.Id != 0) {
        Id = other.Id;
      }
      if (other.Name.Length != 0) {
        Name = other.Name;
      }
      if (other.Describe.Length != 0) {
        Describe = other.Describe;
      }
      if (other.Type != 0) {
        Type = other.Type;
      }
      incidents_.Add(other.incidents_);
      tireMoveConditionNum_.Add(other.tireMoveConditionNum_);
      condition_.Add(other.condition_);
      if (other.LogicExpression.Length != 0) {
        LogicExpression = other.LogicExpression;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            Id = input.ReadInt32();
            break;
          }
          case 18: {
            Name = input.ReadString();
            break;
          }
          case 26: {
            Describe = input.ReadString();
            break;
          }
          case 32: {
            Type = input.ReadInt32();
            break;
          }
          case 42: {
            incidents_.AddEntriesFrom(input, _repeated_incidents_codec);
            break;
          }
          case 50:
          case 48: {
            tireMoveConditionNum_.AddEntriesFrom(input, _repeated_tireMoveConditionNum_codec);
            break;
          }
          case 58: {
            condition_.AddEntriesFrom(input, _map_condition_codec);
            break;
          }
          case 66: {
            LogicExpression = input.ReadString();
            break;
          }
        }
      }
    }

  }

  public sealed partial class Adventure_Story_Map : pb::IMessage<Adventure_Story_Map> {
    private static readonly pb::MessageParser<Adventure_Story_Map> _parser = new pb::MessageParser<Adventure_Story_Map>(() => new Adventure_Story_Map());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Adventure_Story_Map> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Deploy.AdventureStoryReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Adventure_Story_Map() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Adventure_Story_Map(Adventure_Story_Map other) : this() {
      items_ = other.items_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Adventure_Story_Map Clone() {
      return new Adventure_Story_Map(this);
    }

    /// <summary>Field number for the "Items" field.</summary>
    public const int ItemsFieldNumber = 1;
    private static readonly pbc::MapField<int, global::Deploy.Adventure_Story>.Codec _map_items_codec
        = new pbc::MapField<int, global::Deploy.Adventure_Story>.Codec(pb::FieldCodec.ForInt32(8, 0), pb::FieldCodec.ForMessage(18, global::Deploy.Adventure_Story.Parser), 10);
    private readonly pbc::MapField<int, global::Deploy.Adventure_Story> items_ = new pbc::MapField<int, global::Deploy.Adventure_Story>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::MapField<int, global::Deploy.Adventure_Story> Items {
      get { return items_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Adventure_Story_Map);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Adventure_Story_Map other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!Items.Equals(other.Items)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= Items.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      items_.WriteTo(output, _map_items_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      size += items_.CalculateSize(_map_items_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Adventure_Story_Map other) {
      if (other == null) {
        return;
      }
      items_.Add(other.items_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            items_.AddEntriesFrom(input, _map_items_codec);
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code