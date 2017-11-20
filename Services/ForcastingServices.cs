using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Data;
using Abstraction;
using MoreLinq;


/*
        COMMENTS

   1. 4kolon (product-store-date-quantity) csv datasindan 4colon (product-totalSale-AvgWeeklySale-WeeklyStDev) csv datasi uretir ve downloada hazirlar;
   2. UI uzerinde kullanilabilen, entegre olmus DataAccess-BusinnessLogic katmani gorevi gorur,
   3.  Bir CSV Dosyasi ciktisi icin Tum akis server uzerinde ortalama 39 sn.
     , UI uzerinde ortalalma 1 dakika 20 sn.

    Services Patterninin;
   4. Class larin erisilebilirlik seviyeleri nested class lar ve abstraction yontemleri ile kontrol edildi.
   5. Bazi Anonim Metotlar ve bazi objeler UI erisimine acik birakildi,
   6. Data tasiyan classlarin instanslanmalari abstraction ile engellendi,
   7. Servisler clasinin bir tek ResponseCSV() methodu UI erisimine acik birakildi
   8. Servisler clasininin diger izin verilen hizmetleri abstraction ile gizlendi
   
    Pattern Haritasi;

Public serviser:
 Meta Data listelerin hazirlanmasi
InputDataFactory:
  Datalari tasiyacak objelerin seri uretimi,
  Data kaynaginin IEnumerable tipine hazirlanmasi,
ModelServices
  SampleMeanModel
     Sonuc analizi
     Sonuclari dosyaya yazridma

*/
//Extra frameworks:
//MoreLinq [NuGet packages](https://www.nuget.org/packages/morelinq/)


namespace Forcasting.Services
{
    #region ExtensionMethods
    /// <summary>
    /// For harvesting the each row of the input file as the properties of this object
    /// </summary>
    public class SaleData : ISaleDataInfo
    {
        #region PrivateFields
        private string _product;
        private string _store;
        private DateTime _date = new DateTime();
        private double _quantity;
        #endregion

        #region Ctors
        public SaleData()
        {
         
        }
        public SaleData(string Product, string Store, DateTime Date, int Quantity)
        {
            _product = Product;
            _store = Store;
            _date = Date;
            _quantity = Quantity;
        }
        #endregion

        #region PublicProperties
        public int iD { get; set; }
        public string Product
        {
            get
            {
                if (_product.Length > 0)
                {
                    return _product;
                }
                else
                    throw new Exception("Bad data detected; Check data source or serialization process"); //double check
            }
            set
            {
                if (value != null)
                {
                    _product = value;
                }
                else
                    throw new Exception("Every product must have a non-null value");
            }
        }
        public string Store
        {
            get
            {
                if (_store != null)
                {
                    return _store;
                }
                else
                    throw new Exception("Bad data detected; Check data source or serialization process"); //double check
            }
            set
            {
                if (value != null)
                {
                    _store = value;
                }
                else
                    throw new Exception("Every Store must have a non-null value");
            }
        }
        public DateTime Date
        {
            get
            {
                if (_date != default(DateTime))
                {
                    return _date;
                }
                else
                    throw new Exception("Bad data detected; Check data source or serialization process"); //double check
            }
            set
            {
                if (value != null)
                {
                    _date = value;
                }
                else
                    throw new Exception("Every product must have a non-null value");
            }
        }
        public double Quantity
        {
            get
            {
                if (_quantity >= 0)
                {
                    return _quantity;
                }
                else
                    throw new Exception("Bad data detected; Check data source or serialization process"); //double check
            }
            set
            {
                if (value >= 0)
                {
                    _quantity = value;
                }
                else
                    throw new Exception("Every quantity must have a positive value");
            }
        }
        //Will Be set while creating data source list
        public double WeekNo { get; set; }
        //Will Be set while creating data source list
        public double TotalSellAmountInModel { get; set; }
        //Will Be set while creating data source list
        public double TotalWeekNumberInModel { get; set; }
        #endregion
    }
    /// <summary>
    /// Some extension methods for dealing with populated columns
    /// </summary>
    public static class ExtensionMethods
    {
        public class Delegetes
        {
            /// <summary>
            /// Takes Initial Sale date and sale date, gives WeekNo value of a sale
            /// </summary>
          public static  Func<DateTime, DateTime, int> WeekNoConverter = (DateTime SaleDate, DateTime InitialDate) => (int)(SaleDate.Subtract(InitialDate).TotalDays / 7) + 1;
            /// <summary>
            /// Creates a formatted string over the properties of WSDDATAProduct object
            /// </summary>
            public static Func<WSDDATAProduct, string> StringFormatPreparer = (WSDDATAProduct wsdDataforProduct) => string.Format("{0},{1},{2},{3}", wsdDataforProduct.Product, wsdDataforProduct.TotalSale, wsdDataforProduct.WeeklyAverage,
                        wsdDataforProduct.WeeklyStDev, Environment.NewLine);
            /// <summary>
            /// Appends formatted string to a StringBuilder object
            /// </summary>
            public static Action<string, StringBuilder> StringBuilderAppender = (string subject, StringBuilder stringBuilder) => stringBuilder.AppendLine(subject);
        }
        /// <summary>
        /// Prepares the WSD output datas for every product to writte by stringFormating the lines
        /// </summary>
        /// <param name="ResultList">Any WSD data result set</param>
        /// <param name="StringFormatterPreparer"> Prepares each column as comma separated lines </param>
        /// <returns>Returns a List of strings as 4 column for csv format</returns>
        public static List<string> PrepareForWrite (this IEnumerable<WSDDATAProduct> ResultList, Func<WSDDATAProduct,string> StringFormatterPreparer)
        {
            
            List<string> Returning = new List<string>();
            foreach (WSDDATAProduct item in ResultList)
            {
               Returning.Add(StringFormatterPreparer(item));
            }
            return Returning;
        }
        /// <summary>
        /// Creates and builds the StringBuilder object for appending rows
        /// </summary>
        /// <param name="StringList">Represents a string list corresponds the lines of CSV putput</param>
        /// <param name="LineWriter">Returns a single SingleBuilderObject encompassing all lines to write</param>
        /// <returns></returns>
        public static StringBuilder  BuildTheStrings(this IEnumerable<string> StringList, Action<string, StringBuilder> LineAppender)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string StringItem in StringList)
            {
                LineAppender(StringItem,sb);
            }
            return sb;
        }
        /// <summary>
        /// An extension method to StringBuilder object to materialize
        /// </summary>
        /// <param name="FilledStringBuilderObject">String Builder Object Filled With Lines to Write</param>
        /// <param name="path">Returns a void action</param>
        public static void MaterializeBuildedString (this StringBuilder FilledStringBuilderObject, string path)
        {
            File.WriteAllText(path, FilledStringBuilderObject.ToString());
        }
        /// <summary>
        /// Determines the WeekNo value of  given dates
        /// </summary>
        /// <param name="Date">SaleDate</param>
        /// <param name="InitialDate">InitialSaleDate</param>
        /// <returns></returns>
        public static int ConvertDateTimeToWeekNo(this DateTime Date, DateTime InitialDate)
        {
            return (int)(Date.Subtract(InitialDate).TotalDays / 7) + 1;
        }

        /// <summary>
        /// Sets the WeekNo property of any ISalesDataInfo objects
        /// </summary>
        /// <param name="SaleDataInfo">Represents the object that harvests a sale row line </param>
        /// <param name="InitialSaleDate">Initial Sale date in the model</param>
        /// <param name="ConvertSaleDatetimeToWeekno">Returns the WeekNo value of  ISaleDataInfo object</param>
        public static void SetWeekNo(this IEnumerable<ISaleDataInfo> SaleDataInfo, DateTime InitialSaleDate, Func<DateTime,DateTime,int> ConvertSaleDatetimeToWeekno)
        {
           
            foreach (ISaleDataInfo item in SaleDataInfo)
            {
                item.WeekNo = ConvertSaleDatetimeToWeekno(item.Date,InitialSaleDate);
            }
        }
    }
    /// <summary>
    /// This is for limiting the UI Access
    /// </summary>
    public static class ServiceAccess 
    {
        public static IService AccessForcastingService()
        {
            return new Services();
        }
    }
    #endregion
    /// <summary>
    /// Represents all input and output services intgrated with DataAccessLayer.If public, UI can access mostly classess
    /// </summary>
    public class Services :IDisposable, IService //Make internal for UI filtering of the services
    {
        /// <summary>
        /// Required path to read input file
        /// </summary>
        private static string _Path;
        public Services()
        {

        }
        /// <summary>
        /// Constructor with path of not setted by property
        /// </summary>
        /// <param name="path"></param>
        public Services(string path)
        {
            _Path = path;
          this.Path = _Path;
        }
        /// <summary>
        /// Permitted UI Service
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="TargetPath"></param>
        /// <returns></returns>
        public bool ResponseCSVFile(string sourcePath, string TargetPath)
        {
            using (Services s = new Services(sourcePath))
            {
                IDataSource ds = s.InputDataFactory.CreateDataSource();
                IProcessedData ps = s.CreateProcessedData(ds);
                IWSDAnalizer ws = s.ModelService.ConstantModel.CreateWSDAnalizer(ps);
                IEnumerable<WSDDATAProduct> wsd = ws.WSDListForProduct;
                wsd.PrepareForWrite(ExtensionMethods.Delegetes.StringFormatPreparer).BuildTheStrings(ExtensionMethods.Delegetes.StringBuilderAppender).MaterializeBuildedString(TargetPath);
            }
            return true;
        } //UI Direct Service
        /// <summary>
        /// Permitted Public property for setting Path property on will;
        /// </summary>
        public virtual string Path { get { return _Path; } set { _Path = value; } }
        #region ServiceMenu
        /// <summary>
        /// Represents the services related to data importing and data processing
        /// </summary>
        public IInputDataFactory InputDataFactory { get { return new InputFactoryServices(_Path); } }
        /// <summary>
        /// Creates an object that harvests filtered data to prepare for computations
        /// </summary>
        /// <param name="DataSource">Represents the collection of ISaleDataInfo object</param>
        /// <returns></returns>
        public IProcessedData CreateProcessedData(IDataSource DataSource)
        {
            return new ProcessedData(DataSource);
        }
        /// <summary>
        /// Represents the services for input handling tools
        /// </summary>
        private class InputFactoryServices : IInputDataFactory
        {
            /// <summary>
            /// Filled by higher hierarchy thread
            /// </summary>
            private string _path;
            public InputFactoryServices(string path)
            {
                _path = path;
            }
            #region ServiceMenu
            /// <summary>
            /// Creates the IDataSource object that Harvests the input data List and some metadata lists
            /// </summary>
            /// <returns></returns>
            public IDataSource CreateDataSource()
            {
                return new DataSource();
            }
            /// <summary>
            ///  Creates the IDataSource object that Harvests the input data List and some metadata lists
            /// </summary>
            /// <param name="Path"></param>
            /// <returns></returns>
            public IDataSource CreateDataSource(string Path)
            {
                return new DataSource(Path);
            }
            /// <summary>
            /// Creates a single ISaleDataInfo object to harves raw data
            /// </summary>
            /// <returns></returns>
            public ISaleDataInfo CreateSaleInfoObject()
            {
                return new SaleData();
            }
            /// <summary>
            ///   /// Creates a single ISaleDataInfo object to harves raw data
            /// </summary>
            /// <param name="Product"></param>
            /// <param name="Store"></param>
            /// <param name="Date"></param>
            /// <param name="Quantity"></param>
            /// <returns></returns>
            public ISaleDataInfo CreateSaleInfoObject(string Product, string Store, DateTime Date, int Quantity)
            {
                return new SaleData() { Product = Product, Store = Store, Date = Date, Quantity = Quantity };
            }
            #endregion
            /// <summary>
            /// Represents surce data list and some metadata
            /// </summary>
            private class DataSource : Services, IDataSource
            {
                #region PrivateFields
                private string _SetPath;
                private List<ISaleDataInfo> _Sales;
                private double _NumberOfWeeksInModel;
                private DateTime _InitialSaleDate;
               
                #endregion
                #region Ctor
                public DataSource()
                {
                    //Path Control
                    _SetPath = this.Path;
                 
                    _Sales = new List<ISaleDataInfo>();
                   
                    if (_SetPath != null && _SetPath.Length > 0)
                    {
                        //OrderIsImportant
                        SetSalesData(_SetPath);
                        SetMinDate();
                        SetWeekNo();
                        SetTotalNumberOfWeeksInModel();
                    }
                    else
                        throw new Exception("File or directory exception. Make sure you set the Path property. Otherwise use constructor with parameter. Check Path");
                }
                public DataSource(string path):base()
                {
                    //Path Kontrol
                    _SetPath = Path;
                    this.Path = _SetPath;
                    if (_SetPath != null && _SetPath.Length > 0)
                    {
                        //OrderIsImportant
                        SetSalesData(_SetPath);
                        SetMinDate();
                        SetWeekNo();
                        SetTotalNumberOfWeeksInModel();
                    }
                    else
                        throw new Exception("File or directory exception. Make sure you set the Path property. Otherwise use constructor with parameter. Check Path");
                }
                #endregion
                #region PublicProperties
                /// <summary>
                /// To catch the Path property from Services class if constructor parameters in not used, insted path property is used
                /// </summary>
                public override string Path
                {
                    get
                    {
                        return base.Path;
                    }

                    set
                    {
                        base.Path = value;
                    }
                }
                /// <summary>
                /// List of  ISaleDataInfo objects filled with raw datas as properties
                /// </summary>
                public List<ISaleDataInfo> SalesData
                {
                    get
                    {
                        return _Sales;
                    }
                }
                public double NumberOfWeeksInModel { get { return (double)_NumberOfWeeksInModel; } }
                public DateTime InitialSaleDate { get { return _InitialSaleDate.Date; } }
                #endregion

                #region PrivateMethods
                /// <summary>
                /// Sets the SalesData properties
                /// </summary>
                /// <param name="Path"></param>
                private void SetSalesData(string FilePath)
                {
                    int elementCount = 0;
                    DateTime _date = new DateTime();
                    ISaleDataInfo saleData;
                    IEnumerable[] sliced;
                    IEnumerable allRows = File.ReadLines(FilePath);
                    foreach (string item in allRows)
                    {
                        elementCount++;
                        sliced = item.Split(',');
                        int lengh = sliced.Length;
                        saleData = InputDataFactory.CreateSaleInfoObject();
                        for (int i = 0; i < lengh; i++)
                        {
                            if (i == 0)
                            {
                                saleData.Product = (string)sliced.GetValue(i);
                                continue;
                            }
                            else if (i == 1)
                            {
                                saleData.Store = (string)sliced.GetValue(i);
                                continue;
                            }
                            else if (i == 2)
                            {
                                if (DateTime.TryParse((string)sliced.GetValue(i), out _date))
                                {
                                    saleData.Date = _date;
                                }
                            }
                            else
                                saleData.Quantity = Convert.ToInt32((string)sliced.GetValue(i));
                        }
                        saleData.iD = elementCount;
                        _Sales.Add(saleData);
                    }
                }
                /// <summary>
                /// Sets The parameters required to determine WeekNo value
                /// </summary>
                private void SetMinDate()
                {
                    _InitialSaleDate = _Sales.MinBy(p => p.Date).Date;
                }
                /// <summary>
                /// for WeekNo values to set via an Extension method over IEnumerable<ISaleDataInfo>
                /// </summary>
                public void SetWeekNo()
                {
                    _Sales.SetWeekNo(_InitialSaleDate, ExtensionMethods.Delegetes.WeekNoConverter);
                }
                /// <summary>
                /// Sets the maximum WeekNo of the model
                /// </summary>
                public void SetTotalNumberOfWeeksInModel()
                {
                    _NumberOfWeeksInModel = _Sales.MaxBy(a => a.WeekNo).WeekNo;
                }
                #endregion
            }
        }
        /// <summary>
        /// Represents the details of ProcessedData services
        /// </summary>
        private class ProcessedData : IProcessedData
        {
            #region PrivateFields
            private IDataSource _dataSource;
            private IEnumerable<WeeklyProductSale> _ProductVSSaleByWeek;
            private IEnumerable<TotallyProductSale> _ProductVSTotalSale;
            private double _TotalNumberOfweeks; 
            #endregion
            public ProcessedData(IDataSource DataSource)
            {
                _dataSource = DataSource;
                _TotalNumberOfweeks = DataSource.NumberOfWeeksInModel; 
                SetProductVSSaleByWeek();
                SetProductVSTotalSale();
            }
            /// <summary>
            /// Sets ProductVSSaleByWeek Metadata list
            /// </summary>
            private void SetProductVSSaleByWeek()
            {
                _ProductVSSaleByWeek = _dataSource.SalesData.GroupBy(p => new
                {
                    Product = p.Product,
                    WeekNo = p.WeekNo,
                    NumberOfWeeksInModel = this._TotalNumberOfweeks,
                }).Select(a => new WeeklyProductSale()
                {
                    Product = a.Key.Product,
                    WeekNo = a.Key.WeekNo,
                    Sale = a.Sum(q => q.Quantity),
                    TotalWeekNoInModel = a.Key.NumberOfWeeksInModel
                }).ToList();
            }
            /// <summary>
            /// Sets SetProductVSTotalSale() Metadata list
            /// </summary>
            private void SetProductVSTotalSale()
            {
                _ProductVSTotalSale = _dataSource.SalesData.GroupBy(p => new
                {
                    Product = p.Product,
                    TotalWeekNoInModel = this.TotalNumberOfWeeksInModel,
                }).Select(t => new TotallyProductSale()
                {
                    Product = t.Key.Product,
                    TotalSale = t.Sum(q => q.Quantity),
                    TotalWeekNoInModel =t.Key.TotalWeekNoInModel

                }).ToList();
            }
            /// <summary>
            /// Gets a collection of WeeklyProductSale objects
            /// </summary>
            public IEnumerable<WeeklyProductSale> ProductVSSaleByWeek { get { return _ProductVSSaleByWeek; } }
            /// <summary>
            /// Gets a collection of TotallyProductSale objects
            /// </summary>
            public IEnumerable<TotallyProductSale> ProductVSTotalSale { get { return _ProductVSTotalSale; } }
            /// <summary>
            /// Gets the TotalNumberOfWeeksInModel
            /// </summary>
            public double TotalNumberOfWeeksInModel { get { return _TotalNumberOfweeks; } }
        }
        /// <summary>
        /// Represents the services that provides potential alternative Forcasting models
        /// </summary>
        public IModelServices ModelService { get { return new ModelServices(); } }
        #endregion
        #region ModelServices
        private class ModelServices : IModelServices
        {
            public ISampleMeanModel ConstantModel { get { return new SampleMeanModel(); } }
            /// <summary>
            /// Represents the Ingredients of ConstantModel service
            /// </summary>
            private class SampleMeanModel : ISampleMeanModel
            {
                /// <summary>
                /// Weekly Standard Deviation data Analizer creator
                /// </summary>
                /// <param name="ProcessedData"></param>
                /// <returns></returns>
                public IWSDAnalizer CreateWSDAnalizer(IProcessedData ProcessedData)
                {
                    return new WSDAnalizer(ProcessedData);
                }
                /// <summary>
                /// Represents the Weekly Standard Deviation data operations
                /// </summary>
                private class WSDAnalizer : IWSDAnalizer
                {
                    #region PrivateFields
                    private IProcessedData _ProcessedData;
                    private IEnumerable<WeeklyProductSale> _WeeklySaleList;
                    private IEnumerable<TotallyProductSale> _TotallySaleList;
                    private double _TotalNumberOfWeeksInModel;
                    private IEnumerable<WSDDATAProduct> _WSDListForProduct;
                    #endregion
                    /// <summary>
                    /// Represents an anonymious method that create String.Format object 
                    /// </summary>
                    
                    /// <summary>
                    /// Represents the anonymious Action that appends a string to a string builder
                    /// </summary>
                 
                    public WSDAnalizer(IProcessedData ProcessedData)
                    {
                        _ProcessedData = ProcessedData;
                        _WeeklySaleList = ProcessedData.ProductVSSaleByWeek;
                        _TotallySaleList = ProcessedData.ProductVSTotalSale;
                        _TotalNumberOfWeeksInModel = ProcessedData.TotalNumberOfWeeksInModel;
                        SetWSDListForProduct();
                    }
                    /// <summary>
                    /// Gets WeeklyStandardDeviation data list for Products
                    /// </summary>
                    public IEnumerable<WSDDATAProduct> WSDListForProduct { get { return _WSDListForProduct; }}
                    /// <summary>
                    /// Sets WeeklyStandardDeviation data list for products
                    /// </summary>
                    private void SetWSDListForProduct()
                    {
                        _WSDListForProduct = _WeeklySaleList.Select(p => new
                        {
                            TotalNumberOfWeeksInModel = p.TotalWeekNoInModel,
                            Product = p.Product,
                            Quantity = p.Sale,
                            Wkno = p.WeekNo,
                            // 1/N-1 for sample variance formula
                            WeeklyVariance = 1/(this._TotalNumberOfWeeksInModel-1) * Math.Pow(p.Sale - _TotallySaleList.First(x => x.Product == p.Product).TotalSale / p.TotalWeekNoInModel, 2)
                        }).GroupBy(p => new
                        {
                            Product = p.Product,
                            TotalNumOfWeeksInModel = this._TotalNumberOfWeeksInModel,

                        }).Select(p => new WSDDATAProduct()
                        {
                            Product = p.Key.Product,
                            TotalSale = p.Sum(q => q.Quantity),
                            WeeklyAverage = p.Sum(q => q.Quantity) / p.Key.TotalNumOfWeeksInModel,
                            WeeklyStDev = Math.Pow(p.Sum(z => z.WeeklyVariance), 0.5) ///sum z.weeklyVariance~sum z.quantity
                        }).ToList();
                    }
                    /// <summary>
                    /// Writes a collection of formatted and prepared strings to a CSV file
                    /// </summary>
                    /// <param name="path"></param>
                    /// <param name="List"></param>
                    public void WriteListToCSVFile(string path, IEnumerable<WSDDATAProduct> List) 
                    {
                        List.PrepareForWrite(ExtensionMethods.Delegetes.StringFormatPreparer).BuildTheStrings(ExtensionMethods.Delegetes.StringBuilderAppender).MaterializeBuildedString(path);
                    }//write has been done over UI Object
                }
            }
        }
        #endregion
        #region InputDataServices
        #endregion
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Services() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}

//Abstraction is required for framework to perform
namespace Abstraction
{
    /// <summary>
    /// Service interface that this UI is allowed to use
    /// </summary>
    public interface IService
    {
        bool ResponseCSVFile(string sourcePath, string TargetPath);
        string Path { get; set; }
    }
    #region ProcessedData

    public interface IProcessedData
    {
        IEnumerable<WeeklyProductSale> ProductVSSaleByWeek { get; }
        IEnumerable<TotallyProductSale> ProductVSTotalSale { get; }
        double TotalNumberOfWeeksInModel { get; }
    }
    public class WeeklyProductSale : IProduct, IWeeklySale
    {
        public double WeekNo { get; set; }
        public double Sale { get; set; }
        public string Product { get; set; }
        public double TotalWeekNoInModel { get; set; }
    }
    public class TotallyProductSale : IProduct, ITotalSale
    {
        public double TotalSale { get; set; }
        public string Product { get; set; }
        public double TotalWeekNoInModel { get; set; }


    }
    public class WeeklyStoreSale : IStore, IWeeklySale
    {
        public double WeekNo { get; set; }
        public double Sale { get; set; }
        public string Store { get; set; }
    }
    public class TotallyStoreSale : IStore, ITotalSale
    {
        public double TotalSale { get; set; }
        public string Store { get; set; }
    }
    public interface IWeeklySale
    {
        double WeekNo { get; set; }
        double Sale { get; set; }
    }
    public interface ITotalSale
    {
        double TotalSale { get; set; }
    }
    public interface IStore
    {
        string Store { get; set; }
    }
    public interface IProduct
    {
        string Product { get; set; }
    }

    #endregion
    #region InputDataServices
    public interface ISaleDataInfo
    {
        int iD { get; set; }
        string Product { get; set; }
        string Store { get; set; }
        DateTime Date { get; set; }
        double Quantity { get; set; }
        double WeekNo { get; set; }
        double TotalSellAmountInModel { get; set; }
        double TotalWeekNumberInModel { get; set; }
    }//
    public interface IDataSource
    {
        DateTime InitialSaleDate { get; }
        List<ISaleDataInfo> SalesData { get; }
        double NumberOfWeeksInModel { get; }
    }
    public interface IInputDataFactory
    {

        ISaleDataInfo CreateSaleInfoObject();
        ISaleDataInfo CreateSaleInfoObject(string Product, string Store, DateTime Date, int Quantity);
        IDataSource CreateDataSource();
        IDataSource CreateDataSource(string Path);
    }
    #endregion
    #region ModelServices
    public interface ISampleMeanModel
    {
        IWSDAnalizer CreateWSDAnalizer(IProcessedData ProcessedData);
    }
    public interface IModelServices
    {
        ISampleMeanModel ConstantModel { get; }
    }
    #endregion
    #region OutputDataServices
    public interface IWSDAnalizer
    {
        IEnumerable<WSDDATAProduct> WSDListForProduct { get; }
        void WriteListToCSVFile(string path, IEnumerable<WSDDATAProduct> List);

    }
    public interface IWSDDATA
    {
        double WeeklyStDev { get; set; }
        double WeeklyAverage { get; set; }
    }
    public interface IWSDDATAProduct : IProduct, IWSDDATA, ITotalSale
    {

    }
    public interface IWSDDATASTore : IStore, IWSDDATA
    {

    }
    public class WSDDATAProduct : IWSDDATAProduct
    {
        public string Product { get; set; }
        public double TotalSale { get; set; }
        public double WeeklyAverage { get; set; }
        public double WeeklyStDev { get; set; }

    }
    #endregion
}

