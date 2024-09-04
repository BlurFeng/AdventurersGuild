// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: Language_WordGroup.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Deploy {

  /// <summary>Holder for reflection information generated from Language_WordGroup.proto</summary>
  public static partial class LanguageWordGroupReflection {

    #region Descriptor
    /// <summary>File descriptor for Language_WordGroup.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static LanguageWordGroupReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChhMYW5ndWFnZV9Xb3JkR3JvdXAucHJvdG8SBmRlcGxveSJKChJMYW5ndWFn",
            "ZV9Xb3JkR3JvdXASCgoCSWQYASABKAUSFAoMV29yZElkc1N0YXJ0GAIgASgF",
            "EhIKCldvcmRJZHNFbmQYAyABKAUinAEKFkxhbmd1YWdlX1dvcmRHcm91cF9N",
            "YXASOAoFSXRlbXMYASADKAsyKS5kZXBsb3kuTGFuZ3VhZ2VfV29yZEdyb3Vw",
            "X01hcC5JdGVtc0VudHJ5GkgKCkl0ZW1zRW50cnkSCwoDa2V5GAEgASgFEikK",
            "BXZhbHVlGAIgASgLMhouZGVwbG95Lkxhbmd1YWdlX1dvcmRHcm91cDoCOAFi",
            "BnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Deploy.Language_WordGroup), global::Deploy.Language_WordGroup.Parser, new[]{ "Id", "WordIdsStart", "WordIdsEnd" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Deploy.Language_WordGroup_Map), global::Deploy.Language_WordGroup_Map.Parser, new[]{ "Items" }, null, null, null, new pbr::GeneratedClrTypeInfo[] { null, })
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class Language_WordGroup : pb::IMessage<Language_WordGroup> {
    private static readonly pb::MessageParser<Language_WordGroup> _parser = new pb::MessageParser<Language_WordGroup>(() => new Language_WordGroup());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Language_WordGroup> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Deploy.LanguageWordGroupReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Language_WordGroup() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Language_WordGroup(Language_WordGroup other) : this() {
      id_ = other.id_;
      wordIdsStart_ = other.wordIdsStart_;
      wordIdsEnd_ = other.wordIdsEnd_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Language_WordGroup Clone() {
      return new Language_WordGroup(this);
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

    /// <summary>Field number for the "WordIdsStart" field.</summary>
    public const int WordIdsStartFieldNumber = 2;
    private int wordIdsStart_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int WordIdsStart {
      get { return wordIdsStart_; }
      set {
        wordIdsStart_ = value;
      }
    }

    /// <summary>Field number for the "WordIdsEnd" field.</summary>
    public const int WordIdsEndFieldNumber = 3;
    private int wordIdsEnd_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int WordIdsEnd {
      get { return wordIdsEnd_; }
      set {
        wordIdsEnd_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Language_WordGroup);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Language_WordGroup other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Id != other.Id) return false;
      if (WordIdsStart != other.WordIdsStart) return false;
      if (WordIdsEnd != other.WordIdsEnd) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Id != 0) hash ^= Id.GetHashCode();
      if (WordIdsStart != 0) hash ^= WordIdsStart.GetHashCode();
      if (WordIdsEnd != 0) hash ^= WordIdsEnd.GetHashCode();
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
      if (WordIdsStart != 0) {
        output.WriteRawTag(16);
        output.WriteInt32(WordIdsStart);
      }
      if (WordIdsEnd != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(WordIdsEnd);
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
      if (WordIdsStart != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(WordIdsStart);
      }
      if (WordIdsEnd != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(WordIdsEnd);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Language_WordGroup other) {
      if (other == null) {
        return;
      }
      if (other.Id != 0) {
        Id = other.Id;
      }
      if (other.WordIdsStart != 0) {
        WordIdsStart = other.WordIdsStart;
      }
      if (other.WordIdsEnd != 0) {
        WordIdsEnd = other.WordIdsEnd;
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
            WordIdsStart = input.ReadInt32();
            break;
          }
          case 24: {
            WordIdsEnd = input.ReadInt32();
            break;
          }
        }
      }
    }

  }

  public sealed partial class Language_WordGroup_Map : pb::IMessage<Language_WordGroup_Map> {
    private static readonly pb::MessageParser<Language_WordGroup_Map> _parser = new pb::MessageParser<Language_WordGroup_Map>(() => new Language_WordGroup_Map());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Language_WordGroup_Map> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Deploy.LanguageWordGroupReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Language_WordGroup_Map() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Language_WordGroup_Map(Language_WordGroup_Map other) : this() {
      items_ = other.items_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Language_WordGroup_Map Clone() {
      return new Language_WordGroup_Map(this);
    }

    /// <summary>Field number for the "Items" field.</summary>
    public const int ItemsFieldNumber = 1;
    private static readonly pbc::MapField<int, global::Deploy.Language_WordGroup>.Codec _map_items_codec
        = new pbc::MapField<int, global::Deploy.Language_WordGroup>.Codec(pb::FieldCodec.ForInt32(8, 0), pb::FieldCodec.ForMessage(18, global::Deploy.Language_WordGroup.Parser), 10);
    private readonly pbc::MapField<int, global::Deploy.Language_WordGroup> items_ = new pbc::MapField<int, global::Deploy.Language_WordGroup>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::MapField<int, global::Deploy.Language_WordGroup> Items {
      get { return items_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Language_WordGroup_Map);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Language_WordGroup_Map other) {
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
    public void MergeFrom(Language_WordGroup_Map other) {
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