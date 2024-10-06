using UploadFiles.Data;
using UploadFiles.Models;
using UploadFiles.Interfaces;
using CsvHelper;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace UploadFiles.Services
{
    public enum ColumnMaxLength
    {
        TransactionId = 50,
        AccountNumber = 30,
        CurrencyCode = 3
    }

    public enum Status
    {
        Approved,
        Failed,
        Finished,
        Rejected,
        Done
    }

    public class TransactionDataService : ITransactionDataService
    {
        private readonly TransactionDataContext _context;
        public TransactionDataService(TransactionDataContext context)
        {
            _context = context;
        }

        public static int GetIntFromEnumValue(ColumnMaxLength paymentMethod)
        {
            return (int)paymentMethod;
        }

        public async Task<ResponseUploadFile> UploadFile(IFormFile file)
        {
            ResponseUploadFile response = new ResponseUploadFile();
            try
            {
                string fiilePath = Path.GetFullPath(file.FileName);

                //##### Check file format #####
                if (file.FileName.Split(".")[1].ToUpper() == "CSV" || file.FileName.Split(".")[1].ToUpper() == "XML")
                {
                    //##### Check File #####
                    if (!System.IO.File.Exists(fiilePath))
                    {
                        using (FileStream fileStream = System.IO.File.Create(fiilePath))
                        {
                            file.CopyTo(fileStream);
                            fileStream.Flush();
                        }
                    }
                    else
                    {
                        //##### Delete file and copy file in case of duplicate file #####
                        System.IO.File.Delete(fiilePath);
                        using (FileStream fileStream = System.IO.File.Create(fiilePath))
                        {
                            file.CopyTo(fileStream);
                            fileStream.Flush();
                        }
                    }

                    //##### Check CSV File #####
                    if (file.FileName.Split(".")[1].ToUpper() == "CSV")
                    {
                        response = await CSVFile(fiilePath);
                    }
                    else
                    {
                        //##### Check XML File #####
                    }
                }
                else
                {
                    response.Status = 400;
                    response.StatusText = "Unknown format";
                }

                return response;
            }
            catch(Exception ex)
            {
                response.Status = 500;
                response.StatusText = ex.Message;
                return response;
            }
        }

        public async Task<ResponseUploadFile> CSVFile(string filePath)
        {
            ResponseUploadFile response = new ResponseUploadFile();
            string path = string.Empty;
            try
            {
                await Task.Delay(0);
                List<TransactionData> transactionDataList = new List<TransactionData>();
                List<ResponseInvalidCSVFile> ResponseInvalidCSVFile = new List<ResponseInvalidCSVFile>();

                //##### Read the file #####
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader();

                    //##### Check header record #####
                    if (csv.HeaderRecord != null)
                    {
                        string msgColumn = string.Empty;

                        //##### Check header column #####
                        foreach (string headerColumn in csv.HeaderRecord.ToArray())
                        {
                            if (headerColumn != "Transaction Identificator" && headerColumn != "Account Number"
                                && headerColumn != "Amount" && headerColumn != "Currency Code"
                                && headerColumn != "Transaction Date" && headerColumn != "Status")
                            {
                                msgColumn += headerColumn + ", ";
                            }
                        }

                        if (!string.IsNullOrEmpty(msgColumn))
                        {
                            response.Status = 400;
                            response.StatusText = "Invalid header column : " + msgColumn.Remove(msgColumn.Length - 2, 2);
                            return response;
                        }
                    }
                    else
                    {
                        //##### No Header Record #####
                        response.Status = 400;
                        response.StatusText = "No header column";
                        return response;
                    }

                    //##### Read each row #####
                    while (csv.Read())
                    {
                        //##### Check header is completed #####
                        var data = csv.GetRecord<CSVFile>();
                        if (data != null)
                        {
                            ResponseInvalidCSVFile responseInvalidCSVFile = new ResponseInvalidCSVFile();
                            string message = await ValidateData(data);

                            responseInvalidCSVFile.TransactionIdentificator = data.TransactionIdentificator;
                            responseInvalidCSVFile.AccountNumber = data.AccountNumber;
                            responseInvalidCSVFile.Amount = data.Amount;
                            responseInvalidCSVFile.CurrencyCode = data.CurrencyCode;
                            responseInvalidCSVFile.TransactionDate = data.TransactionDate;
                            responseInvalidCSVFile.Status = data.Status;

                            if (!string.IsNullOrEmpty(message))
                            {
                                responseInvalidCSVFile.Remark = message;
                            }

                            ResponseInvalidCSVFile.Add(responseInvalidCSVFile);
                        }
                        else
                        {
                            //##### No data #####
                            response.Status = 400;
                            response.StatusText = "Data not found";
                            return response;
                        }
                    }

                    path = filePath;
                }

                List<ResponseInvalidCSVFile> invalidCSVFiles = ResponseInvalidCSVFile.Where(w => w.Remark != string.Empty).ToList();
                if (invalidCSVFiles.Count > 0)
                {
                    response.Status = 400;
                    response.StatusText = "File didn’t pass validation";
                }
                else
                {
                    //##### CSV File is pass validation then into database ("TRANSACTION_DATA" table) #####
                    foreach (ResponseInvalidCSVFile data in ResponseInvalidCSVFile)
                    {
                        TransactionData transactionData = new TransactionData();

                        transactionData.TRANSACTION_ID = data.TransactionIdentificator;
                        transactionData.ACCOUNT_NO = data.AccountNumber;
                        transactionData.AMOUNT = Convert.ToDecimal(data.Amount);
                        transactionData.CURRENCY_CODE = data.CurrencyCode;
                        transactionData.TRANSACTION_DATE = DateTime.Now;

                        //transactionData.TRANSACTION_DATE = DateTime.ParseExact(data.TransactionDate, "yyyy-MM-ddThh:mm:ss", null);
                        transactionData.STATUS = data.Status;
                        transactionData.FILE_FORMATS = "CSV";
                        transactionData.CREATED_BY = "SYSTEM";
                        transactionData.CREATED_DATE = DateTime.Now;
                        transactionData.UPDATED_BY = null;
                        transactionData.UPDATED_DATE = null;

                        transactionDataList.Add(transactionData);
                    }

                    //##### Save data #####
                    await _context.TransactionData.AddRangeAsync(transactionDataList);
                    await _context.SaveChangesAsync();

                    response.Status = 200;
                    response.StatusText = string.Empty;
                    response.ResponseInvalidCSVFileList = new();
                }

                response.TransactionDataList = transactionDataList;
                System.IO.File.Delete(path);

                return response;
            }
            catch(Exception ex)
            {
                response.Status = 500;
                response.StatusText = ex.Message;
                return response;
            }
        }
        
        public async Task<string> ValidateData(CSVFile data)
        {
            await Task.Delay(0);
            string msgError = string.Empty;
            if (data != null)
            {
                //##### Transaction Identificator #####
                if (data.TransactionIdentificator.Length > GetIntFromEnumValue(ColumnMaxLength.TransactionId))
                {
                    msgError += "Transaction Identification length is must less than or equal 50; ";
                }

                if (string.IsNullOrEmpty(data.TransactionIdentificator))
                {
                    msgError += "Transaction Identificator is not empty; ";
                }

                //##### Account Number #####
                if (data.AccountNumber.Length > GetIntFromEnumValue(ColumnMaxLength.AccountNumber))
                {
                    msgError += "Account Number length is must less than or equal 30; ";
                }

                if (string.IsNullOrEmpty(data.AccountNumber))
                {
                    msgError += "Account Number is not empty; ";
                }

                //##### Amount #####
                if (string.IsNullOrEmpty(data.Amount))
                {
                    msgError += "Amount is not empty; ";
                }

                decimal value;
                if (Decimal.TryParse(data.Amount, out value))
                {
                    //##### It's a decimal #####
                    data.Amount = value.ToString();
                }
                else
                {
                    //##### No it's not decimal #####
                    msgError += "Invalid Amount; ";
                }

                //##### Currency Code #####
                if ((data.CurrencyCode.Length > GetIntFromEnumValue(ColumnMaxLength.CurrencyCode)) || (!TryGetCurrencyCode(data.CurrencyCode)))
                {
                    msgError += "Invalid Currency Code; ";
                }

                if (string.IsNullOrEmpty(data.CurrencyCode))
                {
                    msgError += "Currency Code is not empty; ";
                }

                //##### Transaction Date #####
                if (string.IsNullOrEmpty(data.TransactionDate))
                {
                    msgError += "Transaction Date is not empty; ";
                }

                //DateTime formatDate;

                //if (DateTime.TryParse(data.TransactionDate, out formatDate))
                //{
                //    String.Format("{0:dd/MM/yyyy hh:mm:ss}", formatDate);
                //}
                //else
                //{
                //    Console.WriteLine("Invalid"); // <-- Control flow goes here

                //    msgError += "Invalid Transaction Date (Format : dd/MM/yyyy hh:mm:ss); ";
                //}

                //##### Status #####
                if (string.IsNullOrEmpty(data.Status))
                {
                    msgError += "Status is not empty; ";
                }

                if ((data.Status != Status.Approved.ToString()) && (data.Status != Status.Failed.ToString())
                    && (data.Status != Status.Finished.ToString()))
                {
                    msgError += "Invalid Status : " + data.Status + "; ";
                }

            }
            else
            {
                msgError += "Not found data";
            }

            if (!string.IsNullOrEmpty(msgError))
            {
                msgError = msgError.Remove(msgError.Length - 2, 2);
            }

            return msgError;
        }

        public bool TryGetCurrencyCode(string currencyCode)
        {
            bool isValidCurrencyCode = false;
            var symbol = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(c => !c.IsNeutralCulture)
                    .Select(culture =>
                    {
                        try
                        {
                            return new RegionInfo(culture.Name);
                        }
                        catch
                        {
                            return null;
                        }
                    })
                .Where(ri => ri != null && ri.ISOCurrencySymbol == currencyCode)
                .Select(ri => ri?.ISOCurrencySymbol)
                .FirstOrDefault();

            if (symbol != null)
            {
                isValidCurrencyCode = true;
            }
            else
            {
                isValidCurrencyCode = false;
            }

            return isValidCurrencyCode;
        }

        public async Task<ResponseTransactionData> GetAllTransactionData()
        {
            ResponseTransactionData response = new ResponseTransactionData();
            try
            {
                List<TransactionData> transactionDataList = await _context.TransactionData.ToListAsync();
                if (transactionDataList.Count == 0)
                {
                    response.Status = 404;
                    response.StatusText = "Not found data";
                    return response;
                }
                else
                {
                    foreach (TransactionData transactionData in transactionDataList)
                    {
                        TransactionDisplay transactionDisplay = new TransactionDisplay();
                        transactionDisplay.Id = transactionData.TRANSACTION_ID;
                        transactionDisplay.Payment = transactionData.AMOUNT.ToString() + " " + transactionData.CURRENCY_CODE;

                        //##### CSV status #####
                        if (transactionData.FILE_FORMATS == "CSV")
                        {
                            if (transactionData.STATUS == Status.Approved.ToString())
                                transactionDisplay.Status = "A";
                            else if (transactionData.STATUS == Status.Failed.ToString())
                                transactionDisplay.Status = "R";
                            else if (transactionData.STATUS == Status.Finished.ToString())
                                transactionDisplay.Status = "D";
                        }
                        //##### XML status #####
                        else if (transactionData.FILE_FORMATS == "XML")
                        {
                            if (transactionData.STATUS == Status.Approved.ToString())
                                transactionDisplay.Status = "A";
                            else if (transactionData.STATUS == Status.Rejected.ToString())
                                transactionDisplay.Status = "R";
                            else if (transactionData.STATUS == Status.Done.ToString())
                                transactionDisplay.Status = "D";
                        }

                        response.TransactionDisplays.Add(transactionDisplay);
                    }
                    response.Status = 200;
                }
               
                return response;
            }
            catch(Exception ex)
            {
                response.Status = 500;
                response.StatusText = ex.Message;
                return response;
            }
        }

        public async Task<ResponseTransactionData> GetTransactionDataByCurrency(string currencyCode)
        {
            ResponseTransactionData response = new ResponseTransactionData();
            try
            {
                List<TransactionData> transactionDataList = await _context.TransactionData.ToListAsync();
                transactionDataList = transactionDataList.Where(w => w.CURRENCY_CODE.ToUpper() == currencyCode.ToUpper()).ToList();

                if (transactionDataList.Count == 0)
                {
                    response.Status = 404;
                    response.StatusText = "Not found data";
                    return response;
                }
                else
                {
                    foreach (TransactionData transactionData in transactionDataList)
                    {
                        TransactionDisplay transactionDisplay = new TransactionDisplay();
                        transactionDisplay.Id = transactionData.TRANSACTION_ID;
                        transactionDisplay.Payment = transactionData.AMOUNT.ToString() + " " + transactionData.CURRENCY_CODE;

                        //##### CSV status #####
                        if (transactionData.FILE_FORMATS == "CSV")
                        {
                            if (transactionData.STATUS == Status.Approved.ToString())
                                transactionDisplay.Status = "A";
                            else if (transactionData.STATUS == Status.Failed.ToString())
                                transactionDisplay.Status = "R";
                            else if (transactionData.STATUS == Status.Finished.ToString())
                                transactionDisplay.Status = "D";
                        }
                        //##### XML status #####
                        else if (transactionData.FILE_FORMATS == "XML")
                        {
                            if (transactionData.STATUS == Status.Approved.ToString())
                                transactionDisplay.Status = "A";
                            else if (transactionData.STATUS == Status.Rejected.ToString())
                                transactionDisplay.Status = "R";
                            else if (transactionData.STATUS == Status.Done.ToString())
                                transactionDisplay.Status = "D";
                        }

                        response.TransactionDisplays.Add(transactionDisplay);
                    }
                    response.Status = 200;
                }

                return response;
            }
            catch (Exception ex)
            {
                response.Status = 500;
                response.StatusText = ex.Message;
                return response;
            }
        }

        public async Task<ResponseTransactionData> GetTransactionDataByStatus(string status)
        {
            ResponseTransactionData response = new ResponseTransactionData();
            try
            {
                List<TransactionData> transactionDataList = await _context.TransactionData.ToListAsync();
                transactionDataList = transactionDataList.Where(w => w.STATUS.ToUpper() == status.ToUpper()).ToList();

                if (transactionDataList.Count == 0)
                {
                    response.Status = 404;
                    response.StatusText = "Not found data";
                    return response;
                }
                else
                {
                    foreach (TransactionData transactionData in transactionDataList)
                    {
                        TransactionDisplay transactionDisplay = new TransactionDisplay();
                        transactionDisplay.Id = transactionData.TRANSACTION_ID;
                        transactionDisplay.Payment = transactionData.AMOUNT.ToString() + " " + transactionData.CURRENCY_CODE;

                        //##### CSV status #####
                        if (transactionData.FILE_FORMATS == "CSV")
                        {
                            if (transactionData.STATUS == Status.Approved.ToString())
                                transactionDisplay.Status = "A";
                            else if (transactionData.STATUS == Status.Failed.ToString())
                                transactionDisplay.Status = "R";
                            else if (transactionData.STATUS == Status.Finished.ToString())
                                transactionDisplay.Status = "D";
                        }
                        //##### XML status #####
                        else if (transactionData.FILE_FORMATS == "XML")
                        {
                            if (transactionData.STATUS == Status.Approved.ToString())
                                transactionDisplay.Status = "A";
                            else if (transactionData.STATUS == Status.Rejected.ToString())
                                transactionDisplay.Status = "R";
                            else if (transactionData.STATUS == Status.Done.ToString())
                                transactionDisplay.Status = "D";
                        }

                        response.TransactionDisplays.Add(transactionDisplay);
                    }
                    response.Status = 200;
                }

                return response;
            }
            catch (Exception ex)
            {
                response.Status = 500;
                response.StatusText = ex.Message;
                return response;
            }
        }

    }
}
