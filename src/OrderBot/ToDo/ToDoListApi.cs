using Discord;
using Microsoft.EntityFrameworkCore;
using OrderBot.Core;
using OrderBot.Discord;
using OrderBot.EntityFramework;
using System.Transactions;

namespace OrderBot.ToDo;

/// <summary>
/// Separate the core functionalitiy for automated testing, potential reuse and command aliasing.
/// </summary>
/// <remarks>
/// For implementors:
/// <list type="number">
/// <li>
/// Code as you would normally in C#, e.g. throw exceptions for errors. It is the wrapping code's 
/// responsibility to respond to the caller, audit and log details.
/// </li>
/// <li>
/// The class is stateless. Do not save or require state from one call to the next. Passing in 
/// the <see cref="OrderBotDbContext"/> in the constructor forces the caller to instantiate a
/// new instance each interaction, anyway.
/// </li>
/// <li>
/// The caller should always wrap calls in a <see cref="TransactionScope"> passing
/// <see cref="TransactionScopeAsyncFlowOption.Enabled"/> to ensure consistency even with 
/// async/await. The methods in this class cannot do it themselves because exceptions may be 
/// thrown as part of normal behaviour, in turn throwing a <see cref="TransactionAbortedException"/>.
/// </li>
/// <li>
/// Since the code uses <see cref="TransactionScope"/> for transactions, avoid async/await.
/// While this decision means a small decrease in responsiveness
/// </li>
/// </list>
/// </remarks>
public class ToDoListApi
{
    /// <summary>
    /// Create a new <see cref="ToDoListApi"/>.
    /// </summary>
    /// <param name="dbContext">
    /// The <see cref="OrderBotDbContext"/> to use.
    /// </param>
    /// <param name="validator">
    /// Used to validate minor factions and star systems via web services.
    /// </param>
    public ToDoListApi(OrderBotDbContext dbContext, INameValidator validator)
    {
        DbContext = dbContext;
        Validator = validator;
    }

    internal OrderBotDbContext DbContext { get; }
    public INameValidator Validator { get; }

    /// <summary>
    /// Get the list of suggestions.
    /// </summary>
    /// <param name="guild">
    /// Generate the list for this guild.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Either there is no <see cref="DiscordGuild"/> for <paramref name="guild"/>
    /// in the database, that guild supports no minor factions.
    /// </exception>
    /// <exception cref="UnknownGoalException">
    /// A goal in a star system for a minor faction is not known.
    /// </exception>
    public string GetTodoList(IGuild guild)
    {
        return new ToDoListFormatter().Format(new ToDoListGenerator(DbContext).Generate(guild.Id));
    }

    /// <summary>
    /// Set the supported minor faction.
    /// </summary>
    /// <param name="guild">
    /// Use this guild.
    /// </param>
    /// <param name="minorFactionName">
    /// The minor faction to support.
    /// </param>
    /// <exception cref="UnknownMinorFactionException">
    /// <paramref name="minorFactionName"/> is not a known or valid minor faction.
    /// </exception>
    public async Task SetSupportedMinorFactionAsync(IGuild guild, string minorFactionName)
    {
        MinorFaction? minorFaction = DbContext.MinorFactions.FirstOrDefault(mf => mf.Name == minorFactionName);
        if (minorFaction == null && await Validator.IsKnownMinorFaction(minorFactionName))
        {
            minorFaction = new() { Name = minorFactionName };
            DbContext.MinorFactions.Add(minorFaction);
            DbContext.SaveChanges();
        }
        if (minorFaction == null)
        {
            throw new UnknownMinorFactionException(minorFactionName);
        }

        DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, guild,
            DbContext.DiscordGuilds.Include(e => e.SupportedMinorFactions));
        discordGuild.SupportedMinorFactions.Clear();
        discordGuild.SupportedMinorFactions.Add(minorFaction);

        DbContext.SaveChanges();
    }

    /// <summary>
    /// Get the supported minor faction.
    /// </summary>
    /// <param name="guild">
    /// Use this guild.
    /// </param>
    /// <returns>
    /// The supported minor faction or <see cref="null"/> if there is none.
    /// </returns>
    public MinorFaction? GetSupportedMinorFaction(IGuild guild)
    {
        DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, guild,
            DbContext.DiscordGuilds.Include(e => e.SupportedMinorFactions));
        return discordGuild.SupportedMinorFactions.Any()
            ? discordGuild.SupportedMinorFactions.FirstOrDefault()
            : null;
    }

    /// <summary>
    /// Clear the supported minor faction.
    /// </summary>
    /// <param name="guild">
    /// Use this guild.
    /// </param>
    public void ClearSupportedMinorFaction(IGuild guild)
    {
        DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, guild,
            DbContext.DiscordGuilds.Include(e => e.SupportedMinorFactions));
        discordGuild.SupportedMinorFactions.Clear();
        DbContext.SaveChanges();
    }

    /// <summary>
    /// Add goals.
    /// </summary>
    /// <param name="guild">
    /// Use this guild.
    /// </param>
    /// <param name="goals">
    /// The goal(s) to add.
    /// </param>
    /// <exception cref="UnknownMinorFactionException">
    /// An unknown minor faction was specified in <paramref name="goals"/>..
    /// </exception>
    /// <exception cref="UnknownStarSystemException">
    /// An unknown star system was specified in <paramref name="goals"/>..
    /// </exception>
    /// <exception cref="UnknownGoalException">
    /// An unknown goal was specified in <paramref name="goals"/>..
    /// </exception>
    public async Task AddGoals(IGuild guild,
        IEnumerable<(string minorFactionName, string starSystemName, string goalName)> goals)
    {
        DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, guild);

        foreach ((string minorFactionName, string starSystemName, string goalName) in goals)
        {
            MinorFaction? minorFaction = DbContext.MinorFactions.FirstOrDefault(mf => mf.Name == minorFactionName);
            if (minorFaction == null && await Validator.IsKnownMinorFaction(minorFactionName))
            {
                minorFaction = new MinorFaction() { Name = minorFactionName };
                DbContext.MinorFactions.Add(minorFaction);
                DbContext.SaveChanges();
            }
            if (minorFaction == null)
            {
                throw new UnknownMinorFactionException(minorFactionName);
            }

            StarSystem? starSystem = DbContext.StarSystems.FirstOrDefault(ss => ss.Name == starSystemName);
            if (starSystem == null && await Validator.IsKnownStarSystem(starSystemName))
            {
                starSystem = new StarSystem() { Name = starSystemName };
                DbContext.StarSystems.Add(starSystem);
                DbContext.SaveChanges();
            }
            if (starSystem == null)
            {
                throw new UnknownStarSystemException(starSystemName);
            }

            if (!Goals.Map.TryGetValue(goalName, out Goal? goal))
            {
                throw new UnknownGoalException(goalName, starSystemName, minorFactionName);
            }

            Presence? starSystemMinorFaction =
                DbContext.Presences.Include(ssmf => ssmf.StarSystem)
                                   .Include(ssmf => ssmf.MinorFaction)
                                   .FirstOrDefault(ssmf => ssmf.StarSystem.Name == starSystemName
                                                        && ssmf.MinorFaction.Name == minorFactionName);
            if (starSystemMinorFaction == null)
            {
                starSystemMinorFaction = new Presence() { MinorFaction = minorFaction, StarSystem = starSystem };
                DbContext.Presences.Add(starSystemMinorFaction);
            }

            DiscordGuildPresenceGoal? discordGuildStarSystemMinorFactionGoal =
                DbContext.DiscordGuildPresenceGoals
                         .Include(dgssmfg => dgssmfg.Presence)
                         .Include(dgssmfg => dgssmfg.Presence.StarSystem)
                         .Include(dgssmfg => dgssmfg.Presence.MinorFaction)
                         .FirstOrDefault(
                             dgssmfg => dgssmfg.DiscordGuild == discordGuild
                                     && dgssmfg.Presence.MinorFaction == minorFaction
                                     && dgssmfg.Presence.StarSystem == starSystem);
            if (discordGuildStarSystemMinorFactionGoal == null)
            {
                discordGuildStarSystemMinorFactionGoal = new DiscordGuildPresenceGoal()
                { DiscordGuild = discordGuild, Presence = starSystemMinorFaction };
                DbContext.DiscordGuildPresenceGoals.Add(discordGuildStarSystemMinorFactionGoal);
            }
            discordGuildStarSystemMinorFactionGoal.Goal = goalName;
            DbContext.SaveChanges();
        }
    }

    /// <summary>
    /// List goals.
    /// </summary>
    /// <param name="guild">
    /// Use this guild.
    /// </param>
    /// <returns>
    /// The goals.
    /// </returns>
    public IEnumerable<DiscordGuildPresenceGoal> ListGoals(IGuild guild)
    {
        DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, guild);
        return DbContext.DiscordGuildPresenceGoals
                        .Include(dgssmfg => dgssmfg.Presence)
                        .Include(dgssmfg => dgssmfg.Presence.StarSystem)
                        .Include(dgssmfg => dgssmfg.Presence.MinorFaction);
    }

    /// <summary>
    /// Remove a goal.
    /// </summary>
    /// <param name="guild">
    /// Use this guild.
    /// </param>
    /// <param name="minorFactionName">
    /// </param>
    /// <param name="starSystemName">
    /// </param>
    /// <exception cref="UnknownMinorFactionException">
    /// <paramref name="minorFactionName"/> is not a valid minor faction.
    /// </exception>
    /// <exception cref="UnknownStarSystemException">
    /// <paramref name="starSystemName"/> is not a valid star system.
    /// </exception>
    public void RemoveGoal(IGuild guild, string minorFactionName,
        string starSystemName)
    {
        DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, guild);

        MinorFaction? minorFaction = DbContext.MinorFactions.FirstOrDefault(mf => mf.Name == minorFactionName);
        if (minorFaction == null)
        {
            throw new UnknownMinorFactionException(minorFactionName);
        }

        StarSystem? starSystem = DbContext.StarSystems.FirstOrDefault(ss => ss.Name == starSystemName);
        if (starSystem == null)
        {
            throw new UnknownStarSystemException(starSystemName);
        }

        DiscordGuildPresenceGoal? discordGuildStarSystemMinorFactionGoal =
            DbContext.DiscordGuildPresenceGoals
                        .Include(dgssmfg => dgssmfg.Presence)
                        .Include(dgssmfg => dgssmfg.Presence.StarSystem)
                        .Include(dgssmfg => dgssmfg.Presence.MinorFaction)
                        .FirstOrDefault(
                            dgssmfg => dgssmfg.DiscordGuild == discordGuild
                                    && dgssmfg.Presence.MinorFaction == minorFaction
                                    && dgssmfg.Presence.StarSystem == starSystem);
        if (discordGuildStarSystemMinorFactionGoal != null)
        {
            DbContext.DiscordGuildPresenceGoals.Remove(discordGuildStarSystemMinorFactionGoal);
        }
        DbContext.SaveChanges();
    }
}
