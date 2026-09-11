using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpringClient
{
    public class OffsetData
    {
        public long dwViewMatrix { get; set; } = 37560816; // 0x23D1730
        public long dwEntityList { get; set; } = 39287776; // 0x2576BE0
        public long dwLocalPlayerPawn { get; set; } = 37538824; // 0x23CC2A8
        public long dwLocalPlayerController { get; set; } = 37386448; // 0x23A6ED0
        public long dwViewAngles { get; set; } = 37629080; // 0x23E0CB8
        public long dwCSGOInput { get; set; } = 37627408; // 0x23E07B0
        public long dwPlantedC4 { get; set; } = 37319608; // 0x23969B8
        public long dwGlobalVars { get; set; } = 37381280;

        // Buttons
        public long jump { get; set; } = 34316304; // 0x20B8210
        public long attack { get; set; } = 34315008; // 0x20B7D00
        public long attack2 { get; set; } = 34315152; // 0x20B7D90
        public long duck { get; set; } = 34316448; // 0x20B82A0

        // Client schema offsets
        public long m_iHealth { get; set; } = 0x344;
        public long m_iTeamNum { get; set; } = 0x3E3;
        public long m_vOldOrigin { get; set; } = 0x1324;
        public long m_pClippingWeapon { get; set; } = 0x13A0;
        public long m_aimPunchAngle { get; set; } = 0x187C;
        public long m_iShotsFired { get; set; } = 0x23E4;
        public long m_hPlayerPawn { get; set; } = 0x80C;
        public long m_lifeState { get; set; } = 0x348;
        public long m_bIsScoped { get; set; } = 0x23D8;
        public long m_fFlags { get; set; } = 0x3EC;

        public DateTime LastSynced { get; set; } = DateTime.UtcNow;
        public string Source { get; set; } = "Embedded Cache (sezzyaep/CS2-OFFSETS)";
    }

    public static class OffsetManager
    {
        private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(5) };
        public static OffsetData Current { get; private set; } = new OffsetData();

        private const string OFFSETS_URL = "https://raw.githubusercontent.com/sezzyaep/CS2-OFFSETS/main/offsets.json";
        private const string BUTTONS_URL = "https://raw.githubusercontent.com/sezzyaep/CS2-OFFSETS/main/buttons.json";
        private const string CLIENT_DLL_URL = "https://raw.githubusercontent.com/sezzyaep/CS2-OFFSETS/main/client_dll.json";

        public static async Task<(bool success, string message)> SyncOffsetsAsync()
        {
            try
            {
                var offsetsTask = _httpClient.GetStringAsync(OFFSETS_URL);
                var buttonsTask = _httpClient.GetStringAsync(BUTTONS_URL);
                
                await Task.WhenAll(offsetsTask, buttonsTask);

                string offsetsJson = await offsetsTask;
                string buttonsJson = await buttonsTask;

                using var docOffsets = JsonDocument.Parse(offsetsJson);
                if (docOffsets.RootElement.TryGetProperty("client.dll", out var clientElem))
                {
                    if (clientElem.TryGetProperty("dwViewMatrix", out var vMatrix)) Current.dwViewMatrix = vMatrix.GetInt64();
                    if (clientElem.TryGetProperty("dwEntityList", out var entList)) Current.dwEntityList = entList.GetInt64();
                    if (clientElem.TryGetProperty("dwLocalPlayerPawn", out var lpPawn)) Current.dwLocalPlayerPawn = lpPawn.GetInt64();
                    if (clientElem.TryGetProperty("dwLocalPlayerController", out var lpCtrl)) Current.dwLocalPlayerController = lpCtrl.GetInt64();
                    if (clientElem.TryGetProperty("dwViewAngles", out var vAngles)) Current.dwViewAngles = vAngles.GetInt64();
                    if (clientElem.TryGetProperty("dwCSGOInput", out var csInput)) Current.dwCSGOInput = csInput.GetInt64();
                    if (clientElem.TryGetProperty("dwPlantedC4", out var c4)) Current.dwPlantedC4 = c4.GetInt64();
                    if (clientElem.TryGetProperty("dwGlobalVars", out var gVars)) Current.dwGlobalVars = gVars.GetInt64();
                }

                using var docButtons = JsonDocument.Parse(buttonsJson);
                if (docButtons.RootElement.TryGetProperty("client.dll", out var btnElem))
                {
                    if (btnElem.TryGetProperty("jump", out var jmp)) Current.jump = jmp.GetInt64();
                    if (btnElem.TryGetProperty("attack", out var atk)) Current.attack = atk.GetInt64();
                    if (btnElem.TryGetProperty("attack2", out var atk2)) Current.attack2 = atk2.GetInt64();
                    if (btnElem.TryGetProperty("duck", out var dck)) Current.duck = dck.GetInt64();
                }

                Current.LastSynced = DateTime.UtcNow;
                Current.Source = "Live GitHub (sezzyaep/CS2-OFFSETS)";

                return (true, $"Offsets synced live! dwLocalPlayerPawn: 0x{Current.dwLocalPlayerPawn:X}, dwViewMatrix: 0x{Current.dwViewMatrix:X}, dwEntityList: 0x{Current.dwEntityList:X}");
            }
            catch (Exception ex)
            {
                Current.Source = "Fallback Cache (sezzyaep/CS2-OFFSETS verified)";
                return (false, $"Using cached offsets (0x{Current.dwLocalPlayerPawn:X}). Network info: {ex.Message}");
            }
        }

        public static string FormatHex(long offset) => $"0x{offset:X}";
    }
}
