using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace StatBot
{
    class SheetsWrapper
    {
        StatBot main;

        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "Google Sheets API for StatBot";

        SheetsService service;

        public SheetsWrapper(StatBot main)
        {
            this.main = main;

            UserCredential credential;

            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                //string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                //credPath = Path.Combine(credPath, ".credentials/sheets.googleapis.com-dotnet-statbot.json");
                string credPath = ".credentials/sheets.googleapis.com-dotnet-statbot.json";

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        public void UpdateUser(PlayerQuestions player)
        {
            int rowNum = GetRowNum(player.userID);

            string spreadSheetId = "1QjRNBh9_2SOdPQPN2JAjW_2TWl_LOGAGGsFD3ZNqNG0";
            string range = String.Format("PlayerStats!A{0}:O", rowNum);

            ValueRange valueRange = new ValueRange()
            {
                MajorDimension = "ROWS"
            };

            var oblist = player.GetList();
            valueRange.Values = new List<IList<object>> { oblist };

            SpreadsheetsResource.ValuesResource.UpdateRequest update = service.Spreadsheets.Values.Update(valueRange, spreadSheetId, range);
            update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            UpdateValuesResponse result = update.Execute();
        }

        public int GetRowNum(ulong userID)
        {
            int rowNum = 2;

            IList<IList<Object>> values = GetTableValues();

            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    if (row[0] == null)
                    {
                        return rowNum;
                    }

                    ulong rowID = Convert.ToUInt64(row[0]);
                    if (rowID == userID)
                    {
                        return rowNum;
                    }
                    rowNum++;
                }
            }

            return rowNum;
        }

        public bool IsUserInTable(ulong userID)
        {
            bool found = false;

            IList<IList<Object>> values = GetTableValues();

            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    ulong rowID = Convert.ToUInt64(row[0]);
                    if (rowID == userID)
                    {
                        found = true;
                    }
                }
            }

            return found;
        }

        public IList<IList<Object>> GetTableValues()
        {
            string spreadSheetId = "1QjRNBh9_2SOdPQPN2JAjW_2TWl_LOGAGGsFD3ZNqNG0";
            string range = "PlayerStats!A2:N";
            SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(spreadSheetId, range);

            ValueRange response = request.Execute();

            return response.Values;
        }
    }
}
