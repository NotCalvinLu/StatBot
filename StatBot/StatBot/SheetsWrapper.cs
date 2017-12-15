using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;

namespace StatBot
{
    public class SheetsWrapper
    {
        private readonly SheetsService _sheetService;
        private readonly string _sheetId;

        public SheetsWrapper()
        {
            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets
            {
                ClientId = ConfigurationManager.AppSettings.Get("GoogleApi_ClientId"),
                ClientSecret = ConfigurationManager.AppSettings.Get("GoogleApi_ClientSecret")
            },
            new List<string> { SheetsService.Scope.Spreadsheets },
            "user",
            CancellationToken.None).Result;

            _sheetService = new SheetsService(new BaseClientService.Initializer
            {
                ApplicationName = ConfigurationManager.AppSettings.Get("GoogleApi_Name"),
                HttpClientInitializer = credential
            });
            _sheetId = ConfigurationManager.AppSettings.Get("GoogleApi_SheetId");
        }

        public void UpdateUser(StatData statData)
        {
            int rowNum = GetRowNum(statData.UserId);

            string range = String.Format("PlayerStats!A{0}:O", rowNum);

            ValueRange valueRange = new ValueRange()
            {
                MajorDimension = "ROWS"
            };

            var oblist = statData.GetDataToPrint();
            valueRange.Values = new List<IList<object>> { oblist };

            SpreadsheetsResource.ValuesResource.UpdateRequest update = _sheetService.Spreadsheets.Values.Update(valueRange, _sheetId, range);
            update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            UpdateValuesResponse result = update.Execute();
        }

        public bool DeleteUser(ulong id)
        {
            if (!IsUserInTable(id)) return false;

            int row = GetRowNum(id);

            Request requestBody = new Request()
            {
                DeleteDimension = new DeleteDimensionRequest()
                {
                    Range = new DimensionRange()
                    {
                        SheetId = 0,
                        Dimension = "ROWS",
                        StartIndex = row - 1,
                        EndIndex = row
                    }
                }
            };

            List<Request> requestContainer = new List<Request>();
            requestContainer.Add(requestBody);

            BatchUpdateSpreadsheetRequest deleteRequest = new BatchUpdateSpreadsheetRequest();
            deleteRequest.Requests = requestContainer;

            SpreadsheetsResource.BatchUpdateRequest deletion = new SpreadsheetsResource.BatchUpdateRequest(_sheetService, deleteRequest, _sheetId);
            deletion.Execute();

            return true;
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
            string range = "PlayerStats!A2:O";
            SpreadsheetsResource.ValuesResource.GetRequest request = _sheetService.Spreadsheets.Values.Get(_sheetId, range);

            ValueRange response = request.Execute();

            return response.Values;
        }
    }
}
