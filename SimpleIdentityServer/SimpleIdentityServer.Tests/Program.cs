using SimpleIdentityServer.Core.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            string idToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6Im9wMSJ9.eyJzdWIiOiAiMWIyZmM5MzQxYTE2YWU0ZTMwMDgyOTY1ZDUzN2FlNDdjMjFhMGYyN2ZkNDNlYWI3ODMzMGVkODE3NTFhZTZkYiIsICJpc3MiOiAiaHR0cHM6Ly9vaWN0ZXN0LnVtZGMudW11LnNlOjgwNjAvIiwgImFjciI6ICJQQVNTV09SRCIsICJleHAiOiAxNDQzMjc3MjA2LCAiYXV0aF90aW1lIjogMTQ0MzE5MDQyMiwgImlhdCI6IDE0NDMxODcyMDYsICJhdWQiOiBbIjhvTFM3SzFoZWM5eCJdfQ.zv9r79FoWl-2mhLGbEmsgqiL7KwiNR2Eox4QQ631p1ii8uZJtzlda5A-sNxnP6eKkaldFlYm1jAWgEAbC8MHxUKSGAF93F6s5QG00RB73MuUH0xgchuFeuCuHbGKZtj1esqulh8Pduvjz1LMFq4C-P3vPJkmJ30wjnTc4-oE9lqbRmIdtMaX1ntik-U8cDzztASWZljFcF_FU0dasA55iZYwEVsIpg-T_wsFIMr1asmDEgoSWKBhf29Uw88O1QJrf6Abcf7iw-n39qOrL9dAgUITsxJlo_GYB_OmeEX1hunGPzpJIISGNbS2b7TElJqOMomzlTIe6o_HYLwhgjry7g";
            var idTokenSplitted = idToken.Split('.');
            var header = idTokenSplitted[0].Base64Decode();
            var body = idTokenSplitted[1].Base64Decode();
            var signature = idTokenSplitted[2].Base64Decode();
            Console.ReadLine();
        }
    }
}
