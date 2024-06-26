﻿using Newtonsoft.Json;

namespace ScratchScript.Compiler.Models;

public record Block
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? Comment;

    public Dictionary<string, List<object>> Fields = [];

    [NonSerialized] public string Id;
    public Dictionary<string, List<object>> Inputs = [];

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Mutation? Mutation;

    public string Next;
    public string Opcode;
    public string Parent;
    public bool Shadow;
    public bool TopLevel;
    public float X;
    public float Y;
}