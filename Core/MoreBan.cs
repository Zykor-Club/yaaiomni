using System.Net;
using System.Text.RegularExpressions;
using TShockAPI;
using TShockAPI.DB;

namespace Chireiden.TShock.Omni;

public partial class Plugin
{
    private bool Detour_CheckBan_IP(Func<BanManager, TSPlayer, bool> orig, BanManager instance, TSPlayer player)
    {
        static bool Match(string pattern, TSPlayer player)
        {
            if (pattern.StartsWith("namea:"))
            {
                var pt = pattern[6..];
                try
                {
                    if (!Regex.IsMatch(player.Name, pt))
                    {
                        return false;
                    }
                }
                catch (ArgumentException ex)
                {
                    TShockAPI.TShock.Log.ConsoleError($"Ban pattern {pt} is invalid: {ex.Message}");
                    return false;
                }
            }
            else if (pattern.StartsWith("ipa:"))
            {
                var addr = pattern[4..].Split('/');
                if (addr.Length != 2
                    || !IPAddress.TryParse(addr[0], out var subnetAddr)
                    || !int.TryParse(addr[1], out var subnetMask))
                {
                    TShockAPI.TShock.Log.ConsoleError($"Ban pattern {pattern} is invalid.");
                    return false;
                }

                if (!IPAddress.TryParse(player.IP, out var ip))
                {
                    return false;
                }

                if (ip.AddressFamily != subnetAddr.AddressFamily)
                {
                    return false;
                }

                byte ReverseBits(byte v)
                {
                    var b = ((v & 0b11110000) >> 4) | ((v & 0b00001111) << 4);
                    b = ((b & 0b11001100) >> 2) | ((b & 0b00110011) << 2);
                    b = ((b & 0b10101010) >> 1) | ((b & 0b01010101) << 1);
                    return (byte) b;
                }

                System.Collections.BitArray GetBitArray(IPAddress ip) => new System.Collections.BitArray(ip.GetAddressBytes().Select(ReverseBits).ToArray());

                // Check network portion (first subnetMask bits) — if any differ, not a match
                var xor = GetBitArray(ip).Xor(GetBitArray(subnetAddr));
                if (subnetMask < 0 || subnetMask > xor.Length)
                {
                    return false;
                }
                for (var i = 0; i < subnetMask; i++)
                {
                    if (xor[i])
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        if (orig(instance, player))
        {
            return true;
        }

        if (!this.config.Enhancements.Value.BanPattern)
        {
            return false;
        }

        foreach (var kvp in TShockAPI.TShock.Bans.Bans)
        {
            var ban = kvp.Value;
            if (Match(ban.Identifier, player) && TShockAPI.TShock.Bans.IsValidBan(ban, player))
            {
                if (ban.ExpirationDateTime == DateTime.MaxValue)
                {
                    player.Disconnect($"#{ban.TicketNumber} - You are banned: {ban.Reason}");
                }
                else
                {
                    player.Disconnect($"#{ban.TicketNumber} - You are banned: {ban.Reason} ({ban.GetPrettyExpirationString()} remaining)");
                }
                return true;
            }
        }
        return false;
    }
}
