using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.GenericAntag;
using Content.Server.Ninja.Systems;
using Content.Server.StationEvents.Events;
using Content.Server.Zombies;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Verbs;
using Robust.Shared.Utility;
using Robust.Shared.Player;

namespace Content.Server.Administration.Systems;

public sealed partial class AdminVerbSystem
{
    [Dependency] private readonly ZombieSystem _zombie = default!;
    [Dependency] private readonly ThiefRuleSystem _thief = default!;
    [Dependency] private readonly TraitorRuleSystem _traitorRule = default!;
    [Dependency] private readonly NukeopsRuleSystem _nukeopsRule = default!;
    [Dependency] private readonly PiratesRuleSystem _piratesRule = default!;
    [Dependency] private readonly RevolutionaryRuleSystem _revolutionaryRule = default!;
    [Dependency] private readonly SharedMindSystem _minds = default!;

    // All antag verbs have names so invokeverb works.
    private void AddAntagVerbs(GetVerbsEvent<Verb> args)
    {
        if (!TryComp<ActorComponent>(args.User, out var actor))
            return;

        var player = actor.PlayerSession;

        if (!_adminManager.HasAdminFlag(player, AdminFlags.Fun))
            return;

        if (!TryComp<MindContainerComponent>(args.Target, out var targetMindComp))
            return;

        Verb traitor = new()
        {
            Text = Loc.GetString("admin-verb-text-make-traitor"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/Structures/Wallmounts/posters.rsi"),
                "poster5_contraband"),
            Act = () =>
            {
                if (!_minds.TryGetSession(targetMindComp.Mind, out var session))
                    return;

                // if its a monkey or mouse or something dont give uplink or objectives
                var isHuman = HasComp<HumanoidAppearanceComponent>(args.Target);
                _traitorRule.MakeTraitor(session, giveUplink: isHuman, giveObjectives: isHuman);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-traitor"),
        };
        args.Verbs.Add(traitor);

        Verb blobAntag = new()
        {
            Text = Loc.GetString("admin-verb-text-make-blob"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Backmen/Interface/Actions/blob.rsi"), "blobFactory"),
            Act = () =>
            {
                EnsureComp<Shared.Backmen.Blob.BlobCarrierComponent>(args.Target).HasMind = targetMindComp.HasMind;
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-text-make-blob"),
        };
        args.Verbs.Add(blobAntag);

        Verb fleshLeaderCultist = new()
        {
            Text = Loc.GetString("admin-verb-text-make-flesh-leader-cultist"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Structures/flesh_heart.rsi"), "base_heart"),
            Act = () =>
            {
                if (!_minds.TryGetSession(targetMindComp.Mind, out var session))
                    return;

                EntityManager.System<Content.Server.Backmen.GameTicking.Rules.FleshCultRuleSystem>()
                    .MakeCultist(session);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-text-make-flesh-leader-cultist"),
        };
        args.Verbs.Add(fleshLeaderCultist);

        Verb fleshCultist = new()
        {
            Text = Loc.GetString("admin-verb-text-make-flesh-cultist"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Mobs/Aliens/FleshCult/flesh_cult_mobs.rsi"), "worm"),
            Act = () =>
            {
                if (!_minds.TryGetSession(targetMindComp.Mind, out var session))
                    return;

                EntityManager.System<Content.Server.Backmen.GameTicking.Rules.FleshCultRuleSystem>()
                    .MakeCultist(session);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-text-make-flesh-cultist"),
        };
        args.Verbs.Add(fleshCultist);

        Verb EvilTwin = new()
        {
            Text = "Make EvilTwin",
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi((new ResPath("/Textures/Structures/Wallmounts/posters.rsi")),
                "poster3_legit"),
            Act = () =>
            {
                if (!_minds.TryGetSession(targetMindComp.Mind, out var session))
                    return;

                EntityManager.System<Content.Server.Backmen.EvilTwin.EvilTwinSystem>()
                    .MakeTwin(out _, session.AttachedEntity);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-eviltwin"),
        };
        args.Verbs.Add(EvilTwin);

        Verb zombie = new()
        {
            Text = Loc.GetString("admin-verb-text-make-zombie"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/Actions/zombie-turn.png")),
            Act = () =>
            {
                _zombie.ZombifyEntity(args.Target);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-zombie"),
        };
        args.Verbs.Add(zombie);


        Verb nukeOp = new()
        {
            Text = Loc.GetString("admin-verb-text-make-nuclear-operative"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Structures/Wallmounts/signs.rsi"), "radiation"),
            Act = () =>
            {
                if (!_minds.TryGetMind(args.Target, out var mindId, out var mind))
                    return;

                _nukeopsRule.MakeLoneNukie(mindId, mind);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-nuclear-operative"),
        };
        args.Verbs.Add(nukeOp);

        Verb pirate = new()
        {
            Text = Loc.GetString("admin-verb-text-make-pirate"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Clothing/Head/Hats/pirate.rsi"), "icon"),
            Act = () =>
            {
                if (!_minds.TryGetMind(args.Target, out var mindId, out var mind))
                    return;

                _piratesRule.MakePirate(mindId, mind);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-pirate"),
        };
        args.Verbs.Add(pirate);

        //todo come here at some point dear lort.
        Verb headRev = new()
        {
            Text = Loc.GetString("admin-verb-text-make-head-rev"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Interface/Misc/job_icons.rsi"), "HeadRevolutionary"),
            Act = () =>
            {
                if (!_minds.TryGetMind(args.Target, out var mindId, out var mind))
                    return;
                _revolutionaryRule.OnHeadRevAdmin(mindId, mind);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-head-rev"),
        };
        args.Verbs.Add(headRev);

        Verb thief = new()
        {
            Text = Loc.GetString("admin-verb-text-make-thief"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/Clothing/Hands/Gloves/ihscombat.rsi"), "icon"),
            Act = () =>
            {
                if (!_minds.TryGetSession(targetMindComp.Mind, out var session))
                    return;

                _thief.AdminMakeThief(session, false); //Midround add pacific is bad
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-thief"),
        };
        args.Verbs.Add(thief);
    }
}
