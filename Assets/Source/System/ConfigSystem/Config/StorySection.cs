// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: Story_Section.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Deploy {

  /// <summary>Holder for reflection information generated from Story_Section.proto</summary>
  public static partial class StorySectionReflection {

    #region Descriptor
    /// <summary>File descriptor for Story_Section.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static StorySectionReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChNTdG9yeV9TZWN0aW9uLnByb3RvEgZkZXBsb3kiPAoNU3RvcnlfU2VjdGlv",
            "bhIKCgJJZBgBIAEoBRIRCglDaGFwdGVySWQYAiABKAUSDAoETmFtZRgDIAEo",
            "CSKNAQoRU3RvcnlfU2VjdGlvbl9NYXASMwoFSXRlbXMYASADKAsyJC5kZXBs",
            "b3kuU3RvcnlfU2VjdGlvbl9NYXAuSXRlbXNFbnRyeRpDCgpJdGVtc0VudHJ5",
            "EgsKA2tleRgBIAEoBRIkCgV2YWx1ZRgCIAEoCzIVLmRlcGxveS5TdG9yeV9T",
            "ZWN0aW9uOgI4AWIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Deploy.Story_Section), global::Deploy.Story_Section.Parser, new[]{ "Id", "ChapterId", "Name" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Deploy.Story_Section_Map), global::Deploy.Story_Section_Map.Parser, new[]{ "Items" }, null, null, null, new pbr::GeneratedClrTypeInfo[] { null, })
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class Story_Section : pb::IMessage<Story_Section> {
    private static readonly pb::MessageParser<Story_Section> _parser = new pb::MessageParser<Story_Section>(() => new Story_Section());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Story_Section> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Deploy.StorySectionReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Story_Section() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Story_Section(Story_Section other) : this() {
      id_ = other.id_;
      chapterId_ = other.chapterId_;
      name_ = other.name_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Story_Section Clone() {
      return new Story_Section(this);
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

    /// <summary>Field number for the "ChapterId" field.</summary>
    public const int ChapterIdFieldNumber = 2;
    private int chapterId_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int ChapterId {
      get { return chapterId_; }
      set {
        chapterId_ = value;
      }
    }

    /// <summary>Field number for the "Name" field.</summary>
    public const int NameFieldNumber = 3;
    private string name_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Name {
      get { return name_; }
      set {
        name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Story_Section);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Story_Section other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Id != other.Id) return false;
      if (ChapterId != other.ChapterId) return false;
      if (Name != other.Name) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Id != 0) hash ^= Id.GetHashCode();
      if (ChapterId != 0) hash ^= ChapterId.GetHashCode();
      if (Name.Length != 0) hash ^= Name.GetHashCode();
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
      if (ChapterId != 0) {
        output.WriteRawTag(16);
        output.WriteInt32(ChapterId);
      }
      if (Name.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(Name);
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
      if (ChapterId != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(ChapterId);
      }
      if (Name.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Story_Section other) {
      if (other == null) {
        return;
      }
      if (other.Id != 0) {
        Id = other.Id;
      }
      if (other.ChapterId != 0) {
        ChapterId = other.ChapterId;
      }
      if (other.Name.Length != 0) {
        Name = other.Name;
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
          case 16: {
            ChapterId = input.ReadInt32();
            break;
          }
          case 26: {
            Name = input.ReadString();
            break;
          }
        }
      }
    }

  }

  public sealed partial class Story_Section_Map : pb::IMessage<Story_Section_Map> {
    private static readonly pb::MessageParser<Story_Section_Map> _parser = new pb::MessageParser<Story_Section_Map>(() => new Story_Section_Map());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Story_Section_Map> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Deploy.StorySectionReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Story_Section_Map() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Story_Section_Map(Story_Section_Map other) : this() {
      items_ = other.items_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Story_Section_Map Clone() {
      return new Story_Section_Map(this);
    }

    /// <summary>Field number for the "Items" field.</summary>
    public const int ItemsFieldNumber = 1;
    private static readonly pbc::MapField<int, global::Deploy.Story_Section>.Codec _map_items_codec
        = new pbc::MapField<int, global::Deploy.Story_Section>.Codec(pb::FieldCodec.ForInt32(8, 0), pb::FieldCodec.ForMessage(18, global::Deploy.Story_Section.Parser), 10);
    private readonly pbc::MapField<int, global::Deploy.Story_Section> items_ = new pbc::MapField<int, global::Deploy.Story_Section>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::MapField<int, global::Deploy.Story_Section> Items {
      get { return items_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Story_Section_Map);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Story_Section_Map other) {
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
    public void MergeFrom(Story_Section_Map other) {
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