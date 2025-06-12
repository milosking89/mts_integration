using System.Net.Http;
using ClosedXML.Excel;
using mts_integration.DTO;

namespace mts_integration.Application.DataService
{
    public class GenerateData : IGenerateData
    {
        public GenerateData()
        {
            //TODO
        }

        public async Task<byte[]> GenerateDevicesExcel(List<DtoDevicesData> data)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Data");

                worksheet.Cell(1, 1).Value = "EUI";
                worksheet.Cell(1, 2).Value = "nwAddress";
                worksheet.Cell(1, 3).Value = "name";
                worksheet.Cell(1, 4).Value = "connectivity";
                worksheet.Cell(1, 5).Value = "Profile ID";
                worksheet.Cell(1, 6).Value = "Profile Name";

                worksheet.Row(1).Style.Font.Bold = true;

                int currentRow = 2;

                for (int i = 0; i < data.Count; i++)
                {
                    var device = data[i];

                    var filtered = device.Briefs.GroupBy(x => x.EUI).Select(g => g.First()).ToList();

                    foreach (var brief in filtered)
                    {
                        worksheet.Cell(currentRow, 1).Value = brief.EUI;
                        worksheet.Cell(currentRow, 2).Value = brief.nwAddress;
                        worksheet.Cell(currentRow, 3).Value = brief.name;
                        worksheet.Cell(currentRow, 4).Value = brief.connectivity;
                        worksheet.Cell(currentRow, 5).Value = brief.appServersRoutingProfile["ID"];
                        worksheet.Cell(currentRow, 6).Value = brief.appServersRoutingProfile["name"];
                        currentRow++;
                    }
                }


                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return await Task.FromResult(stream.ToArray());
                }
            }
        }
        public async Task<byte[]> GenerateDeviceExcelByName(List<DtoDeviceData> data)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Data");

                worksheet.Cell(1, 1).Value = "NAME";
                worksheet.Cell(1, 2).Value = "EUI";
                worksheet.Cell(1, 3).Value = "IMSI";
                worksheet.Cell(1, 4).Value = "nwAddress";
                worksheet.Cell(1, 5).Value = "lastCellID";

                worksheet.Row(1).Style.Font.Bold = true;

                int currentRow = 2;

                for (int i = 0; i < data.Count; i++)
                {
                    worksheet.Cell(currentRow, 1).Value = data[i].name;
                    worksheet.Cell(currentRow, 2).Value = data[i].EUI;
                    worksheet.Cell(currentRow, 3).Value = data[i].imsi;
                    worksheet.Cell(currentRow, 4).Value = data[i].nwAddress;
                    worksheet.Cell(currentRow, 5).Value = data[i].lastCellID;
                    currentRow++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return await Task.FromResult(stream.ToArray());
                }
            }
        }
    }
}
