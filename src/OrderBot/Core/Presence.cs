﻿namespace OrderBot.Core
{
    public record Presence
    {
        public int Id { get; }
        public StarSystem StarSystem { get; init; } = null!;
        public MinorFaction MinorFaction { get; init; } = null!;
        public double Influence;
        public string? SecurityLevel;
        public List<State> States { get; } = new();
    }
}