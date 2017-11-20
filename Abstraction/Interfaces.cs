using System;
using System.Collections.Generic;

namespace Abstraction
{
    public interface IServices
    {
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