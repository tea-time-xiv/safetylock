using System;
using System.Linq;
using Dalamud.Game;

namespace SafetyLockPlugin;

// All translatable strings live here.
// Match strings: every locale's substrings are scanned every time (no client-language check).
// Chat output: selected by Plugin.ClientState.ClientLanguage with EN fallback.
public static class Localization
{
    private static ClientLanguage Lang => Plugin.ClientState.ClientLanguage;

    public static bool ContainsAny(string text, string[] candidates) =>
        candidates.Any(c => text.Contains(c, StringComparison.OrdinalIgnoreCase));

    public static bool StartsWithAny(string text, string[] candidates) =>
        candidates.Any(c => text.StartsWith(c, StringComparison.OrdinalIgnoreCase));

    // ---- Match-bearing features ------------------------------------------

    public static class AdditionalChambers
    {
        public static readonly string[] MatchStrings =
        {
            "private chambers",   // EN
            "company workshop",   // EN
            // DE: TODO
            // FR: TODO
            // JP: TODO
        };
        public static string BlockedMessage => Lang switch
        {
            _ => "Additional chambers access blocked (Safety Lock enabled)",
        };
    }

    public static class GrandCompanyPersonnel
    {
        public static readonly string[] MatchStrings =
        {
            "Undertake supply and provisioning missions", // EN
            "Apply for transfer",                          // EN
            // DE/FR/JP: TODO
        };
        public static string BlockedMessage => Lang switch
        {
            _ => "Grand Company personnel interaction blocked (Safety Lock enabled)",
        };
    }

    public static class HousingFood
    {
        public static readonly string[] MatchStrings =
        {
            "Partake of the",  // EN
            "Parttake of the", // EN typo-resilient variant
            // DE/FR/JP: TODO
        };
        public static string BlockedMessage => Lang switch
        {
            _ => "Housing food consumption blocked (Safety Lock enabled)",
        };
    }

    public static class HousingPictureFrame
    {
        public static readonly string[] MatchStrings =
        {
            "Angler's Print", // EN
            "On Display:",    // EN
            // DE/FR/JP: TODO
        };
        public static string BlockedMessage => Lang switch
        {
            _ => "Housing picture frame access blocked (Safety Lock enabled)",
        };
    }

    public static class LevemeteMenu
    {
        public static readonly string[] MatchStrings =
        {
            "Battlecraft Leves",    // EN
            "Fieldcraft Leves",     // EN
            "Tradecraft Leves",     // EN
            "Information on leves", // EN
            // DE/FR/JP: TODO
        };
        public static string BlockedMessage => Lang switch
        {
            _ => "Levequest interaction blocked (Safety Lock enabled)",
        };
    }

    // ---- Chat-only features (no game-text matching needed) ---------------

    public static class Vendor
    {
        public static string BlockedMessage => Lang switch
        {
            _ => "Vendor interaction blocked (Safety Lock enabled)",
        };
    }

    public static class DutyFinder
    {
        public static string BlockedMessage => Lang switch
        {
            _ => "Duty Finder blocked (Safety Lock enabled)",
        };
    }

    public static class Quest
    {
        public static string BlockedMessage => Lang switch
        {
            _ => "Quest interaction blocked (Safety Lock enabled)",
        };
    }

    public static class GlamourDresser
    {
        public static string BlockedMessage => Lang switch
        {
            _ => "Glamour Dresser blocked (Safety Lock enabled)",
        };
    }

    public static class Armoire
    {
        public static string BlockedMessage => Lang switch
        {
            _ => "Armoire blocked (Safety Lock enabled)",
        };
    }

    public static class FreeCompanyChest
    {
        public static string BlockedMessage => Lang switch
        {
            _ => "Free Company Chest interaction blocked (Safety Lock enabled)",
        };
    }

    public static class HousingRetainer
    {
        public static string BlockedMessage => Lang switch
        {
            _ => "Housing retainer interaction blocked (Safety Lock enabled)",
        };
    }

    public static class Levequest
    {
        public static string BlockedMessage => Lang switch
        {
            _ => "Levequest acceptance blocked (Safety Lock enabled)",
        };
    }

    public static class GrandCompanySealExchange
    {
        public static string BlockedMessage => Lang switch
        {
            _ => "Grand Company Seal Exchange blocked (Safety Lock enabled)",
        };
    }
}
